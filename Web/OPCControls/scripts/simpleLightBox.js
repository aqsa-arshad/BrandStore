var overDiv = null;

function f_scrollTop()
{
	return f_filterResults(
		window.pageYOffset ? window.pageYOffset : 0,
		document.documentElement ? document.documentElement.scrollTop : 0,
		document.body ? document.body.scrollTop : 0
	);
}
function f_filterResults(n_win, n_docel, n_body)
{
	var n_result = n_win ? n_win : 0;
	if (n_docel && (!n_result || (n_result > n_docel)))
		n_result = n_docel;
	return n_body && (!n_result || (n_result > n_body)) ? n_body : n_result;
}

function hideOverlay()
{
	if (overDiv != null)
	{
		document.body.removeChild(overDiv);
		overDiv.style.display = 'none';
		overDiv = null;
	}
}

function showForm(id, backgroundClass)
{
	overDiv = document.createElement("div");
	overDiv.id = 'overdiv';
	overDiv.className = backgroundClass;

	document.body.appendChild(overDiv);
	
	oDiv = document.getElementById(id);
	var vscroll = f_scrollTop();
	var offset = vscroll + 100;
	if (document.all)
	{
	oDiv.style.top = offset;
	}
	else
	{
		oDiv.style.top = offset.toString() + 'px';
	}
	oDiv.style.display = 'block';
	
	return false;
}

function hideForm(id)
{
	hideOverlay();
	oDiv = document.getElementById(id);
	oDiv.style.display = 'none';
	
	return false;
}

Sys.Application.add_load(function() { hideOverlay(); });