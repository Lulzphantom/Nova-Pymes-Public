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
// function = Operation to realize of the role (1 = Create, 2 = Modify, 3 = Delete, 4 = List roles)

// data     = Json data to modify actual role information (id, name, description)
//Jsondata  = {"id":"0","name":"test","description":"test"}


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Roles: Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check user rol permissions
$permission = CheckRolPermissions($_POST['username'], $perm_userrol);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        CreateRol(CheckData($_POST['data']));
    case 2:
        ModifyRol(CheckData($_POST['data']));    
    case 3:
        DeleteRol(CheckData($_POST['data']));    
    case 4:
        ListRol(); 
}

//Create New rol
function CreateRol($JsonData){

    global $mysql_connect_host;
    global $permission;
    
    $default_permissions = file_get_contents('permissions_structure.json');

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["name"]) || empty($JsonData["description"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: Parametros de rol incorrectos.')));
    }

    $Query  = "INSERT INTO userrol (userrol_name, userrol_description, userrol_data) VALUES ('%s', '%s', '%s'); ";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["description"], $default_permissions));


    $last_id = mysqli_insert_id($mysql_connect_host);
    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: El rol '.$JsonData["name"].' ya existe')));
        
    } else {           
        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }       

}

function ModifyRol($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: Parametros de rol incorrectos.')));
    }

    $Query  = "UPDATE userrol SET userrol_name = '%s', userrol_description = '%s' WHERE userrol_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["description"],$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: No se pudo modificar el rol')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 


}

function DeleteRol($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: Parametros de rol incorrectos.')));
    }

    $Query  = "DELETE FROM userrol WHERE userrol_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: No se pudo eliminar el rol')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}


//List all rol data
function ListRol(){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Roles: No tiene los permisos para esta operación.')));
    }
    
    $Query  = "SELECT userrol_id, userrol_name, userrol_description, userrol_data, COUNT(user_rol) AS usercount FROM userrol LEFT JOIN user ON user_rol = userrol_id GROUP BY userrol_id;";
    $Query  = db_query_host($Query);
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {

         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'Error en la base de datos al consutlar el rol de usuario.')));
        
    } else {   
        
        while ($Results = mysqli_fetch_array($Query)) {

            //Encode data from database to JSON
            $data = json_decode($Results["userrol_data"]);

            $jsonData[] = array(
                'rolid'         =>      ''.$Results["userrol_id"].'',
                'rolname'       =>      ''.$Results["userrol_name"].'',
                'roldescription'=>      ''.$Results["userrol_description"].'',
                'usercount'     =>      ''.$Results["usercount"].'',
                'roldata'       =>      $data);           

        }
        
        //Response
        echo json_encode(array('success' => 1, 'userrols' => $jsonData));	
        exit;
    }
}

?>