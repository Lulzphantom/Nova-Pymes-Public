<?php
// REQUERES
require_once("../../_functions/configuration.php");
require_once("../../_functions/functions.php");
require_once("../../_functions/header.php");
require_once("../../_functions/permissions.php");
header('Content-type: application/json');


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check user rol permissions
$permission = CheckRolPermissions($_POST['username'], $perm_post);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        BoxStatus(CheckData($_POST['data']));
    case 2:
        OpenBox(CheckData($_POST['data']),GetUsernameID($_POST['username']));    
    case 3:
        CloseBox(CheckData($_POST['data']),GetUsernameID($_POST['username'])); 
    case 4:
        BoxDetails(CheckData($_POST['data']));
}

//Get Box status
function BoxStatus($JsonData){

    global $mysql_connect_host;

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros incorrectos.')));
    }

    $Query = "SELECT box_movement_id, box_movement_box, box_movement_opendate, user_realname AS username FROM branch_boxes_movements 
    LEFT JOIN user ON user_id = box_movement_openuser 
    WHERE box_movement_box = '%s' AND box_movement_closedate IS NULL";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    //On Query error
    if (!$Query) {
       echo json_encode(array('success' => 0, 'error_message' => 'Error al obtener el estado de la caja de venta - '.mysqli_error($mysql_connect_host)));
       exit; 
    } 

    $Rows   = mysqli_num_rows($Query); 
    $Movement_id = "";
    
    if ($Rows != 0) {
        //Existent open box point
        while ($Results = mysqli_fetch_array($Query)) {

            $Movement_id = $Results["box_movement_id"];
            $jsonData[] = array(
                'id'            =>      ''.$Results["box_movement_id"].'',
                'opendate'      =>      ''.$Results["box_movement_opendate"].'',
                'username'    =>      ''.$Results["username"].'');           

        }        
        echo json_encode(array('success' => 1, 'box_status' => 1,  'movement_id' => $Movement_id, 'box_data' => $jsonData));		
        exit;
    }else{
        //non existent open box point
        echo json_encode(array('success' => 1, 'box_status' => 0));		
        exit;
    }
}

function OpenBox($JsonData,$UserID){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros incorrectos.')));
    }

    //Check if box is opened
    $Query = "SELECT box_movement_id, box_movement_box, box_movement_opendate FROM branch_boxes_movements
    WHERE box_movement_box = '%s' AND box_movement_closedate IS NULL";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    //On Query error
    if (!$Query) {
       echo json_encode(array('success' => 0, 'error_message' => 'Error al obtener el estado de la caja de venta - '.mysqli_error($mysql_connect_host)));
       exit; 
    } 

    $Rows   = mysqli_num_rows($Query); 
    
    if ($Rows != 0) {
        //Existent open box point
        echo json_encode(array('success' => 0, 'error_message' => 'La caja de venta ya se encuentra abierta '));
       exit; 
    }else{
        
        $Query =  "INSERT INTO branch_boxes_movements
        (box_movement_box, box_movement_openvalue, box_movement_openuser)
        VALUES (%s, %s, '%s');";
        $Query  = db_query_host(sprintf($Query,$JsonData["id"],$JsonData["openvalue"],$UserID));   

        if (!$Query) {
            //Bad mysqli response
            die(json_encode(array('success' => 0, 'error_message' => 'No se pudo ingresar el movimiento de caja')));
            
        } else {   
            //Movement ID
            $movement_id = mysqli_insert_id($mysql_connect_host);         
            //Response OK
            echo json_encode(array('success'=>1, 'movement_id' => $movement_id));		
            exit;
        } 
    }   
}

function CloseBox($JsonData,$UserID){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros incorrectos.')));
    }
  
    //Check if box is opened
    $Query = "SELECT box_movement_id, box_movement_box, box_movement_opendate FROM branch_boxes_movements
    WHERE box_movement_id = '%s' AND box_movement_closedate IS NULL";
    $BoxQuery  = db_query_host(sprintf($Query,$JsonData["id"]));

    //On Query error
    if (!$BoxQuery) {
       echo json_encode(array('success' => 0, 'error_message' => 'Error al obtener el estado de la caja de venta - '.mysqli_error($mysql_connect_host)));
       exit; 
    } 

    $Rows   = mysqli_num_rows($BoxQuery);

    if ($Rows != 0) {
        //Existent open box point 

        $BoxQuery = mysqli_fetch_array($BoxQuery);

        $Query =  "UPDATE branch_boxes_movements
	SET
		box_movement_closeuser='%s',
		box_movement_closevalue='%s',
		box_movement_comment='%s'
	WHERE box_movement_id = '%s';";
        $Query  = db_query_host(sprintf($Query,$UserID,$JsonData["closevalue"],$JsonData["comment"],$BoxQuery["box_movement_id"]));   

        if (!$Query) {
            //Bad mysqli response
            die(json_encode(array('success' => 0, 'error_message' => 'No se pudo ingresar el movimiento de caja')));
            
        } else {          
            //Response OK
            echo json_encode(array('success'=>1, 'movement_id' => $BoxQuery["box_movement_id"]));		
            exit;
        } 

    }else{
        echo json_encode(array('success' => 0, 'error_message' => 'La caja de venta no se encuentra abierta '));
       exit;        
    }  
}

function BoxDetails($JsonData){

    global $mysql_connect_host;
    global $permission;

    //Check permissions
    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros incorrectos.')));
    }

    //Get box information
    $BoxQuery = "SELECT box_movement_opendate, box_movement_closedate, box_movement_openvalue, openuser.user_realname AS openusername, closeuser.user_realname AS closeusername, box_movement_closevalue, box_movement_comment
    FROM branch_boxes_movements
    LEFT JOIN user openuser ON openuser.user_id = box_movement_openuser
    LEFT JOIN user closeuser ON closeuser.user_id = box_movement_closeuser 
    WHERE box_movement_id = '%s';";
    $BoxQuery  = db_query_host(sprintf($BoxQuery,$JsonData["id"]));

    //On Query error
    if (!$BoxQuery) {
       echo json_encode(array('success' => 0, 'error_message' => 'Error al obtener el estado de la caja de venta - '.mysqli_error($mysql_connect_host)));
       exit; 
    } 
    $BoxQuery = mysqli_fetch_array($BoxQuery);

    //Get tickets values

    $CashQuery = "SELECT COALESCE(SUM(ticket_totalpayment),0) AS totalcash
    FROM ticket
    WHERE ticket_payment_method = 0 AND ticket_box_movement = '%s' AND ticket_status = 1;";
    $CashQuery = db_query_host(sprintf($CashQuery,$JsonData["id"]));

    //On Query error
    if (!$CashQuery) {
        echo json_encode(array('success' => 0, 'error_message' => 'Error - '.mysqli_error($mysql_connect_host)));
        exit; 
    } 

    $CashQuery = mysqli_fetch_array($CashQuery);

    $OthersQuery = "SELECT COALESCE(SUM(ticket_totalpayment),0) AS totalothers 
    FROM ticket
    WHERE ticket_payment_method != 0 AND ticket_box_movement = '%s' AND ticket_status = 1;";
    $OthersQuery = db_query_host(sprintf($OthersQuery,$JsonData["id"]));

    //On Query error
    if (!$OthersQuery) {
        echo json_encode(array('success' => 0, 'error_message' => 'Error - '.mysqli_error($mysql_connect_host)));
        exit; 
    } 

    $OthersQuery = mysqli_fetch_array($OthersQuery);


    ////////HHHHHHHHHHHH

    $CashQueryH = "SELECT COALESCE(SUM(ticket_totalpayment),0) AS totalcash
    FROM ticket_h
    WHERE ticket_payment_method = 0 AND ticket_box_movement = '%s' AND ticket_status = 1;";
    $CashQueryH = db_query_host(sprintf($CashQueryH,$JsonData["id"]));

    //On Query error
    if (!$CashQueryH) {
        echo json_encode(array('success' => 0, 'error_message' => 'Error - '.mysqli_error($mysql_connect_host)));
        exit; 
    } 

    $CashQueryH = mysqli_fetch_array($CashQueryH);


    $OthersQueryH = "SELECT COALESCE(SUM(ticket_totalpayment),0) AS totalothers 
    FROM ticket_h
    WHERE ticket_payment_method != 0 AND ticket_box_movement = '%s' AND ticket_status = 1;";
    $OthersQueryH = db_query_host(sprintf($OthersQueryH,$JsonData["id"]));

    //On Query error
    if (!$OthersQueryH) {
        echo json_encode(array('success' => 0, 'error_message' => 'Error - '.mysqli_error($mysql_connect_host)));
        exit; 
    } 

    $OthersQueryH = mysqli_fetch_array($OthersQueryH);

    //Response OK
    echo json_encode(array(
        'success'       =>1, 
        'comments'      => $BoxQuery["box_movement_comment"],
        'opendate'      => $BoxQuery["box_movement_opendate"],
        'openuser'      => $BoxQuery["openusername"],
        'closedate'     => $BoxQuery["box_movement_closedate"],
        'closeuser'     => $BoxQuery["closeusername"],
        'openvalue'     => $BoxQuery["box_movement_openvalue"],
        'closevalue'    => $BoxQuery["box_movement_closevalue"],
        'totalsell'     => (int)$CashQuery["totalcash"] + (int)$CashQueryH["totalcash"] + (int)$OthersQuery["totalothers"] + (int)$OthersQueryH["totalothers"],
        'cash'          => (int)$CashQuery["totalcash"] + (int)$CashQueryH["totalcash"],
        'others'        => (int)$OthersQuery["totalothers"] + (int)$OthersQueryH["totalothers"]));		
    exit;

}

?>