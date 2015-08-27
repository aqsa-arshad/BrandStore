// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Kit Product containing the kit groups and it's individual kit items
    /// </summary>
    public class KitProductData
    {
        /// <summary>
        /// KitProductData constructor
        /// </summary>
        public KitProductData()
        {
            Groups = new KitGroupDataCollection();
        }

        private int m_id;
        /// <summary>
        /// Gets or sets the Kit Id
        /// </summary>
        public int Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private string m_name;

        /// <summary>
        /// Gets or sets the Kit name
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private int m_variantid;

        /// <summary>
        /// Gets or sets the default variant id
        /// </summary>
        public int VariantId
        {
            get { return m_variantid; }
            set { m_variantid = value; }
        }

        private Boolean m_showbuybutton;

        /// <summary>
        /// Gets or sets whether or not to show the buy button
        /// </summary>
        public Boolean ShowBuyButton
        {
            get { return m_showbuybutton; }
            set { m_showbuybutton = value; }
        }

        private Boolean m_hidepriceuntilcart;

        /// <summary>
        /// Gets or sets whether or not to show the buy button
        /// </summary>
        public Boolean HidePriceUntilCart
        {
            get { return m_hidepriceuntilcart; }
            set { m_hidepriceuntilcart = value; }
        }

        private int m_shoppingcartrecordid;
        /// <summary>
        /// Gets the id on the shoppingcart table as line item for this kit product
        /// </summary>
        public int ShoppingCartRecordId
        {
            get { return m_shoppingcartrecordid; }
            set { m_shoppingcartrecordid = value; }
        }

        private Customer m_thiscustomer;

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public Customer ThisCustomer
        {
            get { return m_thiscustomer; }
            set { m_thiscustomer = value; }
        }

        private KitGroupDataCollection m_groups;

        /// <summary>
        /// Gets the kit groups
        /// </summary>
        public KitGroupDataCollection Groups
        {
            get { return m_groups; }
            set { m_groups = value; }
        }

        private string m_tempfilestub;

        /// <summary>
        /// Temporary file key for product upload kit items used as prefixe
        /// </summary>
        public string TempFileStub
        {
            get { return m_tempfilestub; }
            set { m_tempfilestub = value; }
        }

        private decimal m_regularbaseprice;

        /// <summary>
        /// Gets or sets the regular base price
        /// </summary>
        public decimal RegularBasePrice
        {
            get { return m_regularbaseprice; }
            set { m_regularbaseprice = value; }
        }

        private decimal m_baseprice;

        /// <summary>
        /// Gets or sets the regular price
        /// </summary>
        public decimal BasePrice
        {
            get { return m_baseprice; }
            set { m_baseprice = value; }
        }

        private decimal m_extendedprice;

        /// <summary>
        /// Gets or sets the regular price
        /// </summary>
        public decimal ExtendedPrice
        {
            get { return m_extendedprice; }
            set { m_extendedprice = value; }
        }
        private decimal m_actualbaseprice;

        /// <summary>
        /// Gets or sets the actual base price inclusive of readonly items if present
        /// </summary>
        public decimal ActualBasePrice
        {
            get { return m_actualbaseprice; }
            set { m_actualbaseprice = value; }
        }

        private decimal m_price;

        /// <summary>
        /// Gets or sets the price
        /// </summary>
        public decimal Price
        {
            get { return m_price; }
            set { m_price = value; }
        }

        private decimal m_salesprice;

        /// <summary>
        /// Gets or sets the sales price
        /// </summary>
        public decimal SalesPrice
        {
            get { return m_salesprice; }
            set { m_salesprice = value; }
        }

        private bool m_isdiscounted;

        /// <summary>
        /// Gets or sets whether this kit is discounted
        /// </summary>
        public bool IsDiscounted
        {
            get { return m_isdiscounted; }
            set { m_isdiscounted = value; }
        }

        private bool m_isextendedpricing;

        /// <summary>
        /// Gets or sets whether this kits price is based on extended pricing
        /// </summary>
        public bool IsExtendedPricing
        {
            get { return m_isextendedpricing; }
            set { m_isextendedpricing = value; }
        }

        private decimal m_customizedprice;
        /// <summary>
        /// Gets or sets the customized price
        /// </summary>
        public decimal CustomizedPrice
        {
            get { return m_customizedprice; }
            set { m_customizedprice = value; }
        }

        private bool m_hascustomerlevelpricing;

        /// <summary>
        /// Gets or sets if this kit has customer level pricing defined
        /// </summary>
        public bool HasCustomerLevelPricing
        {
            get { return m_hascustomerlevelpricing; }
            set { m_hascustomerlevelpricing = value; }
        }

        private decimal m_levelprice;
        /// <summary>
        /// Gets or sets the customer level price
        /// </summary>
        public decimal LevelPrice
        {
            get { return m_levelprice; }
            set { m_levelprice = value; }
        }

        private decimal m_taxrate;

        /// <summary>
        /// Gets or sets the tax rate
        /// </summary>
        public decimal TaxRate 
        {
            get { return m_taxrate; }
            set { m_taxrate = value; }
        }

        private int m_taxclassid;

        /// <summary>
        /// Gets or sets the tax class id
        /// </summary>
        public int TaxClassId
        {
            get { return m_taxclassid; }
            set { m_taxclassid = value; }
        }

        private decimal m_taxmultiplier;
        /// <summary>
        /// Gets or sets the tax multiplier. Used internally for tax computation
        /// </summary>
        private decimal TaxMultiplier
        {
            get { return m_taxmultiplier; }
            set { m_taxmultiplier = value; }
        }

        private decimal m_discount;

        /// <summary>
        /// Gets or sets the discount
        /// </summary>
        public decimal Discount
        {
            get { return m_discount; }
            set { m_discount = value; }
        }

        private decimal m_discountmultiplier;
        /// <summary>
        /// Gets or sets the discount multiplier. Used internally for discount computation
        /// </summary>
        private decimal DiscountMultiplier
        {
            get { return m_discountmultiplier; }
            set { m_discountmultiplier = value; }
        }

        private bool m_showtaxinclusive;

        /// <summary>
        /// Gets or sets whether to show the pricing as vat inclusive
        /// </summary>
        public bool ShowTaxInclusive
        {
            get { return m_showtaxinclusive; }
            set { m_showtaxinclusive = value; }
        }

        private bool m_hascartmapping;
        /// <summary>
        /// Gets or sets if this kit product has already been added to the cart
        /// </summary>
        public bool HasCartMapping
        {
            get { return m_hascartmapping; }
            set { m_hascartmapping = value; }
        }

        private string m_restrictedquantities;
        private int[] m_cachedrestrictedquantities;
        public int[] RestrictedQuantities
        {
            get 
            {
                if (m_cachedrestrictedquantities == null)
                {
                    char[] sep = new char[] { ',' };
                    string[] tmpQuant = m_restrictedquantities.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    m_cachedrestrictedquantities = new int[tmpQuant.Length];
                    for (int i = 0; i < tmpQuant.Length; i++)
                    {
                        if (!int.TryParse(tmpQuant[i], out m_cachedrestrictedquantities[i]))
                        {
                            return null;
                        }
                    }
                    
                }
                if (m_cachedrestrictedquantities.Length == 0)
                {
                    return null;
                }
                return m_cachedrestrictedquantities;
            }
        }

        /// <summary>
        /// Gets the count of kit products that has kit items that are mapped to a specified variant
        /// </summary>
        /// <param name="variantId">The variant id</param>
        /// <returns>The count of kit products</returns>
        public static int GetCountOfProductsThatHasKitItemsMappedToVariant(int variantId)
        {
            string countSql =
            string.Format(@"select count(distinct p.ProductId) as N
                            from kititem ki with(nolock)
                            inner join kitgroup kg with(nolock) on kg.KitGroupId = ki.KitGroupId
                            inner join Product p with(nolock) on p.IsAKit = 1 and kg.ProductId = p.ProductId
                            where ki.InventoryVariantId = {0}", variantId);

            return DB.GetSqlN(countSql);
        }

        /// <summary>
        /// Gets the list of kit products that has kit items that are mapped to a specified variant
        /// </summary>
        /// <param name="variantId"></param>
        /// <param name="ThisCustomer"></param>
        /// <returns></returns>
        public static List<KitProductData> GetProductsThatHasKitItemsMappedToVariant(int variantId, Customer ThisCustomer, string locale)
        {
            List<KitProductData> allProducts = new List<KitProductData>();
            

            // temporary storage just for the product ids
            List<int> productIds = new List<int>();

            string allIdsSql =
            string.Format(@"select distinct p.ProductId
                            from kititem ki with(nolock)
                            inner join kitgroup kg with(nolock) on kg.KitGroupId = ki.KitGroupId
                            inner join Product p with(nolock) on p.IsAKit = 1 and kg.ProductId = p.ProductId
                            where ki.InventoryVariantId = {0}", variantId);

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(allIdsSql, conn))
                {
                    while (rs.Read())
                    {
                        productIds.Add(DB.RSFieldInt(rs, "ProductID"));
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            foreach (int id in productIds)
            {
                KitProductData p = KitProductData.Find(id, ThisCustomer, locale);
                allProducts.Add(p);
            }

            return allProducts;
        }

        /// <summary>
        /// Creates a new kit group and add's it to group collection
        /// </summary>
        public KitGroupData ProvideNewGroup()
        {
            KitGroupData newGroup = this.Groups.FirstOrDefault(group => group.IsNew);
            if (newGroup != null)
            {
                return newGroup;
            }

            newGroup = new KitGroupData();
            newGroup.Kit = this;
            newGroup.Name = string.Empty;

            if (this.Groups.Count > 0)
            {
                newGroup.DisplayOrder = this.Groups.Max(group => group.DisplayOrder) + 1;
            }
            else
            {
                newGroup.DisplayOrder = 1;
            }
            
            newGroup.IsRequired = false;
            newGroup.SelectionControl = KitGroupData.SINGLE_SELECT_RADIO_LIST;
            
            this.Groups.Add(newGroup);

            // sort ascending by DisplayOrder
            this.Groups.Sort((g1, g2) => g1.DisplayOrder.CompareTo(g2.DisplayOrder));

            return newGroup;
        }

        /// <summary>
        /// Gets the selected items for addtocart
        /// </summary>
        public IEnumerable<KitItemData> SelectedItems
        {
            get
            {
                // if we have readonly groups, Everything is selected
                // otherwise we'll go one by one on each of them
                IEnumerable<KitItemData> items = this.Groups.SelectMany(group => group.IsReadOnly? group.Items : group.Items.Where(item => item.IsSelected));

                // make sure every items are switched as selected
                foreach (KitItemData kid in items)
                {
                    kid.IsSelected = true;
                }

                if (AppLogic.AppConfigBool("KitInventory.DisableItemSelection") ||
                    AppLogic.AppConfigBool("KitInventory.HideOutOfStock"))
                {
                    // only allow items that are either Un-Mapped
                    // or items that ARE mapped and has valid stock
                    IEnumerable<KitItemData> allowableItems = from item in items
                                         where !item.HasMappedVariant || (item.HasMappedVariant && item.VariantHasStock)
                                         select item;

                    return allowableItems;
                }
                else
                {
                    return items;
                }
            }
        }

        /// <summary>
        /// Moves the temporary images, used on the product page during kit selection and uploading images.
        /// Upon adding to cart, the temporary images will be moved to the proper filename and set as TextOption
        /// </summary>
        public void MoveAllTempImagesToOrdered()
        {
            IEnumerable<KitItemData> uploadItems = this.Groups
                .Where(group => group.SelectionControl == KitGroupData.FILE_OPTION)
                .SelectMany(group => group.Items);

            foreach (KitItemData kid in uploadItems)
            {
                kid.MoveTempImageAsFinalUploaded();
            }
        }

        /// <summary>
        /// Gets the kit group via the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public KitGroupData GetGroup(int id)
        {
            return this.Groups.FirstOrDefault(g => g.Id == id);
        }

        /// <summary>
        /// Gets the inventory count
        /// </summary>
        public int InventoryCount { get; private set; }

        /// <summary>
        /// Gets whether this kit has stock
        /// </summary>
        public bool HasStock 
        {
            get 
            {
                bool isInStock = !AppLogic.DetermineOutOfStock(this.Id, this.VariantId, false);

                // first this product needs to have stock first                
                if (this.HasRequiredGroups)
                {
                    return isInStock && AllRequiredGroupsHasProbableStocks();
                }
                else
                {
                    return isInStock;
                }
            }
        }

        /// <summary>
        /// Gets whether this kit has required kit groups
        /// </summary>
        public bool HasRequiredGroups
        {
            get { return this.Groups.Where(group => group.IsRequired).Count() > 0; }
        }

        /// <summary>
        /// Gets whether this kit's required kit groups has kit items that still has probable stock
        /// via it's kit item with highest inventory level
        /// </summary>
        /// <returns></returns>
        private bool AllRequiredGroupsHasProbableStocks()
        {
            bool allHasProbableStocks = true;

            foreach (KitGroupData group in Groups.Where( g => g.IsRequired ))
            {
                // let's check the kit item with the highest inventory level
                ProductVariantForKit highestInv = group.GetVariantWithHighestInventoryLevel();
                if (highestInv != null)
                {
                    bool probablyOutOfStock = AppLogic.DetermineOutOfStock(highestInv.ProductId, highestInv.Id, false);

                    // if highest mapped variant went out of stock
                    if (probablyOutOfStock)
                    {
                        // this group is considered out of stock
                        allHasProbableStocks = false;
                        break;
                    }
                }
            }

            return allHasProbableStocks;
        }

        /// <summary>
        /// If the appconfigs: KitInventory.DisableItemSelection or KitInventory.HideOutOfStock is turned on.
        /// Gets if this kit contains a kit group that is un-Oderable because it
        /// has all it's kit items out-of stock already, which renders this group unorderable
        /// </summary>
        public bool HasRequiredOrReadOnlyButUnOrderableKitGroup
        {
            get 
            {
                //bool hasUnOrderableGroup = false;

                KitGroupData anyUnorderableGroup = this.Groups.FirstOrDefault(group => group.HasUnOrderableKitItems);
                return anyUnorderableGroup != null;
            }
        }

        /// <summary>
        /// Gets a collection of distinct kit groups that has kit items mapped to this variant
        /// </summary>
        /// <param name="variantId"></param>
        /// <returns></returns>
        public IEnumerable<KitGroupData> GetGroupsWithItemsMappedToVariant(int variantId)
        {
            return this.Groups.FindAll(group => group.GetItemsMappedToVariant(variantId).Count() > 0);
        }

        /// <summary>
        /// Gets if this kit contains a file upload group
        /// </summary>
        public bool HasFileUploadGroup
        {
            get 
            {
                return Groups.Where(g => g.SelectionControl == KitGroupData.FILE_OPTION).Count() > 0;
            }
        }

        /// <summary>
        /// Computes the prices for this kit
        /// </summary>
        public void ComputePrices()
        {
            ComputePrices(1);
        }

        /// <summary>
        /// Computes the adjusted price for this kit based on the quantity
        /// </summary>
        /// <param name="quantity"></param>
        public void ComputePrices(int quantity)
        {
            ResolveRelativeDeltas();
            ComputeTotals();
            ComputeExtendedPrices(quantity);
            ApplyQuantityDiscountIfDefined(quantity);
        }

        /// <summary>
        /// Compares each kit item's delta price relative to the currently selected
        /// Kit item per group
        /// </summary>
        private void ResolveRelativeDeltas()
        {
            foreach (KitGroupData kgd in this.Groups)
            {
                kgd.ResolveRelativeDeltas();
            }
        }

        public decimal ReadOnlyItemsSum
        {
            get 
            {
                IEnumerable<KitItemData> readOnlyItems = this.SelectedItems.Where(item => item.Group.IsReadOnly);

                Func<KitItemData, decimal> computeKitItem = LineItemComputation;
                return readOnlyItems.Sum(computeKitItem);
            }
        }

        private decimal LineItemComputation(KitItemData item)
        {
            decimal deltaPrice = item.PriceDelta;

            // if customer's tax scheme is Vat-Inc
            if (this.ShowTaxInclusive)
            {
                // show vat inclusive
                deltaPrice *= this.TaxMultiplier;
            }

            return deltaPrice;
        }

        /// <summary>
        /// Computes the total prices for this kit
        /// </summary>
        private void ComputeTotals()
        {
            Func<KitItemData, decimal> computeKitItem = LineItemComputation;

            // get the sum of all kit items using the delegate function 
            // to compute the correct rate per item
            this.CustomizedPrice = this.BasePrice + this.SelectedItems.Sum(computeKitItem);

            // apply customer level discount pricing if has one
            if (this.HasCustomerLevelPricing)
            {
                // delegate function to compute discounted delta prices per kit item
                Func<KitItemData, decimal> computeDiscountedItem = (item) =>
                {
                    // reuse the delegate to get the normal computation
                    decimal discountedDelta = LineItemComputation(item);

                    // apply the discount
                    discountedDelta *= this.DiscountMultiplier;

                    // final discounted amount per item
                    return discountedDelta;
                };

                this.LevelPrice = this.BasePrice + this.SelectedItems.Sum(computeDiscountedItem);
            }
        }


        /// <summary>
        /// Applies the extended price for this kit
        /// </summary>
        /// <param name="quantity"></param>
        private void ComputeExtendedPrices(int quantity)
        {
            decimal qty = (decimal)quantity;
            this.Price *= qty;
            this.BasePrice *= qty;
            this.CustomizedPrice *= qty;
            this.LevelPrice *= qty;
        }

        /// <summary>
        /// Applies quantity discount for this kit if it has one defined
        /// </summary>
        /// <param name="quantity"></param>
        private void ApplyQuantityDiscountIfDefined(int quantity)
        {
            bool customerLevelAllowsQuantityDiscounts = QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID);
            if (customerLevelAllowsQuantityDiscounts)
            {
                QuantityDiscount.QuantityDiscountType qtyDiscountType = QuantityDiscount.QuantityDiscountType.None;
                decimal qtyDiscount = QuantityDiscount.GetQuantityDiscountTablePercentageWithoutCartAwareness(this.Id, quantity, out qtyDiscountType);

                if (!qtyDiscountType.Equals(QuantityDiscount.QuantityDiscountType.None))
                {
                    Decimal oldBasePrice = this.BasePrice;

                    this.BasePrice = Prices.GetQuantityDiscount(this.Id, quantity, this.BasePrice, qtyDiscountType, qtyDiscount, ThisCustomer);
                    this.CustomizedPrice = Prices.GetQuantityDiscount(this.Id, quantity, this.CustomizedPrice, qtyDiscountType, qtyDiscount, ThisCustomer);

                    if (this.HasCustomerLevelPricing)
                    {
                        this.LevelPrice = Prices.GetQuantityDiscount(this.Id, quantity, this.LevelPrice, qtyDiscountType, qtyDiscount, ThisCustomer); 
                    }

                    // check if the base price was reduced based on the quantity discount
                    // then reset to determine if this was discounted
                    this.IsDiscounted = oldBasePrice > this.BasePrice;
                }
            }
        }

        public IXPathNavigable AsXml()
        {
            StringBuilder xml = new StringBuilder();

            using (StringWriter sw = new StringWriter(xml))
            {
                using (XmlWriter writer = new XmlTextWriter(sw))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(KitProductData));
                    serializer.Serialize(writer, this);
                }
            }

            XmlDocument kitXml = new XmlDocument();
            kitXml.LoadXml(xml.ToString());
            return kitXml.DocumentElement;
        }

        public bool Validate(out string errorMessage)
        {
            bool isValid = true;
            errorMessage = string.Empty;

            foreach (KitGroupData group in this.Groups)
            {
                string groupErrorMessage = string.Empty;
                if (!group.Validate(out groupErrorMessage))
                {
                    isValid = false;
                    errorMessage += groupErrorMessage;
                    errorMessage += "";
                }
            }

            if (!isValid)
            {
                //errorMessage = "XmlPackage: product.newkitproduct.xml.config requires some groups to have default items  " + errorMessage;
                //"Kit Product requires some groups to have default items  ";
                Customer thisCustomer = Customer.Current;
                errorMessage = AppLogic.GetString("kitproduct.cs.10", thisCustomer.SkinID, thisCustomer.LocaleSetting) + errorMessage;
            }

            return isValid;
        }

        /// <summary>
        /// Gets the kit product via it's id
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="thisCustomer"></param>
        /// <returns></returns>
        public static KitProductData Find(int productId, Customer thisCustomer)
        {
            return Find(productId, thisCustomer, thisCustomer.LocaleSetting);
        }

        public static KitProductData Find(int productId, Customer thisCustomer, string locale)
        {
            return Find(productId, 0, thisCustomer, locale);
        }

        private static readonly KitProductData NOT_FOUND = null;

        public static KitProductData Find(int productId, int cartRecId, Customer thisCustomer)
        {
            return Find(productId, cartRecId, thisCustomer, thisCustomer.LocaleSetting);
        }

        /// <summary>
        /// Gets the kit product via it's id and cart record id if it's already added to the cart
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="cartRecId"></param>
        /// <param name="thisCustomer"></param>
        /// <returns></returns>
        public static KitProductData Find(int productId, int cartRecId, Customer thisCustomer, string locale)
        {
            bool exists = false;

            KitProductData kit = new KitProductData();
            kit.Id = productId;
            kit.ThisCustomer = thisCustomer;
            kit.ShoppingCartRecordId = cartRecId;

			string query = "exec dbo.aspdnsf_ProductInfo @ProductID = @ProductID, @CustomerLevelID = @CustomerLevelID, @DefaultVariantOnly = 1, @PublishedOnly = 0, @IsAdmin = 1";
			var sqlParameters = new[] 
				{
					new SqlParameter("@ProductID", productId),
					new SqlParameter("@CustomerLevelID", thisCustomer.CustomerLevelID),
				};

            Action<IDataReader> readAction = rs => 
            {
                exists = rs.Read();
                if (exists)
                {
                    kit.ExtendedPrice = DB.RSFieldDecimal(rs, "ExtendedPrice");
                    kit.Price = DB.RSFieldDecimal(rs, "Price");
                    kit.SalesPrice = DB.RSFieldDecimal(rs, "SalePrice");
                    kit.VariantId = DB.RSFieldInt(rs, "VariantId");
                    kit.ShowBuyButton = DB.RSFieldBool(rs, "ShowBuyButton");
                    kit.HidePriceUntilCart = DB.RSFieldBool(rs, "HidePriceUntilCart");

                    if (kit.SalesPrice > decimal.Zero && thisCustomer.CustomerLevelID == 0)
                    {
                        kit.BasePrice = kit.SalesPrice;
                        kit.IsDiscounted = true;
                    }
                    else
                    {
                        kit.BasePrice = kit.Price;
                        kit.IsDiscounted = false;
                    }

                    if (kit.ExtendedPrice > decimal.Zero)
                    {
                        kit.BasePrice = kit.ExtendedPrice;
                        kit.IsDiscounted = true;
                        kit.IsExtendedPricing = true;
                    }

                    kit.TaxClassId = DB.RSFieldInt(rs, "TaxClassId");
                }
            };

            DB.UseDataReader(query, sqlParameters, readAction);

            if (!exists)
            {
                return NOT_FOUND;
            }

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader reader = DB.GetRS(String.Format("select restrictedquantities from productvariant where variantid = {0}", kit.VariantId), dbconn))
                {
                    while (reader.Read())
                    {
                        kit.m_restrictedquantities = DB.RSField(reader, "restrictedquantities");
                    }
                }
            }

            if (Currency.GetDefaultCurrency() != thisCustomer.CurrencySetting)
            {
                kit.BasePrice = Decimal.Round(Currency.Convert(kit.BasePrice, Localization.StoreCurrency(), thisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
            }

            bool vatEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            kit.ShowTaxInclusive = (vatEnabled && thisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);
            
            kit.TaxRate = thisCustomer.TaxRate(kit.TaxClassId);
            kit.TaxMultiplier = 1M + (kit.TaxRate / 100M);

            if (kit.ShowTaxInclusive)
            {
                kit.BasePrice *= kit.TaxMultiplier;
            }

            kit.HasCustomerLevelPricing = thisCustomer.CustomerLevelID > 0;

            if (kit.HasCustomerLevelPricing)
            {
                kit.DiscountMultiplier = 1M - (thisCustomer.LevelDiscountPct / 100);

                if(!kit.IsExtendedPricing || thisCustomer.DiscountExtendedPrices)
                    kit.BasePrice *= kit.DiscountMultiplier;
            }

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader reader = DB.GetRS(String.Format("aspdnsf_getKitItems @ProductID={0}, @CartRecID={1}, @CustomerID={2}", productId, cartRecId, thisCustomer.CustomerID),dbconn))
                {
                    while (reader.Read())
                    {
                        kit.Name = DB.RSFieldByLocale(reader, "ProductName", locale);
                        int itemId = DB.RSFieldInt(reader, "KitItemID");
                        string itemName = DB.RSFieldByLocale(reader, "ItemName", locale);
                        decimal itemPricedelta = DB.RSFieldDecimal(reader, "ItemPriceDelta");
                        decimal itemWeightdelta = DB.RSFieldDecimal(reader, "ItemWeightDelta");
                        bool isDefault = DB.RSFieldBool(reader, "IsDefault");
                        bool isSelected = DB.RSFieldBool(reader, "IsSelected");
                        int groupId = DB.RSFieldInt(reader, "KitGroupID");
                        string groupName = DB.RSFieldByLocale(reader, "GroupName", locale);
                        int selectioncontrol = DB.RSFieldInt(reader, "SelectionControl");
                        int groupDisplayOrder = DB.RSFieldInt(reader, "GroupDisplayOrder");
                        bool isRequired = DB.RSFieldBool(reader, "IsRequired");
                        bool isReadOnly = DB.RSFieldBool(reader, "IsReadOnly");

                        string itemDescription = string.Empty;
                        if (!String.IsNullOrEmpty(DB.RSFieldByLocale(reader, "ItemDescription", locale)))
                        {
                            itemDescription = DB.RSFieldByLocale(reader, "ItemDescription", locale);
                        }

                        string groupDescription = string.Empty;
                        if (!String.IsNullOrEmpty(DB.RSFieldByLocale(reader, "GroupDescription", locale)))
                        {
                            groupDescription = DB.RSFieldByLocale(reader, "GroupDescription", locale);
                        }

                        string groupSummary = string.Empty;
                        if (!String.IsNullOrEmpty(DB.RSFieldByLocale(reader, "GroupSummary", locale)))
                        {
                            groupSummary = DB.RSFieldByLocale(reader, "GroupSummary", locale);
                        }

                        KitItemData item = new KitItemData();
                        item.Kit = kit;

                        //item.itemid = itemId;
                        item.Id = itemId;
                        item.IsDefault = isDefault;
                        item.Name = itemName;
                        item.Description = itemDescription;
                        item.PriceDelta = CommonLogic.IIF(Currency.GetDefaultCurrency() == thisCustomer.CurrencySetting, itemPricedelta, Decimal.Round(Currency.Convert(itemPricedelta, Localization.StoreCurrency(), thisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero));
                        item.WeightDelta = itemWeightdelta;
                        item.IsSelected = isSelected;
                        item.TextOption = DB.RSField(reader, "TextOption");
                        
                        item.DisplayOrder = DB.RSFieldInt(reader, "DisplayOrder");
                        item.InventoryVariantId = DB.RSFieldInt(reader, "InventoryVariantId");
                        item.InventoryQuantityDelta = DB.RSFieldInt(reader, "InventoryQuantityDelta");
                        item.InventoryVariantColor = DB.RSField(reader, "InventoryVariantColor");
                        item.InventoryVariantSize = DB.RSField(reader, "InventoryVariantSize");

                        KitGroupData group = kit.Groups.Find(groupId);
                        if (group == null)
                        {
                            group = new KitGroupData();
                            //group.groupid = groupId;
                            group.Id = groupId;
                            group.Name = groupName;
                            group.Description = groupDescription;
                            group.Summary = groupSummary;
                            group.IsRequired = isRequired;
                            group.IsReadOnly = isReadOnly;
                            group.DisplayOrder = groupDisplayOrder;
                            group.SelectionControl = selectioncontrol;
                            group.Kit = kit;

                            kit.Groups.Add(group);
                        }

                        item.Group = group;
                        group.Items.Add(item);
                    }
                }
            
            }

            // if this kit is already added to the cart, mapped the ones added on the kitcart table
            // and automatically treat them as selected per group
            if (cartRecId > 0)
            {
                Dictionary<int, List<int>> groupMappings = new Dictionary<int, List<int>>();
                string kitCartQuery = "SELECT KitGroupId, KitItemId FROM KitCart WITH (NOLOCK) WHERE CustomerId = {0} AND ShoppingCartRecID = {1} AND ProductId = {2}".FormatWith(thisCustomer.CustomerID, cartRecId, productId);
                Action<IDataReader> getMapAction = (rs) =>
                {
                    while (rs.Read())
                    {
                        int gId = rs.FieldInt("KitGroupId");
                        int kId = rs.FieldInt("KitItemId");
                        if (!groupMappings.ContainsKey(gId))
                        {
                            groupMappings.Add(gId, new List<int>());
                        }

                        List<int> selectedKits = groupMappings[gId];
                        selectedKits.Add(kId);
                    }
                };
                DB.UseDataReader(kitCartQuery, getMapAction);

                // now we have the mapping structure, let's 
                // reconcile it with our kit data
                foreach (int groupId in groupMappings.Keys)
                {
                    List<int> selectedKitIds = groupMappings[groupId];
                    KitGroupData group = kit.GetGroup(groupId);
                    if (group != null)
                    {
                        // First reset all the individual kit item's default selected if any
                        foreach (KitItemData kid in group.Items)
                        {
                            kid.IsSelected = false;
                        }

                        // now determine which ones should be selected
                        foreach (int id in selectedKitIds)
                        {
                            KitItemData kItem = group.GetItem(id);
                            if(kItem != null)
                            {
                                kItem.IsSelected = true;
                            }
                        }
                    }
                }

                kit.HasCartMapping = true;
            }

            // now fill-in the variants that has items 
            // which is mapped to variants, 
            // required kit groups should have every every item mapped to variants
            IEnumerable<KitItemData> mappedItems = kit.Groups.SelectMany(group => group.Items.Where(item => item.InventoryVariantId > 0));
            foreach(KitItemData item in mappedItems)
            {
                item.MappedVariant = ProductVariantForKit.Find(thisCustomer, item.InventoryVariantId);
            }

            return kit;
        }

        /// <summary>
        /// Delete's a particular kit group
        /// </summary>
        /// <param name="group"></param>
        public void DeleteGroup(KitGroupData group)
        {
            group.DeleteAllItems();
            DB.ExecuteSQL(string.Format("DELETE FROM dbo.KitGroup WHERE ProductId =  {0} AND KitGroupId = {1}", this.Id, group.Id));
            this.Groups.Remove(group);
        }
    }

    public class KitGroupDataCollection : List<KitGroupData>
    {
        public KitGroupData Find(string groupName)
        {
            foreach (KitGroupData group in this)
            {
                if (group.Name == groupName)
                {
                    return group;
                }
            }
            
            return null;
        }

        public KitGroupData Find(int id)
        {
            return this.FirstOrDefault(group => group.Id == id);
        }
    }

    /// <summary>
    /// Class for KitGroupType
    /// </summary>
    public class KitGroupType
    {
        private int m_id;
        private string m_name;

        /// <summary>
        /// Gets or sets the kitgrouptype id
        /// </summary>
        public int Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// Gets or sets the kitgrouptype name
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Gets all the kit group types
        /// </summary>
        /// <returns></returns>
        public static List<KitGroupType> GetAll()
        {
            List<KitGroupType> all = new List<KitGroupType>();

            string query = "select KitGroupTypeId,Name from KitGroupType   with (NOLOCK)  order by DisplayOrder,Name";

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(query, conn))
                {
                    while (rs.Read())
                    {
                        all.Add(new KitGroupType { Id = DB.RSFieldInt(rs, "KitGroupTypeId"), Name = DB.RSField(rs, "Name") });
                    }
                    rs.Close();
                    rs.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }

            return all;
        }
    }

    /// <summary>
    /// Interface that provides standard method for a validable object
    /// </summary>
    public interface IValidable
    {
        bool IsValid();
        void Validate();
        List<ValidationError> ValidationErrors();
    }

    /// <summary>
    /// Data structure c
    /// </summary>
    public class ValidationError
    {
        public ValidationError() 
        {
            PropertyName = string.Empty;
            ErrorMessage = string.Empty;
        }

        private string m_propertyname;
        private string m_errormessage;

        public string PropertyName
        {
            get { return m_propertyname; }
            set { m_propertyname = value; }
        }
        public string ErrorMessage
        {
            get { return m_errormessage; }
            set { m_errormessage = value; }
        }
    }

    /// <summary>
    /// The kit group
    /// </summary>
    public class KitGroupData : IValidable
    {
        public const int SINGLE_SELECT_DROPDOWN_LIST = 1;
        public const int SINGLE_SELECT_RADIO_LIST = 2;
        public const int MULTI_SELECT_RADIO_LIST = 3;
        public const int TEXT_OPTION = 4;
        public const int TEXT_AREA = 5;
        public const int FILE_OPTION = 6;

        /// <summary>
        /// Kit group constructor
        /// </summary>
        public KitGroupData()
        {
            Items = new List<KitItemData>(); //KitItemDataCollection();
            IsValid = true;
            ValidationErrors = new List<ValidationError>();
        }
        
        //public KitItemData defaultitem = new KitItemData();
        //public KitItemData selecteditem = new KitItemData();

        private int m_id;
        private string m_name;
        private string m_description;
        private string m_summary;
        private bool m_isrequired;
        private bool m_isreadonly;
        private int m_displayorder;
        private int m_selectioncontrol;
        private bool m_allowsave;

        /// <summary>
        /// The kit group id
        /// </summary>
        public int Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// The kit group name
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// The kit group description
        /// </summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        /// <summary>
        /// The kit group summary
        /// </summary>
        public string Summary
        {
            get { return m_summary; }
            set { m_summary = value; }
        }

        /// <summary>
        /// Gets or sets if this kit group is required
        /// </summary>
        public bool IsRequired
        {
            get { return m_isrequired; }
            set { m_isrequired = value; }
        }

        /// <summary>
        /// Gets or sets if this kit group is readonly
        /// </summary>
        public bool IsReadOnly
        {
            get { return m_isreadonly; }
            set { m_isreadonly = value; }
        }

        /// <summary>
        /// Gets or sets the kit group displayorder
        /// </summary>
        public int DisplayOrder
        {
            get { return m_displayorder; }
            set { m_displayorder = value; }
        }

        /// <summary>
        /// Gets or sets the selection control
        /// </summary>
        public int SelectionControl
        {
            get { return m_selectioncontrol; }
            set { m_selectioncontrol = value; }
        }

        private KitProductData m_kit;
        /// <summary>
        /// Gets or sets the owning kit product
        /// </summary>
        public KitProductData Kit
        {
            get { return m_kit; }
            set { m_kit = value; }
        }

        private List<KitItemData> m_items;

        /// <summary>
        /// Gets or sets the kit items
        /// </summary>
        public List<KitItemData> Items
        {
            get { return m_items; }
            set { m_items = value; }
        }

        private bool m_ismodified;
        /// <summary>
        /// Gets or sets if this group was modified. 
        /// Used as Helper property on the maintenance page, no mapping at all on DB
        /// </summary>
        public bool IsModified
        {
            get { return m_ismodified; }
            set { m_ismodified = value; }
        }

        /// <summary>
        /// If the appconfig: KitInventory.HideOutOfStock is turned on.
        /// Gets or sets the only kit items that can be selected
        /// </summary>
        public List<KitItemData> SelectableItems
        {
            get 
            {
                if (AppLogic.AppConfigBool("KitInventory.HideOutOfStock"))
                {
                    // only show or allow to be selected
                    // items that are either UN-mapped to a variant
                    // or mapped to a variant but HAS stock
                    IEnumerable<KitItemData> allowableItems = from item in Items
                                         where !item.HasMappedVariant || (item.HasMappedVariant && item.VariantHasStock)
                                         select item;

                    return allowableItems.ToList();
                }
                else
                {
                    return this.Items;
                }
            }
        }

        public bool HasUnOrderableKitItems
        {
            get 
            {
                bool isUnOrderable = false;

                if (this.IsReadOnly || this.IsRequired)
                {
                    if (AppLogic.AppConfigBool("KitInventory.DisableItemSelection") ||
                        AppLogic.AppConfigBool("KitInventory.HideOutOfStock"))
                    {
                        IEnumerable<KitItemData> mappedButNoStockItems = from item in this.Items
                                                    where item.HasMappedVariant && !item.VariantHasStock
                                                    select item;

                        // if we don't have at least 1 item that has no stock or mapping
                        // then we consider this group to be invalid
                        // hence renders the main kit Un-Orderable
                        if (mappedButNoStockItems.Count() == this.Items.Count)
                        {
                            isUnOrderable = true;
                        }
                    }
                }

                return isUnOrderable;
            }
        }

        /// <summary>
        /// Gets if this kit group is new
        /// </summary>
        public bool IsNew { get { return this.Id == 0; } }

        /// <summary>
        /// Gets or sets if this kit group can be saved
        /// </summary>
        public bool AllowSave 
        {
            get { return m_allowsave; }
            set { m_allowsave = value; }
        }
      
        /// <summary>
        /// Saves the kit group to the database
        /// </summary>
        /// <returns></returns>
        public bool Save(string locale)
        {
            Func<string, string, string> localize = (fieldName, value) =>
            {
                return AppLogic.FormLocaleXml(fieldName, value, locale, "KitGroup", this.Id);
            };

            SqlParameter[] sqlParams = {
                                new SqlParameter(){ ParameterName="@KitGroupID", Value= this.Id, SqlDbType = SqlDbType.Int},
                                new SqlParameter(){ ParameterName="@Name", Value= localize("Name", this.Name), SqlDbType = SqlDbType.NVarChar, Size=400},
                                new SqlParameter(){ ParameterName="@Description", Value= localize("Description", this.Description), SqlDbType = SqlDbType.NText},
                                new SqlParameter(){ ParameterName="@Summary", Value= localize("Summary", this.Summary), SqlDbType = SqlDbType.NText},
                                new SqlParameter(){ ParameterName="@ProductID", Value= this.Kit.Id, SqlDbType = SqlDbType.Int},
                                new SqlParameter(){ ParameterName="@DisplayOrder", Value= this.DisplayOrder, SqlDbType = SqlDbType.Int},
                                new SqlParameter(){ ParameterName="@KitGroupTypeID", Value= this.SelectionControl, SqlDbType = SqlDbType.Int},
                                new SqlParameter(){ ParameterName="@IsRequired", Value= this.IsRequired, SqlDbType = SqlDbType.Bit},   
                                new SqlParameter(){ ParameterName="@IsReadOnly", Value= this.IsReadOnly, SqlDbType = SqlDbType.Bit},   
                                new SqlParameter(){ ParameterName="@SavedID", Size=4,  SqlDbType = SqlDbType.Int, Direction= ParameterDirection.Output}};

            int id = DB.ExecuteStoredProcInt("dbo.aspdnsf_SaveKitGroup", sqlParams);
            if (id > 0)
            {
                this.Id = id;
            }
            
            foreach (KitItemData kid in Items)
            {
                kid.Save(locale);
            }

            return id > 0;
        }

        /// <summary>
        /// Gets or sets if this kit group has description
        /// </summary>
        public bool HasDescription
        {
            get { return !string.IsNullOrEmpty(this.Description); }
        }

        /// <summary>
        /// Gets or sets if this group has any kit items that has images uploaded
        /// </summary>
        public bool HasAnyKitItemWithImage
        {
            get { return this.Items.Any(item => item.HasImage); }
        }

        /// <summary>
        /// Gets sthe kit items with already uploaded images
        /// </summary>
        public IEnumerable<KitItemData> ItemsWithImage
        {
            get { return this.Items.Where(item => item.HasImage); }
        }

        /// <summary>
        /// Gets the kit items that are not new
        /// </summary>
        public IEnumerable<KitItemData> NonNewItems
        {
            get { return this.Items.Where(item => !item.IsNew); }
        }

        /// <summary>
        /// Gets if this kit group has any editable field, like text options
        /// </summary>
        public bool HasNoEditableField
        {
            get
            {
                return  SelectionControl == KitGroupData.SINGLE_SELECT_DROPDOWN_LIST ||
                        SelectionControl == KitGroupData.SINGLE_SELECT_RADIO_LIST ||
                        SelectionControl == KitGroupData.MULTI_SELECT_RADIO_LIST;
            }
        }

        /// <summary>
        /// Gets or sets the price delta for this group
        /// </summary>
        public decimal PriceDelta { get; private set; }

        /// <summary>
        /// Gets or sets the first selected kit item
        /// </summary>
        public KitItemData FirstSelectedItem
        {
            get 
            {
                KitItemData selectedItem = this.Items.FirstOrDefault(item => item.IsSelected == true);
                if (selectedItem == null)
                {
                    return this.Items[0];
                }

                return selectedItem;
            }
        }

        /// <summary>
        /// Resolves the individual kit item price deltas relative to the currently selected per group
        /// </summary>
        public void ResolveRelativeDeltas()
        {
            CompareItemDeltas();
        }

        /// <summary>
        /// Compares each items price delta relative to the currently selected per group
        /// This only applies to DropDown and Radio list selection groups
        /// </summary>
        private void CompareItemDeltas()
        {
            if (this.SelectionControl == SINGLE_SELECT_DROPDOWN_LIST ||
                this.SelectionControl == SINGLE_SELECT_RADIO_LIST)
            {
                KitItemData selectedItem = this.FirstSelectedItem;

                if (selectedItem != null)
                {
                    selectedItem.RelativePriceDelta = decimal.Zero;

                    if (selectedItem != null)
                    {
                        foreach (KitItemData kid in Items)
                        {
                            if (!kid.Equals(selectedItem))
                            {
                                decimal relativeDelta = kid.PriceDelta - selectedItem.PriceDelta;
                                kid.RelativePriceDelta = relativeDelta;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the kit item via it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public KitItemData GetItem(int id)
        {
            return this.Items.FirstOrDefault(item => item.Id == id);
        }

        /// <summary>
        /// Deletes all individual kit tems
        /// </summary>
        public void DeleteAllItems()
        {
            DB.ExecuteSQL(string.Format("DELETE FROM dbo.KitItem WHERE KitGroupId = {0}", this.Id));
            this.Items.Clear();
        }

        /// <summary>
        /// Deletes a kit item
        /// </summary>
        /// <param name="item"></param>
        public void DeleteItem(KitItemData item)
        {
            DB.ExecuteSQL(string.Format("DELETE FROM dbo.KitItem WHERE KitGroupId = {0} AND KitItemId = {1}", this.Id, item.Id));
            this.Items.Remove(item);
        }

        /// <summary>
        /// Creates a new kit item and add's it to the item collection
        /// </summary>
        public void ProvideNewKitItem()
        {
            bool alreadyContainsNew = this.Items.Where(item => item.IsNew).Count() > 0;
            if (alreadyContainsNew)
            {
                return;
            }

            KitItemData newKitItem = new KitItemData();
            newKitItem.Kit = this.Kit;
            newKitItem.Group = this;

            bool shouldBeDefault = this.Items.Count == 0;
            newKitItem.IsDefault = shouldBeDefault;

            // set the display order next to the last display order for this group
            if (this.Items.Count > 0)
            {
                newKitItem.DisplayOrder = this.Items.Max(item => item.DisplayOrder) + 1;
            }
            else
            {
                newKitItem.DisplayOrder = 1;
            }

            newKitItem.Name = string.Empty;
            this.Items.Add(newKitItem);

            // sort the items make sure that the new item is the last one
            this.Items.Sort((i1, i2) => i1.DisplayOrder.CompareTo(i2.DisplayOrder));
        }

        public bool Validate(out string errorMessage)
        {
            errorMessage = "";
            return true;
        }

        private void ClearDefaults()
        {
            DB.ExecuteSQL("UPDATE KitItem SET IsDefault = 0 WHERE KitGroupID = " + this.Id.ToString());
        }

        /// <summary>
        /// Gets the kit items mapped to the speicified variant id
        /// </summary>
        /// <param name="variantId"></param>
        /// <returns></returns>
        public IEnumerable<KitItemData> GetItemsMappedToVariant(int variantId)
        {
            return this.Items.FindAll(item => item.InventoryVariantId == variantId);
        }

        /// <summary>
        /// Gets the variant that's mapped to this group's kit items that has the highest inventory level
        /// </summary>
        /// <returns></returns>
        public ProductVariantForKit GetVariantWithHighestInventoryLevel()
        {
            List<KitItemData> mappedItems = this.Items.Where(item => item.HasMappedVariant).ToList();

            if (this.IsRequired && mappedItems.Count > 0)
            {
                ProductVariantForKit highestInv = null;                
                highestInv = this.Items[0].MappedVariant; // initial

                for (int ctr = 1; ctr < this.Items.Count; ctr++)
                {
                    KitItemData item = this.Items[ctr];
                    if (item.HasMappedVariant)
                    {
                        ProductVariantForKit currentVariant = item.MappedVariant;
                        if (highestInv != null && currentVariant.InventoryCount > highestInv.InventoryCount)
                        {
                            highestInv = currentVariant;
                        }
                    }
                }

                return highestInv;
            }
            
            return null;
        }

        private bool m_isvalid;
        /// <summary>
        /// Gets or sets if this kitgroup is valid for saving
        /// Checing this property should be made after a call to Validate
        /// </summary>
        public bool IsValid
        {
            get { return m_isvalid; }
            set { m_isvalid = value; }
        }

        /// <summary>
        /// Validates a kit group for business rules violations before saving
        /// </summary>
        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrEmpty(this.Name))
            {
                ValidationErrors.Add(new ValidationError() { PropertyName = "Name", ErrorMessage = "Group Name required" });
            }

            if (this.Items.Count == 0)
            {
                ValidationErrors.Add(new ValidationError() { ErrorMessage = "Group requires one or more kit items" });
            }
            else
            {
                if (this.SelectionControl == SINGLE_SELECT_DROPDOWN_LIST ||
                    this.SelectionControl == SINGLE_SELECT_RADIO_LIST)
                {
                    List<KitItemData> defaultItems = this.Items.FindAll(item => item.IsDefault);
                    if (defaultItems.Count == 0)
                    {
                        ValidationErrors.Add(new ValidationError() { PropertyName = "SelectionControl", ErrorMessage = "Selection control requires 1 default item" });
                    }
                    else if (defaultItems.Count > 1)
                    {
                        ValidationErrors.Add(new ValidationError() { PropertyName = "SelectionControl", ErrorMessage = "Selection control can have only 1 default item" });
                    }
                }
            }

            if (this.IsReadOnly)
            {
                // readonly group enforces every item to be mapped to a product variant
                // check if there are any item that's not mapped
                // let's do it only once so to save successive db calls
                this.Items.FirstOrDefault(item => {
                    ProductVariantForKit variant = ProductVariantForKit.Find(this.Kit.ThisCustomer, item.InventoryVariantId);
                    bool notMapped = variant == null;
                    if (notMapped)
                    {
                        ValidationErrors.Add(new ValidationError() { PropertyName = "IsReadOnly", ErrorMessage = "Read only group must have all kit items mapped to a particular variant" });
                    }

                    return notMapped;
                });
            }
            
            // call validation for each kit item
            foreach (KitItemData kid in Items)
            {
                kid.Validate();
            }

            // this group and ALL it's items should all be valid
            IsValid = ValidationErrors.Count.Equals(0) && this.Items.Where(item => !item.IsValid).Count().Equals(0);
        }

        private List<ValidationError> m_validationerrors;
        /// <summary>
        /// Gets the list of rule violations during valition
        /// </summary>
        public List<ValidationError> ValidationErrors
        {
            get { return m_validationerrors; }
            set { m_validationerrors = value; }
        }

        /// <summary>
        /// Delete's this group's image
        /// </summary>
        public void DeleteImage()
        {
            if (this.HasImage)
            {
                string imgPath = this.ImageFullPath;
                try
                {
                    File.Delete(imgPath);
                }
                catch { }
            }
        }

        /// <summary>
        /// Uploads an image for this kit group
        /// </summary>
        /// <param name="file"></param>
        public void UploadImage(HttpPostedFile file)
        {
            string extension = CommonLogic.ResolveExtensionFromMimeType(file.ContentType);

            if (extension != string.Empty)
            {
                string imgFileName = string.Format("kitgroup_{0}{1}", this.Id, extension);
                string imgFullPath = AppLogic.GetImagePath("product\\kit", string.Empty, true) + imgFileName;

                try
                {
                    if (CommonLogic.FileExists(imgFullPath))
                    {
                        System.IO.File.Delete(imgFullPath);
                    }
                    file.SaveAs(imgFullPath);
                }
                catch { }
            }
        }

        /// <summary>
        /// Gets this group's image path
        /// </summary>
        public string ImagePath
        {
            get { return DetermineImagePath(false); }
        }

        /// <summary>
        /// Gets the image full path for this group
        /// </summary>
        public string ImageFullPath
        {
            get { return DetermineImagePath(true); }
        }

        /// <summary>
        /// Determine's the image path for this group's image
        /// </summary>
        /// <param name="full"></param>
        /// <returns></returns>
        private string DetermineImagePath(bool full)
        {
            bool exists = false;
            return DetermineImagePath(full, out exists);
        }

        /// <summary>
        /// Determine's the image path for this group's image
        /// </summary>
        /// <param name="full"></param>
        /// <param name="exists"></param>
        /// <returns></returns>
        private string DetermineImagePath(bool full, out bool exists)
        {
            exists = false;

            string[] imgExtensions = CommonLogic.SupportedImageTypes;

            string imgFileName = string.Empty;
            string imgPath = string.Empty;
            string imgFullPath = string.Empty;

            foreach (string extension in imgExtensions)
            {
                imgFileName = string.Format("kitgroup_{0}{1}", this.Id, extension);
                imgPath = AppLogic.GetImagePath("product\\kit", string.Empty, false) + imgFileName;
                imgFullPath = AppLogic.GetImagePath("product\\kit", string.Empty, true) + imgFileName;

                if (CommonLogic.FileExists(imgFullPath))
                {
                    exists = true;
                    break;
                }
            }

            if (full)
            {
                return imgFullPath;
            }
            else
            {
                return imgPath;
            }
        }

        /// <summary>
        /// Gets if this kit group has image uploaded
        /// </summary>
        public bool HasImage
        {
            get 
            {
                bool exists = false;
                DetermineImagePath(false, out exists);
                return exists;
            }
        }

        #region IValidable Members

        bool IValidable.IsValid()
        {
            return m_isvalid;
        }

        void IValidable.Validate()
        {
            throw new NotImplementedException();
        }

        List<ValidationError> IValidable.ValidationErrors()
        {
            return m_validationerrors;
        }

        #endregion
    }

    /// <summary>
    /// KitItemData class
    /// </summary>
    public class KitItemData : IValidable
    {
        private int m_id;
        private string m_name;
        private string m_description;
        private bool m_selected;
        private bool m_isdefault;
        private decimal m_pricedelta;
        private decimal m_weightdelta;
        private string m_textoption = string.Empty;
        private int m_displayorder;
        private int m_inventoryvariantid;
        private int m_inventoryquantitydelta;
        private string m_inventoryvariantcolor;
        private string m_inventoryvariantsize;
        private KitProductData m_kit;
        private KitGroupData m_group;
        private decimal m_relativepricedelta;
        private ProductVariantForKit m_mappedvariant;

        /// <summary>
        /// KitItemData constructor
        /// </summary>
        public KitItemData()
        {
            IsValid = true;
            ValidationErrors = new List<ValidationError>();
        }

        /// <summary>
        /// Gets or sets the item id
        /// </summary>
        public int Id 
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name 
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description 
        {
            get { return m_description; }
            set { m_description = value; }
        }

        /// <summary>
        /// Gets or sets if this item is the default per group
        /// </summary>
        public bool IsDefault
        {
            get { return m_isdefault; }
            set { m_isdefault = value; }
        }

        /// <summary>
        /// Gets or sets if this kit item is currently selected on the product page
        /// </summary>
        public bool IsSelected
        {
            get { return m_selected; }
            set { m_selected = value; }
        }

        /// <summary>
        /// Gets or sets the price delta 
        /// </summary>
        public decimal PriceDelta
        {
            get { return m_pricedelta; }
            set { m_pricedelta = value; }
        }
        
        /// <summary>
        /// Gets or sets the weight delta
        /// </summary>
        public decimal WeightDelta
        {
            get { return m_weightdelta; }
            set { m_weightdelta = value; }
        }

        /// <summary>
        /// Gets or sets the text option
        /// </summary>
        public string TextOption
        {
            get { return m_textoption; }
            set { m_textoption = value; }
        }

        /// <summary>
        /// Gets or sets the displayorder
        /// </summary>
        public int DisplayOrder
        {
            get { return m_displayorder; }
            set { m_displayorder = value; }
        }

        /// <summary>
        /// Gets or sets whether this kit item is new
        /// </summary>
        public bool IsNew { get { return this.Id == 0; } }

        /// <summary>
        /// Gets or sets the inventory variant id
        /// </summary>
        public int InventoryVariantId
        {
            get { return m_inventoryvariantid; }
            set { m_inventoryvariantid = value; }
        }

        /// <summary>
        /// Gets or sets the inventory quantity delta
        /// </summary>
        public int InventoryQuantityDelta
        {
            get { return m_inventoryquantitydelta; }
            set { m_inventoryquantitydelta = value; }
        }

        /// <summary>
        /// Gets or sets the inventory variant color
        /// </summary>
        public string InventoryVariantColor
        {
            get { return m_inventoryvariantcolor; }
            set { m_inventoryvariantcolor = value; }
        }

        /// <summary>
        /// Gets or sets the inventory variant size
        /// </summary>
        public string InventoryVariantSize
        {
            get { return m_inventoryvariantsize; }
            set { m_inventoryvariantsize = value; }
        }

        /// <summary>
        /// Gets or sets the owining kit
        /// </summary>
        public KitProductData Kit
        {
            get { return m_kit; }
            set { m_kit = value; }
        }

        /// <summary>
        /// Gets or sets the owning kit group
        /// </summary>
        public KitGroupData Group
        {
            get { return m_group; }
            set { m_group = value; }
        }

        /// <summary>
        /// Gets or sets the price delta relative to the currently selected on the owning group
        /// </summary>
        public decimal RelativePriceDelta
        {
            get { return m_relativepricedelta; }
            set { m_relativepricedelta = value; }
        }

        /// <summary>
        /// Determines if the current relative price delta requires an addition to the kit's base price
        /// </summary>
        public bool RelativePriceDeltaIsAdd
        {
            get { return RelativePriceDelta >= decimal.Zero; }
        }

        /// <summary>
        /// Determines if the current relative price delta requires a subtraction to the kit's base price
        /// </summary>
        public bool RelativePriceDeltaIsSubtract
        {
            get { return !RelativePriceDeltaIsAdd; }
        }

        /// <summary>
        /// Gets the price delta display text
        /// </summary>
        public string PriceDeltaDisplayText
        {
            get 
            {
                return Localization.CurrencyStringForDisplayWithoutExchangeRate(this.PriceDelta, this.Kit.ThisCustomer.CurrencySetting);
            }
        }

        /// <summary>
        /// Gets or sets the mapped variant to this kit item
        /// </summary>
        public ProductVariantForKit MappedVariant
        {
            get { return m_mappedvariant; }
            set { m_mappedvariant = value; }
        }

        /// <summary>
        /// Gets or sets if this kit item can be selected
        /// </summary>
        public bool CanBeSelected
        {
            get             
            {
                // NOTE:
                // Purposely during binding, KitGroup.SelectableItems should already filterout
                // the items that has variants mapped but already out of stock
                if (AppLogic.AppConfigBool("KitInventory.DisableItemSelection"))
                {
                    if (this.HasMappedVariant)
                    {
                        // variant should have stock
                        return this.VariantHasStock;
                    }
                    else
                    {
                        // no mapped variant
                        return true;
                    }
                }
                else
                {
                    // no specific setting that can be toggled
                    // default behavior, just show
                    return true;
                }
            }
        }

        /// <summary>
        /// Gets if this kit item has a product variant mapped to it
        /// </summary>
        public bool HasMappedVariant 
        {
            get { return MappedVariant != null; } 
        }

        /// <summary>
        /// Gets if the currently mapped product variant has stock
        /// </summary>
        public bool VariantHasStock
        {
            get 
            {
                // call for this routine should be made
                // after checking the HasMappedVariant
                // it doesn't make sense to check for stock if this doesn't have variant
                if (this.HasMappedVariant)
                {
                    // Out of stock is determine by couple of other appconfigs
                    bool probablyOutOfStock = AppLogic.DetermineOutOfStock(this.MappedVariant.ProductId, this.MappedVariant.Id, false);
                    return !probablyOutOfStock;
                }
                
                return false;
            }
        }
        
        public void MakeDefault()
        {
            DB.ExecuteSQL("UPDATE KitItem SET IsDefault = 1 WHERE KitItemID = " + this.Id.ToString());
        }

        /// <summary>
        /// Saves this kit item
        /// </summary>
        /// <returns></returns>
        public bool Save(string locale)
        {
            Func<string, string, string> localize = (fieldName, value) =>
            {
                return AppLogic.FormLocaleXml(fieldName, value, locale, "KitItem", this.Id);
            };

            SqlParameter[] sqlParams = {
                new SqlParameter(){ ParameterName="@KitItemID", Value= this.Id, SqlDbType = SqlDbType.Int},
                new SqlParameter(){ ParameterName="@KitGroupID", Value= this.Group.Id, SqlDbType = SqlDbType.Int},
                new SqlParameter(){ ParameterName="@Name", Value= localize("Name", this.Name) , SqlDbType = SqlDbType.NVarChar, Size= 400 },
                new SqlParameter(){ ParameterName="@Description", Value= localize("Description", this.Description), SqlDbType = SqlDbType.NText},
                new SqlParameter(){ ParameterName="@PriceDelta", Value= this.PriceDelta, SqlDbType = SqlDbType.Money},
                new SqlParameter(){ ParameterName="@WeightDelta", Value= this.WeightDelta, SqlDbType = SqlDbType.Money},
                new SqlParameter(){ ParameterName="@IsDefault", Value= this.IsDefault, SqlDbType = SqlDbType.Bit},
                new SqlParameter(){ ParameterName="@DisplayOrder", Value= this.DisplayOrder, SqlDbType = SqlDbType.Int},
                new SqlParameter(){ ParameterName="@InventoryVariantID", Value= this.InventoryVariantId, SqlDbType = SqlDbType.Int},
                new SqlParameter(){ ParameterName="@InventoryQuantityDelta", Value= this.InventoryQuantityDelta, SqlDbType = SqlDbType.Int},
                new SqlParameter(){ ParameterName="@InventoryVariantColor", Value= this.InventoryVariantColor, SqlDbType = SqlDbType.NVarChar, Size= 100},
                new SqlParameter(){ ParameterName="@InventoryVariantSize", Value= this.InventoryVariantSize, SqlDbType = SqlDbType.NVarChar, Size= 100},
                new SqlParameter(){ ParameterName="@SavedID", Size=4,  SqlDbType = SqlDbType.Int, Direction= ParameterDirection.Output} };

            int id = DB.ExecuteStoredProcInt("dbo.aspdnsf_SaveKitItem", sqlParams);
            if (id > 0)
            {
                this.Id = id;
            }

            return id > 0;
        }

        /// <summary>
        /// Gets if this kit item's currently uploaded customer image is a temporary one
        /// </summary>
        public bool IsTempImage
        {
            get { return !string.IsNullOrEmpty(this.Kit.TempFileStub); }
        }

        /// <summary>
        /// Gets the customer uploaded image path
        /// </summary>
        public string CustomerUploadedImagePath
        {
            get
            {
                if (this.IsTempImage)
                {
                    return DetermineTempImagePath(false);
                }
                else
                {
                    return this.TextOption;
                }
            }        
        }

        /// <summary>
        /// Determines the temporary image path
        /// </summary>
        /// <param name="full"></param>
        /// <returns></returns>
        private string DetermineTempImagePath(bool full)
        {
            bool exists = false;
            return DetermineTempImagePath(full, out exists);
        }

        /// <summary>
        /// Determines the temporary image path
        /// </summary>
        /// <param name="full"></param>
        /// <param name="exists"></param>
        /// <returns></returns>
        private string DetermineTempImagePath(bool full, out bool exists)
        {
            exists = false;

            string[] validExtensions = { "gif", "jpg", "png" };

            string tempFileName = string.Empty;

            foreach (string ext in validExtensions)
            {
                tempFileName = string.Format("x-{0}_{1}_{2}_{3}.{4}",
                                this.Kit.TempFileStub, 
                                this.Kit.Id, 
                                this.Group.Id, 
                                this.Id, 
                                ext);

                string tempFileFullPath = AppLogic.GetImagePath("Orders", string.Empty, true) + tempFileName;

                exists = CommonLogic.FileExists(tempFileFullPath);
                if (exists)
                {
                    string returnPath = string.Empty;

                    if (full)
                    {
                        returnPath = tempFileFullPath;
                    }
                    else
                    {
                        // return relative
                        returnPath = AppLogic.GetImagePath("Orders", string.Empty, false) + tempFileName;
                    }
                    
                    return returnPath;
                }
            }

            return tempFileName;
        }

        /// <summary>
        /// Gets if this kit item has customer uploaded image
        /// </summary>
        public bool HasCustomerUploadedImage
        {
            get
            {
                if (IsTempImage)
                {
                    bool exists = false;
                    DetermineTempImagePath(false, out exists);

                    return exists;
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.TextOption))
                    {
                        return CommonLogic.FileExists(CommonLogic.SafeMapPath(this.TextOption));
                    }
                }
                
                return false;
            }
        }

        //public void SaveFile(HttpPostedFile file)
        //{
        //    if (file != null)
        //    {
        //        string extension = CommonLogic.ResolveExtensionFromMimeType(file.ContentType);

        //        if (extension != string.Empty)
        //        {
        //            string saveFileName = string.Empty;
        //            if (this.IsTempFile)
        //            {
        //                saveFileName = string.Format("x-{0}_{1}_{2}_{3}.{4}",
        //                                    this.Kit.TempFileStub,
        //                                    this.Kit.Id,
        //                                    this.Group.Id,
        //                                    this.Id,
        //                                    extension);
        //            }
        //            else
        //            {
        //                saveFileName = string.Format("{0}_{1}_{2}_{3}.{4}",
        //                                        this.Kit.ThisCustomer.CustomerID,
        //                                        this.Kit.Id,
        //                                        this.Id,
        //                                        DateTime.Now.ToString("yyyyMMddHHmmss"),
        //                                        extension);
        //            }
                    

        //            string saveImagePath = AppLogic.GetImagePath("Orders", string.Empty, false) + saveFileName;
        //            string saveImageFullPath = AppLogic.GetImagePath("Orders", string.Empty, true) + saveFileName;

        //            this.TextOption = saveImagePath;

        //            try
        //            {
        //                file.SaveAs(saveImageFullPath);
        //            }
        //            catch { }
        //        }
        //        else
        //        {
        //            this.TextOption = string.Empty;
        //        }
        //    }
        //}

        private bool m_isvalid;
        /// <summary>
        /// Gets if this kit item is valid for saving
        /// </summary>
        public bool IsValid
        {
            get { return m_isvalid; }
            set { m_isvalid = value; }
        }

        /// <summary>
        /// Validates the kit item if can be saved
        /// </summary>
        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrEmpty(this.Name))
            {
                ValidationErrors.Add(new ValidationError() { PropertyName = "Name", ErrorMessage = "Item name required" });
            }

            IsValid = ValidationErrors.Count == 0;
        }

        private List<ValidationError> m_validationerrors;
        /// <summary>
        /// Gets a list of rule violations during validation
        /// </summary>
        public List<ValidationError> ValidationErrors
        {
            get { return m_validationerrors; }
            set { m_validationerrors = value; }
        }

        /// <summary>
        /// Delete's the kit item's image
        /// </summary>
        public void DeleteImage()
        {
            if (this.HasImage)
            {
                string imgPath = this.ImageFullPath;
                try
                {
                    File.Delete(imgPath);
                }
                catch { }
            }
        }

        /// <summary>
        /// Moves the temporary image uploaded by the customer and renames it to the proper added to cart kit item name
        /// </summary>
        public void MoveTempImageAsFinalUploaded()
        {
            if (this.IsTempImage)
            {
                string tempPath = DetermineTempImagePath(true);

                if (File.Exists(tempPath))
                {
                    string ext = Path.GetExtension(tempPath);
                    string moveFileName = string.Format("{0}_{1}_{2}_{3}{4}",
                                            this.Kit.ThisCustomer.CustomerID,
                                            this.Kit.Id,
                                            this.Id,
                                            DateTime.Now.ToString("yyyyMMddHHmmss"),
                                            ext);

                    string moveFilePath = AppLogic.GetImagePath("Orders", string.Empty, false) + moveFileName;
                    string moveFileFullPath = AppLogic.GetImagePath("Orders", string.Empty, true) + moveFileName;

                    try
                    {
                        File.Move(tempPath, moveFileFullPath);

                        // relative file path
                        this.TextOption = moveFilePath;
                    }
                    catch{}
                }
            }
        }

        /// <summary>
        /// Uploads a customer image from the product page
        /// </summary>
        /// <param name="file"></param>
        public void UploadCustomerImage(HttpPostedFile file)
        {
            string extension = CommonLogic.ResolveExtensionFromMimeType(file.ContentType);
            string imgFileName = string.Empty;

            if (this.IsTempImage)
            {
                imgFileName = string.Format("x-{0}_{1}_{2}_{3}{4}",
                                    this.Kit.TempFileStub,
                                    this.Kit.Id,
                                    this.Group.Id,
                                    this.Id,
                                    extension);
            }
            else
            {
                imgFileName = string.Format("{0}_{1}_{2}_{3}{4}",
                                        this.Kit.ThisCustomer.CustomerID,
                                        this.Kit.Id,
                                        this.Id,
                                        DateTime.Now.ToString("yyyyMMddHHmmss"),
                                        extension);
            }

            this.TextOption = SaveImage("Orders", imgFileName, file);
        }

        /// <summary>
        /// Uploads the kit item image
        /// </summary>
        /// <param name="file"></param>
        public void UploadImage(HttpPostedFile hpfile)
        {
            string extension = CommonLogic.ResolveExtensionFromMimeType(hpfile.ContentType);
            string imgFileName = string.Format("kititem_{0}{1}", this.Id, extension);

            SaveImage("product\\kit", imgFileName, hpfile);
        }

        private string SaveImage(string folder, string fileName, HttpPostedFile hpfile)
        {
            string imgPath = AppLogic.GetImagePath(folder, string.Empty, true) + fileName;
            string imgFullPath = AppLogic.GetImagePath(folder, string.Empty, true) + fileName;

            try
            {
                if (CommonLogic.FileExists(imgFullPath))
                {
                    File.Delete(imgFullPath);
                }
                hpfile.SaveAs(imgFullPath);
            }
            catch { }

            return imgPath;
        }

        /// <summary>
        /// Gets the image path
        /// </summary>
        public string ImagePath
        {
            get { return DetermineImagePath(false); }
        }

        /// <summary>
        /// Gets the image full path
        /// </summary>
        public string ImageFullPath
        {
            get { return DetermineImagePath(true); }
        }

        /// <summary>
        /// Determines the image path
        /// </summary>
        /// <param name="full"></param>
        /// <returns></returns>
        private string DetermineImagePath(bool full)
        {
            bool exists = false;
            return DetermineImagePath(full, out exists);
        }

        /// <summary>
        /// Determine's the image path
        /// </summary>
        /// <param name="full"></param>
        /// <param name="exists"></param>
        /// <returns></returns>
        private string DetermineImagePath(bool full, out bool exists)
        {
            exists = false;

            string[] imgExtensions = CommonLogic.SupportedImageTypes;

            string imgFileName = string.Empty;
            string imgPath = string.Empty;
            string imgFullPath = string.Empty;

            foreach (string extension in imgExtensions)
            {
                imgFileName = string.Format("kititem_{0}{1}", this.Id, extension);
                imgPath = AppLogic.GetImagePath("product\\kit", string.Empty, false) + imgFileName;
                imgFullPath = AppLogic.GetImagePath("product\\kit", string.Empty, true) + imgFileName;

                if (CommonLogic.FileExists(imgFullPath))
                {
                    exists = true;
                    break;
                }
            }

            if (full)
            {
                return imgFullPath;
            }
            else
            {
                return imgPath;
            }
        }

        /// <summary>
        /// Gets if this kit item has an image
        /// </summary>
        public bool HasImage
        {
            get
            {
                bool exists = false;
                DetermineImagePath(false, out exists);
                return exists;
            }
        }

        /// <summary>
        /// Gets if this kit item has a description
        /// </summary>
        public bool HasDescription
        {
            get
            {
                return this.Description.Trim().Length > 0;
            }
        }

        #region IValidable Members

        bool IValidable.IsValid()
        {
            return m_isvalid;
        }

        void IValidable.Validate()
        {
            throw new NotImplementedException();
        }

        List<ValidationError> IValidable.ValidationErrors()
        {
            return m_validationerrors;
        }

        #endregion
    }

    public class ProductForKit
    {
        public ProductForKit()
        {
            Variants = new List<ProductVariantForKit>();
        }

        private int m_id;
        public int Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private string m_name;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private string m_description;
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private bool m_ispublished;
        public bool IsPublished
        {
            get { return m_ispublished; }
            set { m_ispublished = value; }
        }

        private bool m_isdeleted;
        public bool IsDeleted
        {
            get { return m_isdeleted; }
            set { m_isdeleted = value; }
        }

        public List<ProductVariantForKit> Variants { get; private set; }

        public const string NO_FILTER = "";
        public const int DONT_PAGE = -1;

        public static PaginatedList<ProductForKit> GetAll(Customer ThisCustomer,
            string alphaFilter,
            string searchFilter,
            int pageSize,
            int page)
        {
            Func<string, string> doLocalize = (str) => { return XmlCommon.GetLocaleEntry(str, ThisCustomer.LocaleSetting, true); };

            string unNamedVariant = AppLogic.GetString("admin.common.UnnamedVariant", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

            PaginatedList<ProductForKit> products = new PaginatedList<ProductForKit>();

            SqlCommand cmd = new SqlCommand("aspdnsf_GetProductWithVariants");
            cmd.CommandType = CommandType.StoredProcedure;

            // nullable parameters
            SqlParameter pAlphaFilter = new SqlParameter("@AlphaFilter", SqlDbType.NVarChar, 30) { Value = DBNull.Value };
            SqlParameter pSearchFilter = new SqlParameter("@SearchFilter", SqlDbType.NVarChar, 30) { Value = DBNull.Value };
            SqlParameter pPageSize = new SqlParameter("@PageSize", SqlDbType.Int) { Value = DBNull.Value };
            SqlParameter pPage = new SqlParameter("@Page", SqlDbType.Int) { Value = DBNull.Value };

            if (!string.IsNullOrEmpty(alphaFilter))
            {
                pAlphaFilter.Value = alphaFilter;
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                pSearchFilter.Value = searchFilter;
            }

            if (pageSize > 0 && page > 0)
            {
                pPageSize.Value = pageSize;
                pPage.Value = page;
            }

            cmd.Parameters.Add(pAlphaFilter);
            cmd.Parameters.Add(pSearchFilter);
            cmd.Parameters.Add(pPageSize);
            cmd.Parameters.Add(pPage);

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                cmd.Connection = conn;

                using (IDataReader rs = cmd.ExecuteReader())
                {
                    while (rs.Read())
                    {
                        // first result set is the paging data
                        if (rs.Read())
                        {
                            products.TotalCount = rs.FieldInt("TotalCount");
                            products.PageSize = rs.FieldInt("PageSize");
                            products.CurrentPage = rs.FieldInt("CurrentPage");
                            products.TotalPages = rs.FieldInt("TotalPages");
                            products.StartIndex = rs.FieldInt("StartIndex");
                            products.EndIndex = rs.FieldInt("EndIndex");
                        }

                        // next is the actual result set
                        rs.NextResult();


                        while (rs.Read())
                        {
                            ProductForKit p = new ProductForKit();
                            p.Id = rs.FieldInt("ProductID");
                            p.Name = doLocalize(rs.Field("Name"));
                            p.Description = doLocalize(rs.Field("Description"));
                            p.IsPublished = rs.FieldBool("Published");
                            p.IsDeleted = rs.FieldBool("Deleted");

                            products.Add(p);
                        }

                        rs.NextResult();

                        List<ProductVariantForKit> mappedVariants = new List<ProductVariantForKit>();

                        // next is the variants mapped to the product returned in one level list
                        // so that we only have to query the result set in one batch
                        while (rs.Read())
                        {
                            ProductVariantForKit v = new ProductVariantForKit();
                            v.Id = rs.FieldInt("VariantId");
                            v.ProductId = rs.FieldInt("ProductId");
                            v.IsDefault = rs.FieldBool("IsDefault");
                            v.IsPublished = rs.FieldBool("Published");
                            v.IsDeleted = rs.FieldBool("Deleted");
                            v.InventoryCount = rs.FieldInt("Inventory");
                            v.Price = rs.FieldDecimal("Price");
                            v.SalePrice = rs.FieldDecimal("SalePrice");
                            v.Weight = rs.FieldDecimal("Weight");
                            v.Description = rs.Field("Description");


                            v.Name = rs.Field("Name");
                            v.Name = string.IsNullOrEmpty(v.Name) ? unNamedVariant : v.Name;

                            mappedVariants.Add(v);
                        }

                        // since the variants are returned to us as one linear list, we'll have to manually
                        // associate them to their products
                        products.ForEach(p =>
                        {
                            // get all the mapped variants
                            List<ProductVariantForKit> pv = mappedVariants.FindAll(v => v.ProductId == p.Id);
                            p.Variants.AddRange(pv);
                        });

                        // we don't need this collection anymore after we've
                        // properly mapped the variants to their owning products
                        mappedVariants.Clear();
                    }
                }
                conn.Close();
                conn.Dispose();
            }

            return products;
        }

    }

    public class ProductVariantForKit
    {
        private int m_id;
        public int Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private int m_productid;
        public int ProductId
        {
            get { return m_productid; }
            set { m_productid = value; }
        }

        private string m_name;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private string m_description;
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private bool m_ispublished;
        public bool IsPublished
        {
            get { return m_ispublished; }
            set { m_ispublished = value; }
        }

        private bool m_isdeleted;
        public bool IsDeleted
        {
            get { return m_isdeleted; }
            set { m_isdeleted = value; }
        }

        private bool m_isdefault;
        public bool IsDefault
        {
            get { return m_isdefault; }
            set { m_isdefault = value; }
        }

        private int m_inventorycount;
        public int InventoryCount
        {
            get { return m_inventorycount; }
            set { m_inventorycount = value; }
        }

        private decimal m_price;
        public decimal Price
        {
            get { return m_price; }
            set { m_price = value; }
        }

        private decimal m_saleprice;
        public decimal SalePrice
        {
            get { return m_saleprice; }
            set { m_saleprice = value; }
        }

        private decimal m_weight;
        public decimal Weight 
        {
            get { return m_weight; }
            set { m_weight = value; }
        }

        public static ProductVariantForKit Find(Customer ThisCustomer, int variantId)
        {
            ProductVariantForKit v = null;

            string unNamedVariant = AppLogic.GetString("admin.common.UnnamedVariant", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

            string query = @"
                select  pv.VariantId, 
			            pv.ProductId, 
			            pv.[Name], 
			            pv.Description, 
			            pv.Published, 
			            pv.Deleted, 
			            pv.IsDefault, 
			            pv.Inventory, 
			            pv.Price, 
			            pv.SalePrice, 
			            pv.Weight
	                    from ProductVariant pv
                        where pv.VariantId = {0}"
                .FormatWith(variantId);

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(query, conn))
                {
                    if (rs.Read())
                    {
                        v = new ProductVariantForKit();
                        v.Id = rs.FieldInt("VariantId");
                        v.ProductId = rs.FieldInt("ProductId");
                        v.IsDefault = rs.FieldBool("IsDefault");
                        v.IsPublished = rs.FieldBool("Published");
                        v.IsDeleted = rs.FieldBool("Deleted");
                        v.InventoryCount = rs.FieldInt("Inventory");
                        v.Price = rs.FieldDecimal("Price");
                        v.SalePrice = rs.FieldDecimal("SalePrice");
                        v.Weight = rs.FieldDecimal("Weight");

                        v.Name = rs.Field("Name");
                        v.Name = string.IsNullOrEmpty(v.Name) ? unNamedVariant : v.Name;
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return v;
        }
    }
    
}
