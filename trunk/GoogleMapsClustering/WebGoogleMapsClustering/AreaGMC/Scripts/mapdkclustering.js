﻿// Author: Kunuk Nykjaer et al.

var markers = [];

var map, infowindow, debugMarker, debuginfo;

var searchInfo = {
    searchMarker: null,
    zoomLevel: 13,
    round: 6,
    prefix: 4
};

var geocoder = new google.maps.Geocoder();

var debug = {
    showGridLines: true,
    showBoundaryMarker: false
};

//prevent async send/receive order problem by using counter ref in send/reply in webservice
var async = {
    lastSendGetMarkers: 0, //get markers
    lastReceivedGetMarkers: 0,
    lastSendMarkerDetail: 0,
    lastReceivedMarkerDetail: 0,
    lastSendSetType: 0,
    lastReceivedSetType: 0,
    lastSendGetAccessToken: 0,
    lastReceivedGetAccessToken: 0,
    lastCache: ""
};


var mymap = {

    latlonsearch: function () {
        // parseFloat() .toFixed(searchInfo.round);    
        var lat = $('#latitude').val() + "";
        var lon = $('#longitude').val() + "";
        if (lat.length > searchInfo.round + searchInfo.prefix) lat = lat.substring(0, searchInfo.round + 2 + searchInfo.prefix);
        if (lon.length > searchInfo.round + searchInfo.prefix) lon = lon.substring(0, searchInfo.round + 2 + searchInfo.prefix);
        lat = parseFloat(lat).toFixed(searchInfo.round);
        lon = parseFloat(lon).toFixed(searchInfo.round);
        $('#latitude').val(lat); //update
        $('#longitude').val(lon);
        $('#lonlat').val(lon + ';' + lat);
        var latlon = new google.maps.LatLng(lat, lon);

        geocoder.geocode({ 'latLng': latlon }, function (results, status) {
            if (status === google.maps.GeocoderStatus.OK) {
                if (results[0]) {
                    $('#search').val(results[0].formatted_address);
                }
            }
            else {
                $('#search').val("");
            }

            searchInfo.searchMarker.setPosition(latlon);
            map.setOptions({
                //zoom: searchInfo.zoomLevel,
                center: latlon
            });
        });
    },

    initialize: function () {
        // initialize is exec as $(document).ready(function()


        var center = new google.maps.LatLng(mymap.settings.mapCenterLat, mymap.settings.mapCenterLon, true);

        map = new google.maps.Map(document.getElementById('map_canvas'), {
            zoom: mymap.settings.zoomLevel,
            center: center,
            scrollwheel: true,
            navigationControl: true,
            mapTypeControl: true,
            draggable: true,
            scaleControl: true,
            streetViewControl: false,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            backgroundColor: '#ffffff',
            draggableCursor: 'move',
            minZoom: 1,
            maxZoom: 19
        });

        google.maps.event.addListener(map, 'idle', function () { mymap.events.getBounds(map, false); });
        google.maps.event.addListener(map, 'zoom_changed', function () {
            document.getElementById("zoomInfo").innerHTML = "zoom: " + map.getZoom() + ".  ";
        });
        google.maps.event.trigger(map, 'zoom_changed');


        // search -------------        
        // http://tech.cibul.net/geocode-with-google-maps-api-v3/

        document.getElementById('search').focus();
        $('#latitude').keypress(function (e) {
            if (e.which == 13) {
                mymap.latlonsearch();
            }
        });
        $('#longitude').keypress(function (e) {
            if (e.which == 13) {
                mymap.latlonsearch();
            }
        });

        searchInfo.searchMarker = new google.maps.Marker({
            map: map,
            draggable: true,
            zIndex: 1
        });

        searchInfo.searchMarker.setPosition(new google.maps.LatLng(mymap.settings.mapCenterLat, mymap.settings.mapCenterLon));
        searchInfo.searchMarker.setVisible(true);

        //Add listener to marker for reverse geocoding
        google.maps.event.addListener(searchInfo.searchMarker, 'drag', function () {
            geocoder.geocode({ 'latLng': searchInfo.searchMarker.getPosition() }, function (results, status) {
                if (status == google.maps.GeocoderStatus.OK) {
                    if (results[0]) {
                        var addr = results[0].formatted_address.replace(/, Danmark/gi, "");
                        //var addr = results[0].formatted_address;
                        $('#search').val(addr);
                        var lat = searchInfo.searchMarker.getPosition().lat() + "";
                        var lon = searchInfo.searchMarker.getPosition().lng() + "";
                        if (lat.length > searchInfo.round + searchInfo.prefix) lat = lat.substring(0, searchInfo.round + 2 + searchInfo.prefix);
                        if (lon.length > searchInfo.round + searchInfo.prefix) lon = lon.substring(0, searchInfo.round + 2 + searchInfo.prefix);
                        lat = parseFloat(lat).toFixed(searchInfo.round);
                        lon = parseFloat(lon).toFixed(searchInfo.round);
                        $('#latitude').val(lat);
                        $('#longitude').val(lon);
                        $('#lonlat').val(lon + ';' + lat);
                    }
                }
            });
        });

        $(function () {
            $("#search").autocomplete({
                //This bit uses the geocoder to fetch address values
                source: function (request, response) {
                    geocoder.geocode({ "address": request.term, "region": "dk" }, function (results, status) { //DENMARK
                        //geocoder.geocode({ 'address': request.term  }, function (results, status) { //WORLD
                        response($.map(results, function (item) {
                            if (item.formatted_address.indexOf(", Danmark") >= 0) {
                                return {
                                    label: item.formatted_address.replace(/, Danmark/gi, ""),
                                    value: item.formatted_address.replace(/, Danmark/gi, ""),
                                    latitude: item.geometry.location.lat(),
                                    longitude: item.geometry.location.lng()
                                }
                            }
                        }));
                    })
                },
                //This bit is executed upon selection of an address
                select: function (event, ui) {
                    //parseFloat()   
                    var lat = ui.item.latitude + "";
                    var lon = ui.item.longitude + "";
                    if (lat.length > searchInfo.round + searchInfo.prefix) lat = lat.substring(0, searchInfo.round + 2 + searchInfo.prefix);
                    if (lon.length > searchInfo.round + searchInfo.prefix) lon = lon.substring(0, searchInfo.round + 2 + searchInfo.prefix);
                    lat = parseFloat(lat).toFixed(searchInfo.round);
                    lon = parseFloat(lon).toFixed(searchInfo.round);

                    $("#latitude").val(lat);
                    $("#longitude").val(lon);
                    $('#lonlat').val(lon + ';' + lat);
                    var location = new google.maps.LatLng(lat, lon);

                    searchInfo.searchMarker.setPosition(location);
                    searchInfo.searchMarker.setVisible(true);

                    map.setOptions({
                        zoom: searchInfo.zoomLevel,
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
        mapCenterLat: 56.1, //-40   56.1  0
        mapCenterLon: 11, //180   11   0
        zoomLevel: 7, //7  1
        zoomlevelClusterStop: 15,
        access_token: 'todo',
        jsonMarkerUrl: 'WebService/MapService.asmx/GetMarkers',
        jsonMarkerDetailUrl: 'WebService/MapService.asmx/GetMarkerDetail',
        jsonSetTypeUrl: 'WebService/MapService.asmx/SetType',
        jsonGetAccessTokenUrl: 'WebService/MapService.asmx/GetAccessToken',

        clusterImage: {
            src: 'Images/cluster2.png', //this is invisible img only used for click-event detecting
            height: 60,
            width: 60,
            offsetH: 30,
            offsetW: 30
        },
        pinImage: {
            src: 'Images/pin24.png',
            height: 24,
            width: 24,
            offsetH: 0,
            offsetW: 0
        },
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


    clusterOptions: { styles: [
        {
            url: "/Images/m1.png", //http://google-maps-utility-library-v3.googlecode.com/svn/trunk/markerclusterer/images/m1.png            
            style: 'font-family: arial;display:block;position: relative; left: -26px;top: -26px;font-weight:bold;text-align:center;line-height:52px;font-size:12px;font-weight:bold;' +
                      'white-space: nowrap;color:#fff;background:url(Images/m1.png) no-repeat 0 0;width:53px;height:52px;'
        },
        {
            url: "/Images/m2.png",
            style: 'font-family: arial;display:block;position: relative; left: -28px;top: -28px;font-weight:bold;text-align:center;line-height:55px;font-size:12px;font-weight:bold;' +
                      'white-space: nowrap;color:#fff;background:url(Images/m2.png) no-repeat 0 0;width:56px;height:55px;'
        },
        {
            url: "/Images/m3.png",
            style: 'font-family: arial;display:block;position: relative; left: -33px;top: -33px;font-weight:bold;text-align:center;line-height:65px;font-size:12px;font-weight:bold;' +
                      'white-space: nowrap;color:#fff;background:url(Images/m3.png) no-repeat 0 0;width:66px;height:65px;'
        },
        {
            url: "/Images/m4.png",
            style: 'font-family: arial;display:block;position: relative; left: -39px;top: -39px;font-weight:bold;text-align:center;line-height:77px;font-size:12px;font-weight:bold;' +
                      'white-space: nowrap;color:#fff;background:url(Images/m4.png) no-repeat 0 0;width:78px;height:77px;'
        },
        {
            url: "/Images/m5.png",
            style: 'font-family: arial;display:block;position: relative; left: -45px;top: -45px;font-weight:bold;text-align:center;line-height:89px;font-size:12px;font-weight:bold;' +
                      'white-space: nowrap;color:#fff;background:url(Images/m5.png) no-repeat 0 0;width:90px;height:89px;'
        }]
    },


    events: {
        getBounds: function (map, forceUpdate) {

            if (!infowindow) {
                infowindow = new google.maps.InfoWindow();
            }

            var bounds = map.getBounds();
            var NE = bounds.getNorthEast();
            var SW = bounds.getSouthWest();
            mapData = [];
            mapData.neLat = NE.lat();
            mapData.neLon = NE.lng();
            mapData.swLat = SW.lat();
            mapData.swLon = SW.lng();
            mapData.zoomLevel = map.getZoom();

            //------------- DEBUG
            if (debug.showBoundaryMarker) {
                var center = map.getCenter();
                if (!debugMarker) { // singleton-ish
                    debugMarker = new google.maps.Marker({
                        position: center,
                        map: map,
                        zIndex: 1
                    });
                }
                if (!debuginfo) {
                    debuginfo = new google.maps.InfoWindow();
                }
                debugMarker.setPosition(center);
                var debugstr = center.lng() + '; ' + center.lat() + ' zoom: ' + map.getZoom() + '<br />SW: ' + SW.lng() + ' ; ' + SW.lat() + '<br/>NE: ' + NE.lng() + ' ; ' + NE.lat();
                debuginfo.setContent(debugstr);
                debuginfo.open(map, debugMarker);
            }
            //-------------


            // avoid repeated request, similar to avoiding double events on doubleclick
            var _ = "_";
            var cache = mapData.neLat + _ + mapData.neLon + _ + mapData.swLat + _ + mapData.swLon + _ + mapData.zoomLevel;
            if (async.lastCache === cache && forceUpdate === false)
                return;
            async.lastCache = cache; // update

            mymap.events.loadMarkers(mapData);
        },



        polys: [], //cache drawn grid lines        
        loadMarkers: function (mapData) {

            var clusterImg = new google.maps.MarkerImage(mymap.settings.clusterImage.src, new google.maps.Size(mymap.settings.clusterImage.width, mymap.settings.clusterImage.height), null, new google.maps.Point(mymap.settings.clusterImage.offsetW, mymap.settings.clusterImage.offsetH));
            var pinImg = new google.maps.MarkerImage(mymap.settings.pinImage.src, new google.maps.Size(mymap.settings.pinImage.width, mymap.settings.pinImage.height), null, null);
            var pinImg1 = new google.maps.MarkerImage(mymap.settings.pinImage1.src, new google.maps.Size(mymap.settings.pinImage1.width, mymap.settings.pinImage1.height), null, null);
            var pinImg2 = new google.maps.MarkerImage(mymap.settings.pinImage2.src, new google.maps.Size(mymap.settings.pinImage2.width, mymap.settings.pinImage2.height), null, null);
            var pinImg3 = new google.maps.MarkerImage(mymap.settings.pinImage3.src, new google.maps.Size(mymap.settings.pinImage3.width, mymap.settings.pinImage3.height), null, null);

            var webMethod = mymap.settings.jsonMarkerUrl;
            var parameters = '{' + '"access_token":"' + mymap.settings.access_token + '","nelat":"' + mapData.neLat + '","nelon":"' + mapData.neLon + '","swlat":"' + mapData.swLat + '","swlon":"' + mapData.swLon + '","zoomlevel":"' + mapData.zoomLevel + '","gridx":"' + mymap.settings.gridx + '","gridy":"' + mymap.settings.gridy + '","zoomlevelClusterStop":"' + mymap.settings.zoomlevelClusterStop + '","sendid":"' + (++async.lastSendGetMarkers) + '"}';

            // http://stackoverflow.com/questions/3020351/javascript-jquery-ajax-post-error-driving-me-mad            
            $.ajax({
                type: 'POST',
                url: webMethod,
                data: parameters,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                success: function (data) {
                    var items = jQuery.parseJSON(data.d);

                    var lastReceivedGetMarkers = items.ReplyId;
                    if (lastReceivedGetMarkers <= async.lastReceivedGetMarkers) {
                        // async mismatch, this is old reply, dont use it
                        //console.log('async mismatch ' + lastReceivedGetMarkers + ' ' + async.lastReceivedGetMarkers);
                        return;
                    }
                    // update
                    async.lastReceivedGetMarkers = lastReceivedGetMarkers;


                    if (items.TokenValid === "0") {
                        alert("Demo time is over, TokenValid is invalid, please relogin");
                        return;
                    }

                    var success = items.Success;

                    if (debug.showGridLines) {
                        $.each(mymap.events.polys, function () {
                            var item = this;
                            item.setMap(null); // clear prev lines
                        });
                        mymap.events.polys.length = 0; // clear array                 

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
                                map: map
                            });
                            mymap.events.polys.push(polyline); // used for ref, for next screen clearing
                        });
                    }

                    var pointsCacheIncome = []; // points to be drawn  
                    var pointsCacheOnMap = []; // drawn points
                    var newmarkersTodo = [];

                    // store new points to be drawn                  
                    for (i in items.Points) {
                        if (items.Points.hasOwnProperty(i)) {
                            var p = items.Points[i];
                            var key = getKey(p.X, p.Y, p.C); //key                            
                            pointsCacheIncome[key] = p;
                        }
                    }
                    // store current existing valid markers
                    for (i in markers) {
                        if (markers.hasOwnProperty(i)) {
                            var m = markers[i];
                            var key = m.get("key"); //key  
                            if (key !== 0);
                            pointsCacheOnMap[key] = 1;
                            if (key === undefined) console.log("error key"); //catch error in code
                        }
                    }

                    // add new markers from event not already drawn
                    for (i in items.Points) {
                        if (items.Points.hasOwnProperty(i)) {
                            var p = items.Points[i];
                            var key = getKey(p.X, p.Y, p.C); //key                            
                            if (pointsCacheOnMap[key] === undefined && pointsCacheOnMap[key] !== 0) {
                                if (pointsCacheIncome[key] === undefined) console.log("error key2"); //catch error in code

                                newmarkersTodo.push(pointsCacheIncome[key]);
                            }
                        }
                    }

                    // remove current markers which should not be displayed
                    for (i in markers) {
                        if (markers.hasOwnProperty(i)) {
                            var m = markers[i];
                            var key = m.get("key"); //key                            
                            if (key !== 0 && pointsCacheIncome[key] === undefined) {
                                $(".countinfo_" + key).remove();
                                markers[i].set("key", 0);
                                markers[i].setMap(null);
                            }
                        }
                    }


                    // trim markers array size
                    var temp = [];
                    for (i in markers) {
                        if (markers.hasOwnProperty(i)) {
                            var key = markers[i].get("key"); //key                            
                            if (key !== 0) {
                                temp.push(markers[i]);
                            }
                        }
                    }
                    markers.length = 0;
                    for (i in temp) {
                        if (temp.hasOwnProperty(i)) {
                            markers.push(temp[i]);
                        }
                    }

                    $.each(newmarkersTodo, function () {
                        var item = this;
                        var lat = item.Y; //lat
                        var lon = item.X; //lon
                        var index = item.I; //id

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
                            map: map,
                            icon: iconImg,
                            zIndex: 1
                        });
                        var key = getKey(item.X, item.Y, item.C);
                        marker.set("key", key); // used for next event, remove or keep the marker


                        if (item.C === 1) {
                            //infowindow.close();
                            google.maps.event.addListener(marker, 'click', function (event) {
                                mymap.events.attachCallOut(marker, item);
                            });
                        }
                        else {
                            google.maps.event.addListener(marker, 'click', function (event) {
                                //infowindow.close();
                                var z = map.getZoom();
                                var n;
                                if (z <= 8) { n = 3; }
                                else if (z <= 12) { n = 2; }
                                else { n = 1; }

                                map.setZoom(z + n);
                                map.setCenter(latLng);
                            });

                            var label = new Label({
                                map: map
                            }, key, item.C);

                            label.bindTo('position', marker, 'position');
                            label.bindTo('visible', marker, 'visible');
                            label.set('text', item.C);
                        }

                        markers.push(marker);
                    });
                },
                error: function (xhr, err) {
                    //alert(mymap.settings.textErrorMessage);
                    alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText);
                }
            });

        },
        getAccessToken: function (username, password) {
            var webMethod = mymap.settings.jsonGetAccessTokenUrl;
            var parameters = '{' + '"username":"' + username + '","password":"' + password + '","sendid":"' + (++async.lastSendGetAccessToken) + '"}';
            $.ajax({
                type: 'POST',
                url: webMethod,
                data: parameters,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                success: function (data) {
                    var items = jQuery.parseJSON(data.d);

                    var lastReceivedGetAccessToken = items.ReplyId;
                    if (lastReceivedGetAccessToken <= async.lastReceivedGetAccessToken) {
                        // async mismatch, this is old reply, dont use it
                        //console.log('async mismatch ' + lastReceivedGetAccessToken + ' ' + async.lastReceivedGetAccessToken);
                        return;
                    }
                    // update
                    async.lastReceivedGetAccessToken = lastReceivedGetAccessToken;

                    var success = items.Success;

                    // update access token
                    mymap.settings.access_token = items.AccessToken;
                },
                error: function (xhr, err) {
                    alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText);
                }

            });
        },

        setType: function (type, isChecked) {
            var webMethod = mymap.settings.jsonSetTypeUrl;
            var parameters = '{' + '"access_token":"' + mymap.settings.access_token + '","type":"' + type + '","isChecked":"' + isChecked + '","sendid":"' + (++async.lastSendSetType) + '"}';
            $.ajax({
                type: 'POST',
                url: webMethod,
                data: parameters,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                success: function (data) {
                    var items = jQuery.parseJSON(data.d);

                    var lastReceivedSetType = items.ReplyId;
                    if (lastReceivedSetType <= async.lastReceivedSetType) {
                        // async mismatch, this is old reply, dont use it
                        //console.log('async mismatch ' + lastReceivedSetType + ' ' + async.lastReceivedSetType);
                        return;
                    }
                    // update
                    async.lastReceivedSetType = lastReceivedSetType;

                    if (items.TokenValid === "0") {
                        alert("Demo time is over, TokenValid is invalid, please relogin");
                        return;
                    }

                    var success = items.Success;

                    // update screen
                    mymap.events.getBounds(map, true);
                },
                error: function (xhr, err) {
                    alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText);
                }

            });
        },

        attachCallOut: function (marker, item) {
            var webMethod = mymap.settings.jsonMarkerDetailUrl;
            var parameters = '{' + '"access_token":"' + mymap.settings.access_token + '","id":"' + item.I + '","type":"' + item.T + '","sendid":"' + (++async.lastSendMarkerDetail) + '"}';

            $.ajax({
                type: 'POST',
                url: webMethod,
                data: parameters,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                success: function (data) {
                    var items = jQuery.parseJSON(data.d);

                    var lastReceivedMarkerDetail = items.ReplyId;
                    if (lastReceivedMarkerDetail <= async.lastReceivedMarkerDetail) {
                        // async mismatch, this is old reply, dont use it
                        //console.log('async mismatch ' + lastReceivedMarkerDetail + ' ' + async.lastReceivedMarkerDetail);
                        return;
                    }
                    // update
                    async.lastReceivedMarkerDetail = lastReceivedMarkerDetail;

                    if (items.TokenValid === "0") {
                        alert("Demo time is over, TokenValid is invalid, please relogin");
                        return;
                    }

                    var success = items.Success;

                    infowindow.setContent(items.Content);
                    infowindow.open(map, marker);
                },
                error: function (xhr, err) {
                    alert("readyState: " + xhr.readyState + "\nstatus: " + xhr.status + "\nresponseText: " + xhr.responseText);
                }

            });
        }
    }
}

// lon, lat, count
var getKey = function (x, y, c) {
    var s = x + "__" + y + "__" + c;
    return s.replace(/\./g, "_"); //replace . with _
}

google.maps.event.addDomListener(window, 'load', mymap.initialize); // load google map
mymap.events.getAccessToken('username', 'password', async.lastSendGetAccessToken); // set access token

var checkboxClicked = function (type, isChecked) {    
    mymap.events.setType(type, isChecked);
}


//COUNT LABELS ON CLUSTERS
var Label = function (opt_options, id, count) {
    this.setValues(opt_options);
    var span = this.span_ = document.createElement('span');

    if (count >= 10000) {
        span.style.cssText = mymap.clusterOptions.styles[4].style;
    }
    else if (count >= 1000) {
        span.style.cssText = mymap.clusterOptions.styles[3].style;
    }
    else if (count >= 100) {
        span.style.cssText = mymap.clusterOptions.styles[2].style;
    }
    else if (count >= 10) {
        span.style.cssText = mymap.clusterOptions.styles[1].style;
    }
    else {
        span.style.cssText = mymap.clusterOptions.styles[0].style;
    }

    var div = this.div_ = document.createElement('div');
    div.appendChild(span);
    div.className = "countinfo_" + id;
    div.style.cssText = 'position: absolute; display: none;';
};

Label.prototype = new google.maps.OverlayView;

Label.prototype.onAdd = function () {
    var pane = this.getPanes().overlayLayer;
    pane.appendChild(this.div_);

    var me = this;
    this.listeners_ = [
        google.maps.event.addListener(this, 'idle',
            function () { me.draw(); }),
        google.maps.event.addListener(this, 'visible_changed',
            function () { me.draw(); }),
        google.maps.event.addListener(this, 'position_changed',
            function () { me.draw(); }),
        google.maps.event.addListener(this, 'text_changed',
            function () { me.draw(); })
    ];
};

Label.prototype.onRemove = function () {
    this.div_.parentNode.removeChild(this.div_);

    for (var i = 0, I = this.listeners_.length; i < I; ++i) {
        google.maps.event.removeListener(this.listeners_[i]);
    }
};

Label.prototype.draw = function () {
    var projection = this.getProjection();
    var position = projection.fromLatLngToDivPixel(this.get('position'));

    var div = this.div_;
    div.style.left = position.x + 'px';
    div.style.top = position.y + 'px';

    var visible = this.get('visible');
    div.style.display = visible ? 'block' : 'none';

    this.span_.innerHTML = this.get('text').toString();
};