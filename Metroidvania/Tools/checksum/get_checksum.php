<?php  

	$filePath = $argv[1];
	echo md5_file($filePath);
	echo "\n";
	
?> 