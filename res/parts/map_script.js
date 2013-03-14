var alliedCacheIcon = 'http://www.google.com/intl/en_us/mapfiles/ms/micons/blue.png';
	var enemyCacheIcon = 'http://www.google.com/intl/en_us/mapfiles/ms/micons/red.png';
	var unownedCacheIcon = 'http://www.google.com/intl/en_us/mapfiles/ms/micons/yellow.png';
	var adminCacheIcon = 'http://www.google.com/intl/en_us/mapfiles/ms/micons/green.png';	

	var mapCaches = new Array();
        var map;

	function initialize() 
	{
		var mapOptions = {
		  	center: new google.maps.LatLng(54.778599, -1.570315),
			zoom: 14,
			mapTypeId: google.maps.MapTypeId.TERRAIN
		};
		
		map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);
	}

	function clearMap ()
	{
		while(mapCaches.length != 0)
		{
			mapCaches.pop().remove();
		}
	}

	function processCoords(loc)
	{
		map.setCenter (loc);
	}

	function get_coords(address)
	{
	    var gc      = new google.maps.Geocoder(),
	        opts    = { 'address' : address };

	    gc.geocode(opts, function (results, status)
	    {
	        if (status == google.maps.GeocoderStatus.OK)
	        {   
	            alert(results[0].geometry.location);
	            processCoords(results[0].geometry.location);

	        }
	        else
	        {
	            document.getElementById("page_response").innerHTML = "<ul class='news'><li><p>We could not find that location. Please try again.</p></li></ul>";
	        }
	    });
	}

        var infoWindowOpen = false;
	var openInfoWindow = false;

        function addInfoWindow(marker, content)
        {
            content.style.color = "black";
	    var infowindow = new google.maps.InfoWindow({
	        content: content
	    });
	    google.maps.event.addListener(marker, "click", function() {
	        if(infoWindowOpen == true)
	        {
		    openInfoWindow.close();
	        }
	        infoWindowOpen = true;
	        infowindow.open(map, marker);
	        openInfoWindow = infowindow;
	    });
	    google.maps.event.addListener(infowindow, "closeclick", function() {
		infowindow.close();
		infoWindowOpen = false;
	    });
	}

	function addAlliedMarker (latitude, longitude, name, content) 
	{
	    var marker = new google.maps.Marker({
	        position: new google.maps.LatLng(latitude,longitude),
	        map: map,
		title: name,
		icon: alliedCacheIcon
	    });
	    addInfoWindow(marker, content);
	    mapCaches.push(marker);
	}

        function addEnemyMarker (latitude, longitude, name, content) 
	{
	    var marker = new google.maps.Marker({
	        position: new google.maps.LatLng(latitude,longitude),
		map: map,
		title: name,
		icon: enemyCacheIcon
	    });
	    addInfoWindow(marker, content);
	    mapCaches.push(marker);
	}

        function addUnownedMarker (latitude, longitude, name, content) 
	{
	    var marker = new google.maps.Marker({
	        position: new google.maps.LatLng(latitude,longitude),
		map: map,
		title: name,
		icon: unownedCacheIcon
	    });
	    addInfoWindow(marker, content);
	    mapCaches.push(marker);
	}

        function addAdminMarker (latitude, longitude, name, content) 
	{
  	    var marker = new google.maps.Marker({
	        position: new google.maps.LatLng(latitude,longitude),
		map: map,
		title: name,
		icon: adminCacheIcon
	    });
	    addInfoWindow(marker, content);
	    mapCaches.push(marker);
	}
