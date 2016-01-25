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
using System.Linq;
using System.Text;
using System.Web;

namespace AspDotNetStorefrontCore
{
    
    /// <summary>
    /// Class containing the individual cart line item
    /// </summary>
    public class CartItem
    {
        #region Variable Declarations

        private DateTime m_CreatedOn;
        private DateTime m_NextRecurringShipDate;
        private int m_RecurringIndex;
        private int m_OriginalRecurringOrderNumber;
        private String m_RecurringSubscriptionID;
        private int m_ShoppingCartRecordID;
        private CartTypeEnum m_CartType;
        private int m_ProductID;
        private int m_VariantID;
        private bool m_IsSystem;
        private bool m_IsAKit;
        private int m_GiftRegistryForCustomerID;
        private String m_ProductName;
        private String m_ProductDescription;
        private String m_VariantName;
        private String m_SKU;
        private String m_ManufacturerPartNumber;
        private int m_Quantity;
        private int m_QuantityDiscountID;
        private String m_QuantityDiscountName;
        private List<int> m_RestrictedQuantities;
        private int m_MinimumQuantity;
        private Decimal m_QuantityDiscountPercent;
        private String m_ChosenColor;
        private String m_ChosenColorSKUModifier;
        private String m_ChosenSize;
        private String m_ChosenSizeSKUModifier;
        private String m_TextOption;
        private String m_TextOptionPrompt;
        private String m_SizeOptionPrompt;
        private String m_ColorOptionPrompt;
        private String m_CustomerEntersPricePrompt;
        private Decimal m_Weight;
        private String m_Dimensions;
        private int m_SubscriptionInterval;
        private DateIntervalTypeEnum m_SubscriptionIntervalType;
        private Decimal m_Price; // of one item! multiply by quantity to get this item subtotal
        private bool m_CustomerEntersPrice;
        private bool m_IsTaxable;
        private int m_TaxClassID;
        private Decimal m_TaxRate;
        private bool m_IsShipSeparately;
        private bool m_IsDownload;
        private String m_DownloadLocation;
        private bool m_FreeShipping;
        private bool m_Shippable;
        private int m_DistributorID;
        private bool m_IsRecurring;
        private int m_RecurringInterval;
        private DateIntervalTypeEnum m_RecurringIntervalType;
        private int m_ShippingAddressID;
        private int m_ShippingMethodID;
        private String m_ShippingMethod;
        private int m_BillingAddressID;
        private Address m_ShippingDetail;
        private String m_Notes;
        private String m_ExtensionData;
        private bool m_IsUpsell;
        private String m_OrderShippingDetail; // only used on ORDER items!
        private int m_RequiresCount;
        private int m_ProductTypeId;
        private String m_SEName;
        private String m_SEAltText;
        private bool m_IsAuctionItem;
        private String m_ImageFileNameOverride;
        private Customer m_ThisCustomer;
        private ShoppingCart m_ThisShoppingCart;
        private bool m_IsGift;
        private Decimal m_BluBuksUsed;
        private Decimal m_CategoryFundUsed;
        private Decimal m_pricewithBluBuksUsed;
        private Decimal m_pricewithCategoryFundUsed;
        private int m_FundID;
        private int m_ProductCategory;
        private Decimal m_BluBucksPercentageUsed;
        private String m_GLcode;
        private String m_FundName;
        private int m_Inventory;





        // computed fields
        private decimal m_computedTaxRate = System.Decimal.Zero;
        private decimal m_computedExtPriceRate = System.Decimal.Zero;
        private decimal m_computedRegularPrice = System.Decimal.Zero;
        private decimal m_computedVatRate = System.Decimal.Zero;
        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor for CartItem
        /// </summary>
        public CartItem() : this(null, null) 
        {
            m_RestrictedQuantities = new List<int>();
        }

        /// <summary>
        /// Default Constructor for CartItem
        /// </summary>
        public CartItem(ShoppingCart cart, Customer thisCustomer)
        {
            m_ThisShoppingCart = cart;
            m_ThisCustomer = thisCustomer;
            m_RestrictedQuantities = new List<int>();
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="ci">CartItem object</param>
        public CartItem(CartItem ci)
        {
            m_CreatedOn = ci.CreatedOn;
            m_NextRecurringShipDate = ci.NextRecurringShipDate;
            m_RecurringIndex = ci.RecurringIndex;
            m_OriginalRecurringOrderNumber = ci.OriginalRecurringOrderNumber;
            m_RecurringSubscriptionID = ci.RecurringSubscriptionID;
            m_ShoppingCartRecordID = ci.ShoppingCartRecordID;
            m_CartType = ci.CartType;
            m_ProductID = ci.ProductID;
            m_VariantID = ci.VariantID;
            m_IsSystem = ci.IsSystem;
            m_IsAKit = ci.IsAKit;
            m_GiftRegistryForCustomerID = ci.GiftRegistryForCustomerID;
            m_ProductName = ci.ProductName;
            m_VariantName = ci.VariantName;
            m_SKU = ci.SKU;
            m_ManufacturerPartNumber = ci.ManufacturerPartNumber;
            m_Quantity = ci.Quantity;
            m_QuantityDiscountID = ci.QuantityDiscountID;
            m_QuantityDiscountName = ci.QuantityDiscountName;
            m_RestrictedQuantities = ci.RestrictedQuantities;
            m_MinimumQuantity = ci.MinimumQuantity;
            m_QuantityDiscountPercent = ci.QuantityDiscountPercent;
            m_ChosenColor = ci.ChosenColor;
            m_ChosenColorSKUModifier = ci.ChosenColorSKUModifier;
            m_ChosenSize = ci.ChosenSize;
            m_ChosenSizeSKUModifier = ci.ChosenSizeSKUModifier;
            m_TextOption = ci.TextOption;
            m_TextOptionPrompt = ci.TextOptionPrompt;
            m_SizeOptionPrompt = ci.SizeOptionPrompt;
            m_ColorOptionPrompt = ci.ColorOptionPrompt;
            m_CustomerEntersPricePrompt = ci.CustomerEntersPricePrompt;
            m_Weight = ci.Weight;
            m_Dimensions = ci.Dimensions;
            m_SubscriptionInterval = ci.SubscriptionInterval;
            m_SubscriptionIntervalType = ci.SubscriptionIntervalType;
            m_Price = ci.Price;
            m_CustomerEntersPrice = ci.CustomerEntersPrice;
            m_IsTaxable = ci.IsTaxable;
            m_TaxClassID = ci.TaxClassID;
            m_TaxRate = ci.TaxRate;
            m_IsShipSeparately = ci.IsShipSeparately;
            m_IsDownload = ci.IsDownload;
            m_DownloadLocation = ci.DownloadLocation;
            m_FreeShipping = ci.FreeShipping;
            m_Shippable = ci.Shippable;
            m_DistributorID = ci.DistributorID;
            m_IsRecurring = ci.IsRecurring;
            m_RecurringInterval = ci.RecurringInterval;
            m_RecurringIntervalType = ci.RecurringIntervalType;
            m_ShippingAddressID = ci.ShippingAddressID;
            m_ShippingMethodID = ci.ShippingMethodID;
            m_ShippingMethod = ci.ShippingMethod;
            m_BillingAddressID = ci.BillingAddressID;
            m_ShippingDetail = ci.ShippingDetail;
            m_Notes = ci.Notes;
            m_ExtensionData = ci.ExtensionData;
            m_IsUpsell = ci.IsUpsell;
            m_OrderShippingDetail = ci.OrderShippingDetail;
            m_RequiresCount = ci.RequiresCount;
            m_ProductTypeId = ci.ProductTypeId;
            m_SEName = ci.SEName;
            m_SEAltText = ci.SEAltText;
            m_IsAuctionItem = ci.IsAuctionItem;
            m_ImageFileNameOverride = ci.ImageFileNameOverride;
            m_ThisCustomer = ci.ThisCustomer;
            m_ThisShoppingCart = ci.ThisShoppingCart;
            m_IsGift = ci.IsGift;
            m_ProductDescription = ci.ProductDescription;
            m_BluBuksUsed=ci.BluBuksUsed;
           m_CategoryFundUsed=ci.CategoryFundUsed;
           m_pricewithBluBuksUsed = ci.pricewithBluBuksUsed;
           m_pricewithCategoryFundUsed = ci.pricewithategoryFundUsed;
           m_FundID = ci.FundID;
           m_ProductCategory=ci.ProductCategoryID;
           m_BluBucksPercentageUsed = ci.BluBucksPercentageUsed;
           m_GLcode = ci.GLcode;
           m_FundName = ci.FundName;
           m_Inventory = ci.Inventory;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public DateTime CreatedOn
        {
            get { return m_CreatedOn; }
            set { m_CreatedOn = value; }
        }
        
        /// <summary>
        /// Gets or sets the NextREcurringShipDate
        /// </summary>
        public DateTime NextRecurringShipDate
        {
            get { return m_NextRecurringShipDate; }
            set { m_NextRecurringShipDate = value; }
        }

        /// <summary>
        /// Gets or sets the RecurringIndex
        /// </summary>
        public int RecurringIndex
        {
            get { return m_RecurringIndex; }
            set { m_RecurringIndex = value; }
        }
        public int Inventory
        {
            get { return m_Inventory; }
            set { m_Inventory = value; }
        }
        
        /// <summary>
        /// Gets or sets the OriginalRecurringOrderNumber
        /// </summary>
        public int OriginalRecurringOrderNumber
        {
            get { return m_OriginalRecurringOrderNumber; }
            set { m_OriginalRecurringOrderNumber = value; }
        }

        /// <summary>
        /// Gets or sets the RecurringSubscriptionID
        /// </summary>
        public String RecurringSubscriptionID
        {
            get { return m_RecurringSubscriptionID; }
            set { m_RecurringSubscriptionID = value; }
        }

        public String GLcode
        {
            get { return m_GLcode; }
            set { m_GLcode = value; }
        }

        public String FundName
        {
            get { return m_FundName; }
            set { m_FundName = value; }
        }

        public String ProductDescription
        {
            get { return m_ProductDescription; }
            set { m_ProductDescription = value; }
        }

        public Decimal BluBuksUsed
        {
            get { return m_BluBuksUsed; }
            set { m_BluBuksUsed = value; }
        }

        public Decimal CategoryFundUsed
        {
            get { return m_pricewithCategoryFundUsed; }
            set { m_pricewithCategoryFundUsed = value; }
        }

        public Decimal pricewithBluBuksUsed
        {
            get { return m_pricewithBluBuksUsed; }
            set { m_pricewithBluBuksUsed = value; }
        }

        public Decimal pricewithategoryFundUsed
        {
            get { return m_CategoryFundUsed; }
            set { m_CategoryFundUsed = value; }
        }

        public Decimal BluBucksPercentageUsed
        {
            get { return m_BluBucksPercentageUsed; }
            set { m_BluBucksPercentageUsed = value; }
        }

        public int FundID
        {
            get { return m_FundID; }
            set { m_FundID = value; }
        }

        public int ProductCategoryID
        {
            get { return m_ProductCategory; }
            set { m_ProductCategory = value; }
        }

        /// <summary>
        /// Gets or sets the ShoppingCartRecordID
        /// </summary>
        public int ShoppingCartRecordID
        {
            get { return m_ShoppingCartRecordID; }
            set { m_ShoppingCartRecordID = value; }
        }

        /// <summary>
        /// Gets or sets the CartType
        /// </summary>
        public CartTypeEnum CartType
        {
            get { return m_CartType; }
            set { m_CartType = value; }
        }

        /// <summary>
        /// Gets or sets the ProductID
        /// </summary>
        public int ProductID
        {
            get { return m_ProductID; }
            set { m_ProductID = value; }
        }

        /// <summary>
        /// Gets or sets the VariantID
        /// </summary>
        public int VariantID
        {
            get { return m_VariantID; }
            set { m_VariantID = value; }
        }

        /// <summary>
        /// Gets or sets the IsSystem
        /// </summary>
        public bool IsSystem
        {
            get { return m_IsSystem; }
            set { m_IsSystem = value; }
        }

        /// <summary>
        /// Gets or sets the IsAKit
        /// </summary>
        public bool IsAKit
        {
            get { return m_IsAKit; }
            set { m_IsAKit = value; }
        }

        /// <summary>
        /// Gets or sets the GiftRegistryForCustomerID
        /// </summary>
        public int GiftRegistryForCustomerID
        {
            get { return m_GiftRegistryForCustomerID; }
            set { m_GiftRegistryForCustomerID = value; }
        }

        /// <summary>
        /// Gets or sets the ProductName
        /// </summary>
        public String ProductName
        {
            get { return m_ProductName; }
            set { m_ProductName = value; }
        }

        /// <summary>
        /// Gets or sets the VariantName
        /// </summary>
        public String VariantName
        {
            get { return m_VariantName; }
            set { m_VariantName = value; }
        }

        /// <summary>
        /// Gets or sets the SKU
        /// </summary>
        public String SKU
        {
            get { return m_SKU; }
            set { m_SKU = value; }
        }

        /// <summary>
        /// Gets or sets the ManufacturerPartNumber
        /// </summary>
        public String ManufacturerPartNumber
        {
            get { return m_ManufacturerPartNumber; }
            set { m_ManufacturerPartNumber = value; }
        }

        /// <summary>
        /// Gets or sets the Quantity
        /// </summary>
        public int Quantity
        {
            get { return m_Quantity; }
            set { m_Quantity = value; }
        }

        /// <summary>
        /// Gets or sets the QuantityDiscountID 
        /// </summary>
        public int QuantityDiscountID
        {
            get { return m_QuantityDiscountID; }
            set { m_QuantityDiscountID = value; }
        }

        /// <summary>
        /// Gets or sets the QuantityDiscountName
        /// </summary>
        public String QuantityDiscountName
        {
            get { return m_QuantityDiscountName; }
            set { m_QuantityDiscountName = value; }
        }

        /// <summary>
        /// Gets or sets the RestrictedQuantities
        /// </summary>
        public List<int> RestrictedQuantities
        {
            get { return m_RestrictedQuantities; }
            set { m_RestrictedQuantities = value; }
        }

        /// <summary>
        /// Gets or sets the MinimumQuantity
        /// </summary>
        public int MinimumQuantity
        {
            get { return m_MinimumQuantity; }
            set { m_MinimumQuantity = value; }
        }

        /// <summary>
        /// Gets or sets the QuantityDiscountPercent
        /// </summary>
        public Decimal QuantityDiscountPercent
        {
            get { return m_QuantityDiscountPercent; }
            set { m_QuantityDiscountPercent = value; }
        }

        /// <summary>
        /// Gets or sets the ChosenColor
        /// </summary>
        public String ChosenColor
        {
            get { return m_ChosenColor; }
            set { m_ChosenColor = value; }
        }

        /// <summary>
        /// Gets or sets the ChosenColorSKUModifier
        /// </summary>
        public String ChosenColorSKUModifier
        {
            get { return m_ChosenColorSKUModifier; }
            set { m_ChosenColorSKUModifier = value; }
        }

        /// <summary>
        /// Gets or sets the ChosenSize
        /// </summary>
        public String ChosenSize
        {
            get { return m_ChosenSize; }
            set { m_ChosenSize = value; }
        }

        /// <summary>
        /// Gets or sets the ChosenSizeSKUModifier
        /// </summary>
        public String ChosenSizeSKUModifier
        {
            get { return m_ChosenSizeSKUModifier; }
            set { m_ChosenSizeSKUModifier = value; }
        }

        /// <summary>
        /// Gets or sets the TextOption
        /// </summary>
        public String TextOption
        {
            get { return m_TextOption; }
            set { m_TextOption = value; }
        }

        /// <summary>
        /// Gets or sets the TextOptionPrompt
        /// </summary>
        public String TextOptionPrompt
        {
            get { return m_TextOptionPrompt; }
            set { m_TextOptionPrompt = value; }
        }

        /// <summary>
        /// Gets or sets the SizeOptionPrompt
        /// </summary>
        public String SizeOptionPrompt
        {
            get { return m_SizeOptionPrompt; }
            set { m_SizeOptionPrompt = value; }
        }

        /// <summary>
        /// Gets or sets the ColorOptionPrompt
        /// </summary>
        public String ColorOptionPrompt
        {
            get { return m_ColorOptionPrompt; }
            set { m_ColorOptionPrompt = value; }
        }

        /// <summary>
        /// Gets or sets the CustomerEntersPricePrompt
        /// </summary>
        public String CustomerEntersPricePrompt
        {
            get { return m_CustomerEntersPricePrompt; }
            set { m_CustomerEntersPricePrompt = value; }
        }

        /// <summary>
        /// Gets or sets the Weight
        /// </summary>
        public Decimal Weight
        {
            get { return m_Weight; }
            set { m_Weight = value; }
        }

        /// <summary>
        /// Gets or sets the Dimensions
        /// </summary>
        public String Dimensions
        {
            get { return m_Dimensions; }
            set { m_Dimensions = value; }
        }

        /// <summary>
        /// Gets or sets the SubscriptionInterval
        /// </summary>
        public int SubscriptionInterval
        {
            get { return m_SubscriptionInterval; }
            set { m_SubscriptionInterval = value; }
        }

        /// <summary>
        /// Gets or sets the SubscriptionIntervalType
        /// </summary>
        public DateIntervalTypeEnum SubscriptionIntervalType
        {
            get { return m_SubscriptionIntervalType; }
            set { m_SubscriptionIntervalType = value; }
        }

        /// <summary>
        /// Gets or sets the Price
        /// </summary>
        public Decimal Price
        {
            get { return m_Price; }
            set { m_Price = value; }
        }

        /// <summary>
        /// Gets or sets the CustomerEntersPrice
        /// </summary>
        public bool CustomerEntersPrice
        {
            get { return m_CustomerEntersPrice; }
            set { m_CustomerEntersPrice = value; }
        }

        /// <summary>
        /// Gets or sets the IsTaxable
        /// </summary>
        public bool IsTaxable
        {
            get { return m_IsTaxable; }
            set { m_IsTaxable = value; }
        }

        /// <summary>
        /// Gets or sets the TaxClassID
        /// </summary>
        public int TaxClassID
        {
            get { return m_TaxClassID; }
            set { m_TaxClassID = value; }
        }

        /// <summary>
        /// Gets or sets the TaxRate
        /// </summary>
        public Decimal TaxRate
        {
            get { return m_TaxRate; }
            set { m_TaxRate = value; }
        }

        /// <summary>
        /// Gets or sets the IsShipSeparately
        /// </summary>
        public bool IsShipSeparately
        {
            get { return m_IsShipSeparately; }
            set { m_IsShipSeparately = value; }
        }

        /// <summary>
        /// Gets or sets the IsDownload
        /// </summary>
        public bool IsDownload
        {
            get { return m_IsDownload; }
            set { m_IsDownload = value; }
        }

        /// <summary>
        /// Gets or sets the DownloadLocation
        /// </summary>
        public String DownloadLocation
        {
            get { return m_DownloadLocation; }
            set { m_DownloadLocation = value; }
        }

        /// <summary>
        /// Gets or sets the FreeShipping
        /// </summary>
        public bool FreeShipping
        {
            get { return m_FreeShipping; }
            set { m_FreeShipping = value; }
        }

        /// <summary>
        /// Gets or sets the Shippable
        /// </summary>
        public bool Shippable
        {
            get { return m_Shippable; }
            set { m_Shippable = value; }
        }

        /// <summary>
        /// Gets or sets the DistributorID 
        /// </summary>
        public int DistributorID
        {
            get { return m_DistributorID; }
            set { m_DistributorID = value; }
        }

        /// <summary>
        /// Gets or sets the IsRecurring
        /// </summary>
        public bool IsRecurring
        {
            get { return m_IsRecurring; }
            set { m_IsRecurring = value; }
        }

        /// <summary>
        /// Gets or sets the RecurringInterval
        /// </summary>
        public int RecurringInterval
        {
            get { return m_RecurringInterval; }
            set { m_RecurringInterval = value; }
        }

        /// <summary>
        /// Gets or sets the RecurringIntervalType
        /// </summary>
        public DateIntervalTypeEnum RecurringIntervalType
        {
            get { return m_RecurringIntervalType; }
            set { m_RecurringIntervalType = value; }
        }

        /// <summary>
        /// Gets or sets the ShippingAddressID
        /// </summary>
        public int ShippingAddressID
        {
            get { return m_ShippingAddressID; }
            set { m_ShippingAddressID = value; }
        }

        /// <summary>
        /// Gets or sets the ShippingMethodID
        /// </summary>
        public int ShippingMethodID
        {
            get { return m_ShippingMethodID; }
            set { m_ShippingMethodID = value; }
        }

        /// <summary>
        /// Gets or sets the ShippingMethod
        /// </summary>
        public String ShippingMethod
        {
            get { return m_ShippingMethod; }
            set { m_ShippingMethod = value; }
        }

        /// <summary>
        /// Gets or sets the BillingAddressID
        /// </summary>
        public int BillingAddressID
        {
            get { return m_BillingAddressID; }
            set { m_BillingAddressID = value; }
        }

        /// <summary>
        /// Gets or sets the ShippingDetail
        /// </summary>
        public Address ShippingDetail
        {
            get { return m_ShippingDetail; }
            set { m_ShippingDetail = value; }
        }

        /// <summary>
        /// Gets or sets the Notes
        /// </summary>
        public String Notes
        {
            get { return m_Notes; }
            set { m_Notes = value; }
        }

        /// <summary>
        /// Gets or sets the ExtensionData
        /// </summary>
        public String ExtensionData
        {
            get { return m_ExtensionData; }
            set { m_ExtensionData = value; }
        }

        /// <summary>
        /// Gets or sets the IsUpsell
        /// </summary>
        public bool IsUpsell
        {
            get { return m_IsUpsell; }
            set { m_IsUpsell = value; }
        }

        /// <summary>
        /// Gets or sets the OrderShippingDetail
        /// </summary>
        public String OrderShippingDetail
        {
            get { return m_OrderShippingDetail; }
            set { m_OrderShippingDetail = value; }
        }

        /// <summary>
        /// Gets or sets the RequiresCount
        /// </summary>
        public int RequiresCount
        {
            get { return m_RequiresCount; }
            set { m_RequiresCount = value; }
        }

        /// <summary>
        /// Gets or sets the ProductTypeId
        /// </summary>
        public int ProductTypeId
        {
            get { return m_ProductTypeId; }
            set { m_ProductTypeId = value; }
        }

        /// <summary>
        /// Gets or sets the SEName
        /// </summary>
        public String SEName
        {
            get { return m_SEName; }
            set { m_SEName = value; }
        }

        /// <summary>
        /// Gets or sets the SEAltText
        /// </summary>
        public String SEAltText
        {
            get { return m_SEAltText; }
            set { m_SEAltText = value; }
        }

        /// <summary>
        /// Gets or sets the IsAuctionItem
        /// </summary>
        public bool IsAuctionItem
        {
            get { return m_IsAuctionItem; }
            set { m_IsAuctionItem = value; }
        }

        /// <summary>
        /// Gets or sets the ImageFileNameOverride
        /// </summary>
        public String ImageFileNameOverride
        {
            get { return m_ImageFileNameOverride; }
            set { m_ImageFileNameOverride = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string GiftRegistryDisplayName
        {
            get
            {
                return AppLogic.GiftRegistryDisplayName(GiftRegistryForCustomerID, false, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
        }

        /// <summary>
        /// Gets or sets the ThisCustomer
        /// </summary>
        public Customer ThisCustomer
        {
            get { return m_ThisCustomer; }
            set { m_ThisCustomer = value; }
        }

        /// <summary>
        /// Gets or sets the ThisShoppingCart
        /// </summary>
        public ShoppingCart ThisShoppingCart
        {
            get { return m_ThisShoppingCart; }
            set { m_ThisShoppingCart = value; }
        }

        /// <summary>
        /// Gets or sets the IsGift
        /// </summary>
        public bool IsGift
        {
            get { return m_IsGift; }
            set { m_IsGift = value; }
        }

        private int m_moveablequantity;
        /// <summary>
        /// Helper property
        /// </summary>
        public int MoveableQuantity 
        {
            get { return m_moveablequantity; }
            set { m_moveablequantity = value; }
        }

        /// <summary>
        /// Gets the ProductPicURL
        /// </summary>
        public string ProductPicURL
        {
            get { return GetLineItemProductPicURL(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public KitComposition KitComposition
        {
            get { return KitComposition.FromCart(ThisCustomer, CartType, ShoppingCartRecordID); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string KitEditURL
        {
            get
            {
                return string.Format("showproduct.aspx?cartrecid={0}&edit={1}&productid={2}&SEName={3}", ShoppingCartRecordID, ShoppingCartRecordID, ProductID, SEName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string RecurringDisplay
        {
            get
            {
                string autoShipString = string.Empty;
                if (IsRecurring)
                {
                    if ((int)m_RecurringIntervalType >= 0)
                    {
                        autoShipString += String.Format(AppLogic.GetString("shoppingcart.cs.26a", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), m_RecurringInterval.ToString(), m_RecurringIntervalType.ToString());
                    }
                    else
                    {
                        autoShipString += String.Format(AppLogic.GetString("shoppingcart.cs.26b", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), m_RecurringIntervalType.ToString());
                    }
                }
                return autoShipString;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string ProperProductName
        {
            get
            {
                return AppLogic.MakeProperObjectName(this.m_ProductName, this.m_VariantName, ThisCustomer.LocaleSetting);
            }
        }

        /// <summary>
        /// gets the Chosen Size Display Format
        /// </summary>
        public string ChosenSizeDisplayFormat
        {
            get
            {
                string sizeDisplay = ChosenSize;
                bool VatIsOn = AppLogic.AppConfigBool("VAT.Enabled") && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;

                if ((VatIsOn || ThisCustomer.CurrencySetting != Localization.StoreCurrency()) || sizeDisplay.IndexOf("[") > -1)
                {
                    decimal SzPrDelta = AppLogic.GetColorAndSizePriceDelta("", sizeDisplay, this.m_TaxClassID, ThisCustomer, true, true);
                    string prMod = CommonLogic.IIF(SzPrDelta > 0, "+", "") + ThisCustomer.CurrencyString(SzPrDelta);
                    if (sizeDisplay.IndexOf("[") > -1)
                    {
                        sizeDisplay = sizeDisplay.Substring(0, sizeDisplay.IndexOf("[")) + " [" + prMod + "]";
                    }
                }
                return sizeDisplay;
            }
        }

        /// <summary>
        /// gets the Chosen Color Display Format
        /// </summary>
        public string ChosenColorDisplayFormat
        {
            get
            {
                string colorDisplay = ChosenColor;
                bool VatIsOn = AppLogic.AppConfigBool("VAT.Enabled") && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT;
                
                if ((VatIsOn || ThisCustomer.CurrencySetting != Localization.StoreCurrency()) || colorDisplay.IndexOf("[") > -1)
                {
                    decimal ClrPrDelta = AppLogic.GetColorAndSizePriceDelta("", colorDisplay, this.m_TaxClassID, ThisCustomer, true, true);
                    string prMod = CommonLogic.IIF(ClrPrDelta > 0, "+", "") + ThisCustomer.CurrencyString(ClrPrDelta);
                    if (colorDisplay.IndexOf("[") > -1)
                    {
                        colorDisplay = colorDisplay.Substring(0, colorDisplay.IndexOf("[")) + " [" + prMod + "]";
                    }
                }
                return colorDisplay;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string LineItemShippingDisplay
        {
            get { return GetLineItemShippingDisplay(ShowMultiShipAddressUnderItemDescription); }
        }
    
        /// <summary>
        /// 
        /// </summary>
        public string TextOptionDisplayFormat
        {
            get
            {
                return HttpContext.Current.Server.HtmlDecode(TextOption);
            }
        }

        public decimal RegularPrice
        {
            get
            {
                return m_computedRegularPrice;
            }
        }

        /// <summary>
        /// Gets the extended price
        /// </summary>
        public decimal ExtPrice
        {
            get
            {
                return m_computedExtPriceRate;
            }
        }

        /// <summary>
        /// Gets the Formatted Regular Price for Display
        /// </summary>
        public string RegularPriceRateDisplayFormat
        {
            get
            {
                decimal displayPrice = this.RegularPrice;

				if (this.m_ThisShoppingCart.VatEnabled && this.m_ThisShoppingCart.VatIsInclusive && AppLogic.AppConfigBool("VAT.RoundPerItem"))
					displayPrice = (this.Price + Prices.GetVATPrice(this.Price, 1, this.ThisCustomer, this.TaxClassID)) * this.Quantity;

                string vatDetails = string.Empty;

                if (ThisShoppingCart.VatEnabled)
                {
                    if (ThisShoppingCart.VatIsInclusive)
                    {
                        if (this.IsTaxable)
                        {
                            vatDetails = AppLogic.GetString("setvatsetting.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                    }
                    else
                    {
                        if (this.IsTaxable)
                        {
                            vatDetails = AppLogic.GetString("setvatsetting.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                    }
                }

                string priceText = Localization.CurrencyStringForDisplayWithExchangeRate(displayPrice, ThisCustomer.CurrencySetting);

                return string.Format("{0} {1}", priceText, vatDetails);
            }
        }

        /// <summary>
        /// Gets the Formatted Extended Price for Display
        /// </summary>
        public string ExtPriceRateDisplayFormat
        {
            get
            {
                decimal displayPrice = this.ExtPrice;
                string vatDetails = string.Empty;

                if (ThisShoppingCart.VatEnabled)
                {
                    if (ThisShoppingCart.VatIsInclusive)
                    {
                        if (this.IsTaxable)
                        {
                            vatDetails = AppLogic.GetString("setvatsetting.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                    }
                    else
                    {
                        if (this.IsTaxable)
                        {
                            vatDetails = AppLogic.GetString("setvatsetting.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                    }
                }

                string priceText = Localization.CurrencyStringForDisplayWithExchangeRate(displayPrice, ThisCustomer.CurrencySetting);

                return string.Format("{0} {1}", priceText, vatDetails);
            }
        }

        /// <summary>
        /// Gets the Vat Rate
        /// </summary>
        public decimal VatRate
        {
            get
            {
                return m_computedVatRate;
            }
        }

        /// <summary>
        /// Gets the Formatted VAT rate
        /// </summary>
        public string VatRateDisplayFormat
        {
            get
            {
                string vatText = string.Empty;
                string vatDisplay = string.Empty;
                if (ThisShoppingCart.VatEnabled)
                {
                    if (this.m_IsTaxable)
                    {

                        vatText = Localization.CurrencyStringForDisplayWithExchangeRate(VatRate, ThisCustomer.CurrencySetting);
                    }
                    else
                    {
                        vatText = AppLogic.GetString("order.cs.9", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }

                vatDisplay = string.Format("{0} {1}", AppLogic.GetString("shoppingcart.aspx.17", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), vatText);

                return vatDisplay;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string LineItemQuantityDiscount
        {
            get
            {
                return GetLineItemQuantityDiscount();
            }
        }

        private bool m_minimumquantityudpated;
        /// <summary>
        /// 
        /// </summary>
        public bool MinimumQuantityUdpated
        {
            get { return m_minimumquantityudpated; }
            set { m_minimumquantityudpated = value; }
        }

        private bool m_showmultishipaddressunderitemdescription;

        /// <summary>
        /// 
        /// </summary>
        public bool ShowMultiShipAddressUnderItemDescription
        {
            get { return m_showmultishipaddressunderitemdescription; }
            set { m_showmultishipaddressunderitemdescription = value; }
        }
        #endregion

        #region Methods
        #region ComputeRates
        /// <summary>
        /// Computes the price and tax rates
        /// </summary>
        public void ComputeRates()
        {
            ComputeTaxRate();
            ComputePriceRate();
            ComputeVat();
        }

        #endregion

        #region ComputeTaxRate
        /// <summary>
        /// Gets the tax rate
        /// </summary>
        private void ComputeTaxRate()
        {
            if (this.IsTaxable)
            {
                m_computedTaxRate = Prices.TaxRate(ThisCustomer, this.TaxClassID);
            }
        }

        #endregion

        #region ComputePriceRate
        /// <summary>
        /// Computes the price rate
        /// </summary>
        private void ComputePriceRate()
        {
            List<CouponObject> emptyList = new List<CouponObject>();
            m_computedRegularPrice = Prices.LineItemPrice(this, emptyList, ThisShoppingCart.CartItems.QuantityDiscountList, ThisCustomer);
            m_computedExtPriceRate = Prices.LineItemPrice(this, ThisShoppingCart.CartItems.CouponList, ThisShoppingCart.CartItems.QuantityDiscountList, ThisCustomer);
        }

        #endregion

        #region ComputeVat
        /// <summary>
        /// Computes the VAT
        /// </summary>
        private void ComputeVat()
        {
            m_computedVatRate = Prices.LineItemVAT(this, ThisShoppingCart.CartItems.CouponList, ThisShoppingCart.CartItems.QuantityDiscountList, ThisCustomer);
        }

        #endregion


        /// <summary>
        /// Increments the Quantity of this CartItem
        /// </summary>
        /// <param name="quantity"></param>
        public void IncrementQuantity(int quantity)
        {

            SqlParameter[] spa = {DB.CreateSQLParameter("@ProductID", SqlDbType.Int, 4, m_ProductID, ParameterDirection.Input),
                                  DB.CreateSQLParameter("@VariantID", SqlDbType.Int, 4, m_VariantID, ParameterDirection.Input),
                                  DB.CreateSQLParameter("@ShoppingCartRecID", SqlDbType.Int, 4, m_ShoppingCartRecordID, ParameterDirection.Input),
                                  DB.CreateSQLParameter("@Quantity", SqlDbType.Int, 4, quantity, ParameterDirection.Input),
                                  DB.CreateSQLParameter("@NewQuantity", SqlDbType.Int, 4, null, ParameterDirection.Output)
                                 };
            this.m_Quantity = DB.ExecuteStoredProcInt("dbo.aspdnsf_UpdateCartItemQuantity", spa, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetLineItemProductPicURL()
        {
            String ThePic = AppLogic.LookupImage("Variant", VariantID, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
			
            if (ThePic.Length == 0 || ThePic.IndexOf("nopicture") != -1)
            {

                if (ChosenColor == "")
                {
                    ThePic = AppLogic.LookupImage("Product", ProductID, m_ImageFileNameOverride, SKU, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                else
                {
                    ThePic = AppLogic.LookupProductImageByNumberAndColor(ProductID, ThisCustomer.SkinID, m_ImageFileNameOverride, SKU, ThisCustomer.LocaleSetting, 1, AppLogic.RemoveAttributePriceModifier(ChosenColor), "icon");
                    //if (ThePic.Contains("nopictureicon"))
                    //{
                    //    ThePic = AppLogic.LookupImage("Product", ProductID, m_ImageFileNameOverride, SKU, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    //}
                }
            }
            if (ProductID == AppLogic.MicropayProductID)
            {
                ThePic = "images/spacer.gif"; // don't show pic for this
            }

            return ThePic;
        }


        /// <summary>
        /// Gets the Line item shipping Display
        /// </summary>
        /// <param name="ShowMultiShipAddressUnderItemDescription">if set to <c>true</c> [show multi ship address under item description].</param>
        /// <returns></returns>
        public String GetLineItemShippingDisplay(bool ShowMultiShipAddressUnderItemDescription)
        {
            CartItem c = this;
            StringBuilder tmpS = new StringBuilder(4096);

            if (c.m_IsDownload && !c.IsSystem)
            {
                tmpS.Append("");
                tmpS.Append(AppLogic.GetString("shoppingcart.cs.84", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
            }
            if (!c.m_IsDownload && !c.IsSystem && c.m_FreeShipping && !c.IsSystem && c.m_Shippable)
            {
                tmpS.Append("");
                tmpS.Append(AppLogic.GetString("shoppingcart.cs.104", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
            }

            //Check for requires shipping (FreeShipping=2)

            if ((ThisShoppingCart.HasMultipleShippingAddresses() || ThisShoppingCart.HasGiftRegistryComponents()) && ShowMultiShipAddressUnderItemDescription && !c.m_IsDownload && !c.IsSystem)
            {
                if (!c.IsAKit)
                {
                    tmpS.Append("");
                }

                //Make sure that the rates properly display for the EMail Gift Card products
                //and the products that don't require shipping

                if (c.m_Shippable)
                {
                    if (GiftCard.s_IsEmailGiftCard(c.m_ProductID))
                    {
                        tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.28", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                    }
                    else
                    {
                        tmpS.Append(AppLogic.GetString("shoppingcart.cs.87", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                        tmpS.Append(" ");
                        if (c.GiftRegistryForCustomerID != 0 && !Customer.OwnsThisAddress(ThisCustomer.CustomerID, c.m_ShippingAddressID))
                        {
                            tmpS.Append(AppLogic.GetString("checkoutshippingmult.aspx.15", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                        }
                        else
                        {
                            if (c.m_ShippingAddressID == ThisCustomer.PrimaryShippingAddressID)
                            {
                                tmpS.Append(AppLogic.GetString("account.aspx.32", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                            }
                            else if (c.m_ShippingAddressID == ThisCustomer.PrimaryBillingAddressID)
                            {
                                tmpS.Append(AppLogic.GetString("account.aspx.30", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                            }
                            Address adr = new Address();
                            adr.LoadFromDB(c.m_ShippingAddressID);
                            tmpS.Append("<div style=\"margin-left: 10px;\">");
                            tmpS.Append(adr.DisplayHTML(false));
                            tmpS.Append("</div>");
                        }
                        tmpS.Append("<div>");
                        tmpS.Append(AppLogic.GetString("order.cs.68", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));

                        tmpS.Append(c.m_ShippingMethod);

                        tmpS.Append("</div>");
                    }
                }
                else
                {
                    tmpS.Append(AppLogic.GetString("common.cs.88", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                }
            }
            return tmpS.ToString();
        }

        private string GetLineItemQuantityDiscount()
        {
            //Decimal PR = Prices.GetQuantityDiscount(this, ThisCustomer);

            QuantityDiscount.QuantityDiscountType fixedPriceDID = QuantityDiscount.QuantityDiscountType.Percentage;
            Decimal DIDPercent = QuantityDiscount.GetQuantityDiscountTablePercentageForLineItem(this, out fixedPriceDID);


            string quantityDiscounttxt = "";

            if (DIDPercent != 0.0M)
            {
                if (fixedPriceDID == QuantityDiscount.QuantityDiscountType.FixedAmount)
                {
                    quantityDiscounttxt = "(" + Localization.CurrencyStringForDisplayWithExchangeRate(DIDPercent, ThisCustomer.CurrencySetting) + " " + AppLogic.GetString("shoppingcart.cs.116", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + AppLogic.GetString("shoppingcart.cs.13", ThisCustomer.SkinID, ThisCustomer.LocaleSetting).Replace('%', ' ') + ")";
                }
                else
                {
                    quantityDiscounttxt = "(" + Localization.CurrencyStringForDBWithoutExchangeRate(DIDPercent) + AppLogic.GetString("shoppingcart.cs.13", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + ")";
                }
            }

            return quantityDiscounttxt;
        }

        /// <summary>
        /// Gets a copy of this CartItem
        /// </summary>
        /// <returns></returns>
        public CartItem Copy()
        {
            return new CartItem(this);
        }

        public Boolean MatchesComposition(CartItem ci)
        {
            if (ci.ProductID != this.ProductID || this.VariantID != ci.VariantID)
                return false;

            if (this.CustomerEntersPrice || ci.CustomerEntersPrice)
                return false;

            if (this.GiftRegistryForCustomerID > 0 || ci.GiftRegistryForCustomerID > 0)
                return false;

            if (!this.ChosenColor.Equals(ci.ChosenColor))
                return false;

            if (!this.ChosenSize.Equals(ci.ChosenSize))
                return false;

            if (!this.TextOption.Equals(ci.TextOption))
                return false;

            if (this.ShippingAddressID != ci.ShippingAddressID)
                return false;

            if ((this.IsAKit || ci.IsAKit) && !this.KitComposition.Matches(ci.KitComposition))
                return false;

            return true;
        }

        #endregion
    }

    /// <summary>
    /// Collection class for the CartItems
    /// </summary>
    public class CartItemCollection : List<CartItem>
    {
        #region Internal Variables

        internal List<CouponObject> m_couponlist;
        internal List<QDObject> m_quantitydiscountlist;

        #endregion

        #region Constructors

        public CartItemCollection() 
        {
            m_couponlist = new List<CouponObject>();
            m_quantitydiscountlist = new List<QDObject>();
        }

        public CartItemCollection(IEnumerable<CartItem> collection)
            : base(collection)
        {
            m_couponlist = new List<CouponObject>();
            m_quantitydiscountlist = new List<QDObject>();
        }

        public CartItemCollection(ShoppingCart cart)
        {
            m_couponlist = new List<CouponObject>();
            m_quantitydiscountlist = new List<QDObject>();

            this.Clear();

            foreach (CartItem ci in cart.CartItems)
            {
                this.Add(ci);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a list of <c>CouponObject</c> that belong to this <c>CartItemCollection</c>
        /// </summary>
        public List<CouponObject> CouponList
        {
            get { return m_couponlist; }
            set { m_couponlist = value; }
        }

        /// <summary>
        /// Gets or sets a list of <c>QDObject</c> that belong to this <c>CartItemCollection</c>
        /// </summary>
        public List<QDObject> QuantityDiscountList
        {
            get { return m_quantitydiscountlist; }
            set { m_quantitydiscountlist = value; }
        }
   
        /// <summary>
        ///  Gets whether or not the collection of shopping cart items have any discount results.
        /// </summary>
        public Boolean HasDiscountResults
        {
            get 
            { 
                var firstCartItem = this.FirstItem();

                return firstCartItem != null && firstCartItem.ThisShoppingCart != null
                    && firstCartItem.ThisShoppingCart.DiscountResults != null
                    && firstCartItem.ThisShoppingCart.DiscountResults.Count > 0;
            }
        }

        /// <summary>
        ///  Gets the list of discount results for the collection of shopping cart items.
        /// </summary>
        public IList<AspDotNetStorefront.Promotions.IDiscountResult> DiscountResults
        {
            get 
            {
                if (!HasDiscountResults)
                    return new List<AspDotNetStorefront.Promotions.IDiscountResult>();

                return this.FirstItem().ThisShoppingCart.DiscountResults;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>CartItemCollection</c> has free shipping <c>CartItem</c>
        /// </summary>
        public bool HasFreeShippingItems
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            get { return this.Where(ci => ci.FreeShipping).Count() > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>CartItemCollection</c> has download <c>CartItem</c>
        /// </summary>
        public bool HasDownloadItems
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            get { return this.Where(ci => ci.IsDownload).Count() > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether all <c>CartItem</c> in the <c>CartItemCollection</c> are email <c>GiftCard</c>
        /// </summary>
        public bool IsAllEmailGiftCards
        {
            get
            {
                // CartItemCollection inherits from List, so this.Count in C# doesn't directly
                // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
                return this.Where(ci => GiftCard.s_IsEmailGiftCard(ci.ProductID)).Count() == this.Count();
            }
        }

        /// <summary>
        /// Gets a value indicating whether any <c>CartItem</c> in the <c>CartItemCollection</c> is a download <c>CartItem</c>
        /// </summary>
        public bool HasDownloadComponents
        {
            get { return this.Exists(ci => ci.IsDownload); }
        }

        /// <summary>
        /// Gets a value indicating whether any <c>CartItem</c> in the <c>CartItemCollection</c> is a <c>KitProduct</c>
        /// </summary>
        public bool HasKitComponents
        {
            get { return this.Exists(ci => ci.IsAKit); }
        }

        /// <summary>
        /// Gets a value indicating if any <c>CartItem</c> in the <c>CartItemCollection</c> are taxable
        /// </summary>
        public bool HasTaxableComponents
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            get { return this.Where(ci => ci.IsTaxable).Count() > 0; }
        }

        /// <summary>
        /// Gets a value indicating if any <c>CartItem</c> in the <c>CartItemCollection</c> has free shipping
        /// </summary>
        public bool HasFreeShippingComponents
        {
            get { return Shipping.HasFreeShippingComponents(this); }
        }

        /// <summary>
        /// Gets a value indicating if any <c>CartItem</c> in the <c>CartItemCollection</c> is a micropay <c>CartItem</c>
        /// </summary>
        public bool HasMicropayProduct
        {
            get { return this.Exists(ci => ci.ProductID.Equals(AppLogic.MicropayProductID)); }
        }

        /// <summary>
        /// Gets a value indicating if any <c>CartItem</c> in the <c>CartItemCollection</c> is a system <c>CartItem</c>
        /// </summary>
        public bool HasSystemComponents
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            get { return this.Where(ci => ci.IsSystem).Count() > 0; }
        }

        /// <summary>
        /// Gets a value indicating if any <c>CartItem</c> in the <c>CartItemCollection</c> is a gift registry <c>CartItem</c> 
        /// </summary>
        public bool HasGiftRegistryComponents
        {
            get
            {
                // CartItemCollection inherits from List, so this.Count in C# doesn't directly
                // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
                return this.Where(ci => ci.GiftRegistryForCustomerID > 0).Count() > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating if any <c>CartItem</c> in the <c>CartItemCollection</c> has a distributor
        /// </summary>
        public bool HasDistributorComponents
        {
            get
            {
                // CartItemCollection inherits from List, so this.Count in C# doesn't directly
                // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
                return this.Where(ci => ci.DistributorID > 0).Count() > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether every <c>CartItem</c> in the <c>CartItemCollection</c> is a download <c>CartItem</c>
        /// </summary>
        public bool IsAllDownloadComponents
        {
            get
            {
                // CartItemCollection inherits from List, so this.Count in C# doesn't directly
                // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
                return this.Where(ci => ci.IsDownload).Count() == this.Count();
            }
        }

        /// <summary>
        /// Gets a value indicating whether every <c>CartItem</c> in the <c>CartItemCollection</c> has free shipping
        /// </summary>
        public bool IsAllFreeShippingComponents
        {
            get { return Shipping.IsAllFreeShippingComponents(this); }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>CartItemCollection</c> contains only <c>CartItem</c> that never require shipping
        /// </summary>
        public bool NoShippingRequiredComponents
        {
            get { return Shipping.NoShippingRequiredComponents(this); }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>CartItemCollection</c> contains only system <c>CartItem</c>
        /// </summary>
        public bool IsAllSystemComponents
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            get
            {
                return this.Where(ci => ci.IsSystem).Count() == this.Count();
            }
        }

        /// <summary>
        /// Gets a value indicating whether any <c>CartItem</c> in the <c>CartItemCollection</c> have a download location specified
        /// </summary>
        public bool ThereAreDownloadFilesSpecified
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            get { return this.Where(ci => ci.IsDownload && ci.DownloadLocation.Length > 0).Count() > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether any <c>CartItem</c> in the <c>CartItemCollection</c> is a <c>GiftCard</c>
        /// </summary>
        public bool ContainsGiftCard
        {
            get
            {
                // CartItemCollection inherits from List, so this.Count in C# doesn't directly
                // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
                return this.Where(ci => GiftCard.s_IsGiftCard(ci.ProductID)).Count() > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any <c>CartItem</c> in the <c>CartItemCollection</c> is recurring
        /// </summary>
        public bool ContainsRecurring
        {
            get
            {
                // CartItemCollection inherits from List, so this.Count in C# doesn't directly
                // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
                return this.Where(ci => ci.IsRecurring).Count() > 0;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the <c>CartItemCollection</c> contains <c>CartItem</c> that will ship to different addresses
        /// </summary>
        public bool HasMultipleShippingAddresses
        {
            get { return this.Select(ci => ci.ShippingAddressID).Distinct().Count() > 1; }
        }

#endregion

        #region Public Methods

        /// <summary>
        /// Determines the first <c>CartItem</c> in the <c>CartItemCollection</c>
        /// </summary>
        /// <returns>The first <c>CartItem</c> found in the <c>CartItemCollection</c></returns>
        public CartItem FirstItem()
        {
            // shouldn't call on an empty cart                              
            return this.FirstOrDefault();
        }

        /// <summary>
        /// Clones a <c>CartItemCollection</c>
        /// </summary>
        /// <returns>The cloned <c>CartItemCollection</c>"/></returns>
        public CartItemCollection Clone()
        {
            CartItem[] items = new CartItem[this.Count];
            this.CopyTo(items, 0);

            CartItemCollection cic = new CartItemCollection(items);
            cic.CouponList = m_couponlist;
            cic.QuantityDiscountList = m_quantitydiscountlist;

            return new CartItemCollection(items);
        }

        /// <summary>
        /// Locates a <c>CartItem</c> in the <c>CartItemCollection</c>
        /// </summary>
        /// <param name="cartRecordId"></param>
        /// <returns>A <c>CartItem</c> if one is found, otherwise null</returns>
        public CartItem Find(int cartRecordId)
        {
            foreach (CartItem item in this)
            {
                if (item.ShoppingCartRecordID == cartRecordId)
                {
                    return item;
                }
            }

            return null;
        }
 
        /// <summary>
        /// Converts the <c>CartItemCollection</c> to an ArrayList
        /// </summary>
        /// <returns>An ArrayList populated with the contents of the <c>CartItemCollection</c></returns>
        public ArrayList ToArrayList()
        {
            ArrayList al = new ArrayList();
            al.AddRange(this);
            return al;
        }

        /// <summary>
        /// Determines if the contents of the <c>CartItemCollection</c> contain gift registry <c>CartItem</c> belonging to another customer
        /// </summary>
        /// <param name="ThisCustomer"></param>
        /// <returns>True when the <c>CartItemCollection</c> has gift registry <c>CartItem</c> and the shipping address is not owned by the <c>CartItemCollection</c> owner, otherwise false</returns>
        public bool HasGiftRegistryAddresses(Customer ThisCustomer)
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            return this.Where(ci => ci.GiftRegistryForCustomerID > 0 && !ThisCustomer.OwnsThisAddress(ci.ShippingAddressID)).Count() > 0;
        }

        /// <summary>
        /// Determines how many <c>CartItem</c> in the <c>CartItemCollection</c> belong to a specific address
        /// </summary>
        /// <param name="ShippingAddressID"></param>
        /// <returns>The number of <c>CartItem</c> in the <c>CartItemCollection</c> tied to a specific shipping address id</returns>
        public int NumAtThisShippingAddress(int ShippingAddressID)
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            return this.Where(ci => ci.ShippingAddressID == ShippingAddressID).Count();
        }

        /// <summary>
        /// Determines the shipping address id of the first <c>CartItem</c> in the <c>CartItemCollection</c>
        /// </summary>
        /// <returns>The shipping address id of the first <c>CartItem</c> in the <c>CartItemCollection</c></returns>
        public int FirstItemShippingAddressID()
        {
			return this.Where(sa => sa != null)
				.Select(sa => sa.ShippingAddressID)
				.DefaultIfEmpty(0)
				.First();
        }

        /// <summary>
        /// Determines if all <c>CartItem</c> in the <c>CartItemCollection</c> are shipping to the default shipping address
        /// </summary>
        /// <param name="ThisCustomer">The <c>Customer</c> who owns this <c>CartItemCollection</c></param>
        /// <returns>True if all <c>CartItem</c> are shipping to the default shipping address, otherwise false</returns>
        public bool IsAllDefaultShippingAddressItems(Customer ThisCustomer)
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            return this.Where(ci => ci.ShippingAddressID == ThisCustomer.PrimaryShippingAddressID && ci.ShippingAddressID > 0).Count() == this.Count();
        }

        /// <summary>
        /// Determines if any of the items in the <c>CartItemCollection</c> are shipping to the primary shipping address
        /// </summary>
        /// <param name="ThisCustomer">The <c>Customer</c> who owns this <c>CartItemCollection</c></param>
        /// <returns>True if the <c>CartItemCollection</c> has a <c>CartItem</c> that is shipping to the primary shipping address, otherwise false</returns>
        public bool HasPrimaryShippingAddressItems(Customer ThisCustomer)
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            return this.Where(ci => ci.ShippingAddressID == ThisCustomer.PrimaryShippingAddressID).Count() > 0;
        }

        /// <summary>
        /// Determines if any of the items in the <c>CartItemCollection</c> are shipping to an address that is not the primary shipping address
        /// </summary>
        /// <param name="ThisCustomer">The <c>Customer</c> who owns this <c>CartItemCollection</c></param>
        /// <returns>True if the <c>CartItemCollection</c> has a <c>CartItem</c> that is not shipping to the primary shipping address, otherwise false</returns>
        public bool HasNonPrimaryShippingAddressItems(Customer ThisCustomer)
        {
            // CartItemCollection inherits from List, so this.Count in C# doesn't directly
            // translate to LINQ in VB.  Use .Where(Func predicate).Count() instead...
            return this.Where(ci => ci.ShippingAddressID != ThisCustomer.PrimaryShippingAddressID).Count() > 0;
        }

        public Decimal WeightTotal(IEnumerable<CartItem> itemList)
        {
            Decimal sum = 0.0M;

            foreach (CartItem c in itemList)
            {
                //Don't include items that do not require shipping except when FreeShippingAllowsRateSelection = true
                if (!c.IsDownload && c.Shippable)
                {
                    Decimal thisW = c.Weight;
                    if (Shipping.GetActiveShippingCalculationID() == Shipping.ShippingCalculationEnum.UseRealTimeRates)
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
                sum += AppLogic.AppConfigUSDecimal("PackageExtraWeight");
            }

            return sum;
        }

        #endregion
    }
}
