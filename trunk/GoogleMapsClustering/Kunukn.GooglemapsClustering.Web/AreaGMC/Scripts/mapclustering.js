﻿// Author: Kunuk Nykjaer et al.
// jQuery and google library

var gmcKN = {

    markers: [], // markers on screen
    knnmarkers: [], // K-nearest markers
    map: undefined,
    infowindow: undefined,
    debugMarker: undefined,
    debuginfo: undefined,

    // http://code.google.com/intl/da-DK/apis/maps/documentation/javascript/reference.html
    geocoder: new google.maps.Geocoder(),
    knn: false, // K-nearest neighbor display flag
    knn_K: 5, // number of K-nearest neighbor
    debug: {
        showGridLines: false,
        showBoundaryMarker: false        
    },
    // prevent async send/receive order problem by using counter ref in send/reply in webservice
    async: {
        lastSendGetMarkers: 0, //get markers
        lastReceivedGetMarkers: 0,
        lastSendMarkerDetail: 0,
        lastReceivedMarkerDetail: 0,
        lastCache: ""
    },

    log: function (s) {
        if (console.log) {
            console.log(s);
        }
    },

    round: function (num, decimals) {
        return Math.round(num * Math.pow(10, decimals)) / Math.pow(10, decimals);
    },

    zoomIn: function () {
        var z = gmcKN.map.getZoom();
        gmcKN.map.setZoom(z + 1);
    },

    zoomOut: function () {
        var z = gmcKN.map.getZoom();
        gmcKN.map.setZoom(z - 1);
    },

    mymap: {
        initialize: function () {

            var center = new google.maps.LatLng(gmcKN.mymap.settings.mapCenterLat, gmcKN.mymap.settings.mapCenterLon, true);

            gmcKN.map = new google.maps.Map(document.getElementById('gmcKN_map'), {
                zoom: gmcKN.mymap.settings.zoomLevel,
                center: center,
                scrollwheel: true,
                navigationControl: true,
                mapTypeControl: true,
                draggable: true,
                scaleControl: true,
                streetViewControl: false,
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                backgroundColor: '#fff',
                draggableCursor: 'move',
                minZoom: 1,
                maxZoom: 19
            });

            google.maps.event.addListener(gmcKN.map, 'idle', function () { gmcKN.mymap.events.getBounds(false); });
            google.maps.event.addListener(gmcKN.map, 'zoom_changed', function () {
                document.getElementById("gmcKN_zoomInfo").innerHTML = "&nbsp;Zoom: " + gmcKN.map.getZoom() + ".  ";
                if (gmcKN.map.getZoom() < gmcKN.mymap.settings.alwaysClusteringEnabledWhenZoomLevelLess) {
                    document.getElementById('gmcKN_Clustering_span').style.display = "none";
                }
                else {
                    document.getElementById('gmcKN_Clustering_span').style.display = "block";
                }
            });
            google.maps.event.trigger(gmcKN.map, 'zoom_changed'); //trigger first time event
        },
        settings: {
            mapCenterLat: 35,
            mapCenterLon: 10,
            zoomLevel: 2,
            alwaysClusteringEnabledWhenZoomLevelLess: 2,

            jsonGetMarkerUrl: '/AreaGMC/gmc.svc/GetMarkers', // get
            jsonMarkerUrl: '/AreaGMC/gmc.svc/Markers', // post  //not used by default

            jsonGetMarkerInfoUrl: '/AreaGMC/gmc.svc/GetMarkerInfo', // get
            jsonMarkerInfoUrl: '/AreaGMC/gmc.svc/MarkerInfo', // post //not used by default

            jsonKnnUrl: '/AreaGMC/gmc.svc/Knn', // get // K-nearest neighbors

            clusterImage: {
                src: 'Images/cluster2.png', //this is invisible img only used for click-event detecting
                height: 60,
                width: 60,
                offsetH: 30,
                offsetW: 30
            },
            pinImage: {
                src: 'Images/pin24.png', //default unknown marker
                height: 24,
                width: 24,
                offsetH: 0,
                offsetW: 0
            },

            // specific markers
            pinImage1: {
                src: 'Images/markers/court.png',
                height: 37,
                width: 32,
                offsetH: 0,
                offsetW: 0
            },
            pinImage2: {
                src: 'Images/markers/firstaid.png',
                height: 37,
                width: 32,
                offsetH: 0,
                offsetW: 0
            },
            pinImage3: {
                src: 'Images/markers/house.png',
                height: 37,
                width: 32,
                offsetH: 0,
                offsetW: 0
            },
            textErrorMessage: 'An error has occured'
        },

        events: {
            getBounds: function (forceUpdate) {

                if (gmcKN.infowindow === undefined) {
                    gmcKN.infowindow = new google.maps.InfoWindow();
                }

                var bounds = gmcKN.map.getBounds();
                var NE = bounds.getNorthEast();
                var SW = bounds.getSouthWest();
                var mapData = [];
                mapData.neLat = gmcKN.round(NE.lat(), 7);
                mapData.neLon = gmcKN.round(NE.lng(), 7);
                mapData.swLat = gmcKN.round(SW.lat(), 7);
                mapData.swLon = gmcKN.round(SW.lng(), 7);
                mapData.zoomLevel = gmcKN.map.getZoom();

                //------------- DEBUG
                if (gmcKN.debug.showBoundaryMarker) {
                    var center = gmcKN.map.getCenter();
                    if (gmcKN.debugMarker === undefined) { // singleton-ish
                        gmcKN.debugMarker = new google.maps.Marker({
                            position: center,
                            map: gmcKN.map,
                            zIndex: 1
                        });
                    }
                    if (gmcKN.debuginfo === undefined) {
                        gmcKN.debuginfo = new google.maps.InfoWindow();
                    }
                    gmcKN.debugMarker.setPosition(center);
                    var debugstr = center.lng() + '; ' + center.lat() + ' zoom: ' + gmcKN.map.getZoom() + '<br />SW: ' + SW.lng() + ' ; ' + SW.lat() +
                        '<br/>NE: ' + NE.lng() + ' ; ' + NE.lat();
                    gmcKN.debuginfo.setContent(debugstr);
                    gmcKN.debuginfo.open(gmcKN.map, gmcKN.debugMarker);
                }
                //-------------


                // avoid repeated request, similar to avoiding double events on doubleclick
                var _ = "_";
                var cache = mapData.neLat + _ + mapData.neLon + _ + mapData.swLat + _ + mapData.swLon + _ + mapData.zoomLevel;
                if (gmcKN.async.lastCache === cache && forceUpdate === false)
                    return;
                gmcKN.async.lastCache = cache; // update

                gmcKN.mymap.events.loadMarkers(mapData);
            },

            polys: [], //cache drawn grid lines        
            loadMarkers: function (mapData) {

                var clusterImg = new google.maps.MarkerImage(gmcKN.mymap.settings.clusterImage.src,
                        new google.maps.Size(gmcKN.mymap.settings.clusterImage.width, gmcKN.mymap.settings.clusterImage.height),
                        null, new google.maps.Point(gmcKN.mymap.settings.clusterImage.offsetW, gmcKN.mymap.settings.clusterImage.offsetH)
                    );

                var pinImg = new google.maps.MarkerImage(gmcKN.mymap.settings.pinImage.src,
                    new google.maps.Size(gmcKN.mymap.settings.pinImage.width, gmcKN.mymap.settings.pinImage.height), null, null);
                var pinImg1 = new google.maps.MarkerImage(gmcKN.mymap.settings.pinImage1.src,
                    new google.maps.Size(gmcKN.mymap.settings.pinImage1.width, gmcKN.mymap.settings.pinImage1.height), null, null);
                var pinImg2 = new google.maps.MarkerImage(gmcKN.mymap.settings.pinImage2.src,
                    new google.maps.Size(gmcKN.mymap.settings.pinImage2.width, gmcKN.mymap.settings.pinImage2.height), null, null);
                var pinImg3 = new google.maps.MarkerImage(gmcKN.mymap.settings.pinImage3.src,
                    new google.maps.Size(gmcKN.mymap.settings.pinImage3.width, gmcKN.mymap.settings.pinImage3.height), null, null);

                ++gmcKN.async.lastSendGetMarkers;

                var postParams = '{' +
                    '"nelat":"' + mapData.neLat +
                    '","nelon":"' + mapData.neLon +
                    '","swlat":"' + mapData.swLat +
                    '","swlon":"' + mapData.swLon +
                    '","zoomlevel":"' + mapData.zoomLevel +
                    '","filter":"' + gmcKN.getFilterValues() +
                    '","sendid":"' + gmcKN.async.lastSendGetMarkers + '"}';

                var getParams = "/" +
                    "nelat=" + gmcKN.dEscape(mapData.neLat) + ";" +
                    "nelon=" + gmcKN.dEscape(mapData.neLon) + ";" +
                    "swlat=" + gmcKN.dEscape(mapData.swLat) + ";" +
                    "swlon=" + gmcKN.dEscape(mapData.swLon) + ";" +
                    "zoom=" + mapData.zoomLevel + ";" +
                    "filter=" + gmcKN.getFilterValues() + ";" +
                    "sid=" + gmcKN.async.lastSendGetMarkers;

                $.ajax({

                    type: 'GET', // get
                    url: gmcKN.mymap.settings.jsonGetMarkerUrl + getParams, // get

                    // type: 'POST', // post
                    // url: gmcKN.mymap.settings.jsonMarkerUrl, // post
                    // data: postParams, // post

                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function (data) {

                        if (data.Ok === "0") {
                            gmcKN.log(data.EMsg);
                            return; // invalid state has occured
                        }

                        var lastReceivedGetMarkers = data.Rid; // ReplyId
                        if (lastReceivedGetMarkers <= gmcKN.async.lastReceivedGetMarkers) {
                            // async mismatch, this is old reply, dont use it
                            gmcKN.log('async mismatch ' + lastReceivedGetMarkers + ' ' + gmcKN.async.lastReceivedGetMarkers);
                            return;
                        }
                        // update
                        gmcKN.async.lastReceivedGetMarkers = lastReceivedGetMarkers;

                        var mia = "";
                        if (data.Mia > 0) {
                            mia = "<br/>&nbsp;Mia: " + data.Mia;
                        }
                        document.getElementById("gmcKN_markersCount").innerHTML = "&nbsp;Markers: " + data.Count + " " + mia;

                        // grid lines clear current
                        $.each(gmcKN.mymap.events.polys, function () {
                            this.setMap(null); // clear prev lines
                        });
                        gmcKN.mymap.events.polys.length = 0; // clear array   

                        //gmcKN.mymap.events.loadKnn();

                        if (gmcKN.debug.showGridLines === true && data.Polylines) {

                            $.each(data.Polylines, function () {
                                var item = this;
                                var x = item.X;
                                var y = item.Y;
                                var x2 = item.X2;
                                var y2 = item.Y2;
                                var nowrapIsFalse = false;

                                // Creating the polyline object
                                var polyline = new google.maps.Polyline({
                                    path: [
                                  new google.maps.LatLng(y, x, nowrapIsFalse),
                                  new google.maps.LatLng(y2, x2, nowrapIsFalse)
                                ],
                                    strokeColor: "#f00",
                                    strokeOpacity: 1, //0.7,
                                    strokeWeight: 2,
                                    map: gmcKN.map
                                });
                                // used for ref, for next screen clearing
                                gmcKN.mymap.events.polys.push(polyline);
                            });
                        }

                        var markersDrawTodo = gmcKN.dynamicUpdateMarkers(data.Markers, gmcKN.markers, gmcKN.getKey, true);

                        $.each(markersDrawTodo, function () {
                            var item = this;
                            var lat = item.Y; // lat
                            var lon = item.X; // lon

                            var latLng = new google.maps.LatLng(lat, lon, true);

                            // identify type and select icon
                            var iconImg;
                            if (item.C === 1) {
                                if (item.T === 1) {
                                    iconImg = pinImg1;
                                }
                                else if (item.T === 2) {
                                    iconImg = pinImg2;
                                }
                                else if (item.T === 3) {
                                    iconImg = pinImg3;
                                } else {
                                    iconImg = pinImg; //fallback
                                }
                            } else {
                                iconImg = clusterImg;
                            }

                            // this draws a new marker on map
                            var marker = new google.maps.Marker({
                                position: latLng,
                                map: gmcKN.map,
                                icon: iconImg,
                                zIndex: 1
                            });
                            var key = gmcKN.getKey(item);
                            marker.set("key", key); // ref used for next event, remove or keep the marker

                            if (item.C === 1) { // single item, no cluster
                                //gmcKN.infowindow.close();
                                google.maps.event.addListener(marker, 'click', function (event) {
                                    gmcKN.mymap.events.popupWindow(marker, item);
                                });
                            }
                            else { // cluster marker
                                google.maps.event.addListener(marker, 'click', function (event) {
                                    //gmcKN.infowindow.close();
                                    var z = gmcKN.map.getZoom();
                                    var n;
                                    // zoom in steps are depended on current zoom level
                                    if (z <= 8) { n = 3; }
                                    else if (z <= 12) { n = 2; }
                                    else { n = 1; }

                                    gmcKN.map.setZoom(z + n);
                                    gmcKN.map.setCenter(latLng);
                                });

                                var label = new gmcKN.Label({
                                    map: gmcKN.map
                                }, key, item.C);

                                label.bindTo('position', marker, 'position');
                                label.bindTo('visible', marker, 'visible');
                                var text = item.C === undefined ? "error" : item.C;
                                label.set('text', text);
                            }

                            gmcKN.markers.push(marker);
                        });

                        // clear array
                        markersDrawTodo.length = 0;
                    },
                    error: function (xhr, err) {
                        var msg = "readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText;
                        gmcKN.log(msg);
                        alert(gmcKN.mymap.settings.textErrorMessage); //friendly error msg
                        //alert(msg);                        
                    }
                });

            },

            // load Knn
            loadKnn: function () {

                var k = gmcKN.knn === true ? gmcKN.knn_K : 0;

                // if cache is empty and no k request then stop here
                if (gmcKN.knnmarkers.length === 0 && k === 0) {
                    return;
                }

                // clear currently displayed
                for (var i in gmcKN.knnmarkers) {
                    if (gmcKN.knnmarkers.hasOwnProperty(i)) {
                        gmcKN.knnmarkers[i].setMap(null); // this removes the marker from the map    
                    }
                }

                gmcKN.knnmarkers.length = 0; // clear cache
                if (k === 0) {
                    return;  // don't find Knn
                }

                // dependent on search marker (defined in other js file)
                // locate K-NN from search marker pos
                if (!gmcKN.searchInfo || !gmcKN.searchInfo.searchMarker) return;

                var m = gmcKN.searchInfo.searchMarker;

                // Get params
                var getParams = "/" +
                    "lat=" + gmcKN.dEscape(gmcKN.round(m.getPosition().lat(),7)) + ";" +
                    "lon=" + gmcKN.dEscape(gmcKN.round(m.getPosition().lng(),7)) + ";" +
                    "k=" + k;

                $.ajax({
                    type: 'GET', // get
                    url: gmcKN.mymap.settings.jsonKnnUrl + getParams, // get                   
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function (data) {

                        if (data.Ok === "0") {
                            gmcKN.log(data.EMsg);
                            return; // invalid state has occured
                        }

                        $.each(data.Nns, function () {
                            var item = this;
                            var latlon = new google.maps.LatLng(item.Y, item.X);
                            var iconImg = new google.maps.MarkerImage(gmcKN.mymap.settings.pinImage.src, new google.maps.Size(gmcKN.mymap.settings.pinImage.width, gmcKN.mymap.settings.pinImage.height), null, null);

                            var marker = new google.maps.Marker({
                                position: latlon,
                                map: gmcKN.map,
                                icon: iconImg,
                                zIndex: 2
                            });
                            gmcKN.knnmarkers.push(marker);
                        });

                    },
                    error: function (xhr, err) {
                        var msg = "readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText;
                        gmcKN.log(msg);
                        //alert(msg);
                        alert(gmcKN.mymap.settings.textErrorMessage); //friendly error msg
                    }
                });
            },

            // marker detail
            popupWindow: function (marker, item) {

                ++gmcKN.async.lastSendMarkerDetail;

                // Post params
                var postParams = '{"id":"' + item.I +                    
                    '","sendid":"' + gmcKN.async.lastSendMarkerDetail +
                    '"}';

                // Get params
                var getParams = "/" +
                    "id=" + item.I + ";" +
                    "sid=" + gmcKN.async.lastSendMarkerDetail;

                $.ajax({

                    // GET version
                    type: 'GET', // get
                    url: gmcKN.mymap.settings.jsonGetMarkerInfoUrl + getParams, // get

                    // POST version
                    // type: 'POST', // post                    
                    // url: gmcKN.mymap.settings.jsonMarkerInfoUrl, // post                    
                    // data: postParams, // post

                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function (data) {

                        if (data.Ok === "0") {
                            gmcKN.log(data.EMsg);
                            return; // invalid state has occured
                        }

                        var lastReceivedMarkerDetail = data.Rid; // replyId
                        if (lastReceivedMarkerDetail <= gmcKN.async.lastReceivedMarkerDetail) {
                            // async mismatch, this is old reply, dont use it
                            gmcKN.log('async mismatch ' + lastReceivedMarkerDetail + ' ' + gmcKN.async.lastReceivedMarkerDetail);
                            return;
                        }
                        // update
                        gmcKN.async.lastReceivedMarkerDetail = lastReceivedMarkerDetail;
                        
                        gmcKN.infowindow.setContent(data.Content);
                        gmcKN.infowindow.open(gmcKN.map, marker);
                    },
                    error: function (xhr, err) {
                        var msg = "readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText + "\nerr:" + err;
                        gmcKN.log(msg);
                        //alert(msg);
                        alert(gmcKN.mymap.settings.textErrorMessage); //friendly error msg
                    }
                });
            }
        }
    },


    // lon, lat, count, type, cannot use id because it is not set for cluster markers on return json
    getKey: function (p) {
        var s = p.X + "__" + p.Y + "__" + p.C + "__" + p.T;
        return s.replace(/\./g, "_"); //replace . with _
    },

    // lon, lat, distance
    getKnnKey: function (p) { //point
        var s = p.X + "__" + p.Y + "__" + p.Dist;
        return s.replace(/\./g, "_"); //replace . with _
    },

    dEscape: function (d) { //double escape
        var s = d + "";
        return s.replace(/\./g, "_"); //replace . with _
    },

    // Checkbox values as binary sum
    getFilterValues: function () {
        var s = "";
        var cb1 = $(s = '#gmcKN_Clustering') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;
        var cb2 = $(s = '#gmcKN_Lines') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;

        var cb3 = $(s = '#gmcKN_Type1') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;
        var cb4 = $(s = '#gmcKN_Type2') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;
        var cb5 = $(s = '#gmcKN_Type3') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;
        var cb6 = $(s = '#gmcKN_Type4') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;
        var cb7 = $(s = '#gmcKN_Type5') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;
        var cb8 = $(s = '#gmcKN_Type6') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;
        var cb9 = $(s = '#gmcKN_Type7') ? ($(s).attr('checked') === 'checked' ? 1 : 0) : 0;

        // binary sum, take less space in string request
        var filter = cb1 * 1 + cb2 * 2 + cb3 * 4 + cb4 * 8
            + cb5 * 16 + cb6 * 32 + cb7 * 64 + cb8 * 128 + cb9 * 256;
        return filter + "";
    },

    checkboxClicked: function (type, isChecked) {
        if (type === 'gmc_meta_lines') {
            gmcKN.debug.showGridLines = !gmcKN.debug.showGridLines;
        }

        else if (type === 'gmc_meta_knn') {
            gmcKN.knn = !gmcKN.knn;
        }

        // force update screen
        gmcKN.mymap.events.getBounds(true);
    },

    // set count labels, style and class for the clusters
    Label: function (opt_options, id, count) {
        this.setValues(opt_options);
        var span = this.span_ = document.createElement('span');

        if (count >= 10000) {
            span.className = "gmcKN_clustersize5";
        }
        else if (count >= 1000) {
            span.className = "gmcKN_clustersize4";
        }
        else if (count >= 100) {
            span.className = "gmcKN_clustersize3";
        }
        else if (count >= 10) {
            span.className = "gmcKN_clustersize2";
        }
        else {
            span.className = "gmcKN_clustersize1";
        }

        var div = this.div_ = document.createElement('div');
        div.appendChild(span);
        div.className = "countinfo_" + id;
        div.style.cssText = 'position: absolute; display: none;';
    },

    // Only update new markers not currently drawn and remove obsolete markers on screen
    dynamicUpdateMarkers: function (markers, cache, keyfunction, isclusterbased) {
        var markersCacheIncome = []; // points to be drawn, new points received
        var markersCacheOnMap = [];  // current drawn points

        // points to be displayed, diff of markersCacheIncome and markersCacheOnMap
        var markersDrawTodo = [];

        // store new points to be drawn                  
        for (i in markers) {
            if (markers.hasOwnProperty(i)) {
                p = markers[i];
                key = keyfunction(p); //key                            
                markersCacheIncome[key] = p;
            }
        }
        // store current existing valid markers
        for (i in cache) {
            if (cache.hasOwnProperty(i)) {
                m = cache[i];
                key = m.get("key"); // key  
                if (key !== 0) { // 0 is used as has been deleted
                    markersCacheOnMap[key] = 1;
                }

                if (key === undefined) {
                    gmcKN.log("error in code: key"); //catch error in code
                }
            }
        }

        // add new markers from event not already drawn
        for (var i in markers) {
            if (markers.hasOwnProperty(i)) {
                var p = markers[i];
                key = keyfunction(p); //key                            
                if (markersCacheOnMap[key] === undefined) {
                    if (markersCacheIncome[key] === undefined) {
                        gmcKN.log("error in code: key2"); //catch error in code
                    }
                    var newmarker = markersCacheIncome[key];
                    markersDrawTodo.push(newmarker);
                }
            }
        }

        // remove current markers which should not be displayed
        for (i in cache) {
            if (cache.hasOwnProperty(i)) {
                var m = cache[i];
                key = m.get("key"); //key                            
                if (key !== 0 && markersCacheIncome[key] === undefined) {
                    if (isclusterbased === true) {
                        $(".countinfo_" + key).remove();
                    }
                    cache[i].set("key", 0); // mark as deleted
                    cache[i].setMap(null); // this removes the marker from the map
                }
            }
        }

        // trim markers array size
        var temp = [];
        for (i in cache) {
            if (cache.hasOwnProperty(i)) {
                var key = cache[i].get("key"); //key                            
                if (key !== 0) {
                    tempItem = cache[i];
                    temp.push(tempItem);
                }
            }
        }

        cache.length = 0;
        for (i in temp) {
            if (temp.hasOwnProperty(i)) {
                var tempItem = temp[i];
                cache.push(tempItem);
            }
        }

        // clear array
        markersCacheIncome.length = 0;
        markersCacheOnMap.length = 0;
        temp.length = 0;

        return markersDrawTodo;
    }
};


gmcKN.Label.prototype = new google.maps.OverlayView;
gmcKN.Label.prototype.onAdd = function () {
    var pane = this.getPanes().overlayLayer;
    pane.appendChild(this.div_);

    var that = this;
    this.listeners_ = [
        google.maps.event.addListener(this, 'idle', function () { that.draw(); }),
        google.maps.event.addListener(this, 'visible_changed', function () { that.draw(); }),
        google.maps.event.addListener(this, 'position_changed', function () { that.draw(); }),
        google.maps.event.addListener(this, 'text_changed', function () { that.draw(); })
    ];
};
gmcKN.Label.prototype.onRemove = function () {
    this.div_.parentNode.removeChild(this.div_);

    for (var i = 0, I = this.listeners_.length; i < I; ++i) {
        google.maps.event.removeListener(this.listeners_[i]);
    }
};
gmcKN.Label.prototype.draw = function () {
    var projection = this.getProjection();
    var position = projection.fromLatLngToDivPixel(this.get('position'));

    var div = this.div_;
    div.style.left = position.x + 'px';
    div.style.top = position.y + 'px';

    var visible = this.get('visible');
    div.style.display = visible ? 'block' : 'none';

    this.span_.innerHTML = this.get('text').toString();
};

google.maps.event.addDomListener(window, 'load', gmcKN.mymap.initialize); // load map
