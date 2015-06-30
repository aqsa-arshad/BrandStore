/*
DezinerFolio.com Simple Accordians.

Author  : G.S.Navin Raj Kumar
Website : http://dezinerfolio.com

*/

/*
* The Variable names have been compressed to achive a higher level of compression.
*/

// Prototype Method to get the element based on ID

function gebid(d){
	return document.getElementById(d);
}

// set or get the current display style of the div
function dsp(d,v){
	if(v==undefined){
		return d.style.display;
	}else{
		d.style.display=v;
	}
}

// set or get the height of a div.
function sh(d,v){
    // if you are getting the height then display must be block to return the absolute height
    if (v == undefined) {
	    if (dsp(d) != 'none' && dsp(d) != '') {
			return d.offsetHeight;
		}
		viz = d.style.visibility;
		d.style.visibility = 'hidden';
		o = dsp(d);
		dsp(d,'block');
		r = parseInt(d.offsetHeight);
		dsp(d,o);
		d.style.visibility = viz;
		return r;
	}else{
		d.style.height=v;
	}
}
/*
* Variable 'S' defines the speed of the accordian
* Variable 'T' defines the refresh rate of the accordian
*/

s=33;
t=1;

//Collapse Timer is triggered as a setInterval to reduce the height of the div exponentially.
function ct(d, l){
    d = gebid(d);
    if (l == undefined) {
        l = 0;
    }
    if (d.maxh - l * d.s <= 0) {
        d.style.height = "0px";
        dsp(d, 'none');
    }
	else if (d.offsetHeight > 0) {
	    var remainder = sh(d);
	    v = (remainder < d.s) ? remainder : d.s;
		v = (d.offsetHeight-v);
		sh(d, v + 'px');
		setTimeout("ct('" + d.id + "', " + (l + 1) + ")", d.t);
	}else{
		dsp(d,'none');
	}
}

//Expand Timer is triggered as a setInterval to increase the height of the div exponentially.
function et(d, l) {
    d = gebid(d);
    if (l == undefined) {
        l = 0;
    }
    if (l * d.s >= d.maxh) {
        d.style.height = d.maxh + "px";
    }
    else if (d.offsetHeight < d.maxh) {
        var remainder = d.maxh - sh(d);
        v = (remainder > d.s) ? d.s : remainder;
        v = (d.offsetHeight + v);
        d.style.height = v + 'px';
        setTimeout("et('" + d.id + "', "+(l + 1)+")", d.t);
    }
}

// Collapse Initializer
function cl(d){
	if(dsp(d)=='block'){
		//clearInterval(d.t);
		//d.t = setInterval('ct("' + d.id + '")', t);
		ct(d.id);
	}
}

//Expand Initializer
function ex(d){
	if(dsp(d)=='none'){
	    dsp(d, 'block');
		d.style.height='0px';
		et(d.id);
    }
}

// Removes Classname from the given div.
function cc(n,v){
	s=n.className.split(/\s+/);
	for(p=0;p<s.length;p++){
		if(s[p]==v+n.tc){
			s.splice(p,1);
			n.className=s.join(' ');
			break;
		}
	}
}
//Accordian Initializer
function Accordian(d, sl, tc, dir) {
    pv = d;
    if (dir == undefined) {
        dir = "vertical";
    }
    // get all the elements that have id as content
	l=gebid(d).getElementsByTagName('div');
	c=[];
	for(i=0;i<l.length;i++){
		h=l[i].id;
		if(h.substr(h.indexOf('-')+1,h.length)=='content'){c.push(h);}
	}
	sel=null;
	//then search through headers
	for(i=0;i<l.length;i++){
		h=l[i].id;
		if(h.substr(h.indexOf('-')+1,h.length)=='header'){
		    d = gebid(h.substr(0, h.indexOf('-')) + '-content');
		    h = gebid(h);
		    h.tc = tc;
		    h.c = c;
		    d.style.display = 'none';
		    d.style.overflow = 'hidden';
		    d.maxh = sh(d);
		    d.s = (s == undefined) ? 7 : s;
		    if (h.className.match(/selected+/) != undefined) {
		        sel = h;
		        h.className = h.className + ' ' +  h.tc;
		        d.style.display = 'block';
		     }
		    
			// set the onclick function for each header.
			h.onclick = function(){
				for(i=0;i<this.c.length;i++){
					cn=this.c[i];//accordion node
					n = cn.substr(0, cn.indexOf('-')); //accordion node identifier before "-"
					if ((n + '-header') == this.id && (this.className.match(/highlight/) == undefined || dir == "horizontal")) {
						ex(gebid(n+'-content'));
						n=gebid(n+'-header');
						cc(n,'__');
						n.className=n.className+' '+n.tc;
					}else{
						cl(gebid(n+'-content'));
						cc(gebid(n+'-header'),'');
					}
				}
            }
		}
	}
//	if (sel != undefined) {
//	    addLoadEvent(function() {
//	        //sel.onclick(); removed for blackberry support
//	        setTimeout("gebid('"+sel.id+"').onclick()", 700);
//	    });
//	}
	
}

function popDown(el, display, speed) {
    el.t = t;
    el.s = (speed == undefined) ? s : speed;
    el.maxh = sh(el);
    el.style.display = 'none';
    el.style.overflow = 'hidden';
    this.el = el;
    if (display == undefined || !display) {
        el.style.display = "none";
    }
    this.toggle = togglePop;

}

function togglePop(targetDisplay) {
    el = this.el;
    if (targetDisplay == undefined)
        targetDisplay = "";
    if (el.style.display == undefined || el.style.display == "") {
        el.style.display = "block";
    }
    
    if (el.style.display == "block" && targetDisplay != "show") {
        cl(el);
    }
    else if (targetDisplay != "hide"){
        el.style.display == "none";
        ex(el);
    }
}

var onloads = new Array();
var searchPopDown;
function addLoadEvent(func) {
    onloads.push(func);
}

function onLoadFunction() {
    try { setTimeout(function() { window.scrollTo(0, 1) }, 100); } catch (err) { }
    for (var i = 0; i < onloads.length; i++) {
        onloads[i].apply();
    }
}

function sliderright(accordionid) {
    if (gebid(accordionid) != undefined) {
        var d = accordionid;
        l = gebid(d).getElementsByTagName('div');
        var current = 0;
        var headercount = 0;
        for (i = 0; i < l.length; i++) {
            h = l[i].id;
            if (h.substr(h.indexOf('-') + 1, h.length) == 'header') {
                if (gebid(h).className.match(/highlight/) != undefined) {
                    current = i;
                }
                headercount++;
            }
        }
        var next = 0;
        if (current != headercount - 1) {
            next = current + 1;
        }
        l[next].onclick();
    }
}

function sliderleft(accordionid) {
    if (gebid(accordionid) != undefined) {
        var d = accordionid;
        l = gebid(d).getElementsByTagName('div');
        var current = 0;
        var headercount = 0;
        for (i = 0; i < l.length; i++) {
            h = l[i].id;
            if (h.substr(h.indexOf('-') + 1, h.length) == 'header') {
                if (gebid(h).className.match(/highlight/) != undefined) {
                    current = i;
                }
                headercount++;
            }
        }
        var next = headercount - 1;
        if (current != 0) {
            next = current - 1;
        }
        l[next].onclick();
    }
}



//storefront function fix for Blackberry
submitonce = function(theform) {
    if (document.all || document.getElementById) {
        for (i = 0; i < theform.length; i++) {
            var tempobj = theform.elements[i];
            if (tempobj.type != undefined && (tempobj.type.toLowerCase() == "submit" || tempobj.type.toLowerCase() == "reset"))
                tempobj.disabled = true;
        }
    }
}

submitenabled = function(theform) {
    if (document.all || document.getElementById) {
        for (i = 0; i < theform.length; i++) {
            var tempobj = theform.elements[i];
            if (tempobj.type != undefined && (tempobj.type.toLowerCase() == "submit" || tempobj.type.toLowerCase() == "reset"))
                tempobj.disabled = false;
        }
    }
}
addLoadEvent(function() {
    try {
        var ceditpanel = document.getElementById('ctl00_PageContent_ctrlCreditCardPanel');
        if (creditpanel != null) {
            var credittables = creditpanel.getElementsByTagName('TABLE');
            for (var i = 0; i < credittables.length; i++) {
                credittables[i].style.width = '';
            }
        }
    } catch (err) { }
    try {
        var inputs = document.getElementsByTagName("INPUT");
        for (var i = 0; i < inputs.length; i++) {
            if (inputs[i].type == 'text') {
                inputs[i].style.width = '';
            }
        }
    } catch (err) { }
    try {
        if (document.location.href.indexOf("/t-") != -1) {
            var content = document.getElementById("ctl00_PageContent_pnlContent");
            var uls = content.getElementsByTagName("ul");
            if (uls.length == 0) {
                content.style.padding = "5px";
            }
        }
    } catch (err) { }
});


