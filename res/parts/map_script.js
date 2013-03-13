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
	            processCoords(results[0].geometry.location);
	        }
	        else
	        {
	            document.getElementById("page_response").innerHTML = "<ul class='news'><li><p>We could not find that location. Please try again.</p></li></ul>";
	        }
	    });
	}

	function addAlliedMarker (latitude, longitude, name) 
	{
		mapCaches.push(new google.maps.Marker({
			  position: new google.maps.LatLng(latitude,longitude),
			  map: map,
			  title: name,
			  icon: alliedCacheIcon
			}));
	}

        function addEnemyMarker (latitude, longitude, name) 
	{
		mapCaches.push(new google.maps.Marker({
			  position: new google.maps.LatLng(latitude,longitude),
			  map: map,
			  title: name,
			  icon: enemyCacheIcon
			}));
	}

        function addUnownedMarker (latitude, longitude, name) 
	{
		mapCaches.push(new google.maps.Marker({
			  position: new google.maps.LatLng(latitude,longitude),
			  map: map,
			  title: name,
			  icon: unownedCacheIcon
			}));
	}

        function addAdminMarker (latitude, longitude, name) 
	{
		mapCaches.push(new google.maps.Marker({
			  position: new google.maps.LatLng(latitude,longitude),
			  map: map,
			  title: name,
			  icon: adminCacheIcon
			}));
	}
