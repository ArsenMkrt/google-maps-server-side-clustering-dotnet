<%@ Control Language="C#" AutoEventWireup="true"  %>

 <link type="text/css" rel="stylesheet" href="Styles/Googlemap.css?<%= DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") %>" /> <%-- no cache--%>
 <link type="text/css" rel="stylesheet" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.7.2/themes/excite-bike/jquery-ui.css"  />
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js"></script>
 <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.1/jquery-ui.min.js"></script> 
 <script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>

<script type="text/javascript" src="Scripts/mapdkclustering.js?<%= DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") %> "></script> <%-- no cache--%>

<div id="gmcKN_map_container">
        
    <label>Address: </label><input id="gmcKN_search"  size="20" type="text"/> Server-side clustering with Asp.net C#, <a href="http://kunuk.wordpress.com/2011/11/05/google-map-server-side-clustering-with-asp-net/" target="_blank">description</a>

    <div id="gmcKN_map_canvas"></div>    

    <label>latitude: </label><input id="gmcKN_latitude" size="10" class="mono" maxlength="20"  type="text" /> <label>longitude: </label><input id="gmcKN_longitude" class="mono" size="10" maxlength="20"  type="text" />
     <input id="gmcKN_btnLatLonSearch" type="button" value="search" onclick="gmcKN.mymap.latlonsearch(); return false;"/>
    <span id="gmcKN_zoomInfo" class="mono">zoom: </span> Drag the search marker  &nbsp;&nbsp;&nbsp;<input id="gmcKN_lonlat" class="mono" maxlength="20"  onclick="this.select();" />

    <div id="gmcKN_checkboxContainer">        
        <input type="checkbox" id="gmcKN_Type1" name="Type" value="Type1" checked="checked" onclick="gmcKN.checkboxClicked('1',this.checked+'');" />Type1<br />
        <input type="checkbox" id="gmcKN_Type2" name="Type" value="Type2" checked="checked" onclick="gmcKN.checkboxClicked('2',this.checked+'');" />Type2<br />
        <input type="checkbox" id="gmcKN_Type3" name="Type" value="Type3" checked="checked" onclick="gmcKN.checkboxClicked('3',this.checked+'');" />Type3<br />
        <input type="checkbox" id="gmcKN_Lines" name="Type" value="Lines"                   onclick="gmcKN.checkboxClicked('gmc_meta_lines',this.checked+'');" />Toggle lines<br />
        
        <span id="gmcKN_Clustering_span">
        <input type="checkbox" id="gmcKN_Clustering" name="Type" value="Clustering" checked="checked" onclick="gmcKN.checkboxClicked('gmc_meta_clustering',this.checked+'');" />Toggle clustering<br />
        </span>
    </div>
</div>