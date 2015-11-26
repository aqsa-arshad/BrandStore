// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for showproduct.
    /// </summary>
    [PageType("product")]
    public partial class showproduct : SkinBase
    {
        String SourceEntityInstanceName = String.Empty;
        protected string parentCategoryID = String.Empty;
        protected string parentCategoryName = String.Empty;


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

        private string m_PageOutput = string.Empty;
        private const string ADDTOCART_ACTION_PREFIX = "AddToCart_";

        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (AppLogic.AppConfigBool("GoNonSecureAgain"))
            {
                GoNonSecureAgain();
            }

            ProductID = CommonLogic.QueryStringUSInt("ProductID");
            CategoryID = CommonLogic.QueryStringUSInt("CategoryID");
            SectionID = CommonLogic.QueryStringUSInt("SectionID");
            ManufacturerID = CommonLogic.QueryStringUSInt("ManufacturerID");
            DistributorID = CommonLogic.QueryStringUSInt("DistributorID");
            GenreID = CommonLogic.QueryStringUSInt("GenreID");
            VectorID = CommonLogic.QueryStringUSInt("VectorID");

            String ActualSEName = string.Empty;
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(string.Format("select * from Product a with (NOLOCK) inner join (select a.ProductID, b.StoreID from Product a with (nolock) left join ProductStore b " +
                                                "with (NOLOCK) on a.ProductID = b.ProductID) b on a.ProductID = b.ProductID where Deleted=0 and a.ProductID={0} and ({1}=0 or StoreID={2})", +
                                                ProductID, CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowProductFiltering") == true, 1, 0), AppLogic.StoreID()), dbconn))
                {
                    if (!rs.Read())
                    {
                        HttpContext.Current.Server.Transfer("pagenotfound.aspx");
                    }
                    else
                    {
                        bool published = DB.RSFieldBool(rs, "Published");

                        if (!published)
                            HttpContext.Current.Server.Transfer("pagenotfound.aspx");

                        if (AppLogic.AppConfigBool("ProductPageOutOfStockRedirect"))
                        {
                            bool trackInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(ProductID);
                            bool outOfStock = AppLogic.ProbablyOutOfStock(ProductID, AppLogic.GetProductsDefaultVariantID(ProductID), trackInventoryBySizeAndColor, "Product");

                            if (outOfStock)
                                HttpContext.Current.Server.Transfer("pagenotfound.aspx");
                        }
                    }

                    String SENameINURL = CommonLogic.QueryStringCanBeDangerousContent("SEName");
                    ActualSEName = SE.MungeName(DB.RSField(rs, "SEName"));
                    if (ActualSEName != SENameINURL)
                    {
                        String NewURL = AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeProductLink(ProductID, ActualSEName);

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
                                QStr += key + "=" + Request.QueryString[i];
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


                    #region Vortx Mobile Xml Package Modification
                    m_XmlPackage = Vortx.MobileFramework.MobileXmlPackageController.XmlPackageHook(DB.RSField(rs, "XmlPackage").ToLowerInvariant(), ThisCustomer);
                    #endregion
                    IsAKit = DB.RSFieldBool(rs, "IsAKit");
                    //this part of code is written for kit products. there is no xml package which supports them.
                    if (IsAKit)
                    {
                        IsAKit = false;
                    }
                    //end
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

            if (IsAKit && !Vortx.MobileFramework.MobileHelper.isMobile())
            {
                Server.Transfer(ResolveClientUrl("~/kitproduct.aspx"), true);
                return;
            }
            else if (IsAKit && Vortx.MobileFramework.MobileHelper.isMobile())
            {
                Server.Transfer(ResolveClientUrl("~/mobilekitproduct.aspx"), true);
                return;
            }

            CategoryName = CategoryHelper.GetEntityName(CategoryID, ThisCustomer.LocaleSetting);
            SectionName = SectionHelper.GetEntityName(SectionID, ThisCustomer.LocaleSetting);
            ManufacturerName = ManufacturerHelper.GetEntityName(ManufacturerID, ThisCustomer.LocaleSetting);
            DistributorName = DistributorHelper.GetEntityName(DistributorID, ThisCustomer.LocaleSetting);
            GenreName = GenreHelper.GetEntityName(GenreID, ThisCustomer.LocaleSetting);
            VectorName = VectorHelper.GetEntityName(VectorID, ThisCustomer.LocaleSetting);

            if (ManufacturerID != 0)
            {
                Profile.LastViewedEntityName = EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName;
                Profile.LastViewedEntityInstanceID = ManufacturerID.ToString();
                Profile.LastViewedEntityInstanceName = ManufacturerName;

                String NewURL = AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeProductLink(ProductID, ActualSEName);
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

                String NewURL = AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeProductLink(ProductID, ActualSEName);
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

                String NewURL = AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeProductLink(ProductID, ActualSEName);
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

                String NewURL = AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeProductLink(ProductID, ActualSEName);
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

                String NewURL = AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeProductLink(ProductID, ActualSEName);
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

                String NewURL = AppLogic.GetStoreHTTPLocation(false, false) + SE.MakeProductLink(ProductID, ActualSEName);
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
                String sqlx = string.Format("select count(*) as N from productentity a with (nolock) inner join (select distinct a.entityid, a.EntityType from productentity a with (nolock) left join EntityStore b with (nolock) " +
                    "on a.EntityID = b.EntityID where ({0} = 0 or StoreID = {1})) b on a.EntityID = b.EntityID and a.EntityType=b.EntityType where ProductID = {2} and a.EntityID = {3} and a.EntityType = {4}"
                    , CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowEntityFiltering") == true, 1, 0), AppLogic.StoreID(), ProductID, SourceEntityID, DB.SQuote(SourceEntity));
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
            //Reset LastViewedEntityInstanceID to zero if no entities are mapped to the product so the left nav will render properly.
            if (SourceEntityID <= 0)
            {
                HttpContext.Current.Profile.SetPropertyValue("LastViewedEntityInstanceID", "0");
            }

            if (RequiresReg && !ThisCustomer.IsRegistered)
            {
                m_PageOutput += "<b>" + AppLogic.GetString("showproduct.aspx.1", SkinID, ThisCustomer.LocaleSetting) + "</b><a href=\"signin.aspx?returnurl=" + CommonLogic.GetThisPageName(false) + "?ProductID=" + ProductID.ToString() + CommonLogic.IIF(CommonLogic.ServerVariables("QUERY_STRING").Trim().Length > 0, "&" + Security.HtmlEncode(Security.UrlEncode(CommonLogic.ServerVariables("QUERY_STRING"))), String.Empty) + "\">" + AppLogic.GetString("showproduct.aspx.2", SkinID, ThisCustomer.LocaleSetting) + "</a> " + AppLogic.GetString("showproduct.aspx.3", SkinID, ThisCustomer.LocaleSetting);
            }
            else
            {
                AppLogic.eventHandler("ViewProductPage").CallEvent("&ViewProductPage=true");

                // check if the postback was caused by an addtocart button
                if (this.IsPostBack && this.IsAddToCartPostBack)
                {
                    HandleAddToCart();
                    return;
                }

                DB.ExecuteSQL("update product set Looks=Looks+1 where ProductID=" + ProductID.ToString());

                m_PageOutput = "<!-- XmlPackage: " + m_XmlPackage + " -->\n";
                if (m_XmlPackage.Length == 0)
                {
                    m_PageOutput += "<p><b><font color=red>XmlPackage format was chosen, but no XmlPackage was specified!</font></b></p>";
                }
                else
                {
                    using (XmlPackage2 p = new XmlPackage2(m_XmlPackage, ThisCustomer, SkinID, "", "EntityName=" + SourceEntity + "&EntityID=" + SourceEntityID.ToString() + CommonLogic.IIF(CommonLogic.ServerVariables("QUERY_STRING").IndexOf("cartrecid") != -1, "&cartrecid=" + CommonLogic.QueryStringUSInt("cartrecid").ToString(), "&showproduct=1"), String.Empty, true))
                    {
                        m_PageOutput += AppLogic.RunXmlPackage(p, base.GetParser, ThisCustomer, SkinID, true, true);
                        if (p.SectionTitle != "")
                        {
                            SectionTitle = p.SectionTitle;
                        }
                        if (p.SETitle != "")
                        {
                            SETitle = p.SETitle;
                        }
                        if (p.SEDescription != "")
                        {
                            SEDescription = p.SEDescription;
                        }
                        if (p.SEKeywords != "")
                        {
                            SEKeywords = p.SEKeywords;
                        }
                        if (p.SENoScript != "")
                        {
                            SENoScript = p.SENoScript;
                        }
                    }
                }
            }
            litOutput.Text = m_PageOutput;
            GetParentCategory();
            if (!string.IsNullOrEmpty(SourceEntityInstanceName) && !string.IsNullOrEmpty(parentCategoryID))
            {
                parentCategoryName = CategoryHelper.GetEntityName(Convert.ToInt32(parentCategoryID), ThisCustomer.LocaleSetting);

                ((System.Web.UI.WebControls.HyperLink)Master.FindControl("lnkCategory")).Text = parentCategoryName;
                ((System.Web.UI.WebControls.HyperLink)Master.FindControl("lnkCategory")).NavigateUrl = "~/c-" + parentCategoryID + "-" + parentCategoryName.Replace(" ", "-") + ".aspx";

                ((System.Web.UI.WebControls.Label)Master.FindControl("lblSperator")).Text = ">>";

                ((System.Web.UI.WebControls.HyperLink)Master.FindControl("lnkSubCategory")).Text = SourceEntityInstanceName;
                ((System.Web.UI.WebControls.HyperLink)Master.FindControl("lnkSubCategory")).NavigateUrl = "~/c-" + SourceEntityID + "-" + SourceEntityInstanceName.Replace(" ", "-") + ".aspx";
            }
        }

        /// <summary>
        /// Sets that this page requires a scriptmanager
        /// </summary>
        public override bool RequireScriptManager
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Registers the required scripts and webservice references
        /// </summary>
        /// <param name="scrptMgr"></param>
        public override void RegisterScriptAndServices(ScriptManager scrptMgr)
        {
            scrptMgr.Scripts.Add(new ScriptReference("~/jscripts/product.js"));
            if (AppLogic.AppConfigBool("Minicart.UseAjaxAddToCart") && !Vortx.MobileFramework.MobileHelper.isMobile())
            {
                scrptMgr.Services.Add(new ServiceReference("~/actionservice.asmx"));
            }
        }

        private void HandleKitUpdate()
        {
            ThisCustomer.RequireCustomerRecord();
            AppLogic.ProcessKitForm(ThisCustomer, ProductID);
            if (CommonLogic.FormUSInt("CartRecID") > 0)
            {
                switch (ShoppingCart.CartTypeFromRecID(CommonLogic.FormUSInt("CartRecID")))
                {
                    case CartTypeEnum.GiftRegistryCart:
                        Response.Redirect(ResolveClientUrl("~/giftregistry.aspx"));
                        break;
                    case CartTypeEnum.ShoppingCart:
                        Response.Redirect(ResolveClientUrl("~/shoppingcart.aspx"));
                        break;
                    case CartTypeEnum.WishCart:
                        Response.Redirect(ResolveClientUrl("~/wishlist.aspx"));
                        break;
                }
            }
        }

        private void HandleAddToCart()
        {
            // extract the input parameters from the form post
            AddToCartInfo formInput = AddToCartInfo.FromForm(ThisCustomer);

            if (formInput != AddToCartInfo.INVALID_FORM_COMPOSITION)
            {
                string returnUrl = SE.MakeObjectLink("Product", formInput.ProductId, String.Empty);

                if (!ThisCustomer.IsRegistered && AppLogic.AppConfigBool("DisallowAnonCustomerToCreateWishlist"))
                {
                    string ErrMsg = string.Empty;

                    ErrorMessage er;

                    if (formInput.CartType == CartTypeEnum.WishCart)
                    {
                        ErrMsg = AppLogic.GetString("signin.aspx.27", 1, ThisCustomer.LocaleSetting);
                        er = new ErrorMessage(ErrMsg);
                        Response.Redirect("signin.aspx?ErrorMsg=" + er.MessageId + "&ReturnUrl=" + Security.UrlEncode(returnUrl));
                    }

                    if (formInput.CartType == CartTypeEnum.GiftRegistryCart)
                    {
                        ErrMsg = AppLogic.GetString("signin.aspx.28", 1, ThisCustomer.LocaleSetting);
                        er = new ErrorMessage(ErrMsg);
                        Response.Redirect("signin.aspx?ErrorMsg=" + er.MessageId + "&ReturnUrl=" + Security.UrlEncode(returnUrl));
                    }
                }


                bool success = ShoppingCart.AddToCart(ThisCustomer, formInput);
                AppLogic.eventHandler("AddToCart").CallEvent("&AddToCart=true&VariantID=" + formInput.VariantId.ToString() + "&ProductID=" + formInput.ProductId.ToString() + "&ChosenColor=" + formInput.ChosenColor.ToString() + "&ChosenSize=" + formInput.ChosenSize.ToString());
                if (success)
                {
                    bool stayOnThisPage = AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.InvariantCultureIgnoreCase);
                    if (stayOnThisPage)
                    {
                        // some tokens like the shoppingcart qty may already be rendered
                        // we therefore need to re-display the page to display the correct qty
                        Response.Redirect(this.Request.Url.ToString());
                    }
                    else
                    {

                        if (formInput.CartType == CartTypeEnum.WishCart)
                        {
                            Response.Redirect(ResolveClientUrl("~/wishlist.aspx?ReturnUrl=" + Security.UrlEncode(returnUrl)));
                        }
                        if (formInput.CartType == CartTypeEnum.GiftRegistryCart)
                        {
                            Response.Redirect(ResolveClientUrl("~/giftregistry.aspx?ReturnUrl=" + Security.UrlEncode(returnUrl)));
                        }
                        // default
                        Response.Redirect(ResolveClientUrl("~/ShoppingCart.aspx?add=true&ReturnUrl=" + Security.UrlEncode(returnUrl)));
                    }
                }
            }

            return;
        }

        private bool IsAddToCartPostBack
        {
            get
            {
                return "AddToCart".Equals(CommonLogic.FormCanBeDangerousContent("__EVENTTARGET"),
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {

            HtmlForm form = this.Page.Form;
            if (form.Action == "")
            {
                form.Action = ResolveClientUrl(
                    string.Format("~/showProduct.aspx?SEName={0}&ProductID={1}",
                    CommonLogic.QueryStringCanBeDangerousContent("SEName"),
                    CommonLogic.QueryStringCanBeDangerousContent("ProductID")));
            }
            if ((form != null) && (form.Enctype.Length == 0))
            {
                // change the encoding type of the form if 
                // the current product is a kit
                // and we have file-upload items
                // so that the browser can send the file data
                if (IsAKit)
                {
                    form.Enctype = "multipart/form-data";
                }
            }
            base.OnPreRender(e);
        }

        public override bool IsProductPage
        {
            get
            {
                return true;
            }
        }

        public override int PageID
        {
            get
            {
                return ProductID;
            }
        }

        protected override string OverrideTemplate()
        {
            var masterHome = AppLogic.HomeTemplate();
            if (masterHome.Trim().Length == 0)
            {
                masterHome = "JeldWenTemplate";
            }
            if (masterHome.EndsWith(".ascx"))
            {
                masterHome = masterHome.Replace(".ascx", ".master");
            }
            if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                masterHome = masterHome + ".master";
            }
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

        private void GetParentCategory()
        {
            using (var conn = DB.dbConn())
            {
                conn.Open();
                if (VerifySubCategoryExist())
                {
                    var query = "select ParentCategoryID from Category where CategoryID = '" + SourceEntityID + "'";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text;

                        IDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            parentCategoryID = reader["ParentCategoryID"].ToString();
                        }
                    }
                }
            }
        }

        private bool VerifySubCategoryExist()
        {
            using (var conn = DB.dbConn())
            {
                conn.Open();
                var query = "select * from ProductCategory where CategoryID = '" + SourceEntityID + "' and ProductID = '" + ProductID + "'";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    IDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
