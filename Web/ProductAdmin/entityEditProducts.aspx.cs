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
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class entityEditProducts : AdminPageBase
    {
        private EntityHelper entity;
        private string eName;
        private int pID;
        private int eID;
        private EntitySpecs eSpecs;
        private ProductDescriptionFile pdesc;
        private int currentSkinID = 1;
        protected bool bUseHtmlEditor;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (AppLogic.MaxProductsExceeded())
            {
                ltError.Text = "<font class=\"errorMsg\">WARNING: Maximum number of allowed products exceeded. To add additional products, please delete some products or upgrade to AspDotNetStoreFront ML </font>&nbsp;&nbsp;&nbsp;";
            }

            if (!IsPostBack)
            {
                ViewState.Add("ProductEditID", 0);
            }

            // Determine HTML editor configuration
            bUseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
            radSummary.Visible = bUseHtmlEditor;
            radSummary.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);
            radDescription.Visible = bUseHtmlEditor;
            radDescription.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);

            PopularityRow.Visible = AppLogic.AppConfigBool("ShowPopularity");

            pdesc = new ProductDescriptionFile(pID, ThisCustomer.LocaleSetting, currentSkinID);
            pID = CommonLogic.QueryStringNativeInt("iden");
            eID = CommonLogic.IIF(CommonLogic.QueryStringNativeInt("EntityFilterID") == 0, CommonLogic.QueryStringNativeInt("EntityID"), CommonLogic.QueryStringNativeInt("EntityFilterID"));
            eName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            eSpecs = EntityDefinitions.LookupSpecs(eName);

            if (Localization.ParseNativeInt(ViewState["ProductEditID"].ToString()) > 0)
            {
                pID = Localization.ParseNativeInt(ViewState["ProductEditID"].ToString());
            }

            switch (eName.ToUpperInvariant())
            {
                case "SECTION":
                    entity = AppLogic.SectionStoreEntityHelper[0];
                    break;
                case "MANUFACTURER":
                    entity = AppLogic.ManufacturerStoreEntityHelper[0];
                    break;
                case "DISTRIBUTOR":
                    entity = AppLogic.DistributorStoreEntityHelper[0];
                    break;
                case "LIBRARY":
                    entity = AppLogic.LibraryStoreEntityHelper[0];
                    break;
                default:
                    entity = AppLogic.CategoryStoreEntityHelper[0];
                    break;
            }
            etsMapper.ObjectID = pID;
            if (!IsPostBack)
            {
                etsMapper.DataBind();
                pnlStoreMappings.Visible = etsMapper.StoreCount > 1;
                btnDeleteAll.Attributes.Add("onclick", "return confirm('Confirm Delete');");
                if (ThisCustomer.ThisCustomerSession.Session("entityUserLocale").Length == 0)
                {
                    ThisCustomer.ThisCustomerSession.SetVal("entityUserLocale", Localization.GetDefaultLocale());
                }

                ddLocale.Items.Clear();

                using (System.Data.DataTable dtLocale = Localization.GetLocales())
                {
                    foreach (DataRow dr in dtLocale.Rows)
                    {
                        ddLocale.Items.Add(new ListItem(DB.RowField(dr, "Name"), DB.RowField(dr, "Name")));
                    }
                }

                if (ddLocale.Items.Count < 2)
                {
                    tblLocale.Visible = false;
                }
                LoadScript();
                LoadContent();
            }

            ProductSpecFile pspec = new ProductSpecFile(pID, currentSkinID);

            if (pspec.FN == String.Empty)
            {
                this.btnSpecs.Enabled = false;
                this.imgbtnSpecs.Visible = true;
            }

            //Hide the kit option if the product had recurring variants, so they can't make a recurring kit product
            List<ProductVariant> variantsToCheck = new List<ProductVariant>(ProductVariant.GetVariants(pID, false));
            trKit.Visible = variantsToCheck.Count(pv => pv.IsRecurring == true) < 1;
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ((Literal)Form.FindControl("ltError")).Text = str;
        }

        protected void LoadScript()
        {
            //Deprecated
        }

        protected void LoadContent()
        {
            ddDistributor.Items.Clear();
            ddDiscountTable.Items.Clear();
            ddManufacturer.Items.Clear();
            ddOnSalePrompt.Items.Clear();
            ddXmlPackage.Items.Clear();
            ddFund.Items.Clear();
            ddType.Items.Clear();
            ddTaxClass.Items.Clear();

            //pull locale from user session
            string entityLocale = ThisCustomer.ThisCustomerSession.Session("entityUserLocale");
            if (entityLocale.Length > 0)
            {
                try
                {
                    ddLocale.SelectedValue = entityLocale; // user's locale may not exist any more, so don't let the assignment crash
                }
                catch { }
            }

            string locale = Localization.CheckLocaleSettingForProperCase(ddLocale.SelectedValue);

            if (ddLocale.SelectedValue == string.Empty)
            {
                locale = Localization.GetDefaultLocale();
            }


            bool ProductTracksInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(pID);
            bool IsAKit = AppLogic.IsAKit(pID);
            bool Editing = false;

            if (IsAKit)
            {
                ProductTracksInventoryBySizeAndColor = false;
            }
            ProductSpecFile pspec = new ProductSpecFile(pID, currentSkinID);



            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from Product   with (NOLOCK)  where ProductID=" + pID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                        if (!DB.RSFieldBool(rs, "Published"))
                        {
                            lblPublished.ForeColor = System.Drawing.Color.Red;
                            lblPublished.Font.Bold = true;
                        }
                        else
                        {
                            lblPublished.ForeColor = System.Drawing.Color.FromArgb(105, 105, 105);
                            lblPublished.Font.Bold = false;
                        }

                        if (DB.RSFieldBool(rs, "Deleted"))
                        {
                            btnDeleteProduct.Text = "UnDelete this Product";
                        }
                        else
                        {
                            btnDeleteProduct.Text = "Delete this Product";
                        }
                        pnlDelete.Visible = true;

                        if (0 < DB.GetSqlN("select count(*) N from ShoppingCart with (NOLOCK) where ProductID=" + pID.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
                        {
                            btnDeleteProduct.Text = AppLogic.GetString("admin.common.DeleteNA", currentSkinID, ThisCustomer.LocaleSetting)
                                + "&nbsp;<img title=\"" + "admin.common.DeleteNAInfoRecurring".StringResource() + "\" border=\"0\" align=\"absmiddle\" src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/info.gif") + "\">";
                            btnDeleteProduct.Enabled = false;
                        }
                    }
                    else
                    {
                        pnlDelete.Visible = false;
                    }

                    ViewState.Add("ProductEdit", Editing);

                    //load Product Types
                    ddType.Items.Add(new ListItem(" - Select -", "0"));

                    string query = string.Empty;
                    query = "select * from ProductType   with (NOLOCK)  order by DisplayOrder,Name";

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();

                        using (IDataReader rsst = DB.GetRS(query, conn))
                        {
                            while (rsst.Read())
                            {
                                ddType.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", ThisCustomer.LocaleSetting), DB.RSFieldInt(rsst, "ProductTypeID").ToString()));
                            }
                        }
                    }
                    //load Manufacturers
                    ddManufacturer.Items.Add(new ListItem(" - Select -", "0"));
                    string sql = "select * from Manufacturer  with (NOLOCK)  where deleted=0 order by DisplayOrder,Name";

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();
                        using (IDataReader rsst = DB.GetRS(sql, conn))
                        {
                            while (rsst.Read())
                            {
                                ddManufacturer.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", ThisCustomer.LocaleSetting), DB.RSFieldInt(rsst, "ManufacturerID").ToString()));
                            }
                        }
                    }

                    //load Distributors
                    ddDistributor.Items.Add(new ListItem(" - Select -", "0"));

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();
                        using (IDataReader rsst = DB.GetRS("select * from Distributor   with (NOLOCK)  where Deleted=0 order by DisplayOrder,Name", conn))
                        {
                            while (rsst.Read())
                            {
                                ddDistributor.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", ThisCustomer.LocaleSetting), DB.RSFieldInt(rsst, "DistributorID").ToString()));
                            }
                        }
                    }

                    //load XmlPackages
                    ArrayList xmlPackages = AppLogic.ReadXmlPackages("product", currentSkinID);
                    foreach (String s in xmlPackages)
                    {
                        ddXmlPackage.Items.Add(new ListItem(s, s));
                    }

                    //Load Funds
                    ddFund.Items.Add(new ListItem("None", "0"));

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();
                        using (IDataReader rsst = DB.GetRS("SELECT FundID, FundName FROM Fund WITH (NOLOCK) WHERE FundID NOT IN (1, 2) AND IsActive = 1", conn))
                        {
                            while (rsst.Read())
                            {
                                ddFund.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "FundName", ThisCustomer.LocaleSetting), DB.RSFieldInt(rsst, "FundID").ToString()));
                            }
                        }
                    }

                    //load Discount Tables
                    ddDiscountTable.Items.Add(new ListItem("None", "0"));

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();
                        using (IDataReader rsst = DB.GetRS("select * from QuantityDiscount   with (NOLOCK)  order by DisplayOrder,Name", conn))
                        {
                            while (rsst.Read())
                            {
                                ddDiscountTable.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", ThisCustomer.LocaleSetting), DB.RSFieldInt(rsst, "QuantityDiscountID").ToString()));
                            }
                        }
                    }

                    //load Tax Class
                    if (CommonLogic.IsStringNullOrEmpty(AppLogic.AppConfig("Admin_DefaultTaxClassID")))
                    {
                        ddTaxClass.Items.Add(new ListItem("None", "0"));
                    }

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();
                        using (IDataReader rsst = DB.GetRS("select * from TaxClass   with (NOLOCK)  order by DisplayOrder,Name", conn))
                        {
                            while (rsst.Read())
                            {
                                ddTaxClass.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", ThisCustomer.LocaleSetting), DB.RSFieldInt(rsst, "TaxClassID").ToString()));
                            }
                        }

                    }

                    if (ddTaxClass.Items.FindByValue(AppLogic.AppConfig("Admin_DefaultTaxClassID")) != null)
                    {
                        ddTaxClass.SelectedValue = AppLogic.AppConfig("Admin_DefaultTaxClassID");
                    }

                    //On Sale prompt
                    ddOnSalePrompt.Items.Add(new ListItem(" - Select -", "0"));

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();
                        using (IDataReader rsst = DB.GetRS("select * from salesprompt   with (NOLOCK)  where deleted=0", conn))
                        {
                            while (rsst.Read())
                            {
                                ddOnSalePrompt.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", ThisCustomer.LocaleSetting), DB.RSFieldInt(rsst, "SalesPromptID").ToString()));
                            }
                        }
                    }

                    //MAPPINGS
                    cblCategory.Items.Clear();
                    cblAffiliates.Items.Clear();
                    cblCustomerLevels.Items.Clear();
                    cblGenres.Items.Clear();
                    cblVectors.Items.Clear();
                    cblDepartment.Items.Clear();

                    EntityHelper eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), false, 0);
                    ArrayList al = eTemp.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false);
                    for (int i = 0; i < al.Count; i++)
                    {
                        ListItemClass lic = (ListItemClass)al[i];
                        string value = lic.Value.ToString();
                        string name = lic.Item;

                        cblCategory.Items.Add(new ListItem(name, value));
                    }

                    eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), false, 0);
                    al = eTemp.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false);
                    for (int i = 0; i < al.Count; i++)
                    {
                        ListItemClass lic = (ListItemClass)al[i];
                        string value = lic.Value.ToString();
                        string name = lic.Item;

                        cblDepartment.Items.Add(new ListItem(name, value));
                    }

                    eTemp = new EntityHelper(EntityDefinitions.readonly_AffiliateEntitySpecs, 0);
                    al = eTemp.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false);
                    for (int i = 0; i < al.Count; i++)
                    {
                        ListItemClass lic = (ListItemClass)al[i];
                        string value = lic.Value.ToString();
                        string name = lic.Item;

                        cblAffiliates.Items.Add(new ListItem(name, value));
                    }

                    eTemp = new EntityHelper(EntityDefinitions.readonly_CustomerLevelEntitySpecs, 0);
                    al = eTemp.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false);
                    for (int i = 0; i < al.Count; i++)
                    {
                        ListItemClass lic = (ListItemClass)al[i];
                        string value = lic.Value.ToString();
                        string name = lic.Item;

                        cblCustomerLevels.Items.Add(new ListItem(name, value));
                    }

                    eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), false, 0);
                    al = eTemp.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false);
                    for (int i = 0; i < al.Count; i++)
                    {
                        ListItemClass lic = (ListItemClass)al[i];
                        string value = lic.Value.ToString();
                        string name = lic.Item;

                        cblGenres.Items.Add(new ListItem(name, value));
                    }

                    eTemp = new EntityHelper(0, EntityDefinitions.LookupSpecs("Vector"), false, 0);
                    al = eTemp.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false);
                    for (int i = 0; i < al.Count; i++)
                    {
                        ListItemClass lic = (ListItemClass)al[i];
                        string value = lic.Value.ToString();
                        string name = lic.Item;

                        cblVectors.Items.Add(new ListItem(name, value));
                    }

                    txtSwatchImageMap.Columns = Localization.ParseNativeInt(AppLogic.AppConfig("Admin_TextareaWidth"));
                    txtSwatchImageMap.Rows = AppLogic.AppConfigNativeInt("Admin_TextareaHeightSmall");

                    txtMiscText.Columns = Localization.ParseNativeInt(AppLogic.AppConfig("Admin_TextareaWidth"));
                    txtMiscText.Rows = 10;

                    txtExtensionData.Columns = Localization.ParseNativeInt(AppLogic.AppConfig("Admin_TextareaWidth"));
                    txtExtensionData.Rows = 10;

                    txtFroogle.Columns = Localization.ParseNativeInt(AppLogic.AppConfig("Admin_TextareaWidth"));
                    txtFroogle.Rows = 10;

                    if (Editing)
                    {
                        pnlVariant.Visible = false;
                        this.phAllVariants.Visible = true;
                        this.phAddVariant.Visible = false;

                        SetVariantButtons();

                        ltStatus.Text = "Editing <em>" + AppLogic.GetProductName(pID, ThisCustomer.LocaleSetting) + "</em> (" + pID.ToString() + ")";
                        divltStatus2.Visible = true;
                        ltStatus2.Text = "<a href=\"entityproductvariantsoverview.aspx?productid=" + pID.ToString() + "&entityname=" + eName + "&entityid=" + eID.ToString() + "\">Show Variants</a>";
                        btnSubmit.Text = "Update";
                        btnSubmit.Enabled = true;
                        btnSubmit.CssClass = "normalButtons";

                        txtName.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Name"), locale, false);

                        if (!DB.RSFieldBool(rs, "Published"))
                        {
                            rblPublished.BackColor = System.Drawing.Color.LightYellow;
                        }
                        rblPublished.SelectedIndex = (DB.RSFieldBool(rs, "Published") ? 1 : 0);

                        //match Type
                        foreach (ListItem li in ddType.Items)
                        {
                            if (li.Value.Equals(DB.RSFieldInt(rs, "ProductTypeID").ToString()))
                            {
                                ddType.SelectedValue = DB.RSFieldInt(rs, "ProductTypeID").ToString();
                            }
                        }

                        //match Manufacturer
                        foreach (ListItem li in ddManufacturer.Items)
                        {
                            if (li.Value.Equals(AppLogic.GetProductManufacturerID(pID).ToString()))
                            {
                                ddManufacturer.SelectedValue = AppLogic.GetProductManufacturerID(pID).ToString();
                            }
                        }

                        //match Distributor
                        foreach (ListItem li in ddDistributor.Items)
                        {
                            if (li.Value.Equals(AppLogic.GetProductDistributorID(pID).ToString()))
                            {
                                ddDistributor.SelectedValue = AppLogic.GetProductDistributorID(pID).ToString();
                            }
                        }

                        //match XmlPackage
                        ddXmlPackage.ClearSelection();
                        foreach (ListItem li in ddXmlPackage.Items)
                        {
                            if (li.Value.Equals(DB.RSField(rs, "XmlPackage").ToLowerInvariant()))
                            {
                                ddXmlPackage.SelectedValue = DB.RSField(rs, "XmlPackage").ToLowerInvariant();
                            }
                        }

                        // Match Funds
                        ddFund.ClearSelection();
                        foreach (ListItem li in ddFund.Items)
                        {
                            if (li.Value.Equals(DB.RSFieldInt(rs, "FundID").ToString()))
                            {
                                ddFund.SelectedValue = DB.RSFieldInt(rs, "FundID").ToString();
                            }
                        }

                        //match Discount Table
                        ddDiscountTable.ClearSelection();
                        foreach (ListItem li in ddDiscountTable.Items)
                        {
                            if (li.Value.Equals(DB.RSFieldInt(rs, "QuantityDiscountID").ToString()))
                            {
                                ddDiscountTable.SelectedValue = DB.RSFieldInt(rs, "QuantityDiscountID").ToString();
                            }
                        }

                        //match Tax Class
                        ddTaxClass.ClearSelection();
                        foreach (ListItem li in ddTaxClass.Items)
                        {
                            if (li.Value.Equals(DB.RSFieldInt(rs, "TaxClassID").ToString()))
                            {
                                ddTaxClass.SelectedValue = DB.RSFieldInt(rs, "TaxClassID").ToString();
                            }
                        }

                        //match On Sale
                        ddOnSalePrompt.ClearSelection();
                        foreach (ListItem li in ddOnSalePrompt.Items)
                        {
                            if (li.Value.Equals(DB.RSFieldInt(rs, "SalesPromptID").ToString()))
                            {
                                ddOnSalePrompt.SelectedValue = DB.RSFieldInt(rs, "SalesPromptID").ToString();
                            }
                        }

                        txtSKU.Text = DB.RSField(rs, "SKU");
                        txtManufacturePartNumber.Text = XmlCommon.XmlDecode(DB.RSField(rs, "ManufacturerPartNumber"));

                        rblShowBuyButton.SelectedIndex = (DB.RSFieldBool(rs, "ShowBuyButton") ? 1 : 0);
                        rblIsCallToOrder.SelectedIndex = (DB.RSFieldBool(rs, "IsCallToOrder") ? 1 : 0);
                        rblHidePriceUntilCart.SelectedIndex = (DB.RSFieldBool(rs, "HidePriceUntilCart") ? 1 : 0);
                        rblExcludeFroogle.SelectedIndex = (DB.RSFieldBool(rs, "ExcludeFromPriceFeeds") ? 1 : 0);

                        rblIsKit.SelectedIndex = (DB.RSFieldBool(rs, "IsAKit") ? 1 : 0);
                        ltKit.Text = CommonLogic.IIF(DB.RSFieldBool(rs, "IsAKit"), "<a href=\"editkit2.aspx?productid=" + DB.RSFieldInt(rs, "ProductID").ToString() + "&entityName=" + eName + "&entityFilterID=" + eID + "\">Edit Kit</a>", "");

                        txtPopularity.Text = DB.RSFieldInt(rs, "Popularity").ToString();

                        if (IsAKit)
                        {
                            trInventory1.Visible = false;
                            trInventory2.Visible = false;
                            trInventory3.Visible = false;
                        }
                        else
                        {
                            trInventory1.Visible = true;
                            trInventory2.Visible = true;
                            trInventory3.Visible = true;

                            rblTrackSizeColor.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "TrackInventoryBySizeAndColor"), 1, 0);
                            txtColorOption.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "ColorOptionPrompt"), locale, false);
                            txtSizeOption.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SizeOptionPrompt"), locale, false);
                        }

                        rblRequiresTextField.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "RequiresTextOption"), 1, 0);
                        txtTextFieldPrompt.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "TextOptionPrompt"), locale, false);
                        txtTextOptionMax.Text = DB.RSFieldInt(rs, "TextOptionMaxLength").ToString();

                        rblRequiresRegistrationToView.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "RequiresRegistration"), 1, 0);

                        txtPageSize.Text = CommonLogic.IIF(Editing, DB.RSFieldInt(rs, "PageSize").ToString(), AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "PageSize"));
                        txtColumn.Text = CommonLogic.IIF(Editing, DB.RSFieldInt(rs, "ColWidth").ToString(), AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "ColWidth"));

                        //Date
                        txtStartDate.Text = Localization.ToNativeShortDateString(DB.RSFieldDateTime(rs, "AvailableStartDate"));
                        DateTime stopDate = DB.RSFieldDateTime(rs, "AvailableStopDate");
                        txtStopDate.Text = CommonLogic.IIF(stopDate == DateTime.MinValue, String.Empty, Localization.ToNativeShortDateString(stopDate));

                        //DESCRIPTION
                        if (bUseHtmlEditor)
                        {
                            radDescription.Content = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Description"), locale, false);
                        }
                        else
                        {
                            ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                            ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\">" + XmlCommon.GetLocaleEntry(HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Description")), locale, false) + "</textarea>\n");
                            ltDescription.Text += ("</div>");
                        }


                        btnDescription.Visible = false;
                        ltDescriptionFile1.Text = ltDescriptionFile2.Text = "";
                        if (pdesc.Contents.Length != 0)
                        {
                            btnDescription.Visible = true;
                            ltDescriptionFile1.Text = ("<b>From File: " + pdesc.URL + "</b> &nbsp;&nbsp;\n");
                            ltDescriptionFile2.Text = ("<div style=\"border-style: dashed; border-width: 1px;\">\n");
                            ltDescriptionFile2.Text += (pdesc.Contents);
                            ltDescriptionFile2.Text += ("</div>\n");
                        }

                        //SUMMARY
                        if (bUseHtmlEditor)
                        {
                            radSummary.Content = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Summary"), locale, false);
                        }
                        else
                        {
                            ltSummary.Text = ("<div id=\"idSummary\" style=\"height: 1%;\">");
                            ltSummary.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Summary\" name=\"Summary\">" + XmlCommon.GetLocaleEntry(HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Summary")), locale, false) + "</textarea>\n");
                            ltSummary.Text += ("</div>");
                        }

                        //FROOGLE
                        txtFroogle.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "FroogleDescription"), locale, false);

                        //EXTENSION DATA
                        txtExtensionData.Text = DB.RSField(rs, "ExtensionData");

                        //MISC TEXT
                        txtMiscText.Text = DB.RSField(rs, "MiscText");

                        if (AppLogic.AppConfigBool("ShowAutoFill"))
                        {
                            lnkAutoFill.Visible = true;
                            lnkAutoFill.NavigateUrl = "autofill.aspx?productid=" + pID.ToString();
                        }
                        else
                        {
                            lnkAutoFill.Visible = false;
                        }

                        //MAPPINGS
                        //Category
                        ArrayList alE = EntityHelper.GetProductEntityList(pID, EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName);
                        foreach (ListItem li in cblCategory.Items)
                        {
                            if (alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
                            {
                                li.Selected = true;
                            }
                        }
                        //Section
                        alE = EntityHelper.GetProductEntityList(pID, EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName);
                        foreach (ListItem li in cblDepartment.Items)
                        {
                            if (alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
                            {
                                li.Selected = true;
                            }
                        }

                        //Affiliate
                        alE = EntityHelper.GetProductEntityList(pID, EntityDefinitions.readonly_AffiliateEntitySpecs.m_EntityName);
                        foreach (ListItem li in cblAffiliates.Items)
                        {
                            if (alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
                            {
                                li.Selected = true;
                            }
                        }


                        //Customer Level
                        alE = EntityHelper.GetProductEntityList(pID, EntityDefinitions.readonly_CustomerLevelEntitySpecs.m_EntityName);
                        foreach (ListItem li in cblCustomerLevels.Items)
                        {
                            if (alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
                            {
                                li.Selected = true;
                            }
                        }

                        //Genre
                        alE = EntityHelper.GetProductEntityList(pID, EntityDefinitions.readonly_GenreEntitySpecs.m_EntityName);
                        foreach (ListItem li in cblGenres.Items)
                        {
                            if (alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
                            {
                                li.Selected = true;
                            }
                        }

                        //Vector
                        alE = EntityHelper.GetProductEntityList(pID, EntityDefinitions.readonly_VectorEntitySpecs.m_EntityName);
                        foreach (ListItem li in cblVectors.Items)
                        {
                            if (alE.IndexOf(Localization.ParseNativeInt(li.Value)) > -1)
                            {
                                li.Selected = true;
                            }
                        }


                        //RELATED and REQUIRED
                        txtRelatedProducts.Text = DB.RSField(rs, "RelatedProducts");
                        txtUpsellProducts.Text = DB.RSField(rs, "UpsellProducts");
                        txtUpsellProductsDiscount.Text = Localization.FormatDecimal2Places(DB.RSFieldDecimal(rs, "UpsellProductDiscountPercentage"));
                        txtRequiresProducts.Text = DB.RSField(rs, "RequiresProducts");

                        //SEARCH ENGINE
                        txtSETitle.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SETitle"), locale, false);
                        txtSEKeywords.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEKeywords"), locale, false);
                        txtSEDescription.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEDescription"), locale, false);
                        txtSENoScript.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SENoScript"), locale, false);
                        txtSEAlt.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEAltText"), locale, false);

                        //SPECS
                        txtSpecTitle.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SpecTitle"), locale, false);
                        txtSpecCall.Text = DB.RSField(rs, "SpecCall");
                        rblSpecsInline.SelectedIndex = (DB.RSFieldBool(rs, "SpecsInline") ? 1 : 0);
                        if (pspec.Contents.Length != 0)
                        {
                            ltSpecs1.Text = ("<b>From File: " + pspec.FN + "</b> &nbsp;&nbsp;\n");
                            ltSpecs2.Text = ("<div style=\"border-style: dashed; border-width: 1px;\">\n");
                            ltSpecs2.Text += (pspec.Contents);
                            ltSpecs2.Text += ("</div>\n");
                        }

                        //BG COLOR
                        txtPageBG.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "PageBGColor"), "");
                        txtContentsBG.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ContentsBGColor"), "");
                        txtSkinColor.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "GraphicsColor"), "");

                        // BEGIN IMAGES 
                        txtImageOverride.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ImageFilenameOverride"), "");
                        bool disableupload = (Editing && DB.RSField(rs, "ImageFilenameOverride") != "");
                        if (eSpecs.m_HasIconPic)
                        {
                            fuIcon.Enabled = !disableupload;
                            String Image1URL = AppLogic.LookupImage("Product", pID, "icon", currentSkinID, ThisCustomer.LocaleSetting);
                            if (Image1URL.Length == 0)
                            {
                                Image1URL = AppLogic.NoPictureImageURL(false, currentSkinID, ThisCustomer.LocaleSetting);
                            }
                            if (!CommonLogic.FileExists(Image1URL))
                            {
                                Image1URL = AppLogic.LocateImageURL("Skins/skin_1/images/nopictureicon.gif", ThisCustomer.LocaleSetting);
                            }
                            if (Image1URL.Length != 0)
                            {
                                ltIcon.Text = "";
                                if (Image1URL.IndexOf("nopicture") == -1)
                                {
                                    ltIcon.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image1URL + "','Pic1');\">Click here</a> to delete the current image <br/>\n");
                                }
                                ltIcon.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic1\" name=\"Pic1\" border=\"0\" src=\"" + Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                                if (AppLogic.GetProductsDefaultVariantID(pID) != 0)
                                {
                                    ltIcon.Text += ("&nbsp;&nbsp;<a href=\"javascript:;\" onclick=\"window.open('EntityProductImageMgr.aspx?size=icon&productid=" + pID.ToString() + "','Images','height=450, width=780,  scrollbars=yes, resizable=yes, toolbar=no, status=yes, location=no, directories=no, menubar=no, alwaysRaised=yes');\">Icon Multi-Image Manager</a>");
                                }
                            }
                        }
                        if (eSpecs.m_HasMediumPic)
                        {
                            fuMedium.Enabled = !disableupload;
                            String Image2URL = AppLogic.LookupImage("Product", pID, "medium", currentSkinID, ThisCustomer.LocaleSetting);
                            if (Image2URL.Length == 0)
                            {
                                Image2URL = AppLogic.NoPictureImageURL(false, currentSkinID, ThisCustomer.LocaleSetting);
                            }
                            if (!CommonLogic.FileExists(Image2URL))
                            {
                                Image2URL = AppLogic.LocateImageURL("Skins/skin_1/images/nopicture.gif", ThisCustomer.LocaleSetting);
                            }
                            if (Image2URL.Length != 0)
                            {
                                ltMedium.Text = "";
                                if (Image2URL.IndexOf("nopicture") == -1)
                                {
                                    ltMedium.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image2URL + "','Pic2');\">Click here</a> to delete the current image <br/>\n");
                                }
                                ltMedium.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic2\" name=\"Pic2\" border=\"0\" src=\"" + Image2URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                                if (AppLogic.GetProductsDefaultVariantID(pID) != 0)
                                {
                                    ltMedium.Text += ("&nbsp;&nbsp;<a href=\"javascript:;\" onclick=\"window.open('EntityProductImageMgr.aspx?size=medium&productid=" + pID.ToString() + "','Images','height=550, width=780,  scrollbars=yes, resizable=yes, toolbar=no, status=yes, location=no, directories=no, menubar=no, alwaysRaised=yes');\">Medium Multi-Image Manager</a>");
                                }
                            }
                        }
                        if (eSpecs.m_HasLargePic)
                        {
                            fuLarge.Enabled = !disableupload;
                            String Image3URL = AppLogic.LookupImage("Product", pID, "large", currentSkinID, ThisCustomer.LocaleSetting);
                            if (Image3URL.Length == 0)
                            {
                                Image3URL = AppLogic.NoPictureImageURL(false, currentSkinID, ThisCustomer.LocaleSetting);
                            }
                            if (!CommonLogic.FileExists(Image3URL))
                            {
                                Image3URL = AppLogic.LocateImageURL("Skins/skin_1/images/nopicture.gif", ThisCustomer.LocaleSetting);
                            }
                            if (Image3URL.Length != 0)
                            {
                                ltLarge.Text = "";
                                if (Image3URL.IndexOf("nopicture") == -1)
                                {
                                    ltLarge.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image3URL + "','Pic3');\">Click here</a> to delete the current image <br/>\n");
                                }
                                ltLarge.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic3\" name=\"Pic3\" border=\"0\" src=\"" + Image3URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                                if (AppLogic.GetProductsDefaultVariantID(pID) != 0)
                                {
                                    ltLarge.Text += ("&nbsp;&nbsp;<a href=\"javascript:;\" onclick=\"window.open('EntityProductImageMgr.aspx?size=large&productid=" + pID.ToString() + "','Images','height=650, width=780,  scrollbars=yes, resizable=yes, toolbar=no, status=yes, location=no, directories=no, menubar=no, alwaysRaised=yes');\">Large Multi-Image Manager</a>");
                                }
                            }
                        }
                        String Image4URL = AppLogic.LookupImage("Product", pID, "swatch", currentSkinID, ThisCustomer.LocaleSetting);
                        if (Image4URL.Length == 0)
                        {
                            Image4URL = AppLogic.NoPictureImageURL(false, currentSkinID, ThisCustomer.LocaleSetting);
                        }
                        if (!CommonLogic.FileExists(Image4URL))
                        {
                            Image4URL = AppLogic.LocateImageURL("Skins/skin_1/images/nopictureicon.gif", ThisCustomer.LocaleSetting);
                        }
                        if (Image4URL.Length != 0)
                        {
                            ltSwatch.Text = "";
                            if (Image4URL.IndexOf("nopicture") == -1)
                            {
                                ltSwatch.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image4URL + "','Pic4');\">Click here</a> to delete the current image <br/>\n");
                            }
                            ltSwatch.Text += "<img style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic4\" name=\"Pic4\" border=\"0\" src=\"" + Image4URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                        }
                        txtSwatchImageMap.Text = DB.RSField(rs, "SwatchImageMap");
                        // END IMAGES   

                        //VARIANTS
                    }
                    else
                    {

                        if (AppLogic.MaxProductsExceeded())
                        {
                            btnSubmit.Enabled = false;
                            btnSubmit.CssClass = "normalButtonsDisabled";
                        }
                        else
                        {
                            btnSubmit.Enabled = true;
                            btnSubmit.CssClass = "normalButtons";
                        }

                        pnlVariant.Visible = true;

                        this.phAllVariants.Visible = false;
                        this.phAddVariant.Visible = true;
                        divltStatus2.Visible = false;
                        ltStatus.Text = "Adding Product";
                        btnSubmit.Text = "Add New";

                        txtPageSize.Text = AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "PageSize");
                        txtColumn.Text = AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "ColWidth");

                        //match Type
                        foreach (ListItem li in ddType.Items)
                        {
                            if (li.Value.Equals(AppLogic.AppConfigUSInt("Admin_DefaultProductTypeID").ToString()))
                            {
                                ddType.SelectedValue = AppLogic.AppConfigUSInt("Admin_DefaultProductTypeID").ToString();
                            }
                        }

                        //match On Sale
                        foreach (ListItem li in ddOnSalePrompt.Items)
                        {
                            if (li.Value.Equals(AppLogic.AppConfigUSInt("Admin_DefaultSalesPromptID").ToString()))
                            {
                                ddOnSalePrompt.SelectedValue = AppLogic.AppConfigUSInt("Admin_DefaultSalesPromptID").ToString();
                            }
                        }

                        if (!bUseHtmlEditor)
                        {
                            //DESCRIPTION
                            ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                            ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\"></textarea>\n");
                            ltDescription.Text += ("</div>");

                            //SUMMARY
                            ltSummary.Text = ("<div id=\"idSummary\" style=\"height: 1%;\">");
                            ltSummary.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Summary\" name=\"Summary\"></textarea>\n");
                            ltSummary.Text += ("</div>");
                        }

                        //EXTENSION DATA
                        txtExtensionData.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData"), "");

                        //MISCELLANEOUS DATA
                        txtExtensionData.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "MiscellaneousData"), "");

                        //VARIANT
                        trInventory.Visible = false;
                        if (!ProductTracksInventoryBySizeAndColor)
                        {
                            trInventory.Visible = true;
                        }

                        //set defaults
                        rblExcludeFroogle.SelectedIndex = 0;
                        rblHidePriceUntilCart.SelectedIndex = 0;
                        rblIsCallToOrder.SelectedIndex = 0;
                        rblIsKit.SelectedIndex = 0;
                        rblPublished.SelectedIndex = 1;
                        rblRequiresRegistrationToView.SelectedIndex = 0;
                        rblRequiresTextField.SelectedIndex = 0;
                        rblShowBuyButton.SelectedIndex = 1;
                        rblSpecsInline.SelectedIndex = 0;
                        rblTrackSizeColor.SelectedIndex = 0;

                        //MAPPINGS
                        if (eID > 0)
                        {
                            //match Manufacturer
                            if (eName.Equals("MANUFACTURER", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ddManufacturer.ClearSelection();
                                foreach (ListItem li in ddManufacturer.Items)
                                {
                                    if (li.Value.Equals(eID.ToString()))
                                    {
                                        ddManufacturer.SelectedValue = eID.ToString();
                                    }
                                }
                            }

                            //match Distributor
                            if (eName.Equals("DISTRIBUTOR", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ddDistributor.ClearSelection();
                                foreach (ListItem li in ddDistributor.Items)
                                {
                                    if (li.Value.Equals(eID.ToString()))
                                    {
                                        ddDistributor.SelectedValue = eID.ToString();
                                    }
                                }
                            }

                            //Category
                            if (eName.Equals("CATEGORY", StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (ListItem li in cblCategory.Items)
                                {
                                    if (li.Value.Equals(eID.ToString()))
                                    {
                                        li.Selected = true;
                                    }
                                }
                            }

                            //Section
                            if (eName.Equals("SECTION", StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (ListItem li in cblDepartment.Items)
                                {
                                    if (li.Value.Equals(eID.ToString()))
                                    {
                                        li.Selected = true;
                                    }
                                }
                            }

                            //Affiliate
                            if (eName.Equals("AFFILIATE", StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (ListItem li in cblAffiliates.Items)
                                {
                                    if (li.Value.Equals(eID.ToString()))
                                    {
                                        li.Selected = true;
                                    }
                                }
                            }
                        }
                    }

                    ltScript2.Text = ("<script type=\"text/javascript\">\n");
                    ltScript2.Text += ("function DeleteImage(imgurl,name)\n");
                    ltScript2.Text += ("{\n");
                    ltScript2.Text += ("if(confirm('Are you sure you want to delete this image?')){\n");
                    ltScript2.Text += ("window.open('deleteimage.aspx?imgurl=' + imgurl + '&FormImageName=' + name,\"Admin_ML\",\"height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
                    ltScript2.Text += ("}}\n");
                    ltScript2.Text += ("</SCRIPT>\n");

                    //create iframe
                    ltIFrame.Text = "<iframe src=\"entityProductVariants.aspx?ProductID=" + pID + "&entityname=" + eSpecs.m_EntityName + "&EntityID=" + eID.ToString() + "\" name=\"variantFrame\" id=\"variantFrame\" frameborder=\"0\" height=\"350\" onfocus=\"calcHeight();\" scrolling=\"auto\" width=\"903px\" marginheight=\"0\" marginwidth=\"0\"></iframe>";
                    pnlVariant.Attributes.Add("onFocus", "calcHeight();");

                }
            }
        }


        protected void ddLocale_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddLocale.SelectedValue != string.Empty)
            {
                ThisCustomer.ThisCustomerSession.SetVal("entityUserLocale", ddLocale.SelectedValue);
            }
            else
            {
                ThisCustomer.ThisCustomerSession.SetVal("entityUserLocale", Localization.GetDefaultLocale());
            }
            LoadContent();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            UpdateProduct();
            etsMapper.Save();
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            UpdateProduct();
            etsMapper.Save();

            ClientScript.RegisterStartupScript(Page.GetType(), Guid.NewGuid().ToString(), "CloseAndRebind(" + pID + ");", true);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(Page.GetType(), Guid.NewGuid().ToString(), "CancelEdit();", true);
        }

        protected void UpdateProduct()
        {
            if (!Page.IsValid)
            {
                return;
            }
            bool Editing = Localization.ParseBoolean(ViewState["ProductEdit"].ToString());
            StringBuilder sql = new StringBuilder(2500);

            string locale = ddLocale.SelectedValue;
            if (locale.Equals(string.Empty))
            {
                locale = Localization.GetDefaultLocale();
            }

            if (!Editing)
            {
                // ok to ADD them:
                String NewGUID = DB.GetNewGUID();
                sql.Append("insert into product(ProductGUID,Name,SEName,ContentsBGColor,PageBGColor,GraphicsColor,ImageFilenameOverride,ProductTypeID,Summary,Description,ExtensionData,ColorOptionPrompt,SizeOptionPrompt,RequiresTextOption,TextOptionPrompt,TextOptionMaxLength,FroogleDescription,RelatedProducts,UpsellProducts,UpsellProductDiscountPercentage,RequiresProducts,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,SKU,PageSize,ColWidth,XmlPackage,FundID,ManufacturerPartNumber,SalesPromptID,SpecTitle,SpecCall,Published,ShowBuyButton,IsCallToOrder,HidePriceUntilCart,ExcludeFromPriceFeeds,IsAKit" + CommonLogic.IIF(AppLogic.AppConfigBool("ShowPopularity"), ",Popularity", "") + ",TrackInventoryBySizeAndColor,TrackInventoryBySize,TrackInventoryByColor,RequiresRegistration,SpecsInline,MiscText,SwatchImageMap,QuantityDiscountID,TaxClassID,AvailableStartDate,AvailableStopDate) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name", txtName.Text, locale, "Product", pID)) + ",");
                sql.Append(DB.SQuote(SE.MungeName(AppLogic.GetFormsDefaultLocale("Name", txtName.Text, locale, "Product", pID))) + ",");
                sql.Append(DB.SQuote(txtContentsBG.Text) + ",");
                sql.Append(DB.SQuote(txtPageBG.Text) + ",");
                sql.Append(DB.SQuote(txtSkinColor.Text) + ",");
                sql.Append(DB.SQuote(txtImageOverride.Text) + ",");
                sql.Append(ddType.SelectedValue + ",");
                if (bUseHtmlEditor)
                {
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Summary", radSummary.Content, locale, "product", eID)) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Description", radDescription.Content, locale, "product", eID)) + ",");
                }
                else
                {
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXmlEditor("Summary", "Summary", locale, "Product", pID)) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXmlEditor("Description", "Description", locale, "Product", pID)) + ",");
                }
                sql.Append(DB.SQuote(txtExtensionData.Text) + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml("ColorOptionPrompt", txtColorOption.Text, locale, "Product", pID)) + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SizeOptionPrompt", txtSizeOption.Text, locale, "Product", pID)) + ",");
                sql.Append(rblRequiresTextField.SelectedValue + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml("TextOptionPrompt", txtTextFieldPrompt.Text, locale, "Product", pID)) + ",");
                if (txtTextOptionMax.Text.Length != 0)
                {
                    sql.Append(Localization.ParseNativeInt(txtTextOptionMax.Text).ToString() + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml("FroogleDescription", txtFroogle.Text, locale, "Product", pID)) + ",");
                if (txtRelatedProducts.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtRelatedProducts.Text.Trim().Replace(" ", "")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtUpsellProducts.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtUpsellProducts.Text.Trim().Replace(" ", "")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(Localization.DecimalStringForDB(Localization.ParseNativeDecimal(txtUpsellProductsDiscount.Text)) + ",");
                if (txtRequiresProducts.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtRequiresProducts.Text.Trim().Replace(" ", "")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }

                if (txtSEKeywords.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SEKeywords", txtSEKeywords.Text, locale, "Product", pID)) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtSEDescription.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SEDescription", txtSEDescription.Text, locale, "Product", pID)) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtSETitle.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SETitle", txtSETitle.Text, locale, "Product", pID)) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtSENoScript.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SENoScript", txtSENoScript.Text, locale, "Product", pID)) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtSEAlt.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SEAltText", txtSEAlt.Text, locale, "Product", pID)) + ",");
                }
                else
                {
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name", txtName.Text, locale, "Product", pID)) + ",");
                }

                if (txtSKU.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtSKU.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(CommonLogic.IIF(txtPageSize.Text.Length == 0, AppLogic.AppConfig("Default_ProductPageSize"), txtPageSize.Text) + ",");
                sql.Append(CommonLogic.IIF(txtColumn.Text.Length == 0, AppLogic.AppConfig("Default_ProductColWidth"), txtColumn.Text) + ",");
                if (ddXmlPackage.SelectedValue != "0")
                {
                    sql.Append(DB.SQuote(ddXmlPackage.SelectedValue.ToLowerInvariant()) + ",");
                }
                else
                {
                    if (rblIsKit.SelectedValue == "1")
                    {
                        sql.Append(DB.SQuote(AppLogic.ro_DefaultProductKitXmlPackage) + ","); // force a default!
                    }
                    else
                    {
                        sql.Append(DB.SQuote(AppLogic.ro_DefaultProductXmlPackage) + ","); // force a default!
                    }
                }
                if (ddFund.SelectedValue != "0")
                {
                    sql.Append(DB.SQuote(ddFund.SelectedValue.ToLowerInvariant()) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(DB.SQuote(txtManufacturePartNumber.Text) + ",");
                sql.Append(ddOnSalePrompt.SelectedValue + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml("SpecTitle", txtSpecTitle.Text, locale, "Product", pID)) + ",");
                if (txtSpecCall.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtSpecCall.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(rblPublished.SelectedValue + ",");
                sql.Append(rblShowBuyButton.SelectedValue + ",");
                sql.Append(rblIsCallToOrder.SelectedValue + ",");
                sql.Append(rblHidePriceUntilCart.SelectedValue + ",");
                sql.Append(rblExcludeFroogle.SelectedValue + ",");
                sql.Append(rblIsKit.SelectedValue + ",");
                if (AppLogic.AppConfigBool("ShowPopularity"))
                {
                    sql.Append(Localization.ParseNativeInt(txtPopularity.Text).ToString() + ",");
                }
                if (rblIsKit.SelectedValue == "1")
                {
                    sql.Append("0,"); // cannot track inventory by size and color
                    sql.Append("0,"); // cannot track inventory by size and color
                    sql.Append("0,"); // cannot track inventory by size and color
                }
                else
                {
                    sql.Append(rblTrackSizeColor.SelectedValue + ",");
                    sql.Append(rblTrackSizeColor.SelectedValue + ","); // this is correct. change made in v6.1.1.1
                    sql.Append(rblTrackSizeColor.SelectedValue + ","); // this is correct. change made in v6.1.1.1
                }
                sql.Append(rblRequiresRegistrationToView.SelectedValue + ",");
                sql.Append(rblSpecsInline.SelectedValue + ",");
                if (txtMiscText.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtMiscText.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtSwatchImageMap.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtSwatchImageMap.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(ddDiscountTable.SelectedValue + ",");
                sql.Append(CommonLogic.IIF(ddTaxClass.SelectedValue == "0", AppLogic.AppConfig("Admin_DefaultTaxClassID"), ddTaxClass.SelectedValue) + ",");

                //Dates
                if (txtStartDate.Text.Trim().Length != 0)
                {
                    sql.Append(DB.SQuote(Localization.ToDBShortDateString(Localization.ParseNativeDateTime(txtStartDate.Text))) + ",");
                }
                else
                {
                    sql.Append("getdate(),");
                }
                if (txtStopDate.Text.Trim().Length != 0)
                {
                    sql.Append(DB.SQuote(Localization.ToDBShortDateString(Localization.ParseNativeDateTime(txtStopDate.Text))) + "");
                }
                else
                {
                    sql.Append("NULL");
                }

                sql.Append(")");
                DB.ExecuteSQL(sql.ToString());

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select ProductID from product   with (NOLOCK)  where deleted=0 and ProductGUID=" + DB.SQuote(NewGUID), dbconn))
                    {
                        rs.Read();
                        pID = DB.RSFieldInt(rs, "ProductID");
                        ViewState.Add("ProductEdit", true);
                        ViewState.Add("ProductEditID", pID);
                        etsMapper.ObjectID = pID;
                    }
                }
                // ARE WE ADDING A SIMPLE PRODUCT, IF SO, CREATE THE DEFAULT VARIANT
                #region InsertDefaultVariant


                string sqlInsertVariant = @"
            INSERT INTO ProductVariant(VariantGUID,Name,IsDefault,ProductID,Price,SalePrice,Weight,Dimensions,Inventory,Published,Colors,ColorSKUModifiers,Sizes,SizeSKUModifiers) VALUES(
                @NewGUID, @VariantName, 1, @ProductID, ISNULL(@Price, 0), 
                @SalePrice, @Weight, @Dimensions, ISNULL(@Inventory, 10000), 1,
                @Colors, @ColorSkuModifiers, @Sizes, @SizesSKUModifiers)
            ";
                System.Collections.Generic.List<SqlParameter> _params = new System.Collections.Generic.List<SqlParameter>(new SqlParameter[]
            {
                new SqlParameter("@NewGUID",new Guid(DB.GetNewGUID())),
                new SqlParameter("@VariantName", ""),
                new SqlParameter("@ProductID", pID),
                new SqlParameter("@Dimensions", txtDimensions.Text),
                new SqlParameter("@Colors", txtColors.Text),
                new SqlParameter("@ColorSkuModifiers", txtColorSKUModifiers.Text),
                new SqlParameter("@Sizes", txtSizes.Text),
                new SqlParameter("@SizesSKUModifiers", txtSizeSKUModifiers.Text)
                });

                SqlParameter prmPrice = new SqlParameter("@Price", SqlDbType.Decimal); prmPrice.Value = DBNull.Value;
                SqlParameter prmSalePrice = new SqlParameter("@SalePrice", SqlDbType.Decimal); prmSalePrice.Value = DBNull.Value;
                SqlParameter prmWeight = new SqlParameter("@Weight", SqlDbType.Decimal); prmWeight.Value = DBNull.Value;
                SqlParameter prmInventory = new SqlParameter("@Inventory", SqlDbType.BigInt); prmInventory.Value = DBNull.Value;

                if (txtPrice.Text.Length != 0)
                    prmPrice.Value = Localization.ParseNativeDecimal(txtPrice.Text);
                if (txtSalePrice.Text.Length != 0)
                    prmSalePrice.Value = Localization.ParseNativeDecimal(txtSalePrice.Text);
                if (txtWeight.Text.Length != 0)
                    prmWeight.Value = Localization.ParseNativeDecimal(txtWeight.Text);
                if (txtInventory.Text.Length != 0)
                    prmInventory.Value = Localization.ParseNativeInt(txtInventory.Text);
                else if (CommonLogic.IsInteger(AppLogic.AppConfig("Admin_DefaultInventory")))
                    prmInventory.Value = AppLogic.AppConfigNativeInt("Admin_DefaultInventory");
                _params.AddRange(new SqlParameter[]{
               prmPrice,
               prmSalePrice,
               prmInventory,
               prmWeight
            });

                DB.ExecuteSQL(sqlInsertVariant, _params.ToArray());

                resetError("Product Added and Default variant were added.", false);

                #endregion

            }
            else
            {
                sql = new StringBuilder();
                // ok to update:
                sql.Append("update product set ");
                sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name", txtName.Text, locale, "Product", pID)) + ",");
                sql.Append("SEName=" + DB.SQuote(SE.MungeName(AppLogic.GetFormsDefaultLocale("Name", txtName.Text, locale, "Product", pID))) + ",");
                sql.Append("ContentsBGColor=" + DB.SQuote(txtContentsBG.Text) + ",");
                sql.Append("PageBGColor=" + DB.SQuote(txtPageBG.Text) + ",");
                sql.Append("GraphicsColor=" + DB.SQuote(txtSkinColor.Text) + ",");
                sql.Append("ImageFilenameOverride=" + DB.SQuote(txtImageOverride.Text) + ",");
                sql.Append("ProductTypeID=" + ddType.SelectedValue + ",");
                if (bUseHtmlEditor)
                {
                    sql.Append("Summary=" + DB.SQuote(AppLogic.FormLocaleXml("Summary", radSummary.Content, locale, "product", pID)) + ",");
                    sql.Append("Description=" + DB.SQuote(AppLogic.FormLocaleXml("Description", radDescription.Content, locale, "product", pID)) + ",");

                }
                else
                {
                    sql.Append("Summary=" + DB.SQuote(AppLogic.FormLocaleXmlEditor("Summary", "Summary", locale, "Product", pID)) + ",");
                    sql.Append("Description=" + DB.SQuote(AppLogic.FormLocaleXmlEditor("Description", "Description", locale, "Product", pID)) + ",");
                }
                sql.Append("ExtensionData=" + DB.SQuote(txtExtensionData.Text) + ",");
                sql.Append("ColorOptionPrompt=" + DB.SQuote(AppLogic.FormLocaleXml("ColorOptionPrompt", txtColorOption.Text, locale, "Product", pID)) + ",");
                sql.Append("SizeOptionPrompt=" + DB.SQuote(AppLogic.FormLocaleXml("SizeOptionPrompt", txtSizeOption.Text, locale, "Product", pID)) + ",");
                sql.Append("RequiresTextOption=" + rblRequiresTextField.SelectedValue + ",");
                sql.Append("TextOptionPrompt=" + DB.SQuote(AppLogic.FormLocaleXml("TextOptionPrompt", txtTextFieldPrompt.Text, locale, "Product", pID)) + ",");
                sql.Append("TextOptionMaxLength=" + Localization.ParseNativeInt(txtTextOptionMax.Text) + ",");
                sql.Append("FroogleDescription=" + DB.SQuote(AppLogic.FormLocaleXml("FroogleDescription", txtFroogle.Text, locale, "Product", pID)) + ",");
                sql.Append("RelatedProducts=" + DB.SQuote(txtRelatedProducts.Text.Trim().Replace(" ", "")) + ",");
                sql.Append("UpsellProducts=" + DB.SQuote(txtUpsellProducts.Text.Trim().Replace(" ", "")) + ",");
                sql.Append("UpsellProductDiscountPercentage=" + Localization.DecimalStringForDB(Localization.ParseNativeDecimal(txtUpsellProductsDiscount.Text)) + ",");
                sql.Append("RequiresProducts=" + DB.SQuote(txtRequiresProducts.Text.Trim().Replace(" ", "")) + ",");

                sql.Append("SEKeywords=" + DB.SQuote(AppLogic.FormLocaleXml("SEKeywords", txtSEKeywords.Text, locale, "Product", pID)) + ",");
                sql.Append("SEDescription=" + DB.SQuote(AppLogic.FormLocaleXml("SEDescription", txtSEDescription.Text, locale, "Product", pID)) + ",");
                sql.Append("SETitle=" + DB.SQuote(AppLogic.FormLocaleXml("SETitle", txtSETitle.Text, locale, "Product", pID)) + ",");
                sql.Append("SENoScript=" + DB.SQuote(AppLogic.FormLocaleXml("SENoScript", txtSENoScript.Text, locale, "Product", pID)) + ",");
                sql.Append("SEAltText=" + DB.SQuote(AppLogic.FormLocaleXml("SEAltText", txtSEAlt.Text, locale, "Product", pID)) + ",");

                if (txtSKU.Text.Length != 0)
                {
                    sql.Append("SKU=" + DB.SQuote(txtSKU.Text) + ",");
                }
                else
                {
                    sql.Append("SKU=NULL,");
                }

                sql.Append("PageSize=" + CommonLogic.IIF(txtPageSize.Text.Length == 0, AppLogic.AppConfigNativeInt("Default_ProductPageSize").ToString(), Localization.ParseNativeInt(txtPageSize.Text).ToString()) + ",");
                sql.Append("ColWidth=" + CommonLogic.IIF(txtColumn.Text.Length == 0, AppLogic.AppConfigNativeInt("Default_ProductColWidth").ToString(), Localization.ParseNativeInt(txtColumn.Text).ToString()) + ",");

                if (ddXmlPackage.SelectedValue != "0")
                {
                    sql.Append("XmlPackage=" + DB.SQuote(ddXmlPackage.SelectedValue.ToLowerInvariant()) + ",");
                }
                else
                {
                    if (rblIsKit.SelectedValue == "1")
                    {
                        sql.Append("XmlPackage=" + DB.SQuote(AppLogic.ro_DefaultProductKitXmlPackage) + ","); // force a default!
                    }
                    else
                    {
                        sql.Append("XmlPackage=" + DB.SQuote(AppLogic.ro_DefaultProductXmlPackage) + ","); // force a default!
                    }
                }

                if (ddFund.SelectedValue != "0")
                {
                    sql.Append("FundID=" + DB.SQuote(ddFund.SelectedValue.ToLowerInvariant()) + ",");
                }
                else
                {
                    sql.Append("FundID=NULL,");
                }
                sql.Append("ManufacturerPartNumber=" + DB.SQuote(txtManufacturePartNumber.Text) + ",");
                sql.Append("SalesPromptID=" + Localization.ParseNativeInt(ddOnSalePrompt.SelectedValue).ToString() + ",");
                sql.Append("SpecTitle=" + DB.SQuote(AppLogic.FormLocaleXml("SpecTitle", txtSpecTitle.Text, locale, "Product", pID)) + ",");

                if (txtSpecCall.Text.Length != 0)
                {
                    sql.Append("SpecCall=" + DB.SQuote(txtSpecCall.Text) + ",");
                }
                else
                {
                    sql.Append("SpecCall=NULL,");
                }
                sql.Append("Published=" + rblPublished.SelectedValue + ",");
                sql.Append("ShowBuyButton=" + rblShowBuyButton.SelectedValue + ",");
                sql.Append("IsCallToOrder=" + rblIsCallToOrder.SelectedValue + ",");
                sql.Append("HidePriceUntilCart=" + rblHidePriceUntilCart.SelectedValue + ",");
                sql.Append("ExcludeFromPriceFeeds=" + rblExcludeFroogle.SelectedValue + ",");
                sql.Append("IsAKit=" + rblIsKit.SelectedValue + ",");
                if (AppLogic.AppConfigBool("ShowPopularity"))
                {
                    sql.Append("Popularity=" + Localization.ParseNativeInt(txtPopularity.Text).ToString() + ",");
                }

                if (rblIsKit.SelectedValue == "1")
                {
                    sql.Append("TrackInventoryBySizeAndColor=0,"); // cannot track inventory by size and color
                    sql.Append("TrackInventoryBySize=0,");
                    sql.Append("TrackInventoryByColor=0,");
                }
                else
                {
                    sql.Append("TrackInventoryBySizeAndColor=" + rblTrackSizeColor.SelectedValue + ",");
                    sql.Append("TrackInventoryBySize=" + rblTrackSizeColor.SelectedValue + ","); // this is correct. change made in v6.1.1.1
                    sql.Append("TrackInventoryByColor=" + rblTrackSizeColor.SelectedValue + ","); // this is correct. change made in v6.1.1.1
                }

                sql.Append("RequiresRegistration=" + rblRequiresRegistrationToView.SelectedValue + ",");
                sql.Append("SpecsInline=" + rblSpecsInline.SelectedValue + ",");

                if (txtMiscText.Text.Length != 0)
                {
                    sql.Append("MiscText=" + DB.SQuote(txtMiscText.Text) + ",");
                }
                else
                {
                    sql.Append("MiscText=NULL,");
                }
                if (txtSwatchImageMap.Text.Length != 0)
                {
                    sql.Append("SwatchImageMap=" + DB.SQuote(txtSwatchImageMap.Text) + ",");
                }
                else
                {
                    sql.Append("SwatchImageMap=NULL,");
                }
                sql.Append("QuantityDiscountID=" + Localization.ParseNativeInt(ddDiscountTable.SelectedValue) + ",");

                sql.Append("TaxClassID=" + Localization.ParseNativeInt(CommonLogic.IIF(ddTaxClass.SelectedValue == "0", AppLogic.AppConfig("Admin_DefaultTaxClassID"), ddTaxClass.SelectedValue)) + ",");

                //Dates
                if (txtStartDate.Text.Length != 0)
                {
                    sql.Append("AvailableStartDate=" + DB.SQuote(Localization.ToDBShortDateString(Localization.ParseNativeDateTime(txtStartDate.Text))) + ",");
                }
                else
                {
                    sql.Append("AvailableStartDate=getdate(),");
                }
                if (txtStopDate.Text.Length != 0)
                {
                    sql.Append("AvailableStopDate=" + DB.SQuote(Localization.ToDBShortDateString(Localization.ParseNativeDateTime(txtStopDate.Text))) + "");
                }
                else
                {
                    sql.Append("AvailableStopDate=NULL");
                }


                sql.Append(" where ProductID=" + pID.ToString());
                DB.ExecuteSQL(sql.ToString());

                ViewState.Add("ProductEdit", true);
                ViewState.Add("ProductEditID", pID);

                resetError("Product Updated.", false);
            }

            // UPDATE 1:1 ENTITY MAPPINGS:
            {
                String[] Entities = { "Manufacturer", "Distributor" };
                foreach (String en in Entities)
                {
                    int NewID = 0;
                    if (en.Equals("Manufacturer"))
                    {
                        NewID = Localization.ParseNativeInt(ddManufacturer.SelectedValue);
                    }
                    else
                    {
                        NewID = Localization.ParseNativeInt(ddDistributor.SelectedValue);
                    }
                    if (NewID == 0)
                    {
                        // no mapping (should not be allowed by form validator, but...)
                        DB.ExecuteSQL("delete from Product" + en + " where ProductID=" + pID.ToString());
                    }
                    else
                    {
                        int OldID = CommonLogic.IIF(en == "Manufacturer", AppLogic.GetProductManufacturerID(pID), AppLogic.GetProductDistributorID(pID));
                        if (OldID == 0)
                        {
                            // create default mapping:
                            DB.ExecuteSQL(String.Format("insert into Product{0}(ProductID,{1}ID,DisplayOrder) values({2},{3},1)", en, en, pID.ToString(), NewID.ToString()));
                        }
                        else if (OldID != NewID)
                        {
                            // update existing mapping:
                            DB.ExecuteSQL(String.Format("update Product{0} set {1}ID={2} where {3}ID={4} and ProductID={5}", en, en, NewID.ToString(), en, OldID.ToString(), pID.ToString()));
                        }
                    }
                }
            }

            // UPDATE 1:N ENTITY MAPPINGS:
            {
                String[] Entities2 = { "Category", "Section", "Affiliate", "CustomerLevel", "Genre", "Vector" };

                foreach (String en in Entities2)
                {
                    String EnMap = "";
                    if (en.Equals("Category"))
                    {
                        foreach (ListItem li in cblCategory.Items)
                        {
                            if (li.Selected)
                            {
                                EnMap += "," + li.Value;
                            }
                        }
                    }
                    else if (en.Equals("Section"))
                    {
                        foreach (ListItem li in cblDepartment.Items)
                        {
                            if (li.Selected)
                            {
                                EnMap += "," + li.Value;
                            }
                        }
                    }
                    else if (en.Equals("Affiliate"))
                    {
                        foreach (ListItem li in cblAffiliates.Items)
                        {
                            if (li.Selected)
                            {
                                EnMap += "," + li.Value;
                            }
                        }
                    }
                    else if (en.Equals("CustomerLevel"))
                    {
                        foreach (ListItem li in cblCustomerLevels.Items)
                        {
                            if (li.Selected)
                            {
                                EnMap += "," + li.Value;
                            }
                        }
                    }
                    else if (en.Equals("Genre"))
                    {
                        foreach (ListItem li in cblGenres.Items)
                        {
                            if (li.Selected)
                            {
                                EnMap += "," + li.Value;
                            }
                        }
                    }
                    else
                    {
                        foreach (ListItem li in cblVectors.Items)
                        {
                            if (li.Selected)
                            {
                                EnMap += "," + li.Value;
                            }
                        }
                    }

                    if (EnMap.Length > 1)
                    {
                        EnMap = EnMap.Substring(1);// Remove the leading ,
                    }

                    if (EnMap.Length == 0)
                    {
                        // no mappings
                        DB.ExecuteSQL(String.Format("delete from Product{0} where ProductID={1}", en, pID.ToString()));
                    }
                    else
                    {
                        // remove any mappings not current anymore:
                        DB.ExecuteSQL(String.Format("delete from Product{0} where ProductID={1} and {2}ID not in ({3})", en, pID.ToString(), en, EnMap));
                        // add new default mappings:
                        String[] EnMapArray = EnMap.Split(',');
                        foreach (String EntityID in EnMapArray)
                        {
                            try
                            {
                                DB.ExecuteSQL(String.Format("insert Product{0}(ProductID,{1}ID,DisplayOrder) values({2},{3},1)", en, en, pID.ToString(), EntityID));
                            }
                            catch { }
                        }
                    }
                }
            }

            //Upload Images
            HandleImageSubmits();

            LoadContent();
        }

        public void HandleImageSubmits()
        {
            // handle image uploaded:
            String FN = txtImageOverride.Text.Trim();
            if (AppLogic.AppConfigBool("UseSKUForProductImageName"))
            {
                // txtSKU is in a RadWindow, so needs to be accessed through UniqueID: tabContainer$pnlMain$txtSKU
                FN = CommonLogic.FormCanBeDangerousContent("tabContainer$pnlMain$txtSKU").Trim();
            }
            if (FN.Length == 0)
            {
                FN = pID.ToString();
            }

            String Image1 = String.Empty;
            String TempImage1 = String.Empty;
            HttpPostedFile Image1File = fuIcon.PostedFile;
            if (Image1File != null && Image1File.ContentLength != 0)
            {
                // delete any current image file first
                try
                {
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath("Product", "icon", true) + FN);
                        }
                        else
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath("Product", "icon", true) + FN + ss);
                        }
                    }
                }
                catch
                { }

                switch (Image1File.ContentType)
                {
                    case "image/gif":
                        TempImage1 = AppLogic.GetImagePath("Product", "icon", true) + "tmp_" + FN + ".gif";
                        Image1 = AppLogic.GetImagePath("Product", "icon", true) + FN + ".gif";
                        Image1File.SaveAs(TempImage1);
                        AppLogic.ResizeEntityOrObject("Product", TempImage1, Image1, "icon", "image/gif");
                        AppLogic.DisposeOfTempImage(TempImage1);

                        break;
                    case "image/x-png":
                    case "image/png":
                        TempImage1 = AppLogic.GetImagePath("Product", "icon", true) + "tmp_" + FN + ".png";
                        Image1 = AppLogic.GetImagePath("Product", "icon", true) + FN + ".png";
                        Image1File.SaveAs(TempImage1);
                        AppLogic.ResizeEntityOrObject("Product", TempImage1, Image1, "icon", "image/png");

                        AppLogic.DisposeOfTempImage(TempImage1);
                        break;
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":
                        TempImage1 = AppLogic.GetImagePath("Product", "icon", true) + "tmp_" + FN + ".jpg";
                        Image1 = AppLogic.GetImagePath("Product", "icon", true) + FN + ".jpg";
                        Image1File.SaveAs(TempImage1);
                        AppLogic.ResizeEntityOrObject("Product", TempImage1, Image1, "icon", "image/jpeg");
                        AppLogic.DisposeOfTempImage(TempImage1);

                        break;
                }
            }

            String Image2 = String.Empty;
            String TempImage2 = String.Empty;
            HttpPostedFile Image2File = fuMedium.PostedFile;
            if (Image2File != null && Image2File.ContentLength != 0)
            {
                // delete any current image file first
                try
                {
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        System.IO.File.Delete(AppLogic.GetImagePath("Product", "medium", true) + FN + ss);
                    }
                }
                catch
                { }

                switch (Image2File.ContentType)
                {
                    case "image/gif":

                        TempImage2 = AppLogic.GetImagePath("Product", "medium", true) + "tmp_" + FN + ".gif";
                        Image2 = AppLogic.GetImagePath("Product", "medium", true) + FN + ".gif";
                        Image2File.SaveAs(TempImage2);
                        AppLogic.ResizeEntityOrObject("Product", TempImage2, Image2, "medium", "image/gif");
                        AppLogic.DisposeOfTempImage(TempImage2);

                        break;
                    case "image/x-png":
                    case "image/png":

                        TempImage2 = AppLogic.GetImagePath("Product", "medium", true) + "tmp_" + FN + ".png";
                        Image2 = AppLogic.GetImagePath("Product", "medium", true) + FN + ".png";
                        Image2File.SaveAs(TempImage2);
                        AppLogic.ResizeEntityOrObject("Product", TempImage2, Image2, "medium", "image/png");
                        AppLogic.DisposeOfTempImage(TempImage2);

                        break;
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":

                        TempImage2 = AppLogic.GetImagePath("Product", "medium", true) + "tmp_" + FN + ".jpg";
                        Image2 = AppLogic.GetImagePath("Product", "medium", true) + FN + ".jpg";
                        Image2File.SaveAs(TempImage2);
                        AppLogic.ResizeEntityOrObject("Product", TempImage2, Image2, "medium", "image/jpeg");
                        AppLogic.DisposeOfTempImage(TempImage2);

                        break;
                }
            }

            String Image3 = String.Empty;
            String TempImage3 = String.Empty;
            HttpPostedFile Image3File = fuLarge.PostedFile;
            if (Image3File != null && Image3File.ContentLength != 0)
            {
                // delete any current image file first
                try
                {
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        System.IO.File.Delete(AppLogic.GetImagePath("Product", "large", true) + FN + ss);
                    }
                }
                catch
                { }

                switch (Image3File.ContentType)
                {
                    case "image/gif":

                        TempImage3 = AppLogic.GetImagePath("Product", "large", true) + "tmp_" + FN + ".gif";
                        Image3 = AppLogic.GetImagePath("Product", "large", true) + FN + ".gif";
                        Image3File.SaveAs(TempImage3);
                        AppLogic.ResizeEntityOrObject("Product", TempImage3, Image3, "large", "image/gif");
                        AppLogic.CreateOthersFromLarge("Product", TempImage3, FN, "image/gif");

                        break;
                    case "image/x-png":
                    case "image/png":

                        TempImage3 = AppLogic.GetImagePath("Product", "large", true) + "tmp_" + FN + ".png";
                        Image3 = AppLogic.GetImagePath("Product", "large", true) + FN + ".png";
                        Image3File.SaveAs(TempImage3);
                        AppLogic.ResizeEntityOrObject("Product", TempImage3, Image3, "large", "image/png");
                        AppLogic.CreateOthersFromLarge("Product", TempImage3, FN, "image/png");

                        break;
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":

                        TempImage3 = AppLogic.GetImagePath("Product", "large", true) + "tmp_" + FN + ".jpg";
                        Image3 = AppLogic.GetImagePath("Product", "large", true) + FN + ".jpg";
                        Image3File.SaveAs(TempImage3);
                        AppLogic.ResizeEntityOrObject("Product", TempImage3, Image3, "large", "image/jpeg");
                        AppLogic.CreateOthersFromLarge("Product", TempImage3, FN, "image/jpeg");

                        break;
                }
                AppLogic.DisposeOfTempImage(TempImage3);
            }

            // color swatch image
            String Image4 = String.Empty;
            String TempImage4 = String.Empty;
            HttpPostedFile Image4File = fuSwatch.PostedFile;
            if (Image4File != null && Image4File.ContentLength != 0)
            {
                // delete any current image file first
                try
                {
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        System.IO.File.Delete(AppLogic.GetImagePath("Product", "swatch", true) + FN + ss);
                    }
                }
                catch
                { }

                switch (Image4File.ContentType)
                {
                    case "image/gif":

                        TempImage4 = AppLogic.GetImagePath("Product", "swatch", true) + "tmp_" + FN + ".gif";
                        Image4 = AppLogic.GetImagePath("Product", "swatch", true) + FN + ".gif";
                        Image4File.SaveAs(TempImage4);
                        AppLogic.ResizeEntityOrObject("Product", TempImage4, Image4, "swatch", "image/gif");
                        AppLogic.DisposeOfTempImage(TempImage4);

                        break;
                    case "image/x-png":
                    case "image/png":

                        TempImage4 = AppLogic.GetImagePath("Product", "swatch", true) + "tmp_" + FN + ".png";
                        Image4 = AppLogic.GetImagePath("Product", "swatch", true) + FN + ".png";
                        Image4File.SaveAs(TempImage4);
                        AppLogic.ResizeEntityOrObject("Product", TempImage4, Image4, "swatch", "image/png");
                        AppLogic.DisposeOfTempImage(TempImage4);

                        break;
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":

                        TempImage4 = AppLogic.GetImagePath("Product", "swatch", true) + "tmp_" + FN + ".jpg";
                        Image4 = AppLogic.GetImagePath("Product", "swatch", true) + FN + ".jpg";
                        Image4File.SaveAs(TempImage4);
                        AppLogic.ResizeEntityOrObject("Product", TempImage4, Image4, "swatch", "image/jpeg");
                        AppLogic.DisposeOfTempImage(TempImage4);

                        break;
                }
            }
        }

        protected void SetVariantButtons()
        {
            string temp = ("<a target=\"entityBody\" href=\"entityProductVariantsOverview.aspx?ProductID=" + pID + "&entityname=" + eSpecs.m_EntityName + "&EntityID=" + eID.ToString() + "\">Show/Edit/Add Variants</a>");

            ltVariantsLinks.Text = temp;
        }

        protected void btnDescription_Click(object sender, EventArgs e)
        {
            if (pdesc.FN != string.Empty && File.Exists(pdesc.FN))
                System.IO.File.Delete(pdesc.FN);
            LoadContent();
        }

        protected void btnSpecs_Click(object sender, EventArgs e)
        {
            ProductSpecFile pspec = new ProductSpecFile(pID, currentSkinID);

            if (pspec.FN != String.Empty)
            {
                System.IO.File.Delete(pspec.FN);
            }
            LoadContent();
        }

        protected void btnDeleteAll_Click(object sender, EventArgs e)
        {
            DB.ExecuteSQL("delete from KitCart where VariantID in (select VariantID from ProductVariant where ProductID=" + pID.ToString() + ")");
            DB.ExecuteSQL("delete from ShoppingCart where VariantID in (select VariantID from ProductVariant where ProductID=" + pID.ToString() + ")");
            DB.ExecuteSQL("delete from ProductVariant where ProductID=" + pID.ToString());

            LoadContent();
            resetError("All variants have been deleted", false);
        }

        protected void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            DB.ExecuteSQL("update dbo.Product set Deleted = case deleted when 1 then 0 else 1 end where ProductID = " + pID.ToString());
            LoadContent();
        }


        public void ValidateDateRange(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            if (txtStartDate.Text.Trim() != "" && txtStopDate.Text.Trim() != "")
            {
                DateTime sdate = Localization.ParseLocaleDateTime(txtStartDate.Text, Localization.GetDefaultLocale());
                DateTime edate = Localization.ParseLocaleDateTime(txtStopDate.Text, Localization.GetDefaultLocale());
                if (sdate.CompareTo(edate) > 0)
                {
                    args.IsValid = false;
                }
            }
        }

    }
}
