

	window.onload = function () {
		hide ("owner");
		hide ("admin");
		hide ("unverified");
	}

	function showTab (id) {
		hide ("owner");
		hide ("admin");
		hide ("verified");
		hide ("unverified");

		show (id);
	}

	function show (id) {
		document.getElementById(id).style.display = 'block';
	}

	function hide (id) {
		document.getElementById(id).style.display = 'none';
	}
