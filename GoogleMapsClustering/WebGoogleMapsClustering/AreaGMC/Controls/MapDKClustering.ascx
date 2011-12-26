<%@ Control Language="C#" AutoEventWireup="true"  %>

 <link type="text/css" href="Styles/Googlemap.css?<%= DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") %>" rel="stylesheet" /> 
 <link rel="stylesheet" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/themes/excite-bike/jquery-ui.css" type="text/css" />
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.6.4/jquery.min.js"></script>
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.1/jquery-ui.min.js"></script> 
 <script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>

<script type="text/javascript" src="Scripts/mapdkclustering.js?<%= DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") %> "></script> <%-- no cache--%>

<div id="map_container">
        
    <label>Address: </label><input id="search"  size="20" type="text"/> Server-side clustering with Asp.net C#, <a href="http://kunuk.wordpress.com/2011/11/05/google-map-server-side-clustering-with-asp-net/" target="_blank">description</a>

    <div id="map_canvas"></div>    

    <label>latitude: </label><input id="latitude" size="10" class="mono" maxlength="20"  type="text" /> <label>longitude: </label><input id="longitude" class="mono" size="10" maxlength="20"  type="text" />
     <input id="btnLatLonSearch" type="button" value="search" onclick="mymap.latlonsearch(); return false;"/>
    <span id="zoomInfo" class="mono">zoom: </span> Drag the search marker  &nbsp;&nbsp;&nbsp;<input id="lonlat" class="mono" maxlength="20"  onclick="document.getElementById('lonlat').select();" />

    <div id="checkboxContainer">        
        <input type="checkbox" id="Type1" name="Type" value="Type1" checked="checked" onclick="checkboxClicked('1',this.checked+'');" />Type1<br />
        <input type="checkbox" id="Type2" name="Type" value="Type2" checked="checked" onclick="checkboxClicked('2',this.checked+'');" />Type2<br />
        <input type="checkbox" id="Type3" name="Type" value="Type3" checked="checked" onclick="checkboxClicked('3',this.checked+'');" />Type3<br />
    </div>
</div>