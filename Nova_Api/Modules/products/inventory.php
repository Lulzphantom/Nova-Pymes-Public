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

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        TransferProduct(CheckData($_POST['data']), GetUsernameID($_POST['username']));
    case 2:
        UpdateCostPrice(CheckData($_POST['data']));    
    case 3:
        InventoryIn(CheckData($_POST['data']), GetUsernameID($_POST['username']));    
    case 4:
        ListProduct(CheckData($_POST['data']));
    case 5:
        Adjustproduct(CheckData($_POST['data']),GetUsernameID($_POST['username']));
    case 6:
        ListLowProducts(CheckData($_POST['data']));
    case 7:
        ListMovements(CheckData($_POST['data']));
}

//Transfer product from branch to another branch
function TransferProduct($JsonData,$UserID){

    global $mysql_connect_host;
    global $perm_inventorymodify;

    //Check inventory permissions
    $permission = CheckRolPermissions($_POST['username'], $perm_inventorymodify );

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No tiene los permisos para esta operación.')));
    }    

    //Create inventory Movement
    $Query  = "INSERT INTO inventory_movements
	(movement_branch, movement_type, movement_owner, movement_comment)
	VALUES ('%s', '%s', '%s', '%s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["branch_id"],"3",$UserID,$JsonData["comment"]));

     if (!$Query) {
            //Bad mysqli response
           echo json_encode(array('success' => 0, 'error_message' => 'Inventory: Error al registrar el movimiento - '.mysqli_error($mysql_connect_host)));
           exit; }

    //Movement ID
    $movement_id = mysqli_insert_id($mysql_connect_host);    

    //Modify product count on both branch
    $Query = "UPDATE product SET %s = %s - %s, %s = %s + %s WHERE product_id = '%s'";
    $Query = db_query_host(sprintf($Query,'branch_'.$JsonData["from_branch"],'branch_'.$JsonData["from_branch"],
                                    $JsonData["product_count"],'branch_'.$JsonData["to_branch"],'branch_'.$JsonData["to_branch"],$JsonData["product_count"], $JsonData["product_id"]));

    if (!$Query) {
        //Bad mysqli response
       echo json_encode(array('success' => 0, 'error_message' => 'Inventory: error al modificar las cantidades de inventario'.mysqli_error($mysql_connect_host)));
       exit; }

    //Create inventory transfer data
    $Query = "INSERT INTO inventory_transfer
	(transfer_movement, transfer_from_branch, transfer_to_branch, transfer_product, transfer_count)
	VALUES ('%s', '%s', '%s', '%s', %s);";  
    $Query = db_query_host(sprintf($Query,$movement_id,$JsonData["from_branch"],$JsonData["to_branch"],$JsonData["product_id"],$JsonData["product_count"]));  

    if (!$Query) {
        //Bad mysqli response
       echo json_encode(array('success' => 0, 'error_message' => 'Inventory: Error al ingresar el movimiento de traslado - '.mysqli_error($mysql_connect_host)));
       exit; } else {
        echo json_encode(array('success' => 1));		
        exit;
    }
}

//Update single product cost price
function UpdateCostPrice($JsonData){

}

//Inset product count on brach
function InventoryIn($JsonData, $UserID){

    global $mysql_connect_host;
    global $perm_inventoryin;   

    //Check inventory permissions
    $permission = CheckRolPermissions($_POST['username'], $perm_inventoryin );

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No tiene los permisos para esta operación.')));
    }    

    //Check JSON values
    if (empty($JsonData["branch"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: Parametros de inventario incorrectos.')));
    }

    //Create inventory Movement
    $Query  = "INSERT INTO inventory_movements
	(movement_branch, movement_type, movement_owner, movement_comment)
	VALUES ('%s', '%s', '%s', '%s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["branch"],"0",$UserID,$JsonData["comment"]));

    $last_id = mysqli_insert_id($mysql_connect_host);

    if (!$Query) {
         //Bad mysqli response
        echo json_encode(array('success' => 0, 'error_message' => 'Inventory: Error al ingresar el movimiento'));
        exit;
        
    } else {          

        if (empty($JsonData["expiration_date"])) {
            $JsonData["expiration_date"] = "null";
        } else{
            $JsonData["expiration_date"] = '"'.$JsonData["expiration_date"].'"';
        }

        //Create supplier expend
        $Query  = "INSERT INTO supplier_expend
	    (supplier_expend_supplier, supplier_expend_movement, supplier_expend_bill, supplier_expend_comment, supplier_expend_value, 
        supplier_expend_paymentvalue, supplier_expend_payment_type, supplier_expend_payment_method, supplier_expend_expiration)
	    VALUES ('%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', %s);";
        $Query  = db_query_host(sprintf($Query,$JsonData["supplier_id"],$last_id,$JsonData["bill"],$JsonData["comment"],$JsonData["value"],$JsonData["payment"],
        $JsonData["payment_type"],$JsonData["payment_method"],$JsonData["expiration_date"]));        

        if (!$Query) {
            //Bad mysqli response
           echo json_encode(array('success' => 0, 'error_message' => 'Inventory:'.mysqli_error($mysql_connect_host)));
           exit; }

        //Create products in
        foreach ($JsonData["products"] as $key) {
           
            //Insert product in
            $Query = "INSERT INTO inventory_in
            (in_movement, in_product, in_branch, in_count, in_cost, in_iva, in_iva5, in_iac)
            VALUES ('%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s');";
            $Query  = db_query_host(sprintf($Query,$last_id,$key["product_db_id"],$key["product_branch"],$key["product_count"],$key["product_cost"],$key["product_iva"],$key["product_iva5"],$key["product_iac"]));

            if (!$Query) {
                //Bad mysqli response
               echo json_encode(array('success' => 0, 'error_message' => 'Inventory:'.mysqli_error($mysql_connect_host)));
               exit; }


            //Modify product count on branch
            $Query = "UPDATE product SET %s = %s + %s WHERE product_id = '%s'";
            $Query = db_query_host(sprintf($Query,'branch_'.$key["product_branch"],'branch_'.$key["product_branch"],$key["product_count"],$key["product_db_id"]));
       
            if (!$Query) {
                //Bad mysqli response
               echo json_encode(array('success' => 0, 'error_message' => 'Inventory:'.mysqli_error($mysql_connect_host)));
               exit; }
        }   
        echo json_encode(array('success' => 1));		
        exit;
    }
}

//List all inventory data
function ListProduct($JsonData){

    global $mysql_connect_host;
    global $perm_inventorylist;

    //Check inventory permissions
    $permission = CheckRolPermissions($_POST['username'], $perm_inventorylist );

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No tiene los permisos para esta operación.')));
    }    

    //List Branch
    $Branch_Query = "SELECT branch_id, branch_name FROM branch ORDER BY branch_id";
    $Branch_Query = db_query_host($Branch_Query);
    $Branch_Rows  = mysqli_num_rows($Branch_Query);
    $Branch_Query = mysqli_fetch_all($Branch_Query);
   

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
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {

            $BranchData = array();

            for ($i=0; $i < $Branch_Rows + 1; $i++) { 
                if ($i == $Branch_Rows) {
                    $BranchData[$i] = array(
                        'name'      =>      'H',
                        'count'     =>      ''.$Results["product_branch_h"].''
                    );  
                }else{
                    $BranchData[$i] = array(
                        'name'      =>      ''.$Branch_Query[$i][1].'',
                        'count'     =>      ''.$Results[sprintf("branch_%s",$Branch_Query[$i][0])].''
                    );  
                }                              
            }

            

            $jsonData[] = array(
                'id'            =>      ''.$Results["product_id"].'',
                'name'          =>      ''.$Results["product_name"].'',                
                'code'          =>      ''.$Results["product_code"].'',
                'costprice'     =>      ''.$Results["product_costprice"].'',
                'iva'           =>      ''.$Results["product_iva"].'',
                'iva5'          =>      ''.$Results["product_iva5"].'',
                'iac'           =>      ''.$Results["product_iac"].'',
                'branch_data'   =>      $BranchData,
                'unity_type'    =>      ''.$Results["product_unity_type"].'');             
            
        }
        
        //Response
        echo json_encode(array('success' => 1, 'count' => $Count["count"], 'product' => $jsonData));	
        exit;
    }
}

//Modify product count on branch
function Adjustproduct($JsonData, $UserID){

    global $mysql_connect_host;
    global $perm_inventorymodify;

    //Check inventory permissions
    $permission = CheckRolPermissions($_POST['username'], $perm_inventorymodify );

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No tiene los permisos para esta operación.')));
    }    

    //Create inventory Movement
    $Query  = "INSERT INTO inventory_movements
	(movement_branch, movement_type, movement_owner, movement_comment)
	VALUES ('%s', '%s', '%s', '%s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["id"],"2",$UserID,$JsonData["comment"]));

     if (!$Query) {
            //Bad mysqli response
           echo json_encode(array('success' => 0, 'error_message' => 'Inventory: Error al registrar el movimiento - '.mysqli_error($mysql_connect_host)));
           exit; }

    //Movement ID
    $movement_id = mysqli_insert_id($mysql_connect_host);

    $BranchID = "0";

    if ($JsonData["id"] == "0") {
        $BranchID = "product_branch_h";
    }else{
        $BranchID = 'branch_'.$JsonData["id"];
    }

    //Get product old count
    $Query = "SELECT %s FROM product WHERE product_id = '%s';";
    $Query = db_query_host(sprintf($Query,$BranchID,$JsonData["product_id"]));
    $Query = mysqli_fetch_array($Query);
    
    $Old_Count = $Query[sprintf("%s",$BranchID)];
    
    //Create movement Ajustment
    $Query = "INSERT INTO inventory_ajustments
	(ajustment_movement, ajustment_branch, ajustment_product_id, ajustment_product_count, ajustment_product_count_old)
    VALUES ('%s', '%s', '%s', '%s', '%s')";  
    $Query = db_query_host(sprintf($Query,$movement_id,$JsonData["id"],$JsonData["product_id"],$JsonData["count"],$Old_Count));  

    if (!$Query) {
        //Bad mysqli response
       echo json_encode(array('success' => 0, 'error_message' => 'Inventory: Error al registrar el movimiento de ajuste - '.mysqli_error($mysql_connect_host)));
       exit; }  

    $Query = "UPDATE product SET %s = '%s' WHERE product_id = '%s'";
    $Query = db_query_host(sprintf($Query,$BranchID,$JsonData["count"],$JsonData["product_id"]));

    if (!$Query) {
        //Bad mysqli response
       echo json_encode(array('success' => 0, 'error_message' => 'Inventory: Error al ingresar el ajuste - '.mysqli_error($mysql_connect_host)));
       exit; } else {
        echo json_encode(array('success' => 1));		
        exit;
    }
}

//List products with low count by brach
function ListLowProducts($JsonData){

    global $mysql_connect_host;
    global $perm_inventorylist;

    //Check inventory permissions
    $permission = CheckRolPermissions($_POST['username'], $perm_inventorylist );

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No tiene los permisos para esta operación.')));
    }      
    //Count low products
    $Count = "SELECT COUNT(product_id) AS count FROM product WHERE %s <= product_minstock + %s AND product_minstock != 0 AND product_h = 0;";
    $Count = db_query_host(sprintf($Count,"branch_".$JsonData["branch_id"],$JsonData["low_point"]));
    $Count = mysqli_fetch_array($Count);

    $From = 0;

    //Pagination 
    if (!empty($JsonData["from"])) {
        $From = $JsonData["from"];
    }

    $Query  = "SELECT %s AS branch_count, product_id, product_name, product_code, product_minstock, category_name 
                FROM product LEFT JOIN product_category ON product_category = category_id WHERE %s <= product_minstock + %s AND product_minstock != 0 AND product_h = 0 LIMIT %s,15";
    $Query  = db_query_host(sprintf($Query, "branch_".$JsonData["branch_id"], "branch_".$JsonData["branch_id"],$JsonData["low_point"], $From));
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {
            $ProductData[] = array(
                'id'                =>      ''.$Results["product_id"].'',
                'name'              =>      ''.$Results["product_name"].'',                
                'code'              =>      ''.$Results["product_code"].'',
                'count'             =>      ''.$Results["branch_count"].'',
                'category'          =>      ''.$Results["category_name"].'',
                'product_minstock'  =>      ''.$Results["product_minstock"].'');             
            
        }        
        //Response
        echo json_encode(array('success' => 1, 'count' => $Count["count"], 'product' => $ProductData));	
        exit;
    }
}

//List inventory movements
function ListMovements($JsonData){

    global $mysql_connect_host;
    global $perm_inventorylist;

    //Check inventory permissions
    $permission = CheckRolPermissions($_POST['username'], $perm_inventorylist );

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No tiene los permisos para esta operación.')));
    }   
    
    //Date validation
    if (empty($JsonData["date_from"]) || empty($JsonData["date_to"])) {
        die(json_encode(array('success' => 0, 'error_message' => 'Inventory: No tiene los permisos para esta operación.')));
    }    
    
    $From = 0;
    $Branch = "%";
    $Type = "%";

    //Pagination 
    if(!empty($JsonData["from"])) {
        $From = $JsonData["from"];
    }

    //Branch filter 
    if(!empty($JsonData["branch"])) {
        $Branch = $JsonData["branch"];
    }

    //Movement type filter 
    if(isset($JsonData["type"])) {
        $Type = $JsonData["type"];
    }

    //Filter    
    $Filter = sprintf("WHERE movement_branch LIKE '%s' AND movement_type LIKE '%s' AND movement_date > '%s' AND movement_date < '%s'",
                    $Branch, $Type, $JsonData["date_from"], $JsonData["date_to"]);  

    //Count movements
    $Count = "SELECT COUNT(movement_id) AS count FROM inventory_movements %s;";
    $Count = db_query_host(sprintf($Count,$Filter));
    $Count = mysqli_fetch_array($Count);

    //Movement query
    $Query  = "SELECT movement_id, branch_name, movement_type, user_realname, movement_date, movement_comment FROM inventory_movements 
    LEFT JOIN user ON movement_owner = user_id
    LEFT JOIN branch ON movement_branch = branch_id
    %s
    ORDER BY movement_date DESC LIMIT %s,15;";
    $Query  = db_query_host(sprintf($Query, $Filter, $From));
    $Rows   = mysqli_num_rows($Query);

    if ($Rows == 0) {
        //DB empy data
       die(json_encode(array('success' => 0, 'error_message' => 'Movimientos: No se encontraron resultados.')));       
   } else {           
       while ($Results = mysqli_fetch_array($Query)) {

        $DetailIDQuery = "";

        //Get ID of object detail
        switch ($Results["movement_type"]) {
            case '0': //Buy movement
            $DetailIDQuery = "SELECT supplier_expend_id AS detail_id FROM supplier_expend WHERE supplier_expend_movement = %s";       
                break;
            case '1': //Sell movement
            $DetailIDQuery = "SELECT ticket_id AS detail_id FROM ticket WHERE ticket_movement = %s";                    
                break;
            case '2': //Adjust movement
            $DetailIDQuery = "SELECT ajustment_id AS detail_id FROM inventory_ajustments WHERE ajustment_movement = %s"; 
                break;
            case '3': //Transfer movement
            $DetailIDQuery = "SELECT transfer_id AS detail_id FROM inventory_transfer WHERE transfer_movement = %s"; 
                break;
            default:
                break;
        }

        if (!empty($DetailIDQuery)) {
            $DetailIDQuery = db_query_host(sprintf($DetailIDQuery, $Results["movement_id"]));
            $DetailIDQuery = mysqli_fetch_array($DetailIDQuery);
            $DetailIDQuery = $DetailIDQuery["detail_id"];
        }        

        $MovementData[] = array(
            'id'                =>      ''.$Results["movement_id"].'',
            'branch'            =>      ''.$Results["branch_name"].'',                
            'type'              =>      ''.$Results["movement_type"].'',
            'user'              =>      ''.$Results["user_realname"].'',
            'date'              =>      ''.$Results["movement_date"].'',
            'comment'           =>      ''.$Results["movement_comment"].'',
            'detail_id'         =>      ''.$DetailIDQuery.'');                      
           
       }        
       //Response
       echo json_encode(array('success' => 1, 'count' => $Count["count"], 'movements' => $MovementData));	
       exit;
   }

}
?>