function matches(string, regexp)
{
	return (string.match(regexp) || []).length > 0;
}

function validatePassword(first, second)
{
	alert ("Inside validate password");
	if (first.length < 5) {
		alert("Password must be at least 5 characters long!");
		return false;
	}
	if (first.length > 16) {
		alert("Password must be no longer than 16 characters!");
		return false;
	}
	if (!matches(first, /[0-9]/) || !matches(first, /[a-z]/i)) {
		alert("Password must containt at least one number and one letter!");
		return false;
	}
	if (first != second) {
		alert("Both passwords must match!");
		return false;
	}
	return true;
}

function hash(string)
{
    return CryptoJS.MD5("7¬`j8,#@ytIsQ9$Od" + string + "%fI4\"0lPQz^~U&An4:9£k").toString();
}

function submitPassword(form)
{
	var valid = validatePassword(form.reg_pword.value, form.reg_rpword.value);
	if (valid) form.reg_phash.value = hash(form.reg_pword.value);

	form.reg_pword.value = "";
	form.reg_rpword.value = "";

	return valid;
}


