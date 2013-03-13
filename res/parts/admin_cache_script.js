

	window.onload = function () {
		hide ("unowned");
		hide ("admin");
		hide ("result");
	}

	function showTab (id) {
		hide ("owned");
		hide ("unowned");
		hide ("admin");
		hide ("result");

		show (id);
	}

	function show (id) {
		document.getElementById(id).style.display = 'block';
	}

	function hide (id) {
		document.getElementById(id).style.display = 'none';
	}
