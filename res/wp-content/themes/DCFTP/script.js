	
	
	/*	Directory page only. The map controls setting the location and zooming in or out.
		The zoom and location are stored in global variables so that if the map location is
		changed it can remain at the same zoom as before, and if the map zoom is changed
		the location aslo remains the same. */

	/*	Initial/default values - required as any map modifier function uses them, and they
		may not yet have been set by the user. */
		
	var zoom = 14;
	var mapCenter = "saddler street durham"; // Identify English Durham not American.
	
	getMap = function() {
		/*	Get map of location specified by text input. */
		mapCenter = document.getElementById("location").value;
		if (mapCenter == "" || mapCenter == "durham") {
			mapCenter = "saddler street durham"; // Default map location if none specified.
		} 
		m = document.getElementById("map");
		m.innerHTML = "<img src = \"http://maps.googleapis.com/maps/api/staticmap?center=" + mapCenter + "&zoom=" + zoom + "&size=600x300&maptype=roadmap&sensor=false\">";
	}
	
	zoomIn = function() {
		// Check if zoom is below the maximum (22) and if so, increase.
		if (zoom < 22) {
			zoom = zoom + 1;
			m = document.getElementById("map");
			m.innerHTML = "<img src = \"http://maps.googleapis.com/maps/api/staticmap?center=" + mapCenter + "&zoom=" + zoom + "&size=600x300&maptype=roadmap&sensor=false\">";
		}
	}

	zoomOut = function() {
		// Check if zoom is above the minimum (0) and if so, decrease.
		if (zoom > 0) {
			zoom = zoom - 1;
			m = document.getElementById("map");
			m.innerHTML = "<img src = \"http://maps.googleapis.com/maps/api/staticmap?center=" + mapCenter + "&zoom=" + zoom + "&size=600x300&maptype=roadmap&sensor=false\">";
		}
	}