using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefront;


public partial class controls_PublicUserAfterLogin : System.Web.UI.UserControl
{
    Customer ThisCustomer;
    String WelcomeHeading=String.Empty;
    private String PanelText = String.Empty;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (ThisCustomer == null)
        {
            ThisCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;
        }
        WelcomeHeading = "JELD-WEN PRODUCTS";
        PanelText = "Our offering of countless options and unmatched styles provide a source of light and inspiration. To view our beautiful stock of windows and doors, or for more information, visit jeld-wen.com.";
        JeldWenProductSection.InnerHtml = PanelText;
        HeadingAfterPublicUserLogin.InnerText = WelcomeHeading;

    }
}