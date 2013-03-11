

function updateAvatar() {
	avatars = document.getElementById("new_avatar_select");
	avatar = avatars.value;
	
	new_avatar = document.getElementById("new_avatar");
	new_avatar.innerHTML = '<img src="/images/avatars/' + avatar + '-normal.gif" class="avatar" />';
}

