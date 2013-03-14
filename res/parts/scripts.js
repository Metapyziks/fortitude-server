
function matches(string, regexp)
{
	return (string.match(regexp) || []).length > 0;
}

function validatePassword(first, second)
{
	if (first.length < 5) {
		alert("Password must be at least 5 characters long!");
		return false;
	}
	if (first.length > 16) {
		alert("Password must be no longer than 16 characters!");
		return false;
	}
	if (!matches(first, /[0-9]/) || !matches(first, /[a-z]/i)) {
		alert("Password must contain at least one number and one letter!");
		return false;
	}
	if (first != second) {
		alert("Both passwords must match!");
		return false;
	}
	return true;
}

function validateEmail(first, second)
{
	if (first != second) {
		alert("Both Emails must match!");
		return false;
	}
	return true;
}

function hex2a(hex) {
    var str = '';
    for (var i = 0; i < hex.length; i += 2)
        str += String.fromCharCode(parseInt(hex.substr(i, 2), 16));
    return str;
}

function hash(string)
{
	var temp0 = "37ac606a382c2340797449735139244f64";
	var temp1 = "2566493422306c50517a5e7e5526416e343a39a36b";
    return CryptoJS.MD5(hex2a(temp0) + string + hex2a(temp1)).toString();
}

function submitPassword(form)
{
    if (!validatePassword(form.reg_pword.value, form.reg_rpword.value)) {
        return false;
	}

	if (!validateEmail(form.reg_email.value, form.reg_remail.value)) {
        return false;
    }

    form.reg_phash.value = hash(form.reg_pword.value);

	form.reg_pword.value = "";
	form.reg_rpword.value = "";

	return true;
}

function getCookie(c_name) {
    var i,x,y,ARRcookies = document.cookie.split(";").join(",").split(",");
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0,ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=")+1);
        x = x.replace(/^\s+|\s+$/g,"");
        if ( x == c_name ) {
            return unescape(y);
        }
    }
}

function styleTable(tableId, startFrom) {
    var _colours = ["#3c3c3c", "#303030"];
    
    var table = document.getElementById(tableId);
    var count = 0;
    for (var i = startFrom, row1; row1 = table.rows[i]; ++ i) {
        if (row1.style.display != "none") {
            row1.style.background = _colours[count % _colours.length];
            ++ count;
        }
    }
}
