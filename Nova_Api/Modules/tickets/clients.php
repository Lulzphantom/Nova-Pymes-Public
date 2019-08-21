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
$permission = CheckRolPermissions($_POST['username'], $perm_clients);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        CreateClient(CheckData($_POST['data']));
    case 2:
        ModifyClient(CheckData($_POST['data']));    
    case 3:
        DeleteClient(CheckData($_POST['data']));    
    case 4:
        ListClients(); 
}

//Create New users
function CreateClient($JsonData){

    global $mysql_connect_host;
    global $permission;
    
    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de cliente incorrectos.')));
    }

    $DocumentID = "NULL";

    if (!empty($JsonData["documentid"])) {
        $DocumentID = $JsonData["documentid"];
    }     

    $Query  = "INSERT INTO clients
	(client_name, client_idtype, client_documentid, client_address, client_phone, client_email, client_celphone, client_cancredit)
    VALUES ('%s', '%s', %s, '%s', '%s', '%s', '%s', %s);";    
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["type"],$DocumentID,$JsonData["address"],$JsonData["phone"],
            $JsonData["mail"],$JsonData["celphone"],$JsonData["cancredit"]));

    $last_id = mysqli_insert_id($mysql_connect_host);

    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'Error en el cliente '.$JsonData["name"].' '.mysqli_error($mysql_connect_host)));
        exit;
    } else {           
        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }
}

function ModifyClient($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de cliente incorrectos.')));
    }

    $DocumentID = "NULL";

    if (!empty($JsonData["documentid"])) {
        $DocumentID = $JsonData["documentid"];
    }  

    $Query =  "UPDATE clients
	SET
		client_name='%s',
		client_idtype='%s',
		client_documentid=%s,
		client_address='%s',
		client_phone='%s',
		client_email='%s',
		client_celphone='%s',
		client_cancredit=%s
	WHERE  client_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["type"],$DocumentID,$JsonData["address"],$JsonData["phone"],
            $JsonData["mail"],$JsonData["celphone"],$JsonData["cancredit"],$JsonData["id"]));   

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'No se pudo modificar el cliente - '.mysqli_error($mysql_connect_host))));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}

function DeleteClient($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Parametros de cliente incorrectos.')));
    }
  
    $Query  = "DELETE FROM clients WHERE client_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'No se pudo eliminar el proveedor')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}


//List all clients data
function ListClients(){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }
    
    $Query  = "SELECT * FROM clients";
    $Query  = db_query_host($Query);
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {

            $jsonData[] = array(
                'id'             =>      ''.$Results["client_id"].'',
                'name'           =>      ''.$Results["client_name"].'',
                'type'           =>      ''.$Results["client_idtype"].'',
                'documentid'     =>      ''.$Results["client_documentid"].'',
                'address'        =>      ''.$Results["client_address"].'',
                'phone'          =>      ''.$Results["client_phone"].'',
                'celphone'       =>      ''.$Results["client_celphone"].'',
                'mail'           =>      ''.$Results["client_email"].'',
                'cancredit'      =>      ''.$Results["client_cancredit"].'');           

        }
        
        //Response
        echo json_encode(array('success' => 1, 'clients' => $jsonData));	
        exit;
    }
}

?>