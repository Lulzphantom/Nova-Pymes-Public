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
// function = Operation to realize of the user (1 = Create, 2 = Modify, 3 = Delete, 4 = List users)

// data     = Json data to modify actual user information (id, name, password, rol, branch, photo)


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Users: Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check user rol permissions
$permission = CheckRolPermissions($_POST['username'], $perm_accounts);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        CreateUser(CheckData($_POST['data']));
    case 2:
        ModifyUser(CheckData($_POST['data']));    
    case 3:
        DeleteUser(CheckData($_POST['data']));    
    case 4:
        ListUsers(); 
}

//Create New users
function CreateUser($JsonData){

    global $mysql_connect_host;
    global $permission;
    
    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Users: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["name"]) || empty($JsonData["hash"]) || empty($JsonData["rolid"]) || empty($JsonData["branchid"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Users: Parametros de usuario incorrectos.')));
    }

    if ($JsonData["rolid"] == "1") {
        die(json_encode(array('success' => 0, 'error_message' => 'Users: El usuario no puede ser rol superadmin.')));
    }

    $Query  = "INSERT INTO user (user_username, user_realname, user_password, user_rol, user_branch, user_photo, user_enabled) VALUES ('%s', '%s', '%s', '%s', '%s', '%s', '%s'); ";
    $Query  = db_query_host(sprintf($Query,strtolower($JsonData["name"]),$JsonData["realname"], strtoupper($JsonData["hash"]),$JsonData["rolid"],$JsonData["branchid"],$JsonData["photo"],$JsonData["status"]));

    $last_id = mysqli_insert_id($mysql_connect_host);
    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'Users: El usuario '.$JsonData["name"].' ya existe'));
        exit;
    } else {           
        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }
}

function ModifyUser($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Users: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Users: Parametros de sucursal incorrectos.')));
    }

    $Query =  "UPDATE user SET user_username = '%s', user_realname = '%s', user_password = '%s', user_rol = '%s', user_branch = '%s', user_photo = '%s', user_enabled = '%s' WHERE user_id = '%s';";

    if (empty($JsonData["hash"]) ) {
        //Modify without changed password
        $Query =  "UPDATE user SET user_username = '%s', user_realname = '%s', user_rol = '%s', user_branch = '%s', user_photo = '%s', user_enabled = '%s' WHERE user_id = '%s';";
        $Query  = db_query_host(sprintf($Query,strtolower($JsonData["name"]),$JsonData["realname"],$JsonData["rolid"],$JsonData["branchid"],$JsonData["photo"],$JsonData["status"],$JsonData["id"]));

    } else{
        //Modify change password
        $Query  = db_query_host(sprintf($Query,strtolower($JsonData["name"]),$JsonData["realname"],strtoupper($JsonData["hash"]),$JsonData["rolid"],$JsonData["branchid"],$JsonData["photo"],$JsonData["status"],$JsonData["id"]));
    }    

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Users: No se pudo modificar el usuario')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}

function DeleteUser($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Users: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || $JsonData["id"] == "1" ) {
        die(json_encode(array('success' => 0, 'error_message' => 'Users: Parametros de usuario incorrectos.')));
    }
  
    $Query  = "DELETE FROM user WHERE user_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Users: No se pudo eliminar el usuario')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}


//List all rol data
function ListUsers(){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Users: No tiene los permisos para esta operación.')));
    }
    
    $Query  = "SELECT user_id, user_username, user_rol, user_realname,
    user_branch, user_photo, user_enabled, branch_name, userrol_name, userrol_description FROM user
    LEFT JOIN userrol ON userrol_id = user_rol
    LEFT JOIN branch ON branch_id = user_branch
    GROUP BY user_id
    ;";
    $Query  = db_query_host($Query);
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'Users: No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {

            $jsonData[] = array(
                'id'         =>      ''.$Results["user_id"].'',
                'name'       =>      ''.$Results["user_username"].'',
                'rolid'      =>      ''.$Results["user_rol"].'',
                'rolname'    =>      ''.$Results["userrol_name"].'',
                'roldescrip' =>      ''.$Results["userrol_description"].'',
                'realname'   =>      ''.$Results["user_realname"].'',
                'photo'      =>      ''.$Results["user_photo"].'',
                'branchid'   =>      ''.$Results["user_branch"].'',
                'branchname' =>      ''.$Results["branch_name"].'',
                'status'     =>      ''.$Results["user_enabled"].'');           

        }
        
        //Response
        echo json_encode(array('success' => 1, 'users' => $jsonData));	
        exit;
    }
}

?>