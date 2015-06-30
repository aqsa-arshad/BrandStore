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
using System.Collections.Generic;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for setcurrency.
	/// </summary>
    public partial class setcurrency : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            String CurrencySetting = CommonLogic.QueryStringCanBeDangerousContent("CurrencySetting");

            string query = "select CurrencyCode,Name from Currency  with (NOLOCK) where Published=1  order by Published desc, DisplayOrder,Name";
            List<String> AllowedCurrencies = new List<string>();
            Action<IDataReader> readAction = rs =>
            {
                while (rs.Read())
                {
                    if (!AllowedCurrencies.Contains(DB.RSField(rs, "CurrencyCode")))
                    {
                        AllowedCurrencies.Add(DB.RSField(rs, "CurrencyCode"));
                    }
                }
            };

            DB.UseDataReader(query, readAction);



            if (CurrencySetting.Length == 0)
            {
                CurrencySetting = CommonLogic.QueryStringCanBeDangerousContent("Currency");
            }
            if (CurrencySetting.Length == 0 || !Currency.isValidCurrencyCode(CurrencySetting) || !AllowedCurrencies.Contains(CurrencySetting))
            {
                CurrencySetting = Localization.GetPrimaryCurrency();
            }
            AppLogic.CheckForScriptTag(CurrencySetting);
            CurrencySetting = Localization.CheckCurrencySettingForProperCase(CurrencySetting);
            ThisCustomer.CurrencySetting = CurrencySetting;

			Label1.Text = String.Format(AppLogic.GetString("setCurrency.aspx.1",ThisCustomer.SkinID,ThisCustomer.CurrencySetting),Currency.GetName(CurrencySetting));

			string ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            // recompose the ReturnUrl chunk
            ReturnURL = AppLogic.ReturnURLDecode(ReturnURL);
            AppLogic.CheckForScriptTag(ReturnURL);
            if (ReturnURL.IndexOf("setcurrency.aspx") != -1)
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
