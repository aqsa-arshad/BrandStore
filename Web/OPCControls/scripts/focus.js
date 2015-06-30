function setFocus(eleId)
{
    var focusElement = document.getElementById(eleId);
    if (focusElement == null || focusElement === undefined)
    {
        return;
    }
    
    if (focusElement.disabled == false)
    {
        focusElement.focus();
    }
}

function getActivePanel()
{
	var activePanels = adnsf$(".active");
    if (activePanels.length > 0)
    {
        return activePanels[0];
    }

    return null;
}

function setFirstInputFocus()
{
    var activePanel = getActivePanel();
    var inputElements = activePanel.getElementsByTagName('input');
    if (inputElements.length > 0)
    {
        for (var i = 0; i < inputElements.length; i++)
        {
            var focusElement = document.getElementById(inputElements[i].id);            
            if (focusElement != null && isVisible(focusElement) && (focusElement.disabled == false))
            {
                focusElement.focus();
                return;
            }
        }
    }    
}

function scrollToCurrentPanel()
{
    var activePanel = getActivePanel();
    if (activePanel != null && activePanel.id.indexOf('PanelSubmit') < 0)
		adnsf$('html, body').animate({ scrollTop: adnsf$(activePanel).offset().top - 100 }, 500);
}

function isVisible(obj)
{
	if (obj == document)
		return true;

	if (!obj)
		return false;

	if (!obj.parentNode)
		return false;

    if (obj.style)
    {
    	if (obj.style.display == 'none')
    		return false;
    	if (obj.style.visibility == 'hidden')
    		return false;
    }

    //Try the computed style in a standard way
    if (window.getComputedStyle)
    {
    	var style = window.getComputedStyle(obj, "");

        if (style.display == 'none')
        	return false;

        if (style.visibility == 'hidden')
        	return false;
    }

    //Or get the computed style using IE's silly proprietary way
    var style = obj.currentStyle;
    if (style)
    {
    	if (style['display'] == 'none')
    		return false;

    	if (style['visibility'] == 'hidden')
    		return false;
    }

    return isVisible(obj.parentNode);
}
