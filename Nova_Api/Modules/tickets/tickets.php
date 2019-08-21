<?php
// REQUERES
require_once("../../_functions/configuration.php");
require_once("../../_functions/functions.php");
require_once("../../_functions/header.php");
require_once("../../_functions/permissions.php");
header('Content-type: application/json');


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Check user rol permissions
$permission = CheckRolPermissions($_POST['username'], $perm_post);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        NewTicket(CheckData($_POST['data']), GetUsernameID($_POST['username']));    
    case 2:        
        ListTickets(CheckData($_POST['data'])); 
    case 3:        
        CancelTicket(CheckData($_POST['data']), GetUsernameID($_POST['username']));      
}

//Create new ticket (movement, ticket, ticket_items)
function NewTicket($JsonData,$UserID){

    global $mysql_connect_host;
    global $permission;

    if ($permission["create"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }    

    //Create inventory Movement
    $Query  = "INSERT INTO inventory_movements
	(movement_branch, movement_type, movement_owner, movement_comment)
	VALUES ('%s', '%s', '%s', '%s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["branch_id"],"1",$UserID,$JsonData["ticket_comment"]));

     if (!$Query) {
            //Bad mysqli response
           echo json_encode(array('success' => 0, 'error_message' => ' Error al registrar el movimiento - '.mysqli_error($mysql_connect_host)));
           exit; }

    //Movement ID
    $movement_id = mysqli_insert_id($mysql_connect_host);    

    //Create ticket

    $expiration_date = "";

    if (empty($JsonData["expiration_date"])) {
        $expiration_date = "null";
    } else{
        $expiration_date = "'".$JsonData["expiration_date"]."'";
    }

    $Table = "ticket";

    if ($JsonData["ticket_h"] == "1") {

        $Table = "ticket_h";
    }

    //Ticket query
    $TicketQuery = "INSERT INTO %s
	(ticket_expiration_date, ticket_client, ticket_box, ticket_box_movement, ticket_branch, ticket_payment_type, ticket_payment_method, 
	ticket_user, ticket_movement, ticket_iva, ticket_iac, ticket_total, ticket_totalpayment, ticket_changepayment, ticket_leftpayment, ticket_h)
    VALUES (%s, '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s')";
    
    $TicketQuery = db_query_host(sprintf($TicketQuery, $Table, $expiration_date, $JsonData["client_id"], $JsonData["box_id"], $JsonData["box_movement_id"], 
    $JsonData["branch_id"], $JsonData["payment_type"], $JsonData["payment_method"], $UserID, $movement_id, $JsonData["ticket_iva"], 
    $JsonData["ticket_iac"], $JsonData["ticket_total"], $JsonData["ticket_totalpayment"], $JsonData["ticket_changepayment"], $JsonData["ticket_leftpayment"],
    $JsonData["ticket_h"]));

    //Bad ticket query
    if (!$TicketQuery) {
        //Delete movement
        echo json_encode(array('success' => 0, 'error_message' => 'error al registrar la factura - '.mysqli_error($mysql_connect_host)));

        $DeleteQuery = "DELETE FROM inventory_movements WHERE movement_id = %s";
        $DeleteQuery = db_query_host(sprintf($DeleteQuery, $movement_id));
       
       exit; }


    //Movement ID
    $ticket_id = mysqli_insert_id($mysql_connect_host);  
    
    //Ticket items query and inventory adjust
    foreach ($JsonData["products"] as $key) {
        
        //Ticket item 
        $TicketItemQuery = "INSERT INTO ticket_item
        (item_ticket, item_product, item_count, item_tax, item_pricevalue, item_discountvalue, item_h)
        VALUES ('%s', '%s', '%s', '%s', '%s', '%s', '%s')";
        $TicketItemQuery = db_query_host(sprintf($TicketItemQuery, $ticket_id, $key["product_id"], $key["product_count"], $key["product_tax"],
        $key["product_total"],$key["product_discountvalue"],$key["product_h"]));

        if (!$TicketItemQuery) {           

            //Bad mysqli response
           echo json_encode(array('success' => 0, 'error_message' => ' Error al ingresar los items de la factura - '.mysqli_error($mysql_connect_host)));
           exit; }

        //Product inventory ajust
        $Branch = "branch_".$JsonData["branch_id"];

        if ($JsonData["ticket_h"] == "1") {

            $Branch = "product_branch_h";
        }

        $InventoryQuery = "UPDATE product
        SET
            %s = %s - %s
        WHERE product_id = '%s'";
        $InventoryQuery = db_query_host(sprintf($InventoryQuery, $Branch , $Branch , $key["product_count"], $key["product_id"]));

        if (!$TicketItemQuery) {  
            //Bad mysqli response
           echo json_encode(array('success' => 0, 'error_message' => ' Error al registrar los cambios en el inventario - '.mysqli_error($mysql_connect_host)));
           exit; 
        }
    }

    //Response
    echo json_encode(array('success' => 1, 'ticketid' => $ticket_id));	
    exit;  
}

function ListTickets($JsonData){

    global $mysql_connect_host;
    global $permission;

    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }
    
    //Search Filter vars
    $TicketTable = "ticket";

    $TicketBranch = "%%";
    $TicketBox = "%%";

    $TicketClient = "%%";

    $TicketFrom = "";
    $TicketTo = "";

    //Set Seatch filters
    if (!empty($JsonData["h"])) {
        $TicketTable = "ticket_h";
    }

    if (!empty($JsonData["ticket_branch"])) {
        $TicketBranch = $JsonData["ticket_branch"];
    }

    // if (!empty($JsonData["ticket_box"])) {
    //     $TicketBox = $JsonData["ticket_box"];
    // }

    if (!empty($JsonData["client"])) {
        $TicketClient = $JsonData["client"];
    }
    //DATE
    if (!empty($JsonData["from_date"])) {
        $TicketFrom = $JsonData["from_date"];
    }

    if (!empty($JsonData["to_date"])) {
        $TicketTo = $JsonData["to_date"];
    }

    $From = 0;
    $Filter = "%%";

    //Pagination 
    if (!empty($JsonData["from"])) {
        $From = $JsonData["from"];
    }

    //Pagination 
    if (!empty($JsonData["filter"])) {
        $Filter = $JsonData["filter"];
        //on filter all boxes
        $TicketBox = "%%";
        $TicketBranch = "%%";
        $TicketClient = "%%";
        $TicketFrom = "";
        $TicketTo = "";
    }

    $DateFilter = "";

    if (!empty($TicketFrom) && !empty($TicketTo)) {
        $DateFilter = sprintf("AND (ticket_date > '%s' AND ticket_date < '%s')",$TicketFrom,$TicketTo);
    }

    
    $Count = "SELECT COUNT(ticket_id) as count FROM %s WHERE ticket_branch LIKE '%s' AND ticket_box LIKE '%s' AND ticket_client LIKE '%s' %s ;";
    $Count = db_query_host(sprintf($Count,$TicketTable,$TicketBranch,$TicketBox,$TicketClient, $DateFilter));
    $Count = mysqli_fetch_array($Count);

    $Query  = "SELECT ticket_id, ticket_date, ticket_expiration_date, ticket_client, client_name, ticket_box, ticket_box_movement, ticket_branch, ticket_payment_type, ticket_payment_method, ticket_user, user_realname,
    ticket_movement, ticket_iva, ticket_iva5, ticket_iac, ticket_total, ticket_totalpayment, ticket_changepayment, ticket_leftpayment, ticket_status
    FROM %s
    LEFT JOIN user ON user_id = ticket_user
    LEFT JOIN clients ON client_id = ticket_client
    WHERE ticket_id LIKE '%s' AND ticket_branch LIKE '%s' AND ticket_client LIKE '%s' %s ORDER BY ticket_id DESC LIMIT %s,15";
    $Query  = db_query_host(sprintf($Query, $TicketTable, $Filter, $TicketBranch, $TicketClient, $DateFilter, $From));
    $Rows   = mysqli_num_rows($Query);     

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 1, 'error_message' => 'No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {

            $PH = "0";
            if (!empty($JsonData["h"])) {
                $PH = "1";
            }

            $PQuery = "SELECT item_id, item_ticket, item_product, product_name, product_code, product_unity_type, item_count, item_tax, item_pricevalue, item_discountpercent, item_discountvalue
            FROM ticket_item
            LEFT JOIN product ON product_id = item_product
            WHERE item_ticket = %s AND item_h = '%s'";

            $PQuery = db_query_host(sprintf($PQuery, $Results["ticket_id"], $PH));

            if (!$PQuery) {
                # code...
                die(json_encode(array('success' => 0, 'error_message' => 'Error al listar los productos de la factura - '.mysqli_error($mysql_connect_host))));
            }

            $PRows   = mysqli_num_rows($PQuery);

            while ($ItemsData = mysqli_fetch_array($PQuery)) {             
                $ItemsResult[] = array(
                    'item_id'               =>      ''.$ItemsData["item_id"].'',
                    'item_ticket'           =>      ''.$ItemsData["item_ticket"].'',
                    'item_product'          =>      ''.$ItemsData["item_product"].'',
                    'product_name'          =>      ''.$ItemsData["product_name"].'',
                    'product_code'          =>      ''.$ItemsData["product_code"].'',
                    'product_unity'         =>      ''.$ItemsData["product_unity_type"].'',   
                    'item_count'            =>      ''.$ItemsData["item_count"].'', 
                    'item_tax'              =>      ''.$ItemsData["item_tax"].'', 
                    'item_pricevalue'       =>      ''.$ItemsData["item_pricevalue"].'', 
                    'item_discountpercent'  =>      ''.$ItemsData["item_discountpercent"].'',    
                    'item_discountvalue'    =>      ''.$ItemsData["item_discountvalue"].'');  
            }
           
            if ($PRows == 0) {
                $ItemsResult = null;
            }

            $jsonData[] = array(
                'id'                       =>      ''.$Results["ticket_id"].'',
                'date'                     =>      ''.$Results["ticket_date"].'',
                'expiration_date'          =>      ''.$Results["ticket_expiration_date"].'',
                'client_id'                =>      ''.$Results["ticket_client"].'',
                'client_name'              =>      ''.$Results["client_name"].'',
                'box_id'                   =>      ''.$Results["ticket_box"].'',
                'box_movement_id'          =>      ''.$Results["ticket_box_movement"].'',
                'branch_id'                =>      ''.$Results["ticket_branch"].'',
                'payment_type'             =>      ''.$Results["ticket_payment_type"].'',
                'payment_method'           =>      ''.$Results["ticket_payment_method"].'',
                'user_id'                  =>      ''.$Results["ticket_user"].'',
                'user_realname'            =>      ''.$Results["user_realname"].'',
                'movement'                 =>      ''.$Results["ticket_movement"].'',
                'ticket_iva'               =>      ''.$Results["ticket_iva"].'',
                'ticket_iva5'              =>      ''.$Results["ticket_iva5"].'',
                'ticket_iac'               =>      ''.$Results["ticket_iac"].'',
                'ticket_total'             =>      ''.$Results["ticket_total"].'',
                'ticket_totalpayment'      =>      ''.$Results["ticket_totalpayment"].'',
                'ticket_changepayment'     =>      ''.$Results["ticket_changepayment"].'',
                'ticket_leftpayment'       =>      ''.$Results["ticket_leftpayment"].'',
                'ticket_status'            =>      ''.$Results["ticket_status"].'',
                'items_count'              =>      $PRows,
                'items'                    =>      $ItemsResult); 
                
                unset($ItemsResult);
        }
        
        //Response
        echo json_encode(array('success' => 1, 'count' => $Count["count"], 'tickets' => $jsonData));	
        exit;
    }
}

function CancelTicket($JsonData,$UserID){

    global $mysql_connect_host;
    global $permission;

    if ($permission["delete"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }    

    //Create inventory Movement
    $Query  = "INSERT INTO inventory_movements
	(movement_branch, movement_type, movement_owner, movement_comment)
	VALUES ('%s', '%s', '%s', 'Factura: %s');";
    $Query  = db_query_host(sprintf($Query,$JsonData["branch_id"],"4",$UserID, $JsonData["ticket_id"]));

     if (!$Query) {
            //Bad mysqli response
           echo json_encode(array('success' => 0, 'error_message' => ' Error al registrar el movimiento - '.mysqli_error($mysql_connect_host)));
           exit; }

    //Movement ID
    $movement_id = mysqli_insert_id($mysql_connect_host);    

    //Update ticket
    $Table = "ticket";

    if ($JsonData["ticket_h"] == "1") {

        $Table = "ticket_h";
    }

    //Ticket query
    $TicketQuery = "UPDATE %s SET ticket_status = '%s' WHERE ticket_id = '%s';";
    
    $TicketQuery = db_query_host(sprintf($TicketQuery, $Table, $JsonData["status"], $JsonData["ticket_id"]));

    //Bad ticket query
    if (!$TicketQuery) {
        //Delete movement
        echo json_encode(array('success' => 0, 'error_message' => 'error al cancelar la factura - '.mysqli_error($mysql_connect_host)));

        $DeleteQuery = "DELETE FROM inventory_movements WHERE movement_id = %s";
        $DeleteQuery = db_query_host(sprintf($DeleteQuery, $movement_id));
       
        exit; }


    $ProductQuery = "SELECT item_product, item_count FROM ticket_item WHERE item_ticket = '%s'";
    $ProductQuery = db_query_host(sprintf($ProductQuery, $JsonData["ticket_id"]));

    if (!$ProductQuery) {
        echo json_encode(array('success' => 0, 'error_message' => 'error al verificar los items de la factura - '.mysqli_error($mysql_connect_host)));
        exit;
    }

    while ($Results = mysqli_fetch_array($ProductQuery)) {

         //Product inventory ajust
         $Branch = "branch_".$JsonData["branch_id"];

         if ($JsonData["ticket_h"] == "1") { 
             $Branch = "product_branch_h";
         }
 
         $InventoryQuery = "UPDATE product
         SET
         %s = %s + %s
         WHERE product_id = '%s'";
         $InventoryQuery = db_query_host(sprintf($InventoryQuery, $Branch , $Branch , $Results["item_count"], $Results["item_product"]));
    }
    
    //Response
    echo json_encode(array('success' => 1));	
    exit;  
}

function TicketPayment($JsonData,$UserID){

}

?>