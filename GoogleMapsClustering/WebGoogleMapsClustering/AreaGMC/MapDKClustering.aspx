﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MapDKClustering.aspx.cs" Inherits="Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.MapDKClustering" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Google Maps Server-side Clustering</title>    
    <link rel="SHORTCUT ICON" href="~/favicon.ico" type="image/x-icon" />
</head>

<body>
    <form id="form1" runat="server" onsubmit="return false">
    <div>
    
        <gmc:MapDKClustering id="map1" runat="server" />

    </div>
    </form>
</body>
</html>