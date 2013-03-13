<?
/*

    Shoutcast output to XML
    Copyright (C) 2007 3Scriptz.com

    This program is free software; you can redistribute it and/or modify 
    it under the terms of the GNU General Public License as published by 
    the Free Software Foundation; either version 2 of the License, or 
    (at your option) any later version. 

    This program is distributed in the hope that it will be useful, 
    but WITHOUT ANY WARRANTY; without even the implied warranty of 
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
    GNU General Public License for more details. 

    You should have received a copy of the GNU General Public License 
    along with this program; if not, write to the Free Software 
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 


*/


error_reporting(0);
$title = 'ShoutCast Radio'; //Enter Your player title here
$url = "stream.bollywoodmusicradio.com"; // Enter your shoutcast ip here
$port = 80; //Enter your port number here
$refresh_interval = 1; //in seconds

//Do not edit below unless you know what you're doing
if ($open = @fsockopen($url,$port)) { 
	fputs($open,"GET /7.html HTTP/1.1\nUser-Agent:Mozilla\n\n"); 
	$read = fread($open,1000); 
	$row = explode("content-type:text/html",$read); 
	$row  = explode(",",$row [1]); 
}

// create doctype
$dom = new DOMDocument("1.0");
$row[6] = substr($row[6], 0, -14); //parse properly

// display document in browser as plain text
header("Content-Type: text/xml");

//  root 
$root = $dom->createElement("shoutcast");
$dom->appendChild($root);

// title
$item = $dom->createElement("title");
$root->appendChild($item);
$text = $dom->createTextNode($title);
$item->appendChild($text);

// url - http://ipaddress:port/;stream.nsv
$item = $dom->createElement("url");
$root->appendChild($item);
$text = $dom->createTextNode('http://'.$url.':'.$port);
$item->appendChild($text);

// currentListeners
$item = $dom->createElement("currentListeners");
$root->appendChild($item);
$text = $dom->createTextNode($row[4]);
$item->appendChild($text);

// maxListeners
$item = $dom->createElement("maxListeners");
$root->appendChild($item);
$text = $dom->createTextNode($row[3]);
$item->appendChild($text);

// bitrate
$item = $dom->createElement("bitrate");
$root->appendChild($item);
$text = $dom->createTextNode($row[5]);
$item->appendChild($text);

// currentSong
$item = $dom->createElement("currentSong");
$root->appendChild($item);
$text = $dom->createTextNode($row[6]);
$item->appendChild($text);

// status online/offilne
$item = $dom->createElement("status");
$root->appendChild($item);
$text = $dom->createTextNode($row[1]);
$item->appendChild($text);

// refresh_interval
$item = $dom->createElement("refreshInterval");
$root->appendChild($item);
$text = $dom->createTextNode($refresh_interval*1000);
$item->appendChild($text);

// display
echo $dom->saveXML();
?>