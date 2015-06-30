var lastKeyPressed = -1;

function getCardType(number)
{
	var re = new RegExp("^4");
	if(number.match(re) != null)
	{
	    return "Visa";
	}

	re = new RegExp("^(34|37)");
	if(number.match(re) != null)
	{
	    return "AmericanExpress";
	}

	re = new RegExp("^(6334|6767)");
	if (number.match(re) != null) 
	{
	    return "Solo";
	}

	re = new RegExp("^5[1-5]");
	if(number.match(re) != null)
	{
	    return "MasterCard";
	}

	re = new RegExp("^(5018|5020|5038|6304|6759|6761|6763)");
	if (number.match(re) != null) {
	    return "Maestro";
	}

	re = new RegExp("^6011");
	if(number.match(re) != null)
	{
	    return "Discover";
	}

	return "";
}

function setCreditCardType(e, ccTypeId,
    visaImageId, mcImageId, discImageId, amexImageId, soloImageId, maestroImageId,
    issueBoxId, securityCodeId, ccNumberId)
{
    var ccNumber = e.target;
    if (ccNumber === undefined)
    {
        ccNumber = e.srcElement;
    }

    if (ccNumber.value.indexOf("****") >= 0)
        return;

    // detect backspace, and do not process event in that case    
    if (lastKeyPressed == 8 || lastKeyPressed == 46)
        return;

    setElementOpacity(visaImageId, 'opacity25');
    setElementOpacity(mcImageId, 'opacity25');
    setElementOpacity(discImageId, 'opacity25');
    setElementOpacity(amexImageId, 'opacity25');
    setElementOpacity(soloImageId, 'opacity25');
    setElementOpacity(maestroImageId, 'opacity25');
    disableIssueBox(issueBoxId, true);

	if(ccNumber.value.length >= 4)
	{
		var cardType = getCardType(ccNumber.value);		
		var cardTypeHidden = document.getElementById(ccTypeId);		
		if ((cardTypeHidden != null) && (typeof (cardTypeHidden) != 'undefined'))
		{
		    cardTypeHidden.value = cardType;
		}

		var securityCode = document.getElementById(securityCodeId);
		securityCode.setAttribute('maxlength', 3);

		ccNumber.setAttribute('maxlength', 19);

		var cardTypeVisible = false;
		if (cardType == "Visa")
		{
		    cardTypeVisible = setElementOpacity(visaImageId, 'opacity100');
		}
		else if (cardType == "MasterCard")
		{
		    cardTypeVisible = setElementOpacity(mcImageId, 'opacity100');
		}
		else if (cardType == "AmericanExpress")
		{
		    cardTypeVisible = setElementOpacity(amexImageId, 'opacity100');
		    securityCode.setAttribute('maxlength', 4);
		    ccNumber.setAttribute('maxlength', 17);
		}
		else if (cardType == "Solo")
		{
		    cardTypeVisible = setElementOpacity(soloImageId, 'opacity100');
		}
		else if (cardType == "Discover")
		{
		    cardTypeVisible = setElementOpacity(discImageId, 'opacity100');
		}
		else if (cardType == "Maestro")
		{
		    cardTypeVisible = setElementOpacity(maestroImageId, 'opacity100');
		    disableIssueBox(issueBoxId, false);
		}

		var ccNumberFormatted = getFormattedNumber(ccNumber.value, cardType);		
		ccNumber.value = ccNumberFormatted;
		ccNumber.focus();

        // if card type image is not visible, then the card type isn't available (has been removed)
		if (cardTypeVisible == false)
		{
		    if ((cardTypeHidden != null) && (typeof (cardTypeHidden) != 'undefined'))
		    {
		        cardTypeHidden.value = '';
		    }
		}
	}

	lastKeyPressed = -1;
}

function captureKeyPress(e)
{
	if (!e)
		e = window.event;

    lastKeyPressed = e.keyCode || e.charCode;
}

function getFormattedNumber(ccNumber, cardType)
{
    if (cardType == "AmericanExpress")
    {
        return formatAmex(ccNumber);
    }

    return formatStandard(ccNumber);
}

function formatAmex(ccNumber)
{
    var localCCNumber = ccNumber.replace(/[-\s]/g, '');
    var finalNumber = new String();
    for (var i = 0; i < localCCNumber.length; i++)
    {
        finalNumber = finalNumber.concat(localCCNumber[i]);
        if (i == 3 || i == 10)
        {
            finalNumber = finalNumber.concat('-');
        }
    }
    return finalNumber.toString();
}

function formatStandard(ccNumber)
{
    var localCCNumber = ccNumber.replace(/[-\s]/g, '');
    var finalNumber = new String();
    if (localCCNumber.length >= 4)
    {
        for (var i = 0; i < localCCNumber.length; i++)
        {
            finalNumber = finalNumber.concat(localCCNumber[i]);
            if (i < 15 && (i + 1) % 4 == 0)
            {
                finalNumber = finalNumber.concat('-');
            }
        }
    }

    return finalNumber.toString();
}

function disableIssueBox(issueBoxId, bool)
{
    var element = document.getElementById(issueBoxId);
    
    if ((element != null) && (typeof (element) != 'undefined'))
    {
        element.disabled = bool;
        element.value = '';
    }
}

function setElementOpacity(eleId, opacityClass)
{
    var element = document.getElementById(eleId);    
    if ((element != null) && (typeof (element) != 'undefined'))
    {
        element.className = opacityClass;
        return true;
    }
    return false;
}

function pageLoad()
{
    hideOverlay();
}