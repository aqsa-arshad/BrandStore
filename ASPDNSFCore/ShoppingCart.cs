// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AspDotNetStorefront.Promotions;
using AspDotNetStorefrontCore.CanadaPost;
using AspDotNetStorefrontCore.ShippingCalculation;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// The enumeration types for Cart Types.
    /// </summary>    
    public enum CartTypeEnum
    {
        ShoppingCart = 0,
        WishCart = 1,
        RecurringCart = 2,
        GiftRegistryCart = 3,
        Deleted = 101
    }

    /// <summary>
    /// The enumeration types for Variant Styles.
    /// </summary>
    public enum VariantStyleEnum
    {
        RegularVariantsWithAttributes = 0,
        ERPWithRollupAttributes = 1,
        ERPWithEachVariantListedInDropDown = 2,
        ERPWithEachVariantListedInTable = 3
    }

    /// <summary>
    /// The enumeration types for Date Interval
    /// </summary>
    public enum DateIntervalTypeEnum
    {
        Unknown = 0,
        Day = 1, // designed to be used with interval number also
        Week = 2, // designed to be used with interval number also
        Month = 3, // designed to be used with interval number also
        Year = 4, // designed to be used with interval number also
        Weekly = -1, // used by PayflowPRO recurring API
        BiWeekly = -2, // used by PayflowPRO recurring API
        EveryFourWeeks = -4, // used by PayflowPRO recurring API
        Monthly = -5, // used by PayflowPRO recurring API
        Quarterly = -6, // used by PayflowPRO recurring API
        SemiYearly = -7, // used by PayflowPRO recurring API
        Yearly = -8 // used by PayflowPRO recurring API
    }

    public class OrderOption
    {
        private int m_id;
        /// <summary>
        /// Gets or sets the ID. This field is only used for default db-saved order options
        /// </summary>
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private Guid m_uniqueid;
        /// <summary>
        /// Gets or sets the UniqueID. This is important when it comes to add-in as this will be used as reference
        /// </summary>
        public Guid UniqueID
        {
            get { return m_uniqueid; }
            set { m_uniqueid = value; }
        }

        private string m_name;
        /// <summary>
        /// Gets or sets the option name
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private string m_description;
        /// <summary>
        /// Gets or sets the option description
        /// </summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private decimal m_cost;
        /// <summary>
        /// Gets or sets the cost
        /// </summary>
        public decimal Cost
        {
            get { return m_cost; }
            set { m_cost = value; }
        }

        private decimal m_taxrate;
        /// <summary>
        /// Gets or sets the tax
        /// </summary>
        public decimal TaxRate
        {
            get { return m_taxrate; }
            set { m_taxrate = value; }
        }

        private bool m_defaultischecked;
        /// <summary>
        /// Gets or sets whether this is checked by default
        /// </summary>
        public bool DefaultIsChecked
        {
            get { return m_defaultischecked; }
            set { m_defaultischecked = value; }
        }

        private int m_displayorder;
        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder
        {
            get { return m_displayorder; }
            set { m_displayorder = value; }
        }

        private int m_taxclassid;
        /// <summary>
        /// Gets or sets the Tax Class ID
        /// </summary>
        public int TaxClassID
        {
            get { return m_taxclassid; }
            set { m_taxclassid = value; }
        }

        private string m_imageurl;
        /// <summary>
        /// Gets or sets the Image url
        /// </summary>
        public string ImageUrl
        {
            get { return m_imageurl; }
            set { m_imageurl = value; }
        }
    }

    public partial class ShoppingCart
    {
        #region Private Variables
        private int m_SkinID;
        private CartTypeEnum m_CartType;
        private Customer m_ThisCustomer;
        private CouponObject m_Coupon = new CouponObject();
        private String m_CouponStatus;
        private String m_EMail;
        private CartItemCollection m_CartItems = new CartItemCollection();
        private String m_DeleteString;

        private bool m_CustomerLevelAllowsQuantityDiscounts = false;

        private String m_CardName;
        private String m_CardType;
        private String m_CardNumber;
        private String m_CardExpirationMonth;
        private String m_CardExpirationYear;

        private bool m_ShippingMethodToStateMapIsEmpty;
        private bool m_ShippingMethodToCountryMapIsEmpty;
        private bool m_ShippingMethodToZoneMapIsEmpty;

        Shipping.ShippingCalculationEnum m_ShipCalcID = Shipping.GetActiveShippingCalculationID();

        private String m_OrderNotes;
        private String m_FinalizationData;

        //private List<int> m_OrderOptions;
        // contains all the order options from db and add-ins if availalbe
        private List<OrderOption> m_AllOrderOptions = new List<OrderOption>();

        // comma delimited string saved on the customer table which we'll base later which options are only selected
        private string m_customerSelectedOptions = string.Empty;

        // the selected options
        private List<OrderOption> m_OrderOptions = new List<OrderOption>();

        private bool m_MinimumQuantitiesUpdated;
        private bool m_RecurringScheduleConflict;
        private bool m_ContainsRecurringAutoShip;
        private bool m_OnlyLoadRecurringItemsThatAreDue;
        private int m_OriginalRecurringOrderNumber = 0;
        private String m_RecurringSubscriptionID = String.Empty;

        private Addresses m_CustAddresses = new Addresses();

        // WARNING THESE VALUES ARE ONLY FOR CACHING PERFORMANCE REASONS, they are set after first call in the object, and presumed
        // good for the life of "one" page:
        private Hashtable m_CachedTotals = new Hashtable();

        private Shipping.FreeShippingReasonEnum m_FreeShippingReason = Shipping.FreeShippingReasonEnum.DoesNotQualify;
        private String m_FreeShippingMethod = String.Empty;
        private decimal m_FreeShippingThreshold = 0.0M;
        private decimal m_MoreNeededToReachFreeShipping = 0.0M;
        private bool m_ShippingIsFree = false;
        private bool m_CartAllowsShippingMethodSelection = true;

        private bool m_PromotionsEnabled = false;
        private bool m_GiftCardsEnabled = false;

        SqlTransaction m_DBTrans = null;

        private bool m_VATEnabled = false;
        private bool m_VATOn = false;

        private bool _shippingThresHoldIsDefinedButFreeShippingMethodIDIsNot = false;

        private IList<IDiscountResult> m_discountResults { get; set; }

        #endregion

        #region Properties for Cartsummary

        // Use in estimates of shipping and tax computation for register customer
        private decimal _estimatedTotal;

        public bool VatEnabled
        {
            get { return m_VATEnabled; }
            set { m_VATEnabled = value; }
        }

        public bool VatIsInclusive
        {
            get { return m_VATOn; }
            set { m_VATOn = value; }
        }

        public decimal SubTotalRate
        {
            get { return SubTotal(false, false, true, true, true, true); }
        }

        public decimal SubTotalDiscountIncRate
        {
            get { return SubTotal(true, false, true, true, true, true); }
        }

        public string SubTotalRateDisplayFormat(Boolean includeTax)
        {
            decimal dTotal = SubTotal(true, false, true, true, true, true);
            if (this.Coupon.CouponType == CouponTypeEnum.GiftCard)
            {
                decimal GiftCardDiscountConverted = decimal.Round(Currency.Convert(Coupon.DiscountAmount, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                return Localization.CurrencyStringForDisplayWithExchangeRate(CommonLogic.IIF((dTotal - GiftCardDiscountConverted) < 0, 0, dTotal - GiftCardDiscountConverted), m_ThisCustomer.CurrencySetting);
            }
            else
                return Localization.CurrencyStringForDisplayWithExchangeRate(dTotal, ThisCustomer.CurrencySetting);
        }

        public decimal FreightRate
        {
            get { return ShippingTotal(true, false); }
        }

        public decimal FreightVatIncRate
        {
            get { return ShippingTotal(true, true); }
        }

        public string FreightRateDisplayFormat
        {
            get
            {
                return FormatFreightRate(FreightRate);
            }
        }

        public string FreightVatIncRateDisplayFormat
        {
            get
            {
                return Localization.CurrencyStringForDisplayWithExchangeRate(FreightVatIncRate, ThisCustomer.CurrencySetting);
            }
        }

        public decimal TaxRate
        {
            get { return Prices.TaxTotal(ThisCustomer, CartItems, FreightRate, OrderOptions); }
        }

        /// <summary>
        /// Gets the estimated total for register customer only
        /// </summary>
        /// <value>The estimated total.</value>
        public decimal EstimatedTotal
        {
            get
            {
                if (VatIsInclusive)
                {
                    _estimatedTotal = SubTotalDiscountIncRate + EstimatedShippingTotal;
                }
                else
                {
                    _estimatedTotal = SubTotalDiscountIncRate + EstimatedShippingTotal + TaxTotalForLineItem;
                }

                if (Coupon.m_coupontype == CouponTypeEnum.GiftCard)
                {
                    if (_estimatedTotal > Coupon.m_discountamount)
                    {
                        _estimatedTotal -= Coupon.m_discountamount;
                    }
                    else
                    {
                        _estimatedTotal = decimal.Zero;
                    }
                }

                return _estimatedTotal;
            }
        }

        public string EstimatedTotalDisplay
        {
            get
            {
                return Localization.CurrencyStringForDisplayWithExchangeRate(EstimatedTotal, ThisCustomer.CurrencySetting);
            }
        }

        public decimal TaxTotalForLineItem
        {
            get
            {
                return Prices.TaxTotal(ThisCustomer, CartItems, Prices.ShippingTotal(true, false, CartItems, ThisCustomer, OrderOptions), OrderOptions);
            }
        }

        public decimal EstimatedTotalTax
        {
            get
            {
                return TaxTotalForLineItem + EstimatedShippingTax;
            }
        }

        public string EstimateTotalTaxDisplay
        {
            get
            {
                return Localization.CurrencyStringForDisplayWithExchangeRate(EstimatedTotalTax, ThisCustomer.CurrencySetting);

            }
        }

        /// <summary>
        /// Gets the estimated shipping base on the lowest freight for register customer only.
        /// </summary>
        /// <value>The estimated shipping.</value>
        public decimal EstimatedShippingTotal
        {
            get
            {
                return System.Decimal.Zero;
            }
        }

        public string EstimatedShippingTotalDisplay
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the estimated shipping tax base on the lowest freight.
        /// </summary>
        /// <value>The estimated shipping tax.</value>
        public decimal EstimatedShippingTax
        {
            get
            {
                return System.Decimal.Zero;
            }
        }

        /// <summary>
        /// Gets the lowest freight.
        /// </summary>
        /// <value>The lowest freight.</value>
        public decimal LowestFreight
        {
            get
            {
                return System.Decimal.Zero;
            }
        }

        public string LowestFreightDisplay
        {
            get
            {
                return Localization.CurrencyStringForDisplayWithExchangeRate(LowestFreight, ThisCustomer.CurrencySetting);
            }
        }

        public string LowestfreightName
        {
            get
            {
                return String.Empty;
            }
        }

        public string TaxRateDisplayFormat
        {
            get
            {
                return Localization.CurrencyStringForDisplayWithExchangeRate(TaxRate, ThisCustomer.CurrencySetting);
            }
        }

        public decimal GetTotalRate(Boolean includeDiscounts, Boolean includeTax)
        {
            return Prices.Total(includeDiscounts, CartItems, ThisCustomer, OrderOptions, includeTax);
        }

        public decimal GetTotalRate()
        {
            return Prices.Total(true, CartItems, ThisCustomer, OrderOptions, true);
        }

        public string TotalRateDisplayFormat
        {
            get
            {
                string total = string.Empty;
                if (this.Coupon.CouponType == CouponTypeEnum.GiftCard)
                {
                    decimal GiftCardDiscountConverted = decimal.Round(Currency.Convert(Coupon.DiscountAmount, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                    total = Localization.CurrencyStringForDisplayWithExchangeRate(CommonLogic.IIF((GetTotalRate() - GiftCardDiscountConverted) < 0, 0, GetTotalRate() - GiftCardDiscountConverted), m_ThisCustomer.CurrencySetting);
                }
                else
                {
                    total = Localization.CurrencyStringForDisplayWithExchangeRate(GetTotalRate(), ThisCustomer.CurrencySetting);
                }

                return total;
            }
        }

        public Decimal GiftCardTotal
        {
            get
            {
                Decimal discount = decimal.Round(Currency.Convert(Coupon.DiscountAmount, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                return CommonLogic.IIF((GetTotalRate() - discount) < 0, GetTotalRate(), discount);
            }
        }

        public string GiftCardTotalDisplayFormat
        {
            get
            {
                return Localization.CurrencyStringForDisplayWithExchangeRate(GiftCardTotal, m_ThisCustomer.CurrencySetting);
            }
        }

        public bool AllFreeShippingComponents
        {
            get { return IsAllFreeShippingComponents(); }
        }

        public bool AllSystemComponents
        {
            get { return IsAllSystemComponents(); }
        }

        public bool WithTaxableComponents
        {
            get { return HasTaxableComponents(); }
        }

        public bool WithMultipleShippingAddresses
        {
            get { return this.HasMultipleShippingAddresses(); }
        }

        public bool WithGiftRegistryConponents
        {
            get { return this.HasGiftRegistryComponents(); }
        }

        public CartItem FirstCartItem
        {
            get { return m_CartItems[0]; }
        }

        public IList<IDiscountResult> DiscountResults
        {
            get { return m_discountResults; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCart"/> class.
        /// </summary>
        /// <param name="SkinID">The skinID.</param>
        /// <param name="ThisCustomer">The customer.</param>
        /// <param name="CartType">Type of the cart.</param>
        /// <param name="OriginalRecurringOrderNumber">The original recurring order number.</param>
        /// <param name="OnlyLoadRecurringItemsThatAreDue">if set to <c>true</c> [only load recurring items that are due].</param>
        public ShoppingCart(int SkinID, Customer ThisCustomer, CartTypeEnum CartType, int OriginalRecurringOrderNumber, bool OnlyLoadRecurringItemsThatAreDue)
            : this(null, SkinID, ThisCustomer, CartType, OriginalRecurringOrderNumber, OnlyLoadRecurringItemsThatAreDue)
        { }

        public ShoppingCart(SqlTransaction DBTrans, int SkinID, Customer ThisCustomer, CartTypeEnum CartType, int OriginalRecurringOrderNumber, bool OnlyLoadRecurringItemsThatAreDue)
            : this(null, SkinID, ThisCustomer, CartType, OriginalRecurringOrderNumber, OnlyLoadRecurringItemsThatAreDue, false) { }

        public ShoppingCart(int SkinID, Customer ThisCustomer, CartTypeEnum CartType, int OriginalRecurringOrderNumber, bool OnlyLoadRecurringItemsThatAreDue, bool IsAjaxMinicart)
            : this(null, SkinID, ThisCustomer, CartType, OriginalRecurringOrderNumber, OnlyLoadRecurringItemsThatAreDue, IsAjaxMinicart) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCart"/> class.
        /// </summary>
        /// <param name="DBTrans">The DB trans.</param>
        /// <param name="SkinID">The skinID.</param>
        /// <param name="ThisCustomer">The customer.</param>
        /// <param name="CartType">Type of the cart.</param>
        /// <param name="OriginalRecurringOrderNumber">The original recurring order number.</param>
        /// <param name="OnlyLoadRecurringItemsThatAreDue">if set to <c>true</c> [only load recurring items that are due].</param>
        public ShoppingCart(SqlTransaction DBTrans, int SkinID, Customer ThisCustomer, CartTypeEnum CartType, int OriginalRecurringOrderNumber, bool OnlyLoadRecurringItemsThatAreDue, bool IsAjaxMiniCart)
        {
            m_DBTrans = DBTrans;
            m_VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            m_VATOn = (m_VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

            m_SkinID = SkinID;
            m_ThisCustomer = ThisCustomer;
            m_CartType = CartType;
            m_OriginalRecurringOrderNumber = OriginalRecurringOrderNumber;
            if (m_OriginalRecurringOrderNumber != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    string query = "Select distinct RecurringSubscriptionID from ShoppingCart  with (NOLOCK)  where OriginalRecurringOrderNumber=" + m_OriginalRecurringOrderNumber.ToString();
                    using (IDataReader rsy = DB.GetRS(query, con))
                    {
                        if (rsy.Read())
                        {
                            m_RecurringSubscriptionID = DB.RSField(rsy, "RecurringSubscriptionID");
                        }
                    }
                }
            }
            m_OnlyLoadRecurringItemsThatAreDue = OnlyLoadRecurringItemsThatAreDue;
            m_DeleteString = AppLogic.GetString("shoppingcart.cs.107", m_SkinID, m_ThisCustomer.LocaleSetting);
            if (m_DeleteString.Length == 0)
            {
                m_DeleteString = "Delete";
            }

            m_ShippingMethodToStateMapIsEmpty = Shipping.ShippingMethodToStateMapIsEmpty();
            m_ShippingMethodToCountryMapIsEmpty = Shipping.ShippingMethodToCountryMapIsEmpty();
            m_ShippingMethodToZoneMapIsEmpty = Shipping.ShippingMethodToZoneMapIsEmpty();

            if (CommonLogic.GetThisPageName(false).Equals("shoppingcart.aspx", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                bool cleanUp = true;

                if (AppLogic.AppConfigBool("ShowShippingAndTaxEstimate") &&
                    CommonLogic.GetThisPageName(false).Equals("action.axd"))
                {
                    cleanUp = false;
                }
                if (cleanUp)
                {
                    // if NOT on the cart page itself, remove any quantity 0 items from the cart:
                    // the cart page itself MAY NEED 0 quantity items with an existing ShoppingCartRecID, which is why we are not cleaning those on the cart page itself
                    DB.ExecuteSQL("delete from ShoppingCart where CustomerID=" + ThisCustomer.CustomerID.ToString() + " and Quantity=0", m_DBTrans);
                    DB.ExecuteSQL("delete from KitCart where CustomerID=" + ThisCustomer.CustomerID.ToString() + " and ShoppingCartRecID<>0 and ShoppingCartRecID not in (select ShoppingCartRecID from ShoppingCart  with (NOLOCK)  where CustomerID=" + ThisCustomer.CustomerID.ToString() + ")", m_DBTrans);

                    // Delete any item in the cart whose product start or stop date is no longer valid.
                    DB.ExecuteSQL("delete from ShoppingCart where ShoppingCartRecId in (select ShoppingCartRecId from ShoppingCart s inner join Product p on p.ProductId = s.ProductId where CustomerId = " + ThisCustomer.CustomerID.ToString() + " and ((p.AvailableStartDate is not null and p.AvailableStartDate > getdate()) or (p.AvailableStopDate is not null and p.AvailableStopDate < getdate())))", m_DBTrans);
                }
            }

            if (AppLogic.AppConfigBool("Promotions.Enabled"))
            {
                if (ThisCustomer.CustomerID == 0 || AppLogic.CustomerLevelAllowsCoupons(m_DBTrans, m_ThisCustomer.CustomerLevelID))
                {
                    m_PromotionsEnabled = true;
                }
            }

            m_GiftCardsEnabled = AppLogic.AppConfigBool("GiftCards.Enabled");

            LoadFromDB(CartType);

            if (!m_GiftCardsEnabled && m_Coupon.m_couponcode.Length != 0)
            {
                DB.ExecuteSQL("update Customer set CouponCode=NULL where CustomerID=" + ThisCustomer.CustomerID.ToString(), m_DBTrans);
            }

            m_CouponStatus = Coupons.CheckIfCouponIsValidForOrder(ThisCustomer, m_Coupon, SubTotal(true, false, true, true), SubTotal(false, false, true, true));

            AnalyzeCartForFreeShippingConditions(this.FirstItemShippingAddressID());

            AddRequiredProductsToCart();

            if (AppLogic.MicropayIsEnabled() == false && CommonLogic.GetThisPageName(false).Equals("shoppingcart.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (CartItem ci in CartItems.Where(ci => ci.ProductID == AppLogic.MicropayProductID))
                {
                    this.RemoveItem(ci.ShoppingCartRecordID, false);
                    CartItems.Remove(ci);
                }
            }

            //Need to calculate the discount then reload in case we've added an item to the cart in our promo calculation (fixes UI being one step behind the DB)
            RecalculateCartDiscount();
            LoadFromDB(CartType);

            //V4_0 If A SKU=MICROPAY product exists make sure it is in the cart as the last item, but only IF we are on the shopping cart page
            if (AppLogic.MicropayIsEnabled() &&
                !AppLogic.AppConfigBool("MicroPay.HideOnCartPage") &&
                CommonLogic.GetThisPageName(false).Equals("shoppingcart.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                if (AppLogic.MicropayProductID != 0 && !this.HasMicropayProduct())
                {
                    this.AddItem(m_ThisCustomer, 0, AppLogic.MicropayProductID, AppLogic.MicropayVariantID, 0, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, CartTypeEnum.ShoppingCart, true, false, 0, System.Decimal.Zero);
                }

                // Make sure Micropay is moved to the end of the list.
                foreach (CartItem item in m_CartItems.Where(ci => ci.ProductID == AppLogic.MicropayProductID))
                {
                    m_CartItems.Remove(item);
                    m_CartItems.Add(item);
                    break;
                }
            }
        }

        public void AddRequiredProductsToCart()
        {
            String ProductRequires = String.Empty;

            //Make sure Required items are added to the cart if needed
            foreach (CartItem item in m_CartItems)
            {
                //Collect all the requires in the cart as one string.
                string tmpS = AppLogic.GetRequiresProducts(item.ProductID);
                if (tmpS.Trim().Length != 0)
                {
                    for (int i = 1; i <= item.Quantity; i++)
                    {
                        ProductRequires += CommonLogic.IIF(((ProductRequires.Length != 0) && (tmpS.Length != 0)), ",", "") + tmpS;
                    }
                }
            }

            if (ProductRequires.Length != 0 && m_CartType != CartTypeEnum.RecurringCart) // Don't add required products if this is a recurring cart
            {
                String[] ssplit = ProductRequires.Split(',');
                Hashtable ht = new Hashtable();
                foreach (String s in ssplit)
                {
                    if (ht.Contains(s))
                    {
                        int n = (int)ht[s];
                        n++;
                        ht[s] = n;
                    }
                    else
                    {
                        ht.Add(s, 1);
                    }
                }
                // ht now contains PID,sum(pid) that are required in the cart
                foreach (DictionaryEntry de in ht)
                {
                    try
                    {
                        int PID = Localization.ParseUSInt(de.Key.ToString());
                        int VID = AppLogic.GetDefaultProductVariant(PID, true);
                        int deltaN = (int)de.Value - Count(PID);
                        if (deltaN > 0 && !AppLogic.IsAKit(PID))
                        {
                            AddItem(ThisCustomer, ThisCustomer.PrimaryShippingAddressID, PID, VID, deltaN, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, CartType, false, true, 0, System.Decimal.Zero);
                        }
                    }
                    catch { }
                }
                LoadFromDB(CartType);
            }
        }

        public bool ShippingIsAllValid()
        {
            if (this.Coupon != null && this.Coupon.DiscountIncludesFreeShipping)
                return true;

            if (AppLogic.AppConfigBool("SkipShippingOnCheckout") || this.FreeShippingReason != Shipping.FreeShippingReasonEnum.DoesNotQualify || IsAllEmailGiftCards() || NoShippingRequiredComponents())
            {
                return true;
            }

            return Shipping.ShippingIsAllValid(CartItems);
        }

        /// <summary>
        /// Gets the product info.
        /// </summary>
        /// <param name="ShoppingCartRecID">The ShoppingCartRecID.</param>
        /// <param name="ProductID">The productID.</param>
        /// <param name="VariantID">The variantID.</param>
        /// <param name="ChosenColor">Color of the chosen.</param>
        /// <param name="ChosenSize">Size of the chosen.</param>
        /// <param name="TextOption">The text option.</param>
        public void GetProductInfo(int ShoppingCartRecID, out int ProductID, out int VariantID, out String ChosenColor, out String ChosenSize, out String TextOption)
        {
            ProductID = 0;
            VariantID = 0;
            ChosenColor = String.Empty;
            ChosenSize = String.Empty;
            TextOption = String.Empty;
            foreach (CartItem c in m_CartItems)
            {
                if (c.ShoppingCartRecordID == ShoppingCartRecID)
                {
                    ProductID = c.ProductID;
                    VariantID = c.VariantID;
                    ChosenColor = c.ChosenColor;
                    ChosenSize = c.ChosenSize;
                    TextOption = c.TextOption;
                }
            }
        }

        /// <summary>
        /// Check if it is Valid to proceed check. If anything is not right, send back to the cart page!
        /// </summary>
        public void ValidProceedCheckout()
        {

            if (IsEmpty())
            {
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1");
            }

            //	Inventory Trimming
            if (InventoryTrimmed)
            {
                ErrorMessage e = new ErrorMessage(AppLogic.GetString("shoppingcart.aspx.3", SkinID, ThisCustomer.LocaleSetting));
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + e.MessageId);
            }

            //	Clearing Invalid Coupons
            if (HasCoupon() && !(m_CouponStatus == AppLogic.ro_OK))
            {
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1&discountvalid=false");
            }

            //	CartMinOrderAmount
            if (!MeetsMinimumOrderAmount(AppLogic.AppConfigUSDecimal("CartMinOrderAmount")))
            {
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1");
            }

            //	MinCartItemsBeforeCheckout
            if (!MeetsMinimumOrderQuantity(AppLogic.AppConfigUSInt("MinCartItemsBeforeCheckout")))
            {
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1");
            }

            //	MaxCartItemsBeforeCheckout
            if (ExceedsMaximumOrderQuantity(AppLogic.AppConfigUSInt("MaxCartItemsBeforeCheckout")))
            {
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1");
            }

            //	RecurringSchedule conflicts
            if (RecurringScheduleConflict)
            {
                ErrorMessage em = new ErrorMessage(AppLogic.GetString("shoppingcart.aspx.19", SkinID, ThisCustomer.LocaleSetting));
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + em.MessageId);
            }

        }

        /// <summary>
        /// Gets the Age of the Shopping Cart
        /// </summary>
        /// <param name="NumDays">The number of days.</param>
        /// <param name="CartType">Type of the cart.</param>
        public void Age(int NumDays, CartTypeEnum CartType)
        {
            if (NumDays > 0)
            {
                ShoppingCart.Age(m_ThisCustomer.CustomerID, NumDays, CartType);
                LoadFromDB(CartType);
            }
        }

        /// <summary>
        /// Gets the Age of the ShoppingCart, KitCart and delete it. Except for Recurring Cart.
        /// </summary>
        /// <param name="CustomerID">The customerID.</param>
        /// <param name="NumDays">The number days.</param>
        /// <param name="CartType">Type of the cart.</param>
        public static void Age(int CustomerID, int NumDays, CartTypeEnum CartType)
        {
            if (NumDays > 0 && CustomerID != 0)
            {
                // remember, you can't "age" the recurring cart:
                if (CustomerID != 0 && CartType != CartTypeEnum.RecurringCart)
                {
                    String AgeDate = Localization.ToDBShortDateString(System.DateTime.Now.AddDays(-NumDays));
                    DB.ExecuteSQL("delete from kitcart where CartType=" + ((int)CartType).ToString() + " and CustomerID=" + CustomerID.ToString() + " and CreatedOn<" + DB.DateQuote(AgeDate));
                    DB.ExecuteSQL("delete from ShoppingCart where CartType=" + ((int)CartType).ToString() + " and CustomerID=" + CustomerID.ToString() + " and CreatedOn<" + DB.DateQuote(AgeDate));
                }

                // now clear out any "deleted" products:
                // NOTE: we are also cleaning out "recurring" items that are deleted
                // make sure we don't have "JUST" deleted or unpublished items hanging around
                ClearDeletedAndUnPublishedProducts(CustomerID);
            }
        }

        /// <summary>
        /// Clears the deleted and unpublished products.
        /// </summary>
        /// <param name="CustomerID">The CustomerID.</param>
        public static void ClearDeletedAndUnPublishedProducts(int CustomerID)
        {
            string clearKitCartSql = string.Format(
                                            @"DELETE kc
                                                FROM KitCart kc WITH (NOLOCK)
                                                INNER JOIN Product p WITH (NOLOCK) ON p.productid = kc.productid
                                                INNER JOIN ProductVariant pv WITH (NOLOCK) ON kc.variantid = pv.variantid
                                                WHERE kc.CustomerID = {0} AND (p.Published = 0 OR pv.Published = 0 OR p.Deleted = 1 OR pv.Deleted = 1)", CustomerID);

            string clearShoppingCartSql = string.Format(
                                            @"DELETE sc
                                                FROM ShoppingCart sc WITH (NOLOCK)
                                                INNER JOIN Product p WITH (NOLOCK) ON p.productid = sc.productid
                                                INNER JOIN ProductVariant pv WITH (NOLOCK) ON sc.variantid = pv.variantid
                                                WHERE sc.CustomerID = {0} AND (p.Published = 0 OR pv.Published = 0 OR p.Deleted = 1 OR pv.Deleted = 1)", CustomerID);

            DB.ExecuteSQL(clearKitCartSql);
            DB.ExecuteSQL(clearShoppingCartSql);
        }

        public bool IsEmpty(int CustomerID, CartTypeEnum CartType)
        {
            return (NumItems(CustomerID, CartType) == 0);
        }

        /// <summary>
        /// Determine if the cart is empty.
        /// </summary>
        /// <param name="CustomerID">The CustomerID.</param>
        /// <param name="CartType">Type of the cart.</param>
        /// <returns>Returns TRUE if the cart is empty otherwise FALSE.</returns>
        public static bool CartIsEmpty(int CustomerID, CartTypeEnum CartType)
        {
            return DB.GetSqlN("select count(*) as N from ShoppingCart   with (NOLOCK)  where CartType=" + ((int)CartType).ToString() + " and customerid=" + CustomerID.ToString()) == 0;
        }

        /// <summary>
        /// Total quantity of CartItems.
        /// </summary>
        /// <param name="CustomerID">The CustomerID.</param>
        /// <param name="CartType">Type of the cart.</param>
        /// <returns>Returns the total quantity of the cart.</returns>
        static public int NumItems(int CustomerID, CartTypeEnum CartType)
        {
            int N = 0;
            if (CustomerID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();

                    // Only query items not are available (Published and Deleted = 0)
                    string query = string.Format(
                                    @"SELECT SUM(sc.Quantity) AS NumItems FROM ShoppingCart sc WITH (NOLOCK)
                                    INNER JOIN Product p WITH (NOLOCK) ON p.productid = sc.productid
                                    INNER JOIN ProductVariant pv WITH (NOLOCK) ON sc.variantid = pv.variantid
                                    INNER JOIN (SELECT DISTINCT a.ProductID,a.StoreID FROM ShoppingCart a with (NOLOCK) LEFT JOIN ProductStore b with (NOLOCK) on a.ProductID = b.ProductID where ({0} = 0 or b.StoreID = a.StoreID)) productstore
                                    on sc.ProductID = productstore.ProductID and sc.StoreID = productstore.StoreID
                                    WHERE sc.CustomerID = {1} AND sc.CartType = {2} AND ((p.Published = 1 AND pv.Published = 1 AND p.Deleted = 0 AND pv.Deleted = 0 AND p.AvailableStopDate IS NULL OR p.AvailableStopDate > GetDate())) and ({3} = 0 or sc.StoreID = {4})", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowProductFiltering") == true, 1, 0), CustomerID, ((int)CartType), CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowShoppingcartFiltering") == true, 1, 0), AppLogic.StoreID());

                    using (IDataReader rs = DB.GetRS(query, con))
                    {
                        rs.Read();
                        N = DB.RSFieldInt(rs, "NumItems");
                    }
                }
            }
            return N;
        }

        /// <summary>
        /// Checks the inventory and remove the item being added if the order exceeds the inventory size.
        /// </summary>
        /// <param name="CustomerID">The CustomerID.</param>
        /// <returns>Returns boolean.</returns>
        public bool CheckInventory(int CustomerID)
        {
            InventoryTrimmedReason r;
            return CheckInventory(CustomerID, out r);
        }

        public bool CheckInventory(int CustomerID, out InventoryTrimmedReason Reason)
        {
            Reason = InventoryTrimmedReason.None;
            bool invTrimmed = DoCheckInventory(CustomerID);

            if (!invTrimmed)
            {
                if (MaintainRestrictedQuantities())
                {
                    Reason = InventoryTrimmedReason.RestrictedQuantities;
                    invTrimmed = true;
                }
            }

            if (!invTrimmed)
            {
                invTrimmed = CheckMinimumQuantities(CustomerID);
                if (invTrimmed)
                    Reason = InventoryTrimmedReason.MinimumQuantities;
            }


            else
            {
                Reason = InventoryTrimmedReason.InventoryLevels;
            }

            _TrimmedReason = Reason;

            return invTrimmed;
        }

        private bool MaintainRestrictedQuantities()
        {
            bool quanChanged = false;
            for (int i = 0; i < this.CartItems.Count; i++)
            {
                CartItem ci = this.CartItems[i];

                if (ci.RestrictedQuantities.Count > 1 && !ci.RestrictedQuantities.Contains(ci.Quantity))
                {
                    int newQuantity = 0;
                    int delta = ci.Quantity;
                    foreach (int allowedQuantity in ci.RestrictedQuantities)
                    {
                        if (allowedQuantity < ci.Quantity && ci.Quantity - allowedQuantity < delta)
                        {
                            newQuantity = allowedQuantity;
                            delta = ci.Quantity - allowedQuantity;
                        }
                    }
                    this.SetItemQuantity(ci.ShoppingCartRecordID, newQuantity);
                    quanChanged = true;
                }
            }
            return quanChanged;
        }

        private bool DoCheckInventory(int CustomerID)
        {
            bool ITrimmed = false;

            if (AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand"))
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    string query = "select ShoppingCartRecID, sc.ProductID, sc.VariantID, sc.ChosenSize, sc.ChosenColor, sc.Quantity, p.TrackInventoryBySizeAndColor, p.TrackInventoryBySize, p.TrackInventoryByColor from dbo.ShoppingCart  sc  with (NOLOCK)  join dbo.Product p  with (NOLOCK)  on sc.ProductID = p.ProductID  where sc.CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and sc.customerid=" + CustomerID.ToString();
                    using (IDataReader rsi = DB.GetRS(query, con))
                    {
                        while (rsi.Read())
                        {
                            int ionhand = AppLogic.GetInventory(DB.RSFieldInt(rsi, "ProductID"), DB.RSFieldInt(rsi, "VariantID"), DB.RSField(rsi, "ChosenSize"), DB.RSField(rsi, "ChosenColor"), DB.RSFieldBool(rsi, "TrackInventoryBySizeAndColor"), DB.RSFieldBool(rsi, "TrackInventoryByColor"), DB.RSFieldBool(rsi, "TrackInventoryBySize"));
                            int cartRecId = DB.RSFieldInt(rsi, "ShoppingCartRecID");

                            if (DB.RSFieldInt(rsi, "Quantity") > ionhand)
                            {
                                ITrimmed = true;
                                if (ionhand <= 0)
                                {
                                    this.RemoveItem(cartRecId, false);
                                }
                                else
                                {
                                    this.SetItemQuantity(cartRecId, ionhand);
                                }
                            }
                        }
                    }
                }

                // We need to check again with items grouped together since they can be individual line items in the cart.
                // This is primarily to handle Kit items.
                bool GroupTrimmed = false;
                do
                {
                    bool GroupItemTrimmed = false;

                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        string query = "select sc.ProductID, sc.VariantID, sc.ChosenSize, sc.ChosenColor, p.TrackInventoryBySizeAndColor, p.TrackInventoryBySize, p.TrackInventoryByColor, sum(Quantity) as Quantity, max(ShoppingCartRecID) as ShoppingCartRecID from ShoppingCart sc  with (NOLOCK)   join dbo.Product p  with (NOLOCK)  on sc.ProductID = p.ProductID  where sc.CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and sc.customerid=" + CustomerID.ToString() + " group by sc.ProductID, sc.VariantID, sc.ChosenSize, sc.ChosenColor, p.TrackInventoryBySizeAndColor, p.TrackInventoryBySize, p.TrackInventoryByColor";
                        using (IDataReader rsi = DB.GetRS(query, con))
                        {
                            while (rsi.Read())
                            {
                                int ionhand = AppLogic.GetInventory(DB.RSFieldInt(rsi, "ProductID"), DB.RSFieldInt(rsi, "VariantID"), DB.RSField(rsi, "ChosenSize"), DB.RSField(rsi, "ChosenColor"), DB.RSFieldBool(rsi, "TrackInventoryBySizeAndColor"), DB.RSFieldBool(rsi, "TrackInventoryByColor"), DB.RSFieldBool(rsi, "TrackInventoryBySize"));
                                if (DB.RSFieldInt(rsi, "Quantity") > ionhand)
                                {
                                    ITrimmed = true;
                                    GroupItemTrimmed = true;

                                    int QtyExcess = DB.RSFieldInt(rsi, "Quantity") - ionhand;
                                    int QtyMostRecent = DB.GetSqlN("select Quantity N from ShoppingCart  with (NOLOCK)  where ShoppingCartRecID=" + DB.RSFieldInt(rsi, "ShoppingCartRecID").ToString());

                                    if (QtyExcess >= QtyMostRecent)
                                    {
                                        DB.ExecuteSQL("delete from ShoppingCart where ShoppingCartRecID=" + DB.RSFieldInt(rsi, "ShoppingCartRecID").ToString());
                                    }
                                    else
                                    {
                                        DB.ExecuteSQL("update ShoppingCart set Quantity=" + (QtyMostRecent - QtyExcess).ToString() + " where ShoppingCartRecID=" + DB.RSFieldInt(rsi, "ShoppingCartRecID").ToString());
                                    }
                                }
                            }
                        }
                    }

                    GroupTrimmed = GroupItemTrimmed;
                    // Keep looping through until we have a pass that does not trim any items.
                } while (GroupTrimmed);
            }

            return ITrimmed;
        }

        /// <summary>
        /// Checks the minimum quantities of the item and set it if it order exceeds.
        /// </summary>
        /// <param name="CustomerID">The CustomerID.</param>
        /// <returns>Returns boolean</returns>
        public bool CheckMinimumQuantities(int CustomerID)
        {
            bool QFixed = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                string query = "select ShoppingCart.ShoppingCartRecID, ShoppingCart.Quantity, ProductVariant.MinimumQuantity from ShoppingCart   with (NOLOCK)  left outer join productvariant  with (NOLOCK)  on shoppingcart.variantid=productvariant.variantid where CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and customerid=" + CustomerID.ToString();
                using (IDataReader rsi = DB.GetRS(query, con))
                {
                    while (rsi.Read())
                    {

                        int recID = DB.RSFieldInt(rsi, "ShoppingCartRecID");
                        int MinQ = DB.RSFieldInt(rsi, "MinimumQuantity");
                        if (MinQ > 0)
                        {
                            if (DB.RSFieldInt(rsi, "Quantity") < MinQ)
                            {
                                QFixed = true;
                                DB.ExecuteSQL(string.Format("update ShoppingCart set Quantity = {0} where ShoppingCartRecID= {1}", MinQ, recID));

                                for (int i = 0; i < m_CartItems.Count; i++)
                                {
                                    if (((CartItem)m_CartItems[i]).ShoppingCartRecordID == recID)
                                    {
                                        CartItem ci = (CartItem)m_CartItems[i];
                                        ci.MinimumQuantityUdpated = true;
                                        m_CartItems[i] = ci;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return QFixed;
        }

        static public CartTypeEnum CartTypeFromRecID(int CartRecId)
        {
            return (CartTypeEnum)DB.GetSqlN("select carttype N from dbo.shoppingcart where shoppingcartrecid = " + CartRecId.ToString());
        }

        /// <summary>
        ///  For now, this is only to synchronize the cart productsku base on the product sku,variant skusuffice and inventory Vendorfullsku
        /// </summary>
        private void Synchronize()
        {
            if (AppLogic.AppConfigBool("UseSKUForProductImageName"))
            {
                // Execute the SP that will update the productSKU of the shopping cart.
                // Scenarios happen when admin user update the sku of the product, the image base on the sku will not take  effect on the store site(unless he/she add an item to the cart) cause
                // its getting the Productsku from the shoppingCart table which is not yet been updated yet. That why we need to run this SP to synchronize the cart productsku
                DB.ExecuteSQL(string.Format("dbo.aspdnsf_SynchronizeCart {0} , {1}", ThisCustomer.CustomerID, (int)CartType));
            }
        }

        /// <summary>
        /// Loads the order options for this cart
        /// </summary>
        private void LoadOrderOptions()
        {
            if (this.CartType == CartTypeEnum.ShoppingCart)
            {
                m_AllOrderOptions.Clear();
                m_OrderOptions.Clear();

                LoadDefaultOptions();
                DetermineSelectedOptions();
            }
        }

        /// <summary>
        /// Reloads order options incase changes in computation..
        /// </summary>
        public void ReLoadDefaultOptions()
        {
            LoadOrderOptions();
        }

        /// <summary>
        /// Loads the db order options
        /// </summary>
        private void LoadDefaultOptions()
        {
            var sql = string.Format("SELECT A.* FROM OrderOption A WITH (NOLOCK) INNER JOIN (SELECT DISTINCT A.OrderOptionID FROM OrderOption A WITH (NOLOCK) LEFT JOIN OrderOptionStore B WITH (NOLOCK) ON A.OrderOptionID = B.OrderOptionID " +
                             "WHERE ({0} = 0 OR StoreID = {1})) B ON A.OrderOptionID = B.OrderOptionID ORDER BY A.DisplayOrder", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowOrderOptionFiltering") == true, 1, 0), AppLogic.StoreID());

            Action<IDataReader> readAction = (rs) =>
            {
                while (rs.Read())
                {
                    var option = new OrderOption();
                    option.ID = rs.FieldInt("OrderOptionID");
                    option.Name = rs.FieldByLocale("Name", ThisCustomer.LocaleSetting);
                    option.Description = rs.FieldByLocale("Description", ThisCustomer.LocaleSetting);
                    option.DisplayOrder = rs.FieldInt("DisplayOrder");
                    option.DefaultIsChecked = rs.FieldBool("DefaultIsChecked");

                    option.UniqueID = rs.FieldGuid("OrderOptionGuid");
                    option.TaxClassID = rs.FieldInt("TaxClassID");

                    var cost = rs.FieldDecimal("Cost");
                    option.Cost = cost;
                    option.TaxRate = Prices.OrderOptionVAT(ThisCustomer, cost, DB.RSFieldInt(rs, "TaxClassID"));

                    string imgUrl = AppLogic.LookupImage("OrderOption", option.ID, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    if (imgUrl.StartsWith("../"))
                    {
                        imgUrl = imgUrl.Replace("../", "");
                    }
                    option.ImageUrl = imgUrl;

                    m_AllOrderOptions.Add(option);
                }
            };

            // fetch db order options
            DB.UseDataReader(sql, readAction);
        }

        /// <summary>
        /// Determines which order options are selected by the customer
        /// </summary>
        private void DetermineSelectedOptions()
        {
            // determine selected
            var customerSelectedOptions = m_customerSelectedOptions.Split(',').Select(s => s.Trim());

            foreach (var selectedOptionId in customerSelectedOptions)
            {
                var matchingOption = m_AllOrderOptions.FirstOrDefault(opt => opt.UniqueID.ToString().EqualsIgnoreCase(selectedOptionId));
                if (matchingOption != null)
                {
                    // prevent duplicates
                    if (!m_OrderOptions.Contains(matchingOption))
                    {
                        m_OrderOptions.Add(matchingOption);
                    }
                }
            }
        }

        private void LoadFromDB(CartTypeEnum CartType)
        {
            //m_OrderOptions = new List<int>();

            m_MinimumQuantitiesUpdated = false;
            m_RecurringScheduleConflict = false;
            m_ContainsRecurringAutoShip = false;

            Synchronize();

            if (CartType == CartTypeEnum.ShoppingCart && m_ThisCustomer.CustomerID != 0)
            {
                // only do these on the shopping cart

                //m_MinimumQuantitiesUpdated = CheckMinimumQuantities(m_ThisCustomer.CustomerID);
                if (ThisCustomer.PrimaryShippingAddressID != 0)
                {
                    // repair addresses if required
                    // force all "0" or "invalid" address id records in the cart to be set to the primary shipping address. This is to avoid total confusion
                    // later in the cart where gift registry items, multi-ship items, etc, are all involved!
                    DB.ExecuteSQL("update shoppingcart set ShippingAddressID=" + ThisCustomer.PrimaryShippingAddressID.ToString() + " where CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString() + " and GiftRegistryForCustomerID=0 and (ShippingAddressID=0 or ShippingAddressID not in (select AddressID from Address  with (NOLOCK)  where CustomerID=" + m_ThisCustomer.CustomerID.ToString() + "))", m_DBTrans);
                }
            }

            // We don't support checking out with with multiple recurring (auto-ship) items 
            // when they result in differing shipment schedules.
            string RecurringVariantList = AppLogic.GetRecurringVariantsList();
            if (RecurringVariantList.Length > 0)
            {
                string sqlRecur = String.Format("SELECT COUNT(CustomerID) N FROM (SELECT CustomerID FROM ShoppingCart " +
                    "WHERE CartType={0} AND CustomerID={1} AND VariantID IN ({2}) AND StoreID = {3} " +
                    "GROUP BY CustomerID, RecurringInterval, RecurringIntervalType) T",
                    (int)CartType, m_ThisCustomer.CustomerID, RecurringVariantList, AppLogic.StoreID().ToString());
                int RecurringScheduleCount = DB.GetSqlN(sqlRecur);
                if (RecurringScheduleCount > 1)
                {
                    m_RecurringScheduleConflict = true;
                }
                if (RecurringScheduleCount > 0)
                {
                    m_ContainsRecurringAutoShip = true;
                }
            }

            m_CustomerLevelAllowsQuantityDiscounts = QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(m_ThisCustomer.CustomerLevelID);

            // Need to pull the Shipping Address from Address table
            StringBuilder sql = new StringBuilder(4096);
            sql.Append(String.Format("dbo.aspdnsf_GetShoppingCart {0}, {1}, {2}, {3}, {4}", ((int)CartType).ToString(), m_ThisCustomer.CustomerID.ToString(), m_OriginalRecurringOrderNumber.ToString(), CommonLogic.IIF(m_OnlyLoadRecurringItemsThatAreDue, 1, 0).ToString(), AppLogic.StoreID()));

            m_CartItems.Clear();
            m_CartItems.m_couponlist = new List<CouponObject>();
            m_CartItems.m_quantitydiscountlist = new List<QDObject>();
            //m_OrderOptions = new List<int>();

            int i = 0;

            Decimal LevelDiscountPercent = AppLogic.GetCustomerLevelDiscountPercent(m_ThisCustomer.CustomerLevelID);
            String RecurringProducts = "," + AppLogic.GetRecurringVariantsList() + ",";
            bool couponSet = false;

            m_EMail = String.Empty;
            m_OrderNotes = String.Empty;
            m_FinalizationData = String.Empty;
            m_CardName = String.Empty;
            m_CardType = String.Empty;
            m_CardNumber = String.Empty;
            m_CardExpirationMonth = String.Empty;
            m_CardExpirationYear = String.Empty;

            SqlConnection con = null;
            IDataReader rs = null;

            try
            {
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection                    
                    rs = DB.GetRS(sql.ToString(), m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(sql.ToString(), con);
                }

                using (rs)
                {
                    while (rs.Read())
                    {
                        CartItem newItem = new CartItem(this, ThisCustomer);
                        newItem.CreatedOn = DB.RSFieldDateTime(rs, "CreatedOn");
                        newItem.NextRecurringShipDate = DB.RSFieldDateTime(rs, "NextRecurringShipDate");
                        newItem.RecurringIndex = DB.RSFieldInt(rs, "RecurringIndex");
                        newItem.OriginalRecurringOrderNumber = DB.RSFieldInt(rs, "OriginalRecurringOrderNumber");
                        newItem.RecurringSubscriptionID = DB.RSField(rs, "RecurringSubscriptionID");
                        newItem.ShoppingCartRecordID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                        newItem.CartType = (CartTypeEnum)DB.RSFieldInt(rs, "CartType");
                        newItem.ProductID = DB.RSFieldInt(rs, "ProductID");
                        newItem.VariantID = DB.RSFieldInt(rs, "VariantID");
                        newItem.IsSystem = DB.RSFieldBool(rs, "IsSystem");
                        newItem.IsAKit = DB.RSFieldBool(rs, "IsAKit");
                        newItem.GiftRegistryForCustomerID = DB.RSFieldInt(rs, "GiftRegistryForCustomerID");
                        newItem.ProductName = DB.RSFieldByLocale(rs, "ProductName", m_ThisCustomer.LocaleSetting);
                        newItem.VariantName = DB.RSFieldByLocale(rs, "VariantName", m_ThisCustomer.LocaleSetting);
                        newItem.SKU = DB.RSField(rs, "ProductSKU");
                        newItem.ManufacturerPartNumber = DB.RSField(rs, "ManufacturerPartNumber");
                        newItem.Quantity = DB.RSFieldInt(rs, "Quantity");
                        newItem.RequiresCount = DB.RSFieldInt(rs, "RequiresCount");
                        newItem.ChosenColor = DB.RSFieldByLocale(rs, "ChosenColor", m_ThisCustomer.LocaleSetting);
                        newItem.ChosenColorSKUModifier = DB.RSField(rs, "ChosenColorSKUModifier");
                        newItem.ChosenSize = DB.RSFieldByLocale(rs, "ChosenSize", m_ThisCustomer.LocaleSetting);
                        newItem.ChosenSizeSKUModifier = DB.RSField(rs, "ChosenSizeSKUModifier");
                        newItem.TextOption = DB.RSField(rs, "TextOption");
                        newItem.ShippingMethodID = DB.RSFieldInt(rs, "ShippingMethodID");
                        newItem.ShippingMethod = DB.RSFieldByLocale(rs, "ShippingMethod", m_ThisCustomer.LocaleSetting);
                        newItem.ProductTypeId = DB.RSFieldInt(rs, "ProductTypeId");
                        newItem.SEName = DB.RSField(rs, "SEName");
                        newItem.IsAuctionItem = (DB.RSFieldTinyInt(rs, "Deleted") == 2);
                        newItem.ImageFileNameOverride = DB.RSField(rs, "ImageFileNameOverride");
                        newItem.ProductDescription = DB.RSField(rs, "Description");
                        newItem.BluBuksUsed = DB.RSFieldDecimal(rs, "BluBucksUsed");
                        newItem.CategoryFundUsed = DB.RSFieldDecimal(rs, "CategoryFundUsed");
                        newItem.pricewithBluBuksUsed = DB.RSFieldDecimal(rs, "pricewithBluBucksUsed");
                        newItem.pricewithategoryFundUsed = DB.RSFieldDecimal(rs, "pricewithCategoryFundUsed");
                        newItem.FundID = DB.RSFieldInt(rs, "FundID");
                        newItem.BluBucksPercentageUsed = DB.RSFieldDecimal(rs, "BluBucksPercentageUsed");
                        newItem.ProductCategoryID = DB.RSFieldInt(rs, "ProductCategoryID");
                        newItem.GLcode = DB.RSField(rs, "GLcode");
                        newItem.FundName = DB.RSField(rs, "FundName");
                        newItem.Inventory = DB.RSFieldInt(rs, "Inventory");
                        // undocumented feature for custom job:
                        if (AppLogic.AppConfigBool("HidePriceModifiersInCart"))
                        {
                            if (newItem.ChosenColor.IndexOf('[') != -1)
                            {
                                newItem.ChosenColor = Regex.Replace(newItem.ChosenColor, "\\s+", "", RegexOptions.Compiled).Split('[')[0];
                            }
                            if (newItem.ChosenSize.IndexOf('[') != -1)
                            {
                                newItem.ChosenSize = Regex.Replace(newItem.ChosenSize, "\\s+", "", RegexOptions.Compiled).Split('[')[0];
                            }
                        }

                        newItem.TextOptionPrompt = DB.RSFieldByLocale(rs, "TextOptionPrompt", m_ThisCustomer.LocaleSetting);
                        if (newItem.TextOptionPrompt.Length == 0)
                        {
                            newItem.TextOptionPrompt = AppLogic.GetString("common.cs.70", m_SkinID, m_ThisCustomer.LocaleSetting);
                        }
                        newItem.SizeOptionPrompt = DB.RSFieldByLocale(rs, "SizeOptionPrompt", m_ThisCustomer.LocaleSetting);
                        if (newItem.SizeOptionPrompt.Length == 0)
                        {
                            newItem.SizeOptionPrompt = AppLogic.GetString("AppConfig.SizeOptionPrompt", m_SkinID, m_ThisCustomer.LocaleSetting);
                        }
                        newItem.ColorOptionPrompt = DB.RSFieldByLocale(rs, "ColorOptionPrompt", m_ThisCustomer.LocaleSetting);
                        if (newItem.ColorOptionPrompt.Length == 0)
                        {
                            newItem.ColorOptionPrompt = AppLogic.GetString("AppConfig.ColorOptionPrompt", m_SkinID, m_ThisCustomer.LocaleSetting);
                        }
                        newItem.CustomerEntersPricePrompt = DB.RSFieldByLocale(rs, "CustomerEntersPricePrompt", m_ThisCustomer.LocaleSetting);
                        if (newItem.CustomerEntersPricePrompt.Length == 0)
                        {
                            newItem.CustomerEntersPricePrompt = AppLogic.GetString("AppConfig.CustomerEntersPricePrompt", m_SkinID, m_ThisCustomer.LocaleSetting);
                        }

                        newItem.Weight = DB.RSFieldDecimal(rs, "ProductWeight");
                        newItem.Dimensions = DB.RSField(rs, "ProductDimensions");
                        newItem.SubscriptionInterval = DB.RSFieldInt(rs, "SubscriptionInterval");
                        newItem.SubscriptionIntervalType = (DateIntervalTypeEnum)DB.RSFieldInt(rs, "SubscriptionIntervalType");
                        newItem.Notes = DB.RSField(rs, "Notes");
                        int MinQ = 0;
                        newItem.RestrictedQuantities = CommonLogic.BuildListFromCommaString(AppLogic.GetRestrictedQuantities(DB.RSFieldInt(rs, "VariantID"), out MinQ));
                        newItem.MinimumQuantity = MinQ;
                        newItem.ExtensionData = DB.RSField(rs, "ExtensionData");
                        newItem.IsUpsell = DB.RSFieldBool(rs, "IsUpSell");
                        newItem.OrderShippingDetail = String.Empty;
                        bool IsOnSale = false;

                        newItem.Price = DB.RSFieldDecimal(rs, "ProductPrice");
                        newItem.CustomerEntersPrice = DB.RSFieldBool(rs, "CustomerEntersPrice");

                        if (!newItem.CustomerEntersPrice && m_CartType != CartTypeEnum.RecurringCart && !newItem.IsUpsell)
                        {
                            decimal NewPR = newItem.Price;

                            if (!newItem.IsAKit)
                            {
                                NewPR = AppLogic.DetermineLevelPrice(newItem.VariantID, m_ThisCustomer.CustomerLevelID, out IsOnSale);
                                Decimal PrMod = AppLogic.GetColorAndSizePriceDelta(DB.RSField(rs, "ChosenColor"), DB.RSField(rs, "ChosenSize"), DB.RSFieldInt(rs, "TaxClassID"), ThisCustomer, true, false);
                                if (PrMod != System.Decimal.Zero)
                                {
                                    NewPR += PrMod;
                                }
                            }
                            else
                            {
                                NewPR = DB.RSFieldDecimal(rs, "ProductPrice");
                                if (LevelDiscountPercent != 0.0M)
                                {
                                    NewPR = AppLogic.GetKitTotalPrice(m_ThisCustomer.CustomerID, m_ThisCustomer.CustomerLevelID, newItem.ProductID, newItem.VariantID, newItem.ShoppingCartRecordID);
                                }
                            }
                            if (NewPR < System.Decimal.Zero)
                            {
                                NewPR = System.Decimal.Zero; // never know what people will put in the modifiers :)
                            }
                            if (NewPR != newItem.Price)
                            {
                                //I have commented bellow lines as I dont want to update price once added item to cart
                                // newItem.Price = NewPR;
                                // remember to update the actual db record now!                                
                                // DB.ExecuteSQL("update shoppingcart set ProductPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(newItem.Price) + " where ShoppingCartRecID=" + newItem.ShoppingCartRecordID.ToString(), m_DBTrans);
                            }
                        }

                        int qdID = QuantityDiscount.LookupProductQuantityDiscountID(DB.RSFieldInt(rs, "ProductID"));
                        newItem.QuantityDiscountID = qdID;
                        newItem.QuantityDiscountName = CommonLogic.IIF(qdID > 0, QuantityDiscount.GetQuantityDiscountName(qdID, ThisCustomer.LocaleSetting), String.Empty);
                        newItem.QuantityDiscountPercent = 0.0M;
                        newItem.IsTaxable = DB.RSFieldBool(rs, "IsTaxable");
                        newItem.TaxClassID = DB.RSFieldInt(rs, "TaxClassID");
                        newItem.TaxRate = DB.RSFieldDecimal(rs, "TaxRate");
                        newItem.IsShipSeparately = DB.RSFieldBool(rs, "IsShipSeparately");
                        newItem.IsDownload = DB.RSFieldBool(rs, "IsDownload");
                        newItem.DownloadLocation = DB.RSField(rs, "DownloadLocation");

                        newItem.FreeShipping = DB.RSFieldTinyInt(rs, "FreeShipping") == 1;
                        newItem.Shippable = DB.RSFieldTinyInt(rs, "FreeShipping") != 2;

                        newItem.DistributorID = DB.RSFieldInt(rs, "DistributorID");
                        newItem.IsRecurring = (RecurringProducts.IndexOf("," + newItem.VariantID.ToString() + ",") != -1);
                        newItem.RecurringInterval = DB.RSFieldInt(rs, "RecurringInterval");
                        if (newItem.RecurringInterval == 0)
                        {
                            newItem.RecurringInterval = 1; // for backwards compatability
                        }
                        newItem.RecurringIntervalType = (DateIntervalTypeEnum)DB.RSFieldInt(rs, "RecurringIntervalType");
                        if (newItem.RecurringIntervalType == DateIntervalTypeEnum.Unknown)
                        {
                            newItem.RecurringIntervalType = DateIntervalTypeEnum.Monthly; // for backwards compatibility
                        }
                        // If the CartType = Recurring then use the ShoppingCart AddressIDs recorded at the order rather than the Customer Address IDs
                        if (newItem.CartType == CartTypeEnum.RecurringCart)
                        {
                            newItem.BillingAddressID = DB.RSFieldInt(rs, "ShoppingCartBillingAddressID");
                            newItem.ShippingAddressID = DB.RSFieldInt(rs, "ShoppingCartShippingAddressID");
                        }
                        else
                        {
                            newItem.BillingAddressID = DB.RSFieldInt(rs, "CustomerBillingAddressID");
                            newItem.ShippingAddressID = DB.RSFieldInt(rs, "ShoppingCartShippingAddressID");
                        }
                        newItem.IsGift = DB.RSFieldBool(rs, "IsGift");

                        //newItem.ComputeRates();
                        m_CartItems.Add(newItem);

                        if (i == 0)
                        {
                            // clear the list of coupons
                            CartItems.CouponList.Clear();

                            // first record, so load primary cart variables also:
                            m_EMail = DB.RSField(rs, "EMail");
                            if (m_ThisCustomer.CustomerID == 0 || AppLogic.CustomerLevelAllowsCoupons(m_DBTrans, m_ThisCustomer.CustomerLevelID))
                            {
                                m_Coupon = Coupons.GetCoupon(m_DBTrans, ThisCustomer);
                                couponSet = m_Coupon.m_couponset;
                                if (couponSet)
                                {
                                    CartItems.CouponList.Add(m_Coupon);
                                }
                            }

                            if (CartType == CartTypeEnum.RecurringCart)
                            {
                                Address addr = new Address();
                                addr.LoadByCustomer(m_ThisCustomer.CustomerID, m_ThisCustomer.PrimaryBillingAddressID, AddressTypes.Billing);
                                m_CardName = addr.CardName;
                                m_CardType = addr.CardType;
                                m_CardNumber = addr.CardNumber;
                                m_CardExpirationMonth = addr.CardExpirationMonth;
                                m_CardExpirationYear = addr.CardExpirationYear;
                            }
                            else
                            {
                                m_CardName = String.Empty;
                                m_CardType = String.Empty;
                                m_CardNumber = String.Empty;
                                m_CardExpirationMonth = String.Empty;
                                m_CardExpirationYear = String.Empty;
                            }

                            m_OrderNotes = DB.RSField(rs, "OrderNotes");
                            m_FinalizationData = DB.RSField(rs, "FinalizationData");
                            //m_OrderOptions = CommonLogic.BuildListFromCommaString(DB.RSField(rs, "OrderOptions"));
                            m_customerSelectedOptions = rs.Field("OrderOptions");

                            LoadOrderOptions();
                        }

                        i = i + 1;
                    }
                    foreach (CartItem ci in m_CartItems)
                        ci.ComputeRates();
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }
                // make sure we won't reference this again in code
                con = null;
                rs = null;
            }

            if (!couponSet)
            {
                // clear the list of coupons
                CartItems.CouponList.Clear();

                // create an "empty" coupon, but still set the code:
                m_Coupon = new CouponObject();
                m_Coupon.m_couponcode = m_ThisCustomer.CouponCode;
                m_Coupon.m_coupontype = CouponTypeEnum.OrderCoupon;
                m_Coupon.m_description = String.Empty;
                m_Coupon.m_startdate = System.DateTime.MaxValue;
                m_Coupon.m_expirationdate = System.DateTime.MinValue;
                m_Coupon.m_discountamount = System.Decimal.Zero;
                m_Coupon.m_discountpercent = 0.0M;
                m_Coupon.m_discountincludesfreeshipping = false;
                m_Coupon.m_expiresonfirstusebyanycustomer = false;
                m_Coupon.m_expiresafteroneusagebyeachcustomer = false;
                m_Coupon.m_expiresafternuses = 0;
                m_Coupon.m_requiresminimumorderamount = System.Decimal.Zero;
                m_Coupon.m_validforcustomers = new List<int>();
                m_Coupon.m_validforproducts = new List<int>();
                m_Coupon.m_validforcategories = new List<int>();
                m_Coupon.m_validforsections = new List<int>();
                m_Coupon.m_validformanufacturers = new List<int>();
                m_Coupon.m_validforproductsexpanded = new List<int>();
                m_Coupon.m_validforcategoriesexpanded = new List<int>();
                m_Coupon.m_validforsectionsexpanded = new List<int>();
                m_Coupon.m_validformanufacturersexpanded = new List<int>();
                m_Coupon.m_numuses = 0;
            }


        }

        public CouponObject GetCoupon()
        {
            return m_Coupon;
        }

        /// <summary>
        /// Creates a new CouponObject for the specified coupon code
        /// </summary>
        /// <param name="CouponCode"></param>
        /// <returns></returns>
        public CouponObject GetNewCoupon(string CouponCode)
        {
            CouponObject coupon = new CouponObject();
            coupon.m_couponcode = CouponCode;
            coupon.m_startdate = DateTime.MaxValue;
            coupon.m_expirationdate = DateTime.MinValue;
            coupon.m_coupontype = 0;

            SqlConnection con = null;
            IDataReader rs;

            try
            {
                string query = string.Format("select * from Coupon a with (NOLOCK) inner join (select distinct a.CouponID from Coupon a with (nolock) left join CouponStore b with (nolock) on a.CouponID = b.CouponID where ({0} = 0 or StoreID = {1})) b on a.CouponID = b.CouponID where ExpirationDate>=getdate() and deleted=0 and lower(CouponCode)= {2}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCouponFiltering") == true, 1, 0), AppLogic.StoreID(), DB.SQuote(CouponCode.ToLowerInvariant()));

                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection                    
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        coupon.m_coupontype = (CouponTypeEnum)DB.RSFieldInt(rs, "CouponType");
                        coupon.m_description = DB.RSField(rs, "Description");
                        coupon.m_startdate = DB.RSFieldDateTime(rs, "StartDate");
                        coupon.m_expirationdate = DB.RSFieldDateTime(rs, "ExpirationDate");
                        coupon.m_discountamount = DB.RSFieldDecimal(rs, "DiscountAmount");
                        coupon.m_discountpercent = DB.RSFieldDecimal(rs, "DiscountPercent");
                        coupon.m_discountincludesfreeshipping = DB.RSFieldBool(rs, "DiscountIncludesFreeShipping");
                        coupon.m_expiresonfirstusebyanycustomer = DB.RSFieldBool(rs, "ExpiresOnFirstUseByAnyCustomer");
                        coupon.m_expiresafteroneusagebyeachcustomer = DB.RSFieldBool(rs, "ExpiresAfterOneUsageByEachCustomer");
                        coupon.m_expiresafternuses = DB.RSFieldInt(rs, "ExpiresAfterNUses");
                        coupon.m_requiresminimumorderamount = DB.RSFieldDecimal(rs, "RequiresMinimumOrderAmount");

                        coupon.m_validforcustomers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForCustomers"), "\\s+", "", RegexOptions.Compiled));
                        coupon.m_validforproducts = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForProducts"), "\\s+", "", RegexOptions.Compiled));
                        coupon.m_validforcategories = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForCategories"), "\\s+", "", RegexOptions.Compiled));
                        coupon.m_validforsections = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForSections"), "\\s+", "", RegexOptions.Compiled));
                        coupon.m_validformanufacturers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForManufacturers"), "\\s+", "", RegexOptions.Compiled));
                        coupon.m_numuses = DB.RSFieldInt(rs, "NumUses");

                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }
                // make sure we won't reference this again in code
                con = null;
                rs = null;
            }

            return coupon;
        }

        /// <summary>
        /// Validates the coupon passed to this method against the current cart
        /// </summary>
        /// <param name="coupon">CouponObject object for the new coupon to validate</param>
        /// <param name="m_SkinID">KSinID </param>
        /// <param name="LocaleSetting"></param>
        /// <returns></returns>
        public String ValidateNewCoupon(CouponObject coupon)
        {
            String status = AppLogic.ro_OK;
            if (coupon.m_couponcode.Length != 0)
            {
                // we found a valid match for that coupon code with an expiration date greater than or equal to now, so check additional conditions on the coupon:
                // just return first reason for it not being valid, going from most obvious to least obvious:
                if (coupon.m_expirationdate.Equals(System.DateTime.MinValue))
                {
                    status = AppLogic.GetString("shoppingcart.cs.79", m_SkinID, m_ThisCustomer.LocaleSetting);
                }

                if (status == AppLogic.ro_OK)
                {
                    // We use System.DateTime.Today so that if expiration day and the current day is equal, 
                    // it's still valid regardless of time
                    if (coupon.m_expirationdate < System.DateTime.Today)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.69", m_SkinID, m_ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (coupon.m_expiresonfirstusebyanycustomer && AppLogic.AnyCustomerHasUsedCoupon(coupon.m_couponcode))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.70", m_SkinID, m_ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (coupon.m_expiresafteroneusagebyeachcustomer && Customer.HasUsedCoupon(m_ThisCustomer.CustomerID, m_ThisCustomer.CouponCode))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.71", m_SkinID, m_ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (coupon.m_expiresafternuses > 0 && AppLogic.GetNumberOfCouponUses(m_ThisCustomer.CouponCode) > coupon.m_expiresafternuses)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.72", m_SkinID, m_ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (coupon.m_requiresminimumorderamount > System.Decimal.Zero && SubTotal(false, false, true, true, true, false) < coupon.m_requiresminimumorderamount)
                    {
                        status = String.Format(AppLogic.GetString("shoppingcart.cs.73", m_SkinID, m_ThisCustomer.LocaleSetting), m_ThisCustomer.CurrencyString(coupon.m_requiresminimumorderamount));
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (coupon.m_validforcustomers.Count > 0 && !coupon.m_validforcustomers.Contains(m_ThisCustomer.CustomerID))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.74", m_SkinID, m_ThisCustomer.LocaleSetting);
                    }
                }

                if (status == AppLogic.ro_OK)
                {
                    try
                    {
                        string query = string.Format("select count(productid) as N from ShoppingCart with (NOLOCK) where productid in ({0}) and CartType = {1} and customerid = {2} and ({3} = 0 or StoreID = {4})", CommonLogic.BuildCommaStringFromList(coupon.m_validforproductsexpanded), +
                                                    ((int)CartTypeEnum.ShoppingCart), m_ThisCustomer.CustomerID, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowShoppingcartFiltering") == true, 1, 0), AppLogic.StoreID());
                        if (coupon.m_validforproductsexpanded.Count > 0 && DB.GetSqlN(query) == 0)
                        {
                            status = AppLogic.GetString("shoppingcart.cs.75", m_SkinID, m_ThisCustomer.LocaleSetting);
                        }
                    }
                    catch
                    {
                        status = AppLogic.GetString("shoppingcart.cs.76", m_SkinID, m_ThisCustomer.LocaleSetting);
                    }
                }
            }
            else
            {
                SqlConnection con = null;
                IDataReader dr = null;

                try
                {
                    string query = "select * from GiftCard  with (NOLOCK)  where ExpirationDate>=getdate() and DisabledByAdministrator=0 and Balance>0 and SerialNumber=" + DB.SQuote(coupon.m_couponcode);

                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        dr = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        dr = DB.GetRS(query, con);
                    }

                    using (dr)
                    {
                        if (dr.Read())
                        {
                            status = AppLogic.ro_OK;
                        }
                        else
                        {
                            status = AppLogic.GetString("shoppingcart.cs.79", m_SkinID, m_ThisCustomer.LocaleSetting);
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }
                    // make sure we won't reference this again in code
                    con = null;
                }
            }

            return status;
        }

        public void ClearCoupon()
        {
            DB.ExecuteSQL("update customer set CouponCode=NULL where customerid=" + m_ThisCustomer.CustomerID.ToString(), m_DBTrans);
            m_Coupon.m_couponcode = String.Empty;
            m_Coupon.m_coupontype = CouponTypeEnum.OrderCoupon;
            m_Coupon.m_description = String.Empty;
            m_Coupon.m_startdate = System.DateTime.MaxValue;
            m_Coupon.m_expirationdate = System.DateTime.MinValue;
            m_Coupon.m_discountamount = System.Decimal.Zero;
            m_Coupon.m_discountpercent = 0.0M;
            m_Coupon.m_discountincludesfreeshipping = false;
            m_Coupon.m_expiresonfirstusebyanycustomer = false;
            m_Coupon.m_expiresafteroneusagebyeachcustomer = false;
            m_Coupon.m_expiresafternuses = 0;
            m_Coupon.m_requiresminimumorderamount = System.Decimal.Zero;
            m_Coupon.m_validforcustomers = new List<int>();
            m_Coupon.m_validforproducts = new List<int>();
            m_Coupon.m_validforcategories = new List<int>();
            m_Coupon.m_validforsections = new List<int>();
            m_Coupon.m_validformanufacturers = new List<int>();
            m_Coupon.m_validforproductsexpanded = new List<int>();
            m_Coupon.m_validforcategoriesexpanded = new List<int>();
            m_Coupon.m_validforsectionsexpanded = new List<int>();
            m_Coupon.m_validformanufacturersexpanded = new List<int>();
            m_Coupon.m_numuses = 0;
            m_CachedTotals.Clear();
            m_ThisCustomer.CouponCode = String.Empty;
        }

        public void SetCoupon(String newCoupon, bool UpdateCartObject)
        {
            m_CachedTotals.Clear();
            if (newCoupon.Length == 0)
            {
                ClearCoupon();
            }
            else
            {
                newCoupon = newCoupon.ToUpperInvariant();
                GiftCard GC = new GiftCard(newCoupon);
                CouponObject coupon = GetNewCoupon(newCoupon);
                DB.ExecuteSQL("update customer set CouponCode=" + DB.SQuote(newCoupon) + " where customerid=" + m_ThisCustomer.CustomerID.ToString(), m_DBTrans);
                ThisCustomer.CouponCode = newCoupon;
                if (UpdateCartObject)
                {
                    m_CouponStatus = ValidateNewCoupon(coupon);
                    if (m_CouponStatus == AppLogic.ro_OK)
                    {
                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            string query = string.Format("select * from Coupon a with (NOLOCK) inner join (select distinct a.CouponID from Coupon a with (nolock) left join CouponStore b with (nolock) on a.CouponID = b.CouponID where ({0} = 0 or StoreID = {1})) b on a.CouponID = b.CouponID where ExpirationDate>=getdate() and deleted=0 and lower(CouponCode)= {2}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowCouponFiltering") == true, 1, 0), AppLogic.StoreID(), DB.SQuote(newCoupon.ToLowerInvariant()));

                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(query, m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(query, con);
                            }

                            using (rs)
                            {
                                if (rs.Read())
                                {
                                    m_Coupon.m_couponcode = DB.RSField(rs, "CouponCode");
                                    m_Coupon.m_coupontype = (CouponTypeEnum)DB.RSFieldInt(rs, "CouponType");
                                    m_Coupon.m_description = DB.RSField(rs, "Description");
                                    m_Coupon.m_startdate = DB.RSFieldDateTime(rs, "StartDate");
                                    m_Coupon.m_expirationdate = DB.RSFieldDateTime(rs, "ExpirationDate");
                                    m_Coupon.m_discountamount = DB.RSFieldDecimal(rs, "DiscountAmount");
                                    m_Coupon.m_discountpercent = DB.RSFieldDecimal(rs, "DiscountPercent");
                                    m_Coupon.m_discountincludesfreeshipping = DB.RSFieldBool(rs, "DiscountIncludesFreeShipping");
                                    m_Coupon.m_expiresonfirstusebyanycustomer = DB.RSFieldBool(rs, "ExpiresOnFirstUseByAnyCustomer");
                                    m_Coupon.m_expiresafteroneusagebyeachcustomer = DB.RSFieldBool(rs, "ExpiresAfterOneUsageByEachCustomer");
                                    m_Coupon.m_expiresafternuses = DB.RSFieldInt(rs, "ExpiresAfterNUses");
                                    m_Coupon.m_requiresminimumorderamount = DB.RSFieldDecimal(rs, "RequiresMinimumOrderAmount");

                                    m_Coupon.m_validforcustomers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForCustomers"), "\\s+", "", RegexOptions.Compiled));
                                    m_Coupon.m_validforproducts = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForProducts"), "\\s+", "", RegexOptions.Compiled));
                                    m_Coupon.m_validforcategories = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForCategories"), "\\s+", "", RegexOptions.Compiled));
                                    m_Coupon.m_validforsections = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForSections"), "\\s+", "", RegexOptions.Compiled));
                                    m_Coupon.m_validformanufacturers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForManufacturers"), "\\s+", "", RegexOptions.Compiled));

                                    m_Coupon.m_validforproductsexpanded = new List<int>();
                                    m_Coupon.m_validforcategoriesexpanded = new List<int>();
                                    m_Coupon.m_validforsectionsexpanded = new List<int>();
                                    m_Coupon.m_validformanufacturersexpanded = new List<int>();

                                    if (m_Coupon.m_coupontype == CouponTypeEnum.ProductCoupon && m_Coupon.m_validforcategories.Count > 0)
                                    {
                                        m_Coupon.m_validforcategoriesexpanded = AppLogic.LookupHelper("Category", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforcategories), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        List<int> pList = AppLogic.LookupHelper("Category", 0).GetProductList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforcategoriesexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        m_Coupon.m_validforproductsexpanded.AddRange(pList);
                                    }
                                    if (m_Coupon.m_coupontype == CouponTypeEnum.ProductCoupon && m_Coupon.m_validforsections.Count > 0)
                                    {
                                        m_Coupon.m_validforsectionsexpanded = AppLogic.LookupHelper("Section", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforsections), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        List<int> pList = AppLogic.LookupHelper("Section", 0).GetProductList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforsectionsexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        m_Coupon.m_validforproductsexpanded.AddRange(pList);
                                    }
                                    if (m_Coupon.m_coupontype == CouponTypeEnum.ProductCoupon && m_Coupon.m_validformanufacturers.Count > 0)
                                    {
                                        m_Coupon.m_validformanufacturersexpanded = AppLogic.LookupHelper("Manufacturer", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validformanufacturers), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        List<int> pList = AppLogic.LookupHelper("Manufacturer", 0).GetProductList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validformanufacturersexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        m_Coupon.m_validforproductsexpanded.AddRange(pList);
                                    }

                                    m_Coupon.m_numuses = DB.RSFieldInt(rs, "NumUses");

                                    CartItems.CouponList.Add(m_Coupon);

                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again in code
                            con = null;
                            rs = null;
                        }
                    }
                    else if (this.Coupon.CouponType == CouponTypeEnum.GiftCard || !(GC.SerialNumber.Length == 0))
                    {
                        ClearCoupon();
                        SqlConnection con = null;
                        IDataReader rs = null;
                        try
                        {
                            string query = string.Format("select * from GiftCard a with (nolock) inner join (select distinct a.GiftCardID from GiftCard a with (nolock) left join GiftCardStore b with (nolock) on a.GiftCardID = b.GiftCardID where ({0} = 0 or StoreID = {1})) " +
                                                  "b on a.GiftCardID = b.GiftCardID where ExpirationDate>=getdate() and DisabledByAdministrator=0 and Balance>0 and SerialNumber={2}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowGiftCardFiltering") == true, 1, 0), AppLogic.StoreID(), DB.SQuote(newCoupon.ToLowerInvariant()));
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(query, m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(query, con);
                            }

                            using (rs)
                            {
                                if (rs.Read())
                                {
                                    m_CouponStatus = AppLogic.ro_OK;
                                    m_Coupon.m_couponcode = DB.RSField(rs, "SerialNumber");
                                    m_Coupon.m_coupontype = CouponTypeEnum.GiftCard;
                                    m_Coupon.m_description = "";
                                    m_Coupon.m_startdate = DB.RSFieldDateTime(rs, "StartDate");
                                    m_Coupon.m_expirationdate = DB.RSFieldDateTime(rs, "ExpirationDate");
                                    m_Coupon.m_discountamount = DB.RSFieldDecimal(rs, "Balance");
                                    m_Coupon.m_discountpercent = 0.0M;
                                    m_Coupon.m_discountincludesfreeshipping = false;
                                    m_Coupon.m_expiresonfirstusebyanycustomer = false;
                                    m_Coupon.m_expiresafteroneusagebyeachcustomer = false;
                                    m_Coupon.m_expiresafternuses = 0;
                                    m_Coupon.m_requiresminimumorderamount = System.Decimal.Zero;

                                    m_Coupon.m_validforcustomers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForCustomers"), "\\s+", "", RegexOptions.Compiled));
                                    m_Coupon.m_validforproducts = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForProducts"), "\\s+", "", RegexOptions.Compiled));
                                    m_Coupon.m_validforcategories = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForCategories"), "\\s+", "", RegexOptions.Compiled));
                                    m_Coupon.m_validforsections = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForSections"), "\\s+", "", RegexOptions.Compiled));
                                    m_Coupon.m_validformanufacturers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForManufacturers"), "\\s+", "", RegexOptions.Compiled));

                                    m_Coupon.m_validforproductsexpanded = new List<int>();
                                    m_Coupon.m_validforcategoriesexpanded = new List<int>();
                                    m_Coupon.m_validforsectionsexpanded = new List<int>();
                                    m_Coupon.m_validformanufacturersexpanded = new List<int>();

                                    if (m_Coupon.m_coupontype == CouponTypeEnum.ProductCoupon && m_Coupon.m_validforcategories.Count > 0)
                                    {
                                        m_Coupon.m_validforcategoriesexpanded = AppLogic.LookupHelper("Category", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforcategories), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        List<int> pList = AppLogic.LookupHelper("Category", 0).GetProductList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforcategoriesexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        foreach (int p in pList)
                                        {
                                            m_Coupon.m_validforproductsexpanded.Add(p);
                                        }
                                    }
                                    if (m_Coupon.m_coupontype == CouponTypeEnum.ProductCoupon && m_Coupon.m_validforsections.Count > 0)
                                    {
                                        m_Coupon.m_validforsectionsexpanded = AppLogic.LookupHelper("Section", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforsections), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        List<int> pList = AppLogic.LookupHelper("Section", 0).GetProductList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforsectionsexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        foreach (int p in pList)
                                        {
                                            m_Coupon.m_validforproductsexpanded.Add(p);
                                        }

                                    }
                                    if (m_Coupon.m_coupontype == CouponTypeEnum.ProductCoupon && m_Coupon.m_validformanufacturers.Count != 0)
                                    {
                                        m_Coupon.m_validformanufacturersexpanded = AppLogic.LookupHelper("Manufacturer", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validformanufacturers), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        List<int> pList = AppLogic.LookupHelper("Manufacturer", 0).GetProductList(CommonLogic.BuildCommaStringFromList(m_Coupon.m_validformanufacturersexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                        foreach (int p in pList)
                                        {
                                            m_Coupon.m_validforproductsexpanded.Add(p);
                                        }
                                    }

                                    m_Coupon.m_numuses = 0;
                                    DB.ExecuteSQL("update customer set CouponCode=" + DB.SQuote(DB.RSField(rs, "SerialNumber")) + " where customerid=" + m_ThisCustomer.CustomerID.ToString(), m_DBTrans);
                                    CartItems.CouponList.Add(m_Coupon);
                                    m_CouponStatus = AppLogic.ro_OK;
                                    ThisCustomer.CouponCode = m_Coupon.CouponCode;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again in code
                            con = null;
                            rs = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the quantity in cart for the specified ProductID and VariantID.
        /// </summary>
        /// <param name="ProductID">The ProductID.</param>
        /// <param name="VariantID">The VariantID.</param>
        /// <returns></returns>
        public int GetQuantityInCart(int ProductID, int VariantID)
        {
            int Q = 0;
            // ignore size & colors
            foreach (CartItem c in m_CartItems)
            {
                if (c.ProductID == ProductID && c.VariantID == VariantID)
                {
                    Q += c.Quantity;
                }
            }
            return Q;
        }

        /// <summary>
        /// Determine if the Coupon matched the product.
        /// </summary>
        /// <param name="m_Coupon">The Coupon.</param>
        /// <param name="m_ProductID">The ProductID.</param>
        /// <returns>Returns TRUE if the coupon matches the product otherwise FALSE.</returns>
        private bool CouponMatchesProduct(CouponObject m_Coupon, int m_ProductID)
        {
            if (m_Coupon.m_validforproductsexpanded.Count > 0)
            {
                if (("," + CommonLogic.BuildCommaStringFromList(m_Coupon.m_validforproductsexpanded) + ",").IndexOf("," + m_ProductID.ToString() + ",") != -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the heaviest package weight total.
        /// </summary>
        /// <returns>Returns the heaviest package weight total.</returns>
        public Decimal GetHeaviestPackageWeightTotal()
        {
            Decimal sum = 0.0M;
            IEnumerable<Decimal> weightShipSeparately;

            Decimal defaultWeight = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight"); // NOTHING is weightless!

            // check non-ship separately items:
            sum += m_CartItems
                .Where(ci => !ci.IsDownload && (!ci.FreeShipping || (ci.FreeShipping && AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))) && !ci.IsShipSeparately && ci.Shippable)
                .Sum(ci => CommonLogic.IIF(ci.Weight == System.Decimal.Zero, ci.Quantity * defaultWeight, ci.Quantity * ci.Weight));

            // now check ship separately items, and find heaviest package:
            weightShipSeparately = m_CartItems
                .Where(ci => !ci.IsDownload && !ci.FreeShipping && ci.IsShipSeparately && ci.Shippable)
                .Select(ci => ci.Weight);

            foreach (decimal w in weightShipSeparately)
            {
                if (w > sum)
                {
                    sum = w;
                }
            }

            // 0 weights are NOT allowed :)
            if (sum == 0.0M)
            {
                Decimal MinOrderWeight = 0.5M;
                if (AppLogic.AppConfig("MinOrderWeight").Length != 0)
                {
                    try
                    {
                        MinOrderWeight = AppLogic.AppConfigUSDecimal("MinOrderWeight");
                    }
                    catch { }
                }
                sum = MinOrderWeight; // must have SOMETHING to use!
            }
            return sum;
        }

        /// <summary>
        /// Works off currently active m_CartItems list (which may be a subset of the whole cart on multi-ship orders!)
        /// </summary>
        /// <returns></returns>
        public Decimal WeightTotal()
        {
            Decimal sum = 0.0M;
            foreach (CartItem c in m_CartItems)
            {
                //Don't include items that do not require shipping except when FreeShippingAllowsRateSelection = true
                if (!c.IsDownload && c.Shippable && !(c.IsShipSeparately && c.FreeShipping))
                {
                    Decimal thisW = c.Weight;
                    if (m_ShipCalcID == Shipping.ShippingCalculationEnum.UseRealTimeRates)
                    {
                        // adjust weight to be non-zero for RT shipping only:
                        if (thisW == 0.0M)
                        {
                            thisW = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight"); // NOTHING is weightless!
                        }
                    }
                    sum += (c.Quantity * thisW);
                }
            }
            if (sum == 0.0M)
            {
                Decimal MinOrderWeight = 0.5M;
                if (AppLogic.AppConfig("MinOrderWeight").Length != 0)
                {
                    try
                    {
                        MinOrderWeight = AppLogic.AppConfigUSDecimal("MinOrderWeight");
                    }
                    catch { }
                }
                sum = MinOrderWeight; // must have SOMETHING to use!
            }

            // RTShipping has an optional PackageExtraWeight factor to account for dunnage
            if (Shipping.GetActiveShippingCalculationID() == Shipping.ShippingCalculationEnum.UseRealTimeRates)
            {
                sum += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");
            }

            return sum;
        }

        public Decimal WeightTotalByAddress(int AddressID)
        {
            Decimal sum = 0.0M;
            foreach (CartItem c in m_CartItems)
            {
                //Don't include items that do not require shipping
                if (!c.IsDownload && !c.FreeShipping && c.ShippingAddressID == AddressID && c.Shippable)
                {
                    Decimal thisW = c.Weight;
                    if (c.IsAKit)
                    {
                        thisW += AppLogic.KitWeightDelta(ThisCustomer.CustomerID, c.ProductID, c.ShoppingCartRecordID);
                    }
                    if (m_ShipCalcID == Shipping.ShippingCalculationEnum.UseRealTimeRates)
                    {
                        // adjust weight to be non-zero for RT shipping only:
                        if (thisW == 0.0M)
                        {
                            thisW = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight"); // NOTHING is weightless!
                        }
                    }
                    sum += (c.Quantity * thisW);
                }
            }
            if (sum == 0.0M)
            {
                Decimal MinOrderWeight = 0.5M;
                if (AppLogic.AppConfig("MinOrderWeight").Length != 0)
                {
                    try
                    {
                        MinOrderWeight = AppLogic.AppConfigUSDecimal("MinOrderWeight");
                    }
                    catch { }
                }
                sum = MinOrderWeight; // must have SOMETHING to use!
            }

            // RTShipping has an optional PackageExtraWeight factor to account for dunnage
            if (Shipping.GetActiveShippingCalculationID() == Shipping.ShippingCalculationEnum.UseRealTimeRates)
            {
                sum += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");
            }

            return sum;
        }

        /// <summary>
        /// Return the ArrayList passed in with only the Shipping AddressIDs equal to AddressID
        /// Destroys the list passed in so only work on copies.
        /// </summary>
        /// <param name="AddressID"></param>
        /// <returns>The AddressID of the next AddressID to be read</returns>
        public int FilterItemAddressList(CartItemCollection cartItems, int addressID)
        {
            int result = -1;
            CartItem c = (CartItem)cartItems[0];
            if (addressID == 0)
            {
                addressID = c.ShippingAddressID;
            }
            while ((cartItems.Count > 0) && (c.ShippingAddressID != addressID))
            {
                cartItems.Remove(c);
                c = (CartItem)cartItems[0];
            }
            int i = 0;
            do
            {
                c = (CartItem)cartItems[i];
                i++;
            }
            while ((i < cartItems.Count) && (c.ShippingAddressID == addressID));
            if (c.ShippingAddressID != addressID)
            {
                result = c.ShippingAddressID;
                i--;
                do
                {
                    c = (CartItem)cartItems[i];
                    cartItems.Remove(c);
                }
                while (i < cartItems.Count);
            }
            else
            {
                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Gets the shipping total.
        /// </summary>
        /// <param name="includeDiscount">if set to <c>true</c> [include discount].</param>
        /// <param name="includeTax">if set to <c>true</c> [include tax].</param>
        /// <returns></returns>
        public Decimal ShippingTotal(bool includeDiscount, bool includeTax)
        {
            return Prices.ShippingTotal(includeDiscount, includeTax, CartItems, ThisCustomer, OrderOptions);
        }

        /// <summary>
        /// Calculates tax on shipping
        /// </summary>
        /// <param name="shipCost"></param>
        /// <param name="useAddress"></param>
        /// <returns></returns>
        public Decimal ShippingTax(Decimal shipCost, Address useAddress)
        {
            return Prices.ShippingTax(shipCost, useAddress, ThisCustomer);
        }

        /// <summary>
        /// Calculates tax on shipping
        /// </summary>
        /// <param name="shipCost"></param>
        /// <param name="AddressID"></param>
        /// <returns></returns>
        public Decimal ShippingTax(Decimal shipCost, int AddressID)
        {
            return Prices.ShippingTax(shipCost, AddressID, ThisCustomer);
        }

        /// <summary>
        /// Gets the tax total
        /// </summary>
        /// <returns></returns>
        public Decimal TaxTotal()
        {
            return Prices.TaxTotal(ThisCustomer, CartItems, ShippingTotal(true, false), OrderOptions);
        }

        /// <summary>
        /// Totals the specified include discount.
        /// </summary>
        /// <param name="includeDiscount">if set to <c>true</c> [include discount].</param>
        /// <returns></returns>
        public Decimal Total(bool includeDiscount)
        {
            return Prices.Total(includeDiscount, CartItems, ThisCustomer, OrderOptions, true);
        }

        /// <summary>
        /// Displays the recurring cart item.
        /// </summary>
        /// <param name="OriginalRecurringOrderNumber">The original recurring order number.</param>
        /// <param name="m_SkinID">The SkinID.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="UseParser">Use parser.</param>
        /// <returns></returns>
        public String DisplayRecurring(int OriginalRecurringOrderNumber, int m_SkinID, String LocaleSetting, Parser UseParser)
        {
            return DisplayRecurring(OriginalRecurringOrderNumber, m_SkinID, true, false, false, String.Empty, LocaleSetting, UseParser);
        }

        /// <summary>
        /// Displays the recurring cart item.
        /// </summary>
        /// <param name="OriginalRecurringOrderNumber">The original recurring order number.</param>
        /// <param name="m_SkinID">The skinID.</param>
        /// <param name="ShowCancelButton">if set to <c>true</c> [show cancel button].</param>
        /// <param name="ShowRetryButton">if set to <c>true</c> [show retry button].</param>
        /// <param name="ShowRestartButton">if set to <c>true</c> [show restart button].</param>
        /// <param name="GatewayStatus">The gateway status.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="UseParser">Use parser.</param>
        /// <returns></returns>
        public String DisplayRecurring(int OriginalRecurringOrderNumber, int m_SkinID, bool ShowCancelButton, bool ShowRetryButton, bool ShowRestartButton, String GatewayStatus, String LocaleSetting, Parser UseParser)
        {
            String BACKURL = AppLogic.GetCartContinueShoppingURL(SkinID, ThisCustomer.LocaleSetting);
            if (IsEmpty())
            {
                Topic t1 = new Topic("EmptyRecurringListText", m_ThisCustomer.LocaleSetting, m_SkinID, UseParser);
                return t1.Contents;
            }
            StringBuilder tmpS = new StringBuilder(50000);
            tmpS.Append("<div class=\"recurring-wrap\">");

            CartItem co = (CartItem)m_CartItems[0];

            Order originalOrder = new Order(OriginalRecurringOrderNumber);
            bool isPPECorder = (originalOrder.PaymentMethod == AppLogic.ro_PMPayPalExpress);
            bool IsPayPalPaymentsStandardOrder = (originalOrder.PaymentMethod == AppLogic.ro_PMPayPal && originalOrder.PaymentGateway != "PAYPAL" && originalOrder.PaymentGateway != "PAYFLOWPRO");


            if (co.RecurringSubscriptionID.Length != 0)
            {
                tmpS.Append(String.Format("<div class=\"recurring-header\">Original Recurring Order Number: {0}</div> <div class=\"recurring-id\">SubscriptionID: {1}</div> <div class=\"recurring-index\">RecurringIndex={2}</div><div class=\"recurring-created-on\">Created On {3}</div>", OriginalRecurringOrderNumber, co.RecurringSubscriptionID, co.RecurringIndex, Localization.ToThreadCultureShortDateString(co.CreatedOn)));
            }
            else
            {
                tmpS.Append(String.Format("<div class=\"recurring-header\">Original Recurring Order Number: {0}</div> <div class=\"recurring-id\">RecurringIndex: {1}</div> <div class=\"recurring-created-on\">Created On {2}</div>", OriginalRecurringOrderNumber, co.RecurringIndex, Localization.ToThreadCultureShortDateString(co.CreatedOn)));
            }

            tmpS.Append("<div class=\"recurring-buttons\">");
            if (ShowCancelButton && !IsPayPalPaymentsStandardOrder)
            {
                tmpS.Append(" <input type=\"button\" class=\"button stop-button\" value=\"");
                tmpS.Append("Stop Future Billing");
                tmpS.Append("\" onClick=\"if(confirm('");
                tmpS.Append(AppLogic.GetString("shoppingcart.cs.14", m_SkinID, m_ThisCustomer.LocaleSetting));
                tmpS.Append("')) {self.location='");
                tmpS.Append(CommonLogic.IIF(AppLogic.IsAdminSite, "cst_history.aspx?customerid=" + m_ThisCustomer.CustomerID.ToString() + "&", "account.aspx?") + "deleteid=" + co.OriginalRecurringOrderNumber.ToString() + "';}\">");
            }

            if (!isPPECorder && originalOrder.PaymentGateway != "PAYPAL" && !IsPayPalPaymentsStandardOrder)
            {
                tmpS.Append(" <input type=\"button\" class=\"button update-billing-button\" name=\"button" + OriginalRecurringOrderNumber.ToString() + "\" value=\"Update Billing Info\" onclick=\"javascript:toggleLayer('addressBlock" + OriginalRecurringOrderNumber.ToString() + "');\" />\n");
            }

            // if autobill by gateway
            if (co.RecurringSubscriptionID.Length != 0 && AppLogic.IsAdminSite)
            {
                if (ShowRetryButton)
                {
                    tmpS.Append(" <input type=\"button\" class=\"button retry-button\" value=\"");
                    tmpS.Append("Retry Payment");
                    tmpS.Append("\" onClick=\"if(confirm('");
                    tmpS.Append("Are you sure you want to retry the payment on this Recurring AutoBill order?");
                    tmpS.Append("')) {self.location='");
                    tmpS.Append(CommonLogic.IIF(AppLogic.IsAdminSite, "cst_history.aspx?customerid=" + m_ThisCustomer.CustomerID.ToString() + "&", "account.aspx?") + "retrypaymentid=" + co.OriginalRecurringOrderNumber.ToString() + "';}\">");
                }

                if (ShowRestartButton)
                {
                    tmpS.Append(" <input type=\"button\" class=\"button restart-button\" value=\"");
                    tmpS.Append("Restart Billing");
                    tmpS.Append("\" onClick=\"if(confirm('");
                    tmpS.Append("Are you sure you want to restart the billing for this Recurring AutoBill order?");
                    tmpS.Append("')) {self.location='");
                    tmpS.Append(CommonLogic.IIF(AppLogic.IsAdminSite, "cst_history.aspx?customerid=" + m_ThisCustomer.CustomerID.ToString() + "&", "account.aspx?") + "restartid=" + co.OriginalRecurringOrderNumber.ToString() + "';}\">");
                }

                if (GatewayStatus.Length != 0)
                {
                    tmpS.Append(" " + GatewayStatus);
                }
            }


            tmpS.Append("</div>");
            tmpS.Append("<div class=\"recurring-sub\">");

            // if autobill by gateway, you cannot edit the interval here
            if (AppLogic.IsAdminSite && AppLogic.AppConfigBool("AllowRecurringIntervalEditing") && co.RecurringSubscriptionID.Length == 0 && !isPPECorder)
            {
                //this additional form tag is to offset the aspnet form - a hack until we rewrite this page.
                tmpS.Append("<form></form><form style=\"margin-top: 0px; margin-bottom: 0px;\" name=\"ChangeDate\" action=\"cst_recurring.aspx?customerid=");
                tmpS.Append(m_ThisCustomer.CustomerID.ToString());
                tmpS.Append("\" method=\"POST\">");
                tmpS.Append("<input type=\"hidden\" name=\"OriginalRecurringOrderNumber\" id=\"OriginalRecurringOrderNumber\" value=\"");
                tmpS.Append(OriginalRecurringOrderNumber.ToString());
                tmpS.Append("\">");
                tmpS.Append("<nobr>Next Ship Date: <input type=\"text\" name=\"NextRecurringShipDate\" id=\"NextRecurringShipDate\"> (leave blank for no change) <input type=\"submit\" class=\"normalButtons\" value=\"Save\"></nobr>");
                tmpS.Append("</form>");
                tmpS.Append("<form style=\"margin-top: 0px; margin-bottom: 0px;\" name=\"ChangeDay\" action=\"cst_recurring.aspx?customerid=");
                tmpS.Append(m_ThisCustomer.CustomerID.ToString());
                tmpS.Append("\" method=\"POST\">");
                tmpS.Append("<nobr><input type=\"hidden\" name=\"OriginalRecurringOrderNumber\" id=\"OriginalRecurringOrderNumber\" value=\"");
                tmpS.Append(OriginalRecurringOrderNumber.ToString());
                tmpS.Append("\">");
                tmpS.Append("Recurring Interval: <input type=\"text\" size=\"2\" maxlength=\"4\" name=\"RecurringInterval\" id=\"RecurringInterval\" value=\"");
                tmpS.Append(co.RecurringInterval.ToString());
                tmpS.Append("\"> ");
                tmpS.Append("Recurring Interval Type: <select name=\"RecurringIntervalType\" id=\"RecurringIntervalType\" size=\"1\">");
                if (!AppLogic.UseSpecialRecurringIntervals())
                {
                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.Day).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.Day, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.Day.ToString());
                    tmpS.Append("</option>");
                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.Week).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.Week, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.Week.ToString());
                    tmpS.Append("</option>");
                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.Month).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.Month, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.Month.ToString());
                    tmpS.Append("</option>");
                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.Year).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.Year, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.Year.ToString());
                    tmpS.Append("</option>");
                }
                else
                {
                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.Weekly).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.Weekly, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.Weekly.ToString());
                    tmpS.Append("</option>");

                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.BiWeekly).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.BiWeekly, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.BiWeekly.ToString());
                    tmpS.Append("</option>");

                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.Monthly).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.Monthly, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.Monthly.ToString());
                    tmpS.Append("</option>");
                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.Quarterly).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.Quarterly, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.Quarterly.ToString());
                    tmpS.Append("</option>");

                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.SemiYearly).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.SemiYearly, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.SemiYearly.ToString());
                    tmpS.Append("</option>");

                    tmpS.Append("<option value=\"");
                    tmpS.Append(((int)DateIntervalTypeEnum.Yearly).ToString());
                    tmpS.Append("\" ");
                    tmpS.Append(CommonLogic.IIF(co.RecurringIntervalType == DateIntervalTypeEnum.Yearly, " selected ", ""));
                    tmpS.Append(">");
                    tmpS.Append(DateIntervalTypeEnum.Yearly.ToString());
                    tmpS.Append("</option>");

                }
                tmpS.Append("</select>");
                tmpS.Append(" <input class=\"button call-to-action save-button\"  type=\"submit\" value=\"Save\"></nobr>");
                tmpS.Append("</form>");
            }

            tmpS.Append("</div>");


            if (!isPPECorder)
            {
                tmpS.Append("<div id=\"addressBlock" + OriginalRecurringOrderNumber.ToString() + "\" class=\"addressBlockDiv\">\n");
                Address BA = new Address();
                BA.LoadByCustomer(ThisCustomer.CustomerID, AddressTypes.Billing);
                tmpS.Append(String.Format("<iframe src=\"editaddressrecurring.aspx?addressid={0}&originalrecurringordernumber={1}\" name=\"addressFrame{1}\" id=\"addressFrame{1}\" frameborder=\"0\" height=\"410\" scrolling=\"auto\" width=\"100%\" marginheight=\"0\" marginwidth=\"0\"></iframe>"
                    , BA.AddressID.ToString()
                    , OriginalRecurringOrderNumber));

                tmpS.Append("</div>\n");
            }

            tmpS.Append("<table class=\"table table-striped order-table\">");
            tmpS.Append("<tr class=\"table-header\">");
            tmpS.Append("<th>");
            tmpS.Append(AppLogic.GetString("shoppingcart.cs.1", m_SkinID, m_ThisCustomer.LocaleSetting));
            tmpS.Append("</th>");
            tmpS.Append("<th>");
            tmpS.Append(AppLogic.GetString("shoppingcart.cs.2", m_SkinID, m_ThisCustomer.LocaleSetting));
            tmpS.Append("</th>");
            tmpS.Append("<th>");
            tmpS.Append(AppLogic.GetString("AppConfig.ColorOptionPrompt", m_SkinID, LocaleSetting).ToUpperInvariant());
            tmpS.Append("</th>");
            tmpS.Append("<th>");
            tmpS.Append(AppLogic.GetString("AppConfig.SizeOptionPrompt", m_SkinID, LocaleSetting).ToUpperInvariant());
            tmpS.Append("</th>");
            tmpS.Append("<th>");
            tmpS.Append(AppLogic.GetString("shoppingcart.cs.3", m_SkinID, m_ThisCustomer.LocaleSetting));
            tmpS.Append("</th>");
            tmpS.Append("<th>");
            tmpS.Append(AppLogic.GetString("shoppingcart.cs.4", m_SkinID, m_ThisCustomer.LocaleSetting));
            tmpS.Append("</th>");
            tmpS.Append("<th>");
            tmpS.Append(AppLogic.GetString("shoppingcart.cs.7", m_SkinID, m_ThisCustomer.LocaleSetting));
            tmpS.Append("</th>");
            tmpS.Append("</tr>");

            bool ShowLinkBack = AppLogic.AppConfigBool("LinkToProductPageInCart");

            int OrderShippingAddressID = 0;
            int OrderBillingAddressID = 0;

            foreach (CartItem c in m_CartItems)
            {
                if (c.OriginalRecurringOrderNumber == OriginalRecurringOrderNumber)
                {
                    OrderShippingAddressID = c.ShippingAddressID;
                    OrderBillingAddressID = c.BillingAddressID;

                    tmpS.Append("<tr class=\"table-row\">");
                    tmpS.Append("<td>");
                    if (AppLogic.IsAdminSite)
                    {
                        tmpS.Append("<a href=\"cst_recurring.aspx?customerid=" + m_ThisCustomer.CustomerID.ToString() + "\">");
                    }
                    else
                    {
                        if (ShowLinkBack && !c.IsSystem)
                        {
                            tmpS.Append("<a href=\"" + SE.MakeProductLink(c.ProductID, "") + "\">");
                        }
                    }
                    tmpS.Append(AppLogic.MakeProperObjectName(c.ProductName, c.VariantName, m_ThisCustomer.LocaleSetting));
                    if (c.TextOption.Length != 0)
                    {
                        if (c.TextOption.IndexOf("\n") != -1)
                        {
                            tmpS.Append("");
                            tmpS.Append(AppLogic.GetString("shoppingcart.cs.25", m_SkinID, m_ThisCustomer.LocaleSetting));
                            tmpS.Append("");
                            tmpS.Append(XmlCommon.GetLocaleEntry(c.TextOption, m_ThisCustomer.LocaleSetting, true).Replace("\n", ""));
                        }
                        else
                        {
                            tmpS.Append(" (" + AppLogic.GetString("shoppingcart.cs.25", m_SkinID, m_ThisCustomer.LocaleSetting) + " " + XmlCommon.GetLocaleEntry(c.TextOption, m_ThisCustomer.LocaleSetting, true) + ") ");
                        }
                    }
                    if (AppLogic.IsAdminSite || ShowLinkBack)
                    {
                        tmpS.Append("</a>");
                    }
                    if (c.IsAKit)
                    {
                        String tmp = String.Empty;

                        SqlConnection con = null;
                        IDataReader rsx = null;

                        try
                        {
                            string query = "select kitItem.Name, kitcart.quantity from kitcart   with (NOLOCK)  inner join kititem   with (NOLOCK)  on kitcart.kititemid=kitItem.kititemid where ShoppingCartrecid=" + c.ShoppingCartRecordID.ToString();

                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rsx = DB.GetRS(query, m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rsx = DB.GetRS(query, con);
                            }

                            using (rsx)
                            {
                                while (rsx.Read())
                                {
                                    tmp += " - (" + DB.RSFieldInt(rsx, "Quantity").ToString() + ") " + DB.RSFieldByLocale(rsx, "Name", m_ThisCustomer.LocaleSetting) + "";
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again in code
                            con = null;
                            rsx = null;
                        }

                        tmpS.Append("");
                        tmpS.Append(tmp);
                    }
                    tmpS.Append("</td>");
                    tmpS.Append("<td>");
                    tmpS.Append(c.SKU);
                    tmpS.Append("</td>");
                    tmpS.Append("<td>");
                    tmpS.Append((CommonLogic.IIF(c.ChosenColor.Length == 0, "--", c.ChosenColor)));
                    tmpS.Append("</td>");
                    tmpS.Append("<td>");
                    tmpS.Append((CommonLogic.IIF(c.ChosenSize.Length == 0, "--", c.ChosenSize)));
                    tmpS.Append("</td>");
                    tmpS.Append("<td>");
                    tmpS.Append(c.Quantity);
                    tmpS.Append("</td>");
                    tmpS.Append("<td>");
                    int Q = c.Quantity;
                    decimal PR = c.Price * Q;
                    Decimal DIDPercent = 0.0M;
                    QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;
                    if (m_CustomerLevelAllowsQuantityDiscounts)
                    {
                        DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageForLineItem(c, out fixedPriceDID);
                        if (DIDPercent != 0.0M)
                        {
                            if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
                            {
                                if (Currency.GetDefaultCurrency() == ThisCustomer.CurrencySetting)
                                {
                                    PR = (PR - DIDPercent);
                                }
                                else
                                {
                                    DIDPercent = Decimal.Round(Currency.Convert(DIDPercent, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                                    PR = (PR - DIDPercent);
                                }
                            }
                            else
                            {
                                PR = PR * ((100.0M - DIDPercent) / 100.0M);
                            }
                        }
                    }
                    tmpS.Append(m_ThisCustomer.CurrencyString(PR));
                    if (m_CustomerLevelAllowsQuantityDiscounts)
                    {
                        if (DIDPercent != 0.0M)
                        {
                            tmpS.Append(" <span>(");
                            tmpS.Append(Localization.CurrencyStringForDBWithoutExchangeRate(DIDPercent));
                            tmpS.Append(" ");
                            tmpS.Append(AppLogic.GetString("shoppingcart.cs.12", m_SkinID, m_ThisCustomer.LocaleSetting));
                            tmpS.Append(")</span>");
                        }
                    }
                    tmpS.Append("</td>");
                    tmpS.Append("<td>");
                    tmpS.Append(Localization.ToThreadCultureShortDateString(c.NextRecurringShipDate));
                    tmpS.Append("</td>");
                    tmpS.Append("  </tr>");
                }
            }

            tmpS.Append("  </td>");
            tmpS.Append("  </tr>");
            tmpS.Append("</table>");
            tmpS.Append("</div>");

            return tmpS.ToString();
        }

        /// <summary>
        /// Determine if the option is seleted
        /// </summary>
        /// <param name="OptionID">The OptionID.</param>
        /// <param name="Options">The Options.</param>
        /// <returns></returns>
        public bool OptionIsSelected(int OptionID, IEnumerable<OrderOption> Options)
        {
            //return Options.Contains(OptionID);
            return Options.Any(opt => opt.ID == OptionID);
        }

        // cart state is INVALID after this call and must be reloaded if required:
        public void SetAddressesToXmlSpec(String XmlDoc)
        {
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = CommonLogic.Application("DBConn");
            dbconn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = dbconn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "aspdnsf_ReloadCart";
            cmd.Parameters.Add(new SqlParameter("@CartXML", SqlDbType.Text));
            cmd.Parameters["@CartXML"].Value = XmlDoc;
            cmd.ExecuteNonQuery();
            LoadFromDB(this.CartType);
        }

        /// <summary>
        /// Resets all address to primary shipping address.
        /// </summary>
        public void ResetAllAddressToPrimaryShippingAddress()
        {
            StringBuilder xmlDoc = new StringBuilder(4096);
            xmlDoc.Append("<root>");

            // add NEW address blocks, if necessary:
            foreach (CartItem c in CartItems)
            {
                if (!c.IsDownload && !c.IsSystem)
                {
                    for (int i = 1; i <= c.Quantity; i++)
                    {
                        int ThisAddressID = ThisCustomer.PrimaryShippingAddressID;
                        xmlDoc.Append(String.Format("<row cartid=\"{0}\" addressid=\"{1}\" />", c.ShoppingCartRecordID.ToString(), ThisAddressID.ToString()));
                    }
                }
            }
            xmlDoc.Append("</root>");
            SetAddressesToXmlSpec(xmlDoc.ToString());
        }

        // returns address block selector with (!ID!) as replacement token for you to substitute
        // NO form validation is done here, that is the caller's job
        /// <summary>
        /// Gets the generic address selection block.
        /// </summary>
        /// <param name="ViewingCustomer">The viewing customer.</param>
        /// <param name="IncludeGiftRegistrySelection">if set to <c>true</c> [include gift registry selection].</param>
        /// <param name="GiftRegistryCustomerID">The gift registry customer ID.</param>
        /// <returns></returns>
        private String GetGenericAddressSelectionBlock(Customer ViewingCustomer, bool IncludeGiftRegistrySelection, int GiftRegistryCustomerID)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            tmpS.Append("\n");
            tmpS.Append("<script type=\"text/javascript\" Language=\"JavaScript\">\n");

            tmpS.Append("function ShowExistingDiv_(!ID!)()\n");
            tmpS.Append("{\n");
            tmpS.Append("	document.getElementById('ExistingDiv_(!ID!)').style.display='block';\n");
            tmpS.Append("	document.getElementById('NewDiv_(!ID!)').style.display='none';\n");

            if (IncludeGiftRegistrySelection)
            {
                tmpS.Append("	document.getElementById('GiftDiv_(!ID!)').style.display='none';\n");
            }

            tmpS.Append("	return (true);\n");
            tmpS.Append("}\n");

            tmpS.Append("function ShowNewDiv_(!ID!)()\n");
            tmpS.Append("{\n");
            tmpS.Append("	document.getElementById('ExistingDiv_(!ID!)').style.display='none';\n");
            tmpS.Append("	document.getElementById('NewDiv_(!ID!)').style.display='block';\n");

            if (IncludeGiftRegistrySelection)
            {
                tmpS.Append("	document.getElementById('GiftDiv_(!ID!)').style.display='none';\n");
            }

            tmpS.Append("	return (true);\n");
            tmpS.Append("}\n");

            if (IncludeGiftRegistrySelection)
            {
                tmpS.Append("function ShowGiftDiv_(!ID!)()\n");
                tmpS.Append("{\n");
                tmpS.Append("	document.getElementById('ExistingDiv_(!ID!)').style.display='none';\n");
                tmpS.Append("	document.getElementById('NewDiv_(!ID!)').style.display='none';\n");
                tmpS.Append("	document.getElementById('GiftDiv_(!ID!)').style.display='block';\n");
                tmpS.Append("	return (true);\n");
                tmpS.Append("}\n");
            }

            tmpS.Append("</script>\n");


            int NumNonDefaultFound = 0;
            String ViewingCustomersAddressSelectList = Address.StaticGetAddressSelectList(ViewingCustomer, true, "(!ID!)", true, out NumNonDefaultFound);

            tmpS.Append("<nobr>");
            tmpS.Append("<span class=\"ShipToType\">");
            tmpS.Append("<input type=\"radio\" onClick=\"ShowExistingDiv_(!ID!)()\" name=\"ShipToType_(!ID!)\" id=\"ExistingShipToType_(!ID!)\" value=\"ExistingAddress\">");
            tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.13", ViewingCustomer.SkinID, ViewingCustomer.LocaleSetting));
            tmpS.Append(" <input type=\"radio\" onClick=\"ShowNewDiv_(!ID!)()\" name=\"ShipToType_(!ID!)\" id=\"NewShipToType_(!ID!)\" value=\"NewAddress\">");
            tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.14", ViewingCustomer.SkinID, ViewingCustomer.LocaleSetting));

            if (IncludeGiftRegistrySelection)
            {
                tmpS.Append(" <input type=\"radio\" onClick=\"ShowGiftDiv_(!ID!)()\" name=\"ShipToType_(!ID!)\" id=\"GiftShipToType_(!ID!)\" value=\"GiftRegistryAddress\">");
                tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.15", ViewingCustomer.SkinID, ViewingCustomer.LocaleSetting));
            }

            tmpS.Append("</span>");
            tmpS.Append("</nobr>");

            // select an existing address:
            tmpS.Append("<div id=\"ExistingDiv_(!ID!)\" style=\"display:none;\">");
            tmpS.Append(ViewingCustomersAddressSelectList);
            tmpS.Append("</div>");

            // add a new address:
            tmpS.Append("<div id=\"NewDiv_(!ID!)\" style=\"display:none;\">");
            Address addr = new Address();
            tmpS.Append(addr.InputHTML("_(!ID!)", false));
            tmpS.Append("</div>");

            // or select gift registry address:
            if (IncludeGiftRegistrySelection)
            {
                tmpS.Append("<div id=\"GiftDiv_(!ID!)\" style=\"display:none;\">");
                tmpS.Append(AppLogic.GiftRegistryDisplayName(GiftRegistryCustomerID, true, ViewingCustomer.SkinID, ViewingCustomer.LocaleSetting));
                tmpS.Append("</div>");
            }

            return tmpS.ToString();
        }

        /// <summary>
        /// Displays the multi ship method selector.
        /// </summary>
        /// <param name="VarReadOnly">if set to <c>true</c> [var read only].</param>
        /// <param name="ViewingCustomer">The viewing customer.</param>
        /// <returns></returns>
        public String DisplayMultiShipMethodSelector(bool VarReadOnly, Customer ViewingCustomer)
        {
            int ix = 1;
            StringBuilder tmpS = new StringBuilder(4096);

            //Moved javascript from checkoutshippingmult2.aspx.cs
            //so that if item does not require shipping,  we can keep the javascript 
            //validators from firing on that particular product 

            StringBuilder s = new StringBuilder("");
            StringBuilder smAddr = new StringBuilder("");

            s.Append("<script type=\"text/javascript\">\n");

            s.Append("function get_radio_value(theRadio)\n");
            s.Append("{\n");
            s.Append("for (var i=0; i < theRadio.length; i++)\n");
            s.Append("   {\n");
            s.Append("   if (theRadio[i].checked)\n");
            s.Append("      {\n");
            s.Append("      var rad_val = theRadio[i].value;\n");
            s.Append("		return rad_val;\n");
            s.Append("      }\n");
            s.Append("   }\n");
            s.Append("return '';\n");
            s.Append("}\n");

            // make sure one shipping method is selected for each address id (ignore download & system products):
            s.Append("function CheckoutShippingMult2Form_Validator(theForm)\n");
            s.Append("{\n");
            s.Append("submitonce(theForm);\n");

            if (CartType != CartTypeEnum.ShoppingCart)
            {
                return tmpS.ToString(); // can only do this action on the shoppingcart proper, not wish or registry
            }
            if (IsEmpty())
            {
                tmpS.Append("<div class=\"form-group\">");
                if (CartType == CartTypeEnum.ShoppingCart)
                {
                    Topic t1 = new Topic("EmptyCartText", m_ThisCustomer.LocaleSetting, m_SkinID, null);
                    tmpS.Append(t1.Contents);
                }
                if (CartType == CartTypeEnum.WishCart)
                {
                    Topic t1 = new Topic("EmptyWishListText", m_ThisCustomer.LocaleSetting, m_SkinID, null);
                    tmpS.Append(t1.Contents);
                }
                if (CartType == CartTypeEnum.GiftRegistryCart)
                {
                    Topic t1 = new Topic("EmptyGiftRegistryText", m_ThisCustomer.LocaleSetting, m_SkinID, null);
                    tmpS.Append(t1.Contents);
                }
                tmpS.Append("</div>");
            }
            else
            {
                bool ShowLinkBack = AppLogic.AppConfigBool("LinkToProductPageInCart");

                String LastID = String.Empty;
                if (this.HasDownloadComponents() || this.HasSystemComponents())
                {
                    tmpS.Append("<div class='page-row mult-shipping-row'>");

                    // PRODUCT DESCRIPTION COL
                    tmpS.Append("	<div class='one-half'>");
                    tmpS.Append("		<div class='group-header mult-shipping-group-header'>");
                    tmpS.Append(String.Format(AppLogic.GetString("shoppingcart.cs.102", m_SkinID, m_ThisCustomer.LocaleSetting), ix.ToString()));
                    ix++;
                    tmpS.Append("		</div>");
                    foreach (CartItem c in m_CartItems)
                    {
                        if (c.IsDownload || c.IsSystem || !c.Shippable || GiftCard.s_IsEmailGiftCard(c.ProductID))
                        {
                            tmpS.Append("<div class='multi-ship-item-wrap'>");
                            tmpS.Append(GetLineItemDescription(c, ShowLinkBack, VarReadOnly, false));
                            // include Quantity now:
                            tmpS.Append(AppLogic.GetString("shoppingcart.cs.89", m_SkinID, m_ThisCustomer.LocaleSetting));
                            tmpS.Append(": ");
                            tmpS.Append(c.Quantity.ToString());
                            tmpS.Append("</div>");
                        }
                    }
                    tmpS.Append("	</div>");

                    // SHIPPING METHOD COL
                    tmpS.Append("	<div class='one-half'>");
                    tmpS.Append("		<div class='group-header mult-shipping-group-header'>");
                    tmpS.Append(AppLogic.GetString("shoppingcart.cs.101", m_SkinID, m_ThisCustomer.LocaleSetting));
                    tmpS.Append("		</div>");
                    tmpS.Append("		<div class='mult-shipping-no-ship-reason'>");
                    tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.24", SkinID, ThisCustomer.LocaleSetting));
                    tmpS.Append("		</div>");

                    tmpS.Append("	</div>");
                    tmpS.Append("</div>");
                }

                Boolean CustomerLevelHasFreeShipping = false;
                if (ThisCustomer.CustomerLevelID > 0)
                {
                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        string query = "SELECT LevelHasFreeShipping FROM CustomerLevel WHERE CustomerLevelID=" + ThisCustomer.CustomerLevelID.ToString();
                        using (IDataReader clvl = DB.GetRS(query, con))
                        {
                            while (clvl.Read())
                            {
                                CustomerLevelHasFreeShipping = DB.RSFieldBool(clvl, "LevelHasFreeShipping");
                            }
                        }
                    }
                }

                List<int> DistinctAddrIds = Shipping.GetDistinctShippingAddressIDs(CartItems);
                if (DistinctAddrIds.Count != 0)
                {
                    foreach (int ThisAddressID in DistinctAddrIds)
                    {
                        // create a string so we don't have .ToString() everywhere
                        String AddressID = ThisAddressID.ToString();

                        bool showShipping = false;  //will set to show or hide shipping groups if the items don't require shipping

                        foreach (CartItem c in m_CartItems)
                        {
                            if (!c.IsDownload && !c.IsSystem && c.ShippingAddressID == ThisAddressID && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID))
                            {
                                showShipping = true;
                            }
                        }

                        if (showShipping)
                        {
                            //add the hidden input
                            smAddr.Append("<input type=\"hidden\" id=\"ShippingMethodID_" + AddressID + "\" name=\"ShippingMethodID_" + AddressID + "\" value=\"\">"); // must have this so all ShippingMethodID radio lists are arrays, even if there is only ONE radio button on the form (javascript)

                            s.Append("myOption" + AddressID + " = -1;\n");
                            s.Append("for(i = 0; i < theForm.ShippingMethodID_" + AddressID + ".length; i++)\n");
                            s.Append("{\n");
                            s.Append("	if (theForm.ShippingMethodID_" + AddressID + "[i].checked)\n");
                            s.Append("	{\n");
                            s.Append("		myOption" + AddressID + " = i;\n");
                            s.Append("	}\n");
                            s.Append("}\n");
                            s.Append("if(myOption" + AddressID + " == -1)\n");
                            s.Append("{\n");
                            s.Append("    alert(\"" + AppLogic.GetString("checkoutshippingmult2.aspx.21", SkinID, ThisCustomer.LocaleSetting) + "\");\n");
                            s.Append("	  theForm.ContinueCheckout.value='0';\n");
                            s.Append("    submitenabled(theForm);\n");
                            s.Append("    return (false);\n");
                            s.Append("}\n");

                            tmpS.Append("<div class='page-row mult-shipping-row'>");
                            // PRODUCT DESCRIPTION COL
                            tmpS.Append("<div class='one-half'>");
                            tmpS.Append("	<div class='group-header mult-shipping-group-header'>");
                            tmpS.Append(String.Format(AppLogic.GetString("shoppingcart.cs.102", m_SkinID, m_ThisCustomer.LocaleSetting), ix.ToString()));
                            ix++;
                            tmpS.Append("	</div>");
                            foreach (CartItem c in m_CartItems)
                            {
                                if (!c.IsDownload && !c.IsSystem && c.ShippingAddressID == ThisAddressID && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID))
                                {
                                    tmpS.Append("<div class='multi-ship-item-wrap'>");
                                    tmpS.Append(GetLineItemDescription(c, ShowLinkBack, VarReadOnly, false));
                                    // include Quantity now:
                                    tmpS.Append(AppLogic.GetString("shoppingcart.cs.89", m_SkinID, m_ThisCustomer.LocaleSetting));
                                    tmpS.Append(": ");
                                    tmpS.Append(c.Quantity.ToString());
                                    tmpS.Append("</div>");
                                }
                            }
                            tmpS.Append("</div>");

                            // SHIPING METHOD COL
                            tmpS.Append("<div class='one-half'>");
                            tmpS.Append("	<div class='group-header mult-shipping-group-header'>");
                            tmpS.Append(AppLogic.GetString("shoppingcart.cs.101", m_SkinID, m_ThisCustomer.LocaleSetting));
                            tmpS.Append("	</div>");
                            Address adr = new Address();
                            adr.LoadFromDB(ThisAddressID);
                            tmpS.Append("<span class='multi-ship-address-header'>");
                            tmpS.Append(AppLogic.GetString("shoppingcart.cs.103", m_SkinID, m_ThisCustomer.LocaleSetting));
                            tmpS.Append("</span>");
                            tmpS.Append("<div class='multi-ship-address'>");
                            if (m_ThisCustomer.CustomerID == adr.CustomerID)
                            {
                                tmpS.Append(adr.DisplayHTML(true));
                            }
                            else
                            {
                                tmpS.Append(String.Format(AppLogic.GetString("shoppingcart.cs.92", SkinID, ThisCustomer.LocaleSetting), AppLogic.GiftRegistryDisplayName(adr.CustomerID, false, SkinID, ThisCustomer.LocaleSetting)));
                            }
                            tmpS.Append("</div>");

                            // GET CART TO SHOW SHIPPING METHODS & COSTS FOR ONLY THIS GROUP OF ITEMS BY ADDRESS ID
                            CartItemCollection origCartItems = m_CartItems; // Save the original cart item list
                            CartItemCollection tempItems = origCartItems.Clone(); //Clone the cart item list
                            FilterItemAddressList(tempItems, ThisAddressID); // filter the cart items to only leave those going to this shipping address id
                            m_CartItems = tempItems;
                            bool AnyShippingMethodsFound = false;

                            bool IsAllFreeShipping = DB.GetSqlN("select count(*) N from dbo.shoppingcart where FreeShipping<>1 and ShippingAddressID = " + AddressID + " and customerid = " + ThisCustomer.CustomerID) == 0;

                            AnalyzeCartForFreeShippingConditions(ThisAddressID);

                            String ShipMethods = GetShippingMethodListForAddress("_" + ThisAddressID.ToString(), ThisAddressID, IsAllFreeShipping || ShippingIsFree, out AnyShippingMethodsFound);
                            String ShipSelection = String.Empty;
                            if (this.CartAllowsShippingMethodSelection)
                            {
                                ShipSelection = string.Empty;

                                if (!AppLogic.AppConfigBool("FreeShippingAllowsRateSelection") && (IsAllFreeShipping || (!AnyShippingMethodsFound && this.ShippingIsFree) || CustomerLevelHasFreeShipping || ShippingIsFree))
                                {
                                    tmpS.Append("<div class=\"form-group\">");
                                    AnyShippingMethodsFound = true;
                                    String strValue = Shipping.GetFirstFreeShippingMethodID().ToString();
                                    if (ShipCalcID == Shipping.ShippingCalculationEnum.UseRealTimeRates)
                                    {
                                        strValue += "|" + FreeShippingMethod + "|0.00|0.00";
                                    }
                                    tmpS.Append("<input type=\"radio\" value=\"" + strValue + "\" name=\"ShippingMethodID_" + AddressID + "\" id=\"FreeShippingMethodID_" + AddressID + "\" checked>");
                                    tmpS.Append(AppLogic.GetString("shoppingcart.cs.104", m_SkinID, m_ThisCustomer.LocaleSetting));
                                    tmpS.Append("</div>");
                                }
                                else
                                {
                                    tmpS.Append("<div class=\"form shipping-methods\">");
                                    tmpS.Append("	<div class=\"form-header shipping-method-header multi-ship-method-header\">");
                                    tmpS.Append(AppLogic.GetString("shoppingcart.cs.40", m_SkinID, m_ThisCustomer.LocaleSetting));
                                    tmpS.Append("	</div>");
                                    tmpS.Append(ShipMethods);
                                    tmpS.Append("</div>");
                                }
                            }

                            m_CartItems = origCartItems; // Restore the original cart item list

                            tmpS.Append("</div>");
                            tmpS.Append("</div>");
                        }
                        else
                        {
                            //Shipping not required...so add a generic hidden input in a format the store understands
                            String iValue = String.Format("{0}|{1}|{2}|{3}", "0", AppLogic.GetString("checkoutshippingmult.aspx.28", SkinID, ThisCustomer.LocaleSetting), "0.00", "0.00");
                            smAddr.Append("<input type=\"hidden\" id=\"ShippingMethodID_" + AddressID + "\" name=\"ShippingMethodID_" + AddressID + "\" value=\"" + iValue + "\">"); // must have this so all ShippingMethodID radio lists are arrays, even if there is only ONE radio button on the form (javascript)
                        }
                    }
                }

                s.Append("submitenabled(theForm);\n");
                s.Append("return (true);\n");
                s.Append("}\n");
                s.Append("</script>\n");

                s.Append("<form action='checkoutshippingmult2.aspx' method='post' id='CheckoutShippingMult2Form' name='CheckoutShippingMult2Form' onsubmit='return (validateForm(this) && CheckoutShippingMult2Form_Validator(this))'>\n");
                s.Append("<input type='hidden' id='ContinueCheckout' name='ContinueCheckout' value='0'>");

                s.Append(smAddr.ToString());

                s.Append(tmpS.ToString());

                s.Append("<div class='form-submit-wrap'>");
                s.Append("<input type='submit' class='button update-button' name='update' value='" + "checkoutshippingmult2.aspx.19".StringResource() + "'>");
                s.Append(" <input type='submit' class='button call-to-action' name='continue' value='" + "checkoutshippingmult2.aspx.20".StringResource() + "'>");
                s.Append("</div>");

                s.Append("</form>");
            }
            return s.ToString();
        }

        /// <summary>
        /// Gets the shipping methods.
        /// </summary>
        /// <param name="usingThisAddress">The current address.</param>
        /// <returns></returns>
        public ShippingMethodCollection GetShippingMethods(Address usingThisAddress)
        {
            return GetShippingMethods(usingThisAddress, true);
        }

        /// <summary>
        /// Gets the shipping methods.
        /// </summary>
        /// <param name="usingThisAddress">The current address.</param>
        /// <returns></returns>
        public ShippingMethodCollection GetShippingMethods(Address usingThisAddress, bool getInStorePickupRate)
        {
            decimal taxRate = decimal.Zero;
            if (m_VATOn)
            {
                taxRate = Prices.TaxRate(ThisCustomer, AppLogic.AppConfigUSInt("ShippingTaxClassID")) / 100.0M;
            }

            return GetShippingMethods(usingThisAddress, taxRate, getInStorePickupRate);
        }

        /// <summary>
        /// Gets the active shipping calculation.
        /// </summary>
        /// <returns></returns>
        public IShippingCalculation GetActiveShippingCalculation()
        {
            IShippingCalculation shippingCalculation = null;
            switch (ShipCalcID)
            {
                case Shipping.ShippingCalculationEnum.CalculateShippingByWeight:
                    shippingCalculation = new CalculateShippingByWeightShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.CalculateShippingByTotal:
                    shippingCalculation = new CalculateShippingByTotalShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent:
                    shippingCalculation = new CalculateShippingByTotalByPercentShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.UseFixedPrice:
                    shippingCalculation = new UseFixedPriceShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping:
                    shippingCalculation = new AllOrdersHaveFreeShippingShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal:
                    shippingCalculation = new UseFixedPercentageOfTotalShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts:
                    shippingCalculation = new UseIndividualItemShippingCostsShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone:
                    shippingCalculation = new CalculateShippingByWeightAndZoneShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone:
                    shippingCalculation = new CalculateShippingByTotalAndZoneShippingCalculation();
                    break;

                case Shipping.ShippingCalculationEnum.UseRealTimeRates:
                    shippingCalculation = new UseRealTimeRatesShippingCalculation();
                    break;
            }

            return shippingCalculation;
        }

        /// <summary>
        /// Summary for Shipping method
        /// </summary>
        /// <param name="usingThisAddress">Address</param>
        /// <param name="taxRate">taxrate</param>
        /// <returns>Return a collection of Shipping methods & rates based on the shipping calculation method being used</returns>
        public ShippingMethodCollection GetShippingMethods(Address usingThisAddress, decimal taxRate)
        {
            return GetShippingMethods(usingThisAddress, taxRate, true);
        }

        /// <summary>
        /// Summary for Shipping method
        /// </summary>
        /// <param name="usingThisAddress">Address</param>
        /// <param name="taxRate">taxrate</param>
        /// <param name="returnInStorePickupRate">returnInStorePickupRate</param>
        /// <returns>Return a collection of Shipping methods & rates based on the shipping calculation method being used</returns>
        public ShippingMethodCollection GetShippingMethods(Address usingThisAddress, decimal taxRate, bool returnInStorePickupRate)
        {
            ShippingMethodCollection availableShippingMethods = new ShippingMethodCollection();
            bool shouldWeGetShippingMethods = true;

            if (AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
            {
                if (IsEmpty() ||
                    this.IsAllDownloadComponents() ||
                    this.IsAllSystemComponents() ||
                    NoShippingRequiredComponents() ||
                    IsAllEmailGiftCards())
                {
                    shouldWeGetShippingMethods = false;
                }
            }
            else
            {
                if (IsEmpty() ||
                    this.IsAllDownloadComponents() ||
                    this.IsAllFreeShippingComponents() ||
                    this.IsAllSystemComponents() ||
                    NoShippingRequiredComponents() ||
                    IsAllEmailGiftCards())
                {
                    shouldWeGetShippingMethods = false;
                }
            }

            if (shouldWeGetShippingMethods)
            {
                decimal ExtraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");
                decimal SubTotalWithoutDownload = SubTotal(true, false, false, false);
                StringBuilder tmpS = new StringBuilder(4096);

                IShippingCalculation shippingCalculation = GetActiveShippingCalculation();

                if (shippingCalculation != null)
                {
                    shippingCalculation.ThisCustomer = ThisCustomer;
                    shippingCalculation.Cart = this;
                    shippingCalculation.ShippingAddress = usingThisAddress;
                    shippingCalculation.TaxRate = taxRate;
                    shippingCalculation.ExcludeZeroFreightCosts = AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost");
                    shippingCalculation.ShippingIsFreeIfIncludedInFreeList = this.ShippingIsFree;
                    shippingCalculation.HandlingExtraFee = ExtraFee;

                    var usedStoreId = AppLogic.StoreID();
                    if (AppLogic.GlobalConfigBool("AllowShippingFiltering") == false)
                    {
                        usedStoreId = Shipping.DONT_FILTER_PER_STORE;
                    }

                    availableShippingMethods = shippingCalculation.GetShippingMethods(usedStoreId);

                    if (AppLogic.AppConfigBool("RTShipping.AllowLocalPickup"))
                    {
                        bool pickupIsAllowed = false;

                        String restrictiontype = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionType").Trim();

                        if (restrictiontype.Equals("zone", StringComparison.InvariantCultureIgnoreCase))
                        {
                            String[] allowedzones = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZones").Trim().Split(',');

                            int zoneforzip = Shipping.ZoneLookup(ThisCustomer.PrimaryShippingAddress.Zip);

                            foreach (String allowedzone in allowedzones)
                            {
                                if (int.Parse(allowedzone) == zoneforzip)
                                {
                                    pickupIsAllowed = true;
                                }
                            }
                        }
                        else if (restrictiontype.Equals("zip", StringComparison.InvariantCultureIgnoreCase))
                        {
                            String[] allowedzips = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZips").Trim().Split(',');

                            foreach (String allowedzip in allowedzips)
                            {
                                if (ThisCustomer.PrimaryShippingAddress.Zip == allowedzip)
                                {
                                    pickupIsAllowed = true;
                                }
                            }

                        }
                        else if (restrictiontype.Equals("state", StringComparison.InvariantCultureIgnoreCase))
                        {
                            String allowedstateids = AppLogic.AppConfig("RTShipping.LocalpickupRestrictionStates").Trim();

                            try
                            {
                                using (SqlConnection dbconn = DB.dbConn())
                                {
                                    dbconn.Open();
                                    using (IDataReader rs = DB.GetRS("select * from State with (NOLOCK) where StateID in (" + allowedstateids + ")", dbconn))
                                    {
                                        while (rs.Read())
                                        {
                                            if (ThisCustomer.PrimaryShippingAddress.StateID == DB.RSFieldInt(rs, "StateID"))
                                            {
                                                pickupIsAllowed = true;
                                            }
                                        }
                                    }
                                }

                            }
                            catch
                            {
                                pickupIsAllowed = false;
                            }

                        }
                        else
                        {
                            pickupIsAllowed = true;
                        }

                        if (pickupIsAllowed && returnInStorePickupRate)
                        {
                            ShippingMethod s_methodPickup = new ShippingMethod();
                            s_methodPickup.Name = AppLogic.GetString("RTShipping.LocalPickupMethodName", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                            s_methodPickup.Freight = AppLogic.AppConfigNativeDecimal("RTShipping.LocalPickupCost");
                            s_methodPickup.ShippingIsFree = s_methodPickup.Freight == AppLogic.AppConfigNativeDecimal("RTShipping.LocalPickupCost");
                            availableShippingMethods.Add(s_methodPickup);
                        }
                    }

                    if (availableShippingMethods.Count == 0 &&
                        usedStoreId != Shipping.DONT_FILTER_PER_STORE)
                    {
                        // no shipping methods found, let's try and fallback to the default store
                        // (if we're not already in it)
                        // when we're on a multi-store scenario

                        var stores = Store.GetStoreList();
                        if (stores.Count > 1)
                        {
                            var defStore = stores.FirstOrDefault(store => store.IsDefault);
                            if (defStore != null &&
                                defStore.StoreID != usedStoreId)
                            {
                                usedStoreId = defStore.StoreID;
                                availableShippingMethods = shippingCalculation.GetShippingMethods(usedStoreId);
                            }
                        }

                        // log if we still didn't find any shipping methods
                        if (availableShippingMethods.Count == 0)
                        {
                            string message = "No Shipping method found for StoreId: {0}".FormatWith(usedStoreId);

                            string logDetails = string.Empty;
                            if (ThisCustomer.IsRegistered)
                            {
                                // prepare a formatted log 
                                logDetails = @"
                                            StoreId: {0}
                                            CalculationMode: {1}
                                            CustomerId: {2}                                              
                                            Address: 
                                                {3}"
                                            .FormatWith(usedStoreId, ShipCalcID, ThisCustomer.CustomerID, usingThisAddress.DisplayHTML(true));
                            }

                            SysLog.LogMessage(message, logDetails, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                        }

                    }
                }
            }

            return availableShippingMethods;
        }

        /// <summary>
        /// Gets the shipping method list.
        /// </summary>
        /// <param name="FieldSuffix">The field suffix.</param>
        /// <param name="AnyShippingMethodsFound">if set to <c>true</c> [any shipping methods found].</param>
        /// <returns></returns>
        public String GetShippingMethodList(String FieldSuffix, out bool AnyShippingMethodsFound)
        {
            AnyShippingMethodsFound = false;
            if (AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
            {
                if (IsEmpty() || this.IsAllDownloadComponents() || this.IsAllSystemComponents() || NoShippingRequiredComponents() || IsAllEmailGiftCards())
                {
                    return String.Empty;
                }
            }
            else
            {
                if (IsEmpty() || this.IsAllDownloadComponents() || this.IsAllFreeShippingComponents() || this.IsAllSystemComponents() || NoShippingRequiredComponents() || IsAllEmailGiftCards())
                {
                    return String.Empty;
                }
            }
            decimal ExtraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");
            //value without tax so that shipping by value method don't calulate shipping with the tax amount included
            //include items that are for free shipping
            decimal SubTotalWithoutDownload = SubTotal(true, false, false, true, true, false, 0, true);
            StringBuilder tmpS = new StringBuilder(4096);

            bool ShippingMethodToStateMapIsEmpty = Shipping.ShippingMethodToStateMapIsEmpty();
            bool ShippingMethodToCountryMapIsEmpty = Shipping.ShippingMethodToCountryMapIsEmpty();
            bool ShippingMethodToZoneMapIsEmpty = Shipping.ShippingMethodToZoneMapIsEmpty();

            int FirstItemShippingAddressID = FirstItem().ShippingAddressID;
            Address FirstItemShippingAddress = new Address();
            FirstItemShippingAddress.LoadFromDB(FirstItemShippingAddressID);

            decimal ShippingTaxRate = Prices.TaxRate(ThisCustomer, AppLogic.AppConfigUSInt("ShippingTaxClassID")) / 100.0M;

            if (FreeShippingReason == Shipping.FreeShippingReasonEnum.AllDownloadItems || FreeShippingReason == Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping)
            {
                tmpS.Append(string.Format("<input type=\"radio\" name=\"ShippingMethodID{0}\" id=\"ShippingMethodID{0}0\" value=\"0\" checked > " + String.Format(AppLogic.GetString("shoppingcart.cs.105", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), GetFreeShippingReason()) + "", FieldSuffix));
                AnyShippingMethodsFound = true;
            }


            switch (ShipCalcID)
            {
                case Shipping.ShippingCalculationEnum.CalculateShippingByWeight:
                    {
                        StringBuilder shipsql = new StringBuilder(4096);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }

                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 0;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" ");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append(" ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByWeightCharge(ThisID, WeightTotal());

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }

                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" ");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append(" ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotal:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }

                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" ");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append(" ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByTotalCharge(ThisID, SubTotalWithoutDownload); // exclude download items!

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" ");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append(" ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }

                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" ");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append(" ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByTotalByPercentCharge(ThisID, SubTotalWithoutDownload); // exclude download items!

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" ");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append(" ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseFixedPrice:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }

                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" ");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append(" ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = (decimal)DB.RSFieldDecimal(rs, "FixedRate");

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {

                                            tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" ");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append(" ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping:
                    tmpS.Append("<input type=\"hidden\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + "\" value=\"" + Shipping.GetFirstFreeShippingMethodID() + "\" checked >");
                    AnyShippingMethodsFound = true;
                    break;
                case Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }

                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" ");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append(" ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByFixedPercentageCharge(ThisID, SubTotalWithoutDownload);

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" ");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append(" ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }

                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" ");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append(" ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByItemCharge(ThisID, m_CartItems);

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" ");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append(" ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }

                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + Shipping.ZoneLookup(FirstItemShippingAddress.Zip).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" ");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append(" ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                    }
                                    else
                                    {
                                        int ZoneID = Shipping.ZoneLookup(FirstItemShippingAddress.Zip);
                                        Decimal ThisShipCost = Shipping.GetShipByWeightAndZoneCharge(ThisID, WeightTotal(), ZoneID);

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != -1 && (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost")))
                                        {
                                            tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" ");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append(" ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }

                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }

                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + Shipping.ZoneLookup(FirstItemShippingAddress.Zip).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" ");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append(" ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                    }
                                    else
                                    {
                                        int ZoneID = Shipping.ZoneLookup(FirstItemShippingAddress.Zip);
                                        Decimal ThisShipCost = Shipping.GetShipByTotalAndZoneCharge(ThisID, SubTotalWithoutDownload, ZoneID);

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != -1 && (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost")))
                                        {
                                            tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" ");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append(" ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseRealTimeRates:
                    {
                        string FreeShippingMethodIDs = string.Empty;

                        FreeShippingMethodIDs = Shipping.GetFreeShippingMethodIDs();

                        String RTRates = GetRTShippingRates(FirstItemShippingAddress, FieldSuffix, FreeShippingMethodIDs).Trim();
                        // mark currently selected option (prior chosen by customer):
                        AnyShippingMethodsFound = (RTRates.Length != 0 && RTRates.IndexOf("input type=\"radio\"") != -1);
                        if (AnyShippingMethodsFound)
                        {
                            int TheSelectedID = FirstItem().ShippingMethodID;
                            RTRates = RTRates.Replace("value=\"" + TheSelectedID, "checked value=\"" + TheSelectedID);
                            if (ShippingIsFree)
                            {
                                RTRates = RTRates.Replace(Localization.CurrencyStringForDisplayWithExchangeRate(0.0M, ThisCustomer.CurrencySetting), "(" + AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting) + ")");
                            }
                            tmpS.Append(RTRates);
                        }
                        break;
                    }
            }
            if (!AnyShippingMethodsFound)
            {
                tmpS.Append("<input type=\"radio\" name=\"ShippingMethodID\" id=\"ShippingMethodID\" value=\"0\" checked />");
                tmpS.Append(" ");
                tmpS.Append(String.Format("({0})", AppLogic.GetString("checkoutshipping.aspx.12", SkinID, ThisCustomer.LocaleSetting)));
            }
            else
            {
                tmpS.Append("\n<input type=\"hidden\" name=\"RequireShippingSelection\" value=\"true\">");
            }
            return tmpS.ToString();
        }

        /// <summary>
        /// Gets the shipping method list for address.
        /// </summary>
        /// <param name="FieldSuffix">The field suffix.</param>
        /// <param name="AddressID">The address ID.</param>
        /// <param name="IsAllFreeShipping">if set to <c>true</c> [is all free shipping].</param>
        /// <param name="AnyShippingMethodsFound">if set to <c>true</c> [any shipping methods found].</param>
        /// <returns></returns>
        public String GetShippingMethodListForAddress(String FieldSuffix, int AddressID, bool IsAllFreeShipping, out bool AnyShippingMethodsFound)
        {
            AnyShippingMethodsFound = false;
            if (AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
            {
                if (IsEmpty()
                    || this.IsAllDownloadComponents()
                    || this.IsAllSystemComponents()
                    || IsAllEmailGiftCards()
                    || NoShippingRequiredComponents())
                {
                    return String.Empty;
                }
            }
            else
            {
                if (IsEmpty()
                    || this.IsAllDownloadComponents()
                    || this.IsAllFreeShippingComponents()
                    || this.IsAllSystemComponents()
                    || IsAllEmailGiftCards()
                    || NoShippingRequiredComponents())
                {
                    return String.Empty;
                }
            }
            decimal ExtraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");
            bool tmpVatSetting = m_VATOn;
            m_VATOn = false;
            decimal SubTotalWithoutDownload = SubTotal(true, false, false, IsAllFreeShipping, true, false, AddressID, true); //value without tax so that shipping by value method don't calulate shipping with the tax amount included
            m_VATOn = tmpVatSetting;
            StringBuilder tmpS = new StringBuilder(4096);

            bool ShippingMethodToStateMapIsEmpty = Shipping.ShippingMethodToStateMapIsEmpty();
            bool ShippingMethodToCountryMapIsEmpty = Shipping.ShippingMethodToCountryMapIsEmpty();
            bool ShippingMethodToZoneMapIsEmpty = Shipping.ShippingMethodToZoneMapIsEmpty();

            Address FirstItemShippingAddress = new Address();
            FirstItemShippingAddress.LoadFromDB(AddressID);

            decimal ShippingTaxRate = decimal.Zero;
            Address shipAdr = new Address();
            shipAdr.LoadByCustomer(ThisCustomer.CustomerID, AddressID, AddressTypes.Shipping);
            ShippingTaxRate = Prices.TaxRate(ThisCustomer, AppLogic.AppConfigUSInt("ShippingTaxClassID")) / 100.0M;

            if (FreeShippingReason == Shipping.FreeShippingReasonEnum.AllDownloadItems || FreeShippingReason == Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping)
            {
                tmpS.Append(string.Format("<div class=\"form-group\"><input type=\"radio\" name=\"ShippingMethodID{0}\" id=\"ShippingMethodID{0}0\" value=\"0\" checked > " + String.Format(AppLogic.GetString("shoppingcart.cs.105", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), GetFreeShippingReason()) + "", FieldSuffix) + "</div>");
                AnyShippingMethodsFound = true;
            }


            switch (ShipCalcID)
            {
                case Shipping.ShippingCalculationEnum.CalculateShippingByWeight:
                    {
                        StringBuilder shipsql = new StringBuilder(4096);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty && !ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }
                        else if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State).ToString() + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 0;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (IsAllFreeShipping && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" <span>");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append("</span> ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                        tmpS.Append("</div>");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByWeightCharge(ThisID, WeightTotalByAddress(AddressID));

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }

                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" <span>");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append("</span> ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                            tmpS.Append("</div>");
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotal:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty && !ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }
                        else if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State).ToString() + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (IsAllFreeShipping && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" <span>");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append("</span> ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                        tmpS.Append("</div>");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByTotalCharge(ThisID, SubTotalWithoutDownload); // exclude download items!

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" <span>");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append("</span> ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                            tmpS.Append("</div>");
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty && !ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }
                        else if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State).ToString() + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (IsAllFreeShipping && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" <span>");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append("</span> ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                        tmpS.Append("</div>");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByTotalByPercentCharge(ThisID, SubTotalWithoutDownload); // exclude download items!

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" <span>");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append("</span> ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                            tmpS.Append("</div>");
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseFixedPrice:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty && !ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }
                        else if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State).ToString() + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (IsAllFreeShipping && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append("<span>");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append("</span>");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                        tmpS.Append("</div>");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = (decimal)DB.RSFieldDecimal(rs, "FixedRate");

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append("<span>");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append("</span>");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                            tmpS.Append("</div>");
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping:
                    tmpS.Append("<input type=\"hidden\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + "\" value=\"" + Shipping.GetFirstFreeShippingMethodID() + "\" checked >");
                    AnyShippingMethodsFound = true;
                    break;
                case Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty && !ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }
                        else if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State).ToString() + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (IsAllFreeShipping && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" <span>");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append("</span> ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                        tmpS.Append("</div>");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByFixedPercentageCharge(ThisID, SubTotalWithoutDownload);

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" <span>");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append("</span> ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                            tmpS.Append("</div>");
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    tmpS.Append("");
                                    i++;
                                }
                                rs.Close();
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty && !ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }
                        else if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State).ToString() + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (IsAllFreeShipping && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" <span>");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append("</span> ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                        tmpS.Append("</div>");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByItemCharge(ThisID, m_CartItems);

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" <span>");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append("</span> ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                            tmpS.Append("</div>");
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty && !ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }
                        else if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State).ToString() + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + Shipping.ZoneLookup(FirstItemShippingAddress.Zip).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");
                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (IsAllFreeShipping && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" <span>");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append("</span> ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                        tmpS.Append("</div>");
                                    }
                                    else
                                    {
                                        int ZoneID = Shipping.ZoneLookup(FirstItemShippingAddress.Zip);
                                        Decimal ThisShipCost = Shipping.GetShipByWeightAndZoneCharge(ThisID, WeightTotalByAddress(AddressID), ZoneID);

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }

                                        if (ThisShipCost != -1 && (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost")))
                                        {
                                            tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" <span>");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append("</span> ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                            tmpS.Append("</div>");
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone:
                    {
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty && !ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State, AppLogic.GetCountryID(FirstItemShippingAddress.Country)).ToString() + ")");
                        }
                        else if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + AppLogic.GetStateID(FirstItemShippingAddress.State).ToString() + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + AppLogic.GetCountryID(FirstItemShippingAddress.Country).ToString() + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + Shipping.ZoneLookup(FirstItemShippingAddress.Zip).ToString() + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        int i = 1;
                        int zeroShipCostCountThatShouldBeFiltered = 1;

                        SqlConnection con = null;
                        IDataReader rs = null;

                        try
                        {
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(shipsql.ToString(), m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(shipsql.ToString(), con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    AnyShippingMethodsFound = true;
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (IsAllFreeShipping && Shipping.ShippingMethodIsInFreeList(ThisID))
                                    {
                                        tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                        tmpS.Append(" <span>");
                                        tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                        tmpS.Append("</span> ");
                                        tmpS.Append(String.Format("({0})", AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting)));
                                        tmpS.Append("</div>");
                                    }
                                    else
                                    {
                                        int ZoneID = Shipping.ZoneLookup(FirstItemShippingAddress.Zip);
                                        Decimal ThisShipCost = Shipping.GetShipByTotalAndZoneCharge(ThisID, SubTotalWithoutDownload, ZoneID);

                                        if (ExtraFee != System.Decimal.Zero && m_ShipCalcID != Shipping.ShippingCalculationEnum.UseRealTimeRates && ThisShipCost != System.Decimal.Zero)
                                        {
                                            ThisShipCost += ExtraFee;
                                        }

                                        if (m_VATOn)
                                        {
                                            ThisShipCost += Decimal.Round(ThisShipCost * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                        }
                                        if (ThisShipCost != -1 && (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost")))
                                        {
                                            tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + i.ToString() + "\" value=\"" + ThisID.ToString() + "\" " + CommonLogic.IIF((FirstItem().ShippingMethodID == ThisID), " checked ", "") + ">");
                                            tmpS.Append(" <span>");
                                            tmpS.Append(DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting));
                                            tmpS.Append("</span> ");
                                            tmpS.Append(String.Format("({0})", m_ThisCustomer.CurrencyString(ThisShipCost)));
                                            tmpS.Append("</div>");
                                        }

                                        if (ThisShipCost == decimal.Zero && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            zeroShipCostCountThatShouldBeFiltered++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }
                            // make sure we won't reference this again
                            con = null;
                            rs = null;
                        }

                        AnyShippingMethodsFound = (i > zeroShipCostCountThatShouldBeFiltered);
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseRealTimeRates:
                    {
                        string FreeShippingMethodIDs = string.Empty;

                        FreeShippingMethodIDs = Shipping.GetFreeShippingMethodIDs();

                        String RTRates = GetRTShippingRates(FirstItemShippingAddress, FieldSuffix, FreeShippingMethodIDs).Trim();

                        // mark currently selected option (prior chosen by customer):
                        AnyShippingMethodsFound = (RTRates.Length != 0 && RTRates.IndexOf("input type=\"radio\"") != -1);
                        if (AnyShippingMethodsFound)
                        {
                            int TheSelectedID = FirstItem().ShippingMethodID;
                            RTRates = RTRates.Replace("value=\"" + TheSelectedID, "checked value=\"" + TheSelectedID);
                            RTRates = RTRates.Replace(Localization.CurrencyStringForDisplayWithExchangeRate(0.0M, ThisCustomer.CurrencySetting), "(" + AppLogic.GetString("shoppingcart.aspx.16", SkinID, ThisCustomer.LocaleSetting) + ")");
                            tmpS.Append(RTRates);
                        }
                        break;
                    }
            }
            if (!AnyShippingMethodsFound)
            {
                //Check for IsAllEmailGiftCards() and for NoShippingRequiredComponents()
                if (IsAllEmailGiftCards())
                {
                    AnyShippingMethodsFound = true;
                    String RadioValue = String.Format("{0}|{1}|{2}|{3}", "0", AppLogic.GetString("checkoutshippingmult.aspx.28", SkinID, ThisCustomer.LocaleSetting), decimal.Zero.ToString(), decimal.Zero.ToString());
                    tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + 0 + "\" value=\"" + RadioValue + "\">");
                    tmpS.Append(" <span>");
                    tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.28", SkinID, ThisCustomer.LocaleSetting));
                    tmpS.Append("</span></div>");
                }
                else if (NoShippingRequiredComponents())
                {
                    AnyShippingMethodsFound = true;
                    String RadioValue = String.Format("{0}|{1}|{2}|{3}", "0", AppLogic.GetString("checkoutshippingmult.aspx.28", SkinID, ThisCustomer.LocaleSetting), decimal.Zero.ToString(), decimal.Zero.ToString());
                    tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + 0 + "\" value=\"" + RadioValue + "\">");
                    tmpS.Append(" <span>");
                    tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.28", SkinID, ThisCustomer.LocaleSetting));
                    tmpS.Append("</span></div>");
                }
                else
                {
                    tmpS.Append("<div><input type=\"radio\" name=\"ShippingMethodID\" id=\"ShippingMethodID\" value=\"0\" checked />");
                    tmpS.Append(" <span>");
                    tmpS.Append(String.Format("({0})", AppLogic.GetString("checkoutshipping.aspx.12", SkinID, ThisCustomer.LocaleSetting)));
                    tmpS.Append("</span></div>");
                }
            }
            return tmpS.ToString();
        }

        /// <summary>
        /// Displays the MultiShipping address selector.
        /// </summary>
        /// <param name="VarReadOnly">if set to <c>true</c> [var read only].</param>
        /// <param name="ViewingCustomer">The viewing customer.</param>
        /// <returns></returns>
        public String DisplayMultiShipAddressSelector(bool VarReadOnly, Customer ViewingCustomer)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            if (CartType != CartTypeEnum.ShoppingCart)
            {
                return tmpS.ToString(); // can only do this action on the shoppingcart proper, not wish or registry
            }
            if (IsEmpty())
            {
                if (CartType == CartTypeEnum.ShoppingCart)
                {
                    Topic t1 = new Topic("EmptyCartText", m_ThisCustomer.LocaleSetting, m_SkinID, null);
                    tmpS.Append(t1.Contents);
                }
                if (CartType == CartTypeEnum.WishCart)
                {
                    Topic t1 = new Topic("EmptyWishListText", m_ThisCustomer.LocaleSetting, m_SkinID, null);
                    tmpS.Append(t1.Contents);
                }
                if (CartType == CartTypeEnum.GiftRegistryCart)
                {
                    Topic t1 = new Topic("EmptyGiftRegistryText", m_ThisCustomer.LocaleSetting, m_SkinID, null);
                    tmpS.Append(t1.Contents);
                }
                tmpS.Append("");
            }
            else
            {
                bool ShowLinkBack = AppLogic.AppConfigBool("LinkToProductPageInCart");
                bool ShowPicsInCart = false; // no space for it here, so forcefully turn them off!
                int ColSpan = CommonLogic.IIF(ShowPicsInCart, 3, 2);

                tmpS.Append("\n");
                tmpS.Append("<script type=\"text/javascript\" Language=\"JavaScript\">\n");
                tmpS.Append("function CopyDown(SrcID,TargetID)\n");
                tmpS.Append("{\n");
                tmpS.Append("document.getElementById('AddressNickName' + TargetID).value = document.getElementById('AddressNickName' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressFirstName' + TargetID).value = document.getElementById('AddressFirstName' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressLastName' + TargetID).value = document.getElementById('AddressLastName' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressPhone' + TargetID).value = document.getElementById('AddressPhone' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressCompany' + TargetID).value = document.getElementById('AddressCompany' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressAddress1' + TargetID).value = document.getElementById('AddressAddress1' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressAddress2' + TargetID).value = document.getElementById('AddressAddress2' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressSuite' + TargetID).value = document.getElementById('AddressSuite' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressCity' + TargetID).value = document.getElementById('AddressCity' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressState' + TargetID).selectedIndex = document.getElementById('AddressState' + SrcID).selectedIndex;\n");
                tmpS.Append("document.getElementById('AddressZip' + TargetID).value = document.getElementById('AddressZip' + SrcID).value;\n");
                tmpS.Append("document.getElementById('AddressCountry' + TargetID).selectedIndex = document.getElementById('AddressCountry' + SrcID).selectedIndex;\n");
                tmpS.Append("return (true);\n");
                tmpS.Append("}\n");
                tmpS.Append("</script>\n");


                tmpS.Append("<table width='100%' cellpadding='2' cellspacing='0' border='0'>");

                String GenericAddressSelectionBlock = GetGenericAddressSelectionBlock(ViewingCustomer, false, 0);

                String LastID = String.Empty;
                foreach (CartItem c in m_CartItems)
                {
                    if (c.IsDownload || c.IsSystem || !c.Shippable || GiftCard.s_IsEmailGiftCard(c.ProductID))
                    {
                        String ThisID = c.ShoppingCartRecordID.ToString() + "_1";

                        tmpS.Append("<tr>");
                        if (ShowPicsInCart && !c.IsAuctionItem)
                        {
                            tmpS.Append("<td height='15' bgcolor='#" + AppLogic.AppConfig("LightCellColor") + "' align='left' valign='middle'><b>");
                            tmpS.Append(AppLogic.GetString("shoppingcart.cs.1", m_SkinID, m_ThisCustomer.LocaleSetting));
                            tmpS.Append("</b></td>");
                            tmpS.Append("<td height='15' bgcolor='#" + AppLogic.AppConfig("LightCellColor") + "' align='center' valign='middle'></td>");
                        }
                        else
                        {
                            tmpS.Append("<td class=\"MultiShippingAddressHeaderLeft\" height=\"15\" align=\"left\" valign=\"middle\"><b>");
                            tmpS.Append(AppLogic.GetString("shoppingcart.cs.1", m_SkinID, m_ThisCustomer.LocaleSetting));
                            tmpS.Append("</b></td>");
                        }
                        tmpS.Append("<td class=\"MultiShippingAddressHeaderRight\" height=\"15\" align=\"left\" valign=\"middle\"><b>");
                        tmpS.Append(AppLogic.GetString("shoppingcart.cs.87", m_SkinID, m_ThisCustomer.LocaleSetting));
                        tmpS.Append("</b></td>");
                        tmpS.Append("</tr>");
                        tmpS.Append("<tr><td colspan=\"" + ColSpan.ToString() + "\"><img src=\"images/spacer.gif\" width=\"100%\" height=\"2\"></td></tr>");

                        tmpS.Append("<tr>");
                        if (ShowPicsInCart && !c.IsAuctionItem)
                        {
                            // PICTURE COL:
                            tmpS.Append("<td align=\"center\" valign=\"top\">");
                            String ThePic = AppLogic.LookupImage("Variant", c.VariantID, "icon", m_SkinID, m_ThisCustomer.LocaleSetting);
                            if (ThePic.Length == 0 || ThePic.IndexOf("nopicture") != -1)
                            {
                                ThePic = AppLogic.LookupImage("Product", c.ProductID, "icon", m_SkinID, m_ThisCustomer.LocaleSetting);
                            }
                            if (ShowLinkBack && !c.IsSystem)
                            {
                                tmpS.Append("<a href=\"" + SE.MakeProductLink(c.ProductID, "") + "\">");
                            }
                            tmpS.Append("<img src=\"" + ThePic + "\" border=\"0\">");
                            if (ShowLinkBack && !c.IsSystem)
                            {
                                tmpS.Append("</a>");
                            }
                            tmpS.Append("</td>");
                        }

                        // PRODUCT DESCRIPTION COL
                        tmpS.Append("<td align=\"left\" valign=\"top\">");
                        tmpS.Append(GetLineItemDescription(c, ShowLinkBack, VarReadOnly, false));
                        tmpS.Append("</td>");

                        // SHIP TO COL
                        tmpS.Append("<td align=\"left\" valign=\"top\">");
                        tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.23", SkinID, ThisCustomer.LocaleSetting));
                        tmpS.Append("</td>");
                        tmpS.Append("</tr>");
                        tmpS.Append("<tr><td colspan=\"" + ColSpan.ToString() + "\"><img src=\"images/spacer.gif\" width=\"100%\" height=\"8\"></td></tr>");
                        LastID = ThisID;
                    }
                    else
                    {
                        String GenericAddressSelectionBlockWithGiftRegistry = String.Empty;
                        if (c.GiftRegistryForCustomerID != 0)
                        {
                            GenericAddressSelectionBlockWithGiftRegistry = GetGenericAddressSelectionBlock(ViewingCustomer, true, c.GiftRegistryForCustomerID);
                        }
                        for (int i = 1; i <= c.Quantity; i++)
                        {
                            String ThisID = c.ShoppingCartRecordID.ToString() + "_" + i.ToString();

                            tmpS.Append("<tr>");
                            if (ShowPicsInCart && !c.IsAuctionItem)
                            {
                                tmpS.Append("<td height=\"15\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\" align=\"left\" valign=\"middle\"><b>");
                                tmpS.Append(AppLogic.GetString("shoppingcart.cs.1", m_SkinID, m_ThisCustomer.LocaleSetting));
                                tmpS.Append("</b></td>");
                                tmpS.Append("<td height=\"15\" bgcolor=\"#" + AppLogic.AppConfig("LightCellColor") + "\" align=\"center\" valign=\"middle\"></td>");
                            }
                            else
                            {
                                tmpS.Append("<td class=\"MultiShippingAddressHeaderLeft\" height=\"15\" align=\"left\" valign=\"middle\"><b>");
                                tmpS.Append(AppLogic.GetString("shoppingcart.cs.1", m_SkinID, m_ThisCustomer.LocaleSetting));
                                tmpS.Append("</b></td>");
                            }
                            tmpS.Append("<td class=\"MultiShippingAddressHeaderRight\" height=\"15\" align=\"left\" valign=\"middle\"><b>");
                            tmpS.Append(AppLogic.GetString("shoppingcart.cs.87", m_SkinID, m_ThisCustomer.LocaleSetting));
                            tmpS.Append("</b></td>");
                            tmpS.Append("</tr>");
                            tmpS.Append("<tr><td colspan=\"" + ColSpan.ToString() + "\"><img src=\"images/spacer.gif\" width=\"100%\" height=\"2\"></td></tr>");

                            String AddrBlock = String.Empty;

                            if (c.GiftRegistryForCustomerID == 0)
                            {
                                // mark the currently selected address, and show that existing address select list
                                AddrBlock = GenericAddressSelectionBlock;
                                AddrBlock = AddrBlock.Replace("(!ID!)", ThisID);
                                AddrBlock = AddrBlock.Replace("value=\"ExistingAddress\"", "value=\"ExistingAddress\" checked");
                                AddrBlock = AddrBlock.Replace("<option value=\"" + c.ShippingAddressID.ToString() + "\">", "<option value=\"" + c.ShippingAddressID.ToString() + "\" selected>");
                                AddrBlock = AddrBlock.Replace("<div id=\"ExistingDiv_" + ThisID + "\" style=\"display:none;\">", "<div id=\"ExistingDiv_" + ThisID + "\" style=\"display:block;\">");
                            }
                            else
                            {
                                AddrBlock = GenericAddressSelectionBlockWithGiftRegistry;
                                AddrBlock = AddrBlock.Replace("(!ID!)", ThisID);
                                if (!Customer.OwnsThisAddress(ThisCustomer.CustomerID, c.ShippingAddressID))
                                {
                                    // mark the gift option, and show the gift address div
                                    AddrBlock = AddrBlock.Replace("value=\"GiftRegistryAddress\"", "value=\"GiftRegistryAddress\" checked");
                                    AddrBlock = AddrBlock.Replace("(!ID!)", ThisID);
                                    AddrBlock = AddrBlock.Replace("<div id=\"GiftDiv_" + ThisID + "\" style=\"display:none;\">", "<div id=\"GiftDiv_" + ThisID + "\" style=\"display:block;\">");
                                }
                                else
                                {
                                    // mark the currently selected address, and show that existing address select list
                                    AddrBlock = AddrBlock.Replace("value=\"ExistingAddress\"", "value=\"ExistingAddress\" checked");
                                    AddrBlock = AddrBlock.Replace("<option value=\"" + c.ShippingAddressID.ToString() + "\">", "<option value=\"" + c.ShippingAddressID.ToString() + "\" selected>");
                                    AddrBlock = AddrBlock.Replace("<div id=\"ExistingDiv_" + ThisID + "\" style=\"display:none;\">", "<div id=\"ExistingDiv_" + ThisID + "\" style=\"display:block;\">");
                                }
                            }

                            tmpS.Append("<tr>");
                            if (ShowPicsInCart && !c.IsAuctionItem)
                            {
                                // PICTURE COL:
                                tmpS.Append("<td align=\"center\" valign=\"top\">");
                                String ThePic = AppLogic.LookupImage("Variant", c.VariantID, "icon", m_SkinID, m_ThisCustomer.LocaleSetting);
                                if (ThePic.Length == 0 || ThePic.IndexOf("nopicture") != -1)
                                {
                                    ThePic = AppLogic.LookupImage("Product", c.ProductID, "icon", m_SkinID, m_ThisCustomer.LocaleSetting);
                                }
                                if (ShowLinkBack && !c.IsSystem)
                                {
                                    tmpS.Append("<a href=\"" + SE.MakeProductLink(c.ProductID, "") + "\">");
                                }
                                tmpS.Append("<img src=\"" + ThePic + "\" border=\"0\">");
                                if (ShowLinkBack && !c.IsSystem)
                                {
                                    tmpS.Append("</a>");
                                }
                                tmpS.Append("</td>");
                            }

                            // PRODUCT DESCRIPTION COL
                            tmpS.Append("<td align=\"left\" valign=\"top\">");
                            tmpS.Append(GetLineItemDescription(c, ShowLinkBack, VarReadOnly, false));
                            tmpS.Append("</td>");

                            // SHIP TO COL
                            tmpS.Append("<td align=\"left\" valign=\"top\">");
                            if (LastID.Length != 0)
                            {
                                String CopyPriorAddressJSFunction = " <img align=\"absmiddle\" class=\"actionelement\" onClick=\"CopyDown('_" + LastID + "','_" + ThisID + "')\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ViewingCustomer.SkinID.ToString() + "/images/copynewdown.gif", ViewingCustomer.LocaleSetting) + "\">";
                                AddrBlock = AddrBlock.Replace("<!-- COPY FROM ABOVE -->", CopyPriorAddressJSFunction);
                            }
                            tmpS.Append(AddrBlock);
                            tmpS.Append("</td>");
                            tmpS.Append("</tr>");
                            tmpS.Append("<tr><td colspan=\"" + ColSpan.ToString() + "\"><img src=\"images/spacer.gif\" width=\"100%\" height=\"8\"></td></tr>");
                            LastID = ThisID;
                        }
                    }
                }
                tmpS.Append("</table>");
            }
            return tmpS.ToString();
        }

        /// <summary>
        /// Gets the line item description.
        /// </summary>
        /// <param name="c">The CartItem.</param>
        /// <param name="ShowLinkBack">if set to <c>true</c> [show link back].</param>
        /// <param name="VarReadOnly">if set to <c>true</c> [var read only].</param>
        /// <param name="ShowMultiShipAddressUnderItemDescription">if set to <c>true</c> [show multi ship address under item description].</param>
        /// <returns></returns>
        private String GetLineItemDescription(CartItem c, bool ShowLinkBack, bool VarReadOnly, bool ShowMultiShipAddressUnderItemDescription)
        {
            StringBuilder tmpS = new StringBuilder(4096);

            if (c.IsAuctionItem)
            {
                VarReadOnly = true;
                ShowLinkBack = true;
            }

            if (AppLogic.MicropayIsEnabled() &&
                !AppLogic.AppConfigBool("MicroPay.HideOnCartPage") &&
                c.ProductID == AppLogic.MicropayProductID &&
                CommonLogic.GetThisPageName(false).Equals("SHOPPINGCART.ASPX", StringComparison.InvariantCultureIgnoreCase))
            {
                tmpS.Append("<div>");
                tmpS.Append(String.Format(AppLogic.GetString("shoppingcart.cs.24", m_SkinID, m_ThisCustomer.LocaleSetting), m_ThisCustomer.CurrencyString(AppLogic.GetMicroPayBalance(m_ThisCustomer.CustomerID))));
                tmpS.Append("</div>");
            }

            if (ShowLinkBack && !c.IsSystem && !c.IsAuctionItem)
            {
                tmpS.Append("<a class=\"cart-item-back-link\" href=\"" + SE.MakeProductLink(c.ProductID, "") + "\">");
            }
            tmpS.Append("<span class=\"cart-item-name\">");
            tmpS.Append(AppLogic.MakeProperObjectName(c.ProductName, c.VariantName, ThisCustomer.LocaleSetting));
            if (c.IsAuctionItem)
            {
                tmpS.Append(" (Auction)");
            }
            tmpS.Append("</span>");
            if (ShowLinkBack && !c.IsSystem && !c.IsAuctionItem)
            {
                tmpS.Append("</a>");
            }

            if (!c.IsSystem)
            {
                tmpS.Append("<div>");
                tmpS.Append(AppLogic.GetString("showproduct.aspx.21", SkinID, ThisCustomer.LocaleSetting) + " " + c.SKU);
                tmpS.Append("</div>");
            }

            if (c.GiftRegistryForCustomerID != 0)
            {
                tmpS.Append("<div>");
                tmpS.Append(String.Format(AppLogic.GetString("shoppingcart.cs.92", SkinID, ThisCustomer.LocaleSetting), AppLogic.GiftRegistryDisplayName(c.GiftRegistryForCustomerID, false, SkinID, ThisCustomer.LocaleSetting)));
                tmpS.Append("</div>");
            }

            if (c.ChosenSize.Length != 0)
            {
                tmpS.Append("<div>");
                tmpS.Append(c.SizeOptionPrompt);
                tmpS.Append(": ");
                string ChosenSize = c.ChosenSize;
                if ((m_VATOn || ThisCustomer.CurrencySetting != Localization.StoreCurrency()) || ChosenSize.IndexOf("[") > -1)
                {
                    decimal SzPrDelta = AppLogic.GetColorAndSizePriceDelta("", ChosenSize, c.TaxClassID, ThisCustomer, true, true);
                    string prMod = CommonLogic.IIF(SzPrDelta > 0, "+", "") + ThisCustomer.CurrencyString(SzPrDelta);
                    if (ChosenSize.IndexOf("[") > -1)
                    {
                        ChosenSize = ChosenSize.Substring(0, ChosenSize.IndexOf("[")) + " [" + prMod + "]";
                    }
                }
                tmpS.Append(ChosenSize);
                tmpS.Append("</div>");
            }

            if (c.ChosenColor.Length != 0)
            {
                tmpS.Append("<div>");
                tmpS.Append(c.ColorOptionPrompt);
                tmpS.Append(": ");
                string ChosenColor = c.ChosenColor;
                if ((m_VATOn || ThisCustomer.CurrencySetting != Localization.StoreCurrency()) || ChosenColor.IndexOf("[") > -1)
                {
                    decimal ClrPrDelta = AppLogic.GetColorAndSizePriceDelta("", ChosenColor, c.TaxClassID, ThisCustomer, true, true);
                    string prMod = CommonLogic.IIF(ClrPrDelta > 0, "+", "") + ThisCustomer.CurrencyString(ClrPrDelta);
                    if (ChosenColor.IndexOf("[") > -1)
                    {
                        ChosenColor = ChosenColor.Substring(0, ChosenColor.IndexOf("[")) + " [" + prMod + "]";
                    }
                }
                tmpS.Append(ChosenColor);
                tmpS.Append("</div>");
            }

            if (c.TextOption.Length != 0)
            {
                tmpS.Append("<div>");
                tmpS.Append(c.TextOptionPrompt);
                if (c.TextOption.IndexOf("\n") == -1)
                {
                    tmpS.Append(": ");
                    tmpS.Append(HttpContext.Current.Server.HtmlDecode(c.TextOption));
                }
                else
                {
                    tmpS.Append(":<br/>");
                    tmpS.Append(HttpContext.Current.Server.HtmlDecode(c.TextOption).Replace("\n", "<br/>"));
                }
                tmpS.Append("</div>");
            }

            string AutoShipString = String.Empty;
            if (c.IsRecurring)
            {
                AutoShipString = "<span>";
                if ((int)c.RecurringIntervalType >= 0)
                {
                    AutoShipString += String.Format(AppLogic.GetString("shoppingcart.cs.26a", m_SkinID, m_ThisCustomer.LocaleSetting), c.RecurringInterval.ToString(), c.RecurringIntervalType.ToString());
                }
                else
                {
                    AutoShipString += String.Format(AppLogic.GetString("shoppingcart.cs.26b", m_SkinID, m_ThisCustomer.LocaleSetting), c.RecurringIntervalType.ToString());
                }
                AutoShipString += "</span>";
            }

            bool IsAKit = c.IsAKit;
            if (IsAKit)
            {
                if (c.GiftRegistryForCustomerID == 0 && AppLogic.AppConfigBool("ShowEditButtonInCartForKitProducts"))
                {
                    bool isKit2 = false;

                    try
                    {
                        using (SqlConnection conKit2 = new SqlConnection(DB.GetDBConn()))
                        {
                            conKit2.Open();
                            using (IDataReader rs = DB.GetRS(string.Format("SELECT IsKit2 FROM ShoppingCart WITH (NOLOCK) WHERE ShoppingCartRecID = {0}", c.ShoppingCartRecordID), conKit2))
                            {
                                isKit2 = rs.Read() && DB.RSFieldBool(rs, "IsKit2");
                            }
                        }
                    }
                    catch { }

                    if (isKit2)
                    {
                        tmpS.Append(" <a href=\"showproduct.aspx?cartrecid=" + c.ShoppingCartRecordID.ToString() + "&edit=" + c.ShoppingCartRecordID.ToString() + "&productid=" + c.ProductID.ToString() + "&SEName=" + c.SEName + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/edit2.gif") + "\" align=\"absmiddle\" border=\"0\" alt=\"" + AppLogic.GetString("shoppingcart.cs.11", SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                    }
                    else
                    {
                        tmpS.Append(" <a href=\"ShoppingCart_change.aspx?cartrecid=" + c.ShoppingCartRecordID.ToString() + "&edit=" + c.ShoppingCartRecordID.ToString() + "&productid=" + c.ProductID.ToString() + "&SEName=" + c.SEName + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/edit2.gif") + "\" align=\"absmiddle\" border=\"0\" alt=\"" + AppLogic.GetString("shoppingcart.cs.11", SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                    }
                }

                tmpS.Append(AutoShipString);

                StringBuilder tmp = new StringBuilder(4096);

                SqlConnection con = null;
                IDataReader rsx = null;
                try
                {
                    string query = "select KitItem.Name, KitCart.Quantity, KitCart.TextOption from KitCart  with (NOLOCK)  inner join KitItem  with (NOLOCK)  on KitCart.KitItemID=KitItem.KitItemID where ShoppingCartRecID=" + c.ShoppingCartRecordID.ToString();
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rsx = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rsx = DB.GetRS(query, con);
                    }

                    using (rsx)
                    {
                        while (rsx.Read())
                        {

                            tmpS.Append("<div>");
                            tmp.Append(" - ");
                            tmp.Append(DB.RSFieldByLocale(rsx, "Name", m_ThisCustomer.LocaleSetting));
                            if (DB.RSField(rsx, "TextOption").Length > 0)
                            {
                                tmpS.Append(" ");
                                if (DB.RSField(rsx, "TextOption").StartsWith("images/orders", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    tmp.Append(": <a target=\"_blank\" href=\"" + DB.RSField(rsx, "TextOption") + "\">" + AppLogic.GetString("shoppingcart.cs.1000", m_SkinID, m_ThisCustomer.LocaleSetting) + "</a>");
                                }
                                else
                                {
                                    tmp.Append(": " + DB.RSField(rsx, "TextOption"));
                                }
                            }

                            tmpS.Append("</div>");

                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }
                    // make sure we won't reference this again
                    con = null;
                    rsx = null;
                }

                tmpS.Append("<div style=\"margin-left: 10px;\">");
                tmpS.Append(tmp.ToString());
                tmpS.Append("</div>");
            }
            else
            {
                if (!VarReadOnly && AppLogic.AppConfigBool("ShowEditButtonInCartForRegularProducts"))
                {
                    tmpS.Append(" <a href=\"ShoppingCart_change.aspx?cartrecid=" + c.ShoppingCartRecordID.ToString() + "&edit=" + c.ShoppingCartRecordID.ToString() + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/edit2.gif") + "\" align=\"absmiddle\" border=\"0\" alt=\"" + AppLogic.GetString("shoppingcart.cs.11", m_SkinID, m_ThisCustomer.LocaleSetting) + "\"></a>");
                }
                tmpS.Append(AutoShipString);
            }

            if (c.IsDownload && !c.IsSystem)
            {
                tmpS.Append("<div class='download-label'>");
                tmpS.Append(AppLogic.GetString("shoppingcart.cs.84", m_SkinID, m_ThisCustomer.LocaleSetting));
                tmpS.Append("</div>");
            }
            if (!c.IsDownload && !c.IsSystem && c.FreeShipping && !c.IsSystem && c.Shippable)
            {
                tmpS.Append("<div class='free-ship-label'>");
                tmpS.Append(AppLogic.GetString("shoppingcart.cs.104", m_SkinID, m_ThisCustomer.LocaleSetting));
                tmpS.Append("</div>");
            }

            //Check for requires shipping (FreeShipping=2)
            if ((this.HasMultipleShippingAddresses() || this.HasGiftRegistryComponents()) && ShowMultiShipAddressUnderItemDescription && !c.IsDownload && !c.IsSystem)
            {

                //Make sure that the rates properly display for the EMail Gift Card products
                //and the products that don't require shipping

                if (c.Shippable)
                {
                    if (GiftCard.s_IsEmailGiftCard(c.ProductID))
                    {
                        tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.28", m_SkinID, m_ThisCustomer.LocaleSetting));
                    }
                    else
                    {
                        tmpS.Append(AppLogic.GetString("shoppingcart.cs.87", m_SkinID, m_ThisCustomer.LocaleSetting));
                        tmpS.Append(" ");
                        if (c.GiftRegistryForCustomerID != 0 && !Customer.OwnsThisAddress(ThisCustomer.CustomerID, c.ShippingAddressID))
                        {
                            tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.15", m_SkinID, m_ThisCustomer.LocaleSetting));
                        }
                        else
                        {
                            if (c.ShippingAddressID == m_ThisCustomer.PrimaryShippingAddressID)
                            {
                                tmpS.Append(AppLogic.GetString("account.aspx.32", m_SkinID, m_ThisCustomer.LocaleSetting));
                            }
                            else if (c.ShippingAddressID == m_ThisCustomer.PrimaryBillingAddressID)
                            {
                                tmpS.Append(AppLogic.GetString("account.aspx.30", m_SkinID, m_ThisCustomer.LocaleSetting));
                            }
                            Address adr = new Address();
                            adr.LoadFromDB(c.ShippingAddressID);
                            tmpS.Append("<div style=\"margin-left: 10px;\">");
                            tmpS.Append(adr.DisplayHTML(false));
                            tmpS.Append("</div>");
                        }
                        tmpS.Append("<div>");
                        tmpS.Append(AppLogic.GetString("order.cs.68", m_SkinID, m_ThisCustomer.LocaleSetting));

                        tmpS.Append(c.ShippingMethod);

                        tmpS.Append("</div>");
                    }
                }
                else
                {
                    tmpS.Append(AppLogic.GetString("common.cs.88", m_SkinID, m_ThisCustomer.LocaleSetting));
                }
            }

            if (AppLogic.AppConfigBool("AllowShoppingCartItemNotes") && !c.IsSystem && CartType == CartTypeEnum.ShoppingCart && HttpContext.Current.Request.Url.AbsoluteUri.IndexOf("checkoutshippingmult.aspx") == -1)
            {
                if (!VarReadOnly)
                {
                    tmpS.Append("<b>" + AppLogic.GetString("shoppingcart.cs.86", m_SkinID, m_ThisCustomer.LocaleSetting) + "</b><textarea class=\"addressselect\" rows=\"" + AppLogic.AppConfigUSInt("ShoppingCartItemNotesTextareaRows") + "\" cols=\"" + AppLogic.AppConfigUSInt("ShoppingCartItemNotesTextareaCols") + "\" name=\"Notes_" + c.ShoppingCartRecordID.ToString() + "\" id=\"Notes_" + c.ShoppingCartRecordID.ToString() + "\">" + HttpContext.Current.Server.HtmlEncode(c.Notes) + "</textarea>");
                }
                else
                {
                    if (c.Notes.Length != 0)
                    {
                        tmpS.Append("<b>" + AppLogic.GetString("shoppingcart.cs.86", m_SkinID, m_ThisCustomer.LocaleSetting) + "</b>" + HttpContext.Current.Server.HtmlEncode(c.Notes));
                    }
                }
            }
            return tmpS.ToString();
        }

        /// <summary>
        /// Displays the summary.
        /// </summary>
        /// <param name="ShowSubtotal">if set to <c>true</c> [show subtotal].</param>
        /// <param name="ShowShipping">if set to <c>true</c> [show shipping].</param>
        /// <param name="ShowTax">if set to <c>true</c> [show tax].</param>
        /// <param name="ShowTotal">if set to <c>true</c> [show total].</param>
        /// <returns></returns>
        public String DisplaySummary(bool ShowSubtotal, bool ShowShipping, bool ShowTax, bool ShowTotal)
        {
            return DisplaySummary(ShowSubtotal, ShowShipping, ShowTax, ShowTotal, true);
        }

        /// <summary>
        /// Displays the summary.
        /// </summary>
        /// <param name="ShowSubtotal">if set to <c>true</c> [show subtotal].</param>
        /// <param name="ShowShipping">if set to <c>true</c> [show shipping].</param>
        /// <param name="ShowTax">if set to <c>true</c> [show tax].</param>
        /// <param name="ShowTotal">if set to <c>true</c> [show total].</param>
        /// <param name="ShowItemDeleteButton">if set to <c>true</c> [show item delete button].</param>
        /// <returns></returns>
        public String DisplaySummary(bool ShowSubtotal, bool ShowShipping, bool ShowTax, bool ShowTotal, bool ShowItemDeleteButton)
        {
            //Determine Region setting for tax calculation if VAT is enabled
            //decimal TaxRate = 0.0M;
            int CountryID = 0;
            int StateID = 0;
            string ZipCode = string.Empty;
            bool VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            bool VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);
            if (VATEnabled)
            {
                CountryID = AppLogic.AppConfigUSInt("VAT.CountryID");
                StateID = 0;
                ZipCode = string.Empty;

                if (ThisCustomer != null && ThisCustomer.IsRegistered)
                {
                    if (ThisCustomer.PrimaryShippingAddress.CountryID > 0)
                    {
                        CountryID = ThisCustomer.PrimaryShippingAddress.CountryID;
                    }
                    if (ThisCustomer.PrimaryShippingAddress.StateID > 0)
                    {
                        StateID = ThisCustomer.PrimaryShippingAddress.StateID;
                    }
                    if (ThisCustomer.PrimaryShippingAddress.Zip.Trim().Length != 0)
                    {
                        ZipCode = ThisCustomer.PrimaryShippingAddress.Zip.Trim();
                    }
                }
            }

            if (AppLogic.AppConfigBool("SkipShippingOnCheckout"))
            {
                ShowShipping = false;
            }
            CartItem FirstCartItem = (CartItem)CartItems[0];
            Address FirstItemShippingAddress = new Address();
            FirstItemShippingAddress.LoadByCustomer(ThisCustomer.CustomerID, FirstCartItem.ShippingAddressID, AddressTypes.Shipping);
            StringBuilder tmpS = new StringBuilder(4096);

            tmpS.Append("<div align=\"left\">");

            // ----------------------------------------------------------------------------------------
            // WRITE OUT GIFT CARD AREA (IF NOT DISABLED):
            // ----------------------------------------------------------------------------------------
            if (m_GiftCardsEnabled && ThisCustomer.CouponCode.Length != 0)
            {
                tmpS.Append("");
                tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
                tmpS.Append("<div class=\"group-header checkout-header\">" + AppLogic.GetString("Global.GiftCards", ThisCustomer.LocaleSetting) + "</div>");
                tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
                tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

                tmpS.Append(AppLogic.GetString("order.cs.28", SkinID, ThisCustomer.LocaleSetting));

                tmpS.Append(HttpContext.Current.Server.HtmlEncode(ThisCustomer.CouponCode));

                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
                tmpS.Append("</td></tr>\n");
                tmpS.Append("</table>\n");
            }

            if (OrderNotes.Length != 0)
            {
                // ----------------------------------------------------------------------------------------
                // WRITE OUT NOTES AREA(IF NOT DISABLED):
                // ----------------------------------------------------------------------------------------
                if (!AppLogic.AppConfigBool("DisallowOrderNotes"))
                {
                    tmpS.Append("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + AppLogic.AppConfig("HeaderBGColor") + "\">\n");
                    tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");
                    tmpS.Append("<div class=\"group-header checkout-header\">" + AppLogic.GetString("Header.OrderNotes", ThisCustomer.LocaleSetting) + "</div>");
                    tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"0\" style=\"" + AppLogic.AppConfig("BoxFrameStyle") + "\">\n");
                    tmpS.Append("<tr><td align=\"left\" valign=\"top\">\n");

                    tmpS.Append(OrderNotes);

                    tmpS.Append("</td></tr>\n");
                    tmpS.Append("</table>\n");
                    tmpS.Append("</td></tr>\n");
                    tmpS.Append("</table>\n");

                }
            }

            tmpS.Append("</div>");
            return tmpS.ToString();
        }

        // calls RT service, checks rates, updates customer table, and m_RTShippingMethod with new rate cost based on current cart contents!
        // returns radio list of available rates
        /// <summary>
        /// Gets the Real Time shipping rates.
        /// </summary>
        /// <param name="State">The state.</param>
        /// <param name="Country">The country.</param>
        /// <param name="FieldSuffix">The field suffix.</param>
        /// <param name="FreeIDs">The FreeIds.</param>
        /// <returns></returns>
        public String GetRTShippingRates(String State, String Country, String FieldSuffix, String FreeIDs)
        {
            String result = String.Empty;
            String CacheName = String.Format("GetRTShippingRates_{0}_{1}_{2}_{3}_{4}_{5}", ThisCustomer.CustomerID.ToString(), State, Country, FieldSuffix, FirstItem().ShippingAddressID.ToString(), FreeIDs);
            if (m_CachedTotals.ContainsKey(CacheName))
            {
                result = m_CachedTotals[CacheName].ToString();
                return result;
            }

            ShippingMethods s_methods = GetRates(AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee"));

            if (AppLogic.AppConfigBool("RTShipping.AllowLocalPickup"))
            {
                bool pickupIsAllowed = false;

                String restrictiontype = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionType").Trim();

                if (restrictiontype.Equals("zone", StringComparison.InvariantCultureIgnoreCase))
                {
                    String[] allowedzones = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZones").Trim().Split(',');

                    int zoneforzip = Shipping.ZoneLookup(ThisCustomer.PrimaryShippingAddress.Zip);

                    foreach (String allowedzone in allowedzones)
                    {
                        if (int.Parse(allowedzone) == zoneforzip)
                        {
                            pickupIsAllowed = true;
                        }
                    }
                }
                else if (restrictiontype.Equals("zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    String[] allowedzips = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZips").Trim().Split(',');

                    foreach (String allowedzip in allowedzips)
                    {
                        if (ThisCustomer.PrimaryShippingAddress.Zip == allowedzip)
                        {
                            pickupIsAllowed = true;
                        }
                    }

                }
                else if (restrictiontype.Equals("state", StringComparison.InvariantCultureIgnoreCase))
                {
                    String allowedstateids = AppLogic.AppConfig("RTShipping.LocalpickupRestrictionStates").Trim();

                    try
                    {
                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (IDataReader rs = DB.GetRS("select * from State with (NOLOCK) where StateID in (" + allowedstateids + ")", dbconn))
                            {
                                while (rs.Read())
                                {
                                    if (ThisCustomer.PrimaryShippingAddress.StateID == DB.RSFieldInt(rs, "StateID"))
                                    {
                                        pickupIsAllowed = true;
                                    }
                                }
                            }
                        }

                    }
                    catch
                    {
                        pickupIsAllowed = false;
                    }

                }
                else
                {
                    pickupIsAllowed = true;
                }

                if (pickupIsAllowed)
                {
                    ShipMethod s_methodPickup = new ShipMethod();
                    s_methodPickup.ServiceName = AppLogic.GetString("RTShipping.LocalPickupMethodName", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    s_methodPickup.ServiceRate = AppLogic.AppConfigNativeDecimal("RTShipping.LocalPickupCost");
                    s_methods.AddMethod(s_methodPickup);
                }
            }


            // Display CallForShipping if it was the error returned AND there are no other valid shipping methods.
            if (s_methods.Count == 0 && s_methods.ErrorMsg.Length > 0 && s_methods.ErrorMsg.IndexOf(AppLogic.AppConfig("RTShipping.CallForShippingPrompt")) != -1)
            {
                String ThisSvc = s_methods.ErrorMsg;

                if (DB.GetSqlN("select count(*) as N from ShippingMethod  with (NOLOCK)  where IsRTShipping=1 and convert(nvarchar(4000),Name)=" + DB.SQuote(ThisSvc)) == 0)
                {
                    DB.ExecuteSQL(String.Format("insert ShippingMethod(Name,IsRTShipping) values({0},1)", DB.SQuote(ThisSvc)), m_DBTrans);
                }

                int ShippingMethodID = Shipping.GetShippingMethodID(ThisSvc);
                String RadioValue = String.Format("{0}|{1}|{2}|{3}", ShippingMethodID.ToString(), ThisSvc, decimal.Zero.ToString(), decimal.Zero.ToString());
                result = "<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + 0 + "\" value=\"" + RadioValue + "\">";
                result = result + (" ");
                result = result + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                m_CachedTotals.Add(CacheName, result);
                return result;

            }

            String RateErrors = String.Empty;

            StringBuilder RateSelect = new StringBuilder(4096);
            // Note: try to select the customer's prior rate selection (may not be valid anymore though):

            string weightMessage = AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
            string weightPrompt = String.Empty;

            int Count = 0;
            if (s_methods.Count > 0)
            {
                foreach (ShipMethod s_method in s_methods)
                {
                    Count++;

                    if (DB.GetSqlN("select count(*) as N from ShippingMethod  with (NOLOCK)  where IsRTShipping=1 and convert(nvarchar(4000),Name)=" + DB.SQuote(s_method.ServiceName)) == 0)
                    {
                        DB.ExecuteSQL(String.Format("insert ShippingMethod(Name,IsRTShipping) values({0},1)", DB.SQuote(s_method.ServiceName)), m_DBTrans);
                    }

                    int ShippingMethodID = Shipping.GetShippingMethodID(s_method.ServiceName);
                    s_method.ShippingMethodID = ShippingMethodID;

                    //need to adjust the rate by subtracting the free shipping items rate from the 
                    //ShippingMethods collection so that we can subtract it from the free shipping 
                    //method if there are non-free shipping items in the cart
                    if (CommonLogic.IntegerIsInIntegerList(ShippingMethodID, FreeIDs) && s_method.FreeItemsRate > 0.0M)
                    {
                        s_method.ServiceRate = s_method.ServiceRate - s_method.FreeItemsRate;

                        //sanity check
                        if (s_method.ServiceRate < 0)
                            s_method.ServiceRate = 0.0M;
                    }

                    bool tempShippingIsFree = ShippingIsFree;
                    if (AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") && FieldSuffix.Length != 0)
                    {
                        // must re-analyze based on pared down cart for current shipping addressID
                        int ThisAddressID = Localization.ParseNativeInt(FieldSuffix.Trim('_'));
                        AnalyzeCartForFreeShippingConditions(ThisAddressID);

                    }

                    if (CommonLogic.IntegerIsInIntegerList(ShippingMethodID, FreeIDs) && tempShippingIsFree)
                    {
                        s_method.ServiceRate = 0.0M;
                        s_method.VatRate = 0.0M;
                    }
                }

                if (AppLogic.AppConfigBool("RTShipping.SortByRate"))
                {
                    //Sort shipping costs low to high
                    RTShipping.ShippingCostsSorter rtsComparer = new RTShipping.ShippingCostsSorter();
                    s_methods.Sort(rtsComparer);
                }

                NumberFormatInfo usNumberFormat = new CultureInfo("en-US").NumberFormat;

                foreach (ShipMethod s_method in s_methods)
                {
                    if (s_method.ServiceName.Length > 0)
                    {
                        // we must store some additional data for later retreival, format: shippingmethodid|description|cost
                        // NOTE :
                        //  For localization issues, we need to format the price for 
                        String RadioValue = String.Format("{0}|{1}|{2}|{3}", s_method.ShippingMethodID.ToString(), s_method.ServiceName, s_method.ServiceRate.ToString("0.00", usNumberFormat), s_method.VatRate);
                        RateSelect.Append("<input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + Count.ToString() + "\" value=\"" + RadioValue + "\">");
                        RateSelect.Append(" ");

                        decimal shiprate = CommonLogic.IIF(m_VATOn, Localization.ParseDBDecimal(s_method.ServiceRate.ToString()) + Localization.ParseDBDecimal(s_method.VatRate.ToString()), Localization.ParseDBDecimal(s_method.ServiceRate.ToString()));

                        RateSelect.Append(s_method.ServiceName + " " + Localization.CurrencyStringForDisplayWithExchangeRate(shiprate, ThisCustomer.CurrencySetting));

                    }
                    else
                    {
                        if (s_methods.ErrorMsg.IndexOf(weightMessage) != -1)
                        {
                            weightPrompt = s_methods.ErrorMsg;
                        }
                        RateErrors += s_methods.ErrorMsg;
                    }
                }
            }
            else
            {
                if (s_methods.ErrorMsg.IndexOf(weightMessage) != -1)
                {
                    weightPrompt = s_methods.ErrorMsg;
                }
                RateErrors += s_methods.ErrorMsg;
            }

            if (Count == 0)
            {
                //No rates so clear the m_ShippingMethod to force recalc
                result = RateErrors;
            }
            else
            {
                if (AppLogic.AppConfigBool("RTShipping.ShowErrors"))
                {
                    RateSelect.Append(RateErrors);
                }
                else
                {
                    RateSelect.Append(weightPrompt);
                }
                result = RateSelect.ToString();
            }
            m_CachedTotals.Add(CacheName, result);
            return result;
        }

        // overload added for support of Local Store Pickup for RTShipping
        /// <summary>
        /// Gets the Real Time shipping rates.
        /// </summary>
        /// <param name="thisShippingAddress">The this shipping address.</param>
        /// <param name="FieldSuffix">The field suffix.</param>
        /// <param name="FreeIDs">The FreeIds.</param>
        /// <returns></returns>
        public String GetRTShippingRates(Address thisShippingAddress, String FieldSuffix, String FreeIDs)
        {
            String State = thisShippingAddress.State;
            String Country = thisShippingAddress.Country;

            String result = String.Empty;
            String CacheName = String.Format("GetRTShippingRates_{0}_{1}_{2}_{3}_{4}_{5}", ThisCustomer.CustomerID.ToString(), State, Country, FieldSuffix, FirstItem().ShippingAddressID.ToString(), FreeIDs);
            if (m_CachedTotals.ContainsKey(CacheName))
            {
                result = m_CachedTotals[CacheName].ToString();
                return result;
            }

            ShippingMethods s_methods = GetRates(AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee"));

            if (AppLogic.AppConfigBool("RTShipping.AllowLocalPickup"))
            {
                bool pickupIsAllowed = false;

                String restrictiontype = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionType").Trim();

                if (restrictiontype.Equals("zone", StringComparison.InvariantCultureIgnoreCase))
                {
                    String[] allowedzones = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZones").Trim().Split(',');

                    int zoneforzip = Shipping.ZoneLookup(thisShippingAddress.Zip);

                    foreach (String allowedzone in allowedzones)
                    {
                        if (int.Parse(allowedzone) == zoneforzip)
                        {
                            pickupIsAllowed = true;
                        }
                    }
                }
                else if (restrictiontype.Equals("zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    String[] allowedzips = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZips").Trim().Split(',');

                    foreach (String allowedzip in allowedzips)
                    {
                        if (thisShippingAddress.Zip == allowedzip)
                        {
                            pickupIsAllowed = true;
                        }
                    }

                }
                else if (restrictiontype.Equals("state", StringComparison.InvariantCultureIgnoreCase))
                {
                    String allowedstateids = AppLogic.AppConfig("RTShipping.LocalpickupRestrictionStates").Trim();

                    try
                    {
                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (IDataReader rs = DB.GetRS("select * from State with (NOLOCK) where StateID in (" + allowedstateids + ")", dbconn))
                            {
                                while (rs.Read())
                                {
                                    if (thisShippingAddress.StateID == DB.RSFieldInt(rs, "StateID"))
                                    {
                                        pickupIsAllowed = true;
                                    }
                                }
                            }
                        }

                    }
                    catch
                    {
                        pickupIsAllowed = false;
                    }

                }
                else
                {
                    pickupIsAllowed = true;
                }

                if (pickupIsAllowed)
                {
                    ShipMethod s_methodPickup = new ShipMethod();
                    s_methodPickup.ServiceName = AppLogic.GetString("RTShipping.LocalPickupMethodName", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    s_methodPickup.ServiceRate = AppLogic.AppConfigNativeDecimal("RTShipping.LocalPickupCost");
                    s_methods.AddMethod(s_methodPickup);
                }
            }


            // Display CallForShipping if it was the error returned AND there are no other valid shipping methods.
            if (s_methods.Count == 0 && s_methods.ErrorMsg.Length > 0 && s_methods.ErrorMsg.IndexOf(AppLogic.AppConfig("RTShipping.CallForShippingPrompt")) != -1)
            {
                String ThisSvc = s_methods.ErrorMsg;

                if (DB.GetSqlN("select count(*) as N from ShippingMethod  with (NOLOCK)  where IsRTShipping=1 and convert(nvarchar(4000),Name)=" + DB.SQuote(ThisSvc)) == 0)
                {
                    DB.ExecuteSQL(String.Format("insert ShippingMethod(Name,IsRTShipping) values({0},1)", DB.SQuote(ThisSvc)), m_DBTrans);
                }

                int ShippingMethodID = Shipping.GetShippingMethodID(ThisSvc);
                String RadioValue = String.Format("{0}|{1}|{2}|{3}", ShippingMethodID.ToString(), ThisSvc, decimal.Zero.ToString(), decimal.Zero.ToString());
                result = "<div class=\"form-group\"><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + 0 + "\" value=\"" + RadioValue + "\">";
                result += AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                result += "</div>";
                m_CachedTotals.Add(CacheName, result);
                return result;

            }

            String RateErrors = String.Empty;

            StringBuilder RateSelect = new StringBuilder(4096);
            // Note: try to select the customer's prior rate selection (may not be valid anymore though):

            string weightMessage = AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
            string weightPrompt = String.Empty;

            int Count = 0;
            if (s_methods.Count > 0)
            {
                foreach (ShipMethod s_method in s_methods)
                {
                    Count++;

                    if (DB.GetSqlN("select count(*) as N from ShippingMethod  with (NOLOCK)  where IsRTShipping=1 and convert(nvarchar(4000),Name)=" + DB.SQuote(s_method.ServiceName)) == 0)
                    {
                        DB.ExecuteSQL(String.Format("insert ShippingMethod(Name,IsRTShipping) values({0},1)", DB.SQuote(s_method.ServiceName)), m_DBTrans);
                    }

                    int ShippingMethodID = Shipping.GetShippingMethodID(s_method.ServiceName);
                    s_method.ShippingMethodID = ShippingMethodID;

                    //need to adjust the rate by subtracting the free shipping items rate from the 
                    //ShippingMethods collection so that we can subtract it from the free shipping 
                    //method if there are non-free shipping items in the cart
                    if (CommonLogic.IntegerIsInIntegerList(ShippingMethodID, FreeIDs) && s_method.FreeItemsRate > 0.0M)
                    {
                        s_method.ServiceRate = s_method.ServiceRate - s_method.FreeItemsRate;

                        //sanity check
                        if (s_method.ServiceRate < 0)
                            s_method.ServiceRate = 0.0M;
                    }

                    bool tempShippingIsFree = ShippingIsFree;
                    if (AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") && FieldSuffix.Length != 0)
                    {
                        // must re-analyze based on pared down cart for current shipping addressID
                        int ThisAddressID = Localization.ParseNativeInt(FieldSuffix.Trim('_'));
                        AnalyzeCartForFreeShippingConditions(ThisAddressID);

                    }

                    if (CommonLogic.IntegerIsInIntegerList(ShippingMethodID, FreeIDs) && tempShippingIsFree)
                    {
                        s_method.ServiceRate = 0.0M;
                        s_method.VatRate = 0.0M;
                    }
                }

                if (AppLogic.AppConfigBool("RTShipping.SortByRate"))
                {
                    //Sort shipping costs low to high
                    RTShipping.ShippingCostsSorter rtsComparer = new RTShipping.ShippingCostsSorter();
                    s_methods.Sort(rtsComparer);
                }

                NumberFormatInfo usNumberFormat = new CultureInfo("en-US").NumberFormat;

                foreach (ShipMethod s_method in s_methods)
                {
                    if (s_method.ServiceName.Length > 0)
                    {
                        // we must store some additional data for later retreival, format: shippingmethodid|description|cost
                        // NOTE :
                        //  For localization issues, we need to format the price for 
                        String RadioValue = String.Format("{0}|{1}|{2}|{3}", s_method.ShippingMethodID.ToString(), s_method.ServiceName, s_method.ServiceRate.ToString("0.00", usNumberFormat), s_method.VatRate);
                        RateSelect.Append("<div class=\"form-group\"><input type=\"radio\" name=\"ShippingMethodID" + FieldSuffix + "\" id=\"ShippingMethodID" + FieldSuffix + Count.ToString() + "\" value=\"" + RadioValue + "\">");

                        decimal shiprate = CommonLogic.IIF(m_VATOn, s_method.ServiceRate + s_method.VatRate, s_method.ServiceRate);

                        RateSelect.Append(s_method.GetNameForDisplay() + " " + Localization.CurrencyStringForDisplayWithExchangeRate(shiprate, ThisCustomer.CurrencySetting) + "</div>");
                    }
                    else
                    {
                        if (s_methods.ErrorMsg.IndexOf(weightMessage) != -1)
                        {
                            weightPrompt = s_methods.ErrorMsg;
                        }
                        RateErrors += s_methods.ErrorMsg;
                    }
                }
            }
            else
            {
                if (s_methods.ErrorMsg.IndexOf(weightMessage) != -1)
                {
                    weightPrompt = s_methods.ErrorMsg;
                }
                RateErrors += s_methods.ErrorMsg;
            }

            if (Count == 0)
            {
                //No rates so clear the m_ShippingMethod to force recalc
                result = RateErrors;
            }
            else
            {
                if (AppLogic.AppConfigBool("RTShipping.ShowErrors"))
                {
                    RateSelect.Append(RateErrors);
                }
                else
                {
                    RateSelect.Append(weightPrompt);
                }
                result = RateSelect.ToString();
            }
            m_CachedTotals.Add(CacheName, result);
            return result;
        }

        /// <summary>
        /// Get shipping method rates.
        /// </summary>
        /// <param name="ShippingHandlingExtraFee">The shipping handling extra fee.</param>
        /// <returns></returns>
        internal ShippingMethods GetRates(Decimal ShippingHandlingExtraFee)
        {
            // Create Address object
            Address shippingAddress = new Address();
            shippingAddress.LoadFromDB(FirstItem().ShippingAddressID);
            return GetRates(shippingAddress, ShippingHandlingExtraFee);
        }

        /// <summary>
        /// Get shipping method rates.
        /// </summary>
        /// <param name="shipmentAddress">The shipment address.</param>
        /// <param name="ShippingHandlingExtraFee">The shipping handling extra fee.</param>
        /// <returns></returns>
        internal ShippingMethods GetRates(Address shipmentAddress, Decimal ShippingHandlingExtraFee)
        {
            RTShipping realTimeShipping = new RTShipping();

            Decimal MarkupPercent = AppLogic.AppConfigUSDecimal("RTShipping.MarkupPercent");

            Decimal remainingItemsWeight = 0.0M;
            Decimal remainingItemsInsuranceValue = 0.0M;

            // Set shipment info
            realTimeShipping.ShipmentWeight = this.GetHeaviestPackageWeightTotal();

            //Create Shipments Collection
            Shipments shipments = new Shipments();

            // Create Packages Collection
            Packages shipment = new Packages();

            int PackageID = 1;

            #region Multi Distributor Shipping Calculation (Beta)
            if (HasDistributorComponents() && AppLogic.AppConfigBool("RTShipping.MultiDistributorCalculation"))
            {
                shipments.HasDistributorItems = true;

                // Get ship separately cart items with distributors first
                foreach (CartItem c in m_CartItems)
                {
                    if (c.DistributorID > 0)
                    {
                        // Handle ship separately items with distributors first:
                        //Only calculate rates for products that require shipping
                        if (c.IsShipSeparately && (!c.IsDownload && !c.IsSystem && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID) && !c.FreeShipping))
                        {
                            shipment = new Packages();

                            // Set destination address of this shipment group:
                            shipment = ShipmentDestination(shipmentAddress, shipment);
                            realTimeShipping.DestinationResidenceType = shipment.DestinationResidenceType;

                            //set origin address of this shipment group
                            shipment = ShipmentOrigin(shipment, c.DistributorID);

                            Package p = new Package(c);
                            p.PackageId = PackageID++;
                            shipment.AddPackage(p);

                            shipments.AddPackages(shipment);

                            p = null;
                            shipment = null;
                        }
                    }
                }

                // Get ship separately cart items without distributors next
                foreach (CartItem c in m_CartItems)
                {
                    if (c.DistributorID == 0)
                    {
                        //Only calculate rates for products that require shipping 
                        if (c.IsShipSeparately && (!c.IsDownload && !c.IsSystem && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID) && !c.FreeShipping))
                        {
                            for (int n = 1; n <= c.Quantity; n++)
                            {
                                shipment = new Packages();

                                // Set destination address of this shipment group:
                                shipment = ShipmentDestination(shipmentAddress, shipment);
                                realTimeShipping.DestinationResidenceType = shipment.DestinationResidenceType;

                                Package p = new Package(c);

                                p.PackageId = PackageID++;

                                shipment.AddPackage(p);
                                shipments.AddPackages(shipment);

                                p = null;
                                shipment = null;
                            }
                        }
                    }
                }
                // Now get all itmes that do not ship separately, but group them into shipments by distributor

                int DistID = 0;
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader drs = DB.GetRS("SELECT max(DistributorID) as DistID from ProductDistributor", con))
                    {
                        if (drs.Read())
                        {
                            DistID = DB.RSFieldInt(drs, "DistID");
                        }
                    }
                }

                for (int i = 1; i <= DistID; i++)
                {
                    remainingItemsWeight = 0.0M;
                    remainingItemsInsuranceValue = 0.0M;

                    shipment = new Packages();

                    // Set destination address of this shipment group:
                    shipment = ShipmentDestination(shipmentAddress, shipment);
                    realTimeShipping.DestinationResidenceType = shipment.DestinationResidenceType;

                    //set origin address of this shipment group
                    shipment = ShipmentOrigin(shipment, i);

                    Decimal Weight = 0.0M;
                    foreach (CartItem c in m_CartItems)
                    {
                        //Only calculate rates for products that require shipping
                        if (!c.IsDownload && !c.IsShipSeparately && !c.IsSystem && c.DistributorID == i && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID))
                        {
                            Weight = c.Weight;

                            if (Weight == 0.0M)
                            {
                                Weight = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight");
                            }
                            if (Weight == 0.0M)
                            {
                                Weight = 0.5M; // must have SOMETHING to use!
                            }
                            remainingItemsWeight += (Weight * c.Quantity);
                            remainingItemsInsuranceValue += (c.Price * c.Quantity);
                        }
                    }
                    if (remainingItemsWeight != 0.0M)
                    {
                        // Create package object for this item
                        Package p = new Package();

                        p.PackageId = PackageID++;

                        p.Weight = remainingItemsWeight;
                        p.Weight += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");

                        // Set insurance. Get from products db shipping values?
                        p.Insured = AppLogic.AppConfigBool("RTShipping.Insured");
                        p.InsuredValue = remainingItemsInsuranceValue;

                        shipment.AddPackage(p);
                        shipments.AddPackages(shipment);
                        p = null;
                        shipment = null;
                    }
                }

                // Handle NON ship separately items here, but summing weight (dimensions not supported for these)
                // add one package for all of these items, summing the weight making sure that none
                // of them have distributors

                remainingItemsWeight = 0.0M;
                remainingItemsInsuranceValue = 0.0M;

                shipment = new Packages();

                // Set destination address of this shipment group:
                shipment = ShipmentDestination(shipmentAddress, shipment);
                realTimeShipping.DestinationResidenceType = shipment.DestinationResidenceType;

                foreach (CartItem c in m_CartItems)
                {
                    //Only calculate rates for products that require shipping
                    if (!c.IsDownload && !c.IsShipSeparately && !c.IsSystem && c.DistributorID == 0 && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID))
                    {
                        Decimal Weight = c.Weight;
                        if (Weight == 0.0M)
                        {
                            Weight = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight");
                        }
                        if (Weight == 0.0M)
                        {
                            Weight = 0.5M; // must have SOMETHING to use!
                        }
                        remainingItemsWeight += (Weight * c.Quantity);
                        remainingItemsInsuranceValue += (c.Price * c.Quantity);
                    }
                }
                if (remainingItemsWeight != 0.0M)
                {
                    // Create package object for this item
                    Package p = new Package();

                    p.PackageId = PackageID++;

                    p.Weight = remainingItemsWeight;
                    p.Weight += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");

                    // Set insurance. Get from products db shipping values?
                    p.Insured = AppLogic.AppConfigBool("RTShipping.Insured");
                    p.InsuredValue = remainingItemsInsuranceValue;

                    // Add package to collection
                    shipment.AddPackage(p);
                    shipments.AddPackages(shipment);

                    p = null;
                    shipment = null;
                }
            }
            #endregion
            #region Regular Shipping Calculation
            else
            {

                shipments = new Shipments();

                PackageID = 1;

                // Get ship separately cart Items
                foreach (CartItem c in m_CartItems)
                {
                    //Only calculate rates for products that require shipping (c.m_Shippable = true)
                    if (c.IsShipSeparately && (!c.IsDownload && !c.IsSystem && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID) && !c.FreeShipping))
                    {
                        shipment = new Packages();

                        // Set destination address of this shipment group:
                        shipment = ShipmentDestination(shipmentAddress, shipment);
                        realTimeShipping.DestinationResidenceType = shipment.DestinationResidenceType;

                        // Create package object for this item
                        Package p = new Package(c);

                        p.PackageId = PackageID++;

                        shipment.AddPackage(p);
                        shipments.AddPackages(shipment);

                        shipment = null;
                        p = null;
                    }
                }

                // Get all other non-free items now
                remainingItemsWeight = 0.0M;
                remainingItemsInsuranceValue = 0.0M;

                shipment = new Packages();

                // Set destination address of this shipment group:
                shipment = ShipmentDestination(shipmentAddress, shipment);
                realTimeShipping.DestinationResidenceType = shipment.DestinationResidenceType;

                //JH 10.20.2010 
                // this logic needs refactoring and fleshing out.
                // free shipping is currently included in calculations if FreeShippingAllowsRateSelection is true
                // until we fix shipping such that two realtime requests are made (one with free items, one without), we have to assume free shipping items are in all calculations.
                // I've added a check to see if the final package includes all free shipping items
                foreach (CartItem c in m_CartItems)
                {
                    // Handle NON ship separately non free items here, but summing weight (dimensions not supported for these)
                    // add one package for all of these items, summing the weight
                    //Only calculate rates for products that require shipping

                    if (!c.IsDownload && !c.IsShipSeparately && !c.IsSystem && (!c.FreeShipping || AppLogic.AppConfigBool("FreeShippingAllowsRateSelection")) && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID))
                    {
                        Decimal Weight = c.Weight;
                        if (Weight == 0.0M)
                        {
                            Weight = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight");
                        }
                        if (Weight == 0.0M)
                        {
                            Weight = 0.5M; // must have SOMETHING to use!
                        }
                        remainingItemsWeight += (Weight * c.Quantity);
                        remainingItemsInsuranceValue += (c.Price * c.Quantity);
                    }
                }
                if (remainingItemsWeight != 0.0M)
                {
                    // Create package object for this item
                    Package p = new Package();

                    p.PackageId = PackageID;
                    PackageID = PackageID + 1;

                    p.Weight = remainingItemsWeight;
                    p.Weight += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");

                    // Set insurance. Get from products db shipping values?
                    p.Insured = AppLogic.AppConfigBool("RTShipping.Insured");
                    p.InsuredValue = remainingItemsInsuranceValue;

                    // Add package to collection
                    shipment.AddPackage(p);
                    shipments.AddPackages(shipment);

                    p = null;
                    shipment = null;
                }
            }
            #endregion

            realTimeShipping.ShipmentValue = this.SubTotal(true, false, false, false);

            // Get carriers
            string carriers = String.Empty;
            foreach (Packages rtps in shipments)
            {
                if (rtps.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase))
                {
                    carriers = AppLogic.AppConfig("RTshipping.DomesticCarriers");
                }
                else
                {
                    carriers = AppLogic.AppConfig("RTshipping.InternationalCarriers");
                }
            }
            if (carriers.Length == 0)
            {
                carriers = AppLogic.AppConfig("RTShipping.ActiveCarrier");
            }


            // Get result type
            RTShipping.ResultType format = RTShipping.ResultType.CollectionList;

            StringBuilder tmpS = new StringBuilder(4096);

            String RTShipRequest = String.Empty;
            String RTShipResponse = String.Empty;

            decimal ShippingTaxRate = CommonLogic.IIF(m_VATOn, ThisCustomer.TaxRate(AppLogic.AppConfigUSInt("ShippingTaxClassID")) / 100.0M, 0);

            ShippingMethods s_methods = (ShippingMethods)realTimeShipping.GetRates(shipments, carriers, format, "ShippingMethod", "ShippingMethod", ShippingTaxRate, out RTShipRequest, out RTShipResponse, ShippingHandlingExtraFee, (decimal)MarkupPercent, realTimeShipping.ShipmentValue, CartItems);


            DB.ExecuteSQL("update customer set RTShipRequest=" + DB.SQuote(RTShipRequest) + ", RTShipResponse=" + DB.SQuote(RTShipResponse) + " where customerid=" + m_ThisCustomer.CustomerID.ToString(), m_DBTrans);
            realTimeShipping = null;

            return s_methods;
        }

        /// <summary>
        /// Gets the shipment destination.
        /// </summary>
        /// <param name="shippingAddress">The shipping address.</param>
        /// <param name="thisShipment">The current shipment.</param>
        /// <returns></returns>
        public Packages ShipmentDestination(Address shippingAddress, Packages thisShipment)
        {
            thisShipment = new Packages();
            thisShipment.DestinationCity = shippingAddress.City;
            if (shippingAddress.State.Length == 2)
                thisShipment.DestinationStateProvince = shippingAddress.State;
            else
                thisShipment.DestinationStateProvince = "--";
            thisShipment.DestinationZipPostalCode = shippingAddress.Zip;
            thisShipment.DestinationCountry = shippingAddress.Country;
            if (shippingAddress.Country.Length == 2) //off site support
                thisShipment.DestinationCountryCode = shippingAddress.Country;
            else
                thisShipment.DestinationCountryCode = AppLogic.GetCountryTwoLetterISOCode(shippingAddress.Country);
            thisShipment.DestinationResidenceType = shippingAddress.ResidenceType;

            // Set pickup type
            thisShipment.PickupType = AppLogic.AppConfig("RTShipping.UPS.UPSPickupType");
            if (AppLogic.AppConfig("RTShipping.UPS.UPSPickupType").Length == 0)
            {
                thisShipment.PickupType = RTShipping.PickupTypes.UPSCustomerCounter.ToString();
            }

            return thisShipment;
        }

        /// <summary>
        /// Gets the shipment origin.
        /// </summary>
        /// <param name="thisShipment">The current shipment.</param>
        /// <param name="i">The DistributorID.</param>
        /// <returns></returns>
        public Packages ShipmentOrigin(Packages thisShipment, int i)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                using (IDataReader rs = DB.GetRS("Select * from Distributor  with (NOLOCK)  where DistributorID=" + i + " and Deleted=0 and Published=1", con))
                {
                    while (rs.Read())
                    {
                        thisShipment.OriginAddress1 = DB.RSField(rs, "Address1");
                        thisShipment.OriginAddress2 = DB.RSField(rs, "Address2");
                        thisShipment.OriginCity = DB.RSField(rs, "City");
                        thisShipment.OriginStateProvince = DB.RSField(rs, "State");
                        thisShipment.OriginCountryCode = AppLogic.GetCountryTwoLetterISOCode(DB.RSField(rs, "Country"));
                        thisShipment.OriginZipPostalCode = DB.RSField(rs, "ZipCode");
                    }
                }
            }

            return thisShipment;
        }

        public int AddItemToCart(int productId, int variantId, int quantity)
        {
            return AddItem(ThisCustomer
                            , ThisCustomer.PrimaryShippingAddressID
                            , productId
                            , variantId
                            , quantity
                            , ""
                            , ""
                            , ""
                            , ""
                            , ""
                            , CartType
                            , false
                            , false
                            , 0
                            , 0
                            , null
                            , true
                            );

        }

        /// <summary>
        /// Adds an item in the shopping cart.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="ShippingAddressID">The shipping address ID.</param>
        /// <param name="ProductID">The ProductID.</param>
        /// <param name="VariantID">The VariantID.</param>
        /// <param name="Quantity">The quantity.</param>
        /// <param name="ChosenColor">The chosen color.</param>
        /// <param name="ChosenColorSKUModifier">The chosen color SKU modifier.</param>
        /// <param name="ChosenSize">The chosen size.</param>
        /// <param name="ChosenSizeSKUModifier">The chosen size SKU modifier.</param>
        /// <param name="TextOption">The text option.</param>
        /// <param name="CartType">Type of the cart.</param>
        /// <param name="UpdateCartObject">if set to <c>true</c> [update cart object].</param>
        /// <param name="IsRequired">if set to <c>true</c> [is required].</param>
        /// <param name="GiftRegistryForCustomerID">The gift registry for CustomerID.</param>
        /// <param name="CustomerEnteredPrice">The customer entered price.</param>
        /// <returns></returns>
        public int AddItem(Customer ThisCustomer, int ShippingAddressID, int ProductID, int VariantID, int Quantity, String ChosenColor, String ChosenColorSKUModifier, String ChosenSize, String ChosenSizeSKUModifier, String TextOption, CartTypeEnum CartType, bool UpdateCartObject, bool IsRequired, int GiftRegistryForCustomerID, decimal CustomerEnteredPrice, Decimal BluBuksUsed = 0, Decimal CategoryFundUsed = 0, int FundID = 0, Decimal BluBucksPercentageUsed = 0, int ProductCategoryID = 0, String GLcode = "0")
        {
            return AddItem(ThisCustomer, ShippingAddressID, ProductID, VariantID, Quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, TextOption, CartType, UpdateCartObject, IsRequired, GiftRegistryForCustomerID, CustomerEnteredPrice, null, BluBuksUsed, CategoryFundUsed, FundID, BluBucksPercentageUsed, ProductCategoryID, GLcode);
        }

        public int AddItem(Customer ThisCustomer, int ShippingAddressID, int ProductID, int VariantID, int Quantity, String ChosenColor, String ChosenColorSKUModifier, String ChosenSize, String ChosenSizeSKUModifier, String TextOption, CartTypeEnum CartType, bool UpdateCartObject, bool IsRequired, int GiftRegistryForCustomerID, decimal CustomerEnteredPrice, KitComposition preferredComposition, Decimal BluBuksUsed = 0, Decimal CategoryFundUsed = 0, int FundID = 0, Decimal BluBucksPercentageUsed = 0, int ProductCategoryID = 0, String GLcode = "0")
        {
            return AddItem(ThisCustomer, ShippingAddressID, ProductID, VariantID, Quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, TextOption, CartType, UpdateCartObject, IsRequired, GiftRegistryForCustomerID, CustomerEnteredPrice, preferredComposition, false, BluBuksUsed, CategoryFundUsed, FundID, BluBucksPercentageUsed, ProductCategoryID, GLcode);
        }

        // NOTE: ChosenColor and ChosenSize MUST ALWAYS be passed in here in the MASTER WEBCONFIG LOCALE, If in a ML situation!
        /// <summary>
        /// Adds an item in the shopping cart.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="ShippingAddressID">The shipping address ID.</param>
        /// <param name="ProductID">The ProductID.</param>
        /// <param name="VariantID">The VariantID.</param>
        /// <param name="Quantity">The quantity.</param>
        /// <param name="ChosenColor">The chosen color.</param>
        /// <param name="ChosenColorSKUModifier">The chosen color SKU modifier.</param>
        /// <param name="ChosenSize">Size of the chosen.</param>
        /// <param name="ChosenSizeSKUModifier">The chosen size SKU modifier.</param>
        /// <param name="TextOption">The text option.</param>
        /// <param name="CartType">Type of the cart.</param>
        /// <param name="UpdateCartObject">if set to <c>true</c> [update cart object].</param>
        /// <param name="IsRequired">if set to <c>true</c> [is required].</param>
        /// <param name="GiftRegistryForCustomerID">The gift registry for CustomerID.</param>
        /// <param name="CustomerEnteredPrice">The customer entered price.</param>
        /// <param name="preferredComposition">The preferred composition.</param>
        /// <returns></returns>
        public int AddItem(Customer ThisCustomer, int ShippingAddressID, int ProductID, int VariantID, int Quantity, String ChosenColor, String ChosenColorSKUModifier, String ChosenSize, String ChosenSizeSKUModifier, String TextOption, CartTypeEnum CartType, bool UpdateCartObject, bool IsRequired, int GiftRegistryForCustomerID, decimal CustomerEnteredPrice, KitComposition preferredComposition, bool isAGift, Decimal BluBuksUsed = 0, Decimal CategoryFundUsed = 0, int FundID = 0, Decimal BluBucksPercentageUsed = 0, int ProductCategoryID = 0, String GLcode = "0")
        {
            if (null != preferredComposition)
            {
                foreach (CartItem c in m_CartItems)
                {
                    if (ShippingAddressID == c.ShippingAddressID && c.ProductID == ProductID && c.VariantID == VariantID && c.ChosenSize == ChosenSize && c.TextOption == TextOption && c.GiftRegistryForCustomerID == GiftRegistryForCustomerID && c.ChosenColor == ChosenColor && (CustomerEnteredPrice == System.Decimal.Zero || CustomerEnteredPrice == c.Price))
                    {
                        if (null != preferredComposition && AppLogic.IsAKit(c.ProductID))
                        {
                            KitComposition cartItemKitComposition = KitComposition.FromCart(ThisCustomer, CartType, c.ShoppingCartRecordID);

                            if (preferredComposition.Matches(cartItemKitComposition))
                            {
                                c.IncrementQuantity(Quantity);

                                // remove the old shoppingcart item 
                                // which has the same structure as this one
                                RemoveItem(preferredComposition.CartID, true);
                                return c.ShoppingCartRecordID;
                            }
                        }
                    }

                }
            }

            int NewRecID = -1;
            String sql = "Select v.Colors, v.Sizes, p.TaxClassID from product p  with (NOLOCK) , productvariant v  with (NOLOCK)  where p.productid=v.productid and v.variantid=" + VariantID.ToString() + " and p.productid=" + ProductID.ToString();

            SqlConnection con = null;
            IDataReader rs = null;

            try
            {
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(sql, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(sql, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        //validate color selection
                        ChosenColor = GetValidatedAttribute(ChosenColor, DB.RSFieldByLocale(rs, "Colors", ThisCustomer.LocaleSetting), 0);

                        //validate size selection
                        ChosenSize = GetValidatedAttribute(ChosenSize, DB.RSFieldByLocale(rs, "Sizes", ThisCustomer.LocaleSetting), 0);

                        // check for color & size price modifiers:
                        Decimal PrMod = AppLogic.GetColorAndSizePriceDelta(ChosenColor, ChosenSize, DB.RSFieldInt(rs, "TaxClassID"), ThisCustomer, false, false);

                        int isKit2 = CommonLogic.IIF(null != preferredComposition, 1, 0);

                        SqlParameter[] spa = {DB.CreateSQLParameter("@CustomerID", SqlDbType.Int, 4, ThisCustomer.CustomerID, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, ProductID, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@VariantID", SqlDbType.Int, 4, VariantID, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@Quantity", SqlDbType.Int, 4, Quantity, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@ShippingAddressID", SqlDbType.Int, 4, ShippingAddressID, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@BillingAddressID", SqlDbType.Int, 4, ThisCustomer.PrimaryBillingAddressID, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@ChosenColor", SqlDbType.NVarChar, 200, ChosenColor, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@ChosenColorSKUModifier", SqlDbType.NVarChar, 200, ChosenColorSKUModifier, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@ChosenSize", SqlDbType.NVarChar, 200, ChosenSize, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@ChosenSizeSKUModifier", SqlDbType.NVarChar, 200, ChosenSizeSKUModifier, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@CleanColorOption", SqlDbType.NVarChar, 200, AppLogic.CleanSizeColorOption(ChosenColor), ParameterDirection.Input),
                                          DB.CreateSQLParameter("@CleanSizeOption", SqlDbType.NVarChar, 200, AppLogic.CleanSizeColorOption(ChosenSize), ParameterDirection.Input),
                                          DB.CreateSQLParameter("@ColorAndSizePriceDelta", SqlDbType.Money, 8, PrMod, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@TextOption", SqlDbType.NText, 2000000000, TextOption, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@CartType", SqlDbType.Int, 4, ((int)CartType), ParameterDirection.Input),
                                          DB.CreateSQLParameter("@GiftRegistryForCustomerID", SqlDbType.Int, 4, GiftRegistryForCustomerID, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@CustomerEnteredPrice", SqlDbType.Money, 4, CustomerEnteredPrice, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@CustomerLevelID", SqlDbType.Int, 4, ThisCustomer.CustomerLevelID, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@RequiresCount", SqlDbType.Int, 4, 0, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@IsKit2", SqlDbType.TinyInt, 4, isKit2, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@NewShoppingCartRecID", SqlDbType.Int, 4, null, ParameterDirection.Output),
                                          DB.CreateSQLParameter("@StoreID", SqlDbType.Int, 4, AppLogic.StoreID(), ParameterDirection.Input),
                                          DB.CreateSQLParameter("@IsAGift", SqlDbType.Bit, 1, isAGift, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@BluBuksUsed", SqlDbType.Money, 4, BluBuksUsed, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@CategoryFundUsed", SqlDbType.Money, 4, CategoryFundUsed, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@FundID", SqlDbType.Int, 4, FundID, ParameterDirection.Input),
                                           DB.CreateSQLParameter("@BluBucksPercentageUsed", SqlDbType.Money, 4, BluBucksPercentageUsed, ParameterDirection.Input),
                                          DB.CreateSQLParameter("@ProductCategoryID", SqlDbType.Int, 4, ProductCategoryID, ParameterDirection.Input),
                                           DB.CreateSQLParameter("@GLcode", SqlDbType.NVarChar, 200, GLcode, ParameterDirection.Input),
                                         };

                        NewRecID = DB.ExecuteStoredProcInt("dbo.aspdnsf_AddItemToCart", spa, m_DBTrans);

                        if (AppLogic.IsAKit(ProductID))
                        {
                            ProcessKitComposition(preferredComposition, ProductID, VariantID, NewRecID);

                        }

                        if (UpdateCartObject)
                        {
                            m_CartItems.Clear();
                            LoadFromDB(CartType);
                        }
                    }
                }

            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }
                // make sure we won't reference this again
                con = null;
                rs = null;
            }
            if (!isAGift)
                RecalculateCartDiscount();

            return NewRecID;
        }

        private String GetValidatedAttribute(String ChosenAttribute, String CSVValidAttributes, int Depth)
        {
            if (string.IsNullOrEmpty(ChosenAttribute))
                return string.Empty;

            if (ChosenAttribute.IndexOf("[") != -1)
                ChosenAttribute = ChosenAttribute.Substring(0, ChosenAttribute.IndexOf("["));
            ChosenAttribute = ChosenAttribute.Trim().ToLowerInvariant();

            foreach (string variantAttribute in CSVValidAttributes.Split(','))
            {
                String testAttribute;
                if (variantAttribute.IndexOf("[") != -1)
                    testAttribute = variantAttribute.Substring(0, variantAttribute.IndexOf("["));
                else
                    testAttribute = variantAttribute;
                testAttribute = testAttribute.Trim().ToLowerInvariant();

                if (ChosenAttribute == testAttribute)
                    return variantAttribute;
            }

            if (ChosenAttribute != HttpContext.Current.Server.HtmlDecode(ChosenAttribute) && Depth < 3)
                return GetValidatedAttribute(HttpContext.Current.Server.HtmlDecode(ChosenAttribute), CSVValidAttributes, Depth + 1);

            return "";
        }

        public void ProcessKitComposition(KitComposition preferredComposition, int ProductID, int VariantID, int cartRecId)
        {
            if (null != preferredComposition)
            {
                preferredComposition.CartID = cartRecId;
                AppLogic.ProcessKitComposition(ThisCustomer, preferredComposition);
            }

            decimal KitPR = AppLogic.GetKitTotalPrice(m_ThisCustomer.CustomerID, m_ThisCustomer.CustomerLevelID, ProductID, VariantID, cartRecId);
            DB.ExecuteSQL("update ShoppingCart set ProductPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(KitPR) + " where ShoppingCartRecID=" + cartRecId.ToString());

            decimal KitWD = AppLogic.GetKitTotalWeight(m_ThisCustomer.CustomerID, m_ThisCustomer.CustomerLevelID, ProductID, VariantID, cartRecId);
            DB.ExecuteSQL("update ShoppingCart set ProductWeight=" + Localization.CurrencyStringForDBWithoutExchangeRate(KitWD) + " where ShoppingCartRecID=" + cartRecId.ToString());
        }

        /// <summary>
        /// Sets the item quantity.
        /// </summary>
        /// <param name="ProductID">The ProductID.</param>
        /// <param name="VariantID">The VariantID.</param>
        /// <param name="Quantity">The quantity.</param>
        /// <param name="CartType">Type of the cart.</param>
        public void SetItemQuantity(int ProductID, int VariantID, int Quantity, CartTypeEnum CartType)
        {
            m_CachedTotals.Clear();
            int recID;

            SqlConnection con = null;
            IDataReader rs = null;

            try
            {
                string query = "Select ShoppingCartRecID from ShoppingCart   with (NOLOCK)  where CartType=" + ((int)CartType).ToString() + " and VariantID=" + VariantID.ToString() + " and productID=" + ProductID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString();
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        recID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                        SetItemQuantity(recID, Quantity);
                    }
                }

            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }
                // make sure we won't reference this again
                con = null;
                rs = null;
            }

            RecalculateCartDiscount();
        }

        /// <summary>
        /// Sets the item quantity.
        /// </summary>
        /// <param name="cartRecordID">The cart record ID.</param>
        /// <param name="Quantity">The quantity.</param>
        public void SetItemQuantity(int cartRecordID, int Quantity)
        {
            m_CachedTotals.Clear();
            if (Quantity <= 0)
            {
                RemoveItem(cartRecordID, true);
            }
            else
            {
                String sql = "update ShoppingCart set Quantity=" + Quantity.ToString() + " where ShoppingCartRecID=" + cartRecordID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString();
                DB.ExecuteSQL(sql, m_DBTrans);
                for (int i = 0; i < m_CartItems.Count; i++)
                {
                    if (((CartItem)m_CartItems[i]).ShoppingCartRecordID == cartRecordID)
                    {
                        CartItem ci = (CartItem)m_CartItems[i];
                        ci.Quantity = Quantity;
                        m_CartItems[i] = ci;
                        break;
                    }
                }
            }

            RecalculateCartDiscount();

            return;
        }

        //Added By Tayyab on 10-01-2016 to update fund used for item
        public void SetItemFundsUsed(int cartRecordID, Decimal CategoryFundUsed, Decimal BluBucksUsed, String GLcode)
        {
            m_CachedTotals.Clear();
            for (int i = 0; i < m_CartItems.Count; i++)
            {
                if (((CartItem)m_CartItems[i]).ShoppingCartRecordID == cartRecordID)
                {
                    CartItem ci = (CartItem)m_CartItems[i];
                    //CategoryFundUsed =  ci.CategoryFundUsed + CategoryFundUsed;
                    //  BluBucksUsed =  ci.BluBuksUsed + BluBucksUsed;
                    ci.CategoryFundUsed = CategoryFundUsed;
                    ci.BluBuksUsed = BluBucksUsed;
                    ci.GLcode = GLcode;
                    m_CartItems[i] = ci;
                    break;
                }
            }


            String sql = "update ShoppingCart set CategoryFundUsed=" + CategoryFundUsed.ToString() + ",GLcode='" + GLcode.ToString() + "',BluBucksUsed=" + BluBucksUsed.ToString() + "  where ShoppingCartRecID=" + cartRecordID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString();
            DB.ExecuteSQL(sql, m_DBTrans);


            RecalculateCartDiscount();

            return;
        }

        public void SetItemCartType(int cartRecordId, CartTypeEnum type)
        {
            var cItem = m_CartItems.Find(item => item.ShoppingCartRecordID == cartRecordId);
            if (cItem != null)
            {
                string sql = "UPDATE ShoppingCart SET CartType = {0} WHERE ShoppingCartRecID = {1}".FormatWith((int)type, cartRecordId);
                DB.ExecuteSQL(sql);
            }
        }

        /// <summary>
        /// Sets the item address.
        /// </summary>
        /// <param name="cartRecordID">The cart record ID.</param>
        /// <param name="AddressID">The address ID.</param>
        public void SetItemAddress(int cartRecordID, int AddressID)
        {
            m_CachedTotals.Clear();
            String sql = "update ShoppingCart set ShippingAddressID=" + AddressID.ToString() + " where ShoppingCartRecID=" + cartRecordID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString();
            DB.ExecuteSQL(sql, m_DBTrans);
            return;
        }

        /// <summary>
        /// Sets the item ShippingMethodID.
        /// </summary>
        /// <param name="cartRecordID">The cart record ID.</param>
        /// <param name="ShippingMethodID">The shipping method ID.</param>
        public void SetItemShippingMethodID(int cartRecordID, int ShippingMethodID)
        {
            m_CachedTotals.Clear();
            String sql = "update ShoppingCart set ShippingMethodID=" + ShippingMethodID.ToString() + " where ShoppingCartRecID=" + cartRecordID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString();
            DB.ExecuteSQL(sql, m_DBTrans);
            return;
        }

        /// <summary>
        /// Sets the item notes.
        /// </summary>
        /// <param name="cartRecordID">The cart record ID.</param>
        /// <param name="Notes">The notes.</param>
        public void SetItemNotes(int cartRecordID, String Notes)
        {
            String sql = "update ShoppingCart set Notes=" + DB.SQuote(Notes) + " where ShoppingCartRecID=" + cartRecordID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString();
            DB.ExecuteSQL(sql, m_DBTrans);
            return;
        }

        /// <summary>
        /// Removes the shopping cart item.
        /// </summary>
        /// <param name="ProductID">The ProductID.</param>
        /// <param name="VariantID">The VariantID.</param>
        /// <param name="CartType">Type of the cart.</param>
        /// <param name="Recurse">if set to <c>true</c> [recurse].</param>
        public void RemoveItem(int ProductID, int VariantID, CartTypeEnum CartType, Boolean Recurse)
        {
            m_CachedTotals.Clear();
            int recID;

            SqlConnection con = null;
            IDataReader rs = null;

            try
            {
                string query = "Select ShoppingCartRecID from ShoppingCart   with (NOLOCK)  where CartType=" + ((int)CartType) + " and VariantID=" + VariantID.ToString() + " and productID=" + ProductID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString();
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        recID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                        RemoveItem(recID, Recurse);
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }
                // make sure we won't reference this again
                con = null;
                rs = null;
            }
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="cartRecordID">The cart record ID.</param>
        /// <param name="Recurse">if set to <c>true</c> [recurse].</param>
        public void RemoveItem(int cartRecordID, bool Recurse)
        {

            m_CachedTotals.Clear();
            if (cartRecordID != 0)
            {
                int Quantity = 0;
                int RequiresCount = 0;
                int ProductID = 0;
                int VariantID = 0;
                Decimal BluBucksUsed = 0;
                Decimal CategoryFundUsed = 0;
                int FundID = 0;
                string RequiresProducts = String.Empty;
                string UpsellProducts = String.Empty;
                CartTypeEnum CartType = CartTypeEnum.ShoppingCart;

                SqlConnection con = null;
                IDataReader rs = null;

                try
                {
                    string query = "Select sc.ProductID,sc.BluBucksUsed,sc.CategoryFundUsed,sc.FundID, sc.VariantID, sc.Quantity, sc.RequiresCount, sc.CartType, p.upsellproducts, p.RequiresProducts from dbo.ShoppingCart sc  with (NOLOCK)  join dbo.Product p  with (NOLOCK)  on sc.ProductID = p.ProductID where sc.ShoppingCartRecID=" + cartRecordID.ToString();
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        if (rs.Read())
                        {
                            ProductID = DB.RSFieldInt(rs, "ProductID");
                            VariantID = DB.RSFieldInt(rs, "VariantID");
                            Quantity = DB.RSFieldInt(rs, "Quantity");
                            RequiresCount = DB.RSFieldInt(rs, "RequiresCount");
                            CartType = (CartTypeEnum)DB.RSFieldInt(rs, "CartType");
                            RequiresProducts = DB.RSField(rs, "RequiresProducts");
                            UpsellProducts = DB.RSField(rs, "UpsellProducts");
                            BluBucksUsed = DB.RSFieldDecimal(rs, "BluBucksUsed");
                            CategoryFundUsed = DB.RSFieldDecimal(rs, "CategoryFundUsed");
                            FundID = DB.RSFieldInt(rs, "FundID");
                        }
                    }
                }
                catch (Exception ex)
                {
                    SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }
                    // make sure we won't reference this again
                    con = null;
                }

                //Find out if the product we're trying to remove is required by any other products in the cart
                try
                {
                    SqlParameter[] spa = { new SqlParameter("@CustomerID", ThisCustomer.CustomerID) };
                    string requiredByQuery = "SELECT p.RequiresProducts FROM Product p LEFT JOIN ShoppingCart sc ON p.ProductID = sc.ProductId WHERE sc.CustomerID = @CustomerID";

                    SqlConnection con2 = new SqlConnection(DB.GetDBConn());
                    con2.Open();

                    rs = DB.GetRS(requiredByQuery, spa, con2);

                    using (rs)
                    {
                        while (rs.Read())
                        {
                            string[] requiredProductsArray = DB.RSField(rs, "RequiresProducts").Split(',');
                            if (requiredProductsArray.Any(s => s == ProductID.ToString()))
                                return; //The current product is required by others in the cart, so we can't remove it.
                        }
                    }
                }
                catch (Exception ex)
                {
                    SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }

                DB.ExecuteSQL("delete from kitcart where ShoppingCartRecID=" + cartRecordID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString(), m_DBTrans);
                String sql = "delete from ShoppingCart where ShoppingCartRecID=" + cartRecordID.ToString() + " and CustomerID=" + m_ThisCustomer.CustomerID.ToString();

                DB.ExecuteSQL(sql, m_DBTrans);

                //Update Customer Funds when item is deleted from shopping cart
                //StringBuilder sql1 = new StringBuilder(4096);
                //sql1.Append(String.Format("dbo.aspdnsf_CustomerFundUpdateOnItemDelete {0}, {1}, {2}", m_ThisCustomer.CustomerID, 1,BluBucksUsed));
                //DB.ExecuteSQL(sql1.ToString(), m_DBTrans);
                //sql1 = new StringBuilder(4096);
                //sql1.Append(String.Format("dbo.aspdnsf_CustomerFundUpdateOnItemDelete {0}, {1}, {2}", m_ThisCustomer.CustomerID, FundID, CategoryFundUsed));
                //DB.ExecuteSQL(sql1.ToString(), m_DBTrans);
                //End

                if (Recurse && RequiresProducts.Trim().Length > 0)
                {

                    string[] sAry = RequiresProducts.Split(',');
                    try
                    {
                        for (int y = 0; y < sAry.Length; y++)
                        {
                            ProductID = Int32.Parse(sAry[y]);
                            VariantID = AppLogic.GetProductsDefaultVariantID(ProductID);
                            RemoveItem(ProductID, VariantID, CartType, false);
                        }
                    }
                    catch { }
                }

                for (int i = 0; i < m_CartItems.Count; i++)
                {
                    if (((CartItem)m_CartItems[i]).ShoppingCartRecordID == cartRecordID)
                    {
                        m_CartItems.RemoveAt(i);
                        break;
                    }
                }

                if (UpsellProducts.Trim().Length > 0)
                {
                    try
                    {
                        string query = "Select ShoppingCartRecID, sc.ProductID, sc.VariantID from dbo.ShoppingCart sc  with (NOLOCK)  where sc.CustomerID=" + ThisCustomer.CustomerID.ToString() + " and sc.ProductID in (" + UpsellProducts.Trim() + ")";
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            rs = DB.GetRS(query, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            con = new SqlConnection(DB.GetDBConn());
                            con.Open();
                            rs = DB.GetRS(query, con);
                        }

                        using (rs)
                        {
                            if (rs.Read())
                            {
                                bool isonsale = false;
                                decimal productprice = AppLogic.DetermineLevelPrice(DB.RSFieldInt(rs, "VariantID"), ThisCustomer.CustomerLevelID, out isonsale);
                                DB.ExecuteSQL("update dbo.ShoppingCart set ProductPrice = " + Localization.DecimalStringForDB(productprice) + " where ShoppingCartRecID = " + DB.RSFieldInt(rs, "ShoppingCartRecID").ToString(), m_DBTrans);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                    }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            con.Dispose();
                        }
                        // make sure we won't reference this again
                        con = null;
                        rs = null;
                    }

                    this.LoadFromDB(this.CartType);
                }

                AppLogic.eventHandler("RemoveFromCart").CallEvent("&RemoveFromCart=true&VariantID=" + VariantID.ToString() + "&ProductID" + ProductID.ToString());
            }

            RecalculateCartDiscount();

            return;
        }

        /// <summary>
        /// Determines whether this ShoppingCart is empty.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEmpty()
        {
            return (m_CartItems.Count == 0);
        }

        /// <summary>
        /// Clears the contents of the shopping cart.
        /// </summary>
        public void ClearContents()
        {
            StaticClearContents(m_ThisCustomer.CustomerID, m_CartType, m_DBTrans);
            m_CachedTotals.Clear();
            m_CartItems.Clear();

            RecalculateCartDiscount();
        }

        /// <summary>
        /// Clears the contents of the shopping cart.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="CartType">Type of the cart.</param>
        /// <param name="DBTrans">The DB trans.</param>
        public static void StaticClearContents(int CustomerID, CartTypeEnum CartType, SqlTransaction DBTrans)
        {
            DB.ExecuteSQL("delete from KitCart where CartType=" + ((int)CartType).ToString() + " and (ShoppingCartRecID=0 or ShoppingCartRecID in (select ShoppingCartRecID from ShoppingCart where CustomerID=" + CustomerID.ToString() + " and CartType=" + ((int)CartType).ToString() + "))", DBTrans);
            DB.ExecuteSQL("delete from ShoppingCart where CustomerID=" + CustomerID.ToString() + " and CartType=" + ((int)CartType).ToString(), DBTrans);
        }

        /// <summary>
        /// Determines whether this shopping cart has coupon.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance has coupon; otherwise, <c>false</c>.
        /// </returns>
        public bool HasCoupon()
        {
            return (m_Coupon.m_couponcode.Length != 0 || m_ThisCustomer.CouponCode.Length != 0);
        }

        /// <summary>
        /// Gets the options list.
        /// </summary>
        /// <returns></returns>
        public String GetOptionsList()
        {
            var tmpS = new StringBuilder();

            for (int ctr = 0; ctr < this.OrderOptions.Count(); ctr++)
            {
                if (ctr > 0)
                {
                    tmpS.Append("^");
                }
                var opt = this.OrderOptions[ctr];

                tmpS.AppendFormat("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    opt.ID,
                    opt.UniqueID,
                    opt.Name,
                    m_ThisCustomer.CurrencyString(opt.Cost),
                    m_ThisCustomer.CurrencyString(opt.TaxRate),
                    opt.ImageUrl,
                    opt.TaxClassID);
            }

            return tmpS.ToString();
        }

        /// <summary>
        /// Returns true if this cart has any items which are download items.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has download components]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasDownloadComponents()
        {
            return m_CartItems.HasDownloadComponents;
        }

        /// <summary>
        /// Returns true if this cart has any items which are kit items
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has kit components]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasKitComponents()
        {
            return m_CartItems.HasKitComponents;
        }

        /// <summary>
        /// Returns true if this cart has any items which are taxable items.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has taxable components]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasTaxableComponents()
        {
            return m_CartItems.HasTaxableComponents;
        }

        /// <summary>
        /// Returns true if this cart has any items which are free shipping items
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has free shipping components]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasFreeShippingComponents()
        {
            return m_CartItems.HasFreeShippingComponents;
        }

        /// <summary>
        /// Returns true if this cart has the micropay product in it
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has micropay product]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasMicropayProduct()
        {
            return m_CartItems.HasMicropayProduct;
        }

        /// <summary>
        /// Returns true if this cart has any items which are system items
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has system components]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasSystemComponents()
        {
            return m_CartItems.HasSystemComponents;
        }

        /// <summary>
        /// Returns true if this cart has any items that were purchased for another person's Gift Registry
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has gift registry components]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasGiftRegistryComponents()
        {
            return m_CartItems.HasGiftRegistryComponents;
        }

        /// <summary>
        /// Returns true when the cart has gift registry items and the shipping address is not owned by the cart owner
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has gift registry addresses]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasGiftRegistryAddresses()
        {
            return m_CartItems.HasGiftRegistryAddresses(ThisCustomer);
        }

        /// <summary>
        /// Returns true if this cart has any items that has Distributor
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has distributor components]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasDistributorComponents()
        {
            return m_CartItems.HasDistributorComponents;
        }

        /// <summary>
        /// returns true if this order has ONLY download items
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is all download components]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllDownloadComponents()
        {
            return m_CartItems.IsAllDownloadComponents;
        }

        /// <summary>
        /// returns true if this order has all items that
        /// do not require shipping or are marked as free shipping
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is all free shipping components]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllFreeShippingComponents()
        {
            return m_CartItems.IsAllFreeShippingComponents;
        }

        /// <summary>
        /// returns true if this order has ONLY items that don't require shipping...ever
        /// </summary>
        /// <returns></returns>
        public bool NoShippingRequiredComponents()
        {
            return Shipping.NoShippingRequiredComponents(CartItems);
        }

        /// <summary>
        /// returns true if this order has ONLY system items:
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is all system components]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllSystemComponents()
        {
            return m_CartItems.IsAllSystemComponents;
        }

        /// <summary>
        /// returns true if this order has any download items that have download locations
        /// </summary>
        /// <returns></returns>
        public bool ThereAreDownloadFilesSpecified()
        {
            return m_CartItems.ThereAreDownloadFilesSpecified;
        }

        /// <summary>
        /// Determines whether the shopping cart contains the specified product ID.
        /// </summary>
        /// <param name="ProductID">The product ID.</param>
        /// <param name="VariantID">The variant ID.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified product ID]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(int ProductID, int VariantID)
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.ProductID == ProductID && c.VariantID == VariantID)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the shopping cart contains the specified product ID.
        /// </summary>
        /// <param name="ProductID">The product ID.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified product ID]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(int ProductID)
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.ProductID == ProductID)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns quantity of the specified product id in the shopping cart.
        /// </summary>
        /// <param name="ProductID">The product ID.</param>
        /// <returns></returns>
        public int Count(int ProductID)
        {
            return m_CartItems.Where(ci => ci.ProductID.Equals(ProductID)).Sum(p => p.Quantity);
        }

        /// <summary>
        /// Returns the ordinal of the shipping address.
        /// </summary>
        /// <param name="ShippingAddressID">The shipping address ID.</param>
        /// <returns></returns>
        public int NumAtThisShippingAddress(int ShippingAddressID)
        {
            return m_CartItems.NumAtThisShippingAddress(ShippingAddressID);
        }

        /// <summary>
        /// Determines whether the shopping cart contains gift card.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [contains gift card]; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsGiftCard()
        {
            return m_CartItems.ContainsGiftCard;
        }

        /// <summary>
        /// Determines whether the shopping cart items is all email gift cards.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is all email gift cards]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllEmailGiftCards()
        {
            return m_CartItems.IsAllEmailGiftCards;
        }

        /// <summary>
        /// Determins whether the shopping cart has any gift card items
        /// </summary>
        public bool HasGiftCards
        {
            get { return m_CartItems.ContainsGiftCard; }
        }

        /// <summary>
        /// Determines whether the shopping cart contains recurring items.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance contains recurring; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsRecurring()
        {
            return m_CartItems.ContainsRecurring;
        }

        /// <summary>
        /// Determines whether all items in the cart has a default shipping address
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is all default shipping address items]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllDefaultShippingAddressItems()
        {
            return m_CartItems.IsAllDefaultShippingAddressItems(ThisCustomer);
        }

        /// <summary>
        /// Returns the shopping cart's first item.
        /// </summary>
        /// <returns></returns>
        public CartItem FirstItem()
        {
            return m_CartItems.FirstItem();
        }

        /// <summary>
        /// Returns the shopping cart's first item ShippingAddressID.
        /// </summary>
        /// <returns></returns>
        public int FirstItemShippingAddressID()
        {
            return m_CartItems.FirstItemShippingAddressID();
        }

        /// <summary>
        /// Returns true if an item in the shopping cart has primary shipping address.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has primary shipping address items]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasPrimaryShippingAddressItems()
        {
            return m_CartItems.HasPrimaryShippingAddressItems(ThisCustomer);
        }

        /// <summary>
        /// Returns true if no item in the shopping cart has primary shipping address.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has non primary shipping address items]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasNonPrimaryShippingAddressItems()
        {
            return m_CartItems.HasNonPrimaryShippingAddressItems(ThisCustomer);
        }

        /// <summary>
        /// Returns true if the shopping cart has multiple shipping addresses.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has multiple shipping addresses]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasMultipleShippingAddresses()
        {
            return m_CartItems.HasMultipleShippingAddresses;
        }

        /// <summary>
        // set the FreeShipping member variables
        /// </summary>
        /// <param name="AddressID">Shipping AddressID to qualify for free shipping</param>
        public void AnalyzeCartForFreeShippingConditions(int AddressID)
        {
            m_FreeShippingReason = Shipping.FreeShippingReasonEnum.DoesNotQualify;          // assume following tests will all fail
            m_FreeShippingThreshold = AppLogic.AppConfigUSDecimal("FreeShippingThreshold");
            m_MoreNeededToReachFreeShipping = 0;

            if (m_ShipCalcID == Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping) // test if all orders have free shipping
            {
                m_FreeShippingReason = Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping;
            }
            if (IsAllDownloadComponents())                                     // test if all items in cart are downloads
            {
                m_FreeShippingReason = Shipping.FreeShippingReasonEnum.AllDownloadItems;
            }
            if (IsAllFreeShippingComponents())                                 // test if all items in cart have free shipping
            {
                m_FreeShippingReason = Shipping.FreeShippingReasonEnum.AllFreeShippingItems;
            }

            if (FreeShippingThreshold != 0)                                    // test if free shipping threshold is met to the specified address
            {
                // check State and Country for valid FreeShippingMethods (Zones, aka intra-country areas, are not checked, as this is not supported)
                Address shipToAddress = new Address();
                shipToAddress.LoadFromDB(AddressID);

                string shippingMethodIDIfFreeShippingIsOn = AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn");

                if (!CommonLogic.IsStringNullOrEmpty(shippingMethodIDIfFreeShippingIsOn))
                {
                    foreach (string strFreeShippingMethdID in AppLogic.AppConfig("ShippingMethodIDIfFreeShippingIsOn").Split(','))
                    {
                        int FreeShippingMethodID = Convert.ToInt32(strFreeShippingMethdID.Trim());

                        if (Shipping.ShippingMethodIsValid(FreeShippingMethodID, shipToAddress.State, shipToAddress.Country) || m_ShipCalcID == Shipping.ShippingCalculationEnum.UseRealTimeRates)
                        {
                            decimal dSubTotal = SubTotal(true, false, true, true, true, true);

                            m_MoreNeededToReachFreeShipping = Math.Max(FreeShippingThreshold - dSubTotal, 0);  // cannot be negative

                            if (dSubTotal >= FreeShippingThreshold)
                            {
                                m_FreeShippingReason = Shipping.FreeShippingReasonEnum.ExceedsFreeShippingThreshold;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    _shippingThresHoldIsDefinedButFreeShippingMethodIDIsNot = true;
                }
            }

            if (ThisCustomer.CustomerLevelID != 0 && AppLogic.CustomerLevelHasFreeShipping(ThisCustomer.CustomerLevelID)) // test if customer level provides free shipping
            {
                m_FreeShippingReason = Shipping.FreeShippingReasonEnum.CustomerLevelHasFreeShipping;
            }
            if (HasCoupon() && CouponIsValid && Coupon.m_discountincludesfreeshipping) // test if a coupon is being used that offers free shipping
            {
                m_FreeShippingReason = Shipping.FreeShippingReasonEnum.CouponHasFreeShipping;
            }

            // set flags for shopping cart pages
            m_ShippingIsFree = (m_FreeShippingReason != Shipping.FreeShippingReasonEnum.DoesNotQualify);
            m_CartAllowsShippingMethodSelection = !(m_FreeShippingReason == Shipping.FreeShippingReasonEnum.AllDownloadItems);
        }

        public string GetFreeShippingReason()
        {
            String Reason = String.Empty;
            if (FreeShippingReason ==
                Shipping.FreeShippingReasonEnum.AllDownloadItems)
            {
                Reason = AppLogic.GetString("checkoutshipping.aspx.5", SkinID, ThisCustomer.LocaleSetting);
            }
            if (FreeShippingReason ==
                Shipping.FreeShippingReasonEnum.AllFreeShippingItems)
            {
                Reason = AppLogic.GetString("checkoutshipping.aspx.18", SkinID, ThisCustomer.LocaleSetting);
            }
            if (FreeShippingReason ==
                Shipping.FreeShippingReasonEnum.CouponHasFreeShipping)
            {
                Reason = AppLogic.GetString("checkoutshipping.aspx.6", SkinID, ThisCustomer.LocaleSetting);
            }
            if (FreeShippingReason ==
                Shipping.FreeShippingReasonEnum.AllOrdersHaveFreeShipping)
            {
                Reason = String.Format(AppLogic.GetString("checkoutshipping.aspx.7", SkinID, ThisCustomer.LocaleSetting), FreeShippingMethod);
            }
            if (FreeShippingReason ==
                Shipping.FreeShippingReasonEnum.CustomerLevelHasFreeShipping)
            {
                Reason = String.Format(AppLogic.GetString("checkoutshipping.aspx.8", SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CustomerLevelName);
            }
            if (FreeShippingReason ==
                Shipping.FreeShippingReasonEnum.ExceedsFreeShippingThreshold)
            {
                Reason = AppLogic.GetString("checkoutshipping.aspx.24", SkinID, ThisCustomer.LocaleSetting);
            }

            return Reason;
        }

        // NOT updated for multi-ship or gift registry (yet)
        public static String DisplayMiniCart(Customer ThisCustomer, int m_SkinID, bool IncludeFrame)
        {
            return AppLogic.RunXmlPackage("minicart.xml.config", null, ThisCustomer, m_SkinID, "", "", false, false);
        }

        /// <summary>
        /// Gets the add to cart form.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="forPack">if set to <c>true</c> [for pack].</param>
        /// <param name="showWishListButton">if set to <c>true</c> [show wish list button].</param>
        /// <param name="showGiftRegistryButton">if set to <c>true</c> [show gift registry button].</param>
        /// <param name="ProductID">The product ID.</param>
        /// <param name="VariantID">The variant ID.</param>
        /// <param name="SkinID">The skin ID.</param>
        /// <param name="DisplayFormat">The display format.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="ColorChangeProductImage">if set to <c>true</c> [color change product image].</param>
        /// <param name="VariantStyle">The variant style.</param>
        /// <returns></returns>
        static public String GetAddToCartForm(Customer ThisCustomer, bool forPack, bool showWishListButton, bool showGiftRegistryButton, int ProductID, int VariantID, int SkinID, int DisplayFormat, String LocaleSetting, bool ColorChangeProductImage, VariantStyleEnum VariantStyle)
        {
            return GetAddToCartForm(ThisCustomer, forPack, false, showWishListButton, showGiftRegistryButton, ProductID, VariantID, SkinID, DisplayFormat, LocaleSetting, ColorChangeProductImage, VariantStyle);
        }
        static public String GetAddToCartFormCustom(Customer ThisCustomer, bool forPack, bool showWishListButton, bool showGiftRegistryButton, int ProductID, int VariantID, int SkinID, int DisplayFormat, String LocaleSetting, bool ColorChangeProductImage, VariantStyleEnum VariantStyle)
        {
            return GetAddToCartFormCustom(ThisCustomer, forPack, false, showWishListButton, showGiftRegistryButton, ProductID, VariantID, SkinID, DisplayFormat, LocaleSetting, ColorChangeProductImage, VariantStyle);
        }
        // ERPFlag = 0 means to use our normal variant with attributes
        // ERPFlag = 1 means to use ERP variant structure but recompute variants into size/color drop down lists
        // ERPFlag = 2 means to use ERP variant structure, with each variant in a dropdown list to select
        // 
        // To Support Order Editing, if QueryString contains cartrecid=N, we will reset the add to cart form to match the state of that cart record
        //
        /// <summary>
        /// Gets the add to cart form.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="forPack">if set to <c>true</c> [for pack].</param>
        /// <param name="forKit">if set to <c>true</c> [for kit].</param>
        /// <param name="showWishListButton">if set to <c>true</c> [show wish list button].</param>
        /// <param name="showGiftRegistryButton">if set to <c>true</c> [show gift registry button].</param>
        /// <param name="ProductID">The product ID.</param>
        /// <param name="VariantID">The variant ID.</param>
        /// <param name="SkinID">The skin ID.</param>
        /// <param name="DisplayFormat">The display format.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="ColorChangeProductImage">if set to <c>true</c> [color change product image].</param>
        /// <param name="VariantStyle">The variant style.</param>
        /// <returns></returns>
        static public String GetAddToCartForm(Customer ThisCustomer, bool forPack, bool forKit, bool showWishListButton, bool showGiftRegistryButton, int ProductID, int VariantID, int SkinID, int DisplayFormat, String LocaleSetting, bool ColorChangeProductImage, VariantStyleEnum VariantStyle)
        {
            return GetAddToCartForm(ThisCustomer, forPack, forKit, showWishListButton, showGiftRegistryButton, ProductID, VariantID, SkinID, DisplayFormat, LocaleSetting, ColorChangeProductImage, VariantStyle, false);
        }
        static public String GetAddToCartFormCustom(Customer ThisCustomer, bool forPack, bool forKit, bool showWishListButton, bool showGiftRegistryButton, int ProductID, int VariantID, int SkinID, int DisplayFormat, String LocaleSetting, bool ColorChangeProductImage, VariantStyleEnum VariantStyle)
        {
            return GetAddToCartFormCustom(ThisCustomer, forPack, forKit, showWishListButton, showGiftRegistryButton, ProductID, VariantID, SkinID, DisplayFormat, LocaleSetting, ColorChangeProductImage, VariantStyle, false);
        }

        // ERPFlag = 0 means to use our normal variant with attributes
        // ERPFlag = 1 means to use ERP variant structure but recompute variants into size/color drop down lists
        // ERPFlag = 2 means to use ERP variant structure, with each variant in a dropdown list to select
        // 
        // To Support Order Editing, if QueryString contains cartrecid=N, we will reset the add to cart form to match the state of that cart record
        //
        /// <summary>
        /// Gets the add to cart form.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="forPack">if set to <c>true</c> [for pack].</param>
        /// <param name="forKit">if set to <c>true</c> [for kit].</param>
        /// <param name="showWishListButton">if set to <c>true</c> [show wish list button].</param>
        /// <param name="showGiftRegistryButton">if set to <c>true</c> [show gift registry button].</param>
        /// <param name="ProductID">The product ID.</param>
        /// <param name="VariantID">The variant ID.</param>
        /// <param name="SkinID">The skin ID.</param>
        /// <param name="DisplayFormat">The display format.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="ColorChangeProductImage">if set to <c>true</c> [color change product image].</param>
        /// <param name="VariantStyle">The variant style.</param>
        /// <param name="isKit2">Is kit2 xml package</param>
        /// <returns></returns>
        /// <returns></returns>
        //Not supported in Ml express
        static public String GetAddToCartForm(Customer ThisCustomer, bool forPack, bool forKit, bool showWishListButton, bool showGiftRegistryButton, int ProductID, int VariantID, int SkinID, int DisplayFormat, String LocaleSetting, bool ColorChangeProductImage, VariantStyleEnum VariantStyle, bool isKit2)
        {
            int CartRecID = CommonLogic.QueryStringUSInt("CartRecID");
            String TextOptionForEdit = String.Empty;
            int QuantityForEdit = 0;
            String ChosenSizeForEdit = String.Empty;
            String ChosenColorForEdit = String.Empty;
            Decimal ProductPriceForEdit = System.Decimal.Zero;
            if (CartRecID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rscart = DB.GetRS("select * from ShoppingCart where ShoppingCartRecID=" + CartRecID.ToString() + " and VariantID = " + VariantID.ToString(), con))
                    {
                        if (rscart.Read())
                        {
                            TextOptionForEdit = DB.RSField(rscart, "TextOption");
                            QuantityForEdit = DB.RSFieldInt(rscart, "Quantity");
                            ChosenSizeForEdit = DB.RSFieldByLocale(rscart, "ChosenSize", Localization.GetDefaultLocale());
                            ChosenColorForEdit = DB.RSFieldByLocale(rscart, "ChosenColor", Localization.GetDefaultLocale());
                            ProductPriceForEdit = DB.RSFieldDecimal(rscart, "ProductPrice");
                        }
                    }
                }
            }

            string ProductSku = string.Empty;
            string SkuSuffix = string.Empty;
            bool CustomerEntersPrice = false;
            String CustomerEntersPricePrompt = string.Empty;
            String RestrictedQuantities = string.Empty;
            int MinimumQuantity = 0;
            bool TrackInventoryBySizeAndColor = false;
            bool TrackInventoryByColor = false;
            String ColorsMaster = string.Empty;
            String ColorsDisplay = string.Empty;
            String ColorSKUModifiers = string.Empty;
            String ColorOptionPrompt = string.Empty;
            bool TrackInventoryBySize = false;
            String SizesMaster = string.Empty;
            String SizesDisplay = string.Empty;
            String SizeSKUModifiers = string.Empty;
            String SizeOptionPrompt = string.Empty;
            bool HasSizePriceModifiers = false;
            bool HasColorPriceModifiers = false;
            string boardSuffix = string.Empty;
            String TextOptionPrompt = string.Empty;
            int TextOptionMaxLength = 0;
            int TaxClassID = 0;
            String SKU = string.Empty;
            String ExtensionData = string.Empty;
            String ExtensionData2 = string.Empty;
            bool IsRecurring = false;
            bool IsDownload = false;
            bool ShowTextOption = false;
            bool RequiresTextOption = false;
            String FormName = "AddToCartForm_" + ProductID.ToString() + "_" + VariantID.ToString();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select PV.*,P.*, pv.Sizes MLSizes, pv.Colors MLColors, p.ColorOptionPrompt MLColorOptionPrompt, p.SizeOptionPrompt MLSizeOptionPrompt, p.TextOptionPrompt MLTextOptionPrompt, p.TextOptionMaxLength from Product P  with (NOLOCK)  left outer join productvariant PV  with (NOLOCK)  on p.productid=pv.productid where PV.ProductID=P.ProductID and pv.VariantID=" + VariantID.ToString(), con))
                {
                    if (!rs.Read())
                    {
                        rs.Close();
                        return String.Empty;
                    }
                    if (!AppLogic.AppConfigBool("ShowBuyButtons") || !DB.RSFieldBool(rs, "showbuybutton") || AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
                    {
                        return string.Empty;
                    }
                    if (DB.RSFieldBool(rs, "IsCallToOrder"))
                    {
                        return "<div class='call-to-order-wrap' id='" + FormName + "'>" + AppLogic.GetString("common.cs.67", SkinID, LocaleSetting) + "</div>";
                    }

                    ProductSku = DB.RSField(rs, "SKU").Trim();
                    SkuSuffix = DB.RSField(rs, "SkuSuffix").Trim();
                    CustomerEntersPrice = DB.RSFieldBool(rs, "CustomerEntersPrice");
                    CustomerEntersPricePrompt = DB.RSFieldByLocale(rs, "CustomerEntersPricePrompt", ThisCustomer.LocaleSetting);
                    RestrictedQuantities = DB.RSField(rs, "RestrictedQuantities");
                    MinimumQuantity = DB.RSFieldInt(rs, "MinimumQuantity");

                    TrackInventoryBySizeAndColor = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");

                    TrackInventoryByColor = DB.RSFieldBool(rs, "TrackInventoryByColor");
                    ColorsMaster = DB.RSFieldByLocale(rs, "MLColors", Localization.GetDefaultLocale()).Trim();
                    ColorsDisplay = DB.RSFieldByLocale(rs, "MLColors", LocaleSetting).Trim();
                    ColorSKUModifiers = DB.RSField(rs, "ColorSKUModifiers").Trim();
                    ColorOptionPrompt = DB.RSFieldByLocale(rs, "MLColorOptionPrompt", LocaleSetting).Trim();

                    TrackInventoryBySize = DB.RSFieldBool(rs, "TrackInventoryBySize");
                    SizesMaster = DB.RSFieldByLocale(rs, "MLSizes", Localization.GetDefaultLocale()).Trim();
                    SizesDisplay = DB.RSFieldByLocale(rs, "MLSizes", LocaleSetting).Trim();
                    SizeSKUModifiers = DB.RSField(rs, "SizeSKUModifiers").Trim();
                    SizeOptionPrompt = DB.RSFieldByLocale(rs, "MLSizeOptionPrompt", LocaleSetting).Trim();

                    if (SizesDisplay.Length == 0)
                    {
                        SizesDisplay = SizesMaster;
                    }

                    if (ColorsDisplay.Length == 0)
                    {
                        ColorsDisplay = ColorsMaster;
                    }

                    HasSizePriceModifiers = SizesMaster.IndexOf('[') != -1;
                    HasColorPriceModifiers = ColorsMaster.IndexOf('[') != -1;
                    //boardSuffix = string.Format("_{0}_{1}", ProductID.ToString(), VariantID.ToString());
                    boardSuffix = string.Format("_{0}_{1}", "1", "1");

                    TextOptionPrompt = DB.RSFieldByLocale(rs, "MLTextOptionPrompt", LocaleSetting).Trim();
                    TextOptionMaxLength = DB.RSFieldInt(rs, "TextOptionMaxLength");

                    TaxClassID = DB.RSFieldInt(rs, "TaxClassID");

                    SKU = DB.RSField(rs, "SKU");

                    ExtensionData = DB.RSField(rs, "ExtensionData");
                    ExtensionData2 = DB.RSField(rs, "ExtensionData2");

                    IsRecurring = DB.RSFieldBool(rs, "IsRecurring");
                    IsDownload = DB.RSFieldBool(rs, "IsDownload");

                    if (VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
                    {
                        XmlDocument d = new XmlDocument();

                        using (SqlConnection conn2 = new SqlConnection(DB.GetDBConn()))
                        {
                            conn2.Open();
                            using (IDataReader rsx = DB.GetRS("select ExtensionData,ExtensionData2 from Product with (NOLOCK) where ProductID=" + DB.RSFieldInt(rs, "ProductID").ToString(), conn2))
                            {
                                rsx.Read();
                                ColorSKUModifiers = String.Empty;
                                SizeSKUModifiers = String.Empty;
                                try
                                {
                                    d.LoadXml(DB.RSField(rsx, "ExtensionData2"));
                                }
                                catch { }
                            }
                        }

                        foreach (XmlNode n in d.SelectNodes("GroupAttributes/GroupAttribute"))
                        {
                            String AttrName = XmlCommon.XmlAttribute(n, "Name");
                            if (AttrName.Equals("ATTR1", StringComparison.InvariantCultureIgnoreCase))
                            {
                                SizeOptionPrompt = XmlCommon.XmlAttribute(n, "Prompt");
                                String Separator = String.Empty;
                                foreach (XmlNode nn in n.SelectNodes("Values/Value"))
                                {
                                    SizesMaster += Separator;
                                    SizesMaster += nn.InnerText;
                                    Separator = ",";
                                }
                            }
                            else if (AttrName.Equals("ATTR2", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ColorOptionPrompt = XmlCommon.XmlAttribute(n, "Prompt");
                                String Separator = String.Empty;
                                foreach (XmlNode nn in n.SelectNodes("Values/Value"))
                                {
                                    ColorsMaster += Separator;
                                    ColorsMaster += nn.InnerText;
                                    Separator = ",";
                                }
                            }
                        }
                        SizesDisplay = SizesMaster;
                        ColorsDisplay = ColorsMaster;
                    }

                    if (TextOptionMaxLength == 0)
                    {
                        TextOptionMaxLength = 50;
                    }
                    if (ColorOptionPrompt.Length == 0)
                    {
                        ColorOptionPrompt = AppLogic.GetString("AppConfig.ColorOptionPrompt", SkinID, LocaleSetting);
                    }
                    if (ColorOptionPrompt.Length == 0)
                    {
                        ColorOptionPrompt = AppLogic.GetString("common.cs.68", SkinID, LocaleSetting);
                    }
                    if (SizeOptionPrompt.Length == 0)
                    {
                        SizeOptionPrompt = AppLogic.GetString("AppConfig.SizeOptionPrompt", SkinID, LocaleSetting);
                    }
                    if (SizeOptionPrompt.Length == 0)
                    {
                        SizeOptionPrompt = AppLogic.GetString("common.cs.69", SkinID, LocaleSetting);
                    }
                    if (TextOptionPrompt.Length == 0)
                    {
                        TextOptionPrompt = AppLogic.AppConfig("TextOptionPrompt");
                    }
                    if (TextOptionPrompt.Length == 0)
                    {
                        TextOptionPrompt = AppLogic.GetString("common.cs.70", SkinID, LocaleSetting);
                    }
                    ShowTextOption = false;
                    RequiresTextOption = DB.RSFieldBool(rs, "RequiresTextOption");
                    if (RequiresTextOption || DB.RSFieldByLocale(rs, "TextOptionPrompt", LocaleSetting).Trim().Length != 0)
                    {
                        ShowTextOption = true;
                    }
                }
            }

            if (ColorChangeProductImage)
            {
                ProductImageGallery ig = new ProductImageGallery(ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, SKU);
                ColorChangeProductImage = !ig.IsEmpty();
            }

            bool ProtectInventory = AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand");
            int ProtectInventoryMinQuantity = AppLogic.AppConfigUSInt("Inventory.MinQuantity"); // if qty is below this, addtocart will not allow it
            String InventoryControlList = String.Empty;
            if (ProtectInventory)
            {
                InventoryControlList = AppLogic.GetInventoryList(ProductID, VariantID, TrackInventoryByColor, TrackInventoryBySize, ProductSku, SkuSuffix, SizesMaster, ColorsMaster);
            }

            int inv = AppLogic.GetInventory(ProductID, VariantID, "", "");

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<div class='form add-to-cart-form' id='" + FormName + "'>\n");
            //tmpS.Append("<script type=\"text/javascript\" Language=\"JavaScript\">\n<!--\n");
            //tmpS.Append("var VariantMinimumQty_" + ProductID.ToString() + "_" + VariantID.ToString() + " = " + MinimumQuantity.ToString() + ";\n");
            //tmpS.Append("var SelectedVariantInventory_" + "1".ToString() + "_" + "1".ToString() + " = " + inv.ToString() + ";\n");

            //AppLogic.LstInventory.Clear();

            //if (ProtectInventory && InventoryControlList.Length != 0)
            //{
            //    bool first = true;
            //    foreach (String s in InventoryControlList.Split('|'))
            //    {
            //        if (first)
            //        {
            //            tmpS.Append("var board" + boardSuffix + " = new Array(");
            //        }
            //        else
            //        {
            //            tmpS.Append(",");
            //        }
            //        String[] ivals = s.Split(',');
            //        tmpS.Append("new Array('" + ivals[0].Replace("'", "").Trim() + "','" + ivals[1].Replace("'", "").Trim() + "','" + ivals[2].Replace("'", "").Trim() + "')");
            //        AppLogic.LstInventory.Add(new Inventory()
            //        {
            //            Color = ivals[0].Replace("'", "").Trim(),
            //            Size = ivals[1].Replace("'", "").Trim(),
            //            Quantity = ivals[2].Replace("'", "").Trim()
            //        });

            //        first = false;
            //    }
            //    tmpS.Append(");\n");
            //}

            //tmpS.Append("function " + FormName + "_Validator(theForm)\n");
            //tmpS.Append("	{\n");
            //tmpS.Append("	submitonce(theForm);\n");

            //if (!CustomerEntersPrice && (AppLogic.AppConfigBool("ShowQuantityOnProductPage") && !forKit) || (!AppLogic.AppConfigBool("HideKitQuantity") && forKit))
            //{
            //    tmpS.AppendFormat("	if ((theForm.Quantity_{0}_{1}.value*1) < 1)\n", "1", "1"); // convert form val to integer
            //    tmpS.Append("	{\n");
            //    tmpS.Append("		alert(\"" + AppLogic.GetString("common.cs.84", SkinID, LocaleSetting) + "\");\n");
            //    tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.focus();\n", "1", "1");
            //    tmpS.Append("		submitenabled(theForm);\n");
            //    tmpS.Append("		return (false);\n");
            //    tmpS.Append("    }\n");

            //    if (RestrictedQuantities.Length == 0 && MinimumQuantity != 0)
            //    {
            //        tmpS.AppendFormat("	if ((theForm.Quantity_{0}_{1}.value*1) < VariantMinimumQty_" + ProductID.ToString() + "_" + VariantID.ToString() + ")\n", "1", "1"); // convert form val to integer
            //        tmpS.Append("	{\n");
            //        tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.85", SkinID, LocaleSetting), "\"+VariantMinimumQty_" + ProductID.ToString() + "_" + VariantID.ToString() + " + \"") + "\");\n");
            //        tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.focus();\n", "1", "1");
            //        tmpS.Append("		submitenabled(theForm);\n");
            //        tmpS.Append("		return (false);\n");
            //        tmpS.Append("    }\n");
            //    }
            //}

            //if (SizesMaster.Length != 0 && !AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
            //{
            //  //  tmpS.AppendFormat("	if (theForm.Size_{0}_{1}.selectedIndex < 1)\n", ProductID, VariantID);
            //    tmpS.AppendFormat("	if (theForm.Size_{0}_{1}.selectedIndex < 1)\n", "1", "1");
            //    tmpS.Append("	{\n");
            //    tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.71", SkinID, LocaleSetting), SizeOptionPrompt.ToLowerInvariant()) + "\");\n");
            //    tmpS.AppendFormat("		theForm.Size_{0}_{1}.focus();\n", "1", "1");
            //    tmpS.Append("		submitenabled(theForm);\n");
            //    tmpS.Append("		return (false);\n");
            //    tmpS.Append("    }\n");
            //}
            //if (ColorsMaster.Length != 0 && !AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
            //{
            //    tmpS.AppendFormat("	if (theForm.Color_{0}_{1}.selectedIndex < 1)\n", "1", "1");
            //    tmpS.Append("	{\n");
            //    tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.71", SkinID, LocaleSetting), ColorOptionPrompt.ToLowerInvariant()) + "\");\n");
            //    tmpS.AppendFormat("		theForm.Color_{0}_{1}.focus();\n", "1", "1");
            //    tmpS.Append("		submitenabled(theForm);\n");
            //    tmpS.Append("		return (false);\n");
            //    tmpS.Append("    }\n");
            //}
            //if (RequiresTextOption)
            //{
            //    tmpS.AppendFormat("	if (theForm.TextOption_{0}_{1}.value.length == 0)\n", ProductID, VariantID);
            //    tmpS.Append("	{\n");
            //    tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.73", SkinID, LocaleSetting), TextOptionPrompt) + "\");\n");
            //    tmpS.AppendFormat("		theForm.TextOption_{0}_{1}.focus();\n", ProductID, VariantID);
            //    tmpS.AppendFormat("		submitenabled(theForm);\n");
            //    tmpS.Append("		return (false);\n");
            //    tmpS.Append("    }\n");
            //}

            //if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes && ProtectInventory && AppLogic.AppConfigBool("ShowQuantityOnProductPage"))
            //{
            //    if (!TrackInventoryBySizeAndColor)
            //    {
            //        tmpS.AppendFormat("	if (theForm.Quantity_{0}_{1}.value > SelectedVariantInventory_" + ProductID.ToString() + "_" + VariantID.ToString() + ")\n", "1", "1");
            //        tmpS.Append("	{\n");
            //        tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.74", SkinID, LocaleSetting), "\"+SelectedVariantInventory_" + "1".ToString() + "_" + "1".ToString() + "+\"") + "\");\n");
            //        tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.value = SelectedVariantInventory_" + "1".ToString() + "_" + "1".ToString() + ";\n", "1", "1");
            //        tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.focus();\n", "1", "1");
            //        tmpS.Append("		submitenabled(theForm);\n");
            //        tmpS.Append("		return (false);\n");
            //        tmpS.Append("    }\n");
            //    }
            //    else
            //    {
            //        if (SizesMaster.Length != 0)
            //        {
            //            tmpS.AppendFormat("var sel_size = theForm.Size_{0}_{1}[theForm.Size_{0}_{1}.selectedIndex].value;\n", "1", "1");
            //            //JH 10.21.2010 removed append parameters that did not match formatting. This also matches the js builder below
            //            tmpS.Append("sel_size = sel_size.substring(0,sel_size.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n");
            //        }
            //        else
            //        {
            //            tmpS.Append("var sel_size = '';\n");
            //        }

            //        if (ColorsMaster.Length != 0)
            //        {
            //            tmpS.AppendFormat("var sel_color = theForm.Color_{0}_{1}[theForm.Color_{0}_{1}.selectedIndex].value;\n", "1", "1");
            //            tmpS.Append("sel_color = sel_color.substring(0,sel_color.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n");
            //        }
            //        else
            //        {
            //            tmpS.Append("var sel_color = '';\n");
            //        }

            //        tmpS.Append("if(typeof(sel_size) == 'undefined') sel_size = '';\n");
            //        tmpS.Append("if(typeof(sel_color) == 'undefined') sel_color = '';\n");

            //        // clean price delta options if any, so match will work on inventory control list:
            //        tmpS.Append("var j = sel_size.indexOf(\"[\");\n");
            //        tmpS.Append("if(j != -1)\n");
            //        tmpS.Append("{\n");
            //        tmpS.Append("	sel_size = Trim(sel_size.substring(0,j));\n");
            //        tmpS.Append("}\n");

            //        tmpS.Append("var i = sel_color.indexOf(\"[\");\n");
            //        tmpS.Append("if(i != -1)\n");
            //        tmpS.Append("{\n");
            //        tmpS.Append("	sel_color = Trim(sel_color.substring(0,i));\n");
            //        tmpS.Append("}\n");

            //        if (TrackInventoryBySize)
            //        {
            //            tmpS.Append("var sel_size_master = sel_size;\n");
            //        }
            //        else
            //        {
            //            tmpS.Append("var sel_size_master = '';\n");
            //        }

            //        if (TrackInventoryByColor)
            //        {
            //            tmpS.Append("var sel_color_master = sel_color;\n");
            //        }
            //        else
            //        {
            //            tmpS.Append("var sel_color_master = '';\n");
            //        }

            //        tmpS.AppendFormat("var sel_qty = theForm.Quantity_{0}_{1}.value;\n", "1", "1");
            //        tmpS.Append("var sizecolorfound = 0;\n");
            //        tmpS.Append("for(i = 0; i < board" + boardSuffix + ".length; i++)\n");
            //        tmpS.Append("{\n");
            //        tmpS.Append("	if(board" + boardSuffix + "[i][1] == sel_size_master && board" + boardSuffix + "[i][0] == sel_color_master)\n");
            //        tmpS.Append("	{\n");
            //        tmpS.Append("		sizecolorfound = 1;\n");
            //        tmpS.Append("		if(parseInt(sel_qty) > parseInt(board" + boardSuffix + "[i][2]))\n");
            //        tmpS.Append("		{\n");
            //        tmpS.Append("			if(parseInt(board" + boardSuffix + "[i][2]) == 0)\n");
            //        tmpS.Append("			{\n");
            //        tmpS.Append("               if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
            //        tmpS.Append("               if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
            //        tmpS.Append("				alert('" + String.Format(AppLogic.GetString("common.cs.75", SkinID, LocaleSetting), ColorOptionPrompt, SizeOptionPrompt, ColorOptionPrompt, SizeOptionPrompt).Replace(@"\\n", @"\n") + "');\n");
            //        tmpS.Append("			}\n");
            //        tmpS.Append("			else\n");
            //        tmpS.Append("			{\n");
            //        tmpS.Append("               if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
            //        tmpS.Append("               if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
            //        tmpS.Append("				alert('" + String.Format(AppLogic.GetString("common.cs.76", SkinID, LocaleSetting), ColorOptionPrompt, SizeOptionPrompt, ColorOptionPrompt, SizeOptionPrompt).Replace(@"\\n", @"\n").Replace("board", "board" + boardSuffix) + "');\n");
            //        tmpS.Append("			}\n");
            //        tmpS.Append("			submitenabled(theForm);\n");
            //        tmpS.Append("			return (false);\n");
            //        tmpS.Append("		}\n");
            //        tmpS.Append("	}\n");
            //        tmpS.Append("}\n");
            //        tmpS.Append("if(sizecolorfound == 0)\n");
            //        tmpS.Append("{\n");
            //        tmpS.Append("   if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
            //        tmpS.Append("   if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
            //        tmpS.Append("	alert('" + AppLogic.GetString("shoppingcart.cs.115", ThisCustomer.SkinID, ThisCustomer.LocaleSetting).Replace("'", "").Replace("{0}", "[' + sel_color + ']").Replace("{1}", "[' + sel_size + ']") + "');\n");
            //        tmpS.Append("	submitenabled(theForm);\n");
            //        tmpS.Append("	return (false);\n");
            //        tmpS.Append("}\n");
            //        tmpS.Append("submitenabled(theForm);\n");
            //    }
            //}
            //tmpS.Append("	submitenabled(theForm);\n");
            //tmpS.Append("	return (true);\n");
            //tmpS.Append("	}\n//-->\n");
            //tmpS.Append("</script>\n");


            // leave empty for now, let's make sure we set it's value when do a postback

            //tmpS.AppendFormat("<input name=\"AddToCart_{0}_{1}\" id=\"AddToCart_{0}_{1}\" type=\"hidden\" value=\"\">", ProductID, VariantID);
            tmpS.AppendFormat("<input name=\"VariantStyle_{0}_{1}\" id=\"VariantStyle_{0}_{1}\" type=\"hidden\" value=\"" + ((int)VariantStyle).ToString() + "\">", ProductID, VariantID);
            tmpS.AppendFormat("<input name=\"IsWishList_{0}_{1}\" id=\"IsWishList_{0}_{1}\" type=\"hidden\" value=\"0\">", ProductID, VariantID);
            tmpS.AppendFormat("<input name=\"IsGiftRegistry_{0}_{1}\" id=\"IsGiftRegistr_{0}_{1}\" type=\"hidden\" value=\"0\">", ProductID, VariantID);
            tmpS.AppendFormat("<input type=\"hidden\" name=\"UpsellProducts_{0}_{1}\" id=\"UpsellProducts_{0}_{1}\" value=\"\" class=\"aspdnsf_UpsellProducts\" >\n", ProductID, VariantID);
            tmpS.AppendFormat("<input type=\"hidden\" name=\"CartRecID_{0}_{1}\" id=\"CartRecID_{0}_{1}\" value=\"" + CartRecID.ToString() + "\">\n", ProductID, VariantID);
            tmpS.AppendFormat("<input type=\"hidden\" name=\"ProductID_{0}_{1}\" id=\"ProductID_{0}_{1}\" value=\"" + ProductID.ToString() + "\">\n", ProductID, VariantID);
            if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes || VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
            {
                tmpS.AppendFormat("<input type=\"hidden\" name=\"VariantID_{0}_{1}\" id=\"VariantID_{0}_{1}\" value=\"" + VariantID.ToString() + "\">\n", ProductID, VariantID);
            }
            if (forPack)
            {
                tmpS.AppendFormat("<input type=\"hidden\" name=\"IsSubmit_{0}_{1}\" id=\"IsSubmit_{0}_{1}t\" value=\"" + AppLogic.GetString("common.cs.77", SkinID, LocaleSetting) + "\">\n", ProductID, VariantID);
                tmpS.AppendFormat("<input type=\"hidden\" name=\"ProductTypeID_{0}_{1}\" id=\"ProductTypeID_{0}_{1}\" value=\"0\">\n", ProductID, VariantID);
                if (ColorsMaster.Length == 0)
                {
                    tmpS.AppendFormat("<input type=\"hidden\" name=\"Color_{0}_{1}\" id=\"Color_{0}_{1}\" value=\"\">\n", ProductID, VariantID);
                }
                if (SizesMaster.Length == 0)
                {
                    tmpS.AppendFormat("<input type=\"hidden\" name=\"Size_{0}_{1}\" id=\"Size_{0}_{1}\" value=\"\">\n", ProductID, VariantID);
                }
            }
            if (AppLogic.AppConfigBool("ShowPreviousPurchase") && AppLogic.Owns(ProductID, ThisCustomer.CustomerID))
            {
                tmpS.Append("<div class=\"form-text previous-purchase\">" + AppLogic.GetString("common.cs.86", SkinID, LocaleSetting) + "</div>");
            }

            if (RequiresTextOption || ShowTextOption)
            {
                tmpS.Append("<div class=\"form-group text-option-group\">");
                if (TextOptionMaxLength < 50)
                {
                    tmpS.Append("<label>");
                    tmpS.Append(TextOptionPrompt);
                    tmpS.Append("</label>");
                    tmpS.AppendFormat("<input type=\"text\" class=\"form-control text-option\" maxlength=\"" + TextOptionMaxLength.ToString() + "\"  name=\"TextOption_{0}_{1}\" id=\"TextOption{0}_{1}\" value=\"" + TextOptionForEdit + "\">", ProductID, VariantID);
                }
                else
                {
                    tmpS.Append("<label>");
                    tmpS.Append(TextOptionPrompt);
                    tmpS.Append("</label>");
                    tmpS.AppendFormat("<textarea rows=\"4\" class=\"form-control text-option\" name=\"TextOption_{0}_{1}\" id=\"TextOption{0}_{1}\" onkeypress=\"if(this.value.length>=" + TextOptionMaxLength.ToString() + " && ((event.keyCode < 33 || event.keyCode > 40) && event.keyCode != 45 && event.keyCode != 46)) {{return false;}} \">" + TextOptionForEdit + "</textarea>", ProductID, VariantID);
                }
                tmpS.Append("</div>");
            }

            tmpS.Append("<div class=\"item-controls\">");
            if (CustomerEntersPrice)
            {
                tmpS.Append("<label class=\"customer-enters-price-label\" for=\"Price_{0}_{1}\">");
                tmpS.Append(CustomerEntersPricePrompt);
                tmpS.Append("</label>");
                tmpS.AppendFormat(" <input maxLength=\"10\" class=\"form-control price-field\" name=\"Price_{0}_{1}\" id=\"Price_{0}_{1}\" value=\"" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ProductPriceForEdit) + "\">", ProductID, VariantID);
                tmpS.Append("<input type=\"hidden\" name=\"Price_vldt\" value=\"[req][number][blankalert=" + AppLogic.GetString("shoppingcart.cs.113", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("shoppingcart.cs.114", SkinID, LocaleSetting) + "]\">\n");
            }



            tmpS.Append("<p id=\"pInStock\"><span class=\"black-blu-label\"><font>In Stock: </font><label runat=\"server\" ClientIDMode=\"Static\" id=\"lblInStock\"/></span></p>");
            tmpS.Append("<p id=\"pOutofStock\"><span class=\"notify \">Out of Stock</span></p>");





            //Colors Alternative
            if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes || VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
            {
                if (SizesMaster.Length != 0)
                {
                    String[] SizesMasterSplit = SizesMaster.Split(',');
                    String[] SizesDisplaySplit = SizesDisplay.Split(',');
                    String[] SizeSKUsSplit = SizeSKUModifiers.Split(',');
                    // tmpS.AppendFormat(" <select class=\"form-control size-select\" name=\"Size_{0}_{1}\" id=\"Size_{0}_{1}\" >", ProductID, VariantID);
                    tmpS.AppendFormat(" <select class=\"select-list\" name=\"Size_{0}_{1}\" id=\"Size_{0}_{1}\" >", "1", "1");
                    if (!AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
                    {
                        tmpS.Append("<option value=\"-,-\">" + SizeOptionPrompt + "</option>\n");
                    }
                    for (int i = SizesMasterSplit.GetLowerBound(0); i <= SizesMasterSplit.GetUpperBound(0); i++)
                    {
                        string SizeString = SizesDisplaySplit[i].Trim();
                        string SizeStringValue = SizeString;
                        String Modifier = String.Empty;
                        try
                        {
                            Modifier = SizeSKUsSplit[i];
                        }
                        catch
                        { }
                        decimal SizePriceDeltaValue = AppLogic.GetColorAndSizePriceDelta("", SizesMasterSplit[i].Trim(), TaxClassID, ThisCustomer, true, false);
                        decimal SizePriceDelta = AppLogic.GetColorAndSizePriceDelta("", SizesMasterSplit[i].Trim(), TaxClassID, ThisCustomer, true, true);
                        if (SizeString.IndexOf("[") != -1)
                        {
                            SizeString = SizeString.Substring(0, SizeString.IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(SizePriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(SizePriceDelta) + "]";
                            SizeStringValue = SizeString.Substring(0, SizeString.IndexOf("[")).Trim() + " [" + CommonLogic.IIF(SizePriceDeltaValue > 0, "+", "") + Localization.CurrencyStringForGatewayWithoutExchangeRate(SizePriceDeltaValue) + "]";
                        }
                        tmpS.Append("<option value=\"" + AppLogic.CleanSizeColorOption(SizesMasterSplit[i]) + "," + Modifier + "\" " +
                            CommonLogic.IIF(ChosenSizeForEdit == SizeStringValue, " selected ", "") + ">" +
                            HttpContext.Current.Server.HtmlDecode(SizeString) + "</option>\n");
                    }
                    tmpS.Append("</select>");
                }
                if (ColorsMaster.Length != 0)
                {
                    String[] ColorsMasterSplit = ColorsMaster.Split(',');
                    String[] ColorsDisplaySplit = ColorsDisplay.Split(',');
                    String[] ColorSKUsSplit = ColorSKUModifiers.Split(',');
                    tmpS.Append(String.Format(" <select class=\"select-list\" id=\"Color_{0}_{1}\" name=\"Color_{0}_{1}\" onChange=\"" + CommonLogic.IIF(ColorChangeProductImage, "setcolorpic_" + ProductID.ToString() + "(this.value);", "") + "\" >\n", "1", "1"));

                    if (!AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
                    {
                        tmpS.Append("<option value=\"-,-\">" + ColorOptionPrompt + "</option>\n");
                    }
                    for (int i = ColorsMasterSplit.GetLowerBound(0); i <= ColorsMasterSplit.GetUpperBound(0); i++)
                    {
                        string ColorString = ColorsDisplaySplit[i].Trim();
                        string ColorStringValue = ColorString;
                        String Modifier = String.Empty;
                        try
                        {
                            Modifier = ColorSKUsSplit[i];
                        }
                        catch
                        { }
                        decimal ColorPriceDeltaValue = AppLogic.GetColorAndSizePriceDelta(ColorsMasterSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, false);
                        decimal ColorPriceDelta = AppLogic.GetColorAndSizePriceDelta(ColorsMasterSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, true);
                        if (ColorString.IndexOf("[") != -1)
                        {
                            ColorString = ColorString.Substring(0, ColorString.IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(ColorPriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(ColorPriceDelta) + "]";
                            ColorStringValue = ColorString.Substring(0, ColorString.IndexOf("[")).Trim() + " [" + CommonLogic.IIF(ColorPriceDeltaValue > 0, "+", "") + Localization.CurrencyStringForGatewayWithoutExchangeRate(ColorPriceDeltaValue) + "]";
                        }
                        tmpS.Append("<option value=\"" + AppLogic.CleanSizeColorOption(ColorsMasterSplit[i]) + "," + Modifier + "\" " +
                            CommonLogic.IIF(ChosenColorForEdit == ColorStringValue, " selected ", "") + ">" +
                            HttpContext.Current.Server.HtmlDecode(ColorString) + "</option>\n");
                    }
                    tmpS.Append("</select>");
                }
            }

            //if (inv > 4)
            //{
            //    tmpS.Append("<p><span class=\"black-blu-label\"><font>In stock: </font>" + inv + "</span></p>");
            //}



            //Quantity DropDown
            tmpS.Append("<Span class=\"select-quantity black-blu-label\">");
            if (!CustomerEntersPrice && (AppLogic.AppConfigBool("ShowQuantityOnProductPage") && !forKit) || (!AppLogic.AppConfigBool("HideKitQuantity") && forKit))
            {
                if (RestrictedQuantities.Length == 0)
                {
                    int InitialQ = 1;
                    if (MinimumQuantity > 0)
                    {
                        InitialQ = MinimumQuantity;
                    }
                    else if (AppLogic.AppConfig("DefaultAddToCartQuantity").Length != 0)
                    {
                        InitialQ = AppLogic.AppConfigUSInt("DefaultAddToCartQuantity");
                    }
                    if (QuantityForEdit != 0)
                    {
                        InitialQ = QuantityForEdit;
                    }
                    tmpS.AppendFormat("<font class=\"black-color\" for=\"Quantity_{0}_{1}\">" + AppLogic.GetString("common.cs.78", SkinID, LocaleSetting) + "</font> <input class=\"item-quantity\" type=\"text\" value=\"" + InitialQ.ToString() + "\" name=\"Quantity_{0}_{1}\" id=\"Quantity_{0}_{1}\" maxlength=\"6\">", "1", "1");
                    tmpS.Append("<input name=\"Quantity_vldt\" type=\"hidden\" value=\"[req][integer][number][blankalert=" + AppLogic.GetString("common.cs.79", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("common.cs.80", SkinID, LocaleSetting) + "]\">");
                }
                else
                {
                    tmpS.AppendFormat("<font class=\"black-color\" for=\"Quantity_{0}_{1}\">" + AppLogic.GetString("common.cs.78", SkinID, LocaleSetting) + "</font>", ProductID, VariantID);
                    tmpS.AppendFormat("<select class=\"item-quantity\" name=\"Quantity_{0}_{1}\" id=\"Quantity_{0}_{1}\" >", "1", "1");
                    foreach (String s in RestrictedQuantities.Split(','))
                    {
                        if (s.Trim().Length != 0)
                        {
                            int Q = Localization.ParseUSInt(s.Trim());
                            tmpS.Append("<option value=\"" + Q.ToString() + "\" " + CommonLogic.IIF(QuantityForEdit == Q, " selected ", "") + ">" + Q.ToString() + "</option>");
                        }
                    }
                    tmpS.Append("</select> ");
                }
            }
            else
            {
                tmpS.AppendFormat("<input class=\"item-quantity\" name=\"Quantity_{0}_{1}\" id=\"Quantity_{0}_{1}\" type=\"hidden\" value=\"1\">", ProductID, VariantID);
            }
            Decimal M = 1.0M;
            String MM = ThisCustomer.CurrencyString(M).Substring(0, 1); // get currency symbol
            if (CommonLogic.IsInteger(MM))
            {
                MM = String.Empty; // something international happened, so just leave empty, we only want currency symbol, not any digits
            }
            tmpS.Append("</span>");

            tmpS.Append("	</span></div>");

            bool showAddToCartButton = true;
            bool showAddToWishListButton = false;
            bool showAddGiftRegistryButton = false;

            showAddToCartButton = true;

            if (AppLogic.AppConfigBool("AddToCart.UseImageButton") && AppLogic.AppConfig("AddToCart.AddToCartButton") != "")
            {
                // render image button
                var src = AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToCartButton"));
                tmpS.AppendFormat(" <a href='JavaScript:void(0)'><img id=\"AddToCartButton_{0}_{1}\" name=\"AddToCartButton_{0}_{1}\" class=\"AddToCartButton\" src=\"{2}\" alt=\"{3}\" ", ProductID, VariantID, src, AppLogic.GetString("AppConfig.CartButtonPrompt", SkinID, LocaleSetting));

                var mouseOverImage = AppLogic.AppConfig("AddToCart.AddToCartButton_MouseOver");
                if (!string.IsNullOrEmpty(mouseOverImage))
                {
                    var mouseOverSrc = AppLogic.SkinImage(mouseOverImage);
                    tmpS.AppendFormat("  onmouseover=\"this.src = '{0}'\" ", mouseOverSrc);

                    // attach the mouseout event automatically to switch back to the original image
                    tmpS.AppendFormat("  onmouseout=\"this.src = '{0}'\" ", src);
                }

                var mouseDownImage = AppLogic.AppConfig("AddToCart.AddToCartButton_MouseDown");
                if (!string.IsNullOrEmpty(mouseDownImage))
                {
                    var mouseDownImageSrc = AppLogic.SkinImage(mouseDownImage);
                    tmpS.AppendFormat("  onmousedown=\"this.src = '{0}'\" ", mouseDownImageSrc);
                }

                tmpS.AppendFormat(" /></a>");
            }
            else
            {
                // render normal html button
                //Disable add to cart functionality and creat a simple button that opens popup
                // tmpS.AppendFormat(" <input type=\"button\" id=\"AddToCartButton_{0}_{1}\" name=\"AddToCartButton_{0}_{1}\" class=\"btn btn-primary btnaddtocart  btn-block call-to-action add-to-cart-button\" value=\"{2}\">", ProductID, VariantID, AppLogic.GetString("AppConfig.CartButtonPrompt", SkinID, LocaleSetting));
                tmpS.AppendFormat(" <input type=\"button\"  id=\"btnaddtocart\" name=\"AddToCartButton_{0}_{1}\" class=\"btn btn-primary btn-block call-to-action add-to-cart-button\" value=\"{2}\">", ProductID, VariantID, AppLogic.GetString("AppConfig.CartButtonPrompt", SkinID, LocaleSetting));
            }//data-toggle=\"modal\" data-target=\"#myModa2\"

            // TODO: Commented for the Wishlist and Gift button functionality 
            //if (AppLogic.AppConfigBool("ShowWishButtons") && showWishListButton)
            //{
            //    showAddToWishListButton = true;

            //    if (AppLogic.AppConfigBool("AddToCart.UseImageButton") && AppLogic.AppConfig("AddToCart.AddToWishButton") != "")
            //    {
            //        // render image button
            //        var src = AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToWishButton"));
            //        tmpS.AppendFormat("<input type=\"image\" id=\"AddToWishButton_{0}_{1}\" name=\"AddToWishButton_{0}_{1}\" class=\"button add-to-wishlist-button\" src=\"{2}\" alt=\"{3}\" ", ProductID, VariantID, src, AppLogic.GetString("AppConfig.WishButtonPrompt", SkinID, LocaleSetting));

            //        var mouseOverImage = AppLogic.AppConfig("AddToCart.AddToWishButton_MouseOver");
            //        if (!string.IsNullOrEmpty(mouseOverImage))
            //        {
            //            var mouseOverSrc = AppLogic.SkinImage(mouseOverImage);
            //            tmpS.AppendFormat("  onmouseover=\"this.src = '{0}'\" ", mouseOverSrc);

            //            // attach the mouseout event automatically to switch back to the original image
            //            tmpS.AppendFormat("  onmouseout=\"this.src = '{0}'\" ", src);
            //        }

            //        var mouseDownImage = AppLogic.AppConfig("AddToCart.AddToWishButton_MouseDown");
            //        if (!string.IsNullOrEmpty(mouseDownImage))
            //        {
            //            var mouseDownImageSrc = AppLogic.SkinImage(mouseDownImage);
            //            tmpS.AppendFormat("  onmousedown=\"this.src = '{0}'\" ", mouseDownImageSrc);
            //        }
            //    }
            //    else
            //    {
            //        // Render normal html button
            //        tmpS.AppendFormat("<input type=\"button\" id=\"AddToWishButton_{0}_{1}\" name=\"AddToWishButton_{0}_{1}\" class=\"button add-to-wishlist-button\" value=\"" + AppLogic.GetString("AppConfig.WishButtonPrompt", SkinID, LocaleSetting) + "\" >", ProductID, VariantID);
            //    }
            //}

            //if (AppLogic.AppConfigBool("ShowGiftRegistryButtons") && showGiftRegistryButton && !IsRecurring && !IsDownload)
            //{
            //    showAddGiftRegistryButton = true;

            //    tmpS.AppendFormat(" ");

            //    if (AppLogic.AppConfigBool("AddToCart.UseImageButton") && AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton") != "")
            //    {
            //        // render image button
            //        var src = AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton"));
            //        tmpS.AppendFormat("<input type=\"image\" id=\"AddToGiftButton_{0}_{1}\" name=\"AddToGiftButton_{0}_{1}\" class=\"button add-to-registry-button\" src=\"{2}\" alt=\"{3}\" ", ProductID, VariantID, src, AppLogic.GetString("AppConfig.GiftButtonPrompt", SkinID, LocaleSetting));

            //        var mouseOverImage = AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton_MouseOver");
            //        if (!string.IsNullOrEmpty(mouseOverImage))
            //        {
            //            var mouseOverSrc = AppLogic.SkinImage(mouseOverImage);
            //            tmpS.AppendFormat("  onmouseover=\"this.src = '{0}'\" ", mouseOverSrc);

            //            // attach the mouseout event automatically to switch back to the original image
            //            tmpS.AppendFormat("  onmouseout=\"this.src = '{0}'\" ", src);
            //        }

            //        var mouseDownImage = AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton_MouseDown");
            //        if (!string.IsNullOrEmpty(mouseDownImage))
            //        {
            //            var mouseDownImageSrc = AppLogic.SkinImage(mouseDownImage);
            //            tmpS.AppendFormat("  onmousedown=\"this.src = '{0}'\" ", mouseDownImageSrc);
            //        }
            //    }
            //    else
            //    {
            //        // render normal html button
            //        tmpS.AppendFormat("<input type=\"button\" id=\"AddToGiftButton_{0}_{1}\" class=\"button add-to-registry-button\" value=\"" + AppLogic.GetString("AppConfig.GiftButtonPrompt", SkinID, LocaleSetting) + "\" >", ProductID, VariantID);
            //    }
            //}                       

            if (AppLogic.GetNextVariant(ProductID, VariantID) == VariantID) // single variant product
            {
                PayPalAd productPageAd = new PayPalAd(PayPalAd.TargetPage.Product);
                if (productPageAd.Show)
                    tmpS.AppendLine(productPageAd.ImageScript);
            }

            tmpS.AppendLine();
            // For the move to using masterpages, the  primary prerequisite was to have the form on the master template
            // we therefore stripped this extension of it's own form tag
            // we resort to using javascript to do that
            tmpS.Append("<script type=\"text/javascript\" language=\"Javascript\" >\n");
            // attach a delegate function that get's called on browser DOM load            
            tmpS.Append("    $window_addLoad(function(){ \n");

            // also defined here is the delAtcx function
            // which is used on the addto-x button's onclick handler
            // this function will then call the __doPostBack function which is an asp.net standard javascript function
            // which has the following signature:
            //          __doPostBack(eventTarget, eventArgument) 
            // To simulate same-page postback through javascript
            // we use the "AddToCart" as the eventTarget, which we'll try to interpret as a command later during postback
            // and the the cartType, productid and variantid concantenated as the eventArgument
            // this cause the current page(showproduct.aspx) to postback to itself
            // which we will then try to interpret as addtocart postback 
            // via the __EVENTTARGET and __EVENTARGUMENT form values....


            tmpS.AppendFormat("        var ctrl_{0} = new aspdnsf.Controls.AddToCartForm({1}, {2}); \n", FormName, ProductID, VariantID);
            tmpS.AppendFormat("        ctrl_{0}.setValidationRoutine( function(){{ return {0}_Validator(theForm) }} );\n", FormName);

            bool useAjaxBehavior = AppLogic.AppConfigBool("Minicart.UseAjaxAddToCart") && !ThisCustomer.IsImpersonated && !AppLogic.CheckForMobileRequest();
            if (forKit || forPack)
            {
                // ajax addtocart is not supported for kits and packs
                useAjaxBehavior = false;
            }

            tmpS.AppendFormat("        ctrl_{0}.setUseAjaxBehavior({1});\n", FormName, useAjaxBehavior.ToString().ToLowerInvariant());

            if (showAddToCartButton)
            {
                tmpS.AppendFormat("        ctrl_{0}.registerButton('AddToCartButton_{1}_{2}', {3});\n", FormName, ProductID, VariantID, (int)CartTypeEnum.ShoppingCart);
            }
            if (showAddToWishListButton)
            {
                tmpS.AppendFormat("        ctrl_{0}.registerButton('AddToWishButton_{1}_{2}', {3});\n", FormName, ProductID, VariantID, (int)CartTypeEnum.WishCart);
            }
            if (showAddGiftRegistryButton)
            {
                tmpS.AppendFormat("        ctrl_{0}.registerButton('AddToGiftButton_{1}_{2}', {3});\n", FormName, ProductID, VariantID, (int)CartTypeEnum.GiftRegistryCart);
            }

            tmpS.Append("    });");
            tmpS.Append("</script>");
            tmpS.AppendLine();

            if (!AppLogic.AppConfigBool("BuySafe.DisableAddoToCartKicker") && AppLogic.GlobalConfigBool("BuySafe.Enabled") && AppLogic.GlobalConfig("BuySafe.Hash").Length != 0)
            {
                tmpS.AppendLine("<div class=\"form-text buysafe-kicker\" id=\"ProductBuySafeKicker\">");
                tmpS.AppendLine("<span id=\"buySAFE_Kicker\" name=\"buySAFE_Kicker\" type=\"" + AppLogic.AppConfig("BuySafe.KickerType") + "\"></span>");
                tmpS.AppendLine("</div>");
            }
            tmpS.Append("</div>");

            return tmpS.ToString();
        }
        static public String GetAddToCartFormCustom(Customer ThisCustomer, bool forPack, bool forKit, bool showWishListButton, bool showGiftRegistryButton, int ProductID, int VariantID, int SkinID, int DisplayFormat, String LocaleSetting, bool ColorChangeProductImage, VariantStyleEnum VariantStyle, bool isKit2)
        {
            int CartRecID = CommonLogic.QueryStringUSInt("CartRecID");
            String TextOptionForEdit = String.Empty;
            int QuantityForEdit = 0;
            String ChosenSizeForEdit = String.Empty;
            String ChosenColorForEdit = String.Empty;
            Decimal ProductPriceForEdit = System.Decimal.Zero;
            if (CartRecID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rscart = DB.GetRS("select * from ShoppingCart where ShoppingCartRecID=" + CartRecID.ToString() + " and VariantID = " + VariantID.ToString(), con))
                    {
                        if (rscart.Read())
                        {
                            TextOptionForEdit = DB.RSField(rscart, "TextOption");
                            QuantityForEdit = DB.RSFieldInt(rscart, "Quantity");
                            ChosenSizeForEdit = DB.RSFieldByLocale(rscart, "ChosenSize", Localization.GetDefaultLocale());
                            ChosenColorForEdit = DB.RSFieldByLocale(rscart, "ChosenColor", Localization.GetDefaultLocale());
                            ProductPriceForEdit = DB.RSFieldDecimal(rscart, "ProductPrice");
                        }
                    }
                }
            }

            string ProductSku = string.Empty;
            string SkuSuffix = string.Empty;
            bool CustomerEntersPrice = false;
            String CustomerEntersPricePrompt = string.Empty;
            String RestrictedQuantities = string.Empty;
            int MinimumQuantity = 0;
            bool TrackInventoryBySizeAndColor = false;
            bool TrackInventoryByColor = false;
            String ColorsMaster = string.Empty;
            String ColorsDisplay = string.Empty;
            String ColorSKUModifiers = string.Empty;
            String ColorOptionPrompt = string.Empty;
            bool TrackInventoryBySize = false;
            String SizesMaster = string.Empty;
            String SizesDisplay = string.Empty;
            String SizeSKUModifiers = string.Empty;
            String SizeOptionPrompt = string.Empty;
            bool HasSizePriceModifiers = false;
            bool HasColorPriceModifiers = false;
            string boardSuffix = string.Empty;
            String TextOptionPrompt = string.Empty;
            int TextOptionMaxLength = 0;
            int TaxClassID = 0;
            String SKU = string.Empty;
            String ExtensionData = string.Empty;
            String ExtensionData2 = string.Empty;
            bool IsRecurring = false;
            bool IsDownload = false;
            bool ShowTextOption = false;
            bool RequiresTextOption = false;
            String FormName = "AddToCartForm_" + ProductID.ToString() + "_" + VariantID.ToString();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select PV.*,P.*, pv.Sizes MLSizes, pv.Colors MLColors, p.ColorOptionPrompt MLColorOptionPrompt, p.SizeOptionPrompt MLSizeOptionPrompt, p.TextOptionPrompt MLTextOptionPrompt, p.TextOptionMaxLength from Product P  with (NOLOCK)  left outer join productvariant PV  with (NOLOCK)  on p.productid=pv.productid where PV.ProductID=P.ProductID and pv.VariantID=" + VariantID.ToString(), con))
                {
                    if (!rs.Read())
                    {
                        rs.Close();
                        return String.Empty;
                    }
                    if (!AppLogic.AppConfigBool("ShowBuyButtons") || !DB.RSFieldBool(rs, "showbuybutton") || AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
                    {
                        return string.Empty;
                    }
                    if (DB.RSFieldBool(rs, "IsCallToOrder"))
                    {
                        return "<div class='call-to-order-wrap' id='" + FormName + "'>" + AppLogic.GetString("common.cs.67", SkinID, LocaleSetting) + "</div>";
                    }

                    ProductSku = DB.RSField(rs, "SKU").Trim();
                    SkuSuffix = DB.RSField(rs, "SkuSuffix").Trim();
                    CustomerEntersPrice = DB.RSFieldBool(rs, "CustomerEntersPrice");
                    CustomerEntersPricePrompt = DB.RSFieldByLocale(rs, "CustomerEntersPricePrompt", ThisCustomer.LocaleSetting);
                    RestrictedQuantities = DB.RSField(rs, "RestrictedQuantities");
                    MinimumQuantity = DB.RSFieldInt(rs, "MinimumQuantity");

                    TrackInventoryBySizeAndColor = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");

                    TrackInventoryByColor = DB.RSFieldBool(rs, "TrackInventoryByColor");
                    ColorsMaster = DB.RSFieldByLocale(rs, "MLColors", Localization.GetDefaultLocale()).Trim();
                    ColorsDisplay = DB.RSFieldByLocale(rs, "MLColors", LocaleSetting).Trim();
                    ColorSKUModifiers = DB.RSField(rs, "ColorSKUModifiers").Trim();
                    ColorOptionPrompt = DB.RSFieldByLocale(rs, "MLColorOptionPrompt", LocaleSetting).Trim();

                    TrackInventoryBySize = DB.RSFieldBool(rs, "TrackInventoryBySize");
                    SizesMaster = DB.RSFieldByLocale(rs, "MLSizes", Localization.GetDefaultLocale()).Trim();
                    SizesDisplay = DB.RSFieldByLocale(rs, "MLSizes", LocaleSetting).Trim();
                    SizeSKUModifiers = DB.RSField(rs, "SizeSKUModifiers").Trim();
                    SizeOptionPrompt = DB.RSFieldByLocale(rs, "MLSizeOptionPrompt", LocaleSetting).Trim();

                    if (SizesDisplay.Length == 0)
                    {
                        SizesDisplay = SizesMaster;
                    }

                    if (ColorsDisplay.Length == 0)
                    {
                        ColorsDisplay = ColorsMaster;
                    }

                    HasSizePriceModifiers = SizesMaster.IndexOf('[') != -1;
                    HasColorPriceModifiers = ColorsMaster.IndexOf('[') != -1;
                    boardSuffix = string.Format("_{0}_{1}", ProductID.ToString(), VariantID.ToString());

                    TextOptionPrompt = DB.RSFieldByLocale(rs, "MLTextOptionPrompt", LocaleSetting).Trim();
                    TextOptionMaxLength = DB.RSFieldInt(rs, "TextOptionMaxLength");

                    TaxClassID = DB.RSFieldInt(rs, "TaxClassID");

                    SKU = DB.RSField(rs, "SKU");

                    ExtensionData = DB.RSField(rs, "ExtensionData");
                    ExtensionData2 = DB.RSField(rs, "ExtensionData2");

                    IsRecurring = DB.RSFieldBool(rs, "IsRecurring");
                    IsDownload = DB.RSFieldBool(rs, "IsDownload");

                    if (VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
                    {
                        XmlDocument d = new XmlDocument();

                        using (SqlConnection conn2 = new SqlConnection(DB.GetDBConn()))
                        {
                            conn2.Open();
                            using (IDataReader rsx = DB.GetRS("select ExtensionData,ExtensionData2 from Product with (NOLOCK) where ProductID=" + DB.RSFieldInt(rs, "ProductID").ToString(), conn2))
                            {
                                rsx.Read();
                                ColorSKUModifiers = String.Empty;
                                SizeSKUModifiers = String.Empty;
                                try
                                {
                                    d.LoadXml(DB.RSField(rsx, "ExtensionData2"));
                                }
                                catch { }
                            }
                        }

                        foreach (XmlNode n in d.SelectNodes("GroupAttributes/GroupAttribute"))
                        {
                            String AttrName = XmlCommon.XmlAttribute(n, "Name");
                            if (AttrName.Equals("ATTR1", StringComparison.InvariantCultureIgnoreCase))
                            {
                                SizeOptionPrompt = XmlCommon.XmlAttribute(n, "Prompt");
                                String Separator = String.Empty;
                                foreach (XmlNode nn in n.SelectNodes("Values/Value"))
                                {
                                    SizesMaster += Separator;
                                    SizesMaster += nn.InnerText;
                                    Separator = ",";
                                }
                            }
                            else if (AttrName.Equals("ATTR2", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ColorOptionPrompt = XmlCommon.XmlAttribute(n, "Prompt");
                                String Separator = String.Empty;
                                foreach (XmlNode nn in n.SelectNodes("Values/Value"))
                                {
                                    ColorsMaster += Separator;
                                    ColorsMaster += nn.InnerText;
                                    Separator = ",";
                                }
                            }
                        }
                        SizesDisplay = SizesMaster;
                        ColorsDisplay = ColorsMaster;
                    }

                    if (TextOptionMaxLength == 0)
                    {
                        TextOptionMaxLength = 50;
                    }
                    if (ColorOptionPrompt.Length == 0)
                    {
                        ColorOptionPrompt = AppLogic.GetString("AppConfig.ColorOptionPrompt", SkinID, LocaleSetting);
                    }
                    if (ColorOptionPrompt.Length == 0)
                    {
                        ColorOptionPrompt = AppLogic.GetString("common.cs.68", SkinID, LocaleSetting);
                    }
                    if (SizeOptionPrompt.Length == 0)
                    {
                        SizeOptionPrompt = AppLogic.GetString("AppConfig.SizeOptionPrompt", SkinID, LocaleSetting);
                    }
                    if (SizeOptionPrompt.Length == 0)
                    {
                        SizeOptionPrompt = AppLogic.GetString("common.cs.69", SkinID, LocaleSetting);
                    }
                    if (TextOptionPrompt.Length == 0)
                    {
                        TextOptionPrompt = AppLogic.AppConfig("TextOptionPrompt");
                    }
                    if (TextOptionPrompt.Length == 0)
                    {
                        TextOptionPrompt = AppLogic.GetString("common.cs.70", SkinID, LocaleSetting);
                    }
                    ShowTextOption = false;
                    RequiresTextOption = DB.RSFieldBool(rs, "RequiresTextOption");
                    if (RequiresTextOption || DB.RSFieldByLocale(rs, "TextOptionPrompt", LocaleSetting).Trim().Length != 0)
                    {
                        ShowTextOption = true;
                    }
                }
            }

            if (ColorChangeProductImage)
            {
                ProductImageGallery ig = new ProductImageGallery(ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, SKU);
                ColorChangeProductImage = !ig.IsEmpty();
            }

            bool ProtectInventory = AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand");
            int ProtectInventoryMinQuantity = AppLogic.AppConfigUSInt("Inventory.MinQuantity"); // if qty is below this, addtocart will not allow it
            String InventoryControlList = String.Empty;
            if (ProtectInventory)
            {
                InventoryControlList = AppLogic.GetInventoryList(ProductID, VariantID, TrackInventoryByColor, TrackInventoryBySize, ProductSku, SkuSuffix, SizesMaster, ColorsMaster);
            }

            int inv = AppLogic.GetInventory(ProductID, VariantID, "", "");

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<div class='form add-to-cart-form' id='" + FormName + "'>\n");
            tmpS.Append("<script type=\"text/javascript\" Language=\"JavaScript\">\n<!--\n");
            tmpS.Append("var VariantMinimumQty_" + "1".ToString() + "_" + "1".ToString() + " = " + MinimumQuantity.ToString() + ";\n");
            tmpS.Append("var SelectedVariantInventory_" + "1".ToString() + "_" + "1".ToString() + " = " + inv.ToString() + ";\n");

            AppLogic.LstInventory.Clear();

            if (ProtectInventory && InventoryControlList.Length != 0)
            {
                bool first = true;
                foreach (String s in InventoryControlList.Split('|'))
                {
                    if (first)
                    {
                        tmpS.Append("var board" + boardSuffix + " = new Array(");
                    }
                    else
                    {
                        tmpS.Append(",");
                    }
                    String[] ivals = s.Split(',');
                    tmpS.Append("new Array('" + ivals[0].Replace("'", "").Trim() + "','" + ivals[1].Replace("'", "").Trim() + "','" + ivals[2].Replace("'", "").Trim() + "')");
                    AppLogic.LstInventory.Add(new Inventory()
                    {
                        Color = ivals[0].Replace("'", "").Trim(),
                        Size = ivals[1].Replace("'", "").Trim(),
                        Quantity = ivals[2].Replace("'", "").Trim()
                    });

                    first = false;
                }
                tmpS.Append(");\n");
            }
            //if (ProtectInventory && InventoryControlList.Length != 0)
            //{
            //    bool first = true;
            //    foreach (String s in InventoryControlList.Split('|'))
            //    {
            //        if (first)
            //        {
            //            tmpS.Append("var board" + boardSuffix + " = new Array(");
            //        }
            //        else
            //        {
            //            tmpS.Append(",");
            //        }
            //        String[] ivals = s.Split(',');
            //        tmpS.Append("new Array('" + ivals[0].Replace("'", "").Trim() + "','" + ivals[1].Replace("'", "").Trim() + "','" + ivals[2].Replace("'", "").Trim() + "')");
            //        first = false;
            //    }
            //    tmpS.Append(");\n");
            //}

            tmpS.Append("function " + FormName + "_Validator(theForm)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	submitonce(theForm);\n");

            //Shehriyar Uncommented
            if (!CustomerEntersPrice && (AppLogic.AppConfigBool("ShowQuantityOnProductPage") && !forKit) || (!AppLogic.AppConfigBool("HideKitQuantity") && forKit))
            {
                //tmpS.AppendFormat("	if ((theForm.Quantity_{0}_{1}.value*1) < 1)\n", ProductID, VariantID); // convert form val to integer
                tmpS.AppendFormat("	if ((theForm.Quantity_{0}_{1}.value*1) < 1)\n", "1", "1"); // convert form val to integer
                tmpS.Append("	{\n");
                tmpS.Append("		alert(\"" + AppLogic.GetString("common.cs.84", SkinID, LocaleSetting) + "\");\n");
                //tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.focus();\n", ProductID, VariantID);
                tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.focus();\n", "1", "1");
                tmpS.Append("		submitenabled(theForm);\n");
                tmpS.Append("		return (false);\n");
                tmpS.Append("    }\n");

                if (RestrictedQuantities.Length == 0 && MinimumQuantity != 0)
                {
                    tmpS.AppendFormat("	if ((theForm.Quantity_{0}_{1}.value*1) < VariantMinimumQty_" + ProductID.ToString() + "_" + VariantID.ToString() + ")\n", ProductID, VariantID); // convert form val to integer
                    tmpS.Append("	{\n");
                    tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.85", SkinID, LocaleSetting), "\"+VariantMinimumQty_" + ProductID.ToString() + "_" + VariantID.ToString() + " + \"") + "\");\n");
                    tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.focus();\n", ProductID, VariantID);
                    tmpS.Append("		submitenabled(theForm);\n");
                    tmpS.Append("		return (false);\n");
                    tmpS.Append("    }\n");
                }
            }

            if (SizesMaster.Length != 0 && !AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
            {
                //tmpS.AppendFormat("	if (theForm.Size_{0}_{1}.selectedIndex < 1)\n", ProductID, VariantID);
                tmpS.AppendFormat("	if (theForm.Size_{0}_{1}.selectedIndex < 1)\n", "1", "1");
                tmpS.Append("	{\n");
                tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.71", SkinID, LocaleSetting), SizeOptionPrompt.ToLowerInvariant()) + "\");\n");
                //tmpS.AppendFormat("		theForm.Size_{0}_{1}.focus();\n", ProductID, VariantID);
                tmpS.AppendFormat("		theForm.Size_{0}_{1}.focus();\n", "1", "1");
                tmpS.Append("		submitenabled(theForm);\n");
                tmpS.Append("		return (false);\n");
                tmpS.Append("    }\n");
            }
            if (ColorsMaster.Length != 0 && !AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
            {
                //tmpS.AppendFormat("	if (theForm.Color_{0}_{1}.selectedIndex < 1)\n", ProductID, VariantID);
                tmpS.AppendFormat("	if (theForm.Color_{0}_{1}.selectedIndex < 1)\n", "1", "1");
                tmpS.Append("	{\n");
                tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.71", SkinID, LocaleSetting), ColorOptionPrompt.ToLowerInvariant()) + "\");\n");
                //tmpS.AppendFormat("		theForm.Color_{0}_{1}.focus();\n", ProductID, VariantID);
                tmpS.AppendFormat("		theForm.Color_{0}_{1}.focus();\n", "1", "1");
                tmpS.Append("		submitenabled(theForm);\n");
                tmpS.Append("		return (false);\n");
                tmpS.Append("    }\n");
            }
            //End

            if (RequiresTextOption)
            {
                tmpS.AppendFormat("	if (theForm.TextOption_{0}_{1}.value.length == 0)\n", "1", "1");
                tmpS.Append("	{\n");
                tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.73", SkinID, LocaleSetting), TextOptionPrompt) + "\");\n");
                tmpS.AppendFormat("		theForm.TextOption_{0}_{1}.focus();\n", "1", "1");
                tmpS.AppendFormat("		submitenabled(theForm);\n");
                tmpS.Append("		return (false);\n");
                tmpS.Append("    }\n");
            }

            if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes && ProtectInventory && AppLogic.AppConfigBool("ShowQuantityOnProductPage"))
            {
                if (!TrackInventoryBySizeAndColor)
                {
                    //Shehriyar
                    tmpS.AppendFormat("	if (theForm.Quantity_{0}_{1}.value > SelectedVariantInventory_" + 1 + "_" + 1 + ")\n", "1", "1");
                    tmpS.Append("	{\n");
                    tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.74", SkinID, LocaleSetting), "\"+SelectedVariantInventory_" + ProductID.ToString() + "_" + VariantID.ToString() + "+\"") + "\");\n");
                    tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.value = SelectedVariantInventory_" + 1 + "_" + 1 + ";\n", "1", "1");
                    tmpS.AppendFormat("		theForm.Quantity_{0}_{1}.focus();\n", ProductID, VariantID);
                    tmpS.Append("		submitenabled(theForm);\n");
                    tmpS.Append("		return (false);\n");
                    tmpS.Append("    }\n");
                    //End
                }
                else
                {
                    if (SizesMaster.Length != 0)
                    {
                        tmpS.AppendFormat("var sel_size = theForm.Size_{0}_{1}[theForm.Size_{0}_{1}.selectedIndex].value;\n", "1", "1");
                        //JH 10.21.2010 removed append parameters that did not match formatting. This also matches the js builder below
                        tmpS.Append("sel_size = sel_size.substring(0,sel_size.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n");
                    }
                    else
                    {
                        tmpS.Append("var sel_size = '';\n");
                    }

                    if (ColorsMaster.Length != 0)
                    {
                        tmpS.AppendFormat("var sel_color = theForm.Color_{0}_{1}[theForm.Color_{0}_{1}.selectedIndex].value;\n", "1", "1");
                        tmpS.Append("sel_color = sel_color.substring(0,sel_color.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n");
                    }
                    else
                    {
                        tmpS.Append("var sel_color = '';\n");
                    }

                    tmpS.Append("if(typeof(sel_size) == 'undefined') sel_size = '';\n");
                    tmpS.Append("if(typeof(sel_color) == 'undefined') sel_color = '';\n");

                    // clean price delta options if any, so match will work on inventory control list:
                    tmpS.Append("var j = sel_size.indexOf(\"[\");\n");
                    tmpS.Append("if(j != -1)\n");
                    tmpS.Append("{\n");
                    tmpS.Append("	sel_size = Trim(sel_size.substring(0,j));\n");
                    tmpS.Append("}\n");

                    tmpS.Append("var i = sel_color.indexOf(\"[\");\n");
                    tmpS.Append("if(i != -1)\n");
                    tmpS.Append("{\n");
                    tmpS.Append("	sel_color = Trim(sel_color.substring(0,i));\n");
                    tmpS.Append("}\n");

                    if (TrackInventoryBySize)
                    {
                        tmpS.Append("var sel_size_master = sel_size;\n");
                    }
                    else
                    {
                        tmpS.Append("var sel_size_master = '';\n");
                    }

                    if (TrackInventoryByColor)
                    {
                        tmpS.Append("var sel_color_master = sel_color;\n");
                    }
                    else
                    {
                        tmpS.Append("var sel_color_master = '';\n");
                    }

                    tmpS.AppendFormat("var sel_qty = theForm.Quantity_{0}_{1}.value;\n", "1", "1");
                    tmpS.Append("var sizecolorfound = 0;\n");
                    tmpS.Append("for(i = 0; i < board" + boardSuffix + ".length; i++)\n");
                    tmpS.Append("{\n");
                    tmpS.Append("	if(board" + boardSuffix + "[i][1] == sel_size_master && board" + boardSuffix + "[i][0] == sel_color_master)\n");
                    tmpS.Append("	{\n");
                    tmpS.Append("		sizecolorfound = 1;\n");
                    tmpS.Append("		if(parseInt(sel_qty) > parseInt(board" + boardSuffix + "[i][2]))\n");
                    tmpS.Append("		{\n");
                    tmpS.Append("			if(parseInt(board" + boardSuffix + "[i][2]) == 0)\n");
                    tmpS.Append("			{\n");
                    tmpS.Append("               if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("               if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("				alert('" + String.Format(AppLogic.GetString("common.cs.75", SkinID, LocaleSetting), ColorOptionPrompt, SizeOptionPrompt, ColorOptionPrompt, SizeOptionPrompt).Replace(@"\\n", @"\n") + "');\n");
                    tmpS.Append("			}\n");
                    tmpS.Append("			else\n");
                    tmpS.Append("			{\n");
                    tmpS.Append("               if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("               if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("				alert('" + String.Format(AppLogic.GetString("common.cs.76", SkinID, LocaleSetting), ColorOptionPrompt, SizeOptionPrompt, ColorOptionPrompt, SizeOptionPrompt).Replace(@"\\n", @"\n").Replace("board", "board" + boardSuffix) + "');\n");
                    tmpS.Append("			}\n");
                    tmpS.Append("			submitenabled(theForm);\n");
                    tmpS.Append("			return (false);\n");
                    tmpS.Append("		}\n");
                    tmpS.Append("	}\n");
                    tmpS.Append("}\n");
                    tmpS.Append("if(sizecolorfound == 0)\n");
                    tmpS.Append("{\n");
                    tmpS.Append("   if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("   if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("	alert('" + AppLogic.GetString("shoppingcart.cs.115", ThisCustomer.SkinID, ThisCustomer.LocaleSetting).Replace("'", "").Replace("{0}", "[' + sel_color + ']").Replace("{1}", "[' + sel_size + ']") + "');\n");
                    tmpS.Append("	submitenabled(theForm);\n");
                    tmpS.Append("	return (false);\n");
                    tmpS.Append("}\n");
                    tmpS.Append("submitenabled(theForm);\n");
                }
            }
            tmpS.Append("	submitenabled(theForm);\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n//-->\n");
            tmpS.Append("</script>\n");


            // leave empty for now, let's make sure we set it's value when do a postback

            //tmpS.AppendFormat("<input name=\"AddToCart_{0}_{1}\" id=\"AddToCart_{0}_{1}\" type=\"hidden\" value=\"\">", ProductID, VariantID);
            tmpS.AppendFormat("<input name=\"VariantStyle_{0}_{1}\" id=\"VariantStyle_{0}_{1}\" type=\"hidden\" value=\"" + ((int)VariantStyle).ToString() + "\">", ProductID, VariantID);
            tmpS.AppendFormat("<input name=\"IsWishList_{0}_{1}\" id=\"IsWishList_{0}_{1}\" type=\"hidden\" value=\"0\">", ProductID, VariantID);
            tmpS.AppendFormat("<input name=\"IsGiftRegistry_{0}_{1}\" id=\"IsGiftRegistr_{0}_{1}\" type=\"hidden\" value=\"0\">", ProductID, VariantID);
            tmpS.AppendFormat("<input type=\"hidden\" name=\"UpsellProducts_{0}_{1}\" id=\"UpsellProducts_{0}_{1}\" value=\"\" class=\"aspdnsf_UpsellProducts\" >\n", ProductID, VariantID);
            tmpS.AppendFormat("<input type=\"hidden\" name=\"CartRecID_{0}_{1}\" id=\"CartRecID_{0}_{1}\" value=\"" + CartRecID.ToString() + "\">\n", ProductID, VariantID);
            tmpS.AppendFormat("<input type=\"hidden\" name=\"ProductID_{0}_{1}\" id=\"ProductID_{0}_{1}\" value=\"" + ProductID.ToString() + "\">\n", ProductID, VariantID);
            if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes || VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
            {
                tmpS.AppendFormat("<input type=\"hidden\" name=\"VariantID_{0}_{1}\" id=\"VariantID_{0}_{1}\" value=\"" + VariantID.ToString() + "\">\n", ProductID, VariantID);
            }
            if (forPack)
            {
                tmpS.AppendFormat("<input type=\"hidden\" name=\"IsSubmit_{0}_{1}\" id=\"IsSubmit_{0}_{1}t\" value=\"" + AppLogic.GetString("common.cs.77", SkinID, LocaleSetting) + "\">\n", ProductID, VariantID);
                tmpS.AppendFormat("<input type=\"hidden\" name=\"ProductTypeID_{0}_{1}\" id=\"ProductTypeID_{0}_{1}\" value=\"0\">\n", ProductID, VariantID);
                if (ColorsMaster.Length == 0)
                {
                    tmpS.AppendFormat("<input type=\"hidden\" name=\"Color_{0}_{1}\" id=\"Color_{0}_{1}\" value=\"\">\n", "1", "1");
                }
                if (SizesMaster.Length == 0)
                {
                    tmpS.AppendFormat("<input type=\"hidden\" name=\"Size_{0}_{1}\" id=\"Size_{0}_{1}\" value=\"\">\n", ProductID, VariantID);
                }
            }
            if (AppLogic.AppConfigBool("ShowPreviousPurchase") && AppLogic.Owns(ProductID, ThisCustomer.CustomerID))
            {
                tmpS.Append("<div class=\"form-text previous-purchase\">" + AppLogic.GetString("common.cs.86", SkinID, LocaleSetting) + "</div>");
            }

            if (RequiresTextOption || ShowTextOption)
            {
                tmpS.Append("<div class=\"form-group text-option-group\">");
                if (TextOptionMaxLength < 50)
                {
                    tmpS.Append("<label>");
                    tmpS.Append(TextOptionPrompt);
                    tmpS.Append("</label>");
                    tmpS.AppendFormat("<input type=\"text\" class=\"form-control text-option\" maxlength=\"" + TextOptionMaxLength.ToString() + "\"  name=\"TextOption_{0}_{1}\" id=\"TextOption{0}_{1}\" value=\"" + TextOptionForEdit + "\">", ProductID, VariantID);
                }
                else
                {
                    tmpS.Append("<label>");
                    tmpS.Append(TextOptionPrompt);
                    tmpS.Append("</label>");
                    tmpS.AppendFormat("<textarea rows=\"4\" class=\"form-control text-option\" name=\"TextOption_{0}_{1}\" id=\"TextOption{0}_{1}\" onkeypress=\"if(this.value.length>=" + TextOptionMaxLength.ToString() + " && ((event.keyCode < 33 || event.keyCode > 40) && event.keyCode != 45 && event.keyCode != 46)) {{return false;}} \">" + TextOptionForEdit + "</textarea>", ProductID, VariantID);
                }
                tmpS.Append("</div>");
            }

            //tmpS.Append("<div class=\"item-controls\">");
            //if (CustomerEntersPrice)
            //{
            //    tmpS.Append("<label class=\"customer-enters-price-label\" for=\"Price_{0}_{1}\">");
            //    tmpS.Append(CustomerEntersPricePrompt);
            //    tmpS.Append("</label>");
            //    tmpS.AppendFormat(" <input maxLength=\"10\" class=\"form-control price-field\" name=\"Price_{0}_{1}\" id=\"Price_{0}_{1}\" value=\"" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ProductPriceForEdit) + "\">", ProductID, VariantID);
            //    tmpS.Append("<input type=\"hidden\" name=\"Price_vldt\" value=\"[req][number][blankalert=" + AppLogic.GetString("shoppingcart.cs.113", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("shoppingcart.cs.114", SkinID, LocaleSetting) + "]\">\n");
            //}

            //Colors Alternative
            //if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes || VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
            //{
            //    if (SizesMaster.Length != 0)
            //    {
            //        String[] SizesMasterSplit = SizesMaster.Split(',');
            //        String[] SizesDisplaySplit = SizesDisplay.Split(',');
            //        String[] SizeSKUsSplit = SizeSKUModifiers.Split(',');
            //        tmpS.AppendFormat(" <select class=\"form-control size-select\" name=\"Size_{0}_{1}\" id=\"Size_{0}_{1}\" >", ProductID, VariantID);
            //        if (!AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
            //        {
            //            tmpS.Append("<option value=\"-,-\">" + SizeOptionPrompt + "</option>\n");
            //        }
            //        for (int i = SizesMasterSplit.GetLowerBound(0); i <= SizesMasterSplit.GetUpperBound(0); i++)
            //        {
            //            string SizeString = SizesDisplaySplit[i].Trim();
            //            string SizeStringValue = SizeString;
            //            String Modifier = String.Empty;
            //            try
            //            {
            //                Modifier = SizeSKUsSplit[i];
            //            }
            //            catch
            //            { }
            //            decimal SizePriceDeltaValue = AppLogic.GetColorAndSizePriceDelta("", SizesMasterSplit[i].Trim(), TaxClassID, ThisCustomer, true, false);
            //            decimal SizePriceDelta = AppLogic.GetColorAndSizePriceDelta("", SizesMasterSplit[i].Trim(), TaxClassID, ThisCustomer, true, true);
            //            if (SizeString.IndexOf("[") != -1)
            //            {
            //                SizeString = SizeString.Substring(0, SizeString.IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(SizePriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(SizePriceDelta) + "]";
            //                SizeStringValue = SizeString.Substring(0, SizeString.IndexOf("[")).Trim() + " [" + CommonLogic.IIF(SizePriceDeltaValue > 0, "+", "") + Localization.CurrencyStringForGatewayWithoutExchangeRate(SizePriceDeltaValue) + "]";
            //            }
            //            tmpS.Append("<option value=\"" + AppLogic.CleanSizeColorOption(SizesMasterSplit[i]) + "," + Modifier + "\" " +
            //                CommonLogic.IIF(ChosenSizeForEdit == SizeStringValue, " selected ", "") + ">" +
            //                HttpContext.Current.Server.HtmlDecode(SizeString) + "</option>\n");
            //        }
            //        tmpS.Append("</select>");
            //    }
            //    if (ColorsMaster.Length != 0)
            //    {
            //        String[] ColorsMasterSplit = ColorsMaster.Split(',');
            //        String[] ColorsDisplaySplit = ColorsDisplay.Split(',');
            //        String[] ColorSKUsSplit = ColorSKUModifiers.Split(',');
            //        tmpS.Append(String.Format(" <select class=\"form-control color-select\" id=\"Color_{0}_{1}\" name=\"Color_{0}_{1}\" onChange=\"" + CommonLogic.IIF(ColorChangeProductImage, "setcolorpic_" + ProductID.ToString() + "(this.value);", "") + "\" >\n", ProductID, VariantID));

            //        if (!AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
            //        {
            //            tmpS.Append("<option value=\"-,-\">" + ColorOptionPrompt + "</option>\n");
            //        }
            //        for (int i = ColorsMasterSplit.GetLowerBound(0); i <= ColorsMasterSplit.GetUpperBound(0); i++)
            //        {
            //            string ColorString = ColorsDisplaySplit[i].Trim();
            //            string ColorStringValue = ColorString;
            //            String Modifier = String.Empty;
            //            try
            //            {
            //                Modifier = ColorSKUsSplit[i];
            //            }
            //            catch
            //            { }
            //            decimal ColorPriceDeltaValue = AppLogic.GetColorAndSizePriceDelta(ColorsMasterSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, false);
            //            decimal ColorPriceDelta = AppLogic.GetColorAndSizePriceDelta(ColorsMasterSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, true);
            //            if (ColorString.IndexOf("[") != -1)
            //            {
            //                ColorString = ColorString.Substring(0, ColorString.IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(ColorPriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(ColorPriceDelta) + "]";
            //                ColorStringValue = ColorString.Substring(0, ColorString.IndexOf("[")).Trim() + " [" + CommonLogic.IIF(ColorPriceDeltaValue > 0, "+", "") + Localization.CurrencyStringForGatewayWithoutExchangeRate(ColorPriceDeltaValue) + "]";
            //            }
            //            tmpS.Append("<option value=\"" + AppLogic.CleanSizeColorOption(ColorsMasterSplit[i]) + "," + Modifier + "\" " +
            //                CommonLogic.IIF(ChosenColorForEdit == ColorStringValue, " selected ", "") + ">" +
            //                HttpContext.Current.Server.HtmlDecode(ColorString) + "</option>\n");
            //        }
            //        tmpS.Append("</select>");
            //    }
            //}

            //if (inv > 4)
            //{
            //    tmpS.Append("<p><span class=\"black-blu-label\"><font>In stock: </font>" + inv + "</span></p>");
            //}



            //Quantity DropDown
            //tmpS.Append("<Span class=\"select-quantity black-blu-label\">");
            //if (!CustomerEntersPrice && (AppLogic.AppConfigBool("ShowQuantityOnProductPage") && !forKit) || (!AppLogic.AppConfigBool("HideKitQuantity") && forKit))
            //{
            //    if (RestrictedQuantities.Length == 0)
            //    {
            //        int InitialQ = 1;
            //        if (MinimumQuantity > 0)
            //        {
            //            InitialQ = MinimumQuantity;
            //        }
            //        else if (AppLogic.AppConfig("DefaultAddToCartQuantity").Length != 0)
            //        {
            //            InitialQ = AppLogic.AppConfigUSInt("DefaultAddToCartQuantity");
            //        }
            //        if (QuantityForEdit != 0)
            //        {
            //            InitialQ = QuantityForEdit;
            //        }
            //        tmpS.AppendFormat("<font class=\"black-color\" for=\"Quantity_{0}_{1}\">" + AppLogic.GetString("common.cs.78", SkinID, LocaleSetting) + "</font> <input class=\"item-quantity\" type=\"text\" value=\"" + InitialQ.ToString() + "\" name=\"Quantity_{0}_{1}\" id=\"Quantity_{0}_{1}\" maxlength=\"4\">", ProductID, VariantID);
            //        tmpS.Append("<input name=\"Quantity_vldt\" type=\"hidden\" value=\"[req][integer][number][blankalert=" + AppLogic.GetString("common.cs.79", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("common.cs.80", SkinID, LocaleSetting) + "]\">");
            //    }
            //    else
            //    {
            //        tmpS.AppendFormat("<font class=\"black-color\" for=\"Quantity_{0}_{1}\">" + AppLogic.GetString("common.cs.78", SkinID, LocaleSetting) + "</font>", ProductID, VariantID);
            //        tmpS.AppendFormat("<select class=\"item-quantity\" name=\"Quantity_{0}_{1}\" id=\"Quantity_{0}_{1}\" >", ProductID, VariantID);
            //        foreach (String s in RestrictedQuantities.Split(','))
            //        {
            //            if (s.Trim().Length != 0)
            //            {
            //                int Q = Localization.ParseUSInt(s.Trim());
            //                tmpS.Append("<option value=\"" + Q.ToString() + "\" " + CommonLogic.IIF(QuantityForEdit == Q, " selected ", "") + ">" + Q.ToString() + "</option>");
            //            }
            //        }
            //        tmpS.Append("</select> ");
            //    }
            //}
            //else
            //{
            //    tmpS.AppendFormat("<input class=\"item-quantity\" name=\"Quantity_{0}_{1}\" id=\"Quantity_{0}_{1}\" type=\"hidden\" value=\"1\">", ProductID, VariantID);
            //}
            Decimal M = 1.0M;
            String MM = ThisCustomer.CurrencyString(M).Substring(0, 1); // get currency symbol
            if (CommonLogic.IsInteger(MM))
            {
                MM = String.Empty; // something international happened, so just leave empty, we only want currency symbol, not any digits
            }
            tmpS.Append("</span>");

            tmpS.Append("	</span></div>");

            bool showAddToCartButton = true;
            bool showAddToWishListButton = false;
            bool showAddGiftRegistryButton = false;

            showAddToCartButton = true;

            if (AppLogic.AppConfigBool("AddToCart.UseImageButton") && AppLogic.AppConfig("AddToCart.AddToCartButton") != "")
            {
                // render image button
                var src = AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToCartButton"));
                tmpS.AppendFormat(" <a href='JavaScript:void(0)'><img id=\"AddToCartButton_{0}_{1}\" name=\"AddToCartButton_{0}_{1}\" class=\"AddToCartButton\" src=\"{2}\" alt=\"{3}\" ", ProductID, VariantID, src, AppLogic.GetString("AppConfig.CartButtonPrompt", SkinID, LocaleSetting));

                var mouseOverImage = AppLogic.AppConfig("AddToCart.AddToCartButton_MouseOver");
                if (!string.IsNullOrEmpty(mouseOverImage))
                {
                    var mouseOverSrc = AppLogic.SkinImage(mouseOverImage);
                    tmpS.AppendFormat("  onmouseover=\"this.src = '{0}'\" ", mouseOverSrc);

                    // attach the mouseout event automatically to switch back to the original image
                    tmpS.AppendFormat("  onmouseout=\"this.src = '{0}'\" ", src);
                }

                var mouseDownImage = AppLogic.AppConfig("AddToCart.AddToCartButton_MouseDown");
                if (!string.IsNullOrEmpty(mouseDownImage))
                {
                    var mouseDownImageSrc = AppLogic.SkinImage(mouseDownImage);
                    tmpS.AppendFormat("  onmousedown=\"this.src = '{0}'\" ", mouseDownImageSrc);
                }

                tmpS.AppendFormat(" /></a>");
            }
            else
            {
                // render normal html button
                tmpS.AppendFormat(" <input type=\"button\" data-dismiss=\"modal\" id=\"AddToCartButton_{0}_{1}\" name=\"AddToCartButton_{0}_{1}\" class=\"btn btn-primary btn-block call-to-action add-to-cart-button\" value=\"{2}\">", ProductID, VariantID, AppLogic.GetString("AppConfig.CartButtonPrompt", SkinID, LocaleSetting));

            }

            // TODO: Commented for the Wishlist and Gift button functionality 
            //if (AppLogic.AppConfigBool("ShowWishButtons") && showWishListButton)
            //{
            //    showAddToWishListButton = true;

            //    if (AppLogic.AppConfigBool("AddToCart.UseImageButton") && AppLogic.AppConfig("AddToCart.AddToWishButton") != "")
            //    {
            //        // render image button
            //        var src = AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToWishButton"));
            //        tmpS.AppendFormat("<input type=\"image\" id=\"AddToWishButton_{0}_{1}\" name=\"AddToWishButton_{0}_{1}\" class=\"button add-to-wishlist-button\" src=\"{2}\" alt=\"{3}\" ", ProductID, VariantID, src, AppLogic.GetString("AppConfig.WishButtonPrompt", SkinID, LocaleSetting));

            //        var mouseOverImage = AppLogic.AppConfig("AddToCart.AddToWishButton_MouseOver");
            //        if (!string.IsNullOrEmpty(mouseOverImage))
            //        {
            //            var mouseOverSrc = AppLogic.SkinImage(mouseOverImage);
            //            tmpS.AppendFormat("  onmouseover=\"this.src = '{0}'\" ", mouseOverSrc);

            //            // attach the mouseout event automatically to switch back to the original image
            //            tmpS.AppendFormat("  onmouseout=\"this.src = '{0}'\" ", src);
            //        }

            //        var mouseDownImage = AppLogic.AppConfig("AddToCart.AddToWishButton_MouseDown");
            //        if (!string.IsNullOrEmpty(mouseDownImage))
            //        {
            //            var mouseDownImageSrc = AppLogic.SkinImage(mouseDownImage);
            //            tmpS.AppendFormat("  onmousedown=\"this.src = '{0}'\" ", mouseDownImageSrc);
            //        }
            //    }
            //    else
            //    {
            //        // Render normal html button
            //        tmpS.AppendFormat("<input type=\"button\" id=\"AddToWishButton_{0}_{1}\" name=\"AddToWishButton_{0}_{1}\" class=\"button add-to-wishlist-button\" value=\"" + AppLogic.GetString("AppConfig.WishButtonPrompt", SkinID, LocaleSetting) + "\" >", ProductID, VariantID);
            //    }
            //}

            //if (AppLogic.AppConfigBool("ShowGiftRegistryButtons") && showGiftRegistryButton && !IsRecurring && !IsDownload)
            //{
            //    showAddGiftRegistryButton = true;

            //    tmpS.AppendFormat(" ");

            //    if (AppLogic.AppConfigBool("AddToCart.UseImageButton") && AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton") != "")
            //    {
            //        // render image button
            //        var src = AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton"));
            //        tmpS.AppendFormat("<input type=\"image\" id=\"AddToGiftButton_{0}_{1}\" name=\"AddToGiftButton_{0}_{1}\" class=\"button add-to-registry-button\" src=\"{2}\" alt=\"{3}\" ", ProductID, VariantID, src, AppLogic.GetString("AppConfig.GiftButtonPrompt", SkinID, LocaleSetting));

            //        var mouseOverImage = AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton_MouseOver");
            //        if (!string.IsNullOrEmpty(mouseOverImage))
            //        {
            //            var mouseOverSrc = AppLogic.SkinImage(mouseOverImage);
            //            tmpS.AppendFormat("  onmouseover=\"this.src = '{0}'\" ", mouseOverSrc);

            //            // attach the mouseout event automatically to switch back to the original image
            //            tmpS.AppendFormat("  onmouseout=\"this.src = '{0}'\" ", src);
            //        }

            //        var mouseDownImage = AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton_MouseDown");
            //        if (!string.IsNullOrEmpty(mouseDownImage))
            //        {
            //            var mouseDownImageSrc = AppLogic.SkinImage(mouseDownImage);
            //            tmpS.AppendFormat("  onmousedown=\"this.src = '{0}'\" ", mouseDownImageSrc);
            //        }
            //    }
            //    else
            //    {
            //        // render normal html button
            //        tmpS.AppendFormat("<input type=\"button\" id=\"AddToGiftButton_{0}_{1}\" class=\"button add-to-registry-button\" value=\"" + AppLogic.GetString("AppConfig.GiftButtonPrompt", SkinID, LocaleSetting) + "\" >", ProductID, VariantID);
            //    }
            //}                       

            if (AppLogic.GetNextVariant(ProductID, VariantID) == VariantID) // single variant product
            {
                PayPalAd productPageAd = new PayPalAd(PayPalAd.TargetPage.Product);
                if (productPageAd.Show)
                    tmpS.AppendLine(productPageAd.ImageScript);
            }

            tmpS.AppendLine();
            // For the move to using masterpages, the  primary prerequisite was to have the form on the master template
            // we therefore stripped this extension of it's own form tag
            // we resort to using javascript to do that
            tmpS.Append("<script type=\"text/javascript\" language=\"Javascript\" >\n");
            // attach a delegate function that get's called on browser DOM load            
            tmpS.Append("    $window_addLoad(function(){ \n");


            tmpS.Append("Type.registerNamespace('aspdnsf');");
            tmpS.AppendLine();
            tmpS.Append("Type.registerNamespace('aspdnsf.Controls');");

            tmpS.AppendLine();

            // also defined here is the delAtcx function
            // which is used on the addto-x button's onclick handler
            // this function will then call the __doPostBack function which is an asp.net standard javascript function
            // which has the following signature:
            //          __doPostBack(eventTarget, eventArgument) 
            // To simulate same-page postback through javascript
            // we use the "AddToCart" as the eventTarget, which we'll try to interpret as a command later during postback
            // and the the cartType, productid and variantid concantenated as the eventArgument
            // this cause the current page(showproduct.aspx) to postback to itself
            // which we will then try to interpret as addtocart postback 
            // via the __EVENTTARGET and __EVENTARGUMENT form values....


            tmpS.AppendFormat("        var ctrl_{0} = new aspdnsf.Controls.AddToCartForm({1}, {2}); \n", FormName, ProductID, VariantID);
            tmpS.AppendFormat("        ctrl_{0}.setValidationRoutine( function(){{ return {0}_Validator(theForm) }} );\n", FormName);

            bool useAjaxBehavior = AppLogic.AppConfigBool("Minicart.UseAjaxAddToCart") && !ThisCustomer.IsImpersonated && !AppLogic.CheckForMobileRequest();
            if (forKit || forPack)
            {
                // ajax addtocart is not supported for kits and packs
                useAjaxBehavior = false;
            }

            tmpS.AppendFormat("        ctrl_{0}.setUseAjaxBehavior({1});\n", FormName, useAjaxBehavior.ToString().ToLowerInvariant());

            if (showAddToCartButton)
            {
                tmpS.AppendFormat("        ctrl_{0}.registerButton('AddToCartButton_{1}_{2}', {3});\n", FormName, ProductID, VariantID, (int)CartTypeEnum.ShoppingCart);
            }
            if (showAddToWishListButton)
            {
                tmpS.AppendFormat("        ctrl_{0}.registerButton('AddToWishButton_{1}_{2}', {3});\n", FormName, ProductID, VariantID, (int)CartTypeEnum.WishCart);
            }
            if (showAddGiftRegistryButton)
            {
                tmpS.AppendFormat("        ctrl_{0}.registerButton('AddToGiftButton_{1}_{2}', {3});\n", FormName, ProductID, VariantID, (int)CartTypeEnum.GiftRegistryCart);
            }

            tmpS.Append("    });");
            tmpS.Append("</script>");
            tmpS.AppendLine();

            if (!AppLogic.AppConfigBool("BuySafe.DisableAddoToCartKicker") && AppLogic.GlobalConfigBool("BuySafe.Enabled") && AppLogic.GlobalConfig("BuySafe.Hash").Length != 0)
            {
                tmpS.AppendLine("<div class=\"form-text buysafe-kicker\" id=\"ProductBuySafeKicker\">");
                tmpS.AppendLine("<span id=\"buySAFE_Kicker\" name=\"buySAFE_Kicker\" type=\"" + AppLogic.AppConfig("BuySafe.KickerType") + "\"></span>");
                tmpS.AppendLine("</div>");
            }
            tmpS.Append("</div>");

            return tmpS.ToString();
        }
        /// <summary>
        /// Gets the add to cart form.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="forPack">if set to <c>true</c> [for pack].</param>
        /// <param name="forKit">if set to <c>true</c> [for kit].</param>
        /// <param name="showWishListButton">if set to <c>true</c> [show wish list button].</param>
        /// <param name="showGiftRegistryButton">if set to <c>true</c> [show gift registry button].</param>
        /// <param name="ProductID">The product ID.</param>
        /// <param name="VariantID">The variant ID.</param>
        /// <param name="SkinID">The skin ID.</param>
        /// <param name="DisplayFormat">The display format.</param>
        /// <param name="LocaleSetting">The locale setting.</param>
        /// <param name="ColorChangeProductImage">if set to <c>true</c> [color change product image].</param>
        /// <param name="VariantStyle">The variant style.</param>
        /// <param name="insertItemsToKitCart">if set to <c>true</c> [insert items to kit cart].</param>
        /// <param name="breakAttributes">if set to <c>true</c> [break attributes].</param>
        /// <param name="formPrefix">The form prefix.</param>
        /// <returns></returns>
        static public String GetAddToCartForm(Customer ThisCustomer, bool forPack, bool forKit, bool showWishListButton, bool showGiftRegistryButton, int ProductID, int VariantID, int SkinID, int DisplayFormat, String LocaleSetting, bool ColorChangeProductImage, VariantStyleEnum VariantStyle, bool insertItemsToKitCart, bool breakAttributes, string formPrefix)
        {
            int CartRecID = CommonLogic.QueryStringUSInt("CartRecID");
            String TextOptionForEdit = String.Empty;
            int QuantityForEdit = 0;
            String ChosenSizeForEdit = String.Empty;
            String ChosenColorForEdit = String.Empty;
            Decimal ProductPriceForEdit = System.Decimal.Zero;
            if (CartRecID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rscart = DB.GetRS("select * from ShoppingCart where ShoppingCartRecID=" + CartRecID.ToString() + " and VariantID = " + VariantID.ToString(), con))
                    {
                        if (rscart.Read())
                        {
                            TextOptionForEdit = DB.RSField(rscart, "TextOption");
                            QuantityForEdit = DB.RSFieldInt(rscart, "Quantity");
                            ChosenSizeForEdit = DB.RSFieldByLocale(rscart, "ChosenSize", Localization.GetDefaultLocale());
                            ChosenColorForEdit = DB.RSFieldByLocale(rscart, "ChosenColor", Localization.GetDefaultLocale());
                            ProductPriceForEdit = DB.RSFieldDecimal(rscart, "ProductPrice");
                        }
                    }
                }
            }

            string ProductSku = string.Empty;
            string SkuSuffix = string.Empty;
            bool CustomerEntersPrice = false;
            String CustomerEntersPricePrompt = string.Empty;
            String RestrictedQuantities = string.Empty;
            int MinimumQuantity = 0;
            bool TrackInventoryBySizeAndColor = false;
            bool TrackInventoryByColor = false;
            String ColorsMaster = string.Empty;
            String ColorsDisplay = string.Empty;
            String ColorSKUModifiers = string.Empty;
            String ColorOptionPrompt = string.Empty;
            bool TrackInventoryBySize = false;
            String SizesMaster = string.Empty;
            String SizesDisplay = string.Empty;
            String SizeSKUModifiers = string.Empty;
            String SizeOptionPrompt = string.Empty;
            bool HasSizePriceModifiers = false;
            bool HasColorPriceModifiers = false;
            string boardSuffix = string.Empty;
            String TextOptionPrompt = string.Empty;
            int TextOptionMaxLength = 0;
            int TaxClassID = 0;
            String SKU = string.Empty;
            String ExtensionData = string.Empty;
            String ExtensionData2 = string.Empty;
            bool IsRecurring = false;
            bool IsDownload = false;
            bool ShowTextOption = false;
            bool RequiresTextOption = false;
            String FormName = formPrefix + "AddToCartForm_" + ProductID.ToString() + "_" + VariantID.ToString();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select PV.*,P.*, pv.Sizes MLSizes, pv.Colors MLColors, p.ColorOptionPrompt MLColorOptionPrompt, p.SizeOptionPrompt MLSizeOptionPrompt, p.TextOptionPrompt MLTextOptionPrompt, p.TextOptionMaxLength from Product P  with (NOLOCK)  left outer join productvariant PV  with (NOLOCK)  on p.productid=pv.productid where PV.ProductID=P.ProductID and pv.VariantID=" + VariantID.ToString(), con))
                {
                    if (!rs.Read())
                    {
                        rs.Close();
                        return String.Empty;
                    }

                    if (!AppLogic.AppConfigBool("ShowBuyButtons") || !DB.RSFieldBool(rs, "showbuybutton") || AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
                    {
                        return string.Empty;
                    }
                    if (DB.RSFieldBool(rs, "IsCallToOrder"))
                    {
                        return "<div class='page-row call-to-order' id='" + FormName + "'><font class=\"CallToOrder\">" + AppLogic.GetString("common.cs.67", SkinID, LocaleSetting) + "</font></div>";
                    }

                    ProductSku = DB.RSField(rs, "SKU").Trim();
                    SkuSuffix = DB.RSField(rs, "SkuSuffix").Trim();
                    CustomerEntersPrice = DB.RSFieldBool(rs, "CustomerEntersPrice");
                    CustomerEntersPricePrompt = DB.RSFieldByLocale(rs, "CustomerEntersPricePrompt", ThisCustomer.LocaleSetting);
                    RestrictedQuantities = DB.RSField(rs, "RestrictedQuantities");
                    MinimumQuantity = DB.RSFieldInt(rs, "MinimumQuantity");

                    TrackInventoryBySizeAndColor = DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor");

                    TrackInventoryByColor = DB.RSFieldBool(rs, "TrackInventoryByColor");
                    ColorsMaster = DB.RSFieldByLocale(rs, "MLColors", Localization.GetDefaultLocale()).Trim();
                    ColorsDisplay = DB.RSFieldByLocale(rs, "MLColors", LocaleSetting).Trim();
                    ColorSKUModifiers = DB.RSField(rs, "ColorSKUModifiers").Trim();
                    ColorOptionPrompt = DB.RSFieldByLocale(rs, "MLColorOptionPrompt", LocaleSetting).Trim();

                    TrackInventoryBySize = DB.RSFieldBool(rs, "TrackInventoryBySize");
                    SizesMaster = DB.RSFieldByLocale(rs, "MLSizes", Localization.GetDefaultLocale()).Trim();
                    SizesDisplay = DB.RSFieldByLocale(rs, "MLSizes", LocaleSetting).Trim();
                    SizeSKUModifiers = DB.RSField(rs, "SizeSKUModifiers").Trim();
                    SizeOptionPrompt = DB.RSFieldByLocale(rs, "MLSizeOptionPrompt", LocaleSetting).Trim();

                    if (SizesDisplay.Length == 0)
                    {
                        SizesDisplay = SizesMaster;
                    }

                    if (ColorsDisplay.Length == 0)
                    {
                        ColorsDisplay = ColorsMaster;
                    }

                    HasSizePriceModifiers = SizesMaster.IndexOf('[') != -1;
                    HasColorPriceModifiers = ColorsMaster.IndexOf('[') != -1;
                    boardSuffix = string.Format("_{0}_{1}", ProductID.ToString(), VariantID.ToString());

                    TextOptionPrompt = DB.RSFieldByLocale(rs, "MLTextOptionPrompt", LocaleSetting).Trim();
                    TextOptionMaxLength = DB.RSFieldInt(rs, "TextOptionMaxLength");

                    TaxClassID = DB.RSFieldInt(rs, "TaxClassID");

                    SKU = DB.RSField(rs, "SKU");

                    ExtensionData = DB.RSField(rs, "ExtensionData");
                    ExtensionData2 = DB.RSField(rs, "ExtensionData2");

                    IsRecurring = DB.RSFieldBool(rs, "IsRecurring");
                    IsDownload = DB.RSFieldBool(rs, "IsDownload");

                    if (VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
                    {
                        XmlDocument d = new XmlDocument();
                        using (IDataReader rsx = DB.GetRS("select ExtensionData,ExtensionData2 from Product with (NOLOCK) where ProductID=" + DB.RSFieldInt(rs, "ProductID").ToString(), con))
                        {
                            ColorSKUModifiers = String.Empty;
                            SizeSKUModifiers = String.Empty;

                            d.LoadXml(DB.RSField(rsx, "ExtensionData2"));
                        }

                        foreach (XmlNode n in d.SelectNodes("GroupAttributes/GroupAttribute"))
                        {
                            String AttrName = XmlCommon.XmlAttribute(n, "Name");
                            if (AttrName.Equals("ATTR1", StringComparison.InvariantCultureIgnoreCase))
                            {
                                SizeOptionPrompt = XmlCommon.XmlAttribute(n, "Prompt");
                                String Separator = String.Empty;

                                foreach (XmlNode nn in n.SelectNodes("Values/Value"))
                                {
                                    SizesMaster += Separator;
                                    SizesMaster += nn.InnerText;
                                    Separator = ",";
                                }
                            }
                            else if (AttrName.Equals("ATTR2", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ColorOptionPrompt = XmlCommon.XmlAttribute(n, "Prompt");
                                String Separator = String.Empty;
                                foreach (XmlNode nn in n.SelectNodes("Values/Value"))
                                {
                                    ColorsMaster += Separator;
                                    ColorsMaster += nn.InnerText;
                                    Separator = ",";
                                }
                            }
                        }
                        SizesDisplay = SizesMaster;
                        ColorsDisplay = ColorsMaster;
                    }

                    if (TextOptionMaxLength == 0)
                    {
                        TextOptionMaxLength = 50;
                    }
                    if (ColorOptionPrompt.Length == 0)
                    {
                        ColorOptionPrompt = AppLogic.GetString("AppConfig.ColorOptionPrompt", SkinID, LocaleSetting);
                    }
                    if (ColorOptionPrompt.Length == 0)
                    {
                        ColorOptionPrompt = AppLogic.GetString("common.cs.68", SkinID, LocaleSetting);
                    }
                    if (SizeOptionPrompt.Length == 0)
                    {
                        SizeOptionPrompt = AppLogic.GetString("AppConfig.SizeOptionPrompt", SkinID, LocaleSetting);
                    }
                    if (SizeOptionPrompt.Length == 0)
                    {
                        SizeOptionPrompt = AppLogic.GetString("common.cs.69", SkinID, LocaleSetting);
                    }
                    if (TextOptionPrompt.Length == 0)
                    {
                        TextOptionPrompt = AppLogic.AppConfig("TextOptionPrompt");
                    }
                    if (TextOptionPrompt.Length == 0)
                    {
                        TextOptionPrompt = AppLogic.GetString("common.cs.70", SkinID, LocaleSetting);
                    }
                    ShowTextOption = false;
                    RequiresTextOption = DB.RSFieldBool(rs, "RequiresTextOption");
                    if (RequiresTextOption || DB.RSFieldByLocale(rs, "TextOptionPrompt", LocaleSetting).Trim().Length != 0)
                    {
                        ShowTextOption = true;
                    }
                }
            }

            if (ColorChangeProductImage)
            {
                ProductImageGallery ig = new ProductImageGallery(ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, SKU);
                ColorChangeProductImage = !ig.IsEmpty();
            }



            bool ProtectInventory = AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand");
            int ProtectInventoryMinQuantity = AppLogic.AppConfigUSInt("Inventory.MinQuantity"); // if qty is below this, addtocart will not allow it
            String InventoryControlList = String.Empty;
            if (ProtectInventory)
            {
                InventoryControlList = AppLogic.GetInventoryList(ProductID, VariantID, TrackInventoryByColor, TrackInventoryBySize, ProductSku, SkuSuffix, SizesMaster, ColorsMaster);
            }

            int inv = AppLogic.GetInventory(ProductID, VariantID, "", "");

            StringBuilder tmpS = new StringBuilder(10000);
            tmpS.Append("<div class='form add-to-cart-form' id='" + FormName + "'>\n");
            tmpS.Append("<script type=\"text/javascript\" Language=\"JavaScript\">\n<!--\n");
            tmpS.Append("var VariantMinimumQty_" + "1".ToString() + "_" + "1".ToString() + " = " + MinimumQuantity.ToString() + ";\n");
            tmpS.Append("var SelectedVariantInventory_" + "1".ToString() + "_" + "1".ToString() + " = " + inv.ToString() + ";\n");


            if (ProtectInventory && InventoryControlList.Length != 0)
            {
                bool first = true;
                foreach (String s in InventoryControlList.Split('|'))
                {
                    if (first)
                    {
                        tmpS.Append("var board" + boardSuffix + " = new Array(");
                    }
                    else
                    {
                        tmpS.Append(",");
                    }
                    String[] ivals = s.Split(',');
                    tmpS.Append("new Array('" + ivals[0].Replace("'", "").Trim() + "','" + ivals[1].Replace("'", "").Trim() + "','" + ivals[2].Replace("'", "").Trim() + "')");
                    first = false;
                }
                tmpS.Append(");\n");
            }

            tmpS.Append("function " + FormName + "_Validator(theForm)\n");
            tmpS.Append("	{\n");
            tmpS.Append("	submitonce(theForm);\n");

            if (!CustomerEntersPrice && (AppLogic.AppConfigBool("ShowQuantityOnProductPage") && !forKit) || (!AppLogic.AppConfigBool("HideKitQuantity") && forKit))
            {
                tmpS.Append("	if ((theForm.Quantity.value*1) < 1)\n"); // convert form val to integer
                tmpS.Append("	{\n");
                tmpS.Append("		alert(\"" + AppLogic.GetString("common.cs.84", SkinID, LocaleSetting) + "\");\n");
                tmpS.Append("		theForm.Quantity.focus();\n");
                tmpS.Append("		submitenabled(theForm);\n");
                tmpS.Append("		return (false);\n");
                tmpS.Append("    }\n");

                if (RestrictedQuantities.Length == 0 && MinimumQuantity != 0)
                {
                    tmpS.Append("	if ((theForm.Quantity.value*1) < VariantMinimumQty_" + ProductID.ToString() + "_" + VariantID.ToString() + ")\n"); // convert form val to integer
                    tmpS.Append("	{\n");
                    tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.85", SkinID, LocaleSetting), "\"+VariantMinimumQty_" + ProductID.ToString() + "_" + VariantID.ToString() + " + \"") + "\");\n");
                    tmpS.Append("		theForm.Quantity.focus();\n");
                    tmpS.Append("		submitenabled(theForm);\n");
                    tmpS.Append("		return (false);\n");
                    tmpS.Append("    }\n");
                }
            }

            if (SizesMaster.Length != 0 && !AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
            {
                tmpS.Append("	if (theForm.Size.selectedIndex < 1)\n");
                tmpS.Append("	{\n");
                tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.71", SkinID, LocaleSetting), SizeOptionPrompt.ToLowerInvariant()) + "\");\n");
                tmpS.Append("		theForm.Size.focus();\n");
                tmpS.Append("		submitenabled(theForm);\n");
                tmpS.Append("		return (false);\n");
                tmpS.Append("    }\n");
            }
            if (ColorsMaster.Length != 0 && !AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
            {
                tmpS.Append("	if (theForm.Color.selectedIndex < 1)\n");
                tmpS.Append("	{\n");
                tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.71", SkinID, LocaleSetting), ColorOptionPrompt.ToLowerInvariant()) + "\");\n");
                tmpS.Append("		theForm.Color.focus();\n");
                tmpS.Append("		submitenabled(theForm);\n");
                tmpS.Append("		return (false);\n");
                tmpS.Append("    }\n");
            }
            if (RequiresTextOption)
            {
                tmpS.Append("	if (theForm.TextOption.value.length == 0)\n");
                tmpS.Append("	{\n");
                tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.73", SkinID, LocaleSetting), TextOptionPrompt) + "\");\n");
                tmpS.Append("		theForm.TextOption.focus();\n");
                tmpS.Append("		submitenabled(theForm);\n");
                tmpS.Append("		return (false);\n");
                tmpS.Append("    }\n");
            }

            if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes && ProtectInventory && AppLogic.AppConfigBool("ShowQuantityOnProductPage"))
            {
                if (!TrackInventoryBySizeAndColor)
                {
                    tmpS.Append("	if (theForm.Quantity.value > SelectedVariantInventory_" + "1".ToString() + "_" + "1".ToString() + ")\n");
                    tmpS.Append("	{\n");
                    tmpS.Append("		alert(\"" + String.Format(AppLogic.GetString("common.cs.74", SkinID, LocaleSetting), "\"+SelectedVariantInventory_" + "1".ToString() + "_" + "1".ToString() + "+\"") + "\");\n");
                    tmpS.Append("		theForm.Quantity.value = SelectedVariantInventory_" + "1".ToString() + "_" + "1".ToString() + ";\n");
                    tmpS.Append("		theForm.Quantity.focus();\n");
                    tmpS.Append("		submitenabled(theForm);\n");
                    tmpS.Append("		return (false);\n");
                    tmpS.Append("    }\n");
                }
                else
                {
                    if (SizesMaster.Length != 0)
                    {
                        tmpS.Append("var sel_size = theForm.Size[theForm.Size.selectedIndex].value;\n");
                        tmpS.Append("sel_size = sel_size.substring(0,sel_size.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n");
                    }
                    else
                    {
                        tmpS.Append("var sel_size = '';\n");
                    }

                    if (ColorsMaster.Length != 0)
                    {
                        tmpS.Append("var sel_color = theForm.Color[theForm.Color.selectedIndex].value;\n");
                        tmpS.Append("sel_color = sel_color.substring(0,sel_color.indexOf(',')).replace(new RegExp(\"'\", 'gi'), '');\n");
                    }
                    else
                    {
                        tmpS.Append("var sel_color = '';\n");
                    }

                    tmpS.Append("if(typeof(sel_size) == 'undefined') sel_size = '';\n");
                    tmpS.Append("if(typeof(sel_color) == 'undefined') sel_color = '';\n");

                    // clean price delta options if any, so match will work on inventory control list:
                    tmpS.Append("var j = sel_size.indexOf(\"[\");\n");
                    tmpS.Append("if(j != -1)\n");
                    tmpS.Append("{\n");
                    tmpS.Append("	sel_size = Trim(sel_size.substring(0,j));\n");
                    tmpS.Append("}\n");

                    tmpS.Append("var i = sel_color.indexOf(\"[\");\n");
                    tmpS.Append("if(i != -1)\n");
                    tmpS.Append("{\n");
                    tmpS.Append("	sel_color = Trim(sel_color.substring(0,i));\n");
                    tmpS.Append("}\n");

                    if (TrackInventoryBySize)
                    {
                        tmpS.Append("var sel_size_master = sel_size;\n");
                    }
                    else
                    {
                        tmpS.Append("var sel_size_master = '';\n");
                    }

                    if (TrackInventoryByColor)
                    {
                        tmpS.Append("var sel_color_master = sel_color;\n");
                    }
                    else
                    {
                        tmpS.Append("var sel_color_master = '';\n");
                    }


                    tmpS.Append("var sel_qty = theForm.Quantity.value;\n");
                    tmpS.Append("var sizecolorfound = 0;\n");
                    tmpS.Append("for(i = 0; i < board" + boardSuffix + ".length; i++)\n");
                    tmpS.Append("{\n");
                    tmpS.Append("	if(board" + boardSuffix + "[i][1] == sel_size_master && board" + boardSuffix + "[i][0] == sel_color_master)\n");
                    tmpS.Append("	{\n");
                    tmpS.Append("		sizecolorfound = 1;\n");
                    tmpS.Append("		if(parseInt(sel_qty) > parseInt(board" + boardSuffix + "[i][2]))\n");
                    tmpS.Append("		{\n");
                    tmpS.Append("			if(parseInt(board" + boardSuffix + "[i][2]) == 0)\n");
                    tmpS.Append("			{\n");
                    tmpS.Append("               if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("               if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("				alert('" + String.Format(AppLogic.GetString("common.cs.75", SkinID, LocaleSetting), ColorOptionPrompt, SizeOptionPrompt, ColorOptionPrompt, SizeOptionPrompt).Replace(@"\\n", @"\n") + "');\n");
                    tmpS.Append("			}\n");
                    tmpS.Append("			else\n");
                    tmpS.Append("			{\n");
                    tmpS.Append("               if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("               if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("				alert('" + String.Format(AppLogic.GetString("common.cs.76", SkinID, LocaleSetting), ColorOptionPrompt, SizeOptionPrompt, ColorOptionPrompt, SizeOptionPrompt).Replace(@"\\n", @"\n").Replace("board", "board" + boardSuffix) + "');\n");
                    tmpS.Append("			}\n");
                    tmpS.Append("			submitenabled(theForm);\n");
                    tmpS.Append("			return (false);\n");
                    tmpS.Append("		}\n");
                    tmpS.Append("	}\n");
                    tmpS.Append("}\n");
                    tmpS.Append("if(sizecolorfound == 0)\n");
                    tmpS.Append("{\n");
                    tmpS.Append("   if(sel_color == '') sel_color = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("   if(sel_size == '') sel_size = '" + AppLogic.GetString("order.cs.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "';\n");
                    tmpS.Append("	alert('" + AppLogic.GetString("shoppingcart.cs.115", ThisCustomer.SkinID, ThisCustomer.LocaleSetting).Replace("'", "").Replace("{0}", "[' + sel_color + ']").Replace("{1}", "[' + sel_size + ']") + "');\n");
                    tmpS.Append("	submitenabled(theForm);\n");
                    tmpS.Append("	return (false);\n");
                    tmpS.Append("}\n");
                    tmpS.Append("submitenabled(theForm);\n");
                }
            }
            tmpS.Append("	submitenabled(theForm);\n");
            tmpS.Append("	return (true);\n");
            tmpS.Append("	}\n//-->\n");
            tmpS.Append("</script>\n");
            String Action = String.Empty;

            Action = "addtocart.aspx?returnurl=" + Security.UrlEncode(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));

            bool isEditKit = false;

            if (!String.IsNullOrEmpty(CommonLogic.QueryStringCanBeDangerousContent("edit")))
            {
                isEditKit = true;
            }

            tmpS.Append("<form method=\"POST\" name=\"" + FormName + "\" id=\"" + FormName + "\" action=\"" + Action + "\" onsubmit=\"return validateForm(this) && " + FormName + "_Validator(this)\" >");
            tmpS.Append("<input name=\"VariantStyle\" id=\"VariantStyle\" type=\"hidden\" value=\"" + ((int)VariantStyle).ToString() + "\">");

            CartTypeEnum thisCartType = CartTypeEnum.ShoppingCart;

            if (isEditKit)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader reader = DB.GetRS(string.Format("SELECT CartType FROM ShoppingCart WHERE ShoppingCartRecID = {0}", CartRecID), con))
                    {
                        if (reader.Read())
                        {
                            thisCartType = (CartTypeEnum)DB.RSFieldInt(reader, "CartType");

                            tmpS.AppendFormat("<input name=\"IsWishList\" id=\"IsWishList\" type=\"hidden\" value=\"{0}\">", CommonLogic.IIF(thisCartType == CartTypeEnum.WishCart, "1", "0"));
                            tmpS.AppendFormat("<input name=\"IsGiftRegistry\" id=\"IsGiftRegistry\" type=\"hidden\" value=\"{0}\">", CommonLogic.IIF(thisCartType == CartTypeEnum.GiftRegistryCart, "1", "0"));
                        }
                    }
                }
            }
            else
            {
                tmpS.Append("<input name=\"IsWishList\" id=\"IsWishList\" type=\"hidden\" value=\"0\">");
                tmpS.Append("<input name=\"IsGiftRegistry\" id=\"IsGiftRegistry\" type=\"hidden\" value=\"0\">");
            }

            tmpS.Append("<input type=\"hidden\" name=\"UpsellProducts\" id=\"UpsellProducts\" value=\"\">\n");
            tmpS.Append("<input type=\"hidden\" name=\"CartRecID\" id=\"CartRecID\" value=\"" + CartRecID.ToString() + "\">\n");
            tmpS.Append("<input type=\"hidden\" name=\"ProductID\" id=\"ProductID\" value=\"" + ProductID.ToString() + "\">\n");
            if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes || VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
            {
                tmpS.Append("<input type=\"hidden\" name=\"VariantID\" id=\"VariantID\" value=\"" + VariantID.ToString() + "\">\n");
            }

            //our new kit package display does the price computation in the clientside
            //so this is how we'll insert our items to the kitcart
            //anyway we're sure that our product is a kit
            //since insertItemsToKitCart will only be true from 
            //from our new kit product extension routine
            if (insertItemsToKitCart)
            {
                tmpS.Append("<input type=\"hidden\" name=\"KitProductID\" id=\"KitProductID\" value=\"" + ProductID.ToString() + "\">\n");
                tmpS.Append("<input type=\"hidden\" name=\"KitItems\" id=\"KitItems\" class=\"KitItems\" value=\"\">");
                tmpS.AppendLine();


                // check if we are in edit mode for this kit item
                if (isEditKit)
                {
                    tmpS.AppendFormat("<input name=\"IsEditKit\" type=\"hidden\" value=\"{0}\">", (true).ToString().ToLower());
                    tmpS.AppendLine();
                }
            }


            if (forPack)
            {
                tmpS.Append("<input type=\"hidden\" name=\"IsSubmit\" id=\"IsSubmit\" value=\"" + AppLogic.GetString("common.cs.77", SkinID, LocaleSetting) + "\">\n");
                tmpS.Append("<input type=\"hidden\" name=\"ProductTypeID\" id=\"ProductTypeID\" value=\"0\">\n");
                if (ColorsMaster.Length == 0)
                {
                    tmpS.Append("<input type=\"hidden\" name=\"Color\" id=\"Color\" value=\"\">\n");
                }
                if (SizesMaster.Length == 0)
                {
                    tmpS.Append("<input type=\"hidden\" name=\"Size\" id=\"Size\" value=\"\">\n");
                }
            }
            if (AppLogic.AppConfigBool("ShowPreviousPurchase") && AppLogic.Owns(ProductID, ThisCustomer.CustomerID))
            {
                tmpS.Append("<div class=\"form-text previous-purchases\">" + AppLogic.GetString("common.cs.86", SkinID, LocaleSetting) + "</div>");
            }

            if (CustomerEntersPrice)
            {
                tmpS.Append("<div class=\"form-group customer-enters-price\">");
                tmpS.Append("<label>");
                tmpS.Append(CustomerEntersPricePrompt);
                tmpS.Append("</label>");
                tmpS.Append(" <input maxLength=\"10\" class=\"form-control price-field\" name=\"Price\" value=\"" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ProductPriceForEdit) + "\">");
                tmpS.Append("<input type=\"hidden\" name=\"Price_vldt\" value=\"[req][number][blankalert=" + AppLogic.GetString("shoppingcart.cs.113", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("shoppingcart.cs.114", SkinID, LocaleSetting) + "]\">\n");
                tmpS.Append("</div>");
            }
            tmpS.Append("<div class=\"form-group add-to-cart-group\">");

            tmpS.Append("	<span class=\"add-to-cart-quantity\">");
            if (!CustomerEntersPrice && (AppLogic.AppConfigBool("ShowQuantityOnProductPage") && !forKit) || (!AppLogic.AppConfigBool("HideKitQuantity") && forKit))
            {
                if (RestrictedQuantities.Length == 0)
                {
                    int InitialQ = 1;
                    if (MinimumQuantity > 0)
                    {
                        InitialQ = MinimumQuantity;
                    }
                    else if (AppLogic.AppConfig("DefaultAddToCartQuantity").Length != 0)
                    {
                        InitialQ = AppLogic.AppConfigUSInt("DefaultAddToCartQuantity");
                    }
                    if (QuantityForEdit != 0)
                    {
                        InitialQ = QuantityForEdit;
                    }
                    tmpS.Append("<label>" + AppLogic.GetString("common.cs.78", SkinID, LocaleSetting) + "</small> <input type=\"text\" value=\"" + InitialQ.ToString() + "\" name=\"Quantity\" id=\"Quantity\" onChange=\"if(typeof(getShipping) == 'function'){getShipping()}\" onKeyUp=\"if(typeof(getShipping) == 'function'){getShipping()}\" class=\"form-control quantity-field\" maxlength=\"4\">");
                    tmpS.Append("<input name=\"Quantity_vldt\" type=\"hidden\" value=\"[req][integer][number][blankalert=" + AppLogic.GetString("common.cs.79", SkinID, LocaleSetting) + "][invalidalert=" + AppLogic.GetString("common.cs.80", SkinID, LocaleSetting) + "]\">");
                }
                else
                {
                    tmpS.Append("<label for=\"Quantity\">" + AppLogic.GetString("common.cs.78", SkinID, LocaleSetting) + "</label>");
                    tmpS.Append(" <select class=\"form-control quantity-select\" name=\"Quantity\" id=\"Quantity\" onChange=\"if(typeof(getShipping) == 'function'){getShipping()};\" size=\"1\">");
                    foreach (String s in RestrictedQuantities.Split(','))
                    {
                        if (s.Trim().Length != 0)
                        {
                            int Q = Localization.ParseUSInt(s.Trim());
                            tmpS.Append("<option value=\"" + Q.ToString() + "\" " + CommonLogic.IIF(QuantityForEdit == Q, " selected ", "") + ">" + Q.ToString() + "</option>");
                        }
                    }
                    tmpS.Append("</select>");
                }
            }
            else
            {
                tmpS.Append("<input name=\"Quantity\" id=\"Quantity\" type=\"hidden\" value=\"1\">");
            }
            tmpS.Append("	</span>");
            Decimal M = 1.0M;
            String MM = ThisCustomer.CurrencyString(M).Substring(0, 1); // get currency symbol
            if (CommonLogic.IsInteger(MM))
            {
                MM = String.Empty; // something international happened, so just leave empty, we only want currency symbol, not any digits
            }
            if (VariantStyle == VariantStyleEnum.RegularVariantsWithAttributes || VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
            {
                tmpS.Append("	<span class=\"add-to-cart-selectors\">");
                if (SizesMaster.Length != 0)
                {
                    String[] SizesMasterSplit = SizesMaster.Split(',');
                    String[] SizesDisplaySplit = SizesDisplay.Split(',');
                    String[] SizeSKUsSplit = SizeSKUModifiers.Split(',');
                    tmpS.Append(" <select name=\"Size\" id=\"Size\" class=\"select-list\" " + CommonLogic.IIF(HasSizePriceModifiers, "onChange=\"if(typeof(getPricing) == 'function'){getPricing(" + ProductID.ToString() + "," + VariantID.ToString() + ")}\"", "") + ">\n");
                    if (!AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
                    {
                        tmpS.Append("<option value=\"-,-\">" + SizeOptionPrompt + "</option>\n");
                    }
                    for (int i = SizesMasterSplit.GetLowerBound(0); i <= SizesMasterSplit.GetUpperBound(0); i++)
                    {
                        string SizeString = SizesDisplaySplit[i].Trim();
                        string SizeStringValue = SizeString;
                        String Modifier = String.Empty;
                        try
                        {
                            Modifier = SizeSKUsSplit[i];
                        }
                        catch
                        { }
                        decimal SizePriceDeltaValue = AppLogic.GetColorAndSizePriceDelta("", SizesMasterSplit[i].Trim(), TaxClassID, ThisCustomer, true, false);
                        decimal SizePriceDelta = AppLogic.GetColorAndSizePriceDelta("", SizesMasterSplit[i].Trim(), TaxClassID, ThisCustomer, true, true);
                        if (SizeString.IndexOf("[") != -1)
                        {
                            SizeString = SizeString.Substring(0, SizeString.IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(SizePriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(SizePriceDelta) + "]";
                            SizeStringValue = SizeString.Substring(0, SizeString.IndexOf("[")).Trim() + " [" + CommonLogic.IIF(SizePriceDeltaValue > 0, "+", "") + Localization.CurrencyStringForGatewayWithoutExchangeRate(SizePriceDeltaValue) + "]";
                        }
                        tmpS.Append("<option value=\"" + AppLogic.CleanSizeColorOption(SizesMasterSplit[i]) + "," + Modifier + "\" " + CommonLogic.IIF(ChosenSizeForEdit == SizeStringValue, " selected ", "") + ">" + HttpContext.Current.Server.HtmlDecode(SizeString) + "</option>\n");
                    }
                    tmpS.Append("</select>");
                }
                if (ColorsMaster.Length != 0)
                {
                    String[] ColorsMasterSplit = ColorsMaster.Split(',');
                    String[] ColorsDisplaySplit = ColorsDisplay.Split(',');
                    String[] ColorSKUsSplit = ColorSKUModifiers.Split(',');
                    tmpS.Append(" <select id=\"Color\" name=\"Color\" class=\"select-list\" onChange=\"" + CommonLogic.IIF(ColorChangeProductImage, "setcolorpic_" + ProductID.ToString() + "(this.value);", "") + CommonLogic.IIF(HasColorPriceModifiers, "if(typeof(getPricing) == 'function'){getPricing(" + ProductID.ToString() + "," + VariantID.ToString() + ")};", "") + "\">\n");
                    if (!AppLogic.AppConfigBool("AutoSelectFirstSizeColorOption"))
                    {
                        tmpS.Append("<option value=\"-,-\">" + ColorOptionPrompt + "</option>\n");
                    }
                    for (int i = ColorsMasterSplit.GetLowerBound(0); i <= ColorsMasterSplit.GetUpperBound(0); i++)
                    {
                        string ColorString = ColorsDisplaySplit[i].Trim();
                        string ColorStringValue = ColorString;
                        String Modifier = String.Empty;
                        try
                        {
                            Modifier = ColorSKUsSplit[i];
                        }
                        catch
                        { }
                        decimal ColorPriceDeltaValue = AppLogic.GetColorAndSizePriceDelta(ColorsMasterSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, false);
                        decimal ColorPriceDelta = AppLogic.GetColorAndSizePriceDelta(ColorsMasterSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, true);
                        if (ColorString.IndexOf("[") != -1)
                        {
                            ColorString = ColorString.Substring(0, ColorString.IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(ColorPriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(ColorPriceDelta) + "]";
                            ColorStringValue = ColorString.Substring(0, ColorString.IndexOf("[")).Trim() + " [" + CommonLogic.IIF(ColorPriceDeltaValue > 0, "+", "") + Localization.CurrencyStringForGatewayWithoutExchangeRate(ColorPriceDeltaValue) + "]";
                        }
                        tmpS.Append("<option value=\"" + AppLogic.CleanSizeColorOption(ColorsMasterSplit[i]) + "," + Modifier + "\" " + CommonLogic.IIF(ChosenColorForEdit == ColorStringValue, " selected ", "") + ">" + HttpContext.Current.Server.HtmlDecode(ColorString) + "</option>\n");
                    }
                    tmpS.Append("</select>");
                }
                tmpS.Append("	</span>");
            }

            if (RequiresTextOption || ShowTextOption)
            {
                tmpS.Append("	<span class=\"add-to-cart-textoption\">");
                if (TextOptionMaxLength < 50)
                {
                    tmpS.Append(" <label>");
                    tmpS.Append(TextOptionPrompt);
                    tmpS.Append("</label>");
                    tmpS.Append(" <input type=\"text\" class=\"form-control text-option\" maxlength=\"" + TextOptionMaxLength.ToString() + "\"  name=\"TextOption\" id=\"TextOption\" value=\"" + TextOptionForEdit + "\">");
                }
                else
                {
                    tmpS.Append(" <label>");
                    tmpS.Append(TextOptionPrompt);
                    tmpS.Append("</label>");
                    tmpS.Append(" <textarea rows=\"4\" class=\"form-control text-option\" name=\"TextOption\" id=\"TextOption\" onkeypress=\"if(this.value.length>=" + TextOptionMaxLength.ToString() + " && ((event.keyCode < 33 || event.keyCode > 40) && event.keyCode != 45 && event.keyCode != 46)) {return false;} \">" + TextOptionForEdit + "</textarea>");
                }
                tmpS.Append("	</span>");
            }
            tmpS.Append("	<span class=\"add-to-cart-buttons\">");
            if (!forPack)
            {
                string cartCaption = AppLogic.GetString("AppConfig.CartButtonPrompt", SkinID, LocaleSetting);

                if (isEditKit)
                {
                    switch (thisCartType)
                    {
                        case CartTypeEnum.ShoppingCart:
                            cartCaption = AppLogic.GetString("shoppingcart.cs.110", SkinID, LocaleSetting);
                            break;
                        case CartTypeEnum.WishCart:
                            cartCaption = AppLogic.GetString("shoppingcart.cs.108", SkinID, LocaleSetting);
                            break;
                        case CartTypeEnum.GiftRegistryCart:
                            cartCaption = AppLogic.GetString("shoppingcart.cs.109", SkinID, LocaleSetting);
                            break;
                    }

                    tmpS.Append(" <input id=\"" + FormName + "_AddToCartButton\" type=\"submit\" class=\"button call-to-action add-to-cart-button\" value=\"" + cartCaption + "\">");
                }
                else
                {
                    tmpS.Append(" <input id=\"" + FormName + "_AddToCartButton\" type=\"submit\" class=\"button call-to-action add-to-cart-button\" value=\"" + cartCaption + "\">");
                }

            }
            else
            {
                tmpS.Append(" <input type=\"submit\" class=\"button call-to-action add-to-cart-button\" value=\"" + AppLogic.GetString("AppConfig.AddToPackButtonPrompt", SkinID, LocaleSetting) + "\">");
            }
            if (AppLogic.AppConfigBool("ShowWishButtons") && showWishListButton)
            {
                if (!isEditKit)
                {
                    tmpS.Append(" <input id=\"" + FormName + "_AddToWishButton\" type=\"button\" class=\"button call-to-action add-to-wish-button\" value=\"" + AppLogic.GetString("AppConfig.WishButtonPrompt", SkinID, LocaleSetting) + "\" >");
                }
            }
            if (!isEditKit)
            {
                if (AppLogic.AppConfigBool("ShowGiftRegistryButtons") && showGiftRegistryButton && !IsRecurring && !IsDownload)
                {
                    tmpS.Append(" <input id=\"" + FormName + "_AddToGiftButton\" type=\"button\" class=\"button call-to-action add-to-registry-button\" onClick=\"document." + FormName + ".IsGiftRegistry.value='1';if(validateForm(document." + FormName + ") && " + FormName + "_Validator(document." + FormName + ")) {document." + FormName + ".submit();}\" value=\"" + AppLogic.GetString("AppConfig.GiftButtonPrompt", SkinID, LocaleSetting) + "\" >");
                }
            }
            tmpS.Append("	</span>");
            tmpS.Append("</div>");
            tmpS.Append("</form>\n");
            tmpS.Append("</div>");

            if (forKit)
            {
                StringBuilder script = new StringBuilder();

                script.AppendFormat("\n<script type=\"text/javascript\" language=\"Javascript\" >\n");
                script.AppendFormat("Event.observe(window, 'load', \n");
                script.AppendFormat("function() {{ \n");
                script.AppendFormat("    if(window.aspdnsf.Controls.KitController){{\n");
                script.AppendFormat("        var ctrl=aspdnsf.Controls.KitController.getControl({0});\n", ProductID);
                script.AppendFormat("        if(ctrl){{\n");
                script.AppendFormat("            var frm = $('{0}');\n", FormName);
                script.AppendFormat("            var handler = ctrl.getPersistCompositionHandler();\n");
                script.AppendFormat("            if(frm) Event.observe(frm, 'submit', handler);\n");

                script.AppendFormat("            var btnAddToCart = $('{0}_AddToCartButton');\n", FormName);
                script.AppendFormat("            if(btnAddToCart) btnAddToCart.onclick = ctrl.chainHandler(btnAddToCart.onclick);\n");
                script.AppendFormat("            var btnAddToWish = $('{0}_AddToWishButton');\n", FormName);
                script.AppendFormat("            if(btnAddToWish) btnAddToWish.onclick = ctrl.chainHandler(btnAddToWish.onclick);\n");
                script.AppendFormat("            var btnAddToGift = $('{0}_AddToGiftButton');\n", FormName);
                script.AppendFormat("            if(btnAddToGift) btnAddToGift.onclick = ctrl.chainHandler(btnAddToGift.onclick);\n");

                script.AppendFormat("        }}\n");
                script.AppendFormat("        else {{\n");
                script.AppendFormat("            var observer = {{ \n");
                script.AppendFormat("                notify : function(c) {{\n");
                script.AppendFormat("                          var frm = $('{0}');\n", FormName);
                script.AppendFormat("                          var handler = c.getPersistCompositionHandler();\n");
                script.AppendFormat("                          if(frm) Event.observe(frm, 'submit', handler);\n");

                script.AppendFormat("                          var btnAddToCart = $('{0}_AddToCartButton');\n", FormName);
                script.AppendFormat("                          if(btnAddToCart) btnAddToCart.onclick = c.chainHandler(btnAddToCart.onclick);\n");
                script.AppendFormat("                          var btnAddToWish = $('{0}_AddToWishButton');\n", FormName);
                script.AppendFormat("                          if(btnAddToWish) btnAddToWish.onclick = c.chainHandler(btnAddToWish.onclick);\n");
                script.AppendFormat("                          var btnAddToGift = $('{0}_AddToGiftButton');\n", FormName);
                script.AppendFormat("                          if(btnAddToGift) btnAddToGift.onclick = c.chainHandler(btnAddToGift.onclick);\n");

                script.AppendFormat("                }}\n");
                script.AppendFormat("            }}\n");
                script.AppendFormat("            aspdnsf.Controls.KitController.addObserver(observer);\n");
                script.AppendFormat("        }}\n");
                script.AppendFormat("    }}\n");
                script.AppendFormat("}}\n");
                script.AppendFormat(");\n");
                script.AppendFormat("</script>\n");

                tmpS.Append(script.ToString());
            }

            return tmpS.ToString();
        }

        // returns true if this order has any items which are download items:
        /// <summary>
        /// Determines whether the shopping cart has recurring components
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [has recurring components]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasRecurringComponents()
        {
            foreach (CartItem c in m_CartItems)
            {
                if (c.IsRecurring)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the total quantity.
        /// </summary>
        /// <returns></returns>
        public int TotalQuantity()
        {
            int sum = 0;
            foreach (CartItem c in m_CartItems)
            {
                sum += c.Quantity;
            }
            return sum;
        }

        /// <summary>
        /// Returns true if shopping cart meets minimum order amount
        /// </summary>
        /// <param name="MinOrderAmount">The min order amount.</param>
        /// <returns></returns>
        public bool MeetsMinimumOrderAmount(Decimal MinOrderAmount)
        {
            if (MinOrderAmount <= System.Decimal.Zero || IsEmpty() || IsAllSystemComponents())
            {
                return true; // disable checking for empty cart or all system products cart (those are handled separately)
            }

            Decimal SubTot = SubTotal(false, false, true, true, false, false, ThisCustomer.PrimaryShippingAddressID, true);

            return (SubTot >= MinOrderAmount);
        }

        /// <summary>
        /// Returns true if shopping cart meets minimum order quantity
        /// </summary>
        /// <param name="MinOrderQuantity">The min order quantity.</param>
        /// <returns></returns>
        public bool MeetsMinimumOrderQuantity(Decimal MinOrderQuantity)
        {
            if (MinOrderQuantity <= 0 || IsEmpty() || IsAllSystemComponents())
            {
                return true; // disable checking for empty cart or all system products cart (those are handled separately)
            }
            int N = 0;
            foreach (CartItem c in m_CartItems)
            {
                if (!c.IsSystem)
                {
                    N += c.Quantity;
                }
            }
            return (N >= MinOrderQuantity);
        }

        /// <summary>
        /// Returns true if the cart contains more line items than are allowed
        /// </summary>
        /// <param name="MaxOrderQuantity">The maximum number of non-system line items allowed in the cart.</param>
        /// <returns></returns>
        public bool ExceedsMaximumOrderQuantity(Decimal MaxOrderQuantity)
        {
            if (IsEmpty() || IsAllSystemComponents())
            {
                return false; // disable checking for empty cart or all system products cart (those are handled separately)
            }
            return (m_CartItems.Where(ci => ci.IsSystem == false).Count() > MaxOrderQuantity);
        }

        /// <summary>
        /// Clears the shipping options.
        /// </summary>
        public void ClearShippingOptions()
        {
            DB.ExecuteSQL("update dbo.ShoppingCart set ShippingMethodID = null, ShippingMethod = null where CustomerID = " + m_ThisCustomer.CustomerID + " and CartType = 0");
        }

        /// <summary>
        /// Gets or sets the type of the cart.
        /// </summary>
        /// <value>The type of the cart.</value>
        public CartTypeEnum CartType
        {
            get
            {
                return m_CartType;
            }
            set
            {
                m_CartType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether shipping is free.
        /// </summary>
        /// <value><c>true</c> if [shipping is free]; otherwise, <c>false</c>.</value>
        public bool ShippingIsFree
        {
            get
            {
                return m_ShippingIsFree;
            }
            set
            {
                m_ShippingIsFree = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Promotions are allowed. Hides/Shows promo box on shoppingcart.aspx
        /// </summary>
        /// <value><c>true</c> if [promotions allowed]; otherwise, <c>false</c>.</value>
        public bool PromotionsEnabled
        {
            get
            {
                return m_PromotionsEnabled;
            }
            set
            {
                m_PromotionsEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Gift Cards are allowed. Hides/Shows Gift card box on shoppingcart.aspx
        /// </summary>
        /// <value><c>true</c> if [gift cards allowed]; otherwise, <c>false</c>.</value>
        public bool GiftCardsEnabled
        {
            get
            {
                return m_GiftCardsEnabled;
            }
            set
            {
                m_GiftCardsEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether cart allows shipping method selection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [cart allows shipping method selection]; otherwise, <c>false</c>.
        /// </value>
        public bool CartAllowsShippingMethodSelection
        {
            get
            {
                return m_CartAllowsShippingMethodSelection;
            }
            set
            {
                m_CartAllowsShippingMethodSelection = value;
            }
        }

        /// <summary>
        /// Gets the customer addresses.
        /// </summary>
        /// <value>The customer addresses.</value>
        public Addresses CustomerAddresses
        {
            get
            {
                return m_CustAddresses;
            }
        }

        /// <summary>
        /// Gets the order notes.
        /// </summary>
        /// <value>The order notes.</value>
        public String OrderNotes
        {
            get
            {
                return m_OrderNotes;
            }
            set
            {
                m_OrderNotes = value;
            }
        }

        /// <summary>
        /// Gets the finalization data.
        /// </summary>
        /// <value>The finalization data.</value>
        public String FinalizationData
        {
            get
            {
                return m_FinalizationData;
            }
        }

        public List<OrderOption> AllOrderOptions
        {
            get { return m_AllOrderOptions; }
        }

        public List<OrderOption> OrderOptions
        {
            get { return m_OrderOptions; } //.Select(o => o.ID).ToList(); }
        }

        public List<int> OrderOptionsIDs
        {
            get
            {
                List<int> ooIDs = new List<int>();

                ooIDs = m_OrderOptions.Select(oo => oo.ID).ToList();

                return ooIDs;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the inventory is trimmed.
        /// </summary>
        /// <value><c>true</c> if [inventory trimmed]; otherwise, <c>false</c>.</value>
        public bool InventoryTrimmed
        {
            get
            {
                return CheckInventory(m_ThisCustomer.CustomerID, out _TrimmedReason);
            }
        }

        private InventoryTrimmedReason _TrimmedReason;
        public InventoryTrimmedReason TrimmedReason { get { return _TrimmedReason; } }

        /// <summary>
        /// Gets a value indicating whether minimum quantities is updated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [minimum quantities updated]; otherwise, <c>false</c>.
        /// </value>
        public bool MinimumQuantitiesUpdated
        {
            get
            {
                return m_MinimumQuantitiesUpdated;
            }
            set
            {
                m_MinimumQuantitiesUpdated = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether recurring schedule has conflict.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [recurring schedule conflict]; otherwise, <c>false</c>.
        /// </value>
        public bool RecurringScheduleConflict
        {
            get
            {
                return m_RecurringScheduleConflict;
            }
        }

        /// <summary>
        /// Gets a value indicating whether shopping cart contains recurring auto ship.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [contains recurring auto ship]; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsRecurringAutoShip
        {
            get
            {
                return m_ContainsRecurringAutoShip;
            }
        }

        /// <summary>
        /// Gets the free shipping reason.
        /// </summary>
        /// <value>The free shipping reason.</value>
        public Shipping.FreeShippingReasonEnum FreeShippingReason
        {
            get
            {
                return m_FreeShippingReason;
            }
        }

        /// <summary>
        /// Gets the ShipCalcID.
        /// </summary>
        /// <value>The ship calc ID.</value>
        public Shipping.ShippingCalculationEnum ShipCalcID
        {
            get
            {
                return m_ShipCalcID;
            }
        }

        /// <summary>
        /// Gets the free shipping method.
        /// </summary>
        /// <value>The free shipping method.</value>
        public String FreeShippingMethod
        {
            get
            {
                return m_FreeShippingMethod;
            }
        }

        /// <summary>
        /// Gets the free shipping threshold.
        /// </summary>
        /// <value>The free shipping threshold.</value>
        public decimal FreeShippingThreshold
        {
            get
            {
                return m_FreeShippingThreshold;
            }
        }

        /// <summary>
        /// Gets the amount needed to reach free shipping.
        /// </summary>
        /// <value>The amount needed to reach free shipping.</value>
        public decimal MoreNeededToReachFreeShipping
        {
            get
            {
                return m_MoreNeededToReachFreeShipping;
            }
        }

        /// <summary>
        /// Gets the Email.
        /// </summary>
        /// <value>The Email.</value>
        public String EMail
        {
            get
            {
                return m_EMail;
            }
        }

        /// <summary>
        /// Gets or sets the cart items.
        /// </summary>
        /// <value>The cart items.</value>
        public CartItemCollection CartItems
        {
            get
            {
                return m_CartItems;
            }
            //}
            //set
            //{
            //    m_CartItems = value;
            //}
        }

        /// <summary>
        /// Gets this customer.
        /// </summary>
        /// <value>The this customer.</value>
        public Customer ThisCustomer
        {
            get
            {
                return m_ThisCustomer;
            }
        }

        /// <summary>
        /// Gets the skinID.
        /// </summary>
        /// <value>The skin ID.</value>
        public int SkinID
        {
            get
            {
                return m_SkinID;
            }
        }

        /// <summary>
        /// Gets the original recurring order number.
        /// </summary>
        /// <value>The original recurring order number.</value>
        public int OriginalRecurringOrderNumber
        {
            get
            {
                return m_OriginalRecurringOrderNumber;
            }
        }

        /// <summary>
        /// Gets the RecurringSubscriptionID.
        /// </summary>
        /// <value>The recurring subscription ID.</value>
        public String RecurringSubscriptionID
        {
            get
            {
                return m_RecurringSubscriptionID;
            }
        }

        /// <summary>
        /// Gets or sets the coupon.
        /// </summary>
        /// <value>The coupon.</value>
        public CouponObject Coupon
        {
            get
            {
                return m_Coupon;
            }
            set
            {
                m_Coupon = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether coupon is valid.
        /// </summary>
        /// <value><c>true</c> if [coupon is valid]; otherwise, <c>false</c>.</value>
        public bool CouponIsValid
        {
            get
            {
                return m_CouponStatus == AppLogic.ro_OK;
            }
        }

        /// <summary>
        /// Gets the coupon status message.
        /// </summary>
        /// <value>The coupon status message.</value>
        public String CouponStatusMessage
        {
            get
            {
                return m_CouponStatus;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether shipping threshold is defined but free shipping method ID is not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [shipping thres hold is defined but free shipping method ID is not]; otherwise, <c>false</c>.
        /// </value>
        public bool ShippingThresHoldIsDefinedButFreeShippingMethodIDIsNot
        {
            get { return _shippingThresHoldIsDefinedButFreeShippingMethodIDIsNot; }
            set { _shippingThresHoldIsDefinedButFreeShippingMethodIDIsNot = value; }
        }

        /// <summary>
        /// Returns the customer's address
        /// </summary>
        /// <param name="AddressID">The address ID.</param>
        /// <returns></returns>
        public Address CustomerAddress(int AddressID)
        {
            if (m_CustAddresses == null)
            {
                m_CustAddresses.LoadCustomer(ThisCustomer.CustomerID);
            }
            for (int i = 0; i < m_CustAddresses.Count; i++)
            {
                if (m_CustAddresses[i].AddressID == AddressID)
                {
                    return m_CustAddresses[i];
                }
            }

            return null;

        }

        /// <summary>
        /// Formats the Freight Rate
        /// </summary>
        /// <param name="freightRate">the freight rate</param>
        /// <returns></returns>
        public string FormatFreightRate(decimal freightRate)
        {
            string freightDisplay = m_ThisCustomer.CurrencyString(freightRate);

            if (m_CartAllowsShippingMethodSelection)
            {
                if (ShippingIsFree)
                {
                    Boolean allowsSelection = AppLogic.AppConfigBool("FreeShippingAllowsRateSelection");

                    if (!allowsSelection)
                    {
                        if (NoShippingRequiredComponents())
                        {
                            freightDisplay = AppLogic.GetString("common.cs.88", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                        else
                        {
                            freightDisplay = AppLogic.GetString("common.cs.87", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }

                    }
                    else if (NoShippingRequiredComponents() || IsAllEmailGiftCards() || IsAllDownloadComponents())
                    {
                        freightDisplay = AppLogic.GetString("common.cs.88", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                    else if (HasMultipleShippingAddresses())
                    {
                        List<int> DistinctAddrIds = Shipping.GetDistinctShippingAddressIDs(CartItems);

                        Boolean addrIsFree = true;
                        Boolean addrsAreFree = true;

                        if (DistinctAddrIds.Count > 0)
                        {
                            foreach (int AddressID in DistinctAddrIds)
                            {
                                foreach (CartItem c in m_CartItems)
                                {
                                    if (!c.IsDownload && !c.IsSystem && c.ShippingAddressID == AddressID && c.Shippable && !GiftCard.s_IsEmailGiftCard(c.ProductID) && !Shipping.ShippingMethodIsInFreeList(c.ShippingMethodID))
                                    {
                                        addrIsFree = false;
                                    }
                                }

                                if (!addrIsFree)
                                {
                                    addrsAreFree = false;
                                }
                            }
                        }

                        if (addrsAreFree)
                        {
                            freightDisplay = AppLogic.GetString("common.cs.87", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                    }
                    else
                    {
                        if (allowsSelection)
                        {
                            if (Shipping.ShippingMethodIsInFreeList(FirstItem().ShippingMethodID))
                            {
                                freightDisplay = AppLogic.GetString("common.cs.87", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                            }
                        }
                    }
                }
            }
            else
            {
                if (ShippingIsFree && !NoShippingRequiredComponents() && !IsAllEmailGiftCards() && !IsAllDownloadComponents())
                {
                    freightDisplay = AppLogic.GetString("common.cs.87", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                else if (NoShippingRequiredComponents() || IsAllEmailGiftCards() || IsAllDownloadComponents())
                {
                    freightDisplay = AppLogic.GetString("common.cs.88", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
            }

            return freightDisplay;
        }

        #region GetIndividualCartItems

        public CartItemCollection GetIndividualShippableCartItemsPerQuantity()
        {
            return GetAllIndividualCartItems(true);
        }

        public CartItemCollection GetAllIndividualCartItems()
        {
            return GetAllIndividualCartItems(false);
        }

        private CartItemCollection GetAllIndividualCartItems(bool breakDownPerShippableQuantity)
        {
            CartItemCollection individualItems = new CartItemCollection();

            foreach (var cItem in m_CartItems)
            {
                //If the item is a gift or not shippable then don't allow user to split the quantity for shipping(or we aren't breaking down by quantity)
                if (!breakDownPerShippableQuantity || cItem.IsGift || !cItem.Shippable)
                {
                    individualItems.Add(cItem.Copy());
                }
                else
                {
                    // break down per quantity
                    for (int i = 1; i <= cItem.Quantity; i++)
                    {
                        individualItems.Add(cItem.Copy());
                    }
                }
            }

            return individualItems;
        }

        #endregion

        /// <summary>
        /// Adds an item to the cart based on the AddToCartInfo parameters
        /// </summary>
        /// <param name="ThisCustomer"></param>
        /// <param name="addCartInfo"></param>
        /// <returns></returns>
        public static bool AddToCart(Customer ThisCustomer, AddToCartInfo addCartInfo)
        {
            bool success = false;

            // sanity check
            if (addCartInfo.Quantity <= 0)
            {
                addCartInfo.Quantity = 1;
            }

            if (addCartInfo.Quantity > 0)
            {
                ThisCustomer.RequireCustomerRecord();

                if (addCartInfo.VariantStyle == VariantStyleEnum.ERPWithRollupAttributes)
                {
                    String match = "<GroupAttributes></GroupAttributes>";
                    String match2 = "<GroupAttributes></GroupAttributes>";
                    if (addCartInfo.ChosenSize.Trim().Length != 0 &&
                        addCartInfo.ChosenColor.Trim().Length != 0)
                    {
                        match = "<GroupAttributes><GroupAttributeName=\"Attr1\"Value=\"" + addCartInfo.ChosenSize + "\"/><GroupAttributeName=\"Attr2\"Value=\"" + addCartInfo.ChosenColor + "\"/></GroupAttributes>";
                        match2 = "<GroupAttributes><GroupAttributeName=\"Attr1\"Value=\"" + addCartInfo.ChosenColor + "\"/><GroupAttributeName=\"Attr2\"Value=\"" + addCartInfo.ChosenSize + "\"/></GroupAttributes>";
                    }
                    else if (addCartInfo.ChosenSize.Trim().Length != 0 &&
                        addCartInfo.ChosenColor.Trim().Length == 0)
                    {
                        match = "<GroupAttributes><GroupAttributeName=\"Attr1\"Value=\"" + addCartInfo.ChosenSize + "\"/></GroupAttributes>";
                    }
                    else if (addCartInfo.ChosenSize.Trim().Length == 0 &&
                        addCartInfo.ChosenColor.Trim().Length != 0)
                    {
                        match = "<GroupAttributes><GroupAttributeName=\"Attr1\"Value=\"" + addCartInfo.ChosenColor + "\"/></GroupAttributes>";
                    }

                    // reset variant id to the proper attribute match!
                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rsERP = DB.GetRS("select VariantID,ExtensionData2 from ProductVariant with (NOLOCK) where VariantID=" + addCartInfo.VariantId.ToString(), con))
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
                                    addCartInfo.VariantId = DB.RSFieldInt(rsERP, "VariantID");
                                    break;
                                }
                            }
                        }
                    }
                }

                ShoppingCart cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

                if (addCartInfo.IsAKit && addCartInfo.IsKit2)
                {
                    if (CommonLogic.FormBool("IsEditKit") && CommonLogic.FormUSInt("CartRecID") > 0)
                    {
                        AppLogic.ClearKitItems(ThisCustomer,
                            addCartInfo.ProductId,
                            addCartInfo.VariantId,
                            addCartInfo.ShoppingCartRecordId);
                        cart.RemoveItem(addCartInfo.ShoppingCartRecordId, true);
                    }

                    KitComposition preferredComposition = KitComposition.FromForm(ThisCustomer, addCartInfo.ProductId, addCartInfo.VariantId);

                    String tmp = DB.GetNewGUID();
                    int NewRecID = cart.AddItem(ThisCustomer,
                                    addCartInfo.ShippingAddressId,
                                    addCartInfo.ProductId,
                                    addCartInfo.VariantId,
                                    addCartInfo.Quantity,
                                    string.Empty,
                                    addCartInfo.ChosenColorSKUModifier,
                                    addCartInfo.ChosenSize,
                                    addCartInfo.ChosenSizeSKUModifier,
                                    addCartInfo.TextOption,
                                    addCartInfo.CartType,
                                    false,
                                    false,
                                    0,
                                    System.Decimal.Zero,
                                    preferredComposition,
                                    addCartInfo.BluBucksUsed,
                                    addCartInfo.CategoryFundUsed,
                                    addCartInfo.FundID,
                                    addCartInfo.BluBucksPercentageUsed,
                                    addCartInfo.ProductCategoryID,
                                    addCartInfo.GLcode
                                    );
                }
                else
                {
                    cart.AddItem(ThisCustomer,
                        addCartInfo.ShippingAddressId,
                        addCartInfo.ProductId,
                        addCartInfo.VariantId,
                        addCartInfo.Quantity,
                        addCartInfo.ChosenColor,
                        addCartInfo.ChosenColorSKUModifier,
                        addCartInfo.ChosenSize,
                        addCartInfo.ChosenSizeSKUModifier,
                        addCartInfo.TextOption,
                        addCartInfo.CartType,
                        false,
                        false,
                        0,
                        addCartInfo.CustomerEnteredPrice,
                        addCartInfo.BluBucksUsed,
                        addCartInfo.CategoryFundUsed,
                        addCartInfo.FundID,
                        addCartInfo.BluBucksPercentageUsed,
                        addCartInfo.ProductCategoryID,
                        addCartInfo.GLcode
                       );
                }

                success = true;

                if (addCartInfo.UpsellProductIds.Count > 0 &&
                    addCartInfo.CartType == CartTypeEnum.ShoppingCart)
                {
                    foreach (int UpsellProductID in addCartInfo.UpsellProductIds)
                    {
                        try
                        {
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
                                    cart.AddItem(ThisCustomer,
                                        addCartInfo.ShippingAddressId,
                                        UpsellProductID,
                                        UpsellVariantID,
                                        1,
                                        Colors,
                                        ColorSKUModifiers,
                                        Sizes,
                                        SizeSKUModifiers,
                                        String.Empty,
                                        addCartInfo.CartType,
                                        false,
                                        false,
                                        0,
                                        System.Decimal.Zero
                                        );
                                    Decimal PR = AppLogic.GetUpsellProductPrice(addCartInfo.ProductId, UpsellProductID, ThisCustomer.CustomerLevelID);
                                    DB.ExecuteSQL("update shoppingcart set IsUpsell=1, ProductPrice=" + Localization.CurrencyStringForDBWithoutExchangeRate(PR) + " where CartType=" + ((int)addCartInfo.CartType).ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString() + " and ProductID=" + UpsellProductID.ToString() + " and VariantID=" + UpsellVariantID.ToString() + " and convert(nvarchar(1000),ChosenColor)='' and convert(nvarchar(1000),ChosenSize)='' and convert(nvarchar(1000),TextOption)=''");
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Gets the SubTotal.
        /// </summary>
        /// <param name="includeDiscount">if set to <c>true</c> [include discount].</param>
        /// <param name="onlyIncludeTaxableItems">if set to <c>true</c> [only include taxable items].</param>
        /// <param name="includeDownloadItems">if set to <c>true</c> [include download items].</param>
        /// <param name="includeFreeShippingItems">if set to <c>true</c> [include free shipping items].</param>
        /// <returns></returns>
        public Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems)
        {
            return Prices.SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, true, false, CartItems, ThisCustomer, OrderOptions);
        }

        /// <summary>
        /// Gets the SubTotal.
        /// </summary>
        /// <param name="includeDiscount">if set to <c>true</c> [include discount].</param>
        /// <param name="onlyIncludeTaxableItems">if set to <c>true</c> [only include taxable items].</param>
        /// <param name="includeDownloadItems">if set to <c>true</c> [include download items].</param>
        /// <param name="includeFreeShippingItems">if set to <c>true</c> [include free shipping items].</param>
        /// <param name="includeSystemItems">if set to <c>true</c> [include system items].</param>
        /// <returns></returns>
        public Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems)
        {
            return Prices.SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, false, CartItems, ThisCustomer, OrderOptions);
        }

        /// <summary>
        /// Gets the SubTotal.
        /// </summary>
        /// <param name="includeDiscount">if set to <c>true</c> [include discount].</param>
        /// <param name="onlyIncludeTaxableItems">if set to <c>true</c> [only include taxable items].</param>
        /// <param name="includeDownloadItems">if set to <c>true</c> [include download items].</param>
        /// <param name="includeFreeShippingItems">if set to <c>true</c> [include free shipping items].</param>
        /// <param name="includeSystemItems">if set to <c>true</c> [include system items].</param>
        /// <param name="UseCustomerCurrencySetting">if set to <c>true</c> [use customer currency setting].</param>
        /// <returns></returns>
        public Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting)
        {
            return Prices.SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, CartItems, ThisCustomer, OrderOptions);
        }

        /// <summary>
        /// Gets the SubTotal.
        /// </summary>
        /// <param name="includeDiscount">if set to <c>true</c> [include discount].</param>
        /// <param name="onlyIncludeTaxableItems">if set to <c>true</c> [only include taxable items].</param>
        /// <param name="includeDownloadItems">if set to <c>true</c> [include download items].</param>
        /// <param name="includeFreeShippingItems">if set to <c>true</c> [include free shipping items].</param>
        /// <param name="includeSystemItems">if set to <c>true</c> [include system items].</param>
        /// <param name="UseCustomerCurrencySetting">if set to <c>true</c> [use customer currency setting].</param>
        /// <param name="ForShippingAddressID">For shipping address ID.</param>
        /// <returns></returns>
        public Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, int ForShippingAddressID)
        {
            return Prices.SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, ForShippingAddressID, CartItems, ThisCustomer, OrderOptions);
        }

        /// <summary>
        /// Gets the SubTotal.
        /// </summary>
        /// <param name="includeDiscount">if set to <c>true</c> [include discount].</param>
        /// <param name="onlyIncludeTaxableItems">if set to <c>true</c> [only include taxable items].</param>
        /// <param name="includeDownloadItems">if set to <c>true</c> [include download items].</param>
        /// <param name="includeFreeShippingItems">if set to <c>true</c> [include free shipping items].</param>
        /// <param name="includeSystemItems">if set to <c>true</c> [include system items].</param>
        /// <param name="UseCustomerCurrencySetting">if set to <c>true</c> [use customer currency setting].</param>
        /// <param name="ForShippingAddressID">For shipping address ID.</param>
        /// <param name="ExcludeTax">if set to <c>true</c> [exclude tax].</param>
        /// <returns></returns>
        public Decimal SubTotal(bool includeDiscount, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems, bool UseCustomerCurrencySetting, int ForShippingAddressID, bool ExcludeTax)
        {
            return Prices.SubTotal(includeDiscount, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, UseCustomerCurrencySetting, ForShippingAddressID, ExcludeTax, CartItems, ThisCustomer, OrderOptions, OriginalRecurringOrderNumber, m_OnlyLoadRecurringItemsThatAreDue);
        }

        public String PageToBeginCheckout(Boolean forceOnePageCheckout, Boolean internationalCheckout)
        {
            AppLogic.eventHandler("BeginCheckout").CallEvent("&BeginCheckout=true");

            var checkoutPageController = CheckOutPageControllerFactory.CreateCheckOutPageController(ThisCustomer, this);

            // If ForceOnePageCheckout(customer click Quick Checkout), then try to go to one page checkout
            if (forceOnePageCheckout && checkoutPageController.CanUseOnePageCheckout())
            {
                return checkoutPageController.GetOnePageCheckoutPage();
            }
            // If InternationalCheckout(customer clicked International Checkout), then go to IC page
            if (internationalCheckout && checkoutPageController.CanUseInternationalCheckout())
            {
                return "internationalcheckout.aspx";
            }

            // Otherwise, let controller decide which page to use.
            return checkoutPageController.GetContinueCheckoutPage();
        }

        public void ApplyShippingRules()
        {
            if (!AppLogic.AppConfigBool("SurpressCartShippingRulesCode"))
            {
                if (!AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") || AppLogic.AppConfigBool("SkipShippingOnCheckout"))
                {
                    DB.ExecuteSQL("update shoppingcart set ShippingAddressID=" + ThisCustomer.PrimaryShippingAddressID.ToString() + " where CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString() + " and GiftRegistryForCustomerID=0", m_DBTrans);
                }
                LoadFromDB(CartType);
            }
        }

        public void ConsolidateCartItems()
        {
            ConsolidateCartItems(0);
        }

        private void ConsolidateCartItems(int round)
        {
            if (!AppLogic.AppConfigBool("SurpressCartMergeCode"))
            {
                if (round > this.CartItems.Count)
                {
                    AspDotNetStorefrontCore.SysLog.LogMessage("Failed to consolidate cart.", "Failed to consolidate cart.", MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                    return; //something has gone wrong.
                }

                Boolean didMerge = false;

                foreach (CartItem cic in this.CartItems)
                {
                    CartItem mergeItem = this.CartItems.FirstOrDefault(c => c.ShoppingCartRecordID != cic.ShoppingCartRecordID && c.MatchesComposition(cic));
                    if (mergeItem != null)
                    {
                        mergeItem.IncrementQuantity(cic.Quantity);
                        this.RemoveItem(cic.ShoppingCartRecordID, false);
                        LoadFromDB(CartType);
                        didMerge = true;
                        break;
                    }
                }

                if (didMerge)
                    ConsolidateCartItems(round + 1);
            }
        }

        public void RecalculateCartDiscount()
        {
            //Clear out the cart gift items if the only thing in the cart are gift items
            if (CartItems.Count > 0 && CartItems.Count == CartItems.Count(ci => ci.IsGift))
                ClearContents();

            if (!this.IsEmpty())
            {
                IRuleContext ruleContext = PromotionManager.CreateRuleContext(this);
                PromotionManager.AutoAssignPromotions(ThisCustomer.CustomerID, ruleContext);
                PromotionManager.PrioritizePromotions(ruleContext);
                m_discountResults = PromotionManager.GetDiscountResultList(ruleContext);
            }
            else
            {
                //clean up promos if the user has nothing in their cart.
                if (PromotionManager.GetPromotionUsagesByCustomer(ThisCustomer.CustomerID).Any())
                    PromotionManager.ClearAllPromotionUsages(ThisCustomer.CustomerID);
                m_discountResults = new List<AspDotNetStorefront.Promotions.IDiscountResult>();

                //Reset auto assigned anytime the cart is emptied out so that users won't have auto assigned only happen once per session.
                PromotionManager.ResetAutoAssignedPromotionList(ThisCustomer.CustomerID);
            }
        }
    }

    /// <summary>
    /// Data Structure to hold addtocart input parameters
    /// </summary>
    public class AddToCartInfo
    {
        public static readonly AddToCartInfo INVALID_FORM_COMPOSITION = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="thisCustomer"></param>
        /// <param name="productId"></param>
        private AddToCartInfo(Customer thisCustomer, int productId)
        {
            this.ThisCustomer = thisCustomer;
            this.ProductId = productId;

            UpsellProductIds = new List<int>();
        }

        private Customer m_thiscustomer;
        /// <summary>
        /// Gets or sets the current customer
        /// </summary>
        public Customer ThisCustomer
        {
            get { return m_thiscustomer; }
            set { m_thiscustomer = value; }
        }

        private int m_productid;
        /// <summary>
        /// Gets the Product Id
        /// </summary>
        public int ProductId
        {
            get { return m_productid; }
            set { m_productid = value; }
        }

        private int m_variantid;
        /// <summary>
        /// Gets or sets the variant id
        /// </summary>
        public int VariantId
        {
            get { return m_variantid; }
            set { m_variantid = value; }
        }

        private int m_shippingaddressid;
        /// <summary>
        /// Gets or sets the shipping address id
        /// </summary>
        public int ShippingAddressId
        {
            get { return m_shippingaddressid; }
            set { m_shippingaddressid = value; }
        }

        private int m_quantity;
        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity
        {
            get { return m_quantity; }
            set { m_quantity = value; }
        }

        private CartTypeEnum m_carttype;
        /// <summary>
        /// Gets or sets the carttype
        /// </summary>
        public CartTypeEnum CartType
        {
            get { return m_carttype; }
            set { m_carttype = value; }
        }

        private VariantStyleEnum m_variantstyle;
        /// <summary>
        /// Gets or sets the variant style
        /// </summary>
        public VariantStyleEnum VariantStyle
        {
            get { return m_variantstyle; }
            set { m_variantstyle = value; }
        }

        private string m_textoption;
        /// <summary>
        /// Gets or sets the textoption
        /// </summary>
        public string TextOption
        {
            get { return m_textoption; }
            set { m_textoption = value; }
        }

        private decimal m_customerenteredprice;
        /// <summary>
        /// Gets or sets the customer entered price
        /// </summary>
        public decimal CustomerEnteredPrice
        {
            get { return m_customerenteredprice; }
            set { m_customerenteredprice = value; }
        }

        private decimal m_BluBucksUsed;
        /// <summary>
        /// Gets or sets the customer entered price
        /// </summary>
        public decimal BluBucksUsed
        {
            get { return m_BluBucksUsed; }
            set { m_BluBucksUsed = value; }
        }

        private decimal m_CategoryFundUsed;
        /// <summary>
        /// Gets or sets the customer entered price
        /// </summary>
        public decimal CategoryFundUsed
        {
            get { return m_CategoryFundUsed; }
            set { m_CategoryFundUsed = value; }
        }

        private int m_FundID;
        /// <summary>
        /// Gets or sets the shipping address id
        /// </summary>
        public int FundID
        {
            get { return m_FundID; }
            set { m_FundID = value; }
        }

        private int m_ProductCategoryID;
        public int ProductCategoryID
        {
            get { return m_ProductCategoryID; }
            set { m_ProductCategoryID = value; }
        }

        private decimal m_BluBucksPercentageUsed;
        /// <summary>
        /// Gets or sets the customer entered price
        /// </summary>
        public decimal BluBucksPercentageUsed
        {
            get { return m_BluBucksPercentageUsed; }
            set { m_BluBucksPercentageUsed = value; }
        }

        private string m_GLcode;
        /// <summary>
        /// Gets or sets the customer entered price
        /// </summary>
        public string GLcode
        {
            get { return m_GLcode; }
            set { m_GLcode = value; }
        }

        private string m_chosencolor;
        /// <summary>
        /// Gets or sets the chosen color
        /// </summary>
        public string ChosenColor
        {
            get { return m_chosencolor; }
            set { m_chosencolor = value; }
        }

        private string m_chosencolorskumodifier;
        /// <summary>
        /// Gets or sets the chosen color sku modifier
        /// </summary>
        public string ChosenColorSKUModifier
        {
            get { return m_chosencolorskumodifier; }
            set { m_chosencolorskumodifier = value; }
        }

        private string m_chosensize;
        /// <summary>
        /// Gets or sets the chosen size
        /// </summary>
        public string ChosenSize
        {
            get { return m_chosensize; }
            set { m_chosensize = value; }
        }

        private string m_chosensizeskumodifier;
        /// <summary>
        /// Gets or sets the chosen size sku modifier
        /// </summary>
        public string ChosenSizeSKUModifier
        {
            get { return m_chosensizeskumodifier; }
            set { m_chosensizeskumodifier = value; }
        }

        private bool m_isakit;
        /// <summary>
        /// Gets or sets if this product is a kit product
        /// </summary>
        public bool IsAKit
        {
            get { return m_isakit; }
            set { m_isakit = value; }
        }

        private bool m_iseditkit;
        /// <summary>
        /// Gets or sets edit kit mode
        /// </summary>
        public bool IsEditKit
        {
            get { return m_iseditkit; }
            set { m_iseditkit = value; }
        }

        private bool m_iskit2;
        /// <summary>
        /// Gets or sets if this product uses kit2 xml package
        /// </summary>
        public bool IsKit2
        {
            get { return m_iskit2; }
            set { m_iskit2 = value; }
        }

        private int m_shoppingcartrecordid;
        /// <summary>
        /// Gets or sets the existing shoppingcart line item record id
        /// </summary>
        public int ShoppingCartRecordId
        {
            get { return m_shoppingcartrecordid; }
            set { m_shoppingcartrecordid = value; }
        }

        private List<int> m_upsellproductids;
        /// <summary>
        /// Gets or sets the upsell product ids
        /// </summary>
        public List<int> UpsellProductIds
        {
            get { return m_upsellproductids; }
            set { m_upsellproductids = value; }
        }

        /// <summary>
        /// Instantiates a new AddToCartInfo
        /// </summary>
        /// <param name="thisCustomer"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static AddToCartInfo NewAddToCartInfo(Customer thisCustomer, int productId)
        {
            thisCustomer.RequireCustomerRecord();
            AddToCartInfo nfo = new AddToCartInfo(thisCustomer, productId);

            string query = string.Format("SELECT IsAKit, IsAPack FROM Product WHERE ProductId = {0}", productId);
            bool exists = false;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(query, conn))
                {
                    if (rs.Read())
                    {
                        exists = true;
                        nfo.IsAKit = DB.RSFieldBool(rs, "IsAKit");
                    }

                    rs.Close();
                    rs.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }

            if (!exists)
            {
                return INVALID_FORM_COMPOSITION;
            }

            return nfo;
        }

        /// <summary>
        /// Parses the input string to set the chosen color or chosen color sku modifier
        /// </summary>
        /// <param name="colorSelection"></param>
        public void ParseColorOptions(string colorSelection)
        {
            String[] ColorSel = colorSelection.Split(',');
            try
            {
                this.ChosenColor = ColorSel[0].Trim();
            }
            catch { }
            try
            {
                this.ChosenColorSKUModifier = ColorSel[1];
            }
            catch { }
        }

        /// <summary>
        /// Parses the input string to se the chosen size or chosen size sku modifier
        /// </summary>
        /// <param name="sizeSelection"></param>
        public void ParseSizeOptions(string sizeSelection)
        {
            String[] SizeSel = sizeSelection.Split(',');
            try
            {
                this.ChosenSize = SizeSel[0].Trim();
            }
            catch { }
            try
            {
                this.ChosenSizeSKUModifier = SizeSel[1];
            }
            catch { }
        }

        /// <summary>
        /// Parses the input string to set the shipping address id and defaults to the customer's primary if not valid
        /// </summary>
        /// <param name="sId"></param>
        public void ParseValidShippingAddressId(string sId)
        {
            int id = 0;
            int.TryParse(sId, out id);
            this.ShippingAddressId = id;

            if ((id == 0 || !ThisCustomer.OwnsThisAddress(id)) &&
                ThisCustomer.PrimaryShippingAddressID != 0)
            {
                this.ShippingAddressId = ThisCustomer.PrimaryShippingAddressID;
            }
        }

        /// <summary>
        /// Parses the input string to set the customer entered price and performs currency exchange for foreign currencies
        /// </summary>
        /// <param name="sEnteredPrice"></param>
        public void ParseValidCustomerEnteredPrice(string sEnteredPrice)
        {
            decimal enteredPrice = decimal.Zero;
            decimal.TryParse(sEnteredPrice, out enteredPrice);

            // if input is invalid, will still default to zero
            this.CustomerEnteredPrice = enteredPrice;


            if (Currency.GetDefaultCurrency() != ThisCustomer.CurrencySetting &&
                enteredPrice != 0)
            {
                this.CustomerEnteredPrice = Currency.Convert(enteredPrice, ThisCustomer.CurrencySetting, Localization.StoreCurrency());
            }
        }

        /// <summary>
        /// Parses the comma delimited input string for valid upsell product ids
        /// </summary>
        /// <param name="commaDelimited"></param>
        public void ParseUpsellProductIds(string commaDelimited)
        {
            IEnumerable<int> validUpsellProductIds = commaDelimited
                                                    .Split(',')                     // split comma separated
                                                    .Where(p => p.IsInt())      // filter valid integer values
                                                    .Select(p => p.ToNativeInt());    // select converted int

            this.UpsellProductIds.Clear();
            this.UpsellProductIds.AddRange(validUpsellProductIds);
        }

        /// <summary>
        /// Extracts and creates an instance of AddToCartInfo from form post parameters
        /// </summary>
        /// <param name="ThisCustomer"></param>
        /// <returns></returns>
        public static AddToCartInfo FromForm(Customer ThisCustomer)
        {
            CartTypeEnum cartType;
            int productId;
            int variantId;

            // By design, we stash the product info in the __EVENTARGUMENT
            // hidden input when we simulate a postback on the AddToCart xml package extension
            // The format is {CartType}_{ProductId}_{VariantId}
            if (!ParseAddToCartInfoFromForm(out cartType, out productId, out variantId))
            {
                return INVALID_FORM_COMPOSITION;
            }

            AddToCartInfo nfo = AddToCartInfo.NewAddToCartInfo(ThisCustomer, productId);
            if (nfo == INVALID_FORM_COMPOSITION)
            {
                return INVALID_FORM_COMPOSITION;
            }

            nfo.VariantId = variantId;
            nfo.CartType = cartType;
            nfo.Quantity = CommonLogic.FormNativeInt(string.Format("Quantity_{0}_{1}", "1", "1"));//productId, variantId

            if (nfo.Quantity == 0)
            {
                SqlCommand sc = new SqlCommand(string.Format("select variantid from dbo.ProductVariant where isDefault=1 and productid={0}", productId));
                variantId = DB.Scalar<int>.ExecuteScalar(sc);
                nfo.Quantity = CommonLogic.FormNativeInt(string.Format("Quantity_{0}_{1}", "1", "1"));//productId, variantId
            }

            nfo.ParseValidShippingAddressId(CommonLogic.FormCanBeDangerousContent(string.Format("ShippingAddressID_{0}_{1}", productId, variantId)));//productId, variantId
            nfo.ParseValidCustomerEnteredPrice(CommonLogic.FormCanBeDangerousContent(string.Format("Price_{0}_{1}", productId, variantId)));//productId, variantId

            nfo.ParseColorOptions(HttpContext.Current.Server.HtmlDecode(CommonLogic.FormCanBeDangerousContent(string.Format("Color_{0}_{1}", "1", "1"))));//productId, variantId
            nfo.ParseSizeOptions(HttpContext.Current.Server.HtmlDecode(CommonLogic.FormCanBeDangerousContent(string.Format("Size_{0}_{1}", "1", "1"))));//productId, variantId

            nfo.TextOption = CommonLogic.FormCanBeDangerousContent(string.Format("TextOption_{0}_{1}", productId, variantId));
            nfo.VariantStyle = (VariantStyleEnum)CommonLogic.FormNativeInt(string.Format("VariantStyle_{0}_{1}", productId, variantId));

            // kit specific
            nfo.IsEditKit = CommonLogic.FormBool("IsEditKit");
            nfo.ShoppingCartRecordId = CommonLogic.FormUSInt("CartRecID");
            nfo.IsKit2 = !CommonLogic.IsStringNullOrEmpty(CommonLogic.FormCanBeDangerousContent("KitItems"));

            nfo.ParseUpsellProductIds(CommonLogic.FormCanBeDangerousContent("Upsell"));

            return nfo;
        }

        /// <summary>
        /// Parses form post parameters to retrieve the cartype, productid and variant id
        /// </summary>
        /// <param name="cartType"></param>
        /// <param name="productId"></param>
        /// <param name="variantId"></param>
        /// <returns></returns>
        private static bool ParseAddToCartInfoFromForm(out CartTypeEnum cartType, out int productId, out int variantId)
        {
            // default values
            cartType = CartTypeEnum.ShoppingCart;
            productId = 0;
            variantId = 0;

            // By design, we stash the product info in the __EVENTARGUMENT
            // hidden input when we simulate a postback on the AddToCart xml package extension
            // The format is {CartType}_{ProductId}_{VariantId}
            string value = CommonLogic.FormCanBeDangerousContent("__EVENTARGUMENT");

            if (!CommonLogic.IsStringNullOrEmpty(value))
            {
                string[] values = value.Split('_');

                if (values[0].IsInt() &&
                    values[1].IsInt() &&
                    values[2].IsInt())
                {
                    cartType = (CartTypeEnum)values[0].ToNativeInt();
                    productId = values[1].ToNativeInt();

                    int vID = CommonLogic.FormUSInt("VariantID_" + values[1] + "_" + values[2]);

                    if (vID > 0)
                    {
                        variantId = vID;
                    }
                    else
                    {
                        variantId = values[2].ToNativeInt();
                    }

                    return true;
                }
            }

            return false;
        }
    }

    public enum InventoryTrimmedReason
    {
        None,
        RestrictedQuantities,
        InventoryLevels,
        MinimumQuantities
    }
}
