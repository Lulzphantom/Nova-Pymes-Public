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
// userid   = ID of the user to operate.
// function = Operation to realize of the role permissions (1 = Consult, 2 = Modify)

// data     = Json data to modify actual role permissions
// jsondata = {"id":"0","permissions": [{"id": 0, "name": "test", "permissions": {"create": 0, "delete": 0, "consult": 0}}]}


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Permisssions: Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);



//Check user rol permissions
$permission = CheckRolPermissions($_POST['username'], $perm_userpermission);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:
        //SET VARIABLES
        
        if (empty($_POST['userid'])) {
            die(json_encode(array(
            'success' => 0, 'error_message' => 'Permisssions: Bad Request userid'      
            )));
        } 
        ConsultPermissions($_POST['userid']);
    case 2:
        ModifyPermissions(CheckData($_POST['data']));     
}

//CONSULT FUNCTION
function ConsultPermissions($id){
    
    global $mysql_connect_host;   

    $Query  = "SELECT user_rol FROM user WHERE user_id = '%s'";
    $Query  = db_query_host(sprintf($Query, mysqli_real_escape_string($mysql_connect_host, $id)));
    $Rows   = mysqli_num_rows($Query); 
    $Results= mysqli_fetch_array($Query);
    
    if ($Rows == 0) {

        //Bad user id
        die(json_encode(array('success' => 0, 'error_message' => 'El usuario no existe.')));

    } else {

        $Query  = "SELECT * FROM userrol WHERE userrol_id = '%s'";
        $Query  = db_query_host(sprintf($Query, mysqli_real_escape_string($mysql_connect_host, $Results["user_rol"])));
        $Rows   = mysqli_num_rows($Query); 
        $Results= mysqli_fetch_array($Query);

        if ($Rows == 0) {

             //Bad user rol
            die(json_encode(array('success' => 0, 'error_message' => 'El rol de usuario no existe.')));
            
        } else {   
            
            //Encode data from database to JSON
            $data = json_decode($Results["userrol_data"]);
            //Response
            echo json_encode(array(
                'success'       =>      1,
                'rolname'       =>      ''.$Results["userrol_name"].'',
                'roldescription'=>      ''.$Results["userrol_description"].'',
                'roldata'       =>      $data));		
            exit;

        }

    }
}


function ModifyPermissions($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Permisos: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["permissions"]) ) {
        die(json_encode(array('success' => 0, 'error_message' => 'Permisos: Parametros de permisos incorrectos.')));
    }

    $Query  = "UPDATE userrol SET userrol_data = '%s' WHERE userrol_id = '%s';";
    $Query  = db_query_host(sprintf($Query,json_encode($JsonData["permissions"]),mysqli_real_escape_string($mysql_connect_host, $JsonData["id"])));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Permisos: No se pudo modificar el permiso')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}

?>