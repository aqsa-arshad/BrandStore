// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class entityEdit : AdminPageBase
    {
        #region Fields

        private Int32 eID;
        private String eName;
        private EntityHelper entity;
        private EntitySpecs eSpecs;
        protected Boolean bUseHtmlEditor;

        #endregion

        #region Event Handlers
                
        protected void Page_Load(Object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (AppLogic.MaxEntitiesExceeded())
            {
                ltError.Text = "<font class=\"errorMsg\">WARNING: Maximum number of allowed entities exceeded. To add additional entities, please delete some entities or upgrade to AspDotNetStoreFront ML </font>";
            }

            if (!IsPostBack)
            {
                ViewState.Add("EntityEditID", 0);
            }

            // Determine HTML editor configuration
            bUseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
            radSummary.Visible = bUseHtmlEditor;
            radDescription.Visible = bUseHtmlEditor;
            radDescription.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);
            radSummary.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);

            eID = CommonLogic.QueryStringNativeInt("EntityID");
            if (Localization.ParseNativeInt(ViewState["EntityEditID"].ToString()) > 0)
            {
                eID = Localization.ParseNativeInt(ViewState["EntityEditID"].ToString());
            }

            eName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            eSpecs = EntityDefinitions.LookupSpecs(eName);

            Tr2.Visible = AppLogic.AppConfigBool("TemplateSwitching.Enabled");

            switch (eName.ToUpperInvariant())
            {
                case "SECTION":
                    ltPreEntity.Text = "Sections";
                    entity = AppLogic.SectionStoreEntityHelper[0];
                    break;
                case "MANUFACTURER":
                    ltPreEntity.Text = "Manufacturers";
                    entity = AppLogic.ManufacturerStoreEntityHelper[0];
                    break;
                case "DISTRIBUTOR":
                    ltPreEntity.Text = "Distributors";
                    entity = AppLogic.DistributorStoreEntityHelper[0];
                    break;
                case "GENRE":
                    ltPreEntity.Text = "Genres";
                    entity = AppLogic.GenreStoreEntityHelper[0];
                    break;
                case "VECTOR":
                    ltPreEntity.Text = "Vectors";
                    entity = AppLogic.VectorStoreEntityHelper[0];
                    break;
                case "LIBRARY":
                    ltPreEntity.Text = "Libraries";
                    entity = AppLogic.LibraryStoreEntityHelper[0];
                    break;
                default:
                    ltPreEntity.Text = "Categories";
                    entity = AppLogic.CategoryStoreEntityHelper[0];
                    break;
            }
            etsMapper.ObjectID = eID;
            etsMapper.EntityType = eName;
            if (!IsPostBack)
            {
                etsMapper.DataBind();
                pnlStoreMappings.Visible = etsMapper.StoreCount > 1;
                if (!(entity == AppLogic.CategoryStoreEntityHelper[0] || entity == AppLogic.SectionStoreEntityHelper[0] || entity == AppLogic.ManufacturerStoreEntityHelper[0]))
                {
                    etsMapper.Visible = false;
                    pnlStoreMapNotSupported.Visible = true;
                }
                if (ThisCustomer.ThisCustomerSession.Session("entityUserLocale").Length == 0)
                {
                    ThisCustomer.ThisCustomerSession.SetVal("entityUserLocale", Localization.GetDefaultLocale());
                }

                ddLocale.Items.Clear(); 
                
                using (DataTable dtLocale = Localization.GetLocales())
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
                ddLocale.SelectedValue = Localization.GetDefaultLocale();
                LoadContent();
                if (eID > 0)
                {
                    ltIFrame.Text = "<iframe src=\"entityBulkDisplayOrder.aspx?EntityName=" + eName + "&EntityID=" +
                                    eID.ToString() +
                                    "\" name=\"bulkEntityFrame\" id=\"bulkEntityFrame\" frameborder=\"0\" height=\"250\" width=\"100%\" marginheight=\"0\" marginwidth=\"0\" SCROLLING=\"auto\"></iframe>";
                    pnlProducts.Visible = true;
                    pnlDisplayOrder.Visible = true;
                }
                else
                {
                    pnlProducts.Visible = false;
                    pnlDisplayOrder.Visible = false;
                }

                ctrlEntityProductMap.ThisCustomer = Customer.Current;
                ctrlEntityProductMap.EntityType = eName;
                ctrlEntityProductMap.EntityId = eID;
            }
        }

        protected void ddLocale_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (ddLocale.SelectedValue != String.Empty)
            {
                ThisCustomer.ThisCustomerSession.SetVal("entityUserLocale", ddLocale.SelectedValue);
            }
            else
            {
                ThisCustomer.ThisCustomerSession.SetVal("entityUserLocale", Localization.GetDefaultLocale());
            }
            LoadContent();
        }

        protected void btnSubmit_Click(Object sender, EventArgs e)
        {
            UpdateEntity();
            etsMapper.Save();
        }

        protected void btnClose_Click(Object sender, EventArgs e)
        {
            UpdateEntity();
            etsMapper.Save();

            ClientScript.RegisterStartupScript(Page.GetType(), Guid.NewGuid().ToString(), "CloseAndRebind(" + eID + ");", true);
        }

        protected void btnCancel_Click(Object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(Page.GetType(), Guid.NewGuid().ToString(), "CancelEdit(" + eID + ");", true);
        }

        protected void lnkProducts_Click(Object sender, EventArgs e)
        {
            trBulkFrame.Visible = false;
            trProductMap.Visible = true;
        }

        protected void lnkBulkDisplayOrder_Click(Object sender, EventArgs e)
        {
            trProductMap.Visible = false;
            trBulkFrame.Visible = true;
            bulkFrame.Attributes.Add("src", "entityProductBulkDisplayOrder.aspx?entityname=" + eSpecs.m_EntityName + "&EntityID=" + eID.ToString());
        }

        protected void lnkBulkInventory_Click(Object sender, EventArgs e)
        {
            trProductMap.Visible = false;
            trBulkFrame.Visible = true;
            bulkFrame.Attributes.Add("src", "entityBulkInventory.aspx?entityname=" + eSpecs.m_EntityName + "&EntityID=" + eID.ToString());
        }

        protected void lnkBulkSEFields_Click(Object sender, EventArgs e)
        {
            trProductMap.Visible = false;
            trBulkFrame.Visible = true;
            bulkFrame.Attributes.Add("src", "entityBulkSE.aspx?entityname=" + eSpecs.m_EntityName + "&EntityID=" + eID.ToString());
        }

        protected void lnkBulkPrices_Click(Object sender, EventArgs e)
        {
            trProductMap.Visible = false;
            trBulkFrame.Visible = true;
            bulkFrame.Attributes.Add("src", "entityBulkPrices.aspx?entityname=" + eSpecs.m_EntityName + "&EntityID=" + eID.ToString());
        }

        protected void lnkBulkShippingMethods_Click(Object sender, EventArgs e)
        {
            trProductMap.Visible = false;
            trBulkFrame.Visible = true;
            bulkFrame.Attributes.Add("src", "entityBulkShipping.aspx?entityname=" + eSpecs.m_EntityName + "&EntityID=" + eID.ToString());
        }

        protected void lnkBulkDownloadFiles_Click(Object sender, EventArgs e)
        {
            trProductMap.Visible = false;
            trBulkFrame.Visible = true;
            bulkFrame.Attributes.Add("src", "entityBulkDownloadFiles.aspx?entityname=" + eSpecs.m_EntityName + "&EntityID=" + eID.ToString());
        }

        #endregion

        #region Overrides

        protected override void OnInit(EventArgs e)
        {
           

            base.OnInit(e);
        }

        #endregion

        #region Protected Methods

        private void ResetError(String errorx, Boolean isError)
        {
            String str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
            {
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";
            }

            if (errorx.Length > 0)
            {
                str += errorx + "";
            }
            else
            {
                str = "";
            }

            ((Literal)Form.FindControl("ltError")).Text = str;
        }

        private void LoadScript(Boolean load)
        {
            if (load) { }
            else
            {
                ltScript.Text = "";
            }
        }

        private void LoadContent()
        {
            ddCountry.Items.Clear();
            ddDiscountTable.Items.Clear();
            ddParent.Items.Clear();
            ddState.Items.Clear();
            ddXmlPackage.Items.Clear();

            //pull locale from user session
            String entityLocale = ThisCustomer.ThisCustomerSession.Session("entityUserLocale");
            if (entityLocale.Length > 0)
            {
                try
                {
                    ddLocale.SelectedValue = entityLocale;
                    // user's locale may not exist any more, so don't let the assignment crash
                }
                catch { }
            }

            Int32 SkinID = 1;
            String locale = Localization.CheckLocaleSettingForProperCase(ddLocale.SelectedValue);

            if (ddLocale.SelectedValue == String.Empty)
            {
                locale = Localization.GetDefaultLocale();
            }

            Boolean Editing = false;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from " + eSpecs.m_EntityName + "  with (NOLOCK)  where " + eSpecs.m_EntityName + "ID=" + eID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    ViewState.Add("EntityEdit", Editing);

                    rfvName.ErrorMessage = "Please enter the " +
                                           AppLogic.GetString("AppConfig." + eSpecs.m_EntityName + "PromptSingular", SkinID,
                                                               ThisCustomer.LocaleSetting).ToLowerInvariant() + " name (Main Tab)";

                    //load parents
                    if (eSpecs.m_HasParentChildRelationship)
                    {
                        trParent.Visible = true;
                        ddParent.Items.Add(new ListItem("--ROOT LEVEL--", "0"));

                        ArrayList al = entity.GetEntityArrayList(0, "", 0, ThisCustomer.LocaleSetting, false);
                        for (Int32 i = 0; i < al.Count; i++)
                        {
                            ListItemClass lic = (ListItemClass)al[i];
                            String value = lic.Value.ToString();
                            String name = HttpUtility.HtmlDecode(lic.Item);

                            ddParent.Items.Add(new ListItem(name, value));
                        }
                    }
                    else
                    {
                        trParent.Visible = false;
                    }
                    //load XmlPackages
                    ArrayList xmlPackages = AppLogic.ReadXmlPackages("entity", SkinID);
                    foreach (String s in xmlPackages)
                    {
                        ddXmlPackage.Items.Add(new ListItem(s, s));
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
                                ddDiscountTable.Items.Add(
                                        new ListItem(DB.RSFieldByLocale(rsst, "Name", ThisCustomer.LocaleSetting),
                                                      DB.RSFieldInt(rsst, "QuantityDiscountID").ToString()));
                            }
                        }

                    }

                    if (eSpecs.m_EntityName == "Category" ||
                         eSpecs.m_EntityName == "Section")
                    {
                        trBrowser.Visible = true;
                    }
                    else
                    {
                        trBrowser.Visible = false;
                    }

                    //address
                    phAddress.Visible = false;
                    if (eSpecs.m_HasAddress)
                    {
                        rfvEmail.Visible = false;
                        if (eName.ToUpperInvariant().Equals("DISTRIBUTOR"))
                        {
                            rfvEmail.Visible = true;
                        }

                        phAddress.Visible = true;

                        ddState.Items.Add(new ListItem("SELECT ONE", "0"));

                        using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                        {
                            conn.Open();
                            using (IDataReader reader =
                                DB.GetRS("select * from state   with (NOLOCK)  order by DisplayOrder,Name", conn))
                            {
                                while (reader.Read())
                                {
                                    ddState.Items.Add(
                                        new ListItem(Security.HtmlEncode(DB.RSField(reader, "Name")),
                                                      Security.HtmlEncode(DB.RSField(reader, "Abbreviation"))));
                                }
                            }

                        }
                        using (SqlConnection connect = new SqlConnection(DB.GetDBConn()))
                        {
                            connect.Open();
                            using (IDataReader rss = DB.GetRS("select * from country   with (NOLOCK)  order by DisplayOrder,Name", connect))
                            {
                                while (rss.Read())
                                {
                                    ddCountry.Items.Add(new ListItem(DB.RSField(rss, "Name"), DB.RSField(rss, "Name")));

                                }
                            }

                        }

                    }

                    if (Editing)
                    {
                        pnlProducts.Visible = this.eID > 0;

                        //SET Product Buttons
                        SetProductButtons();

                        ltStatus.Text = "Editing " + eName;
                        btnSubmit.Text = "Update";
                        ltEntity.Text = entity.GetEntityBreadcrumb6(eID, ThisCustomer.LocaleSetting);

                        txtName.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Name"), locale, false);

                        if (!DB.RSFieldBool(rs, "Published"))
                        {
                            rblPublished.BackColor = Color.LightYellow;
                        }
                        rblPublished.SelectedIndex = (DB.RSFieldBool(rs, "Published") ? 1 : 0);

                        if (eSpecs.m_EntityName == "Category" ||
                             eSpecs.m_EntityName == "Section")
                        {
                            rblBrowser.SelectedIndex = (DB.RSFieldBool(rs, "ShowIn" + eSpecs.m_ObjectName + "Browser") ? 1 : 0);
                        }

                        //match parent
                        ddParent.ClearSelection();
                        ddParent.SelectedValue = DB.RSFieldInt(rs, "Parent" + eSpecs.m_EntityName + "ID").ToString();

                        //match XmlPackage
                        ddXmlPackage.ClearSelection();
                        foreach (ListItem li in ddXmlPackage.Items)
                        {
                            if (li.Value.Equals(DB.RSField(rs, "XmlPackage").ToLowerInvariant()))
                            {
                                ddXmlPackage.SelectedValue = DB.RSField(rs, "XmlPackage").ToLowerInvariant();
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

                        txtPageSize.Text =
                                CommonLogic.IIF(Editing, DB.RSFieldInt(rs, "PageSize").ToString(),
                                                 AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "PageSize"));
                        txtColumn.Text =
                                CommonLogic.IIF(Editing, DB.RSFieldInt(rs, "ColWidth").ToString(),
                                                 AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "ColWidth"));

                        rblLooks.SelectedIndex = (DB.RSFieldBool(rs, "SortByLooks") ? 1 : 0);

                        if (eSpecs.m_HasAddress)
                        {
                            txtAddress1.Text =
                                    CommonLogic.IIF(Editing, Security.HtmlEncode(DB.RSField(rs, "Address1")), "");
                            txtApt.Text = CommonLogic.IIF(Editing, Security.HtmlEncode(DB.RSField(rs, "Suite")), "");
                            txtAddress2.Text =
                                    CommonLogic.IIF(Editing, Security.HtmlEncode(DB.RSField(rs, "Address2")), "");
                            txtCity.Text = CommonLogic.IIF(Editing, Security.HtmlEncode(DB.RSField(rs, "City")), "");

                            //match State
                            try
                            {
                                ddState.ClearSelection();
                                ddState.SelectedValue = DB.RSField(rs, "State").ToString();
                            }
                            catch { }

                            txtZip.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ZipCode"), "");

                            //match country
                            try
                            {
                                ddCountry.ClearSelection();
                                ddCountry.SelectedValue = DB.RSField(rs, "Country").ToString();
                            }
                            catch { }

                            txtURL.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "URL"), "");
                            txtEmail.Text =
                                    CommonLogic.IIF(Editing, DB.RSField(rs, "EMail"),
                                                     CommonLogic.QueryStringCanBeDangerousContent("EMail"));
                            txtPhone.Text =
                                    CommonLogic.IIF(Editing, CommonLogic.GetPhoneDisplayFormat(DB.RSField(rs, "Phone")), "");
                            txtFax.Text =
                                    CommonLogic.IIF(Editing, CommonLogic.GetPhoneDisplayFormat(DB.RSField(rs, "Fax")), "");
                        }

                        // BEGIN IMAGES 
                        txtImageOverride.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ImageFilenameOverride"), "");
                        Boolean disableupload = (Editing && DB.RSField(rs, "ImageFilenameOverride") != "");
                        if (eSpecs.m_HasIconPic)
                        {
                            fuIcon.Enabled = !disableupload;
                            String Image1URL =
                                    AppLogic.LookupImage(eSpecs.m_EntityName, eID, "icon", SkinID, ThisCustomer.LocaleSetting);
                            if (Image1URL.Length == 0)
                            {
                                Image1URL = AppLogic.NoPictureImageURL(false, SkinID, ThisCustomer.LocaleSetting);
                            }
                            if (!CommonLogic.FileExists(Image1URL))
                            {
                                Image1URL = AppLogic.LocateImageURL("Skins/skin_1/images/nopictureicon.gif", ThisCustomer.LocaleSetting);
                            }
                            if (Image1URL.Length != 0)
                            {
                                ltIcon.Text = "";
                                if (Image1URL.IndexOf("nopicture") ==
                                     -1)
                                {
                                    ltIcon.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image1URL +
                                                    "','Pic1');\">Click here</a> to delete the current image <br/>\n");
                                }
                                ltIcon.Text +=
                                        "<img style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic1\" name=\"Pic1\" border=\"0\" src=\"" +
                                        Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                            }
                        }
                        if (eSpecs.m_HasMediumPic)
                        {
                            fuMedium.Enabled = !disableupload;
                            String Image2URL =
                                    AppLogic.LookupImage(eSpecs.m_EntityName, eID, "medium", SkinID, ThisCustomer.LocaleSetting);
                            if (Image2URL.Length == 0)
                            {
                                Image2URL = AppLogic.NoPictureImageURL(false, SkinID, ThisCustomer.LocaleSetting);
                            }
                            if (!CommonLogic.FileExists(Image2URL))
                            {
                                Image2URL = AppLogic.LocateImageURL("Skins/skin_1/images/nopicture.gif", ThisCustomer.LocaleSetting);
                            }
                            if (Image2URL.Length != 0)
                            {
                                ltMedium.Text = "";
                                if (Image2URL.IndexOf("nopicture") ==
                                     -1)
                                {
                                    ltMedium.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image2URL +
                                                      "','Pic2');\">Click here</a> to delete the current image <br/>\n");
                                }
                                ltMedium.Text +=
                                        "<img style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic2\" name=\"Pic2\" border=\"0\" src=\"" +
                                        Image2URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                            }
                        }
                        if (eSpecs.m_HasLargePic)
                        {
                            fuLarge.Enabled = !disableupload;
                            String Image3URL =
                                    AppLogic.LookupImage(eSpecs.m_EntityName, eID, "large", SkinID, ThisCustomer.LocaleSetting);
                            if (Image3URL.Length == 0)
                            {
                                Image3URL = AppLogic.NoPictureImageURL(false, SkinID, ThisCustomer.LocaleSetting);
                            }
                            if (!CommonLogic.FileExists(Image3URL))
                            {
                                Image3URL = AppLogic.LocateImageURL("Skins/skin_1/images/nopicture.gif", ThisCustomer.LocaleSetting);
                            }
                            if (Image3URL.Length != 0)
                            {
                                ltLarge.Text = "";
                                if (Image3URL.IndexOf("nopicture") ==
                                     -1)
                                {
                                    ltLarge.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image3URL +
                                                     "','Pic3');\">Click here</a> to delete the current image <br/>\n");
                                }
                                ltLarge.Text +=
                                        "<img style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic3\" name=\"Pic3\" border=\"0\" src=\"" +
                                        Image3URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                            }
                        }
                        // END IMAGES

                        //DESCRIPTION
                        if (bUseHtmlEditor)
                        {
                            radDescription.Content = DB.RSFieldByLocale(rs, "Description", locale);
                        }
                        else
                        {
                            ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                            ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") +
                                                    "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") +
                                                    "\" id=\"Description\" name=\"Description\">" +
                                                    XmlCommon.GetLocaleEntry(Security.HtmlEncode(DB.RSField(rs, "Description")),
                                                                              locale, false) + "</textarea>\n");
                            ltDescription.Text += ("</div>");
                        }

                        //SUMMARY
                        if (bUseHtmlEditor)
                        {
                            radSummary.Content = DB.RSFieldByLocale(rs, "Summary", locale);
                        }
                        else
                        {
                            ltSummary.Text = ("<div id=\"idSummary\" style=\"height: 1%;\">");
                            ltSummary.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" +
                                                AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Summary\" name=\"Summary\">" +
                                                XmlCommon.GetLocaleEntry(Security.HtmlEncode(DB.RSField(rs, "Summary")),
                                                                          locale, false) + "</textarea>\n");
                            ltSummary.Text += ("</div>");
                        }
                        //EXTENSION DATA
                        txtExtensionData.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData"), "");
                        txtExtensionData.Columns = Localization.ParseNativeInt(AppLogic.AppConfig("Admin_TextareaWidth"));
                        txtExtensionData.Rows = 10;

                        txtUseSkinTemplateFile.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "TemplateName"), "");
                        txtUseSkinID.Text = CommonLogic.IIF(Editing, DB.RSFieldInt(rs, "SkinID"), 0).ToString();
                        if (txtUseSkinID.Text == "0")
                        {
                            txtUseSkinID.Text = String.Empty;
                        }
                        //SEARCH ENGINE
                        txtSETitle.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SETitle"), locale, false);
                        txtSEKeywords.Text =
                                XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEKeywords"), locale, false);
                        txtSEDescription.Text =
                                XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEDescription"), locale, false);
                        txtSENoScript.Text =
                                XmlCommon.GetLocaleEntry(DB.RSField(rs, "SENoScript"), locale, false);
                        txtSEAlt.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SEAltText"), locale, false);

                    }
                    else
                    {
                        if (AppLogic.MaxEntitiesExceeded())
                        {
                            btnSubmit.Enabled = false;
                            btnSubmit.CssClass = "normalButtonsDisabled";
                        }
                        else
                        {
                            btnSubmit.Enabled = true;
                            btnSubmit.CssClass = "normalButtons";
                        }

                        ltStatus.Text = "Adding " + eName;
                        btnSubmit.Text = "Add New";

                        txtPageSize.Text = AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "PageSize");
                        txtColumn.Text = AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "ColWidth");

                        Int32 parentID = CommonLogic.QueryStringNativeInt("entityparent");
                        if (parentID > 0)
                        {
                            ddParent.SelectedValue = parentID.ToString();
                            ltEntity.Text = entity.GetEntityBreadcrumb6(parentID, ThisCustomer.LocaleSetting);
                        }
                        else
                        {
                            ltEntity.Text = "Root Level";
                        }

                        if (!bUseHtmlEditor)
                        {
                            //DESCRIPTION
                            ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                            ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") +
                                                    "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") +
                                                    "\" id=\"Description\" name=\"Description\"></textarea>\n");
                            ltDescription.Text += ("</div>");

                            //SUMMARY
                            ltSummary.Text = ("<div id=\"idSummary\" style=\"height: 1%;\">");
                            ltSummary.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" +
                                                AppLogic.AppConfigUSInt("Admin_TextareaWidth") +
                                                "\" id=\"Summary\" name=\"Summary\"></textarea>\n");
                            ltSummary.Text += ("</div>");
                        }

                        //EXTENSION DATA
                        txtExtensionData.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData"), "");
                        txtExtensionData.Columns = Localization.ParseNativeInt(AppLogic.AppConfig("Admin_TextareaWidth"));
                        txtExtensionData.Rows = 10;
                    }
                }
            }

            ltScript2.Text = ("<script type=\"text/javascript\">\n");
            ltScript2.Text += ("function DeleteImage(imgurl,name)\n");
            ltScript2.Text += ("{\nif(confirm(\'Are you sure you want to delete this image?\'))\n");
            ltScript2.Text += ("{\n");
            ltScript2.Text +=
                    ("window.open('deleteimage.aspx?imgurl=' + imgurl + '&FormImageName=' + name,\"Admin_ML\",\"height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
            ltScript2.Text += ("}}\n");
            ltScript2.Text += ("</SCRIPT>\n");

        }

        private void UpdateEntity()
        {
            Boolean Editing = Localization.ParseBoolean(ViewState["EntityEdit"].ToString());

            StringBuilder sql = new StringBuilder(2500);
            Int32 ParID = Localization.ParseNativeInt(ddParent.SelectedValue);
            if (ParID == eID) // prevent case which causes endless recursion
            {
                ParID = 0;
            }

            String locale = ddLocale.SelectedValue;
            if (locale.Equals(String.Empty))
            {
                locale = Localization.GetDefaultLocale();
            }


            try
            {
                if (!Editing)
                {
                    // ok to add them:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into " + eSpecs.m_EntityName + "(" + eSpecs.m_EntityName + "GUID,Name,SEName," +
                                CommonLogic.IIF(eSpecs.m_HasAddress,
                                                 "Address1,Address2,Suite,City,State,ZipCode,Country,Phone,FAX,URL,EMail,",
                                                 "") + "TemplateName,SkinID,ImageFilenameOverride," +
                                CommonLogic.IIF(eSpecs.m_HasParentChildRelationship,
                                                 "Parent" + eSpecs.m_EntityName + "ID,", "") +
                                "Summary,Description,ExtensionData,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,Published," +
                                CommonLogic.IIF(eSpecs.m_EntityName == "Category" || eSpecs.m_EntityName == "Section",
                                                 "ShowIn" + eSpecs.m_ObjectName + "Browser,", "") +
                                "PageSize,ColWidth,XmlPackage,SortByLooks,QuantityDiscountID) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(
                            DB.SQuote(
                                    AppLogic.FormLocaleXml("Name", txtName.Text, locale, eSpecs, eID)) +
                            ",");
                    sql.Append(
                            DB.SQuote(
                                    SE.MungeName(
                                            AppLogic.GetFormsDefaultLocale("Name", txtName.Text, locale,
                                                                            eSpecs, eID))) + ",");
                    if (eSpecs.m_HasAddress)
                    {
                        if (txtAddress1.Text.Length != 0)
                        {
                            sql.Append(DB.SQuote(txtAddress1.Text) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (txtAddress2.Text.Length != 0)
                        {
                            sql.Append(DB.SQuote(txtAddress2.Text) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (txtApt.Text.Length != 0)
                        {
                            sql.Append(DB.SQuote(txtApt.Text) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (txtCity.Text.Length != 0)
                        {
                            sql.Append(DB.SQuote(txtCity.Text) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (!ddState.SelectedValue.Equals("0"))
                        {
                            sql.Append(DB.SQuote(ddState.SelectedValue) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (txtZip.Text.Length != 0)
                        {
                            sql.Append(DB.SQuote(txtZip.Text) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (!ddCountry.SelectedValue.Equals("0"))
                        {
                            sql.Append(DB.SQuote(ddCountry.SelectedValue) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (txtPhone.Text.Length != 0)
                        {
                            sql.Append(DB.SQuote(AppLogic.MakeProperPhoneFormat(txtPhone.Text)) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (txtFax.Text.Length != 0)
                        {
                            sql.Append(DB.SQuote(AppLogic.MakeProperPhoneFormat(txtFax.Text)) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (txtURL.Text.Length != 0)
                        {
                            String theUrl = CommonLogic.Left(txtURL.Text, 80);
                            if (theUrl.IndexOf("http://") == -1 &&
                                 theUrl.Length != 0)
                            {
                                theUrl = "http://" + theUrl;
                            }
                            if (theUrl.Length == 0)
                            {
                                sql.Append("NULL,");
                            }
                            else
                            {
                                sql.Append(DB.SQuote(theUrl) + ",");
                            }
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (txtEmail.Text.Length != 0)
                        {
                            sql.Append(DB.SQuote(CommonLogic.Left(txtEmail.Text, 100)) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                    }
                    sql.Append(DB.SQuote(txtUseSkinTemplateFile.Text) + ",");
                    Int32 x = 0;
                    if (txtUseSkinID.Text.Length != 0)
                    {
                        x = Localization.ParseUSInt(txtUseSkinID.Text);
                    }
                    sql.Append(x.ToString() + ",");
                    sql.Append(DB.SQuote(txtImageOverride.Text) + ",");
                    if (eSpecs.m_HasParentChildRelationship)
                    {
                        sql.Append(ParID.ToString() + ",");
                    }
                    if (bUseHtmlEditor)
                    {
                        sql.Append(
                                DB.SQuote(AppLogic.FormLocaleXml("Summary", radSummary.Content, locale, eSpecs, eID)) + ",");
                        sql.Append(
                                DB.SQuote(AppLogic.FormLocaleXml("Description", radDescription.Content, locale, eSpecs, eID)) + ",");
                    }
                    else
                    {
                        sql.Append(DB.SQuote(AppLogic.FormLocaleXmlEditor("Summary", "Summary", locale, eSpecs, eID)) + ",");
                        sql.Append(DB.SQuote(AppLogic.FormLocaleXmlEditor("Description", "Description", locale, eSpecs, eID)) + ",");
                    }
                    sql.Append(DB.SQuote(txtExtensionData.Text) + ",");

                    if (txtSEKeywords.Text.Length != 0)
                    {
                        sql.Append(
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SEKeywords", txtSEKeywords.Text, locale,
                                                                eSpecs, eID)) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (txtSEDescription.Text.Length != 0)
                    {
                        sql.Append(
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SEDescription", txtSEDescription.Text,
                                                                locale, eSpecs, eID)) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (txtSETitle.Text.Length != 0)
                    {
                        sql.Append(
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SETitle", txtSETitle.Text, locale,
                                                                eSpecs, eID)) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (txtSENoScript.Text.Length != 0)
                    {
                        sql.Append(
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SENoScript", txtSENoScript.Text, locale,
                                                                eSpecs, eID)) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (txtSEAlt.Text.Length != 0)
                    {
                        sql.Append(
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SEAltText", txtSEAlt.Text, locale,
                                                                eSpecs, eID)) + ",");
                    }
                    else
                    {
                        sql.Append(DB.SQuote(
                                        AppLogic.FormLocaleXml("SEAltText", txtName.Text, locale,
                                                                eSpecs, eID)) + ",");
                    }

                    sql.Append(Localization.ParseNativeInt(rblPublished.SelectedValue) + ",");
                    if (eSpecs.m_EntityName == "Category" ||
                         eSpecs.m_EntityName == "Section")
                    {
                        sql.Append(Localization.ParseNativeInt(rblBrowser.SelectedValue) + ",");
                    }
                    sql.Append(
                            CommonLogic.IIF(txtPageSize.Text.Length == 0,
                                             AppLogic.AppConfigUSInt("Default_" + eSpecs.m_EntityName + "PageSize").
                                                     ToString(), txtPageSize.Text) + ",");
                    sql.Append(
                            CommonLogic.IIF(txtColumn.Text.Length == 0,
                                             AppLogic.AppConfigUSInt("Default_" + eSpecs.m_EntityName + "ColWidth").
                                                     ToString(), txtColumn.Text) + ",");
                    if (ddXmlPackage.SelectedValue != "0")
                    {
                        sql.Append(DB.SQuote(ddXmlPackage.SelectedValue.ToLowerInvariant()) + ",");
                    }
                    else
                    {
                        sql.Append(DB.SQuote(AppLogic.ro_DefaultEntityXmlPackage) + ","); // force a default!
                    }
                    sql.Append(rblLooks.SelectedValue + ",");
                    sql.Append(ddDiscountTable.SelectedValue);
                    sql.Append(")");

                    DB.ExecuteSQL(sql.ToString());

                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using (IDataReader rs =
                            DB.GetRS("select " + eSpecs.m_EntityName + "ID from " + eSpecs.m_EntityName + " with (NOLOCK) where Deleted=0 and " + eSpecs.m_EntityName + "GUID=" +
                                      DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            eID = DB.RSFieldInt(rs, eSpecs.m_EntityName + "ID");
                            etsMapper.ObjectID = eID;
                            ctrlEntityProductMap.EntityId = eID;
                            ViewState.Add("EntityEdit", true);
                            ViewState.Add("EntityEditID", eID);
                            ResetError("Entity Added", false);
                        }
                    }

                }
                else
                {
                    // ok to update:
                    sql.Append("update " + eSpecs.m_EntityName + " set ");
                    sql.Append("Name=" +
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("Name", txtName.Text, locale, eSpecs,
                                                                eID)) + ",");
                    sql.Append("SEName=" +
                                DB.SQuote(
                                        SE.MungeName(
                                                AppLogic.GetFormsDefaultLocale("Name", txtName.Text,
                                                                                locale, eSpecs, eID))) +
                                ",");

                    if (eSpecs.m_HasAddress)
                    {
                        if (txtAddress1.Text.Length != 0)
                        {
                            sql.Append("Address1=" + DB.SQuote(txtAddress1.Text) + ",");
                        }
                        else
                        {
                            sql.Append("Address1=NULL,");
                        }
                        if (txtAddress2.Text.Length != 0)
                        {
                            sql.Append("Address2=" + DB.SQuote(txtAddress2.Text) + ",");
                        }
                        else
                        {
                            sql.Append("Address2=NULL,");
                        }
                        if (txtApt.Text.Length != 0)
                        {
                            sql.Append("Suite=" + DB.SQuote(txtApt.Text) + ",");
                        }
                        else
                        {
                            sql.Append("Suite=NULL,");
                        }
                        if (txtCity.Text.Length != 0)
                        {
                            sql.Append("City=" + DB.SQuote(txtCity.Text) + ",");
                        }
                        else
                        {
                            sql.Append("City=NULL,");
                        }
                        if (!ddState.SelectedValue.Equals("0"))
                        {
                            sql.Append("State=" + DB.SQuote(ddState.SelectedValue) + ",");
                        }
                        else
                        {
                            sql.Append("State=NULL,");
                        }
                        if (txtZip.Text.Length != 0)
                        {
                            sql.Append("ZipCode=" + DB.SQuote(txtZip.Text) + ",");
                        }
                        else
                        {
                            sql.Append("ZipCode=NULL,");
                        }
                        if (!ddCountry.SelectedValue.Equals("0"))
                        {
                            sql.Append("Country=" + DB.SQuote(ddCountry.SelectedValue) + ",");
                        }
                        else
                        {
                            sql.Append("Country=NULL,");
                        }
                        if (txtPhone.Text.Length != 0)
                        {
                            sql.Append("Phone=" + DB.SQuote(AppLogic.MakeProperPhoneFormat(txtPhone.Text)) + ",");
                        }
                        else
                        {
                            sql.Append("Phone=NULL,");
                        }
                        if (txtFax.Text.Length != 0)
                        {
                            sql.Append("FAX=" + DB.SQuote(AppLogic.MakeProperPhoneFormat(txtFax.Text)) + ",");
                        }
                        else
                        {
                            sql.Append("FAX=NULL,");
                        }
                        if (txtURL.Text.Length != 0)
                        {
                            String theUrl2 = CommonLogic.Left(txtURL.Text, 80);
                            if (theUrl2.IndexOf("http://") == -1 &&
                                 theUrl2.Length != 0)
                            {
                                theUrl2 = "http://" + theUrl2;
                            }
                            if (theUrl2.Length != 0)
                            {
                                sql.Append("URL=" + DB.SQuote(theUrl2) + ",");
                            }
                            else
                            {
                                sql.Append("URL=NULL,");
                            }
                        }
                        else
                        {
                            sql.Append("URL=NULL,");
                        }
                        if (txtEmail.Text.Length != 0)
                        {
                            sql.Append("EMail=" + DB.SQuote(CommonLogic.Left(txtEmail.Text, 100)) + ",");
                        }
                        else
                        {
                            sql.Append("EMail=NULL,");
                        }
                    }

                    sql.Append("TemplateName=" + DB.SQuote(txtUseSkinTemplateFile.Text) + ",");
                    Int32 x = 0;
                    if (txtUseSkinID.Text.Length != 0)
                    {
                        x = Localization.ParseUSInt(txtUseSkinID.Text);
                    }
                    sql.Append("SkinID=" + x.ToString() + ",");
                    sql.Append("ImageFilenameOverride=" + DB.SQuote(txtImageOverride.Text) + ",");
                    if (eSpecs.m_HasParentChildRelationship)
                    {
                        sql.Append("Parent" + eSpecs.m_EntityName + "ID=" + ParID.ToString() + ",");
                    }
                    if (bUseHtmlEditor)
                    {
                        sql.Append("Summary=" +
                                    DB.SQuote(AppLogic.FormLocaleXml("Summary", radSummary.Content, locale, eSpecs, eID)) + ",");
                        sql.Append("Description=" +
                                    DB.SQuote(AppLogic.FormLocaleXml("Description", radDescription.Content, locale, eSpecs, eID)) + ",");
                    }
                    else
                    {
                        sql.Append("Summary=" + DB.SQuote(AppLogic.FormLocaleXmlEditor("Summary", "Summary", locale, eSpecs, eID)) + ",");
                        sql.Append("Description=" + DB.SQuote(AppLogic.FormLocaleXmlEditor("Description", "Description", locale, eSpecs, eID)) + ",");
                    }
                    sql.Append("ExtensionData=" + DB.SQuote(txtExtensionData.Text) + ",");

                    sql.Append("SEKeywords=" +
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SEKeywords", txtSEKeywords.Text, locale,
                                                                eSpecs, eID)) + ",");
                    sql.Append("SEDescription=" +
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SEDescription", txtSEDescription.Text,
                                                                locale, eSpecs, eID)) + ",");
                    sql.Append("SETitle=" +
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SETitle", txtSETitle.Text, locale,
                                                                eSpecs, eID)) + ",");
                    sql.Append("SENoScript=" +
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SENoScript", txtSENoScript.Text, locale,
                                                                eSpecs, eID)) + ",");

                    sql.Append("SEAltText=" +
                                DB.SQuote(
                                        AppLogic.FormLocaleXml("SEAltText", txtSEAlt.Text, locale,
                                                                eSpecs, eID)) + ",");

                    sql.Append("Published=" + rblPublished.SelectedValue + ",");
                    if (eSpecs.m_EntityName == "Category" ||
                         eSpecs.m_EntityName == "Section")
                    {
                        sql.Append("ShowIn" + eSpecs.m_ObjectName + "Browser=" +
                                    Localization.ParseNativeInt(rblBrowser.SelectedValue) + ",");
                    }
                    sql.Append("PageSize=" +
                                CommonLogic.IIF(txtPageSize.Text.Length == 0,
                                                 AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "PageSize"),
                                                 txtPageSize.Text) + ",");
                    sql.Append("ColWidth=" +
                                CommonLogic.IIF(txtColumn.Text.Length == 0,
                                                 AppLogic.AppConfig("Default_" + eSpecs.m_EntityName + "ColWidth"),
                                                 txtColumn.Text) + ",");
                    if (ddXmlPackage.SelectedValue != "0")
                    {
                        sql.Append("XmlPackage=" + DB.SQuote(ddXmlPackage.SelectedValue.ToLowerInvariant()) + ",");
                    }
                    else
                    {
                        sql.Append("XmlPackage=" + AppLogic.ro_DefaultEntityXmlPackage + ",");
                    }
                    sql.Append("SortByLooks=" + rblLooks.SelectedValue + ",");
                    sql.Append("QuantityDiscountID=" + ddDiscountTable.SelectedValue);
                    sql.Append(" where " + eSpecs.m_EntityName + "ID=" + eID.ToString());

                    DB.ExecuteSQL(sql.ToString());
                    ViewState.Add("EntityEdit", true);

                    ctrlEntityProductMap.UpdateChanges();
                }


                //refresh the static entityhelper
                switch (eSpecs.m_EntityName.ToUpperInvariant())
                {
                    case "CATEGORY":
                        AppLogic.CategoryStoreEntityHelper[0] =
                                new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), true, 0);
                        break;
                    case "SECTION":
                        AppLogic.SectionStoreEntityHelper[0] =
                                new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), true, 0);
                        break;
                    case "MANUFACTURER":
                        AppLogic.ManufacturerStoreEntityHelper[0] =
                                new EntityHelper(0, EntityDefinitions.LookupSpecs("Manufacturer"), true, 0);
                        break;
                    case "DISTRIBUTOR":
                        AppLogic.DistributorStoreEntityHelper[0] =
                                new EntityHelper(0, EntityDefinitions.LookupSpecs("Distributor"), true, 0);
                        break;
                    case "GENRE":
                        AppLogic.GenreStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), true, 0);
                        break;
                    case "VECTOR":
                        AppLogic.VectorStoreEntityHelper[0] =
                                new EntityHelper(0, EntityDefinitions.LookupSpecs("VECTOR"), true, 0);
                        break;
                    case "LIBRARY":
                        AppLogic.LibraryStoreEntityHelper[0] =
                                new EntityHelper(0, EntityDefinitions.LookupSpecs("Library"), true, 0);
                        break;
                }

                HandleImageSubmits();
                LoadContent();
            }
            catch (Exception ex)
            {
                ResetError("Error updating " + txtName.Text + ": " + ex.ToString(), true);
            }
        }

        private void HandleImageSubmits()
        {
            // handle image uploaded:
            String FN = txtImageOverride.Text.Trim();
            if (FN.Length == 0)
            {
                FN = eID.ToString();
            }
            String ErrorMsg = String.Empty;

            if (eSpecs.m_HasIconPic)
            {
                String Image1 = String.Empty;
                String TempImage1 = String.Empty;
                HttpPostedFile Image1File = fuIcon.PostedFile;
                if (Image1File != null &&
                     Image1File.ContentLength != 0)
                {
                    // delete any current image file first
                    try
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                                 FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) ||
                                 FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                            {
                                File.Delete(AppLogic.GetImagePath(eSpecs.m_ObjectName, "icon", true) + FN);
                            }
                            else
                            {
                                File.Delete(AppLogic.GetImagePath(eSpecs.m_EntityName, "icon", true) + FN + ss);
                            }
                        }
                    }
                    catch { }

                    String s = Image1File.ContentType;
                    switch (Image1File.ContentType)
                    {
                        case "image/gif":

                            TempImage1 = AppLogic.GetImagePath(eSpecs.m_EntityName, "icon", true) + "tmp_" + FN + ".gif";
                            Image1 = AppLogic.GetImagePath(eSpecs.m_EntityName, "icon", true) + FN + ".gif";
                            Image1File.SaveAs(TempImage1);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage1, Image1, "icon", "image/gif");

                            break;
                        case "image/x-png":
                        case "image/png":

                            TempImage1 = AppLogic.GetImagePath(eSpecs.m_EntityName, "icon", true) + "tmp_" + FN + ".png";
                            Image1 = AppLogic.GetImagePath(eSpecs.m_EntityName, "icon", true) + FN + ".png";
                            Image1File.SaveAs(TempImage1);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage1, Image1, "icon", "image/png");

                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/pjpeg":

                            TempImage1 = AppLogic.GetImagePath(eSpecs.m_EntityName, "icon", true) + "tmp_" + FN + ".jpg";
                            Image1 = AppLogic.GetImagePath(eSpecs.m_EntityName, "icon", true) + FN + ".jpg";
                            Image1File.SaveAs(TempImage1);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage1, Image1, "icon", "image/jpeg");

                            break;
                    }
                    AppLogic.DisposeOfTempImage(TempImage1);
                }
            }

            if (eSpecs.m_HasMediumPic)
            {
                String Image2 = String.Empty;
                String TempImage2 = String.Empty;
                HttpPostedFile Image2File = fuMedium.PostedFile;
                if (Image2File != null &&
                     Image2File.ContentLength != 0)
                {
                    // delete any current image file first
                    try
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            File.Delete(AppLogic.GetImagePath(eSpecs.m_EntityName, "medium", true) + FN + ss);
                        }
                    }
                    catch { }

                    String s = Image2File.ContentType;
                    switch (Image2File.ContentType)
                    {
                        case "image/gif":

                            TempImage2 = AppLogic.GetImagePath(eSpecs.m_EntityName, "medium", true) + "tmp_" + FN +
                                         ".gif";
                            Image2 = AppLogic.GetImagePath(eSpecs.m_EntityName, "medium", true) + FN + ".gif";
                            Image2File.SaveAs(TempImage2);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage2, Image2, "medium",
                                                           "image/gif");

                            break;
                        case "image/x-png":
                        case "image/png":

                            TempImage2 = AppLogic.GetImagePath(eSpecs.m_EntityName, "medium", true) + "tmp_" + FN +
                                         ".png";
                            Image2 = AppLogic.GetImagePath(eSpecs.m_EntityName, "medium", true) + FN + ".png";
                            Image2File.SaveAs(TempImage2);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage2, Image2, "medium",
                                                           "image/png");

                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/pjpeg":
                            TempImage2 = AppLogic.GetImagePath(eSpecs.m_EntityName, "medium", true) + "tmp_" + FN +
                                         ".jpg";
                            Image2 = AppLogic.GetImagePath(eSpecs.m_EntityName, "medium", true) + FN + ".jpg";
                            Image2File.SaveAs(TempImage2);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage2, Image2, "medium",
                                                           "image/jpeg");

                            break;
                    }
                    AppLogic.DisposeOfTempImage(TempImage2);
                }
            }

            if (eSpecs.m_HasLargePic)
            {
                String Image3 = String.Empty;
                String TempImage3 = String.Empty;
                HttpPostedFile Image3File = fuLarge.PostedFile;
                if (Image3File != null &&
                     Image3File.ContentLength != 0)
                {
                    // delete any current image file first
                    try
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            File.Delete(AppLogic.GetImagePath(eSpecs.m_EntityName, "large", true) + FN + ss);
                        }
                    }
                    catch { }

                    String s = Image3File.ContentType;
                    switch (Image3File.ContentType)
                    {
                        case "image/gif":

                            TempImage3 = AppLogic.GetImagePath(eSpecs.m_EntityName, "large", true) + "tmp_" + FN +
                                         ".gif";
                            Image3 = AppLogic.GetImagePath(eSpecs.m_EntityName, "large", true) + FN + ".gif";
                            Image3File.SaveAs(TempImage3);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage3, Image3, "large", "image/gif");
                            AppLogic.CreateOthersFromLarge(eSpecs.m_EntityName, TempImage3, FN, "image/gif");

                            break;
                        case "image/x-png":
                        case "image/png":

                            TempImage3 = AppLogic.GetImagePath(eSpecs.m_EntityName, "large", true) + "tmp_" + FN +
                                         ".png";
                            Image3 = AppLogic.GetImagePath(eSpecs.m_EntityName, "large", true) + FN + ".png";
                            Image3File.SaveAs(TempImage3);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage3, Image3, "large", "image/png");
                            AppLogic.CreateOthersFromLarge(eSpecs.m_EntityName, TempImage3, FN, "image/png");

                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/pjpeg":

                            TempImage3 = AppLogic.GetImagePath(eSpecs.m_EntityName, "large", true) + "tmp_" + FN +
                                         ".jpg";
                            Image3 = AppLogic.GetImagePath(eSpecs.m_EntityName, "large", true) + FN + ".jpg";
                            Image3File.SaveAs(TempImage3);
                            AppLogic.ResizeEntityOrObject(eSpecs.m_EntityName, TempImage3, Image3, "large",
                                                           "image/jpeg");
                            AppLogic.CreateOthersFromLarge(eSpecs.m_EntityName, TempImage3, FN, "image/jpeg");

                            break;
                    }
                    AppLogic.DisposeOfTempImage(TempImage3);
                }
            }
        }

        private void SetProductButtons()
        {
            lnkProducts.Text = eSpecs.m_ObjectNamePlural + " for " + eName;

            if (Shipping.GetActiveShippingCalculationID() != Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts)
                lnkBulkShippingMethods.Visible = false;
        }

        #endregion
    }
}
