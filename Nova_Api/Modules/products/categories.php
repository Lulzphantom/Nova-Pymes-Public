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
      'success' => 0, 'error_message' => 'Category: Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check products permissions
$permission = CheckRolPermissions($_POST['username'], $perm_productcategory);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        CreateCategory(CheckData($_POST['data']));
    case 2:
        ModifyCategory(CheckData($_POST['data']));    
    case 3:
        DeleteCategory(CheckData($_POST['data']));    
    case 4:
        ListCategory(); 
}

//Create New category
function CreateCategory($JsonData){

    global $mysql_connect_host;
    global $permission;
    
    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Category: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["name"]) || empty($JsonData["code"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Category: Parametros de proveedor incorrectos.')));
    }

    $Query  = "INSERT INTO product_category
            (category_name, category_code)
            VALUES ('%s', '%s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["code"]));

    $last_id = mysqli_insert_id($mysql_connect_host);
    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'Category: La categoria '.$JsonData["name"].' ya existe'));
        exit;
    } else {           
        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }
}

function ModifyCategory($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Category: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["name"]) || empty($JsonData["code"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Category: Parametros de categoria incorrectos.')));
    }

    $Query =  "UPDATE product_category SET
        category_name='%s',
        category_code='%s'
        WHERE category_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["name"],$JsonData["code"],$JsonData["id"]));   

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Category: No se pudo modificar la categoria')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}

function DeleteCategory($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Category: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Category: Parametros de categoria incorrectos.')));
    }

    $Query  = "DELETE FROM product_category WHERE category_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Category: No se pudo eliminar la categoria')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}


//List all category data
function ListCategory(){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Category: No tiene los permisos para esta operación.')));
    }
    
    $Query  = "SELECT category_id, category_name, category_code, COUNT(product_id) AS products FROM product_category LEFT JOIN product ON product_category = category_id GROUP BY category_id;";
    $Query  = db_query_host($Query);
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'Category: No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {

            $jsonData[] = array(
                'id'            =>      ''.$Results["category_id"].'',
                'name'          =>      ''.$Results["category_name"].'',
                'code'          =>      ''.$Results["category_code"].'',
                'products'      =>      ''.$Results["products"].'');   
        }
        
        //Response
        echo json_encode(array('success' => 1, 'category' => $jsonData));	
        exit;
    }
}

?>