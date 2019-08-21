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
      'success' => 0, 'error_message' => 'Product: Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check products permissions
$permission = CheckRolPermissions($_POST['username'], $perm_productcategory);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        CreateProduct(CheckData($_POST['data']));
    case 2:
        ModifyProduct(CheckData($_POST['data']));    
    case 3:
        DeleteProduct(CheckData($_POST['data']));    
    case 4:
        ListProduct(CheckData($_POST['data'])); 
    case 5:
        ListPrice(CheckData($_POST['data'])); 
    case 6:
        ModifyPrice(CheckData($_POST['data'])); 
}

//Create New Product
function CreateProduct($JsonData){

    global $mysql_connect_host;
    global $permission;
    
    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["name"]) || empty($JsonData["code"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: Parametros de producto incorrectos.')));
    }

    //SEARCH FOR EXISTING PRODUCT NAME OR CODE



    $Query  = "INSERT INTO product
	(product_name, product_category, product_code, product_costprice, product_sellprice, product_minstock, product_maxstock, product_unity_type, product_h, product_iva, product_iac, product_iva5)
	VALUES ('%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["category"],$JsonData["code"],$JsonData["costprice"]
    ,$JsonData["sellprice"],$JsonData["minstock"],$JsonData["maxstock"],$JsonData["unity_type"],$JsonData["hproduct"],$JsonData["iva"],$JsonData["iac"],$JsonData["iva5"]));

    $last_id = mysqli_insert_id($mysql_connect_host);
    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'producto '.$JsonData["name"].' ' .mysqli_error($mysql_connect_host)));
        exit;
    } else {           
        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }
}

function ModifyProduct($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["name"]) || empty($JsonData["code"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: Parametros de producto incorrectos.')));
    }

    $Query =  "UPDATE product
	SET
		product_name='%s',
		product_category='%s',
		product_code='%s',
		product_costprice='%s',
		product_sellprice='%s',
		product_minstock='%s',
		product_maxstock='%s',
		product_unity_type='%s',
        product_iva = '%s',
        product_iac = '%s',
        product_iva5 = '%s'
    WHERE product_id='%s';";
    
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],
    $JsonData["category"],$JsonData["code"],$JsonData["costprice"],
    $JsonData["sellprice"],$JsonData["minstock"],$JsonData["maxstock"],
    $JsonData["unity_type"],$JsonData["iva"],$JsonData["iac"],$JsonData["iva5"],$JsonData["id"]));   

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No se pudo modificar el producto')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}

function DeleteProduct($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: Parametros de producto incorrectos.')));
    }

    //------------------------------------

    $Query  = "DELETE FROM product WHERE product_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    //------------------------------------

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No se pudo eliminar el producto')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}


//List all category data
function ListProduct($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No tiene los permisos para esta operación.')));
    }
    

    $Count = "SELECT COUNT(product_id) as count FROM product WHERE product_h = %s";
    $Count = db_query_host(sprintf($Count,$JsonData["h"]));
    $Count = mysqli_fetch_array($Count);

    $From = 0;
    $Filter = "";

    //Pagination 
    if (!empty($JsonData["from"])) {
        $From = $JsonData["from"];
    }

    //Filter
    if (!empty($JsonData["filter"])) {
        $Filter = sprintf(" AND (product_name LIKE '%%%s%%' OR product_code LIKE '%%%s%%')",mysqli_real_escape_string($mysql_connect_host, $JsonData["filter"]),mysqli_real_escape_string($mysql_connect_host, $JsonData["filter"]));
    }

    $Query  = "SELECT * FROM product WHERE product_h = %s %s  ORDER BY product_name LIMIT %s,15";
    $Query  = db_query_host(sprintf($Query, $JsonData["h"], $Filter, $From));
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {


            $Branch = $Results["branch_".$JsonData["branch"]];

            if ($JsonData["h"] == "1") {
                $Branch = $Results["product_branch_h"];
            }

            $jsonData[] = array(
                'id'            =>      ''.$Results["product_id"].'',
                'name'          =>      ''.$Results["product_name"].'',
                'category'      =>      ''.$Results["product_category"].'',
                'code'          =>      ''.$Results["product_code"].'',
                'iva'           =>      ''.$Results["product_iva"].'',
                'iva5'           =>      ''.$Results["product_iva5"].'',
                'iac'           =>      ''.$Results["product_iac"].'',
                'costprice'     =>      ''.$Results["product_costprice"].'',
                'sellprice'     =>      ''.$Results["product_sellprice"].'',
                'minstock'      =>      ''.$Results["product_minstock"].'',
                'maxstock'      =>      ''.$Results["product_maxstock"].'',
                'branch_count'  =>      ''.$Branch.'',
                'description'   =>      ''.$Results["product_description"].'',
                'unity_type'    =>      ''.$Results["product_unity_type"].'');   
        }
        
        //Response
        echo json_encode(array('success' => 1, 'count' => $Count["count"], 'product' => $jsonData));	
        exit;
    }
}

function ListPrice($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No tiene los permisos para esta operación.')));
    }
        
    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: Parametros de producto incorrectos.')));
    }

    $Query  = "SELECT * FROM product_price;";
    $Query  = db_query_host($Query);
    $Rows   = mysqli_num_rows($Query);    
    
    if ($Rows != 0) {
        
        while ($Results = mysqli_fetch_array($Query)) {

            $Value = $Results["price_value"];
            
            if ($Results["price_type"] == "0") {
             
                $Price_query = "SELECT %s FROM product WHERE product_id = '%s';";
                $Price_query = db_query_host(sprintf($Price_query,'price_'.$Results["price_id"],$JsonData["id"]));
                $Price_query = mysqli_fetch_array($Price_query);


                $Value = $Price_query['price_'.$Results["price_id"]];
            }
            
            $jsonData[] = array(
                'id'            =>      ''.$Results["price_id"].'',
                'name'          =>      ''.$Results["price_name"].'',
                'type'          =>      ''.$Results["price_type"].'',
                'value'         =>      ''.$Value.'');

        }
        //Response
        echo json_encode(array('success' => 1, 'prices' => $jsonData));	
        exit;       
    } else {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No se encontraron resultados')));
    }
}

function ModifyPrice($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: No tiene los permisos para esta operación.')));
    }
        
    //Check JSON values
    if (empty($JsonData["price_id"]) || empty($JsonData["product_id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Product: Parametros de producto incorrectos.')));
    }

    $Query = "UPDATE product SET %s = '%s' WHERE product_id = '%s'";
    $Query = db_query_host(sprintf($Query,'price_'.$JsonData["price_id"],$JsonData["price_value"],$JsonData["product_id"]));

    if (!$Query) {
        //Bad mysqli response
       die(json_encode(array('success' => 0, 'error_message' => 'Product: No se pudo modificar el precio')));
       
   } else {           
       //Response OK
       echo json_encode(array('success'=>1));		
       exit;
   } 
}

?>