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
// function = Operation to realize of the user (1 = Create, 2 = Modify, 3 = Delete, 4 = List suppliers)

// data     = Json data to modify actual user information


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Suppliers: Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check user rol permissions
$permission = CheckRolPermissions($_POST['username'], $perm_suppliers);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        CreateSupplier(CheckData($_POST['data']));
    case 2:
        ModifySupplier(CheckData($_POST['data']));    
    case 3:
        DeleteSupplier(CheckData($_POST['data']));    
    case 4:
        ListSuppliers(); 
}

//Create New users
function CreateSupplier($JsonData){

    global $mysql_connect_host;
    global $permission;
    
    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["socialname"]) || empty($JsonData["comercialname"]) || empty($JsonData["documentid"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: Parametros de proveedor incorrectos.')));
    }

    $Query  = "INSERT INTO supplier
            (supplier_socialname, supplier_comercialname, supplier_documentid, supplier_idtype, supplier_phone, supplier_celphone, supplier_mail, supplier_address, supplier_contact, supplier_observation, supplier_status)
            VALUES ('%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["socialname"],$JsonData["comercialname"],$JsonData["documentid"],$JsonData["idtype"],$JsonData["phone"],
            $JsonData["celphone"],$JsonData["mail"],$JsonData["address"],$JsonData["contact"],$JsonData["observation"],$JsonData["status"]));

    $last_id = mysqli_insert_id($mysql_connect_host);
    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'Supplier: El proveedor '.$JsonData["socialname"].' ya existe'));
        exit;
    } else {           
        //Response OK
        echo json_encode(array('success' => 1, 'last_id' => ''.$last_id.''));		
        exit;
    }
}

function ModifySupplier($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"]) || empty($JsonData["socialname"]) || empty($JsonData["comercialname"]) || empty($JsonData["documentid"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: Parametros de proveedor incorrectos.')));
    }

    $Query =  "UPDATE supplier SET
        supplier_socialname='%s',
        supplier_comercialname='%s',
        supplier_documentid='%s',
        supplier_idtype='%s',
        supplier_phone='%s',
        supplier_celphone='%s',
        supplier_mail='%s',
        supplier_address='%s',
        supplier_contact='%s',
        supplier_observation='%s', supplier_status='%s' WHERE supplier_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["socialname"],$JsonData["comercialname"],$JsonData["documentid"],$JsonData["idtype"],$JsonData["phone"],
            $JsonData["celphone"],$JsonData["mail"],$JsonData["address"],$JsonData["contact"],$JsonData["observation"],$JsonData["status"],$JsonData["id"]));   

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: No se pudo modificar el proveedor')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}

function DeleteSupplier($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: No tiene los permisos para esta operación.')));
    }

    //Check JSON values
    if (empty($JsonData["id"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: Parametros de proveedor incorrectos.')));
    }
  
    $Query  = "DELETE FROM supplier WHERE supplier_id = '%s';";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"]));

    if (!$Query) {
         //Bad mysqli response
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: No se pudo eliminar el proveedor')));
        
    } else {           
        //Response OK
        echo json_encode(array('success'=>1));		
        exit;
    } 
}


//List all suppliers data
function ListSuppliers(){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Supplier: No tiene los permisos para esta operación.')));
    }
    
    $Query  = "SELECT * FROM supplier";
    $Query  = db_query_host($Query);
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'Suppliers: No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {

            $jsonData[] = array(
                'id'            =>      ''.$Results["supplier_id"].'',
                'socialname'    =>      ''.$Results["supplier_socialname"].'',
                'comercialname' =>      ''.$Results["supplier_comercialname"].'',
                'documentid'    =>      ''.$Results["supplier_documentid"].'',
                'idtype'        =>      ''.$Results["supplier_idtype"].'',
                'phone'         =>      ''.$Results["supplier_phone"].'',
                'celphone'      =>      ''.$Results["supplier_celphone"].'',
                'mail'          =>      ''.$Results["supplier_mail"].'',
                'address'       =>      ''.$Results["supplier_address"].'',                
                'contact'       =>      ''.$Results["supplier_contact"].'',
                'status'        =>      ''.$Results["supplier_status"].'',
                'observation'   =>      ''.$Results["supplier_observation"].'');           

        }
        
        //Response
        echo json_encode(array('success' => 1, 'suppliers' => $jsonData));	
        exit;
    }
}

?>