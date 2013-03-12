

function updateAvatar() {
	avatars = document.getElementById("new_avatar_select");
	avatar = avatars.value;
	
	new_avatar = document.getElementById("new_avatar");
	new_avatar.innerHTML = '<img src="/images/avatars/' + avatar + '-normal.gif" class="avatar" />';
}

function delete_account (account_id, email) {
	alert ('(dummy function)' + account_id + ' ' + email);
	document.getElementById("confirm_delete").innerHTML = "An email has been sent to you containing a link to delete your account.";
}

function updatePasswords(form) {
	var valid = validatePassword(form.new_pword.value, form.new_rpword.value);
	if (valid) {
		form.pword_phash.value = hash(form.pword.value);
		form.new_pword_phash.value = hash(form.new_pword.value);
	}

	form.new_pword.value = "";
	form.new_rpword.value = "";

	return valid;
}