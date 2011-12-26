<%@ Control Language="C#" AutoEventWireup="true"  %>

 <link type="text/css" href="Styles/Googlemap.css" rel="stylesheet" /> 
 <link rel="stylesheet" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/themes/excite-bike/jquery-ui.css" type="text/css" />
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.6.4/jquery.min.js"></script>
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.1/jquery-ui.min.js"></script> 
 <script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>

<script type="text/javascript" src="Scripts/mapdk.js"></script> <%-- no cache--%>

<div id="map_container">
        
    <label>Address: </label><input id="search"  size="40" type="text"/>      
    Drag the search marker.

    <div id="map_canvas"></div>    

    <label>latitude: </label><input id="latitude" class="mono" type="text" size="10" maxlength="20" /> <label>longitude: </label><input id="longitude" class="mono" size="10" maxlength="20" type="text" />
    <input id="btnLatLonSearch" type="button" value="search" onclick="mymap.latlonsearch(); return false;"/>
    <span id="zoomInfo" class="mono">zoom: </span> &nbsp;&nbsp;&nbsp;<input id="lonlat" class="mono" maxlength="20" onclick="document.getElementById('lonlat').select();" />

</div>