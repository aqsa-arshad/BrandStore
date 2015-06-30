// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for setvatsetting.
    /// </summary>
    public partial class setvatsetting : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            String VATSetting = Customer.ValidateVATSetting(CommonLogic.QueryStringCanBeDangerousContent("VATSetting"));
            String vtr = CommonLogic.QueryStringCanBeDangerousContent("VATRegistrationID");
            if (vtr.Trim().Length != 0)
            {
                ThisCustomer.RequireCustomerRecord();
            }
            VATSettingEnum vt = (VATSettingEnum)System.Int32.Parse(VATSetting);
            ThisCustomer.VATSettingRAW = vt;
			Exception vatServiceException;
            if (vtr.Length != 0 && !AppLogic.VATRegistrationIDIsValid(ThisCustomer, vtr, out vatServiceException))
            {
                vtr = String.Empty;
            }
            if (ThisCustomer.VATRegistrationID != vtr)
            {
                ThisCustomer.SetVATRegistrationID(vtr);
            }

            String msg = String.Empty;
            switch (vt)
            {
                case VATSettingEnum.ShowPricesInclusiveOfVAT:
                    msg = AppLogic.GetString("setvatsetting.aspx.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    break;
                case VATSettingEnum.ShowPricesExclusiveOfVAT:
                    msg = AppLogic.GetString("setvatsetting.aspx.4", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    break;
                default:
                    vt = VATSettingEnum.ShowPricesInclusiveOfVAT;
                    msg = AppLogic.GetString("setvatsetting.aspx.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    break;
            }
            Label1.Text = String.Format(AppLogic.GetString("setvatsetting.aspx.1", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), msg);

            string ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            // recompose the ReturnUrl chunk
            ReturnURL = AppLogic.ReturnURLDecode(ReturnURL);
            AppLogic.CheckForScriptTag(ReturnURL);
            if (ReturnURL.IndexOf("setvatsetting.aspx") != -1)
            {
                ReturnURL = String.Empty;
            }

            if (ReturnURL.Length == 0)
            {
                ReturnURL = "default.aspx";
            }
            Response.AddHeader("REFRESH", "1; URL=" + Server.UrlDecode(ReturnURL));

        }

    }
}
