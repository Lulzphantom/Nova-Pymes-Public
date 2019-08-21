<?php
// REQUERES
require_once("../../_functions/configuration.php");
require_once("../../_functions/functions.php");
require_once("../../_functions/header.php");
require_once("../../_functions/permissions.php");
header('Content-type: application/json');


if ( empty($_POST['username']) || empty($_POST['token']) || empty($_POST['function']) ) {
    die(json_encode(array(
      'success' => 0, 'error_message' => 'Product: Bad Request'      
    )));
}

//TOKEN CHECK
checkSecurityVariables($_POST['username'], $_POST['token']);

//Create permissions object
$permission = CheckRolPermissions($_POST['username'], $perm_reports);

//CHECK FUNCTION
switch ($_POST['function']) {
    case 1:        
        BoxMovementsReport(CheckData($_POST['data']));
    case 2:
        DailySellsReport(CheckData($_POST['data']));  
}

function BoxMovementsReport($JsonData){

    global $mysql_connect_host;
    global $permission;

    //Check user permission
    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Declare filter values
    $FilterBranch   = $JsonData["branch_id"];
    $FilterBox      = "";
    $FilterFrom     = "%%";
    $FilterTo       = "%%";

    $Pagination     = 0;

    //Set filter values
    if (!empty($JsonData["box_id"])) { //Box data id

        $FilterBox = $JsonData["box_id"];

    }else{
        
        $BoxesQuery = "SELECT box_id FROM branch_boxes WHERE box_branch = %s;";
        $BoxesQuery = db_query_host(sprintf($BoxesQuery,$FilterBranch));
        while ($BoxResult = mysqli_fetch_array($BoxesQuery)) {

            $FilterBox = $FilterBox.$BoxResult["box_id"]."|";
        }         	
        $FilterBox = substr_replace($FilterBox ,"", -1);
    }    

    if (!empty($JsonData["date_from"])) { //Date from
        $FilterFrom = $JsonData["date_from"];
    }

    if (!empty($JsonData["date_to"])) { //Date to
        $FilterTo = $JsonData["date_to"];
    }

    if (!empty($JsonData["from"])) { //Pagination
        $Pagination = $JsonData["from"];
    }

    //Count items
    $Count = "SELECT COUNT(box_movement_id) as count FROM branch_boxes_movements WHERE box_movement_box REGEXP '%s' AND (box_movement_opendate > '%s' AND box_movement_closedate < '%s');";
    $Count = db_query_host(sprintf($Count,$FilterBox,$FilterFrom,$FilterTo));
    $Count = mysqli_fetch_array($Count);    

    //Query items
    $Query  = "SELECT box_movement_id, box_name, box_movement_opendate, box_movement_closedate, useropen.user_realname AS openuser, userclose.user_realname AS closeuser,
    box_movement_openvalue, box_movement_closevalue, box_movement_comment
    FROM branch_boxes_movements
    LEFT JOIN branch_boxes ON box_id = box_movement_box
    LEFT JOIN user AS useropen ON useropen.user_id = box_movement_openuser
    LEFT JOIN user AS userclose ON userclose.user_id = box_movement_closeuser
    WHERE box_movement_box REGEXP '%s' AND (box_movement_opendate > '%s' AND box_movement_closedate < '%s') ORDER BY box_movement_id DESC LIMIT %s,15";
    $Query  = db_query_host(sprintf($Query, $FilterBox, $FilterFrom, $FilterTo, $Pagination));
    $Rows   = mysqli_num_rows($Query);

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {
            $jsonData[] = array(
                'id'           =>      ''.$Results["box_movement_id"].'',
                'name'         =>      ''.$Results["box_name"].'',
                'opendate'     =>      ''.$Results["box_movement_opendate"].'',
                'openuser'     =>      ''.$Results["openuser"].'',
                'closedate'    =>      ''.$Results["box_movement_closedate"].'',
                'closeuser'    =>      ''.$Results["closeuser"].'',
                'openvalue'    =>      ''.$Results["box_movement_openvalue"].'',
                'closevalue'   =>      ''.$Results["box_movement_closevalue"].'',
                'comment'      =>      ''.$Results["box_movement_comment"].'');   
        }        
        //Response
        echo json_encode(array('success' => 1, 'count' => $Count["count"], 'box_items' => $jsonData));	
        exit;
    }
}

function DailySellsReport($JsonData){
    global $mysql_connect_host;
    global $permission;

    //Check user permission
    if ($permission["consult"] != 1) {
        die(json_encode(array('success' => 0, 'error_message' => 'No tiene los permisos para esta operación.')));
    }

    //Declare filter values
    $FilterBranch   = $JsonData["branch_id"];
    $FilterBox      = "";
    $FilterFrom     = "%%";
    $FilterTo       = "%%";

    $Pagination     = 0;

    //Set filter values
    if (!empty($JsonData["box_id"])) { //Box data id

        $FilterBox = $JsonData["box_id"];

    }else{
        
        $BoxesQuery = "SELECT box_id FROM branch_boxes WHERE box_branch = %s;";
        $BoxesQuery = db_query_host(sprintf($BoxesQuery,$FilterBranch));
        while ($BoxResult = mysqli_fetch_array($BoxesQuery)) {

            $FilterBox = $FilterBox.$BoxResult["box_id"]."|";
        }         	
        $FilterBox = substr_replace($FilterBox ,"", -1);
    }    

    if (!empty($JsonData["date_from"])) { //Date from
        $FilterFrom = $JsonData["date_from"];
    }

    if (!empty($JsonData["date_to"])) { //Date to
        $FilterTo = $JsonData["date_to"];
    }

    if (!empty($JsonData["from"])) { //Pagination
        $Pagination = $JsonData["from"];
    }

    //VALUES total
    $ValuesQuery = "SELECT SUM(ticket_total) AS total, SUM(ticket_leftpayment) AS credits 
    FROM ticket
    WHERE ticket_box REGEXP '%s' AND ticket_h = 0 AND (ticket_date > '%s' AND ticket_date < '%s');";
    $ValuesQuery = db_query_host(sprintf($ValuesQuery,$FilterBox,$FilterFrom,$FilterTo));

    if (!$ValuesQuery) {
        die(json_encode(array('success' => 0, 'error_message' => 'Error: '.mysqli_error($mysql_connect_host))));
    }

    $ValuesQuery = mysqli_fetch_array($ValuesQuery);    

    //Count items
    $Count = "SELECT COUNT(item_id) AS count
    FROM ticket_item
    LEFT JOIN ticket ON ticket_id = item_ticket
    WHERE ticket_box REGEXP '%s' AND item_h = 0 AND (ticket_date > '%s' AND ticket_date < '%s');";
    $Count = db_query_host(sprintf($Count,$FilterBox,$FilterFrom,$FilterTo));
    $Count = mysqli_fetch_array($Count);    

    //Query items
    $Query  = "SELECT ticket_date, item_ticket, product_name, item_count, product_unity_type, item_pricevalue, if(ticket_leftpayment = 0,'CONTADO','CREDITO') AS payment
    FROM ticket_item
    LEFT JOIN ticket ON ticket_id = item_ticket
    LEFT JOIN product ON product_id = item_product
    WHERE ticket_box REGEXP '%s' AND item_h = 0 AND (ticket_date > '%s' AND ticket_date < '%s') ORDER BY ticket_id DESC LIMIT %s,15;";
    $Query  = db_query_host(sprintf($Query, $FilterBox, $FilterFrom, $FilterTo, $Pagination));

    if (!$Query) {
        die(json_encode(array('success' => 0, 'error_message' => 'Error: '.mysqli_error($mysql_connect_host))));
    }

    $Rows   = mysqli_num_rows($Query);

    if ($Rows == 0) {
         //DB ERROR
        die(json_encode(array('success' => 0, 'error_message' => 'No se encontraron resultados.')));
        
    } else {           
        while ($Results = mysqli_fetch_array($Query)) {
            $jsonData[] = array(
                'date'          =>      ''.$Results["ticket_date"].'',
                'ticket_id'     =>      ''.$Results["item_ticket"].'',
                'product_name'  =>      ''.$Results["product_name"].'',
                'product_count' =>      ''.$Results["item_count"].'',
                'product_type'  =>      ''.$Results["product_unity_type"].'',
                'product_price' =>      ''.$Results["item_pricevalue"].'',
                'payment'       =>      ''.$Results["payment"].'');   
        }        
        //Response
        echo json_encode(array('success' => 1, 'count' => $Count["count"], 'total' => $ValuesQuery["total"], 'credits' => $ValuesQuery["credits"], 'sell_items' => $jsonData));	
        exit;
    }    
}