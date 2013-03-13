	
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

		return map;
	}

	function addMarker (latitude, longitude, name, map) 
	{
		mapCaches.push(new google.maps.Marker({
			  position: new google.maps.LatLng(latitude,longitude),
			  map: map,
			  title: name
			}));
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