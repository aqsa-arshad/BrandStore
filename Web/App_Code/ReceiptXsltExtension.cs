// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using AspDotNetStorefrontCore;

/// <summary>
/// Receipt Xslt Extension Class
/// </summary>
public class ReceiptXsltExtension : XSLTExtensionBase
{
    #region Variable Declaration

    private const int ORDER_COUPONTYPE = 0;
    private const int PRODUCT_COUPONTYPE = 1;
    private const int GIFTCARD_COUPONTYPE = 2;

    #endregion

    #region Constructor

    /// <summary>
    /// ReceiptXsltExtension Constructor
    /// </summary>
    public ReceiptXsltExtension()
        : this(null, 1, null)
    {
    }

    public ReceiptXsltExtension(Customer cust, int SkinID, Dictionary<string, EntityHelper> EntityHelpers)
        : base(cust, SkinID)
    {
    }

    #endregion

    #region Methods

    #region FormatCurrencyWithoutCurrencyCode
    /// <summary>
    /// Format the currency value into it's localized currency pattern
    /// </summary>
    /// <param name="sCurrencyValue">The currency value</param>
    /// <returns>The formatted currency string</returns>
    public virtual string FormatCurrencyWithoutCurrencyCode(string sCurrencyValue)
    {
        return FormatCurrencyWithoutCurrencyCode(sCurrencyValue, ThisCustomer.CurrencySetting);
    }
    public virtual string WCurrencyCode(string sCurrencyValue)
    {
        return FormatCurrencyWithoutCurrencyCode(sCurrencyValue, ThisCustomer.CurrencySetting);
    }
    /// <summary>
    /// Format the currency value into it's localized currency pattern
    /// </summary>
    /// <param name="sCurrencyValue">The currency value</param>
    /// <param name="sTargetCurrency">The target currency to base on</param>
    /// <returns>The formatted currency string</returns>
    public virtual string FormatCurrencyWithoutCurrencyCode(string sCurrencyValue, string sTargetCurrency)
    {
        InputValidator iv = new InputValidator("FormatCurrencyWithoutCurrencyCode");
        decimal value = iv.ValidateDecimal("CurrencyValue", sCurrencyValue);
        string targetCurrency = iv.ValidateString("TargetCurrency", sTargetCurrency);

        decimal amt = Currency.Convert(value, Localization.GetPrimaryCurrency(), targetCurrency);

        String tmpS = String.Empty;
        // get currency specs for display control:
        String DisplaySpec = Currency.GetDisplaySpec(targetCurrency);
        String DisplayLocaleFormat = Currency.GetDisplayLocaleFormat(targetCurrency);

        if (DisplaySpec.Length != 0)
        {
            CultureInfo ci = new CultureInfo(DisplayLocaleFormat);

            if (DisplaySpec.LastIndexOf(',') > DisplaySpec.LastIndexOf("."))
            {
                ci.NumberFormat.CurrencyDecimalSeparator = ",";

                if (DisplaySpec.LastIndexOf(".") > -1)
                {
                    ci.NumberFormat.CurrencyGroupSeparator = ".";
                }
            }

            if (!DisplaySpec.StartsWith("#"))
            {
                int indexOf = DisplaySpec.IndexOf('#');

                if (indexOf > -1)
                {
                    ci.NumberFormat.CurrencySymbol = DisplaySpec.Substring(0, indexOf);
                }
            }

            tmpS = amt.ToString("C", ci.NumberFormat);
        }
        else if (DisplayLocaleFormat.Length != 0)
        {
            CultureInfo formatter = new CultureInfo(DisplayLocaleFormat);

            // for debugging purposes
            if (CommonLogic.QueryStringUSInt("dec") > 0)
            {
                int decimalPlaces = CommonLogic.QueryStringUSInt("dec");
                formatter.NumberFormat.CurrencyDecimalDigits = decimalPlaces;
            }

            tmpS = amt.ToString("C", formatter);
            if (tmpS.StartsWith("("))
            {
                tmpS = "-" + tmpS.Replace("(", "").Replace(")", "");
            }
        }
        else
        {
            tmpS = Localization.CurrencyStringForDisplayWithoutExchangeRate(amt, false); // use some generic default!
        }

        return tmpS;
    }

    #endregion

    #region Inject
    /// <summary>
    /// Inject element string into xslt output
    /// </summary>
    /// <param name="element">The literal element</param>
    /// <returns>The element itself</returns>
    public string Inject(string element)
    {
        return element;
    }
    #endregion

    #region AttachAndPreComputeIntrinsics
    /// <summary>
    /// Attaches the precomputed intrinsics like discounts, etc.. for the receipt xml for easier display and manipulation on the xml package
    /// </summary>
    /// <param name="root">The root element</param>
    /// <returns>The root element</returns>
    public IXPathNavigable AttachAndPreComputeIntrinsics(IXPathNavigable root)
    {
        XPathNavigator nav = root.CreateNavigator();

        if (nav.SelectSingleNode("Order/OrderInfo") != null)
        {
            // attach the order options as xml format as children of order node
            AttachOrderOptionsXml(nav);

            // assign discounts
            PreComputeLineItemIntrinsics(nav);

            // precompute total discounts
            PreComputeDiscounts(nav);

            // misc
            CheckIfWeShouldRequireShipping(nav);

            CheckIfFreeShipping(nav);
        }

        return root;
    }
    #endregion

    /// <summary>
    /// Check in orderInfoNode if a coupon is applied
    /// </summary>
    /// <param name="orderInfoNode"></param>
    /// <returns>returns true if there is a couponcode and coupon includes free shipping</returns>
    private bool CheckIfCouponApplied(XmlNode orderInfoNode)
    {
        bool hasCouponApplied = CommonLogic.IIF(!CommonLogic.IsStringNullOrEmpty(XmlCommon.XmlField(orderInfoNode, "CouponCode")), true, false);
        bool couponIncludesFreeshipping = CommonLogic.IIF(XmlCommon.XmlFieldNativeInt(orderInfoNode, "CouponIncludesFreeShipping") == 1, true, false);

        return hasCouponApplied && couponIncludesFreeshipping;
    }

    /// <summary>
    /// Check if customer level includes free shipping
    /// </summary>
    /// <param name="orderInfoNode"></param>
    /// <returns>returns true if customer level includes free shipping</returns>
    private bool CheckIfLevelHasFreeShipping(XmlNode orderInfoNode)
    {
        bool hascustomerLevel = CommonLogic.IIF(XmlCommon.XmlFieldNativeInt(orderInfoNode, "LevelID") != 0, true, false);
        bool levelHasFreeShipping = CommonLogic.IIF(XmlCommon.XmlFieldNativeInt(orderInfoNode, "LevelHasFreeShipping") == 1, true, false);

        return hascustomerLevel && levelHasFreeShipping;
    }

    /// <summary>
    /// Check if order is All Downloads
    /// </summary>
    /// <param name="orderInfoNode"></param>
    /// <returns>returns true if order is all downloads</returns>
    private bool CheckIfAllDownloads(XmlNode orderInfoNode)
    {
        return CommonLogic.IIF(XmlCommon.XmlFieldNativeInt(orderInfoNode, "allDownloads") == 1, true, false);
    }

    /// <summary>
    /// Check if order is all free shipping
    /// </summary>
    /// <param name="orderInfoNode"></param>
    /// <returns>returns true if all order is free shipping</returns>
    private bool CheckIfAllFreeShipping(XmlNode orderInfoNode)
    {
        return CommonLogic.IIF(XmlCommon.XmlFieldNativeInt(orderInfoNode, "allFreeShipping") == 1, true, false);
    }

    /// <summary>
    /// Check if order is all system products
    /// </summary>
    /// <param name="orderInfoNode"></param>
    /// <returns>returns true if order is all system products</returns>
    private bool CheckIfAllSystemProducts(XmlNode orderInfoNode)
    {
        return CommonLogic.IIF(XmlCommon.XmlFieldNativeInt(orderInfoNode, "allSystemproducts") == 1, true, false); ;
    }


    /// <summary>
    /// Check if order is free shipping and customer selects free shipping methods
    /// </summary>
    /// <param name="nav">xml</param>
    private void CheckIfFreeShipping(XPathNavigator nav)
    {
        XmlNode orderInfoNode = GetXmlNode(nav.SelectSingleNode("Order/OrderInfo"));

        bool freeShipping = false;

        if ((CheckIfCouponApplied(orderInfoNode) || CheckIfLevelHasFreeShipping(orderInfoNode)) || (CheckIfAllDownloads(orderInfoNode) || CheckIfAllFreeShipping(orderInfoNode) || CheckIfAllSystemProducts(orderInfoNode)))
        {
            freeShipping = true;
        }

        bool customerChoseFreeShippingMethod = true;
        if (AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
        {
            int shippingMethodId = XmlCommon.XmlFieldNativeInt(orderInfoNode, "ShippingMethodID");
            string commaSeparatedIds = AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn");

            customerChoseFreeShippingMethod = CommonLogic.IntegerIsInIntegerList(shippingMethodId, commaSeparatedIds);
        }

        freeShipping = freeShipping && customerChoseFreeShippingMethod;

        XmlNode isFreeShippingNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "IsFreeShipping", string.Empty);
        isFreeShippingNode.InnerText = XmlCommon.XmlEncode(freeShipping.ToString());
        orderInfoNode.InsertAfter(isFreeShippingNode, orderInfoNode.LastChild);
    }

    #region CheckIfWeShouldRequireShipping
    /// <summary>
    /// Attaches an element named ShippingNotRequired that serves as a flag whether our order does not require shipping checking all the available factors
    /// </summary>
    /// <param name="nav">The XPathNavigator</param>
    private void CheckIfWeShouldRequireShipping(XPathNavigator nav)
    {
        XmlNode orderInfoNode = GetXmlNode(nav.SelectSingleNode("Order/OrderInfo"));
        bool weDontRequireShipping = false;

        if (AppLogic.AppConfigBool("SkipShippingOnCheckout") == true)
        {
            weDontRequireShipping = true;
        }
        else
        {
            bool isMultiShipping = XmlCommon.XmlFieldBool(orderInfoNode, "multiship");
            bool allAreDownloadProducts = XmlCommon.XmlFieldBool(orderInfoNode, "allDownloads");
            bool allAreSystemProducts = XmlCommon.XmlFieldBool(orderInfoNode, "allSystemproducts");

            weDontRequireShipping = isMultiShipping == false &&
                                    (allAreDownloadProducts || allAreSystemProducts);

            if (weDontRequireShipping == false)
            {
                // now check if all of the line items is No Shipping Required..
                XPathNodeIterator lineItemsThatAreNotFreeShippingIfAnyNodeIterator = nav.Select("OrderItems/Item[FreeShipping != 2]");
                weDontRequireShipping = !lineItemsThatAreNotFreeShippingIfAnyNodeIterator.MoveNext();
            }
        }

        XmlNode shippingNotRequiredNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "ShippingNotRequired", string.Empty);
        shippingNotRequiredNode.InnerText = XmlCommon.XmlEncode(weDontRequireShipping.ToString());
        orderInfoNode.InsertAfter(shippingNotRequiredNode, orderInfoNode.LastChild);
    }
    #endregion

    #region GetXmlNode
    /// <summary>
    /// Extracts the XmlNode provided an XPathNavigator object
    /// </summary>
    /// <param name="nav">The XPathNavigator</param>
    /// <returns></returns>
    private XmlNode GetXmlNode(XPathNavigator nav)
    {
        IHasXmlNode node = nav as IHasXmlNode;
        return node.GetNode();
    }
    #endregion

    #region PreComputeDiscounts
    /// <summary>
    /// Precompute the order discounts and attach it to the OrderInfo node
    /// </summary>
    /// <param name="nav">The XPathNavigator</param>
    private void PreComputeDiscounts(XPathNavigator nav)
    {
        // Precompute the totals
        XmlNode orderInfoNode = GetXmlNode(nav.SelectSingleNode("Order/OrderInfo"));
        decimal rawSubTotal = decimal.Zero;
        decimal rawSubTotalVat = decimal.Zero;
        XPathNodeIterator lineItemIterator = nav.Select("OrderItems/Item");

        bool incVat = ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;
        while (lineItemIterator.MoveNext())
        {
            XmlNode lineItemNode = GetXmlNode(lineItemIterator.Current);
            rawSubTotal += XmlCommon.XmlFieldNativeDecimal(lineItemNode, "OrderedProductPrice");

            if (incVat)
            {
                rawSubTotalVat += XmlCommon.XmlFieldNativeDecimal(lineItemNode, "ExtVatAmount");
            }
        }

        XPathNodeIterator orderOptionIterator = nav.Select("Order/OrderInfo/OrderOptionsXml/OrderOption");
        while (orderOptionIterator.MoveNext())
        {
            XmlNode orderOptionNode = GetXmlNode(orderOptionIterator.Current);
            rawSubTotal += XmlCommon.XmlFieldNativeDecimal(orderOptionNode, "Price");
        }

        decimal appliedSubTotal = rawSubTotal;
        decimal subTotal = XmlCommon.XmlFieldNativeDecimal(orderInfoNode, "OrderSubtotal");

        XmlNode rawSubTotalNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "RawSubTotal", string.Empty);
        decimal actualRawSubTotal = rawSubTotal + rawSubTotalVat;
        rawSubTotalNode.InnerText = XmlCommon.XmlEncode(actualRawSubTotal.ToString());
        orderInfoNode.InsertAfter(rawSubTotalNode, orderInfoNode.LastChild);

        XmlNode discountsNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "Discounts", string.Empty);
        orderInfoNode.InsertAfter(discountsNode, orderInfoNode.LastChild);

        string couponCode = XmlCommon.XmlField(orderInfoNode, "CouponCode");

        int couponType = XmlCommon.XmlFieldNativeInt(orderInfoNode, "CouponType");
        bool hasCouponApplied = !string.IsNullOrEmpty(couponCode) && couponType != GIFTCARD_COUPONTYPE;

        decimal couponDiscountPercent = XmlCommon.XmlFieldNativeDecimal(orderInfoNode, "CouponDiscountPercent");
        decimal couponDiscountAmount = XmlCommon.XmlFieldNativeDecimal(orderInfoNode, "CouponDiscountAmount");

        if (hasCouponApplied)
        {
            // were only interested in the amount
            if (couponDiscountPercent > decimal.Zero)
            {
                XmlNode discountNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "Discount", string.Empty);
                XmlAttribute typeAttribute = orderInfoNode.OwnerDocument.CreateAttribute("type");
                typeAttribute.Value = AppLogic.GetString("notification.betareceipt.xml.config.49", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);//"Coupon Percent";
                discountNode.Attributes.Append(typeAttribute);

                decimal couponDiscountPercentAmount = decimal.Zero;

                // check whether the coupon type is by percent
                // if it is, compute by line items included only
                string validProductIdCommaSeparated = XmlCommon.XmlField(orderInfoNode, "ValidProductsForCoupon");
                if (!string.IsNullOrEmpty(validProductIdCommaSeparated) &&
                    couponType == PRODUCT_COUPONTYPE)
                {
                    string[] validProductsIds = validProductIdCommaSeparated.Split(',');

                    foreach (string productId in validProductsIds)
                    {
                        // let's find the line item this product is mapped to
                        string xpath = string.Format("OrderItems/Item[ProductID={0}]", productId);
                        XPathNavigator validProductNav = nav.SelectSingleNode(xpath);
                        if (validProductNav != null)
                        {
                            XmlNode validProductNode = GetXmlNode(validProductNav);
                            if (validProductNode != null)
                            {
                                decimal lineItemExtPrice = XmlCommon.XmlFieldNativeDecimal(validProductNode, "OrderedProductPrice");
                                if (incVat)
                                {
                                    lineItemExtPrice += XmlCommon.XmlFieldNativeDecimal(validProductNode, "ExtVatAmount");
                                }
                                decimal lineItemCouponDiscountAmount = lineItemExtPrice * (couponDiscountPercent / 100M);

                                couponDiscountPercentAmount += lineItemCouponDiscountAmount;
                            }
                        }
                    }
                }
                else
                {
                    // coupon is applied to order
                    if (couponDiscountPercent == 100M && subTotal == decimal.Zero)
                    {
                        couponDiscountPercentAmount = appliedSubTotal;
                    }
                    else
                    {
                        couponDiscountPercentAmount = (appliedSubTotal * (couponDiscountPercent / 100M));
                    }
                }

                XmlAttribute valueAttribute = orderInfoNode.OwnerDocument.CreateAttribute("value");
                valueAttribute.Value = couponDiscountPercentAmount.ToString();
                discountNode.Attributes.Append(valueAttribute);

                discountsNode.AppendChild(discountNode);

                // apply the discount at this one
                appliedSubTotal -= couponDiscountPercentAmount;
            }

            if (couponDiscountAmount > decimal.Zero &&
                appliedSubTotal > decimal.Zero)
            {
                // check whether the coupon type is by percent
                // if it is, compute by line items included only
                string validProductIdCommaSeparated = XmlCommon.XmlField(orderInfoNode, "ValidProductsForCoupon");
                if (!string.IsNullOrEmpty(validProductIdCommaSeparated) &&
                    couponType == PRODUCT_COUPONTYPE)
                {
                    string[] validProductsIds = validProductIdCommaSeparated.Split(',');

                    foreach (string productId in validProductsIds)
                    {
                        // let's find the line item this product is mapped to
                        string xpath = string.Format("OrderItems/Item[ProductID={0}]", productId);
                        XPathNavigator validProductNav = nav.SelectSingleNode(xpath);
                        if (validProductNav != null)
                        {
                            XmlNode validProductNode = GetXmlNode(validProductNav);
                            if (validProductNode != null)
                            {
                                decimal lineItemExtPrice = XmlCommon.XmlFieldNativeDecimal(validProductNode, "OrderedProductPrice");

                                decimal lineItemDiscountAmount = couponDiscountAmount;
                                if (couponDiscountAmount > lineItemExtPrice)
                                {
                                    couponDiscountAmount += lineItemExtPrice;
                                }

                                // need to take into account the quantity
                                int qty = XmlCommon.XmlFieldNativeInt(validProductNode, "Quantity");
                                couponDiscountAmount *= qty;
                            }
                        }
                    }
                }
                else if (couponDiscountAmount > appliedSubTotal)
                {
                    couponDiscountAmount = appliedSubTotal;
                }

                XmlNode discountNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "Discount", string.Empty);
                XmlAttribute typeAttribute = orderInfoNode.OwnerDocument.CreateAttribute("type");
                typeAttribute.Value = AppLogic.GetString("notification.betareceipt.xml.config.50", ThisCustomer.SkinID, ThisCustomer.LocaleSetting); // "Coupon Amount";
                discountNode.Attributes.Append(typeAttribute);

                XmlAttribute valueAttribute = orderInfoNode.OwnerDocument.CreateAttribute("value");
                valueAttribute.Value = couponDiscountAmount.ToString();
                discountNode.Attributes.Append(valueAttribute);

                discountsNode.AppendChild(discountNode);

                appliedSubTotal -= couponDiscountAmount;
            }
        }

        decimal levelDiscountAmount = XmlCommon.XmlFieldNativeDecimal(orderInfoNode, "LevelDiscountAmount");
        if (XmlCommon.XmlFieldNativeInt(orderInfoNode, "LevelID") > 0 &&
            levelDiscountAmount > 0)
        {
            if (levelDiscountAmount > decimal.Zero &&
                appliedSubTotal > decimal.Zero)
            {
                if (levelDiscountAmount > appliedSubTotal)
                {
                    levelDiscountAmount = appliedSubTotal;
                }
            }
            // were only interested in the amount
            XmlNode discountNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "Discount", string.Empty);
            XmlAttribute typeAttribute = orderInfoNode.OwnerDocument.CreateAttribute("type");
            typeAttribute.Value = AppLogic.GetString("notification.betareceipt.xml.config.51", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);//"Level"
            discountNode.Attributes.Append(typeAttribute);

            XmlAttribute valueAttribute = orderInfoNode.OwnerDocument.CreateAttribute("value");
            valueAttribute.Value = levelDiscountAmount.ToString();
            discountNode.Attributes.Append(valueAttribute);

            discountsNode.AppendChild(discountNode);

            appliedSubTotal -= levelDiscountAmount;
        }

        // gift card discount{shown as payment}
        decimal giftCardPaymentAppliedAmount = couponDiscountAmount; // we save the applied gift card value on the coupon discount amount col

        bool hasGiftCardApplied = (couponType == GIFTCARD_COUPONTYPE) && giftCardPaymentAppliedAmount > decimal.Zero;

        decimal orderTotal = XmlCommon.XmlFieldNativeDecimal(orderInfoNode, "OrderTotal");
        decimal netTotal = orderTotal;

        if (hasGiftCardApplied)
        {
            // we have Gift Card Applied            
            netTotal = orderTotal - giftCardPaymentAppliedAmount;
        }

        XmlNode hasgiftCardAppliedNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "HasGiftCardApplied", string.Empty);
        hasgiftCardAppliedNode.InnerText = XmlCommon.XmlEncode(hasGiftCardApplied.ToString());
        orderInfoNode.InsertAfter(hasgiftCardAppliedNode, orderInfoNode.LastChild);

        XmlNode netTotalNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "NetTotal", string.Empty);
        netTotalNode.InnerText = XmlCommon.XmlEncode(netTotal.ToString());
        orderInfoNode.InsertAfter(netTotalNode, orderInfoNode.LastChild);
    }
    #endregion

    #region PreComputeLineItemDiscounts
    /// <summary>
    /// Precompute the line item vat and discounts and attach it inside the order info node
    /// </summary>
    /// <param name="nav">The XPathNavigator</param>
    private void PreComputeLineItemIntrinsics(XPathNavigator nav)
    {
        XmlNode orderInfoNode = GetXmlNode(nav.SelectSingleNode("Order/OrderInfo"));
        XPathNodeIterator lineItemNodeIterator = nav.Select("OrderItems/Item");

        decimal allLineItemDiscounts = decimal.Zero;

        while (lineItemNodeIterator.MoveNext())
        {
            XmlNode lineItemNode = GetXmlNode(lineItemNodeIterator.Current);

            bool isAKit = XmlCommon.XmlFieldBool(lineItemNode, "IsAKit");
            bool isAPack = XmlCommon.XmlFieldBool(lineItemNode, "IsAPack");

            int quantity = 1;
            int.TryParse(XmlCommon.XmlField(lineItemNode, "Quantity"), out quantity);

            decimal price = 0;
            Decimal.TryParse(XmlCommon.XmlField(lineItemNode, "OrderedProductRegularPrice"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price);

            decimal orderedExtendedPrice = 0;
            Decimal.TryParse(XmlCommon.XmlField(lineItemNode, "OrderedProductPrice"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out orderedExtendedPrice);

            decimal taxRate = Decimal.Parse(XmlCommon.XmlField(lineItemNode, "TaxRate"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

            decimal vatAmount = decimal.Zero;
            decimal extendedVatAmount = decimal.Zero;

            int CategoryFundType = 0;
            CategoryFundType = int.Parse(XmlCommon.XmlField(lineItemNode, "CategoryFundType"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            decimal CreditPrice = Decimal.Parse(XmlCommon.XmlField(lineItemNode, "CreditPrice"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            decimal BLuBucksUsed = Decimal.Parse(XmlCommon.XmlField(lineItemNode, "BluBucksUsed"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            decimal CategoryFundsUsed = Decimal.Parse(XmlCommon.XmlField(lineItemNode, "CategoryFundUsed"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            String CreditFundUsedName = String.Empty;
            decimal CreditFundUsed = decimal.Zero;
            if (BLuBucksUsed > 0)
            {
                CreditFundUsedName = "BLU™ Bucks Discount: "; 
                CreditFundUsed = BLuBucksUsed;
            }
            else if (CategoryFundType == (int)FundType.SOFFunds)
            {
                CreditFundUsed = CategoryFundsUsed;
                CreditFundUsedName = "Sales Funds Discount: ";
            }
            else if (CategoryFundType == (int)FundType.DirectMailFunds)
            {
                CreditFundUsed = CategoryFundsUsed;
                CreditFundUsedName = "Direct Mail Funds Discount: ";
            }
            else if (CategoryFundType == (int)FundType.DisplayFunds)
            {
                CreditFundUsed = CategoryFundsUsed;
                CreditFundUsedName = "Display Funds Discount: ";
            }
            else if (CategoryFundType == (int)FundType.LiteratureFunds)
            {
                CreditFundUsed = CategoryFundsUsed;
                CreditFundUsedName = "Literature Funds Discount: ";
            }
            else if (CategoryFundType == (int)FundType.POPFunds)
            {
                CreditFundUsed = CategoryFundsUsed;
                CreditFundUsedName = "POP Funds Discount: ";
            }

            bool applyVat = AppLogic.AppConfigBool("VAT.Enabled") == true &&
                            XmlCommon.XmlFieldBool(orderInfoNode, "LevelHasNoTax") == false &&
                            XmlCommon.XmlFieldBool(lineItemNode, "IsTaxable") == true &&
                            string.IsNullOrEmpty(XmlCommon.XmlField(orderInfoNode, "VATRegistrationID"));

            if (applyVat)
            {
                if (AppLogic.AppConfigBool("VAT.RoundPerItem"))
                {
                    vatAmount = (((price / quantity) * taxRate) / 100M) * quantity;
                    extendedVatAmount = (((orderedExtendedPrice / quantity) * taxRate) / 100M) * quantity;
                }
                else
                {
                    vatAmount = (price * taxRate) / 100M;
                    extendedVatAmount = (orderedExtendedPrice * taxRate) / 100M;
                }

                if (isAKit || isAPack)
                {
                    vatAmount = extendedVatAmount / quantity;
                }
            }
            // let's save these as decimal values, leave out formatting on a later call
            XmlNode vatAmountNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "VatAmount", string.Empty);
            vatAmountNode.InnerText = XmlCommon.XmlEncode(vatAmount.ToString(CultureInfo.InvariantCulture));

            XmlNode extVatAmountNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "ExtVatAmount", string.Empty);
            extVatAmountNode.InnerText = XmlCommon.XmlEncode(extendedVatAmount.ToString(CultureInfo.InvariantCulture));

            // insert these nodes on the bottom
            lineItemNode.InsertAfter(vatAmountNode, lineItemNode.LastChild);
            lineItemNode.InsertAfter(extVatAmountNode, lineItemNode.LastChild);

            decimal regularExtendedPrice = decimal.Zero;
            decimal discount = decimal.Zero;

            // kit and pack products don't save the regular price only the sales price
            // we won't be supporting line item discounts on these item types then
            if (isAKit || isAPack)
            {
                regularExtendedPrice = decimal.Zero;
                discount = decimal.Zero;

                price = orderedExtendedPrice / quantity;
            }
            else
            {
                // make sure to round to 2 decimal places
                price = Math.Round(price, 2, MidpointRounding.AwayFromZero);
                regularExtendedPrice = price * quantity;
                discount = (regularExtendedPrice - orderedExtendedPrice);
            }

            bool showPriceVatInclusive = ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;

            decimal displayPrice = price;
            if (applyVat && showPriceVatInclusive)
            {
                displayPrice += vatAmount;
            }

            decimal displayExtPrice = orderedExtendedPrice;
            if (applyVat && showPriceVatInclusive)
            {
                displayExtPrice += extendedVatAmount;
            }

            // let's save these as decimal values, leave out formatting on a later call
            XmlNode extRegularPriceNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "ExtendedRegularPrice", string.Empty);
            extRegularPriceNode.InnerText = XmlCommon.XmlEncode(regularExtendedPrice.ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(extRegularPriceNode, lineItemNode.LastChild);

            XmlNode discountNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "DiscountAmount", string.Empty);
            discountNode.InnerText = XmlCommon.XmlEncode(discount.ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(discountNode, lineItemNode.LastChild);

            // price column
            XmlNode priceNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "Price", string.Empty);
            priceNode.InnerText = XmlCommon.XmlEncode(price.ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(priceNode, lineItemNode.LastChild);

            XmlNode displayPriceNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "DisplayPrice", string.Empty);
            displayPriceNode.InnerText = XmlCommon.XmlEncode(displayPrice.ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(displayPriceNode, lineItemNode.LastChild);

            XmlNode displayExtPriceNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "DisplayExtPrice", string.Empty);
            displayExtPriceNode.InnerText = XmlCommon.XmlEncode(displayExtPrice.ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(displayExtPriceNode, lineItemNode.LastChild);

            // Funds used
            XmlNode displayCreditUsedNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "CreditFundUsed", string.Empty);
            displayCreditUsedNode.InnerText = XmlCommon.XmlEncode(CreditFundUsed.ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(displayCreditUsedNode, lineItemNode.LastChild);

            XmlNode CreditFundUsedNameNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "CreditFundUsedName", string.Empty);
            CreditFundUsedNameNode.InnerText = XmlCommon.XmlEncode(CreditFundUsedName.ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(CreditFundUsedNameNode, lineItemNode.LastChild);

            // credit price
            XmlNode displayCreditPriceNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "CreditPrice", string.Empty);
            displayCreditPriceNode.InnerText = XmlCommon.XmlEncode(CreditPrice.ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(displayCreditPriceNode, lineItemNode.LastChild);

            
            XmlNode discountWithVATNode = lineItemNode.OwnerDocument.CreateNode(XmlNodeType.Element, "DiscountWithVAT", string.Empty);
            discountWithVATNode.InnerText = XmlCommon.XmlEncode((displayPrice - displayExtPrice).ToString(CultureInfo.InvariantCulture));
            // insert these nodes on the bottom
            lineItemNode.InsertAfter(discountWithVATNode, lineItemNode.LastChild);

            // store the line item discounts
            allLineItemDiscounts += XmlCommon.XmlFieldNativeDecimal(lineItemNode, "DiscountAmount");

        }

        bool hasLineItemDiscounts = false;

        hasLineItemDiscounts = allLineItemDiscounts > decimal.Zero;

        XmlNode hasLineItemDiscountsNode = orderInfoNode.OwnerDocument.CreateNode(XmlNodeType.Element, "HasLineItemDiscounts", string.Empty);
        hasLineItemDiscountsNode.InnerText = XmlCommon.XmlEncode(hasLineItemDiscounts.ToString(CultureInfo.InvariantCulture));
        orderInfoNode.InsertAfter(hasLineItemDiscountsNode, orderInfoNode.LastChild);
    }
    #endregion

    #region ParseAmount
    /// <summary>
    /// Parses the amount string to extract decimal amount values
    /// </summary>
    /// <param name="stringAmount">The amount string</param>
    /// <param name="nfo">The culture to base on</param>
    /// <returns>The parsed amount</returns>
    private decimal ParseAmount(string stringAmount, CultureInfo nfo)
    {
        decimal amount = decimal.Zero;
        string parsedAmount = string.Empty;

        char decimalSeparator = nfo.NumberFormat.CurrencyDecimalSeparator.ToCharArray()[0];

        foreach (Char c in stringAmount)
        {
            bool charIsDecimalSeparator = (c == decimalSeparator);
            bool charIsDecimalSeparatorButDoesNotPrecedeAnyNumbers = (charIsDecimalSeparator && parsedAmount.Length > 0 && char.IsNumber(parsedAmount[0]));
            bool charIsDecimalSepratorButInclusiveIsAlreadyDefined = (charIsDecimalSeparator && parsedAmount.IndexOf(decimalSeparator) < 0);

            if (char.IsNumber(c) ||
                (charIsDecimalSeparator &&
                charIsDecimalSeparatorButDoesNotPrecedeAnyNumbers &&
                charIsDecimalSepratorButInclusiveIsAlreadyDefined))
            {
                parsedAmount += c;
            }
        }

        decimal.TryParse(parsedAmount, NumberStyles.Currency, nfo, out amount);

        return amount;
    }
    #endregion

    #region AttachOrderOptionsXml
    /// <summary>
    /// Attaches the Order Options text as Xml element contained in the OrderInfo Node
    /// </summary>
    /// <param name="nav">The XPathNavigator</param>
    private void AttachOrderOptionsXml(XPathNavigator nav)
    {
        XmlNode orderInfoNode = GetXmlNode(nav.SelectSingleNode("Order/OrderInfo"));

        CultureInfo culture = new CultureInfo(ThisCustomer.LocaleSetting);

        try
        {
            XmlNode orderOptionsNode = orderInfoNode.SelectSingleNode("OrderOptions");
            if (null == orderOptionsNode) return;
            XmlDocument doc = orderInfoNode.OwnerDocument;

            string orderOptions = orderOptionsNode.InnerText;

            XmlNode orderOptionsXml = doc.CreateNode(XmlNodeType.Element, "OrderOptionsXml", string.Empty);

            if (!string.IsNullOrEmpty(orderOptions))
            {
                string[] orderOptionDelimitedValues = orderOptions.Split('^');
                foreach (string orderOptionDelimitedValue in orderOptionDelimitedValues)
                {
                    string[] orderOptionValues = orderOptionDelimitedValue.Split('|');
                    if (orderOptionValues != null && orderOptionValues.Length > 0)
                    {
                        int id = int.Parse(orderOptionValues[0]);
                        Guid uniqueID = new Guid(orderOptionValues[1]);
                        string name = orderOptionValues[2];

                        string priceFormatted = orderOptionValues[3];
                        // NOTE:
                        // Since the order options are attached to the order as a | delimited string
                        // and the price and tax amounts are already hardcoded as strings together
                        // with their currency symbols, we need to extract only the numeric values
                        decimal price = ParseAmount(priceFormatted, culture);
                        // since the order option is saved as one whole string
                        // the price saved here is already converted into the target curency format
                        // we'll need to revert to the original currency setting so to display properly especially on different currencies
                        price = Currency.Convert(price, ThisCustomer.CurrencySetting, Localization.GetPrimaryCurrency());

                        string extPriceFormatted = priceFormatted;

                        bool withVat = orderOptionValues.Length >= 4;
                        string vatFormatted = string.Empty;
                        decimal vat = decimal.Zero;
                        if (withVat)
                        {
                            vatFormatted = orderOptionValues[4];
                            vat = ParseAmount(vatFormatted, culture);
                        }

                        XmlNode orderOptionNode = doc.CreateNode(XmlNodeType.Element, "OrderOption", string.Empty);

                        // the details
                        XmlNode idNode = doc.CreateNode(XmlNodeType.Element, "ID", string.Empty);
                        XmlNode nameNode = doc.CreateNode(XmlNodeType.Element, "ProductName", string.Empty);
                        XmlNode priceNode = doc.CreateNode(XmlNodeType.Element, "Price", string.Empty);
                        XmlNode vatNode = doc.CreateNode(XmlNodeType.Element, "VAT", string.Empty);
                        XmlNode imageUrlNode = doc.CreateNode(XmlNodeType.Element, "ImageUrl", string.Empty);

                        idNode.InnerText = XmlCommon.XmlEncode(id.ToString());
                        nameNode.InnerXml = XmlCommon.XmlEncode(name); // NOTE: this value may be localized, make sure to call GetMLValue on the xml package!!!                        
                        priceNode.InnerText = XmlCommon.XmlEncode(price.ToString());
                        vatNode.InnerText = XmlCommon.XmlEncode(vat.ToString());

                        // get the image info
                        string imgUrl = orderOptionValues[5];

                        if (!string.IsNullOrEmpty(CommonLogic.ServerVariables("HTTP_HOST")))
                        {
                            imgUrl = "http://" + CommonLogic.ServerVariables("HTTP_HOST") + imgUrl;
                        }

                        imageUrlNode.InnerText = XmlCommon.XmlEncode(imgUrl);

                        orderOptionNode.AppendChild(idNode);
                        orderOptionNode.AppendChild(nameNode);
                        orderOptionNode.AppendChild(priceNode);
                        orderOptionNode.AppendChild(vatNode);
                        orderOptionNode.AppendChild(imageUrlNode);

                        orderOptionsXml.AppendChild(orderOptionNode);
                    }
                }
            }

            orderInfoNode.InsertAfter(orderOptionsXml, orderInfoNode.LastChild);
        }
        catch { }
    }
    #endregion

    #region Debug
    /// <summary>
    /// Generate runtime receipt xml structure for display on a textbox for debugging purposes
    /// </summary>
    /// <param name="root">The root element</param>
    /// <returns>Html textbox containing the xml</returns>
    public string Debug(IXPathNavigable root)
    {
        StringBuilder html = new StringBuilder();

        if (CommonLogic.QueryStringBool("debug"))
        {
            html.AppendFormat("<textarea cols=\"100\" rows=\"80\" >\n");
            XPathNavigator nav = root.CreateNavigator();
            html.Append(nav.OuterXml);
            html.AppendFormat("</textarea>\n");
        }

        return html.ToString();
    }
    #endregion

    #region PadWithCompare
    /// <summary>
    /// Pads the string to match the length of the parameter to compare to
    /// </summary>
    /// <param name="compareTo">The string to compare and match length with</param>
    /// <param name="compareWith">The string to pad</param>
    /// <param name="padding">The padding character string</param>
    /// <returns>The padded character</returns>
    public string PadWithCompare(string compareTo, string compareWith, string padding)
    {
        return compareWith.PadLeft(compareTo.Length, padding.ToCharArray()[0]);
    }
    #endregion

    #region EvalDecimal
    /// <summary>
    /// Evaluate the input as decimal value using the current thread's locale
    /// </summary>
    /// <param name="sDecimal"></param>
    /// <returns></returns>
    public decimal EvalDecimal(string sDecimal)
    {
        return Localization.ParseNativeDecimal(sDecimal);
    }
    #endregion

    #region StringContains
    /// <summary>
    /// Evaluates whether the input parameter 1 contains the input parameter 2
    /// </summary>
    /// <param name="s1">Input parameter 1</param>
    /// <param name="s2">Input parameter 2</param>
    /// <returns></returns>
    public bool StringContains(string s1, string s2)
    {
        return s1.IndexOf(s2, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }
    #endregion

    #endregion

}
