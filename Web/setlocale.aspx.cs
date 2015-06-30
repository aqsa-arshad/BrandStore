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
	/// Summary description for setlocale.
	/// </summary>
	public partial class setlocale : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			 
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            ThisCustomer.RequireCustomerRecord();

			String LocaleSetting = CommonLogic.QueryStringCanBeDangerousContent("LocaleSetting");
            if (LocaleSetting.Length == 0)
            {
                LocaleSetting = CommonLogic.QueryStringCanBeDangerousContent("Locale");
            }
            if (LocaleSetting.Length == 0)
            {
                LocaleSetting = Localization.GetDefaultLocale();
            }
            AppLogic.CheckForScriptTag(LocaleSetting);
            LocaleSetting = Localization.CheckLocaleSettingForProperCase(LocaleSetting);
            ThisCustomer.LocaleSetting = LocaleSetting;

            String CurrencySetting = CommonLogic.QueryStringCanBeDangerousContent("CurrencySetting");
            if (CurrencySetting.Length == 0)
            {
                CurrencySetting = CommonLogic.QueryStringCanBeDangerousContent("Currency");
            }
            if (CurrencySetting.Length == 0)
            {
                CurrencySetting = AppLogic.GetLocaleDefaultCurrency(LocaleSetting);
            }
            if (CurrencySetting.Length == 0)
            {
                CurrencySetting = Localization.GetPrimaryCurrency();
            }
            AppLogic.CheckForScriptTag(CurrencySetting);
            CurrencySetting = Localization.CheckCurrencySettingForProperCase(CurrencySetting);
            ThisCustomer.CurrencySetting = CurrencySetting;

            Label1.Text = String.Format(AppLogic.GetString("setlocale.aspx.1",ThisCustomer.SkinID,ThisCustomer.LocaleSetting),AppLogic.GetLocaleSettingDescription(LocaleSetting));

			string ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            // recompose the ReturnUrl chunk
            ReturnURL = AppLogic.ReturnURLDecode(ReturnURL);
            AppLogic.CheckForScriptTag(ReturnURL);

            if (ReturnURL.IndexOf("setlocale.aspx") != -1)
			{
				ReturnURL = String.Empty;
			}

			if (ReturnURL.Length == 0)
			{
				ReturnURL = "default.aspx";
			}
			Response.AddHeader("REFRESH","1; URL=" + Server.UrlDecode(ReturnURL));
		}

	}
}
