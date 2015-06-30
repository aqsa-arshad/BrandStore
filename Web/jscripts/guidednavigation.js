var delimeter = "-";

function getQueryStringParameters() {
	var idx = document.URL.indexOf('?');
	var params = new Array();
	if (idx != -1) {
		var pairs = document.URL.substring(idx + 1, document.URL.length).split('&');
		for (var i = 0; i < pairs.length; i++) {
			nameVal = pairs[i].split('=');
			params[nameVal[0]] = nameVal[1];
		}
	}
	return params;
}

function getParamsbyref() {
	paramsbyref = getParamsbyrefString(document.URL);
	return paramsbyref;
}

function getParamsbyrefString(url) {
	var idx = url.indexOf('?');
	var paramsbyref = new Array();
	if (idx != -1) {
		var pairs = url.substring(idx + 1, url.length).split('&');
		for (var i = 0; i < pairs.length; i++) {
			nameVal = pairs[i].split('=');
			paramsbyref[i] = nameVal;
		}
	}
	return paramsbyref;
}

function setParam(id, value) {
	if (value === "") {
		return false;
	}
	paramsbyref = getParamsbyref();
	var url = 'not';
	if (document.URL.indexOf('?') == -1) {
		url = "?" + id + "=" + value;
	}
	else if (document.URL.indexOf("&" + id + "=") != -1 || document.URL.indexOf("?" + id + "=") != -1) {
		url = "?";
		for (var i = 0; i < paramsbyref.length; i++) {
			if (i != 0)
				url += "&";
			url += paramsbyref[i][0] + "=";
			if (paramsbyref[i][0].toLowerCase() != id.toLowerCase())
				url += paramsbyref[i][1];
			else
				url += value;
		}
		if (id.toLowerCase() != 'pagenum') {
			url = removeParamString("pagenum", url);
		}
	}
	else {
		var idx = document.URL.indexOf('?');
		url = "?" + document.URL.substring(idx + 1, document.URL.length) + "&" + id + "=" + value;
		if (id.toLowerCase() != 'pagenum') {
			url = removeParamString("pagenum", url);
		}
	}
	document.location.href = url;
}

function removeParamString(id, instring) {
	if (instring == "")
		paramsbyref = getParamsbyref();
	else
		paramsbyref = getParamsbyrefString(instring);
	var url = "";
	for (var i = 0; i < paramsbyref.length; i++) {
		if (paramsbyref[i][0].toLowerCase() != id.toLowerCase()) {
			if (url.length != 0)
				url += "&";
			else
				url += "?";
			url += paramsbyref[i][0] + "=" + paramsbyref[i][1];
		}
	}
	if (url == "") {
		var idx = document.URL.indexOf('?');
		url = document.URL.substring(0, idx);
	}
	return url;
}

function removeParam(id) {
	var url = removeParamString(id, "");
	document.location.href = url;
}

function guidedNavigationStartOver() {
	var url = removeParamString("section", "");
	url = removeParamString("category", url);
	url = removeParamString("manufacturer", url);
	url = removeParamString("distributor", url);
	url = removeParamString("genre", url);
	url = removeParamString("vector", url);
	if (url.indexOf('search.aspx') === -1) {
		url = removeParamString("searchterm", url);
	}
	document.location.href = url;
}

function sectionLink(sec) { entityLink(sec, "sections"); }    // override for backwards compatability
function entityLink(id, paramname) {
	params = getQueryStringParameters();
	if (params[paramname] != null && params[paramname].match(delimeter + id + delimeter) == delimeter + id + delimeter)
	{ return; }
	if (params[paramname] != null)
		var querystring = params[paramname] + id + delimeter;
	else
		var querystring = delimeter + id + delimeter;
	setParam(paramname, querystring);
}

function sectionReplaceLink(sec, oldsec) { entityReplaceLink(sec, oldsec, "sections"); }//override for backwards compatability
function entityReplaceLink(id, oldid, paramname) {
	params = getQueryStringParameters();
	if (params[paramname] != null && params[paramname].match(delimeter + id + delimeter) == delimeter + id + delimeter)
	{ return; }
	if (params[paramname] != null)
		var querystring = params[paramname].replace(delimeter + oldid + delimeter, delimeter + id + delimeter);
	else
		var querystring = delimeter + id + delimeter;
	setParam(paramname, querystring);
}

function removeSection(id) { removeEntity(id, "sections"); } //override for backwards compatability
function removeEntity(id, paramname) {
	params = getQueryStringParameters();
	var out = removeID(params[paramname], delimeter + id + delimeter);
	if (out == delimeter)
		removeParam(paramname);
	else
		setParam(paramname, out);
}

function removeID(s, t) {
	i = s.indexOf(t);
	r = "";
	if (i == -1) return s;
	r += s.substring(0, i) + delimeter + removeID(s.substring(i + t.length), t);
	return r;
}

function replaceBasePage(newpage) {
	var idx = document.URL.indexOf('?');
	var qs = '';
	if (idx > 0) {
		qs = document.URL.substring(idx, document.URL.length);
	}
	document.location.href = newpage + qs;
}




//Cookie Code
function getCookie(c_name) {
	if (document.cookie.length > 0) {
		c_start = document.cookie.indexOf(c_name + "=")
		if (c_start != -1) {
			c_start = c_start + c_name.length + 1
			c_end = document.cookie.indexOf(";", c_start)
			if (c_end == -1) c_end = document.cookie.length
			return unescape(document.cookie.substring(c_start, c_end))
		}
	}
	return ""
}

function setCookie(c_name, value, expiredays) {
	var exdate = new Date()
	exdate.setDate(exdate.getDate() + expiredays)
	document.cookie = c_name + "=" + escape(value) +
	((expiredays == null) ? "" : ";expires=" + exdate.toGMTString())
}

function checkCookie() {
	username = getCookie('username')
	if (username != null && username != "")
	{ alert('Welcome again ' + username + '!') }
	else
	{
		username = prompt('Please enter your name:', "")
		if (username != null && username != "") {
			setCookie('username', username, 365)
		}
	}
}

function show(sid, name, sectionname) { showEntity(sid, name, sectionname, "sections"); } //override for backwards compatability
function showEntity(id, name, entityname, paramname) {
	if (params[paramname] != null && params[paramname].match(delimeter + id + delimeter) == delimeter + id + delimeter)
	{ selectedArray.push([id, name, entityname]); }
	document.getElementById(entityname + "Header").style.display = "block";
}


var selectedArray = new Array();
var showSelected = false;
params = getQueryStringParameters();
