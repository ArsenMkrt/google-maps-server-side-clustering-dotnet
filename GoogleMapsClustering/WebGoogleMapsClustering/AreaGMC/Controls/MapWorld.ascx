<%@ Control Language="C#" AutoEventWireup="true"  %>

 <link type="text/css" href="Styles/Googlemap.css" rel="stylesheet" /> 
 <link rel="stylesheet" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/themes/excite-bike/jquery-ui.css" type="text/css" />
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js"></script>
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.1/jquery-ui.min.js"></script> 
 <script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>

<script type="text/javascript" src="Scripts/mapworld.js?<%= DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") %> "></script> <%-- no cache--%>

<div id="gmcKN_map_container">
        
    <label>Address: </label><input id="gmcKN_search"  size="40" type="text"/>      
    Drag the search marker.

    <div id="gmcKN_map_canvas"></div>    

    <label>latitude: </label><input id="gmcKN_latitude" class="mono" type="text" size="10" maxlength="20" /> <label>longitude: </label><input id="gmcKN_longitude" size="10" class="mono" maxlength="20"  type="text" />
    <input id="gmcKN_btnLatLonSearch" type="button" value="search" onclick="mymap.latlonsearch(); return false;"/>
    <span id="gmcKN_zoomInfo" class="mono">zoom: </span> &nbsp;&nbsp;&nbsp;<input id="gmcKN_lonlat" class="mono" maxlength="20" onclick="this.select();" />

</div>