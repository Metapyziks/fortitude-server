

	window.onload = function () {
		hide ("unowned");
		hide ("admin");
		hide ("special");
		hide ("result");

		styleTable ("tspecial", 1);
		styleTable ("tadmin", 1);
		styleTable ("towned", 1);
		styleTable ("tunowned", 1);
	}

	function showTab (id) {
		hide ("owned");
		hide ("unowned");
		hide ("admin");
		hide ("special");
		hide ("result");

		show (id);
	}

	function show (id) {
		document.getElementById(id).style.display = 'block';
	}

	function hide (id) {
		document.getElementById(id).style.display = 'none';
	}


	function deleteCache(cacheid) {
		if (!confirm('Delete this cache? Action cannot be reversed.')) {
			return;
		}
		document.getElementById("row_" + cacheid).style.display = "none";

	    new Ajax.Request("/api/deletecache", {
	        method : "get",
	        parameters : {
	            uid : getCookie("auth-uid"),
	            session : getCookie("auth-session"),
	            cacheid : cacheid
	        }
	    });
	}
