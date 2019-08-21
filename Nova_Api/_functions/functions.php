<?php

        /*
            DB FUNCTIONS - HOST ADMIN
        */
        function db_query_host( $sql ){
            global $mysql_connect_host;
            $db_query_sql = mysqli_query($mysql_connect_host, $sql);
            return $db_query_sql;
        }
        function db_select_host( $db ){
            global $mysql_connect_host;
            mysqli_select_db($mysql_connect_host, $db) or die(json_encode(array('success' => 0,   'error_message' => 'API - Error host function' )));
        }


        /*
            DB FUNCTIONS - GENERAL
        */

        function generateToken (){
            
		    $token = 0;
		    $token_length = 20;
	    	$token_characters = "0123456789abcdefqhijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; 

            for ($i=0; $i < $token_length ; $i++) { 
                $token .= $token_characters[mt_rand(0, $token_length)];
            }

            return $token;
        }


        /*
        CHECK VARIABLES - APP
       */
        function checkSecurityVariables ($User, $Hash) {
        if ((empty($User)) || (empty($Hash))) {
            die(json_encode(array('success' => 0,   'error_message' => 'Bad Request' )));
        } else {                
            global $mysql_connect_host;    
            $_User = mysqli_real_escape_string($mysql_connect_host ,strtolower($User));
            $LoginQuery = db_query_host("SELECT user_token FROM user WHERE user_username = '".$_User."';");
            $Api_Hash = mysqli_fetch_array($LoginQuery);                
            if ($Hash == $Api_Hash["user_token"]){                 
            } else {                    
                die(json_encode(array('success' => 0,   'error_message' => 'Bad token request' )));
            }
        }           
    }


    function GetUsernameID ($User) {
        if ((empty($User))) {
            die(json_encode(array('success' => 0,   'error_message' => 'Bad Request' )));
        } else {                
            global $mysql_connect_host;    
            $_User = mysqli_real_escape_string($mysql_connect_host ,strtolower($User));
            $LoginQuery = db_query_host("SELECT user_id FROM user WHERE user_username = '".$_User."';");
            $UserID = mysqli_fetch_array($LoginQuery);                
            
            return $UserID["user_id"];
        }           
    }

//CHECK JSON DATA
function CheckData($data){
    if ( empty($data) ) {
        die(json_encode(array(
          'success' => 0, 'error_message' => 'Array Bad Request'      
        )));
    }
    //Set JSON to Array
    $Variables = json_decode($data,true);     

    if (json_last_error() == JSON_ERROR_NONE) {
        //Return valid json array
        return $Variables;

    } else {
        die(json_encode(array(
            'success' => 0, 'error_message' => 'Bad Data Request'      
          )));
    }
}

//CHECK USER ROL PERMISSION
//Consult, Create/Modify, Delete
function CheckRolPermissions($username, $permissionID){

    global $mysql_connect_host;    

    //Select user rol
    $user = mysqli_real_escape_string($mysql_connect_host ,strtolower($username));
    $Query = db_query_host("SELECT user_rol FROM user WHERE user_username = '".$user."';");
    $Result = mysqli_fetch_array($Query);

    //Select user rol data
    $Query = db_query_host(sprintf("SELECT userrol_data FROM userrol WHERE userrol_id = %s;", $Result["user_rol"]));
    $Result = mysqli_fetch_array($Query);

    $permissions = CheckData($Result["userrol_data"]);

    //Check permissions for match permission ID
    foreach ($permissions as $value) {
        if ($value["id"] == $permissionID ) {
            return $value["permissions"];
        }
    }

    return 0;    
}
?>