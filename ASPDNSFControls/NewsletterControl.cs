// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;


namespace AspDotNetStorefrontControls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:NewsletterControl runat=server></{0}:NewsletterControl>")]
    public class NewsletterControl : WebControl, IScriptControl
    {
        #region "Internal Properties"



        /// <summary>
        /// whether or not to collect the first and last name
        /// </summary>
        private bool getName
        {
            get 
            {
                if (DesignMode)
                {
                    return false;
                }
                else
                {
                    return bool.Parse(AppLogic.AppConfig("Newsletter.GetFirstAndLast"));
                }
            }
        }
        /// <summary>
        /// Text prompting user
        /// </summary>
        private string SubscribeText
        {
            get
            {
                if (DesignMode)
                {
                    return "'Topic -- Newsletter.Subscribe'";
                }
                else
                    return AppLogic.GetString("SubscriptionToken.Subscribe", Customer.Current.SkinID, Customer.Current.LocaleSetting);
            }
        }

        /// <summary>
        /// Indicator of whether the user has captcha's turned on or not
        /// </summary>
        private bool CaptchaOn
        {
            get
            {
                return bool.Parse(AppLogic.AppConfig("Newsletter.UseCaptcha"));
            }
        }

 
        /// <summary>
        /// string Resource for first name
        /// </summary>
        private static string firstNameLabel
        {
            get
            {
                return AppLogic.GetString("Newsletter.FirstName", Customer.Current.SkinID, Customer.Current.LocaleSetting);
            }
        }
        /// <summary>
        /// String Resource for last name
        /// </summary>
        private static string lastNameLabel
        {
            get
            {
                return AppLogic.GetString("Newsletter.LastName", Customer.Current.SkinID, Customer.Current.LocaleSetting);
            }
        }

        #endregion


        #region "HTML"
        
        /// <summary>
        /// Returns HTML for first and last name fields for if getNames is false, empty spans with apropriate IDs
        /// </summary>
        private string firstAndLastHTML
        {
            get { return NewsletterControlService.firstAndLastHTML; }
        }


        /// <summary>
        /// Base HTML for the control
        /// </summary>
        private string baseHTML
        {
            get
            {
                if (DesignMode)
                {
                    return

 @"<span id='ptkSubscribe'> 
		<table class='NewsletterBox'> 
		<tr> 
			<td colspan='2'>Subscribe to our newsletter:</td> 
		</tr> 
		<tr> 
		<td>Email Address</td> 
		<td><input type='text' id='txtEmailAddress' onkeypress='enterSubmit(event);' /><input type='hidden' id='txtCaptcha' value='' /></td> 
	</tr> 
	<tr> 
		<td align='left' colspan=2><input type='button' value='Submit'id='cmdSubmit' onclick='clickSubmit();' /></td> 
	</tr> 
	</table> 
  </span> 
";

                }
                string controlHTML = NewsletterControlService.GetNewsletterToken()
                    .Replace("%SubscribeText%", SubscribeText)
                    .Replace("%FirstAndLastHTML%", firstAndLastHTML);

                return string.Format("{0}\n<script>{1}</script>",
                    new object[]
                    {
                           controlHTML,
                           ScriptControls.NewsletterControl
                    }
                );
            }
        }


        #endregion

        private ScriptManager xMgr;

        protected override void OnPreRender(EventArgs e)
        {

            xMgr = ScriptManager.GetCurrent(Page);
            xMgr.Services.Add(new ServiceReference("~/Newsletter.Subscribe.asmx"));
            xMgr.RegisterScriptControl(this);
            base.OnPreRender(e);
        }
        protected override void RenderContents(HtmlTextWriter output)
        {
            
            if (!DesignMode)
            {
                xMgr.RegisterScriptDescriptors(this);
            }
            output.Write(baseHTML);
            

        }

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if (DesignMode)
            {
                return null;
            }

            ScriptControlDescriptor descriptor =
                new ScriptControlDescriptor("AspDotNetStorefrontControls.NewsletterControl", this.ClientID);
            //descriptor.AddProperty("Xtext", this.Text);
            return new ScriptDescriptor[] { descriptor };
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            if (DesignMode)
            {
                return null;
            }
            ScriptReference xRef = new ScriptReference();
            if (Page != null)
                xRef.Path = Page.ClientScript.GetWebResourceUrl(this.GetType(), "AspDotNetStorefrontControls.ScriptControls.NewsletterControl");
            return new ScriptReference[]{};
        }

        #endregion
    }
}
