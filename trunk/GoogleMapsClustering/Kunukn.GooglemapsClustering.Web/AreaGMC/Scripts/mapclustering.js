﻿// Author: Kunuk Nykjaer et al.

var gmcKN = {

    markers: [],
    map: undefined,
    infowindow: undefined,
    debugMarker: undefined,
    debuginfo: undefined,

    searchInfo: {
        searchMarker: null,
        zoomLevel: 12,
        round: 6,
        prefix: 4
    },

    // http://code.google.com/intl/da-DK/apis/maps/documentation/javascript/reference.html
    geocoder: new google.maps.Geocoder(),
    debug: {
        showGridLines: false,
        showBoundaryMarker: false,
        showCalloutLatLon: true
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
        console.log(s);
    },

    mymap: {
        latlonsearch: function () {
            var lat = $('#gmcKN_latitude').val() + "";
            var lon = $('#gmcKN_longitude').val() + "";
            if (lat.length > gmcKN.searchInfo.round + gmcKN.searchInfo.prefix) {
                lat = lat.substring(0, gmcKN.searchInfo.round + 2 + gmcKN.searchInfo.prefix);
            }

            if (lon.length > gmcKN.searchInfo.round + gmcKN.searchInfo.prefix) {
                lon = lon.substring(0, gmcKN.searchInfo.round + 2 + gmcKN.searchInfo.prefix);
            }

            lat = parseFloat(lat).toFixed(gmcKN.searchInfo.round);
            lon = parseFloat(lon).toFixed(gmcKN.searchInfo.round);
            $('#gmcKN_latitude').val(lat); //update
            $('#gmcKN_longitude').val(lon);
            $('#gmcKN_lonlat').val(lon + ';' + lat);
            var latlon = new google.maps.LatLng(lat, lon);

            gmcKN.geocoder.geocode({ 'latLng': latlon }, function (results, status) {
                if (status === google.maps.GeocoderStatus.OK) {
                    if (results[0]) {
                        $('#gmcKN_search').val(results[0].formatted_address);
                    }
                }
                else {
                    $('#gmcKN_search').val("");
                }

                gmcKN.searchInfo.searchMarker.setPosition(latlon);
                gmcKN.map.setOptions({
                    //zoom: gmcKN.searchInfo.zoomLevel,
                    center: latlon
                });
            });
        },
        
        initialize: function () {            
            var center = new google.maps.LatLng(gmcKN.mymap.settings.mapCenterLat, gmcKN.mymap.settings.mapCenterLon, true);

            gmcKN.map = new google.maps.Map(document.getElementById('gmcKN_map_canvas'), {
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
                document.getElementById("gmcKN_zoomInfo").innerHTML = "zoom: " + gmcKN.map.getZoom() + ".  ";
                if (gmcKN.map.getZoom() < gmcKN.mymap.settings.alwaysClusteringEnabledWhenZoomLevelLess) {
                    $('#gmcKN_Clustering_span').hide();
                }
                else {
                    $('#gmcKN_Clustering_span').show();
                }
            });
            google.maps.event.trigger(gmcKN.map, 'zoom_changed'); //trigger first time event


            // search -------------        
            // http://tech.cibul.net/geocode-with-google-maps-api-v3/

            $('#gmcKN_search').focus();
            $('#gmcKN_latitude').keypress(function (e) {
                if (e.which === 13) {
                    gmcKN.mymap.latlonsearch();
                }
            });
            $('#gmcKN_longitude').keypress(function (e) {
                if (e.which === 13) {
                    gmcKN.mymap.latlonsearch();
                }
            });

            gmcKN.searchInfo.searchMarker = new google.maps.Marker({ //init
                map: gmcKN.map,
                draggable: true,
                zIndex: 1
            });

            gmcKN.searchInfo.searchMarker.setPosition(new google.maps.LatLng(gmcKN.mymap.settings.mapCenterLat, gmcKN.mymap.settings.mapCenterLon));
            gmcKN.searchInfo.searchMarker.setVisible(true);

            //Add listener to marker for reverse geocoding
            google.maps.event.addListener(gmcKN.searchInfo.searchMarker, 'drag', function () {
                gmcKN.geocoder.geocode({ 'latLng': gmcKN.searchInfo.searchMarker.getPosition() }, function (results, status) {
                    if (status === google.maps.GeocoderStatus.OK) {
                        if (results[0]) {
                            var addr = results[0].formatted_address;
                            $('#gmcKN_search').val(addr);
                            var lat = gmcKN.searchInfo.searchMarker.getPosition().lat() + "";
                            var lon = gmcKN.searchInfo.searchMarker.getPosition().lng() + "";
                            if (lat.length > gmcKN.searchInfo.round + gmcKN.searchInfo.prefix)
                                lat = lat.substring(0, gmcKN.searchInfo.round + 2 + gmcKN.searchInfo.prefix);

                            if (lon.length > gmcKN.searchInfo.round + gmcKN.searchInfo.prefix)
                                lon = lon.substring(0, gmcKN.searchInfo.round + 2 + gmcKN.searchInfo.prefix);

                            lat = parseFloat(lat).toFixed(gmcKN.searchInfo.round);
                            lon = parseFloat(lon).toFixed(gmcKN.searchInfo.round);
                            $('#gmcKN_latitude').val(lat);
                            $('#gmcKN_longitude').val(lon);
                            $('#gmcKN_lonlat').val(lon + ';' + lat);
                        }
                    }
                });
            });

            $(function () {
                $("#gmcKN_search").autocomplete({
                    //This uses the geocoder to fetch address values
                    source: function (request, response) {
                        gmcKN.geocoder.geocode({ 'address': request.term }, function (results, status) { //WORLD
                            response($.map(results, function (item) {

                                return {
                                    label: item.formatted_address,
                                    value: item.formatted_address,
                                    latitude: item.geometry.location.lat(),
                                    longitude: item.geometry.location.lng()
                                }

                            }));
                        })
                    },
                    //This is executed upon selection of an address
                    select: function (event, ui) {
                        //parseFloat()   
                        var lat = ui.item.latitude + "";
                        var lon = ui.item.longitude + "";
                        if (lat.length > gmcKN.searchInfo.round + gmcKN.searchInfo.prefix) lat = lat.substring(0, gmcKN.searchInfo.round +
                            2 + gmcKN.searchInfo.prefix);
                        if (lon.length > gmcKN.searchInfo.round + gmcKN.searchInfo.prefix) lon = lon.substring(0, gmcKN.searchInfo.round +
                            2 + gmcKN.searchInfo.prefix);
                        lat = parseFloat(lat).toFixed(gmcKN.searchInfo.round);
                        lon = parseFloat(lon).toFixed(gmcKN.searchInfo.round);

                        $("#gmcKN_latitude").val(lat);
                        $("#gmcKN_longitude").val(lon);
                        $('#gmcKN_lonlat').val(lon + ';' + lat);
                        var location = new google.maps.LatLng(lat, lon);

                        gmcKN.searchInfo.searchMarker.setPosition(location);
                        gmcKN.searchInfo.searchMarker.setVisible(true);

                        gmcKN.map.setOptions({
                            zoom: gmcKN.searchInfo.zoomLevel,
                            center: location
                        });
                    }
                });
            });
            // end search -------------

        },
        settings: {
            gridx: 6,
            gridy: 5,
            mapCenterLat: 35,
            mapCenterLon: 10,
            zoomLevel: 2,
            zoomlevelClusterStop: 15,
            alwaysClusteringEnabledWhenZoomLevelLess: 8,
            
            jsonMarkerUrl: '/AreaGMC/gmc.svc/GetMarkers',            
            jsonMarkerDetailUrl: '/AreaGMC/gmc.svc/GetMarkerDetail',

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
            textErrorMessage: 'Error'            
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
                mapData.neLat = NE.lat();
                mapData.neLon = NE.lng();
                mapData.swLat = SW.lat();
                mapData.swLon = SW.lng();
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

                var parameters = '{' +
                    '"nelat":"' + mapData.neLat +
                    '","nelon":"' + mapData.neLon +
                    '","swlat":"' + mapData.swLat +
                    '","swlon":"' + mapData.swLon +
                    '","zoomlevel":"' + mapData.zoomLevel +
                    '","gridx":"' + gmcKN.mymap.settings.gridx +
                    '","gridy":"' + gmcKN.mymap.settings.gridy +
                    '","zoomlevelClusterStop":"' + gmcKN.mymap.settings.zoomlevelClusterStop +
                    '","filter":"' + gmcKN.getFilterValues() +
                    '","sendid":"' + (++gmcKN.async.lastSendGetMarkers) + '"}';

                $.ajax({
                    type: 'POST',
                    url: gmcKN.mymap.settings.jsonMarkerUrl,
                    data: parameters,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function (data) {
                        var items = data;

                        var lastReceivedGetMarkers = items.ReplyId;
                        if (lastReceivedGetMarkers <= gmcKN.async.lastReceivedGetMarkers) {
                            // async mismatch, this is old reply, dont use it
                            gmcKN.log('async mismatch ' + lastReceivedGetMarkers + ' ' + gmcKN.async.lastReceivedGetMarkers);
                            return;
                        }
                        // update
                        gmcKN.async.lastReceivedGetMarkers = lastReceivedGetMarkers;

                        // grid lines clear current
                        $.each(gmcKN.mymap.events.polys, function () {
                            var item = this;
                            item.setMap(null); // clear prev lines
                        });
                        gmcKN.mymap.events.polys.length = 0; // clear array   


                        if (gmcKN.debug.showGridLines === true && items.Polylines !== null) {
                            $.each(items.Polylines, function () {
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
                                gmcKN.mymap.events.polys.push(polyline); // used for ref, for next screen clearing
                            });
                        }

                        var pointsCacheIncome = []; // points to be drawn, new points received
                        var pointsCacheOnMap = [];  // current drawn points
                        var newmarkersTodo = [];    // points to be displayed

                        // store new points to be drawn                  
                        for (i in items.Points) {
                            if (items.Points.hasOwnProperty(i)) {
                                var p = items.Points[i];
                                var key = gmcKN.getKey(p); //key                            
                                pointsCacheIncome[key] = p;
                            }
                        }
                        // store current existing valid markers
                        for (i in gmcKN.markers) {
                            if (gmcKN.markers.hasOwnProperty(i)) {
                                var m = gmcKN.markers[i];
                                var key = m.get("key"); //key  
                                if (key !== 0) {//0 is used as has been deleted
                                    pointsCacheOnMap[key] = 1;
                                }

                                if (key === undefined) {
                                    gmcKN.log("error in code: key"); //catch error in code
                                }
                            }
                        }

                        // add new markers from event not already drawn
                        for (i in items.Points) {
                            if (items.Points.hasOwnProperty(i)) {
                                var p = items.Points[i];
                                var key = gmcKN.getKey(p); //key                            
                                if (pointsCacheOnMap[key] === undefined) {
                                    if (pointsCacheIncome[key] === undefined) {
                                        gmcKN.log("error in code: key2"); //catch error in code
                                    }
                                    var newmarker = pointsCacheIncome[key];
                                    newmarkersTodo.push(newmarker);
                                }
                            }
                        }

                        // remove current markers which should not be displayed
                        for (i in gmcKN.markers) {
                            if (gmcKN.markers.hasOwnProperty(i)) {
                                var m = gmcKN.markers[i];
                                var key = m.get("key"); //key                            
                                if (key !== 0 && pointsCacheIncome[key] === undefined) {
                                    $(".countinfo_" + key).remove();
                                    gmcKN.markers[i].set("key", 0); // mark as deleted
                                    gmcKN.markers[i].setMap(null); // this removes the marker from the map
                                }
                            }
                        }


                        // trim markers array size
                        var temp = [];
                        for (i in gmcKN.markers) {
                            if (gmcKN.markers.hasOwnProperty(i)) {
                                var key = gmcKN.markers[i].get("key"); //key                            
                                if (key !== 0) {
                                    var tempItem = gmcKN.markers[i];
                                    temp.push(tempItem);
                                }
                            }
                        }

                        gmcKN.markers.length = 0;
                        for (i in temp) {
                            if (temp.hasOwnProperty(i)) {
                                var tempItem = temp[i];
                                gmcKN.markers.push(tempItem);
                            }
                        }

                        // clear array
                        pointsCacheIncome.length = 0;
                        pointsCacheOnMap.length = 0;
                        temp.length = 0;

                        $.each(newmarkersTodo, function () {
                            var item = this;
                            var lat = item.Y; //lat
                            var lon = item.X; //lon

                            var latLng = new google.maps.LatLng(lat, lon, true);

                            var iconImg;
                            if (item.C === 1) {
                                if (item.T === '1') {
                                    iconImg = pinImg1;
                                }
                                else if (item.T === '2') {
                                    iconImg = pinImg2;
                                }
                                else if (item.T === '3') {
                                    iconImg = pinImg3;
                                } else {
                                    iconImg = pinImg; //fallback
                                }
                            } else {
                                iconImg = clusterImg;
                            }

                            var marker = new google.maps.Marker({
                                position: latLng,
                                map: gmcKN.map,
                                icon: iconImg,
                                zIndex: 1
                            });
                            var key = gmcKN.getKey(item);
                            marker.set("key", key); // used for next event, remove or keep the marker

                            if (item.C === 1) {
                                //gmcKN.infowindow.close();
                                google.maps.event.addListener(marker, 'click', function (event) {
                                    gmcKN.mymap.events.attachCallOut(marker, item);
                                });
                            }
                            else {
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
                        newmarkersTodo.length = 0;
                    },
                    error: function (xhr, err) {
                        //alert(gmcKN.mymap.settings.textErrorMessage); //friendly error msg
                        alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText);
                    }
                });

            },

            // popup window
            attachCallOut: function (marker, item) {
                var parameters = '{"id":"' + item.I +
                    '","type":"' + item.T +
                    '","sendid":"' + (++gmcKN.async.lastSendMarkerDetail) +
                    '"}';

                $.ajax({
                    type: 'POST',
                    url: gmcKN.mymap.settings.jsonMarkerDetailUrl,
                    data: parameters,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function (data) {
                        items = data;

                        var lastReceivedMarkerDetail = items.ReplyId;
                        if (lastReceivedMarkerDetail <= gmcKN.async.lastReceivedMarkerDetail) {
                            // async mismatch, this is old reply, dont use it
                            gmcKN.log('async mismatch ' + lastReceivedMarkerDetail + ' ' + gmcKN.async.lastReceivedMarkerDetail);
                            return;
                        }
                        // update
                        gmcKN.async.lastReceivedMarkerDetail = lastReceivedMarkerDetail;

                        var latlon = "";
                        if (gmcKN.debug.showCalloutLatLon === true)
                            latlon = "<br/>lat: " + marker.getPosition().lat() + " lon: " + marker.getPosition().lng();
                        gmcKN.infowindow.setContent(items.Content + latlon);
                        gmcKN.infowindow.open(gmcKN.map, marker);
                    },
                    error: function (xhr, err) {
                        alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText);
                    }

                });
            }
        }
    },

    // lon, lat, count, type
    getKey: function (p) { //point
        var s = p.X + "__" + p.Y + "__" + p.C + "__" + p.T;
        return s.replace(/\./g, "_"); //replace . with _
    },

    // Checkbox values as binary sum
    getFilterValues: function () {
        var cb1 = $('#gmcKN_Clustering').attr('checked') === 'checked' ? 1 : 0;
        var cb2 = $('#gmcKN_Lines').attr('checked') === 'checked' ? 1 : 0;

        var cb3 = $('#gmcKN_Type1').attr('checked') === 'checked' ? 1 : 0;
        var cb4 = $('#gmcKN_Type2').attr('checked') === 'checked' ? 1 : 0;
        var cb5 = $('#gmcKN_Type3').attr('checked') === 'checked' ? 1 : 0;

        var filter = cb1 * 1 + cb2 * 2 + cb3 * 4 + cb4 * 8 + cb5 * 16; // binary sum
        return filter + '';
    },

    checkboxClicked: function (type, isChecked) {
        if (type === 'gmc_meta_lines') {
            gmcKN.debug.showGridLines = !gmcKN.debug.showGridLines;
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
