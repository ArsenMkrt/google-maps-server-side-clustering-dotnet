<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Kunukn.GooglemapsClustering.WebGoogleMapClustering._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Google Maps Clustering Demo</title>
    <link rel="SHORTCUT ICON" href="favicon.ico" type="image/x-icon" />
    <link type="text/css" href="AreaGMC/Styles/Default.css" rel="stylesheet" /> 
</head>
<body>
        
    <form id="form1" runat="server" onsubmit="return false">    
    <div>    
    
    <h1>Google Maps Clustering Demo</h1> 
    <img src="AreaGMC/Images/clusteringpreview.gif" alt="preview"/>   

    <a href="AreaGMC/Documents/Readme.htm" target="_blank">Readme</a><br />

    <h3>Clustering</h3>
    <ul id="clustering">
        <li><a href="AreaGMC/MapClustering.aspx">Google Maps Server-side Clustering</a></li>
        <li><a href="AreaGMC/MapDKClustering.aspx">Google Maps Server-side Clustering Denmark with Search</a></li>               
    </ul>    
    
    <br /><br />

    <h3>Search</h3>
     <ul id="search">
        <li><a href="AreaGMC/MapWorld.aspx">Google Maps Search</a></li>
        <li><a href="AreaGMC/MapDK.aspx">Google Maps Search Denmark</a></li>               
    </ul>  
                
    </div>    
    </form>
</body>
</html>
