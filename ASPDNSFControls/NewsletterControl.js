///<reference name="MicrosoftAjax.js"/>

Type.registerNamespace("AspDotNetStorefrontControls");

AspDotNetStorefrontControls.NewsletterControl = function(element) {
    AspDotNetStorefrontControls.NewsletterControl.initializeBase(this, [element]);
}

AspDotNetStorefrontControls.NewsletterControl.prototype = {
    initialize: function() {
        AspDotNetStorefrontControls.NewsletterControl.callBaseMethod(this, 'initialize');
        
        // Add custom initialization here
    },
    dispose: function() {        
        //Add custom dispose actions here
        AspDotNetStorefrontControls.NewsletterControl.callBaseMethod(this, 'dispose');
    }
    
    
}
function enterSubmit(e)
{
    var key;
    if(window.event) 
	{
	key = e.keyCode;
	}
else if(e.which) 
	{
	key = e.which;
	}
	if (key == 13)
	{
	    clickSubmit();
	}
}
function clickSubmit()
{
   var firstName = $get('txt_NL_FirstName').value;
   var lastName = $get('txt_NL_LastName').value;
   var address = $get('txtEmailAddress').value;
   var captcha = $get('txtCaptcha').value;
   
    AspDotNetStorefrontControls.NewsletterControlService.Subscribe(firstName, lastName, address, captcha, callback);
       
}

function callback(res)
{   
    if (res == "CAPTCHA_ERROR")
    {
        AspDotNetStorefrontControls.NewsletterControlService.getCapthaErrorBlock(callback);
        AspDotNetStorefrontControls.NewsletterControlService.ErrorStallback(callback);
    }
    else if (res == "ADDRESS_ERROR")
    {
        AspDotNetStorefrontControls.NewsletterControlService.getAddressErrorBlock(callback);
        AspDotNetStorefrontControls.NewsletterControlService.ErrorStallback(callback);
    }
    else
    {
        $get("ptkSubscribe").innerHTML = res;
    }
}


AspDotNetStorefrontControls.NewsletterControl.registerClass('AspDotNetStorefrontControls.NewsletterControl', Sys.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
