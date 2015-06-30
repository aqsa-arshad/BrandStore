// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Class designed to retrieve form variables from a calling page, and add the item to the customers cart.
    /// </summary>
    public partial class addtocart : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            ThisCustomer.RequireCustomerRecord();

            String ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
            AppLogic.CheckForScriptTag(ReturnURL);

            CartTypeEnum CartType = CartTypeEnum.ShoppingCart;
            if (CommonLogic.FormNativeInt("IsWishList") == 1 || CommonLogic.QueryStringUSInt("IsWishList") == 1)
            {
                CartType = CartTypeEnum.WishCart;
            }
            if (CommonLogic.FormNativeInt("IsGiftRegistry") == 1 || CommonLogic.QueryStringUSInt("IsGiftRegistry") == 1)
            {
                CartType = CartTypeEnum.GiftRegistryCart;
            }
            if (AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
            {
                Response.Redirect("Default.aspx");
            }

            if (!ThisCustomer.IsRegistered && AppLogic.AppConfigBool("DisallowAnonCustomerToCreateWishlist"))
            {
                string ErrMsg = string.Empty;

                ErrorMessage er;

                if (CommonLogic.FormNativeInt("IsWishList") == 1 || CommonLogic.QueryStringUSInt("IsWishList") == 1)
                {
                    ErrMsg = AppLogic.GetString("signin.aspx.27", 1, ThisCustomer.LocaleSetting);
                    er = new ErrorMessage(ErrMsg);
                    Response.Redirect("signin.aspx?ErrorMsg=" + er.MessageId + "&ReturnUrl=" + Security.UrlEncode(ReturnURL));
                }

                if (CommonLogic.FormNativeInt("IsGiftRegistry") == 1 || CommonLogic.QueryStringUSInt("IsGiftRegistry") == 1)
                {
                    ErrMsg = AppLogic.GetString("signin.aspx.28", 1, ThisCustomer.LocaleSetting);
                    er = new ErrorMessage(ErrMsg);
                    Response.Redirect("signin.aspx?ErrorMsg=" + er.MessageId + "&ReturnUrl=" + Security.UrlEncode(ReturnURL));
                }
            }

            // if editing, nuke what was there, it will be replaced from what was submitted now from the product page.
            // NOTE. if a kit or pack was "edited", you don't have to do this, and ShoppingCartRecID is not material (and should not be in the form post)
            // kits and packs are "moved" from active cart to temp cart records, so they won't have a cart record id to begin with. They are built in the KitCart table instead
            int ShoppingCartRecID = CommonLogic.FormUSInt("CartRecID"); // only used for (non kit or pack) product/order edits from prior cart record
            if (ShoppingCartRecID == 0)
            {
                ShoppingCartRecID = CommonLogic.QueryStringUSInt("CartRecID");
            }
            if (ShoppingCartRecID != 0)
            {
                DB.ExecuteSQL("delete from ShoppingCart where ShoppingCartRecID=" + ShoppingCartRecID.ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString() + " and CartType=" + ((int)CartType).ToString() + " and StoreID = " + AppLogic.StoreID());
            }

            int ShippingAddressID = CommonLogic.QueryStringUSInt("ShippingAddressID"); // only used for multi-ship
            if (ShippingAddressID == 0)
            {
                ShippingAddressID = CommonLogic.FormNativeInt("ShippingAddressID");
            }
            if ((ShippingAddressID == 0 || !ThisCustomer.OwnsThisAddress(ShippingAddressID)) && ThisCustomer.PrimaryShippingAddressID != 0)
            {
                ShippingAddressID = ThisCustomer.PrimaryShippingAddressID;
            }

            int ProductID = CommonLogic.QueryStringUSInt("ProductID");
            if (ProductID == 0)
            {
                ProductID = CommonLogic.FormUSInt("ProductID");
            }

            int VariantID = CommonLogic.QueryStringUSInt("VariantID");
            if (VariantID == 0)
            {
                VariantID = CommonLogic.FormUSInt("VariantID");
            }
            if (ProductID == 0)
            {
                ProductID = AppLogic.GetVariantProductID(VariantID);
            }

            // if no VariantID is located, get the default variantID for the product
            if (VariantID == 0)
            {
                VariantID = AppLogic.GetDefaultProductVariant(ProductID);
            }

            int Quantity = CommonLogic.QueryStringUSInt("Quantity");
            if (Quantity == 0)
            {
                Quantity = CommonLogic.FormNativeInt("Quantity");
            }
            if (Quantity == 0)
            {
                Quantity = 1;
            }

            VariantStyleEnum VariantStyle = (VariantStyleEnum)CommonLogic.QueryStringUSInt("VariantStyle");
            if (CommonLogic.QueryStringCanBeDangerousContent("VariantStyle").Length == 0)
            {
                VariantStyle = (VariantStyleEnum)CommonLogic.FormNativeInt("VariantStyle");
            }

            decimal CustomerEnteredPrice = CommonLogic.FormNativeDecimal("Price");
            if (CustomerEnteredPrice == System.Decimal.Zero)
            {
                CustomerEnteredPrice = CommonLogic.QueryStringNativeDecimal("Price");
            }
            if (!AppLogic.VariantAllowsCustomerPricing(VariantID))
            {
                CustomerEnteredPrice = System.Decimal.Zero;
            }
            if (CustomerEnteredPrice < System.Decimal.Zero)
            {
                CustomerEnteredPrice = -CustomerEnteredPrice;
            }
            if (Currency.GetDefaultCurrency() != ThisCustomer.CurrencySetting && CustomerEnteredPrice != 0)
            {
                CustomerEnteredPrice = Currency.Convert(CustomerEnteredPrice, ThisCustomer.CurrencySetting, Localization.StoreCurrency()) ;
            }
           
           
            // QueryString params override Form Params!

            String ChosenColor = String.Empty;
            String ChosenColorSKUModifier = String.Empty;
            String ChosenSize = String.Empty;
            String ChosenSizeSKUModifier = String.Empty;
            String TextOption = CommonLogic.FormCanBeDangerousContent("TextOption");
            if (CommonLogic.QueryStringCanBeDangerousContent("TextOption").Length != 0)
            {
                TextOption = Security.HtmlEncode(CommonLogic.QueryStringCanBeDangerousContent("TextOption"));
            }


            // the color & sizes coming in here are MUST be in the Master WebConfig Locale ALWAYS!
            if (CommonLogic.QueryStringCanBeDangerousContent("Color").Length != 0)
            {
                String[] ColorSel = CommonLogic.QueryStringCanBeDangerousContent("Color").Split(',');
                try
                {
                    ChosenColor = Security.HtmlEncode(ColorSel[0]);
                }
                catch { }
                try
                {
                    ChosenColorSKUModifier = Security.HtmlEncode(ColorSel[1]);
                }
                catch { }
            }

            if (ChosenColor.Length == 0 && CommonLogic.FormCanBeDangerousContent("Color").Length != 0)
            {
                String[] ColorSel = CommonLogic.FormCanBeDangerousContent("Color").Split(',');
                try
                {
                    ChosenColor = Security.HtmlEncode(ColorSel[0]).Trim();                    
                }
                catch { }
                try
                {
                    ChosenColorSKUModifier = Security.HtmlEncode(ColorSel[1]);
                }
                catch { }
            }

            if (CommonLogic.QueryStringCanBeDangerousContent("Size").Length != 0)
            {
                String[] SizeSel = CommonLogic.QueryStringCanBeDangerousContent("Size").Split(',');
                try
                {
                    ChosenSize = Security.HtmlEncode(SizeSel[0]).Trim();
                }
                catch { }
                try
                {
                    ChosenSizeSKUModifier = Security.HtmlEncode(SizeSel[1]);
                }
                catch { }
            }

            if (ChosenSize.Length == 0 && CommonLogic.FormCanBeDangerousContent("Size").Length != 0)
            {
                String[] SizeSel = CommonLogic.FormCanBeDangerousContent("Size").Split(',');
                try
                {
                    ChosenSize = Security.HtmlEncode(SizeSel[0]).Trim();
                }
                catch { }
                try
                {
                    ChosenSizeSKUModifier = Security.HtmlEncode(SizeSel[1]);
                }
                catch { }
            }


            if (VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
            {

                String match = "<GroupAttributes></GroupAttributes>";
                String match2 = "<GroupAttributes></GroupAttributes>";
                if (ChosenSize.Trim().Length != 0 && ChosenColor.Trim().Length != 0)
                {
                    match = "<GroupAttributes><GroupAttributeName=\"Attr1\"Value=\"" + ChosenSize + "\"/><GroupAttributeName=\"Attr2\"Value=\"" + ChosenColor + "\"/></GroupAttributes>";
                    match2 = "<GroupAttributes><GroupAttributeName=\"Attr1\"Value=\"" + ChosenColor + "\"/><GroupAttributeName=\"Attr2\"Value=\"" + ChosenSize + "\"/></GroupAttributes>";
                }
                else if (ChosenSize.Trim().Length != 0 && ChosenColor.Trim().Length == 0)
                {
                    match = "<GroupAttributes><GroupAttributeName=\"Attr1\"Value=\"" + ChosenSize + "\"/></GroupAttributes>";
                }
                else if (ChosenSize.Trim().Length == 0 && ChosenColor.Trim().Length != 0)
                {
                    match = "<GroupAttributes><GroupAttributeName=\"Attr1\"Value=\"" + ChosenColor + "\"/></GroupAttributes>";
                }

                // reset variant id to the proper attribute match!
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rsERP = DB.GetRS("select VariantID,ExtensionData2 from ProductVariant with (NOLOCK) where VariantID=" + VariantID.ToString(), con))
                    {
                        while (rsERP.Read())
                        {

                            String thisVariantMatch = DB.RSField(rsERP, "ExtensionData2").Replace(" ", "").Trim();
                            match = Regex.Replace(match, "\\s+", "", RegexOptions.Compiled);

                            match2 = Regex.Replace(match2, "\\s+", "", RegexOptions.Compiled);

                            thisVariantMatch = Regex.Replace(thisVariantMatch, "\\s+", "", RegexOptions.Compiled);
                            if (match.Equals(thisVariantMatch, StringComparison.InvariantCultureIgnoreCase) ||
                                match2.Equals(thisVariantMatch, StringComparison.InvariantCultureIgnoreCase))
                            {
                                VariantID = DB.RSFieldInt(rsERP, "VariantID");
                                break;
                            }
                        }
                    }
                }
            }

            ShoppingCart cart = new ShoppingCart(1, ThisCustomer, CartType, 0, false);
            if (Quantity > 0)
            {
                if (AppLogic.IsAKit(ProductID))
                {
                    // -- new kit format -- //
                    bool productIsUsingKit2XmlPackage = !CommonLogic.IsStringNullOrEmpty(CommonLogic.FormCanBeDangerousContent("KitItems"));
                    if (productIsUsingKit2XmlPackage)
                    {
                        if (CommonLogic.FormBool("IsEditKit") && CommonLogic.FormUSInt("CartRecID") > 0)
                        {
                            int cartId = CommonLogic.FormUSInt("CartRecID");
                            AppLogic.ClearKitItems(ThisCustomer, ProductID, VariantID, cartId);
                        }

                        KitComposition preferredComposition = KitComposition.FromForm(ThisCustomer, ProductID, VariantID);

                        cart.AddItem(ThisCustomer, ShippingAddressID, ProductID, VariantID, Quantity, string.Empty, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, TextOption, CartType, false, false, 0, System.Decimal.Zero, preferredComposition);
                    }
                    else
                    {
                        cart.AddItem(ThisCustomer, ShippingAddressID, ProductID, VariantID, Quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, TextOption, CartType, false, false, 0, System.Decimal.Zero);
                    }
                }
                else
                {
                    cart.AddItem(ThisCustomer, ShippingAddressID, ProductID, VariantID, Quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, TextOption, CartType, false, false, 0, CustomerEnteredPrice);
                }
            }

            // handle upsell products:
            String UpsellProducts = CommonLogic.FormCanBeDangerousContent("UpsellProducts").Trim();
            if (UpsellProducts.Length != 0 && CartType == CartTypeEnum.ShoppingCart)
            {
                foreach (String s in UpsellProducts.Split(','))
                {
                    String PID = s.Trim();
                    if (PID.Length != 0)
                    {
                        int UpsellProductID = 0;
                        try
                        {
                            UpsellProductID = Localization.ParseUSInt(PID);
                            if (UpsellProductID != 0)
                            {
								int UpsellVariantID = AppLogic.GetProductsDefaultVariantID(UpsellProductID);
                                if (UpsellVariantID != 0)
                                {
                                    // this variant COULD have one size or color, so set it up like that:
                                    String Sizes = String.Empty;
                                    String SizeSKUModifiers = String.Empty;
                                    String Colors = String.Empty;
                                    String ColorSKUModifiers = String.Empty;

                                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                                    {
                                        con.Open();
                                        using (IDataReader rs = DB.GetRS("select Sizes,SizeSKUModifiers,Colors,ColorSKUModifiers from ProductVariant  with (NOLOCK)  where VariantID=" + UpsellVariantID.ToString(), con))
                                        {
                                            if (rs.Read())
                                            {
                                                Sizes = DB.RSFieldByLocale(rs, "Sizes", Localization.GetDefaultLocale());
                                                SizeSKUModifiers = DB.RSFieldByLocale(rs, "SizeSKUModifiers", Localization.GetDefaultLocale());
                                                Colors = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale());
                                                ColorSKUModifiers = DB.RSFieldByLocale(rs, "ColorSKUModifiers", Localization.GetDefaultLocale());
                                            }
                                        }
                                    }

                                    // safety check:
                                    if (Sizes.IndexOf(',') != -1)
                                    {
                                        Sizes = String.Empty;
                                        SizeSKUModifiers = String.Empty;
                                    }
                                    // safety check:
                                    if (Colors.IndexOf(',') != -1)
                                    {
                                        Colors = String.Empty;
                                        ColorSKUModifiers = String.Empty;
                                    }
                                    cart.AddItem(ThisCustomer, ShippingAddressID, UpsellProductID, UpsellVariantID, 1, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, String.Empty, CartType, false, false, 0, System.Decimal.Zero);
                                    Decimal PR = AppLogic.GetUpsellProductPrice(ProductID, UpsellProductID, ThisCustomer.CustomerLevelID);
                                    DB.ExecuteSQL("update shoppingcart set IsUpsell=1, ProductPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(PR) + " where CartType=" + ((int)CartType).ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString() + " and ProductID=" + UpsellProductID.ToString() + " and VariantID=" + UpsellVariantID.ToString() + " and convert(nvarchar(1000),ChosenColor)='' and convert(nvarchar(1000),ChosenSize)='' and convert(nvarchar(1000),TextOption)=''");
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            cart = null;

            AppLogic.eventHandler("AddToCart").CallEvent("&AddToCart=true&VariantID=" + VariantID.ToString() + "&ProductID=" + ProductID.ToString() + "&ChosenColor=" + ChosenColor + "&ChosenSize=" + ChosenSize);

            if (AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.InvariantCultureIgnoreCase) && 
                ReturnURL.Length != 0)
            {
                Response.Redirect(ReturnURL);
            }
            else
            {
                if (ReturnURL.Length == 0)
                {
                    ReturnURL = String.Empty;
                    if (Request.UrlReferrer != null)
                    {
                        ReturnURL = Request.UrlReferrer.AbsoluteUri; // could be null
                    }
                    if (ReturnURL == null)
                    {
                        ReturnURL = String.Empty;
                    }
                }
                if (CartType == CartTypeEnum.WishCart)
                {
                    Response.Redirect("wishlist.aspx?ReturnUrl=" + Security.UrlEncode(ReturnURL));
                }
                if (CartType == CartTypeEnum.GiftRegistryCart)
                {
                    Response.Redirect("giftregistry.aspx?ReturnUrl=" + Security.UrlEncode(ReturnURL));
                }
                Response.Redirect("ShoppingCart.aspx?add=true&ReturnUrl=" + Security.UrlEncode(ReturnURL));
            }


        }

    }
}
