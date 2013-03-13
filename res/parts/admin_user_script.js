

	window.onload = function () {
		hide ("owner");
		hide ("admin");
		hide ("unverified");
		hide ("result");
	}

	function showTab (id) {
		hide ("owner");
		hide ("admin");
		hide ("verified");
		hide ("unverified");
		hide ("result");

		show (id);
	}

	function show (id) {
		document.getElementById(id).style.display = 'block';
	}

	function hide (id) {
		document.getElementById(id).style.display = 'none';
	}
