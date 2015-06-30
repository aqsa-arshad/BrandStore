// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for tableorder_process.
    /// </summary>
    public partial class tableorder_process : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            CartTypeEnum CartType = CartTypeEnum.ShoppingCart;

            Customer ThisCustomer = Customer.Current;

            ThisCustomer.RequireCustomerRecord();
            int CustomerID = ThisCustomer.CustomerID;

            ShoppingCart cart = new ShoppingCart(1, ThisCustomer, CartType, 0, false);
            for (int i = 0; i < Request.Form.Count; i++)
            {
                String FieldName = Request.Form.Keys[i];
                String FieldVal = Request.Form[Request.Form.Keys[i]];
                if (FieldName.ToUpperInvariant().IndexOf("_VLDT") == -1 && (FieldName.ToLowerInvariant().StartsWith("price_") || FieldName.ToLowerInvariant().StartsWith("qty_")) && FieldVal.Trim().Length != 0)
                {
                    try // ignore errors, just add what items we can:
                    {
                        decimal CustomerEnteredPrice = Decimal.Zero;
                        int Qty = 0;
                        if (FieldName.ToLowerInvariant().StartsWith("price_"))
                        {
                            CustomerEnteredPrice = Currency.ConvertToBaseCurrency(Localization.ParseLocaleDecimal(FieldVal, ThisCustomer.LocaleSetting), ThisCustomer.CurrencySetting);
                        }
                        else
                        {
                            Qty = Localization.ParseNativeInt(FieldVal);
                        }
                        String[] flds = FieldName.Split('_');
                        int ProductID = Localization.ParseUSInt(flds[1]);
                        int VariantID = Localization.ParseUSInt(flds[2]);
                        int ColorIdx = Localization.ParseUSInt(flds[3]);
                        int SizeIdx = Localization.ParseUSInt(flds[4]);
                        if (Qty == 0)
                        {
                            Qty = 1;
                        }
                        String ChosenColor = String.Empty;
                        String ChosenSize = String.Empty;
                        String ChosenColorSKUModifier = String.Empty;
                        String ChosenSizeSKUModifier = String.Empty;
                        if (ColorIdx > -1 || SizeIdx > -1)
                        {

                            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                            {
                                dbconn.Open();
                                using (IDataReader rs = DB.GetRS("select * from productvariant where VariantID=" + VariantID.ToString(), dbconn))
                                {
                                    rs.Read();
                                    ChosenColor = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale()).Split(',')[ColorIdx].Trim();
                                    if (DB.RSField(rs, "ColorSKUModifiers").Length != 0)
                                    {
                                        ChosenColorSKUModifier = DB.RSField(rs, "ColorSKUModifiers").Split(',')[ColorIdx].Trim();
                                    }
                                    ChosenSize = DB.RSFieldByLocale(rs, "Sizes", Localization.GetDefaultLocale()).Split(',')[SizeIdx].Trim();
                                    if (DB.RSField(rs, "SizeSKUModifiers").Length != 0)
                                    {
                                        ChosenSizeSKUModifier = DB.RSField(rs, "SizeSKUModifiers").Split(',')[SizeIdx].Trim();
                                    }
                                }
                            }
                        }

                        String TextOption = String.Empty;
                        cart.AddItem(ThisCustomer, 0, ProductID, VariantID, Qty, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, TextOption, CartType, false, false, 0, CustomerEnteredPrice);

                    }
                    catch { }
                }
            }
            cart = null;


            String ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            if (AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.InvariantCultureIgnoreCase) && ReturnURL.Length != 0)
            {
                Response.Redirect(ReturnURL);
            }
            else
            {

                String url = CommonLogic.IIF(CartType == CartTypeEnum.WishCart, "wishlist.aspx", "ShoppingCart.aspx?add=true");
                Response.Redirect(url);
            }
        }

    }
}
