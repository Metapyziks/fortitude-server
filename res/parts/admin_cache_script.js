

	window.onload = function () {
		hide ("unowned");
		hide ("admin");
		hide ("special");
		hide ("result");
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
