function initialize() {
			var mapOptions = {
			  center: new google.maps.LatLng(54.778599, -1.570315),
			  zoom: 14,
			  mapTypeId: google.maps.MapTypeId.TERRAIN
			};
			var map = new google.maps.Map(document.getElementById("map_canvas"),
			  mapOptions);
			var mapCaches = new Array();
	<?
	  foreach (var cache in allCaches) {
	?>
			mapCaches.push(new google.maps.Marker({
			  position: new google.maps.LatLng({$cache.Latitude},{$cache.Longitude}),
			  map: map,
			  title:"{$cache.Name}"
			}));
	<?
	  }
	  // End for each var cache in cacheas
	?>
		  }
		  
		  window.onload = initialize();