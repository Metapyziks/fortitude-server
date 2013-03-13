if (typeof String.prototype.endsWith !== 'function') {
    String.prototype.endsWith = function(suffix) {
        return this.indexOf(suffix, this.length - suffix.length) !== -1;
    };
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

function toggleMessage(messageid) {
    var contentElement = document.getElementById("msg_" + messageid);
    var iconElement = document.getElementById("icon_" + messageid);
    if (contentElement.style.display == "none") {
        if (!iconElement.src.endsWith("_read.gif")) {
            iconElement.src = "/images/news_envelope_read.gif";
            new Ajax.Request("/api/readmessage", {
                method : "get",
                parameters : {
                    uid : getCookie("auth-uid"),
                    session : getCookie("auth-session"),
                    messageid : messageid
                }
            });
        }
        contentElement.style.display = "inline";
    } else {
        contentElement.style.display = "none";        
    }
}
