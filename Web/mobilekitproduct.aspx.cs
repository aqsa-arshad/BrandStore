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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using Vortx.MobileFramework;

namespace AspDotNetStorefront
{
    public partial class mobilekitproduct : SkinBase
    {
        int ProductID;
        bool IsAKit;
        bool RequiresReg;
        String ProductName;
        private String m_XmlPackage;

        String CategoryName;
        String SectionName;
        String ManufacturerName;
        String DistributorName;
        String GenreName;
        String VectorName;

        int CategoryID;
        int SectionID;
        int ManufacturerID;
        int DistributorID;
        int GenreID;
        int VectorID;

        EntityHelper CategoryHelper;
        EntityHelper SectionHelper;
        EntityHelper ManufacturerHelper;
        EntityHelper DistributorHelper;
        EntityHelper GenreHelper;
        EntityHelper VectorHelper;

        string SourceEntity = "Category";
        int SourceEntityID = 0;
        int fromCartQuantity = 1;

        public KitProductData KitData { get; private set; }

        public string GetTempFileStub()
        {
            string name = GetTempFileStubName();
            return CommonLogic.FormCanBeDangerousContent(name);
        }

        public string GetTempFileStubName()
        {
            return string.Format("x$Kit${0}", this.KitData.Id);
        }

        private void GenerateTempFileStub(string value)
        {
            string name = GetTempFileStubName();
            litTempFileStub.Text = string.Format("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", name, value);
        }

        public override bool RequireScriptManager
        {
            get
            {
                return true;
            }
        }

        private void LoadThisKitData()
        {
            int productId = Request.QueryStringNativeInt("productId");
            int cartId = Request.QueryStringNativeInt("cartrecid");
            if (cartId > 0)
            {
                this.KitData = KitProductData.Find(productId, cartId, ThisCustomer);
                GetQuantityFromCart();
            }
            else
            {
                this.KitData = KitProductData.Find(productId, ThisCustomer);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            LoadThisKitData();

            if (this.KitData == null)
            {
                HttpContext.Current.Server.Transfer("pagenotfound.aspx");
            }

            if (KitData.HasFileUploadGroup)
            {
                string key = string.Empty;
                if (this.IsPostBack)
                {
                    key = GetTempFileStub();
                }
                else
                {
                    key = Guid.NewGuid().ToString().Substring(0, 7);
                    GenerateTempFileStub(key);
                }

                this.KitData.TempFileStub = key;
            }

            DetermineIfOrderable();
            BindData();

            if (this.KitData.HasCartMapping)
            {
                SetupCartLineItemDefaults();
            }

            if (!this.IsPostBack)
            {
                ProductID = CommonLogic.QueryStringUSInt("ProductID");
                CategoryID = CommonLogic.QueryStringUSInt("CategoryID");
                SectionID = CommonLogic.QueryStringUSInt("SectionID");
                ManufacturerID = CommonLogic.QueryStringUSInt("ManufacturerID");
                DistributorID = CommonLogic.QueryStringUSInt("DistributorID");
                GenreID = CommonLogic.QueryStringUSInt("GenreID");
                VectorID = CommonLogic.QueryStringUSInt("VectorID");

                SetupProductDefaults();
                RenderXmlPackageHeader();
            }

            if (this.KitData.RestrictedQuantities != null)
            {
                txtQuantity.Visible = false;
                foreach (int i in this.KitData.RestrictedQuantities)
                {
                    ddQuantity.Items.Add(new ListItem(i.ToString(), i.ToString()));
                }
                ddQuantity.Visible = true;
            }

            base.OnInit(e);
        }

        private void GetQuantityFromCart()
        {
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS("SELECT Quantity FROM ShoppingCart WHERE ShoppingCartRecId = {0}".FormatWith(KitData.ShoppingCartRecordId), conn))
                {
                    if (rs.Read())
                    {
                        fromCartQuantity = DB.RSFieldInt(rs, "Quantity");
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }
        }

        private void SetupCartLineItemDefaults()
        {
            btnAddToCart.Text = AppLogic.GetString("shoppingcart.cs.110", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            GetQuantityFromCart();
            txtQuantity.Text = fromCartQuantity.ToString();
            ddQuantity.SelectedValue = fromCartQuantity.ToString();
        }

        private void RenderXmlPackageHeader()
        {
            using (XmlPackage2 p = new XmlPackage2("mobile.kitheader.xml.config", ThisCustomer, SkinID, "", "EntityName=" + SourceEntity + "&EntityID=" + SourceEntityID.ToString() + CommonLogic.IIF(CommonLogic.ServerVariables("QUERY_STRING").IndexOf("cartrecid") != -1, "&cartrecid=" + CommonLogic.QueryStringUSInt("cartrecid").ToString(), "&showproduct=1"), String.Empty, true))
            {
                litKitHeader.Text = AppLogic.RunXmlPackage(p, base.GetParser, ThisCustomer, SkinID, true, true);
            }
        }

        private void SetupProductDefaults()
        {
            String ActualSEName = string.Empty;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from Product  with (NOLOCK)  where Deleted=0 and ProductID=" + ProductID.ToString(), dbconn))
                {
                    if (!rs.Read())
                    {
                        HttpContext.Current.Server.Transfer("pagenotfound.aspx");
                    }
                    else
                    {
                        bool a = DB.RSFieldBool(rs, "Published");
                        if (!a)
                        {
                            HttpContext.Current.Server.Transfer("pagenotfound.aspx");
                        }
                    }

                    String SENameINURL = CommonLogic.QueryStringCanBeDangerousContent("SEName");
                    ActualSEName = SE.MungeName(DB.RSField(rs, "SEName"));
                    if (ActualSEName != SENameINURL)
                    {
                        String NewURL = AppLogic.GetStoreHTTPLocation(false) + SE.MakeProductLink(ProductID, ActualSEName);

                        string QStr = "?";
                        bool first = true;
                        for (int i = 0; i < Request.QueryString.Count; i++)
                        {
                            string key = Request.QueryString.GetKey(i);
                            if ((key.Equals("productid", StringComparison.InvariantCultureIgnoreCase)) == false && (key.Equals("sename", StringComparison.InvariantCultureIgnoreCase)) == false)
                            {
                                if (!first)
                                {
                                    QStr += "&";
                                }
                                QStr += key + "=" + Request.QueryString[i].ToString();
                                first = false;
                            }
                        }
                        if (QStr.Length > 1)
                        {
                            NewURL += QStr;
                        }

                        HttpContext.Current.Response.Write("<html><head><title>Object Moved</title></head><body><b>Object moved to <a href=\"" + NewURL + "\">HERE</a></b></body></html>");
                        Response.Status = "301 Moved Permanently";
                        Response.AddHeader("Location", NewURL);
                        HttpContext.Current.Response.End();
                    }

                    m_XmlPackage = DB.RSField(rs, "XmlPackage").ToLowerInvariant();
                    IsAKit = DB.RSFieldBool(rs, "IsAKit");
                    if (m_XmlPackage.Length == 0)
                    {
                        if (IsAKit)
                        {
                            m_XmlPackage = AppLogic.ro_DefaultProductKitXmlPackage; // provide a default
                        }
                        else
                        {
                            m_XmlPackage = AppLogic.ro_DefaultProductXmlPackage; // provide a default
                        }
                    }
                    RequiresReg = DB.RSFieldBool(rs, "RequiresRegistration");
                    ProductName = DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting);

                    CategoryHelper = AppLogic.LookupHelper("Category", 0);
                    SectionHelper = AppLogic.LookupHelper("Section", 0);
                    ManufacturerHelper = AppLogic.LookupHelper("Manufacturer", 0);
                    DistributorHelper = AppLogic.LookupHelper("Distributor", 0);
                    GenreHelper = AppLogic.LookupHelper("Genre", 0);
                    VectorHelper = AppLogic.LookupHelper("Vector", 0);

                    String SEName = String.Empty;
                    if (DB.RSFieldByLocale(rs, "SETitle", ThisCustomer.LocaleSetting).Length == 0)
                    {
                        SETitle = Security.HtmlEncode(AppLogic.AppConfig("StoreName") + " - " + ProductName);
                    }
                    else
                    {
                        SETitle = DB.RSFieldByLocale(rs, "SETitle", ThisCustomer.LocaleSetting);
                    }
                    if (DB.RSFieldByLocale(rs, "SEDescription", ThisCustomer.LocaleSetting).Length == 0)
                    {
                        SEDescription = Security.HtmlEncode(ProductName);
                    }
                    else
                    {
                        SEDescription = DB.RSFieldByLocale(rs, "SEDescription", ThisCustomer.LocaleSetting);
                    }
                    if (DB.RSFieldByLocale(rs, "SEKeywords", ThisCustomer.LocaleSetting).Length == 0)
                    {
                        SEKeywords = Security.HtmlEncode(ProductName);
                    }
                    else
                    {
                        SEKeywords = DB.RSFieldByLocale(rs, "SEKeywords", ThisCustomer.LocaleSetting);
                    }
                    SENoScript = DB.RSFieldByLocale(rs, "SENoScript", ThisCustomer.LocaleSetting);
                }
            }

            //Log all views of unknown and registered customer
            if (AppLogic.AppConfigBool("DynamicRelatedProducts.Enabled") || AppLogic.AppConfigBool("RecentlyViewedProducts.Enabled"))
            {
                ThisCustomer.LogProductView(ProductID);
            }

            CategoryName = CategoryHelper.GetEntityName(CategoryID, ThisCustomer.LocaleSetting);
            SectionName = SectionHelper.GetEntityName(SectionID, ThisCustomer.LocaleSetting);
            ManufacturerName = ManufacturerHelper.GetEntityName(ManufacturerID, ThisCustomer.LocaleSetting);
            DistributorName = DistributorHelper.GetEntityName(DistributorID, ThisCustomer.LocaleSetting);
            GenreName = GenreHelper.GetEntityName(GenreID, ThisCustomer.LocaleSetting);
            VectorName = VectorHelper.GetEntityName(VectorID, ThisCustomer.LocaleSetting);

            String SourceEntityInstanceName = String.Empty;

            if (ManufacturerID != 0)
            {
                Profile.LastViewedEntityName = EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName;
                Profile.LastViewedEntityInstanceID = ManufacturerID.ToString();
                Profile.LastViewedEntityInstanceName = ManufacturerName;

                String NewURL = AppLogic.GetStoreHTTPLocation(false) + SE.MakeProductLink(ProductID, ActualSEName);
                HttpContext.Current.Response.Write("<html><head><title>Object Moved</title></head><body><b>Object moved to <a href=\"" + NewURL + "\">HERE</a></b></body></html>");
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", NewURL);
                HttpContext.Current.Response.End();
            }
            else if (DistributorID != 0)
            {
                Profile.LastViewedEntityName = EntityDefinitions.readonly_DistributorEntitySpecs.m_EntityName;
                Profile.LastViewedEntityInstanceID = DistributorID.ToString();
                Profile.LastViewedEntityInstanceName = DistributorName;

                String NewURL = AppLogic.GetStoreHTTPLocation(false) + SE.MakeProductLink(ProductID, ActualSEName);
                HttpContext.Current.Response.Write("<html><head><title>Object Moved</title></head><body><b>Object moved to <a href=\"" + NewURL + "\">HERE</a></b></body></html>");
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", NewURL);
                HttpContext.Current.Response.End();
            }
            else if (GenreID != 0)
            {
                Profile.LastViewedEntityName = EntityDefinitions.readonly_GenreEntitySpecs.m_EntityName;
                Profile.LastViewedEntityInstanceID = GenreID.ToString();
                Profile.LastViewedEntityInstanceName = GenreName;

                String NewURL = AppLogic.GetStoreHTTPLocation(false) + SE.MakeProductLink(ProductID, ActualSEName);
                HttpContext.Current.Response.Write("<html><head><title>Object Moved</title></head><body><b>Object moved to <a href=\"" + NewURL + "\">HERE</a></b></body></html>");
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", NewURL);
                HttpContext.Current.Response.End();
            }
            else if (VectorID != 0)
            {
                Profile.LastViewedEntityName = EntityDefinitions.readonly_VectorEntitySpecs.m_EntityName;
                Profile.LastViewedEntityInstanceID = VectorID.ToString();
                Profile.LastViewedEntityInstanceName = VectorName;

                String NewURL = AppLogic.GetStoreHTTPLocation(false) + SE.MakeProductLink(ProductID, ActualSEName);
                HttpContext.Current.Response.Write("<html><head><title>Object Moved</title></head><body><b>Object moved to <a href=\"" + NewURL + "\">HERE</a></b></body></html>");
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", NewURL);
                HttpContext.Current.Response.End();
            }
            else if (CategoryID != 0)
            {
                Profile.LastViewedEntityName = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
                Profile.LastViewedEntityInstanceID = CategoryID.ToString();
                Profile.LastViewedEntityInstanceName = CategoryName;

                String NewURL = AppLogic.GetStoreHTTPLocation(false) + SE.MakeProductLink(ProductID, ActualSEName);
                HttpContext.Current.Response.Write("<html><head><title>Object Moved</title></head><body><b>Object moved to <a href=\"" + NewURL + "\">HERE</a></b></body></html>");
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", NewURL);
                HttpContext.Current.Response.End();
            }
            else if (SectionID != 0)
            {
                Profile.LastViewedEntityName = EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName;
                Profile.LastViewedEntityInstanceID = SectionID.ToString();
                Profile.LastViewedEntityInstanceName = SectionName;

                String NewURL = AppLogic.GetStoreHTTPLocation(false) + SE.MakeProductLink(ProductID, ActualSEName);
                HttpContext.Current.Response.Write("<html><head><title>Object Moved</title></head><body><b>Object moved to <a href=\"" + NewURL + "\">HERE</a></b></body></html>");
                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", NewURL);
                HttpContext.Current.Response.End();
            }

            SourceEntity = Profile.LastViewedEntityName;
            SourceEntityInstanceName = Profile.LastViewedEntityInstanceName;
            SourceEntityID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.LastViewedEntityInstanceID), Profile.LastViewedEntityInstanceID, "0"));

            // validate that source entity id is actually valid for this product:
            if (SourceEntityID != 0)
            {
                String sqlx = "select count(*) as N from dbo.productentity with (NOLOCK) where ProductID=" + ProductID.ToString() + " and EntityID=" + SourceEntityID.ToString() + " and EntityType = " + DB.SQuote(SourceEntity);
                if (DB.GetSqlN(sqlx) == 0)
                {
                    SourceEntityID = 0;
                }
            }

            // we had no entity context coming in, try to find a category context for this product, so they have some context if possible:
            if (SourceEntityID == 0)
            {
                SourceEntityID = EntityHelper.GetProductsFirstEntity(ProductID, EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName);
                if (SourceEntityID > 0)
                {
                    CategoryID = SourceEntityID;
                    CategoryName = CategoryHelper.GetEntityName(CategoryID, ThisCustomer.LocaleSetting);

                    Profile.LastViewedEntityName = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
                    Profile.LastViewedEntityInstanceID = CategoryID.ToString();
                    Profile.LastViewedEntityInstanceName = CategoryName;

                    SourceEntity = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
                    SourceEntityInstanceName = CategoryName;
                }
            }

            // we had no entity context coming in, try to find a section context for this product, so they have some context if possible:
            if (SourceEntityID == 0)
            {
                SourceEntityID = EntityHelper.GetProductsFirstEntity(ProductID, EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName);
                if (SourceEntityID > 0)
                {
                    SectionID = SourceEntityID;
                    SectionName = CategoryHelper.GetEntityName(SectionID, ThisCustomer.LocaleSetting);

                    Profile.LastViewedEntityName = EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName;
                    Profile.LastViewedEntityInstanceID = SectionID.ToString();
                    Profile.LastViewedEntityInstanceName = SectionName;

                    SourceEntity = EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName;
                    SourceEntityInstanceName = SectionName;
                }
            }

            // we had no entity context coming in, try to find a Manufacturer context for this product, so they have some context if possible:
            if (SourceEntityID == 0)
            {
                SourceEntityID = EntityHelper.GetProductsFirstEntity(ProductID, EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName);
                if (SourceEntityID > 0)
                {
                    ManufacturerID = SourceEntityID;
                    ManufacturerName = CategoryHelper.GetEntityName(ManufacturerID, ThisCustomer.LocaleSetting);

                    Profile.LastViewedEntityName = EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName;
                    Profile.LastViewedEntityInstanceID = ManufacturerID.ToString();
                    Profile.LastViewedEntityInstanceName = ManufacturerName;

                    SourceEntity = EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName;
                    SourceEntityInstanceName = ManufacturerName;
                }
            }

            // build up breadcrumb if we need:
            SectionTitle = Breadcrumb.GetProductBreadcrumb(ProductID, ProductName, SourceEntity, SourceEntityID, ThisCustomer);
        }

        private void DetermineIfOrderable()
        {
            // only show prompt if appconfigs are turned on
            if (AppLogic.AppConfigBool("KitInventory.DisableItemSelection") ||
                AppLogic.AppConfigBool("KitInventory.HideOutOfStock"))
            {
                if (KitData.HasStock == false ||
                   KitData.HasRequiredOrReadOnlyButUnOrderableKitGroup) // .HasRequiredButUnOrderableKitGroup)
                {
                    pnlUnOrderableKit.Visible = true;
                    pnlAddToCart.Visible = false;
                }
            }
        }

        private void BindData()
        {
            int qty = GetQuantity();

            // reset qty if not valid input
            txtQuantity.Text = qty.ToString();

            this.KitData.ComputePrices(qty);
            if (AppLogic.ProductIsCallToOrder(this.KitData.Id))
            {
                pnlAddToCart.Visible = false;
                string Locale = Localization.GetDefaultLocale();
                litIsCallToOrder.Text = "<form style=\"margin-top: 0px; margin-bottom: 0px;\"><font class=\"CallToOrder\">" + AppLogic.GetString("common.cs.67", SkinID, Locale) + "</font></form>"; // use <form></form> to give same spacing that normal add to cart would have
            }
            rptKitGroups.DataSource = this.KitData.Groups;
            rptKitGroups.DataBind();

            DisplayPricing();

            ScriptManager scrptMgr = ScriptManager.GetCurrent(this);
            if (scrptMgr != null && scrptMgr.IsInAsyncPostBack)
            {
                // only highlight on postback
                AttachHighlightPriceScript();
            }            
        }

        public override void RegisterScriptAndServices(ScriptManager scrptMgr)
        {
            scrptMgr.RegisterPostBackControl(btnAddToCart);
            scrptMgr.RegisterPostBackControl(btnAddToGiftRegistry);
            scrptMgr.RegisterPostBackControl(btnAddToWishList);

            base.RegisterScriptAndServices(scrptMgr);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            MobileHelper.RedirectPageWhenMobileIsDisabled("~/kitproduct.aspx", ThisCustomer);

            if (this.IsPostBack)
            {
                ResolveTemplateGroupsSelection();
            }

            if (!AppLogic.AppConfigBool("ShowBuyButtons") || !KitData.ShowBuyButton || AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
            {
                pnlAddToCart.Visible = false;
            }
                    
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Form.Action = ResolveClientUrl(
                string.Format("~/mobilekitproduct.aspx?SEName={0}&ProductID={1}&cartrecid={2}",
                CommonLogic.QueryStringCanBeDangerousContent("SEName"),
                CommonLogic.QueryStringCanBeDangerousContent("ProductID"), 
                Request.QueryStringNativeInt("cartrecid")));
        }

        private void ResolveTemplateGroupsSelection()
        {
            rptKitGroups.ForEachItemTemplate(ResolveGroupSelection_ItemTemplateAction);
            BindData();
        }

        private void ResolveGroupSelection_ItemTemplateAction(RepeaterItem item)
        {
            BaseKitGroupControl ctrlKitGroupTemplate = item.FindControl<BaseKitGroupControl>("ctrlKitGroupTemplate");
            ctrlKitGroupTemplate.ResolveSelection();
        }

        protected string FormatCurrencyDisplay(decimal amount)
        {
            return Localization.CurrencyStringForDisplayWithoutExchangeRate(amount, ThisCustomer.CurrencySetting);
        }

        protected void txtQuantity_TextChanged(object sender, EventArgs e)
        {
        }

        private void DisplayPricing()
        {
            Func<string, decimal, string> displayPrice = (stringResourceKey, amount) =>
            {
                return string.Format("{0} {1}",
                    AppLogic.GetString(stringResourceKey, ThisCustomer.SkinID, ThisCustomer.LocaleSetting),
                    FormatCurrencyDisplay(amount));
            };

            int cartId = Request.QueryStringNativeInt("cartrecid");
            if (cartId > 0 && !IsPostBack)
            {
                //"Base Price: " 
                lblBasePrice.Text = displayPrice("kitproduct.aspx.13", (KitData.BasePrice + KitData.ReadOnlyItemsSum) * fromCartQuantity);

                //"Regular Base Price: " 
                lblRegularBasePrice.Text = displayPrice("kitproduct.aspx.16", KitData.Price * fromCartQuantity);

                // "Customized Price: "
                lblCustomizedPrice.Text = displayPrice("kitproduct.aspx.14", KitData.CustomizedPrice * fromCartQuantity);

                // "Level Price: "
                lblLevelPrice.Text = displayPrice("kitproduct.aspx.15", KitData.LevelPrice * fromCartQuantity);
            }
            else
            {
                //"Base Price: " 
                lblBasePrice.Text = displayPrice("kitproduct.aspx.13", KitData.BasePrice + KitData.ReadOnlyItemsSum);

                //"Regular Base Price: " 
                lblRegularBasePrice.Text = displayPrice("kitproduct.aspx.16", KitData.Price);

                // "Customized Price: "
                lblCustomizedPrice.Text = displayPrice("kitproduct.aspx.14", KitData.CustomizedPrice);

                // "Level Price: "
                lblLevelPrice.Text = displayPrice("kitproduct.aspx.15", KitData.LevelPrice);
            }
        }

        private void AttachHighlightPriceScript()
        {
            StringBuilder script = new StringBuilder();
            script.AppendFormat(" Sys.Application.add_load(function() {{ aspdnsf.Pages.KitPage.highlightPriceChange('{0}'); }});FixAddToCartInputs();", pnlPrice.ClientID);

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script.ToString(), true);
        }

        public string StringResource(string key)
        {
            return AppLogic.GetString(key, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
        }

        private KitComposition ComposeAddToCart()
        {
            KitComposition composition = new KitComposition(0);

            foreach (KitItemData selectedItem in KitData.SelectedItems)
            {
                KitCartItem kcItem = new KitCartItem();
                kcItem.ProductID = KitData.Id;
                kcItem.VariantID = KitData.VariantId;
                kcItem.KitGroupID = selectedItem.Group.Id;
                kcItem.KitItemID = selectedItem.Id;
                kcItem.CustomerID = ThisCustomer.CustomerID;
                kcItem.TextOption = selectedItem.TextOption;
                int qty = 1;
                if (selectedItem.HasMappedVariant && 
                    selectedItem.InventoryQuantityDelta> 1)
                {
                    qty = selectedItem.InventoryQuantityDelta;
                }

                kcItem.Quantity = qty;

                composition.Compositions.Add(kcItem);
            }

            return composition;
        }

        private int GetQuantity()
        {
            int qty;
            if (this.KitData.RestrictedQuantities != null)
            {
                if (int.TryParse(ddQuantity.SelectedValue, out qty))
                {
                    return qty;
                }
                return 1;
            }
            qty = txtQuantity.Text.ToNativeInt();
            qty = (qty <= 0) ? 1 : qty;

            return qty;
        }

        protected void ShowError(String message)
        {
            ltKitError.Text = message;
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            AddToCart(CartTypeEnum.ShoppingCart);
        }

        protected void btnAddToWishList_Click(object sender, EventArgs e)
        {
            AddToCart(CartTypeEnum.WishCart);
        }

        protected void btnAddToGiftRegistry_Click(object sender, EventArgs e)
        {
            AddToCart(CartTypeEnum.GiftRegistryCart);
        }

        protected void AddToCart(CartTypeEnum cartType)
        {            
            try
            {
                if (KitData.HasFileUploadGroup)
                {
                    KitData.MoveAllTempImagesToOrdered();
                }

                KitComposition composition = ComposeAddToCart();

                if (KitData.HasRequiredGroups)
                {
                    List<String> RequiredGroupNames = new List<String>();

                    foreach (KitGroupData requiredGroup in KitData.Groups)
                    {
                        if (requiredGroup.IsRequired)
                        {
                            int hasBeenSelected = composition.Compositions.Where(kci => kci.KitGroupID.Equals(requiredGroup.Id)).Count();
                            if (hasBeenSelected == 0)
                            {
                                RequiredGroupNames.Add(requiredGroup.Name);
                            }
                        }
                    }

                    if (RequiredGroupNames.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder(1000);
                        sb.Append(AppLogic.GetString("product.kit2product.xml.config.16", ThisCustomer.LocaleSetting));
                        sb.Append("<ul  data-role=\"listview\">");
                        foreach(String requiredGroup in RequiredGroupNames)
                        {
                            sb.Append("<li>" + requiredGroup + "</li>");
                        }
                        sb.Append("</ul>");
                        ShowError(sb.ToString());
                        return;
                    }
                }

                String tmp = DB.GetNewGUID();
                ShoppingCart cart = new ShoppingCart(1, ThisCustomer, cartType, 0, false);

                int qty = GetQuantity();

                if (KitData.HasCartMapping)
                {
                    AppLogic.ClearKitItems(ThisCustomer, KitData.Id, KitData.VariantId, KitData.ShoppingCartRecordId);
                    CartItem lineItem = cart.CartItems.FirstOrDefault(item => item.ShoppingCartRecordID == KitData.ShoppingCartRecordId);
                    cart.SetItemQuantity(lineItem.ShoppingCartRecordID, qty);
                    cart.ProcessKitComposition(composition, KitData.Id, KitData.VariantId, KitData.ShoppingCartRecordId);
                }
                else
                {
                    //GFS - If customer a session has been cleared and no cookies are available, we must create a customer record to associate the new cart ID to.
                    //If this is not done, adding a kit product within the said environment will render an empty shopping cart.
                    ThisCustomer.RequireCustomerRecord();
                    int shipId = ThisCustomer.PrimaryShippingAddressID;
                    int NewRecID = cart.AddItem(ThisCustomer,
                                    ThisCustomer.PrimaryShippingAddressID,
                                    KitData.Id, KitData.VariantId, qty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, cartType, false, false, 0, System.Decimal.Zero, composition);
                }                                
            }
            catch { }
                        

            bool stayOnThisPage = AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.InvariantCultureIgnoreCase);
            if (stayOnThisPage)
            {
                // some tokens like the shoppingcart qty may already be rendered
                // we therefore need to re-display the page to display the correct qty
                Response.Redirect(this.Request.Url.ToString());
            }
            else
            {
                string returnUrl = CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING");

                switch (cartType)
                {
                    case CartTypeEnum.ShoppingCart:
                        Response.Redirect(ResolveClientUrl("~/ShoppingCart.aspx?add=true&ReturnUrl=" + Security.UrlEncode(returnUrl)));
                        break;
                    case CartTypeEnum.GiftRegistryCart:
                        Response.Redirect(ResolveClientUrl("~/giftregistry.aspx?ReturnUrl=" + Security.UrlEncode(returnUrl)));
                        break;
                    case CartTypeEnum.WishCart:
                        Response.Redirect(ResolveClientUrl("~/wishlist.aspx?ReturnUrl=" + Security.UrlEncode(returnUrl)));
                        break;
                }
            }
            
        }

    }
}


