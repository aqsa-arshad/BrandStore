// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using ASPDNSFRadWrapper.Telerik.Web.UI;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontLayout
{
    public abstract class TLayoutEditBase : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            this.Controls.Add(RadWindowScript);
            ((Button)FindControl(DefaultButton)).OnClientClick = "closeSave();";
            ((Button)FindControl(CancelButton)).OnClientClick = "closeCancel();";
            base.OnInit(e);
        }

        public abstract string DefaultButton
        { get; }
        public abstract string CancelButton
        { get; }

        protected LiteralControl RadWindowScript
        {
            get
            {
                return new LiteralControl(@"
                <script type='text/javascript'>
                    function GetRadWindow() 
                    {
                        var oWindow = null;
                        if (window.radWindow)
                            oWindow = window.radWindow;
                        else if (window.frameElement.radWindow)
                            oWindow = window.frameElement.radWindow;
                        return oWindow;
                    }
                    function closeSave() {
                        var oWindow = GetRadWindow();
                        oWindow.argument = 'save';
                        oWindow.close();
                    }
                    function closeCancel() {
                        var oWindow = GetRadWindow();
                        oWindow.argument = null;
                        oWindow.close();
                    }
                </script>
                    ");
            }
        }


    }
}
