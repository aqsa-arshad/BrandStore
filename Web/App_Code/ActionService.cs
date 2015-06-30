// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using AspDotNetStorefrontCore;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.Script.Services;

/// <summary>
/// Summary description for ActionService
/// </summary>
[WebService(Namespace = "http://www.aspdotnetstorefront.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[ScriptService]
public class ActionService : System.Web.Services.WebService
{

    public ActionService() {}

    
    /// <summary>
    /// AddTocart web method
    /// </summary>
    /// <param name="sProductId"></param>
    /// <param name="sVariantId"></param>
    /// <param name="sCartType"></param>
    /// <param name="sQuantity"></param>
    /// <param name="sShippingAddressId"></param>
    /// <param name="sVariantStyle"></param>
    /// <param name="textOption"></param>
    /// <param name="sCustomerEnteredPrice"></param>
    /// <param name="colorOptions"></param>
    /// <param name="sizeOptions"></param>
    /// <param name="upsellProductIds"></param>
    /// <param name="sShoppingCartRecordId"></param>
    /// <param name="sIsEditKit"></param>
    /// <param name="kitItemValues"></param>
    /// <returns></returns>
    [WebMethod(EnableSession = true), ScriptMethod]    
    public bool AddToCart(string sProductId, 
        string sVariantId, 
        string sCartType, 
        string sQuantity, 
        string sShippingAddressId, 
        string sVariantStyle, 
        string textOption, 
        string sCustomerEnteredPrice, 
        string colorOptions, 
        string sizeOptions,         
        string upsellProductIds,
        string sShoppingCartRecordId, 
        string sIsEditKit,        
        string kitItemValues)
    {
        bool success = false;

        Customer ThisCustomer = Customer.Current;
        if (ThisCustomer != null)
        {
            int productId = Localization.ParseNativeInt(sProductId);
                        
            AddToCartInfo formInput = AddToCartInfo.NewAddToCartInfo(ThisCustomer, productId);
            if (formInput == AddToCartInfo.INVALID_FORM_COMPOSITION)
            {
                success = false;
            }
            else
            {
                formInput.CartType = (CartTypeEnum)Localization.ParseNativeInt(sCartType);
                formInput.VariantId = Localization.ParseNativeInt(sVariantId);
                formInput.Quantity = Localization.ParseNativeInt(sQuantity);

                formInput.ParseValidShippingAddressId(sShippingAddressId);
                formInput.ParseValidCustomerEnteredPrice(sCustomerEnteredPrice);

                formInput.ParseColorOptions(colorOptions);
                formInput.ParseSizeOptions(sizeOptions);

                formInput.TextOption = textOption;
                formInput.VariantStyle = (VariantStyleEnum)Localization.ParseNativeInt(sVariantStyle);

                // kit specific
                formInput.IsEditKit = IsFormBool(sIsEditKit);
                formInput.ShoppingCartRecordId = Localization.ParseNativeInt(sShoppingCartRecordId);
                formInput.IsKit2 = !CommonLogic.IsStringNullOrEmpty(kitItemValues);

                formInput.ParseUpsellProductIds(upsellProductIds);

                success = ShoppingCart.AddToCart(ThisCustomer, formInput);
            }

        }

        return success;
    }

    private bool IsFormBool(String tmpS)
    {
        tmpS = tmpS.ToUpperInvariant();
        return (tmpS == "TRUE" || tmpS == "YES" || tmpS == "1");
    }

}

