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
// function = Operation to realize of the category (1 = Create, 2 = Modify, 3 = Delete, 4 = List suppliers)

// data     = Json data to modify actual category information


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Price: Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check products permissions
$permission = CheckRolPermissions($_POST['username'], $perm_productcategory);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        CreatePrice(CheckData($_POST['data']));
    case 2:
        ModifyPrice(CheckData($_POST['data']));    
    case 3:
        DeletePrice(CheckData($_POST['data']));    
    case 4:
        ListPrice(); 
}

//Create New Product
function CreatePrice($JsonData){

    global $mysql_connect_host;
    global $permission;
    
    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Price: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Price: Parametros de producto incorrectos.')));
    }

    $Query  = "INSERT INTO product_price
	(price_name, price_type, price_value)
	VALUES ('%s', '%s', '%s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["type"],$JsonData["value"]));

    $last_id = mysqli_insert_id($mysql_connect_host);
    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'Price: El precio '.$JsonData["name"].' ya existe'));
        exit;
    } else {         

        //Create price Column on price_type 0
        if ($JsonData["type"] == 0) {

            $Query = "ALTER TABLE product ADD COLUMN %s INT(11) NOT NULL DEFAULT '0';";
            $Query  = db_query_host(sprintf($Query,'price_'.$last_id));
        } 

        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }
}

function ModifyPrice($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Price: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["name"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Price: Parametros de precio incorrectos.')));
    }

    $Query =  "UPDATE product_price
	SET
		price_name='%s',
        price_value = '%s'
    WHERE price_id='%s';";
    
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],
    $JsonData["value"],$JsonData["id"]));   

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Price: No se pudo modificar el producto')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}

function DeletePrice($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Price: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Price: Parametros de precio incorrectos.')));
    }

    //Chech price type
    $TypeQuery  = "SELECT price_type FROM product_price WHERE price_id = '%s';";
    $TypeQuery  = db_query_host(sprintf($TypeQuery,$JsonData["id"]));    
    $Rows       = mysqli_num_rows($TypeQuery);   
    if ($Rows > 0) {        
        $TypeQuery  = mysqli_fetch_array($TypeQuery);
        $TypeQuery  = $TypeQuery["price_type"];
    } else{
        die(json_encode(array('success' => 0, 'error_message' => 'Price: No se pudo eliminar el producto, no se encontraron resultados.')));
    }

    $Query  = "DELETE FROM product_price WHERE price_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Price: No se pudo eliminar el producto')));
        
    } else {
        if ($TypeQuery == '0') {
            $Query  = "ALTER TABLE product DROP COLUMN %s;";
            $Query  = db_query_host(sprintf($Query,'price_'.$JsonData["id"]));
        }
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}


//List all prices data
function ListPrice(){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Price: No tiene los permisos para esta operación.')));
    }

    $Query  = "SELECT * FROM product_price";
    $Query  = db_query_host($Query);
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'Price: No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {
            $jsonData[] = array(
                'id'            =>      ''.$Results["price_id"].'',
                'name'          =>      ''.$Results["price_name"].'',
                'type'          =>      ''.$Results["price_type"].'',
                'value'         =>      ''.$Results["price_value"].'');   
        }
        
        //Response
        echo json_encode(array('success' => 1, 'price' => $jsonData));	
        exit;
    }
}

?>