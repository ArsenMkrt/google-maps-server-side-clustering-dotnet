var map;

var searchInfo = {
    searchMarker: null,
    zoomLevel: 13,
    round: 6,
    prefix: 4
};

var geocoder = new google.maps.Geocoder();

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
            maxZoom: 20
        });

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
            draggable: true
        });

        searchInfo.searchMarker.setPosition(new google.maps.LatLng(55.5, 11.8));
        searchInfo.searchMarker.setVisible(true);

        //Add listener to marker for reverse geocoding
        google.maps.event.addListener(searchInfo.searchMarker, 'drag', function () {
            geocoder.geocode({ 'latLng': searchInfo.searchMarker.getPosition() }, function (results, status) {
                if (status === google.maps.GeocoderStatus.OK) {
                    if (results[0]) {
                        $('#search').val(results[0].formatted_address);
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
                    geocoder.geocode({ "address": request.term }, function (results, status) {
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
        mapCenterLat: 55.7,
        mapCenterLon: 12.6,
        zoomLevel: 2
    }
}

google.maps.event.addDomListener(window, 'load', mymap.initialize); // load google map