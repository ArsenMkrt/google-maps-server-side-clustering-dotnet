<%@ Control Language="C#" AutoEventWireup="true"  %>

 <link type="text/css" href="Styles/Googlemap.css" rel="stylesheet" /> 
 <link rel="stylesheet" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/themes/excite-bike/jquery-ui.css" type="text/css" />
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.6.4/jquery.min.js"></script>
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.1/jquery-ui.min.js"></script> 
 <script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>

<script type="text/javascript" src="Scripts/mapdkclustering.js?<%= DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") %> "></script> <%-- no cache--%>

<div id="map_container">
        
    <label>Address: </label><input id="search"  size="20" type="text"/> Server-side clustering with Asp.net C#, <a href="http://kunuk.wordpress.com/2011/11/05/google-map-server-side-clustering-with-asp-net/" target="_blank">description</a>

    <div id="map_canvas"></div>    

    <label>latitude: </label><input id="latitude" size="8" maxlength="20"  type="text" /> <label>longitude: </label><input id="longitude" size="8" maxlength="20"  type="text" />
     <input id="btnLatLonSearch" type="button" value="search" onclick="mymap.latlonsearch(); return false;"/>
    <span id="zoomInfo">zoom: </span> Drag the search marker  &nbsp;&nbsp;&nbsp;<input id="lonlat" maxlength="20"  onclick="document.getElementById('lonlat').select();" />

</div>