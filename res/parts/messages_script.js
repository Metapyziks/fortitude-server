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

function colourMessages() {
    var _colours = ["#3c3c3c", "#303030"];

    var table = document.getElementById("msg_table");
    var count = 0;
    for (var i = 1, row1; row1 = table.rows[i]; i += 2) {
        if (row1.style.display != "none") {
            var row2 = table.rows[i+1];
            row1.style.background = _colours[count % _colours.length];
            row2.style.background = row1.style.background;
            ++ count;
        }
    }
}

function toggleMessage(messageid) {
    var contentElement = document.getElementById("msg_" + messageid);
    var buttonsElement = document.getElementById("buttons_" + messageid);
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
        contentElement.style.display = buttonsElement.style.display = "inline";
    } else {
        contentElement.style.display = buttonsElement.style.display = "none";        
    }
}

function deleteMessage(messageid) {
    var row1Element = document.getElementById("row1_" + messageid);
    var row2Element = document.getElementById("row2_" + messageid);

    row1Element.style.display = "none";
    row2Element.style.display = "none";

    new Ajax.Request("/api/deletemessage", {
        method : "get",
        parameters : {
            uid : getCookie("auth-uid"),
            session : getCookie("auth-session"),
            messageid : messageid
        }
    });

    colourMessages();
}

window.onload = function() {
    colourMessages();
}
