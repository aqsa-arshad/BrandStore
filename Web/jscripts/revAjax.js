function makeHttpRequest(url, element, calltype) {
  var http_request = false;
  if (window.XMLHttpRequest) { // Mozilla, Safari,...
    http_request = new XMLHttpRequest();
    if (http_request.overrideMimeType) {
      http_request.overrideMimeType('text/xml');
    }
  } else if (window.ActiveXObject) { // IE
    try {
      http_request = new ActiveXObject("Msxml2.XMLHTTP");
    } catch (e) {
      try {
        http_request = new ActiveXObject("Microsoft.XMLHTTP");
      } catch (e) {}
    }
  }
  if (!http_request) {
    alert('Browser doesn\'t support Ajax. Site will NOT FULLY function properly.');
    return false;
  }
  http_request.onreadystatechange = function() {
    if (http_request.readyState == 4) {
      if (http_request.status == 200) {
        loadXML(http_request.responseXML,calltype);
      } else {
        alert('There was a problem with the request. (Code: ' + http_request.status + ')');
      }
    }
  }
  http_request.open('GET', url, true);
  http_request.send(null);
}

function loadXML(xml,calltype)
{
	if(calltype == 'shipping')
	{
		var string = '';
		var root = xml.getElementsByTagName('Shipping')[0];
		for (i = 0; i < root.childNodes.length; i++)
		{
    		var node = root.childNodes[i].tagName;
		    string += root.getElementsByTagName(node)[0].childNodes[0].nodeValue + "";
		}
		if (document.getElementById('ShipQuote'))
		{
			document.getElementById('ShipQuote').innerHTML = string;
		}
	}
	if(calltype == 'pricing')
	{
		var prnode = xml.getElementsByTagName('PriceHTML')[0];
		var variantnode = xml.getElementsByTagName('VariantID')[0];
		var NewPrice = "Not Found";
		var VariantID = "0";
		if(prnode != undefined)
		{
			NewPrice = xml.getElementsByTagName('PriceHTML')[0].firstChild.data
		}
		if(variantnode != undefined)
		{
			VariantID = xml.getElementsByTagName('VariantID')[0].firstChild.data
		}
		//alert("VariantID=" + VariantID + ", NewPrice=" + NewPrice);
		if (document.getElementById('VariantPrice_' + VariantID))
		{
			document.getElementById('VariantPrice_' + VariantID).innerHTML = NewPrice;
		}
	}
}

function getShipping()
{
	if(document.getElementById('Quantity') == undefined || document.getElementById('VariantID') == undefined)
	{
		return;
	}
	var VariantID = document.getElementById('VariantID');
	var Quantity = document.getElementById('Quantity');
  if(Quantity == '')
  {
   Quantity = '1';
  }
  var Country = '';
  if(document.getElementById('Country').length > 0)
  {
	  Country = document.getElementById('Country').options[document.getElementById('Country').selectedIndex].value;
  }
  else
  {
	  Country = document.getElementById('Country').value;
  }
  var State = '';
  if(document.getElementById('State').length > 0)
  {
	  State = document.getElementById('State').options[document.getElementById('State').selectedIndex].value;
  }
  else
  {
	  State = document.getElementById('State').value;
  }
  var PostalCode = document.getElementById('PostalCode');
  
  if (Country.length > 0) {
    if (State.length > 0) {
      if (PostalCode.value.length > 4) {
        if (Quantity.value > 0) {
          Cookies.create('countrycookie',Country,99);
          Cookies.create('statecookie',State,99);
          Cookies.create('postalcookie',PostalCode.value,99);
          var url = "ajaxShipping.aspx?VariantID="+VariantID.value+"&Quantity="+Quantity.value+"&Country="+escape(Country)+"&State="+escape(State)+"&PostalCode="+escape(PostalCode.value);
          //alert(url);
          makeHttpRequest(url,undefined,'shipping');
        } else {
          Cookies.erase('countrycookie');
          Cookies.erase('statecookie');
          Cookies.erase('postalcookie');
          Error('qty');
        }
      } else {
        Cookies.erase('countrycookie');
        Cookies.erase('statecookie');
        Cookies.erase('postalcookie');
        Error('postal');
      }
    } else {
      Cookies.erase('countrycookie');
      Cookies.erase('statecookie');
      Cookies.erase('postalcookie');
      Error('state');
    }
  } else {
    Cookies.erase('countrycookie');
    Cookies.erase('statecookie');
    Cookies.erase('postalcookie');
    Error('country');
  }
}


function getPricing(ProductID,VariantID)
{
	//alert('VariantID=' + VariantID);
	if(ProductID == undefined || VariantID == undefined)
	{
		return;
	}

	var ChosenSize = "";
	//var ChosenSizeList = document.getElementById('Size');
	var ChosenSizeList = document.getElementById('AddToCartForm_' + ProductID + '_' + VariantID).Size;
	if(ChosenSizeList != undefined)
	{
		ChosenSize = ChosenSizeList.options[ChosenSizeList.selectedIndex].text;
	}

	var ChosenColor = "";
	//var ChosenColorList = document.getElementById('Color');
	var ChosenColorList = document.getElementById('AddToCartForm_' + ProductID + '_' + VariantID).Color
	if(ChosenColorList != undefined)
	{
		ChosenColor = ChosenColorList.options[ChosenColorList.selectedIndex].text;
	}

    var url = "ajaxPricing.aspx?ProductID=" + ProductID + "&VariantID=" + VariantID + "&size=" + escape(ChosenSize) + "&color=" + escape(ChosenColor);

    //alert("Ajax Url=" + url);
    makeHttpRequest(url,undefined,'pricing');
}


function Error(type) {
  if (type == 'country') {
    document.getElementById('ShipQuote').innerHTML = "Select A Country";
  }
  if (type == 'state') {
    document.getElementById('ShipQuote').innerHTML = "Select A State";
  }
  if (type == 'postal') {
    document.getElementById('ShipQuote').innerHTML = "Enter Postal Code";
  }
  if (type == 'qty') {
    document.getElementById('ShipQuote').innerHTML = "Enter A Quantity";
  }
}

var Cookies = {
  init: function () {
    var allCookies = document.cookie.split('; ');
    for (var i=0;i<allCookies.length;i++) {
      var cookiePair = allCookies[i].split('=');
      this[cookiePair[0]] = cookiePair[1];
    }
  },
  create: function (name,value,days) {
    if (days) {
      var date = new Date();
      date.setTime(date.getTime()+(days*24*60*60*1000));
      var expires = "; expires="+date.toGMTString();
    }
    else var expires = "";
    document.cookie = name+"="+value+expires+"; path=/";
    this[name] = value;
  },
  erase: function (name) {
    this.create(name,'',-1);
    this[name] = undefined;
  }
};
Cookies.init();

window.onload=function readCookies() {
  if (!document.getElementById) return false;
  var countrycookie = Cookies['countrycookie'];
  var statecookie = Cookies['statecookie'];
  var postalcookie = Cookies['postalcookie'];
  if (countrycookie) {
    if (statecookie) {
      if (postalcookie) {
        if (document.getElementById('Country') != null) {
          document.getElementById('Country').value = Cookies['countrycookie'];
          if (document.getElementById('State') != null) {
            document.getElementById('State').value = Cookies['statecookie'];
            if (document.getElementById('PostalCode') != null) {
              document.getElementById('PostalCode').value = Cookies['postalcookie'];
              if (document.getElementById('VariantID') != null) {
                if (document.getElementById('Quantity') != null) {
                  getShipping();
                }
              }
            }
          }
        }
      }
    }
  }
  // Set Focus to SearchBox
  if (document.topsearchform.SearchTerm) {
     document.topsearchform.SearchTerm.focus();
  }
}