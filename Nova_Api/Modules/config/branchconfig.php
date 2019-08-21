<?php
// REQUERES
require_once("../../_functions/configuration.php");
require_once("../../_functions/functions.php");
require_once("../../_functions/header.php");
require_once("../../_functions/permissions.php");
header('Content-type: application/json');

//POST CHECK

// username = Username of the loged user.
// token    = Token of the loged user.
// function = Operation to realize of the branch (1 = Create, 2 = Modify, 3 = Delete, 4 = List branches)

// data     = Json data to modify actual role information (id, name, description)
//Jsondata  = {"id":"0","name":"test","address":"test","phone":"test","enabled":1}


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check user rol permissions
$permission = CheckRolPermissions($_POST['username'], $perm_branch);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        CreateBranch(CheckData($_POST['data']));
    case 2:
        ModifyBranch(CheckData($_POST['data']));    
    case 3:
        DeleteBranch(CheckData($_POST['data']));    
    case 4:
        ListBranch(); 
    case 5:
        ListBoxes(CheckData($_POST['data'])); 
    case 6:
        DeleteBoxes(CheckData($_POST['data'])); 
    case 7:
        CreateBox(CheckData($_POST['data'])); 
    case 8:
        ModifyBox(CheckData($_POST['data'])); 
}

//Create New rol
function CreateBranch($JsonData){

    global $mysql_connect_host;
    global $permission;
    
    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de sucursal incorrectos.')));
    }

    $Query  = "INSERT INTO branch (branch_name, branch_address, branch_phone) VALUES ('%s', '%s', '%s'); ";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["address"], $JsonData["phone"]));
    $last_id = mysqli_insert_id($mysql_connect_host);

    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'Branch: La sucursal '.$JsonData["name"].' ya existe'));
        exit;
    } else {      
        
        //Create banch product inventory
        $Query = "ALTER TABLE product ADD COLUMN %s INT(11) NOT NULL DEFAULT '0' AFTER product_unity_type;";
        $Query  = db_query_host(sprintf($Query,'branch_'.$last_id));

        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }       

}

function ModifyBranch($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de sucursal incorrectos.')));
    }

    $Query  = "UPDATE branch SET branch_name = '%s', branch_address = '%s', branch_phone = '%s' WHERE branch_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["address"],$JsonData["phone"],$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'No se pudo modificar la sucursal')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}

function DeleteBranch($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de sucursal incorrectos.')));
    }

    //Check for content data
    $Query  = "SELECT * FROM user WHERE user_branch = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));
    $Rows   = mysqli_num_rows($Query);

    if ($Rows != 0) {
         //Filled branch
         die(json_encode(array('success' => 0, 'error_message' => 'No se puede eliminar una sucursal con usuarios activos')));
    }


    $Query  = "DELETE FROM branch WHERE branch_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'No se pudo eliminar la sucursal')));
        
    } else {        
        
        $Query  = "ALTER TABLE product DROP COLUMN %s;";
        $Query  = db_query_host(sprintf($Query,'branch_'.$JsonData["id"]));

        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}


//List all rol data
function ListBranch(){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }
    
    $Query  = "SELECT branch_id, branch_name, branch_address, branch_phone, branch_enabled, COUNT(box_id) AS boxcount
    FROM branch 
    LEFT JOIN branch_boxes ON box_branch = branch_id 
    GROUP BY branch_id;";
    $Query  = db_query_host($Query);
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {

         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'No se encontraron resultados.')));
        
    } else {   
        
        while ($Results = mysqli_fetch_array($Query)) {

            $jsonData[] = array(
                'id'         =>      ''.$Results["branch_id"].'',
                'name'       =>      ''.$Results["branch_name"].'',
                'address'   =>      ''.$Results["branch_address"].'',
                'phone'      =>      ''.$Results["branch_phone"].'',
                'boxes'      =>      ''.$Results["boxcount"].'');    
        }
        
        //Response
        echo json_encode(array('success' => 1, 'branch' => $jsonData));	
        exit;
    }
}

//List boxes data by branch
function ListBoxes($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Branch: No tiene los permisos para esta operación.')));
    }

    $Query = "SELECT * FROM branch_boxes WHERE box_branch = %s";
    $Query  = db_query_host(sprintf($Query,$JsonData["branch_id"]));
    $Rows   = mysqli_num_rows($Query); 
    
    if ($Rows == 0) {
        //DB ERROR
       die(json_encode(array('success' => 0, 'error_message' => 'No se encontraron resultados.')));       
   } else {          
       while ($Results = mysqli_fetch_array($Query)) {
           $jsonData[] = array(
               'id'         =>      ''.$Results["box_id"].'',
               'name'       =>      ''.$Results["box_name"].'',
               'status'       =>      ''.$Results["box_status"].'',
               'branch'     =>      ''.$Results["box_branch"].'');    
       }       
       //Response
       echo json_encode(array('success' => 1, 'boxes' => $jsonData));	
       exit;    
    }
}

function DeleteBoxes($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de punto de venta incorrectos.')));
    }

    //BILL VERIFICATION----------------------------------------------------

    $Query  = "DELETE FROM branch_boxes WHERE box_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'No se pudo eliminar el punto de venta')));
        
    } else {        
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    }     
}

function CreateBox($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de punto de venta incorrectos.')));
    }

    $Query  = "INSERT INTO branch_boxes (box_name, box_branch, box_status) VALUES ('%s', '%s', '%s'); ";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["branch_id"], $JsonData["status"]));
    $last_id = mysqli_insert_id($mysql_connect_host);

    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'Error al crear el punto de venta'.mysqli_error($mysql_connect_host)));
        exit;
    } else {
        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }  


}

function ModifyBox($JsonData){
    
    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

     //Check JSON values
     if (empty($JsonData["id"]) || empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de punto de venta incorrectos.')));
    }

    $Query  = "UPDATE branch_boxes SET box_name = '%s', box_status = '%s' WHERE box_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["status"],$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'No se pudo modificar el punto de venta')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}
?>