<?php
// REQUERES
require_once("_functions/configuration.php");
require_once("_functions/functions.php");
require_once("_functions/header.php");
header('Content-type: application/json');

//Get app data status
$DataQuery  = "SELECT appdata_name, appdata_status FROM appdata";
$DataQuery  = mysqli_fetch_array(db_query_host($DataQuery));


if ($DataQuery["appdata_status"] == "1") {
    
    //GET BRANCH INFORMATION
    $Query     = "SELECT * FROM branch";
    $Query     = db_query_host($Query);
    $Rows      = mysqli_num_rows($Query); 

    $BoxQuery  = "SELECT * FROM branch_boxes";
    $BoxQuery  = db_query_host($BoxQuery);
    $BoxRows   = mysqli_num_rows($BoxQuery); 

    if ($Rows != 0){
        while ($Branch = mysqli_fetch_array($Query) ) {
            $JsonData[] = array(
                            'BranchID'      =>''.$Branch["branch_id"].'',
                            'BranchName'    =>''.$Branch["branch_name"].'');
        }

        if ($BoxRows != 0) {
            while ($Box = mysqli_fetch_array($BoxQuery) ) {
                $BoxData[] = array(
                                'BoxID'      =>''.$Box["box_id"].'',
                                'BoxName'    =>''.$Box["box_name"].'',
                                'BoxBranch'  =>''.$Box["box_branch"].'');
            }
        }        

        echo json_encode(array('success' => 1, 'name' => $DataQuery["appdata_name"], 'branch' => $JsonData, 'boxes' => $BoxData));

    } else {
        //Null branches
        die(json_encode(array('success' => 0, 'error_message' => 'Sin datos.')));
    }
} else {
    //ON Account disabled
    die(json_encode(array('success' => 0, 'error_message' => 'Cuenta deshabilitada.')));
}
?>