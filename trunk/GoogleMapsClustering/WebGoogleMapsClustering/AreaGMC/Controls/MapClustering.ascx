<%@ Control Language="C#" AutoEventWireup="true"  %>

 <link type="text/css" href="Styles/Googlemap.css" rel="stylesheet" /> 
 
 <script type="text/javascript" src="Scripts/jquery.1.6.4.min.js"></script> 
 <script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>

<script type="text/javascript" src="Scripts/mapclustering.js?<%= DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") %> "></script> <%-- no cache--%>

<div id="map_container">            
    <div id="map_canvas"></div>
</div>