<?php
	/*
		DB FUNCTIONS - HOST ADMIN
	*/
		$mysql_connect_host = @mysqli_connect($host_ip, $host_username, $host_password) or die(json_encode(array('success' => 0,   'error_message' => 'API - Host error' ))); 
		db_select_host($host_database);
?>