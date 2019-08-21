<?php
// REQUERES
require_once("_functions/configuration.php");
require_once("_functions/functions.php");
require_once("_functions/header.php");
header('Content-type: application/json');


//ON LOGOUT
if (!empty($_POST['token'])) {
    
    //TOKEN CHECK
    checkSecurityVariables($_POST['username'], $_POST['token']);

    $Query  = "UPDATE user SET user_token = NULL WHERE user_username = '%s'";
    $Query  = db_query_host(sprintf($Query, mysqli_real_escape_string($mysql_connect_host, $_POST['username'])));

    die(json_encode(array(
        'success' => 1)));
}

//POST CHECK
if (empty($_POST['username']) || empty($_POST['password'])) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'bad Request'      
    )));
}

//LOAD VARIABLES
$API_user        = strtolower($_POST["username"]);
$API_password    = strtoupper($_POST["password"]);

//Get app data status
$DataQuery  = "SELECT appdata_name, appdata_status FROM appdata";
$DataQuery  = mysqli_fetch_array(db_query_host($DataQuery));

if ($DataQuery["appdata_status"] == "0") {
    die(json_encode(array('success' => 0, 'error_message' => 'cuenta deshabilitada.')));
}

//LOGIN
$LoginQuery     = "SELECT user_id, user_username, user_password, user_rol, user_photo, user_realname FROM user WHERE user_username = '%s'";
$LoginQuery     = db_query_host(sprintf($LoginQuery, mysqli_real_escape_string($mysql_connect_host, $API_user)));
$LoginRows      = mysqli_num_rows($LoginQuery); 
$LoginResults   = mysqli_fetch_array($LoginQuery);

//CHECK RESULTS
if ($LoginRows == 0){

    //BAD LOGIN RESULT
    die(json_encode(array('success' => 0, 'error_message' => 'usuario incorrecto.')));

} else {

    //COMPARE PASSWORDS
    if ($LoginResults["user_password"] != $API_password){
        
        //BAD LOGIN RESULT
        die(json_encode(array('success' => 0, 'error_message' => 'clave incorrecta.')));

    } else {

        //SUCCESS LOGIN
        $token      =   generatetoken();

        //UPDATE TOKEN
        db_query_host(sprintf("UPDATE user SET user_token = '%s', user_lastlogin = NOW() WHERE user_id = %s ;", $token, $LoginResults["user_id"]));
        
        //LOGIN RESPONSE 
		echo json_encode(array(
            'success'       =>      1,
            'user'          =>      ''.$LoginResults["user_username"].'',
            'userid'        =>      $LoginResults["user_id"],
            'token'         =>      ''.$token.'',
            'userrol'       =>      $LoginResults["user_rol"],
            'realname'      =>      ''.$LoginResults["user_realname"].'',
            'photo'         =>      ''.$LoginResults["user_photo"].''));		
		exit;
    }
}

?>