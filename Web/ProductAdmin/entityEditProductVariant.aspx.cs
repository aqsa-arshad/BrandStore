// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontAdmin
{
    public partial class entityEditProductVariant : AdminPageBase
    {
        private int eID;
        private string eName;
        private EntityHelper entity;
        private EntitySpecs eSpecs;
        private int pID;
        private bool ProductTracksInventoryBySizeAndColor;
        private Shipping.ShippingCalculationEnum ShipCalcID = Shipping.ShippingCalculationEnum.Unknown;
        private int skinID = 1;
        private bool UseSpecialRecurringIntervals = false;
        private int vID;
        protected bool bUseHtmlEditor;
        private GatewayProcessor _gwActual;
        //this is assumed not to be null.
        private GatewayProcessor GWActual
        {
            get
            {
                if (_gwActual == null)
                {
                    _gwActual = GatewayLoader.GetProcessor(AppLogic.ActivePaymentGatewayCleaned());
                    if (_gwActual == null)
                        _gwActual = GatewayLoader.GetProcessor("Manual");
                }
                return _gwActual;
            }
        }
        private List<DateIntervalTypeEnum> RecurringIntervals
        {
            get
            {
                return GWActual.GetAllowedRecurringIntervals();
            }
        }
        private DateIntervalTypeEnum SelectedInterval
        {
            get
            {
                int SelectedInt;
                if (!int.TryParse(ddRecurringInterval.SelectedValue, out SelectedInt))
                    return DateIntervalTypeEnum.Unknown;

                return (DateIntervalTypeEnum)SelectedInt;
            }
        }

        protected void DetermineKitItemMapppingVisiblity()
        {
            if (KitProductData.GetCountOfProductsThatHasKitItemsMappedToVariant(vID) > 0)
            {
                string pageLocale = ddLocale.SelectedValue;
                if (pageLocale.Equals(string.Empty))
                {
                    pageLocale = Localization.GetDefaultLocale();
                }

                pnlUpdateMappedKitItem.Visible = true;
                lnkMappedKitItems.NavigateUrl = string.Format("javascript:window.open('./kititemsmappedtovariant.aspx?variantid={0}&locale={1}', 'managekititems', 'scrollbars=1;width:800;status=0;toolbar=0;location=0;'); javascript:void(0);", vID, pageLocale);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (!IsPostBack)
            {
                ViewState.Add("VariantEditID", 0);
            }

            // Determine HTML editor configuration
            bUseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
            radDescription.Visible = bUseHtmlEditor;
            radDescription.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);

            UseSpecialRecurringIntervals = AppLogic.UseSpecialRecurringIntervals();
            ShipCalcID = Shipping.GetActiveShippingCalculationID();
            vID = CommonLogic.QueryStringNativeInt("VariantID");
            pID = CommonLogic.QueryStringNativeInt("ProductID");
            eID = CommonLogic.QueryStringNativeInt("EntityID");
            eName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            eSpecs = EntityDefinitions.LookupSpecs(eName);

            if (pID > 0)
            {
                ltProduct.Text = "<a href=\"" + AppLogic.AdminLinkUrl("entityEditProducts.aspx") + "?iden=" + pID + "&entityName=" + eName + "&entityFilterID=" + eID + "\">" + AppLogic.GetProductName(pID, ThisCustomer.LocaleSetting) + " (" + pID + ")</a>&nbsp;";
            }

            if (Localization.ParseNativeInt(ViewState["VariantEditID"].ToString()) > 0)
            {
                vID = Localization.ParseNativeInt(ViewState["VariantEditID"].ToString());
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

            lblerr.Text = string.Empty;
            if (!IsPostBack)
            {
                if (ThisCustomer.ThisCustomerSession.Session("entityUserLocale").Length == 0)
                {
                    ThisCustomer.ThisCustomerSession.SetVal("entityUserLocale", Localization.GetDefaultLocale());
                }

                if (!GWActual.RecurringIntervalSupportsMultiplier)
                {
                    txtRecurringIntervalMultiplier.Text = "1";
                    trRecurringInterval.Visible = false;
                }

                ddRecurringInterval.Items.Clear();
                //Only display PayFlowPro's supported intervals
                if (UseSpecialRecurringIntervals)
                {
                    ddRecurringInterval.Items.Add(
                            new ListItem(DateIntervalTypeEnum.Weekly.ToString(),
                                          ((int)DateIntervalTypeEnum.Weekly).ToString()));
                    ddRecurringInterval.Items.Add(
                            new ListItem(DateIntervalTypeEnum.BiWeekly.ToString(),
                                          ((int)DateIntervalTypeEnum.BiWeekly).ToString()));
                    ddRecurringInterval.Items.Add(
                            new ListItem(DateIntervalTypeEnum.Monthly.ToString(),
                                          ((int)DateIntervalTypeEnum.Monthly).ToString()));
                    ddRecurringInterval.Items.Add(
                            new ListItem(DateIntervalTypeEnum.EveryFourWeeks.ToString(),
                                          ((int)DateIntervalTypeEnum.EveryFourWeeks).ToString()));
                    ddRecurringInterval.Items.Add(
                            new ListItem(DateIntervalTypeEnum.Quarterly.ToString(),
                                          ((int)DateIntervalTypeEnum.Quarterly).ToString()));
                    ddRecurringInterval.Items.Add(
                            new ListItem(DateIntervalTypeEnum.SemiYearly.ToString(),
                                          ((int)DateIntervalTypeEnum.SemiYearly).ToString()));
                    ddRecurringInterval.Items.Add(
                            new ListItem(DateIntervalTypeEnum.Yearly.ToString(),
                                          ((int)DateIntervalTypeEnum.Yearly).ToString()));
                }
                else
                {
                    foreach (DateIntervalTypeEnum intType in GWActual.GetAllowedRecurringIntervals())
                        ddRecurringInterval.Items.Add(new ListItem(intType.ToString(), ((int)intType).ToString()));
                }

                Product kitCheck = new Product(pID);
                if (kitCheck.IsAKit)    //Hide recurring options for kits
                {
                    pnlRecurring.Visible = false;
                }

                rblSubscriptionIntervalType.Items.Clear();
                if (UseSpecialRecurringIntervals)
                {
                    rblSubscriptionIntervalType.Items.Add(
                            new ListItem(DateIntervalTypeEnum.Weekly.ToString(),
                                          ((int)DateIntervalTypeEnum.Weekly).ToString()));
                    rblSubscriptionIntervalType.Items.Add(
                            new ListItem(DateIntervalTypeEnum.BiWeekly.ToString(),
                                          ((int)DateIntervalTypeEnum.BiWeekly).ToString()));
                    rblSubscriptionIntervalType.Items.Add(
                            new ListItem(DateIntervalTypeEnum.Monthly.ToString(),
                                          ((int)DateIntervalTypeEnum.Monthly).ToString()));
                    rblSubscriptionIntervalType.Items.Add(
                            new ListItem(DateIntervalTypeEnum.EveryFourWeeks.ToString(),
                                          ((int)DateIntervalTypeEnum.EveryFourWeeks).ToString()));
                    rblSubscriptionIntervalType.Items.Add(
                            new ListItem(DateIntervalTypeEnum.Quarterly.ToString(),
                                          ((int)DateIntervalTypeEnum.Quarterly).ToString()));
                    rblSubscriptionIntervalType.Items.Add(
                            new ListItem(DateIntervalTypeEnum.SemiYearly.ToString(),
                                          ((int)DateIntervalTypeEnum.SemiYearly).ToString()));
                    rblSubscriptionIntervalType.Items.Add(
                            new ListItem(DateIntervalTypeEnum.Yearly.ToString(),
                                          ((int)DateIntervalTypeEnum.Yearly).ToString()));
                }
                else
                {
                    foreach (DateIntervalTypeEnum intType in GWActual.GetAllowedRecurringIntervals())
                        rblSubscriptionIntervalType.Items.Add(new ListItem(intType.ToString(), ((int)intType).ToString()));
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
                LoadScript();
                LoadContent();
                LoadStringResources();
            }
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
            {
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";
            }

            if (error.Length > 0)
            {
                str += error + "";
            }
            else
            {
                str = "";
            }

            ((Literal)Form.FindControl("ltError")).Text = str;
        }

        protected void LoadScript()
        {
            ltScript.Text = "";

            ltStyles.Text = "";
        }

        protected void LoadContent()
        {
            //pull locale from user session
            string entityLocale = ThisCustomer.ThisCustomerSession.Session("entityUserLocale");
            if (entityLocale.Length > 0)
            {
                try
                {
                    ddLocale.SelectedValue = entityLocale;
                    // user's locale may not exist any more, so don't let the assignment crash
                }
                catch { }
            }

            string locale = Localization.CheckLocaleSettingForProperCase(ddLocale.SelectedValue);
            if (ddLocale.SelectedValue == string.Empty)
            {
                locale = Localization.GetDefaultLocale();
            }

            bool Editing = false;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select PV.*,P.ColorOptionPrompt,P.SizeOptionPrompt from Product P  with (NOLOCK)  , productvariant PV   with (NOLOCK)  where PV.ProductID=P.ProductID and VariantID=" + vID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                        if (DB.RSFieldBool(rs, "Deleted"))
                        {
                            btnDeleteVariant.Text = "UnDelete this Variant";
                        }
                        else
                        {
                            btnDeleteVariant.Text = "Delete this Variant";
                        }

                        if (0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where VariantID=" + vID.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
                        {
                            btnDeleteVariant.Text = AppLogic.GetString("admin.common.DeleteNA", skinID, ThisCustomer.LocaleSetting) + " <img title=\"" + AppLogic.GetString("admin.common.DeleteNAInfoRecurring", skinID, ThisCustomer.LocaleSetting) + "\" src=\"../App_Themes/Admin_Default/images/info.gif\" border=\"0\" align=\"absmiddle\" alt=\"\" />";
                            btnDeleteVariant.Enabled = false;
                        }
                    }

                    ViewState.Add("VariantEdit", Editing);

                    bool IsAKit = AppLogic.IsAKit(pID);

                    String ColorOptionPrompt = DB.RSFieldByLocale(rs, "ColorOptionPrompt", ddLocale.SelectedValue);
                    String SizeOptionPrompt = DB.RSFieldByLocale(rs, "SizeOptionPrompt", ddLocale.SelectedValue);
                    if (ColorOptionPrompt.Length == 0)
                    {
                        ColorOptionPrompt =
                                AppLogic.GetString("AppConfig.ColorOptionPrompt", skinID, ThisCustomer.LocaleSetting);
                    }
                    if (SizeOptionPrompt.Length == 0)
                    {
                        SizeOptionPrompt = AppLogic.GetString("AppConfig.SizeOptionPrompt", skinID, ThisCustomer.LocaleSetting);
                    }

                    txtFroogle.Columns = Localization.ParseNativeInt(AppLogic.AppConfig("Admin_TextareaWidth"));
                    txtFroogle.Rows = 10;

                    //SHIPPING
                    trShippingCost.Visible = false;
                    trShipSeparately.Visible = false;
                    if (ShipCalcID == Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts)
                    {
                        trShippingCost.Visible = true;
                        int NF = 0;
                        string temp = "";

                        using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                        {
                            conn.Open();
                            using (IDataReader rs3 = DB.GetRS("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 order by DisplayOrder", conn))
                            {
                                while (rs3.Read())
                                {
                                    temp += (DB.RSFieldByLocale(rs3, "Name", ThisCustomer.LocaleSetting) + ": ");
                                    temp += ("<input class=\"default\" maxLength=\"10\" size=\"10\" name=\"ShippingCost_" + DB.RSFieldInt(rs3, "ShippingMethodID") + "\" value=\"" + CommonLogic.IIF(Editing, Localization.CurrencyStringForDBWithoutExchangeRate(Shipping.GetVariantShippingCost(vID, DB.RSFieldInt(rs3, "ShippingMethodID"))), "") + "\">  (in format x.xx)\n");
                                    temp += ("<input type=\"hidden\" name=\"ShippingCost_" + DB.RSFieldInt(rs3, "ShippingMethodID") + "_vldt\" value=\"[number][invalidalert=Please enter a valid dollar amount, e.g. 10.00 without the leading $ sign!]\">\n");
                                    NF++;
                                }

                            }
                        }

                        if (NF == 0)
                        {
                            temp += ("No Shipping Methods Are Found");
                        }
                        ltShippingCost.Text = temp;
                        rblShipSeparately.SelectedIndex = 0;
                    }
                    else
                    {
                        trShipSeparately.Visible = true;
                        rblShipSeparately.SelectedIndex = (DB.RSFieldBool(rs, "IsShipSeparately") ? 1 : 0);
                    }

                    //INVENTORY
                    ProductTracksInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(pID);

                    if (Editing)
                    {
                        btnDeleteVariant.Visible = true;
                        ltStatus.Text = "Editing Variant";
                        btnSubmit.Text = "Update";

                        txtName.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "Name"), locale, false);

                        txtSKU.Text = Server.HtmlEncode(DB.RSField(rs, "SKUSuffix"));
                        txtManufacturePartNumber.Text = Server.HtmlEncode(DB.RSField(rs, "ManufacturerPartNumber"));
                        txtGTIN.Text = Server.HtmlEncode(DB.RSField(rs, "GTIN"));
                        if (!DB.RSFieldBool(rs, "Published"))
                        {
                            rblPublished.BackColor = Color.LightYellow;
                        }
                        rblPublished.SelectedIndex = (DB.RSFieldBool(rs, "Published") ? 1 : 0);

                        rblRecurring.SelectedIndex = (DB.RSFieldBool(rs, "IsRecurring") ? 1 : 0);

                        if (!UseSpecialRecurringIntervals)
                        {
                            txtRecurringIntervalMultiplier.Text = DB.RSFieldInt(rs, "RecurringInterval").ToString();
                        }
                        else
                        {
                            txtRecurringIntervalMultiplier.Text = "1";
                            trRecurringInterval.Visible = false;
                        }
                        try
                        {
                            ddRecurringInterval.SelectedValue = DB.RSFieldInt(rs, "RecurringIntervalType").ToString();
                        }
                        catch { }

                        //DESCRIPTION
                        if (bUseHtmlEditor)
                        {
                            radDescription.Content = DB.RSFieldByLocale(rs, "Description", locale);
                        }
                        else
                        {
                            ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                            ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\">" + XmlCommon.GetLocaleEntry(HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Description")), locale, false) + "</textarea>\n");
                            ltDescription.Text += ("</div>");
                        }

                        //FROOGLE
                        txtFroogle.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "FroogleDescription"), locale, false);

                        txtRestrictedQuantities.Text = DB.RSField(rs, "RestrictedQuantities");
                        txtMinimumQuantity.Text = CommonLogic.IIF(DB.RSFieldInt(rs, "MinimumQuantity") != 0, DB.RSFieldInt(rs, "MinimumQuantity").ToString(), "");

                        int NumCustomerLevels = DB.GetSqlN("select count(*) as N from dbo.CustomerLevel cl  with (NOLOCK)  join dbo.ProductCustomerLevel pcl  with (NOLOCK)  on cl.CustomerLevelID = pcl.CustomerLevelID where pcl.ProductID = " + pID.ToString() + " and cl.deleted = 0");
                        //PRICE
                        txtPrice.Text = Localization.ParseNativeCurrency(DB.RSFieldDecimal(rs, "Price").ToString()).ToString();
                        if (NumCustomerLevels > 0)
                        {
                            ltExtendedPricing.Text = "<a href=\"EditExtendedPrices.aspx?ProductID=" + pID.ToString() + "&VariantID=" + vID.ToString() + "\">" + "admin.defineextendedprices".StringResource() + "</a> (Defined By Customer Level)";
                        }
                        txtSalePrice.Text = CommonLogic.IIF(DB.RSFieldDecimal(rs, "SalePrice") != Decimal.Zero, Localization.ParseNativeCurrency(DB.RSFieldDecimal(rs, "SalePrice").ToString()).ToString(), "");

                        trCustomerEntersPrice1.Visible = false;
                        trCustomerEntersPricePrompt.Visible = false;
                        if (IsAKit)
                        {
                            rblCustomerEntersPrice.SelectedIndex = 0;
                            txtCustomerEntersPricePrompt.Text = "";
                        }
                        else
                        {
                            trCustomerEntersPrice1.Visible = true;
                            trCustomerEntersPricePrompt.Visible = true;
                            rblCustomerEntersPrice.SelectedIndex = (DB.RSFieldBool(rs, "CustomerEntersPrice") ? 1 : 0);
                            txtCustomerEntersPricePrompt.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "CustomerEntersPricePrompt"), locale, false);
                        }
                        txtMSRP.Text = CommonLogic.IIF(DB.RSFieldDecimal(rs, "MSRP") != Decimal.Zero, Localization.ParseNativeCurrency(DB.RSFieldDecimal(rs, "MSRP").ToString()).ToString(), "");
                        txtActualCost.Text = CommonLogic.IIF(DB.RSFieldDecimal(rs, "Cost") != Decimal.Zero, Localization.ParseNativeCurrency(DB.RSFieldDecimal(rs, "Cost").ToString()).ToString(), "");

                        rblTaxable.SelectedIndex = (DB.RSFieldBool(rs, "IsTaxable") ? 1 : 0);

                        rblFreeShipping.SelectedIndex = (DB.RSFieldTinyInt(rs, "FreeShipping"));

                        rblDownload.SelectedIndex = (DB.RSFieldBool(rs, "IsDownload") ? 1 : 0);
                        rblCondition.SelectedIndex = (DB.RSFieldTinyInt(rs, "Condition"));
                        txtDownloadLocation.Text = DB.RSField(rs, "DownloadLocation");
                        txtValidForDays.Text = DB.RSFieldInt(rs, "DownloadValidDays") > 0 ? DB.RSFieldInt(rs, "DownloadValidDays").ToString() : string.Empty;
                        txtWeight.Text = Localization.ParseNativeDecimal(DB.RSFieldDecimal(rs, "Weight").ToString()).ToString();
                        txtDimensions.Text = Server.HtmlEncode(DB.RSField(rs, "Dimensions"));

                        //INVENTORY
                        trCurrentInventory.Visible = false;
                        trManageInventory.Visible = false;
                        if (ProductTracksInventoryBySizeAndColor)
                        {
                            trManageInventory.Visible = true;
                            ltManageInventory.Text = ("<a href=\"entityEditInventory.aspx?productid=" + pID.ToString() + "&variantid=" + vID.ToString() + "&entityname=" + eName + "&EntityID=" + eID.ToString() + "\">Click Here</a>\n");
                        }
                        else
                        {
                            trCurrentInventory.Visible = true;
                            CurrentInventory = DB.RSFieldInt(rs, "Inventory");
                        }

                        if (!UseSpecialRecurringIntervals)
                        {
                            txtSubscriptionInterval.Text = DB.RSFieldInt(rs, "SubscriptionInterval").ToString();
                            if (DB.RSFieldInt(rs, "SubscriptionIntervalType") ==
                                 ((int)DateIntervalTypeEnum.Day))
                            {
                                rblSubscriptionIntervalType.SelectedIndex = 0;
                            }
                            else if (DB.RSFieldInt(rs, "SubscriptionIntervalType") ==
                                      ((int)DateIntervalTypeEnum.Week))
                            {
                                rblSubscriptionIntervalType.SelectedIndex = 1;
                            }
                            else if (DB.RSFieldInt(rs, "SubscriptionIntervalType") ==
                                      ((int)DateIntervalTypeEnum.Month))
                            {
                                rblSubscriptionIntervalType.SelectedIndex = 2;
                            }
                            else
                            {
                                rblSubscriptionIntervalType.SelectedIndex = 3;
                            }
                        }
                        else
                        {
                            try
                            {
                                rblSubscriptionIntervalType.SelectedValue = DB.RSFieldInt(rs, "SubscriptionIntervalType").ToString();
                            }
                            catch { }
                        }

                        //BG COLOR
                        txtPageBG.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "PageBGColor"), "");
                        txtContentsBG.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ContentsBGColor"), "");
                        txtSkinColor.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "GraphicsColor"), "");

                        // BEGIN IMAGES 
                        txtImageOverride.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ImageFilenameOverride"), "");
                        txtSEAlt.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "SEAltText"), "");

                        bool disableupload = (Editing && DB.RSField(rs, "ImageFilenameOverride") != "");

                        if (eSpecs.m_HasIconPic)
                        {
                            fuIcon.Enabled = !disableupload;
                        }
                        if (eSpecs.m_HasMediumPic)
                        {
                            fuMedium.Enabled = !disableupload;
                        }
                        if (eSpecs.m_HasLargePic)
                        {
                            fuLarge.Enabled = !disableupload;
                        }
                        String Image1URL = AppLogic.LookupImage("Variant", vID, "icon", skinID, ThisCustomer.LocaleSetting);
                        if (Image1URL.Length == 0)
                        {
                            Image1URL = AppLogic.NoPictureImageURL(false, skinID, ThisCustomer.LocaleSetting);
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
                                ltIcon.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image1URL + "','Pic1');\">Click here</a> to delete the current image <br/>\n");
                            }
                            ltIcon.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic1\" name=\"Pic1\" border=\"0\" src=\"" + Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                        }
                        String Image2URL = AppLogic.LookupImage("Variant", vID, "medium", skinID, ThisCustomer.LocaleSetting);
                        if (Image2URL.Length == 0)
                        {
                            Image2URL = AppLogic.NoPictureImageURL(false, skinID, ThisCustomer.LocaleSetting);
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
                                ltMedium.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image2URL + "','Pic2');\">Click here</a> to delete the current image <br/>\n");
                            }
                            ltMedium.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic2\" name=\"Pic2\" border=\"0\" src=\"" + Image2URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                        }
                        String Image3URL = AppLogic.LookupImage("Variant", vID, "large", skinID, ThisCustomer.LocaleSetting);
                        if (Image3URL.Length == 0)
                        {
                            Image3URL = AppLogic.NoPictureImageURL(false, skinID, ThisCustomer.LocaleSetting);
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
                                ltLarge.Text = ("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image3URL + "','Pic3');\">Click here</a> to delete the current image <br/>\n");
                            }
                            ltLarge.Text += "<img align=\"absmiddle\" style=\"margin-top: 3px; margin-bottom: 5px;\" id=\"Pic3\" name=\"Pic3\" border=\"0\" src=\"" + Image3URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\" />\n";
                        }
                        // END IMAGES   

                        // COLORS & SIZES:
                        VariantColors = XmlCommon.GetLocaleEntry(Server.HtmlDecode(DB.RSField(rs, "Colors")), locale, false);
                        txtColorSKUModifiers.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "ColorSKUModifiers"), locale, false);
                        VariantSizes = XmlCommon.GetLocaleEntry(Server.HtmlDecode(DB.RSField(rs, "Sizes")), locale, false);
                        txtSizeSKUModifiers.Text = XmlCommon.GetLocaleEntry(DB.RSField(rs, "SizeSKUModifiers"), locale, false);

                        string temp = "";
                        trColorSizeSummary.Visible = false;
                        // size/color tables for display purposes only:
                        if (Editing && (DB.RSField(rs, "Colors").Length != 0 || DB.RSField(rs, "ColorSKUModifiers").Length != 0 || DB.RSField(rs, "Sizes").Length != 0 || DB.RSField(rs, "SizeSKUModifiers").Length != 0))
                        {
                            trColorSizeSummary.Visible = true;
                            temp += "<br/><b>" + ColorOptionPrompt + "/" + SizeOptionPrompt + " Tables:&nbsp;&nbsp;<small>(summary)</small></b>";
                            temp += ("<table border=\"1\" cellpadding=\"0\" cellspacing=\"0\">\n");
                            temp += ("<tr valign=\"left\">\n");
                            temp += ("<td align=\"left\" valign=\"top\">\n");

                            //decode special characters for color & size attribute to be displayed on a summary table
                            String[] Colors = Server.HtmlDecode(DB.RSFieldByLocale(rs, "Colors", locale)).Split(',');
                            String[] Sizes = Server.HtmlDecode(DB.RSFieldByLocale(rs, "Sizes", locale)).Split(',');
                            String[] ColorSKUModifiers = DB.RSFieldByLocale(rs, "ColorSKUModifiers", locale).Split(',');
                            String[] SizeSKUModifiers = DB.RSFieldByLocale(rs, "SizeSKUModifiers", locale).Split(',');

                            for (int i = Colors.GetLowerBound(0); i <= Colors.GetUpperBound(0); i++)
                            {
                                Colors[i] = Colors[i].Trim();
                            }

                            for (int i = Sizes.GetLowerBound(0); i <= Sizes.GetUpperBound(0); i++)
                            {
                                Sizes[i] = Sizes[i].Trim();
                            }

                            for (int i = ColorSKUModifiers.GetLowerBound(0); i <= ColorSKUModifiers.GetUpperBound(0); i++)
                            {
                                ColorSKUModifiers[i] = ColorSKUModifiers[i].Trim();
                            }

                            for (int i = SizeSKUModifiers.GetLowerBound(0); i <= SizeSKUModifiers.GetUpperBound(0); i++)
                            {
                                SizeSKUModifiers[i] = SizeSKUModifiers[i].Trim();
                            }

                            int ColorCols = Colors.GetUpperBound(0);
                            int SizeCols = Sizes.GetUpperBound(0);
                            ColorCols = Math.Max(ColorCols, ColorSKUModifiers.GetUpperBound(0));
                            SizeCols = Math.Max(SizeCols, SizeSKUModifiers.GetUpperBound(0));

                            if (DB.RSField(rs, "Colors").Length != 0 ||
                                 DB.RSField(rs, "ColorSKUModifiers").Length != 0)
                            {
                                temp += ("<tr>\n");
                                temp += ("<td><b>" + ColorOptionPrompt + "</b></td>\n");
                                for (int i = 0; i <= ColorCols; i++)
                                {
                                    String ColorVal = String.Empty;
                                    try
                                    {
                                        ColorVal = Colors[i];
                                    }
                                    catch { }
                                    if (ColorVal.Length == 0)
                                    {
                                        ColorVal = "&nbsp;";
                                    }
                                    temp += ("<td align=\"center\">" + (ColorVal.EqualsIgnoreCase("&nbsp;") ? ColorVal : Server.HtmlEncode(ColorVal)) + "</td>\n");
                                }
                                temp += ("<tr>\n");
                                temp += ("<tr>\n");
                                temp += ("<td><b>SKU Modifier</b></td>\n");
                                for (int i = 0; i <= ColorCols; i++)
                                {
                                    String SKUVal = String.Empty;
                                    try
                                    {
                                        SKUVal = ColorSKUModifiers[i];
                                    }
                                    catch { }
                                    if (SKUVal.Length == 0)
                                    {
                                        SKUVal = "&nbsp;";
                                    }
                                    temp += ("<td align=\"center\">" + (SKUVal.EqualsIgnoreCase("&nbsp;") ? SKUVal : Server.HtmlEncode(SKUVal)) + "</td>\n");
                                }
                                temp += ("<tr>\n");
                                temp += ("</table>\n");
                                temp += ("<br/><br/>");
                            }

                            if (DB.RSField(rs, "Sizes").Length != 0 ||
                                 DB.RSField(rs, "SizeSKUModifiers").Length != 0)
                            {
                                temp += ("<table cellpadding=\"2\" cellspacing=\"0\" border=\"1\">\n");
                                temp += ("<tr>\n");
                                temp += ("<td><b>" + SizeOptionPrompt + "</b></td>\n");
                                for (int i = 0; i <= SizeCols; i++)
                                {
                                    String SizeVal = String.Empty;
                                    try
                                    {
                                        SizeVal = Sizes[i];
                                    }
                                    catch { }
                                    if (SizeVal.Length == 0)
                                    {
                                        SizeVal = "&nbsp;";
                                    }
                                    temp += ("<td align=\"center\">" + (SizeVal.EqualsIgnoreCase("&nbsp;") ? SizeVal : Server.HtmlEncode(SizeVal)) + "</td>\n");
                                }
                                temp += ("<tr>\n");
                                temp += ("<tr>\n");
                                temp += ("<td><b>SKU Modifier</b></td>\n");
                                for (int i = 0; i <= SizeCols; i++)
                                {
                                    String SKUVal = String.Empty;
                                    try
                                    {
                                        SKUVal = SizeSKUModifiers[i];
                                    }
                                    catch { }
                                    if (SKUVal.Length == 0)
                                    {
                                        SKUVal = "&nbsp;";
                                    }
                                    temp += ("<td align=\"center\">" + (SKUVal.EqualsIgnoreCase("&nbsp;") ? SKUVal : Server.HtmlEncode(SKUVal)) + "</td>\n");
                                }
                                temp += ("<tr>\n");
                                temp += ("</table>\n");
                            }

                            temp += ("</td>\n");
                            temp += ("</tr></table>\n");
                        }
                        ltColorSizeSummary.Text = temp;
                    }
                    else
                    {
                        btnDeleteVariant.Visible = false;
                        ltStatus.Text = "Adding Variant";
                        btnSubmit.Text = "Add New";

                        //DESCRIPTION
                        if (!bUseHtmlEditor)
                        {
                            ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                            ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\"></textarea>\n");
                            ltDescription.Text += ("</div>");
                        }

                        //INVENTORY
                        trCurrentInventory.Visible = false;
                        trManageInventory.Visible = false;
                        if (ProductTracksInventoryBySizeAndColor)
                        {
                            trManageInventory.Visible = true;
                            ltManageInventory.Text = ("<a href=\"entityEditInventory.aspx?productid=" + pID.ToString() +
                                                       "&variantid=" + vID.ToString() + "\">Click Here</a>\n");
                        }
                        else
                        {
                            trCurrentInventory.Visible = true;
                        }

                        //set defaults
                        rblCustomerEntersPrice.SelectedIndex = 0;
                        rblDownload.SelectedIndex = 0;
                        rblCondition.SelectedIndex = 0;
                        rblFreeShipping.SelectedIndex = 0;
                        rblPublished.SelectedIndex = 1;
                        rblRecurring.SelectedIndex = 0;
                        ddRecurringInterval.SelectedIndex = 2;
                        rblShipSeparately.SelectedIndex = 0;
                        rblSubscriptionIntervalType.SelectedIndex = 2;
                        rblTaxable.SelectedIndex = 1;
                    }

                    if (UseSpecialRecurringIntervals)
                    {
                        txtRecurringIntervalMultiplier.Text = "1";
                        trRecurringInterval.Visible = false;
                    }

                    ltScript2.Text = ("<script type=\"text/javascript\">\n");
                    ltScript2.Text += ("function DeleteImage(imgurl,name)\n");
                    ltScript2.Text += ("{\n");
                    ltScript2.Text += ("if(confirm('Are you sure you want to delete this image?')){\n");
                    ltScript2.Text +=
                            ("window.open('deleteimage.aspx?imgurl=' + imgurl + '&FormImageName=' + name,\"Admin_ML\",\"height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
                    ltScript2.Text += ("}}\n");
                    ltScript2.Text += ("</SCRIPT>\n");
                }
            }

            DetermineKitItemMapppingVisiblity();
        }

        protected void LoadStringResources()
        {
            pnlMain.HeaderText = AppLogic.GetString("admin.tabs.Main", 1, ThisCustomer.LocaleSetting);
            pnlImages.HeaderText = AppLogic.GetString("admin.tabs.Images", 1, ThisCustomer.LocaleSetting);
            pnlDescription.HeaderText = AppLogic.GetString("admin.tabs.Description", 1, ThisCustomer.LocaleSetting);
            pnlProductFeeds.HeaderText = AppLogic.GetString("admin.tabs.ProductFeeds", 1, ThisCustomer.LocaleSetting);
            pnlAttributes.HeaderText = AppLogic.GetString("admin.tabs.Attributes", 1, ThisCustomer.LocaleSetting);
            pnlMiscellaneous.HeaderText = AppLogic.GetString("admin.tabs.Miscellaneous", 1, ThisCustomer.LocaleSetting);
        }

        #region formProperties

        private int CurrentInventory
        {
            get
            {
                try
                {
                    return int.Parse(txtCurrentInventory.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                txtCurrentInventory.Text = value.ToString();
            }
        }

        #endregion


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
            UpdateVariant();
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            if (UpdateVariant())
            {
                ClientScript.RegisterStartupScript(Page.GetType(), Guid.NewGuid().ToString(), "CloseAndRebind(" + vID + ");", true);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(Page.GetType(), Guid.NewGuid().ToString(), "CancelEdit();", true);
        }

        protected Boolean RecurringIntervalIsValid()
        {
            if (rblRecurring.SelectedValue == "0")
                return true;

            Boolean intervalmatches = RecurringIntervals.Contains(SelectedInterval);
            IntervalValidator iv = GWActual.GetIntervalValidator(SelectedInterval);
            ValidatorResult vr = iv.Validate(txtRecurringIntervalMultiplier.Text);

            if (intervalmatches && vr.IsValid)
                return true;

            StringBuilder error = new StringBuilder();
            if (!intervalmatches)
                error.Append("The " + GWActual.DisplayName(ThisCustomer.LocaleSetting) + " gateway does not support the selected interval. Please choose a supported interval.<br />");
            if (!vr.IsValid)
                error.Append("There was an error with the recurring interval that you entered: " + vr.Message);

            resetError(error.ToString(), true);

            return false;
        }
        protected void UpdateCustomerStatus()
        {
            try
            {

            }
            catch { 
            
            }
        }

        protected Boolean UpdateVariant()
        {
            if (!RecurringIntervalIsValid())
                return false;

            bool Editing = Localization.ParseBoolean(ViewState["VariantEdit"].ToString());
            StringBuilder sql = new StringBuilder();

            string locale = ddLocale.SelectedValue;
            if (locale.Equals(string.Empty))
            {
                locale = Localization.GetDefaultLocale();
            }


            decimal Price = Decimal.Zero;
            decimal SalePrice = Decimal.Zero;
            decimal MSRP = Decimal.Zero;
            decimal Cost = Decimal.Zero;
            int Points = 0;
            int MinimumQuantity = 0;
            if (txtPrice.Text.Length != 0)
            {
                Price = Localization.ParseNativeDecimal(txtPrice.Text);
            }
            if (txtSalePrice.Text.Length != 0)
            {
                SalePrice = Localization.ParseNativeDecimal(txtSalePrice.Text);
            }

            if (txtMSRP.Text.Length != 0)
            {
                MSRP = Localization.ParseNativeDecimal(txtMSRP.Text);
            }
            if (txtActualCost.Text.Length != 0)
            {
                Cost = Localization.ParseNativeDecimal(txtActualCost.Text);
            }

            if (txtMinimumQuantity.Text.Length != 0)
            {
                MinimumQuantity = Localization.ParseNativeInt(txtMinimumQuantity.Text);
            }

            bool IsFirstVariantAdded = (DB.GetSqlN("select count(VariantID) as N from ProductVariant  with (NOLOCK)  where ProductID=" + pID.ToString() + " and Deleted=0") == 0);

            if (!Editing)
            {
                // ok to ADD them:
                String NewGUID = DB.GetNewGUID();
                sql.Append("insert into productvariant(VariantGUID,ProductID,Name,ContentsBGColor,PageBGColor,GraphicsColor,ImageFilenameOverride,SEAltText,IsDefault,Description,RestrictedQuantities,FroogleDescription,Price,SalePrice,MSRP,Cost,Points,MinimumQuantity,SKUSuffix,ManufacturerPartNumber,GTIN,Weight,Dimensions,Inventory,SubscriptionInterval,SubscriptionIntervalType,Published,CustomerEntersPrice,CustomerEntersPricePrompt,IsRecurring,RecurringInterval,RecurringIntervalType,Colors,ColorSKUModifiers,Sizes,SizeSKUModifiers,IsTaxable,IsShipSeparately,IsDownload,FreeShipping,DownloadLocation,DownloadValidDays,Condition) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(pID.ToString() + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXmlVariant("Name", txtName.Text, locale, vID)) + ",");
                sql.Append(DB.SQuote(txtContentsBG.Text) + ",");
                sql.Append(DB.SQuote(txtPageBG.Text) + ",");
                sql.Append(DB.SQuote(txtSkinColor.Text) + ",");
                sql.Append(DB.SQuote(txtImageOverride.Text) + ",");
                if (txtSEAlt.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtSEAlt.Text) + ",");
                }
                else
                {
                    sql.Append(DB.SQuote(txtName.Text) + ",");
                }
                if (IsFirstVariantAdded)
                {
                    sql.Append("1,"); // IsDefault=1
                }
                else
                {
                    sql.Append("0,"); // IsDefault=0
                }
                string temp = string.Empty;
                if (bUseHtmlEditor)
                {
                    temp = AppLogic.FormLocaleXmlVariant("Description", radDescription.Content, locale, vID);
                }
                else
                {
                    temp = AppLogic.FormLocaleXmlEditorVariant("Description", "Description", locale, vID);
                }
                if (temp.Length != 0)
                {
                    sql.Append(DB.SQuote(temp) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (txtRestrictedQuantities.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtRestrictedQuantities.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                temp =
                        AppLogic.FormLocaleXmlVariant("FroogleDescription", txtFroogle.Text, locale, vID);
                if (temp.Length != 0)
                {
                    sql.Append(DB.SQuote(temp) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(Localization.DecimalStringForDB(Price) + ",");
                sql.Append(CommonLogic.IIF(SalePrice != Decimal.Zero, Localization.DecimalStringForDB(SalePrice), "NULL") + ",");
                sql.Append(CommonLogic.IIF(MSRP != Decimal.Zero, Localization.DecimalStringForDB(MSRP), "NULL") + ",");
                sql.Append(CommonLogic.IIF(Cost != Decimal.Zero, Localization.DecimalStringForDB(Cost), "NULL") + ",");
                sql.Append(Localization.IntStringForDB(Points) + ",");
                sql.Append(CommonLogic.IIF(MinimumQuantity != 0, Localization.IntStringForDB(MinimumQuantity), "NULL") + ",");

                if (txtSKU.Text.Length != 0)
                    sql.Append(DB.SQuote(txtSKU.Text) + ",");
                else
                    sql.Append("NULL,");

                if (txtManufacturePartNumber.Text.Length != 0)
                    sql.Append(DB.SQuote(txtManufacturePartNumber.Text) + ",");
                else
                    sql.Append("NULL,");

                if (txtGTIN.Text.Length != 0)
                    sql.Append(DB.SQuote(txtGTIN.Text) + ",");
                else
                    sql.Append("NULL,");

                decimal Weight = Localization.ParseNativeDecimal(txtWeight.Text);
                sql.Append(CommonLogic.IIF(Weight != 0.0M, Localization.DecimalStringForDB(Weight), "NULL") + ",");
                if (txtDimensions.Text.Length != 0)
                {
                    sql.Append(DB.SQuote(txtDimensions.Text) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(CurrentInventory.ToString() + ",");
                sql.Append(Localization.ParseNativeInt(txtSubscriptionInterval.Text).ToString() + ",");
                sql.Append(rblSubscriptionIntervalType.SelectedValue.ToString() + ",");
                sql.Append(rblPublished.SelectedValue + ",");
                sql.Append(rblCustomerEntersPrice.SelectedValue + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXmlVariant("CustomerEntersPricePrompt", txtCustomerEntersPricePrompt.Text, locale, vID)) + ",");
                sql.Append(rblRecurring.SelectedValue + ",");
                sql.Append(Localization.ParseNativeInt(txtRecurringIntervalMultiplier.Text).ToString() + ",");
                sql.Append(ddRecurringInterval.SelectedValue + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXmlVariant("Colors", VariantColors, locale, vID).Replace(", ", ",").Replace(" ,", ",").Replace("'", "").Trim()) + ",");
                sql.Append(DB.SQuote(txtColorSKUModifiers.Text.Replace(", ", ",").Replace(" ,", ",").Trim()) + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXmlVariant("Sizes", VariantSizes, locale, vID).Replace(", ", ",").Replace(" ,", ",").Replace("'", "").Trim()) + ",");
                sql.Append(DB.SQuote(txtSizeSKUModifiers.Text.Replace(", ", ",").Replace(" ,", ",").Trim()) + ",");
                sql.Append(rblTaxable.SelectedValue + ",");
                sql.Append(rblShipSeparately.SelectedValue + ",");
                sql.Append(rblDownload.SelectedValue + ",");
                sql.Append(rblFreeShipping.SelectedValue + ",");
                String DLoc = txtDownloadLocation.Text;
                if (DLoc.StartsWith("/"))
                {
                    DLoc = DLoc.Substring(1, DLoc.Length - 1); // remove leading / char!
                }
                sql.Append(DB.SQuote(DLoc) + ",");

                int validDays = 0;
                sql.Append((int.TryParse(txtValidForDays.Text, out validDays) ? validDays.ToString() : "NULL") + ",");
                sql.Append(rblCondition.SelectedValue);
                sql.Append(")");
                try
                {
                    DB.ExecuteSQL(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Error in EditVariant(.RunSql), Msg=[" + CommonLogic.GetExceptionDetail(ex, String.Empty) + "], Sql=[" + sql.ToString() + "]");
                }

                //Get variantID for editing
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select VariantID from productvariant with (NOLOCK) where deleted=0 and VariantGUID=" + DB.SQuote(NewGUID), dbconn))
                    {
                        rs.Read();
                        vID = DB.RSFieldInt(rs, "VariantID");
                        ViewState.Add("VariantEdit", true);
                        ViewState.Add("VariantEditID", vID);

                    }
                }
                resetError("Variant Added.", false);
            }
            else
            {
                
                #region Notify customer when Item is back in Stock
                int c_SkinID = 3;
                int minimumInventory=5;
                Decimal price = 0;
                String ProductName=String.Empty;
                String pLink =String.Empty;
                // get the price of product
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select price from ProductVariant where VariantID=" + vID, dbconn))
                    {
                        if (rs.Read())
                        {
                            price = DB.RSFieldDecimal(rs, "Price");
                        }
                    }
                }
                //get the product name for redirection to that product.
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select Name from Product where ProductID=" + pID, dbconn))
                    {
                        if (rs.Read())
                        {
                            ProductName = DB.RSField(rs, "Name");
                        }
                    }
                }
                if (CurrentInventory >= minimumInventory)
                {

                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select Email from CustomerNotification where ProductID=" + pID + " and VarientID=" + vID + " and Issent=0 and InventoryID=-1", dbconn))
                        {
                            while (rs.Read())
                            {
                                String EMail = DB.RSField(rs, "Email");
                                String FromEMail = AppLogic.AppConfig("MailMe_OutOfStock");
                                String PackageName = AppLogic.AppConfig("XmlPackage.OutOfStock");
                                var currentURL = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");
                                pLink = String.Format(currentURL.ToString()+"p-" + pID + "-" + ProductName.Replace("/", "") + ".aspx");
                                AppLogic.SendOutOfStockMail(AppLogic.AppConfig("StoreName") + " " + AppLogic.GetString("OutOfStock.aspx.6", c_SkinID, ThisCustomer.LocaleSetting), AppLogic.RunXmlPackage(PackageName, null, ThisCustomer, c_SkinID, string.Empty, "productID=" + pID.ToString() + "&VarientID=" + vID.ToString() + "&ProductName=" + ProductName.ToString() + "&price=" + String.Format("{0:C}", price)+"&productLink="+pLink, false, false), true, FromEMail, FromEMail, EMail, EMail, "", AppLogic.MailServer());
                                Boolean SendWasOk = true;
                                if (SendWasOk)
                                {
                                    DB.ExecuteSQL("update CustomerNotification set IsSent=1 where Email='" + EMail + "' and ProductID=" + pID + " and VarientID=" + vID + " and InventoryID=-1");  
                                }
                            }

                        }
                    }
                }
                #endregion 
                // ok to update:
                sql.Append("update productvariant set ");
                sql.Append("ProductID=" + pID.ToString() + ",");
                sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXmlVariant("Name", txtName.Text, locale, vID)) + ",");
                sql.Append("ContentsBGColor=" + DB.SQuote(txtContentsBG.Text) + ",");
                sql.Append("PageBGColor=" + DB.SQuote(txtPageBG.Text) + ",");
                sql.Append("GraphicsColor=" + DB.SQuote(txtSkinColor.Text) + ",");
                sql.Append("ImageFilenameOverride=" + DB.SQuote(txtImageOverride.Text) + ",");
                sql.Append("SEAltText=" + DB.SQuote(txtSEAlt.Text) + ",");
                string temp = string.Empty;
                if (bUseHtmlEditor)
                {
                    temp = AppLogic.FormLocaleXmlVariant("Description", radDescription.Content, locale, vID);
                }
                else
                {
                    temp = AppLogic.FormLocaleXmlEditorVariant("Description", "Description", locale, vID);
                }
                if (temp.Length != 0)
                {
                    sql.Append("Description=" + DB.SQuote(temp) + ",");
                }
                else
                {
                    sql.Append("Description=NULL,");
                }
                if (txtRestrictedQuantities.Text.Length != 0)
                {
                    sql.Append("RestrictedQuantities=" + DB.SQuote(txtRestrictedQuantities.Text) + ",");
                }
                else
                {
                    sql.Append("RestrictedQuantities=NULL,");
                }
                temp =
                        AppLogic.FormLocaleXmlVariant("FroogleDescription", txtFroogle.Text, locale, vID);
                if (temp.Length != 0)
                {
                    sql.Append("FroogleDescription=" + DB.SQuote(temp) + ",");
                }
                else
                {
                    sql.Append("FroogleDescription=NULL,");
                }
                sql.Append("Price=" + Localization.DecimalStringForDB(Price) + ",");
                sql.Append("SalePrice=" + CommonLogic.IIF(SalePrice != Decimal.Zero, Localization.DecimalStringForDB(SalePrice), "NULL") + ",");
                sql.Append("MSRP=" + CommonLogic.IIF(MSRP != Decimal.Zero, Localization.DecimalStringForDB(MSRP), "NULL") + ",");
                sql.Append("Cost=" + CommonLogic.IIF(Cost != Decimal.Zero, Localization.DecimalStringForDB(Cost), "NULL") + ",");
                sql.Append("Points=" + Localization.IntStringForDB(Points) + ",");
                sql.Append("MinimumQuantity=" + CommonLogic.IIF(MinimumQuantity != 0, Localization.IntStringForDB(MinimumQuantity), "NULL") + ",");

                if (txtSKU.Text.Length != 0)
                    sql.Append("SKUSuffix=" + DB.SQuote(txtSKU.Text) + ",");
                else
                    sql.Append("SKUSuffix=NULL,");

                if (txtManufacturePartNumber.Text.Length != 0)
                    sql.Append("ManufacturerPartNumber=" + DB.SQuote(txtManufacturePartNumber.Text) + ",");
                else
                    sql.Append("ManufacturerPartNumber=NULL,");

                if (txtGTIN.Text.Length != 0)
                    sql.Append("GTIN=" + DB.SQuote(txtGTIN.Text) + ",");
                else
                    sql.Append("GTIN=NULL,");



                decimal Weight = Localization.ParseNativeDecimal(txtWeight.Text);
                sql.Append("Weight=" +
                            CommonLogic.IIF(Weight != 0.0M, Localization.DecimalStringForDB(Weight), "NULL") + ",");
                if (txtDimensions.Text.Length != 0)
                {
                    sql.Append("Dimensions=" + DB.SQuote(txtDimensions.Text) + ",");
                }
                else
                {
                    sql.Append("Dimensions=NULL,");
                }
                sql.Append("Inventory=" + CurrentInventory.ToString() + ",");
                sql.Append("SubscriptionInterval=" + Localization.ParseNativeInt(txtSubscriptionInterval.Text).ToString() + ",");
                sql.Append("SubscriptionIntervalType=" + rblSubscriptionIntervalType.SelectedValue.ToString() + ",");
                sql.Append("Published=" + rblPublished.SelectedValue + ",");
                sql.Append("CustomerEntersPrice=" + rblCustomerEntersPrice.SelectedValue + ",");
                sql.Append("CustomerEntersPricePrompt=" + DB.SQuote(AppLogic.FormLocaleXmlVariant("CustomerEntersPricePrompt", txtCustomerEntersPricePrompt.Text, locale, vID)) + ",");
                sql.Append("IsRecurring=" + rblRecurring.SelectedValue + ",");
                sql.Append("RecurringInterval=" + Localization.ParseNativeInt(txtRecurringIntervalMultiplier.Text).ToString() + ",");
                sql.Append("RecurringIntervalType=" + ddRecurringInterval.SelectedValue + ",");
                sql.Append("Colors=" + DB.SQuote(AppLogic.FormLocaleXmlVariant("Colors",
                    XmlCommon.GetLocaleEntry(VariantColors, locale, false),
                    locale, vID).Replace(", ", ",").Replace(" ,", ",").Replace("'", "").Trim()) + ",");
                sql.Append("ColorSKUModifiers=" + DB.SQuote(XmlCommon.GetLocaleEntry(txtColorSKUModifiers.Text, locale, false).Replace(", ", ",").Replace(" ,", ",").Trim()) + ",");
                sql.Append("Sizes=" + DB.SQuote(AppLogic.FormLocaleXmlVariant("Sizes",
                    XmlCommon.GetLocaleEntry(VariantSizes, locale, false),
                    locale, vID).Replace(", ", ",").Replace(" ,", ",").Replace("'", "").Trim()) + ",");
                sql.Append("SizeSKUModifiers=" + DB.SQuote(XmlCommon.GetLocaleEntry(txtSizeSKUModifiers.Text, locale, false).Replace(", ", ",").Replace(" ,", ",").Trim()) + ",");
                sql.Append("IsTaxable=" + rblTaxable.SelectedValue + ",");
                sql.Append("IsShipSeparately=" + rblShipSeparately.SelectedValue + ",");
                sql.Append("IsDownload=" + rblDownload.SelectedValue + ",");
                sql.Append("FreeShipping=" + rblFreeShipping.SelectedValue + ",");
                String DLoc = txtDownloadLocation.Text;
                if (DLoc.StartsWith("/"))
                {
                    DLoc = DLoc.Substring(1, DLoc.Length - 1); // remove leading / char!
                }
                sql.Append("DownloadLocation=" + DB.SQuote(DLoc) + ",");

                int validDays = 0;
                sql.Append(string.Format("DownloadValidDays={0},", (int.TryParse(txtValidForDays.Text, out validDays) ? validDays.ToString() : "NULL")));

                sql.Append("Condition=" + rblCondition.SelectedValue);
                sql.Append(" where VariantID=" + vID.ToString());
                try
                {
                    DB.ExecuteSQL(sql.ToString());
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Error in EditVariant(.RunSql), Msg=[" + CommonLogic.GetExceptionDetail(ex, String.Empty) + "], Sql=[" + sql.ToString() + "]");
                }

                ViewState.Add("VariantEdit", true);
                ViewState.Add("VariantEditID", vID);

                resetError("Variant Updated.", false);
            }

            // handle shipping costs uploaded (if any):
            if (ShipCalcID == Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts)
            {
                DB.ExecuteSQL("delete from ShippingByProduct where VariantID=" + vID.ToString());

                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader rs3 = DB.GetRS("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 order by DisplayOrder", conn))
                    {
                        while (rs3.Read())
                        {
                            String FldName = "ShippingCost_" + DB.RSFieldInt(rs3, "ShippingMethodID");
                            decimal ShippingCost = CommonLogic.FormUSDecimal(FldName);
                            DB.ExecuteSQL("insert ShippingByProduct(VariantID,ShippingMethodID,ShippingCost) values(" + vID.ToString() + "," + DB.RSFieldInt(rs3, "ShippingMethodID").ToString() + "," + Localization.CurrencyStringForDBWithoutExchangeRate(ShippingCost) + ")");
                        }
                    }
                }
            }

            //Upload Images
            HandleImageSubmits();

            LoadContent();
            return true;
        }

        public void HandleImageSubmits()
        {
            // handle image uploaded:
            String FN = CommonLogic.FormCanBeDangerousContent("ImageFilenameOverride").Trim();
            if (FN.Length == 0)
            {
                FN = vID.ToString();
            }

            String Image1 = String.Empty;
            String TempImage1 = String.Empty;
            HttpPostedFile Image1File = fuIcon.PostedFile;
            if (Image1File != null &&
                 Image1File.ContentLength != 0)
            {
                // delete any current image file first
                try
                {
                    if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                         FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) ||
                         FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                    {
                        File.Delete(AppLogic.GetImagePath("Product", "icon", true) + FN);
                    }
                    else
                    {
                        File.Delete(AppLogic.GetImagePath("Variant", "icon", true) + FN + ".jpg");
                        File.Delete(AppLogic.GetImagePath("Variant", "icon", true) + FN + ".gif");
                        File.Delete(AppLogic.GetImagePath("Variant", "icon", true) + FN + ".png");
                    }
                }
                catch { }

                switch (Image1File.ContentType)
                {
                    case "image/gif":

                        TempImage1 = AppLogic.GetImagePath("Variant", "icon", true) + "tmp_" + FN + ".gif";
                        Image1 = AppLogic.GetImagePath("Variant", "icon", true) + FN + ".gif";
                        Image1File.SaveAs(TempImage1);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage1, Image1, "icon", "image/gif");

                        break;
                    case "image/x-png":
                    case "image/png":

                        TempImage1 = AppLogic.GetImagePath("Variant", "icon", true) + "tmp_" + FN + ".png";
                        Image1 = AppLogic.GetImagePath("Variant", "icon", true) + FN + ".png";
                        Image1File.SaveAs(TempImage1);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage1, Image1, "icon", "image/png");

                        break;
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":

                        TempImage1 = AppLogic.GetImagePath("Variant", "icon", true) + "tmp_" + FN + ".jpg";
                        Image1 = AppLogic.GetImagePath("Variant", "icon", true) + FN + ".jpg";
                        Image1File.SaveAs(TempImage1);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage1, Image1, "icon", "image/jpeg");

                        break;
                }
                AppLogic.DisposeOfTempImage(TempImage1);
            }

            String Image2 = String.Empty;
            String TempImage2 = String.Empty;
            HttpPostedFile Image2File = fuMedium.PostedFile;
            if (Image2File != null &&
                 Image2File.ContentLength != 0)
            {
                // delete any current image file first
                try
                {
                    File.Delete(AppLogic.GetImagePath("Variant", "medium", true) + FN + ".jpg");
                    File.Delete(AppLogic.GetImagePath("Variant", "medium", true) + FN + ".gif");
                    File.Delete(AppLogic.GetImagePath("Variant", "medium", true) + FN + ".png");
                }
                catch { }

                switch (Image2File.ContentType)
                {
                    case "image/gif":

                        TempImage2 = AppLogic.GetImagePath("Variant", "medium", true) + "tmp_" + FN + ".gif";
                        Image2 = AppLogic.GetImagePath("Variant", "medium", true) + FN + ".gif";
                        Image2File.SaveAs(TempImage2);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage2, Image2, "medium", "image/gif");

                        break;
                    case "image/x-png":
                    case "image/png":

                        TempImage2 = AppLogic.GetImagePath("Variant", "medium", true) + "tmp_" + FN + ".png";
                        Image2 = AppLogic.GetImagePath("Variant", "medium", true) + FN + ".png";
                        Image2File.SaveAs(TempImage2);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage2, Image2, "medium", "image/png");

                        break;
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":

                        TempImage2 = AppLogic.GetImagePath("Variant", "medium", true) + "tmp_" + FN + ".jpg";
                        Image2 = AppLogic.GetImagePath("Variant", "medium", true) + FN + ".jpg";
                        Image2File.SaveAs(TempImage2);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage2, Image2, "medium", "image/jpeg");

                        break;
                }
                AppLogic.DisposeOfTempImage(TempImage2);
            }

            String Image3 = String.Empty;
            String TempImage3 = String.Empty;
            HttpPostedFile Image3File = fuLarge.PostedFile;
            if (Image3File != null &&
                 Image3File.ContentLength != 0)
            {
                // delete any current image file first
                try
                {
                    File.Delete(AppLogic.GetImagePath("Variant", "large", true) + vID.ToString() + ".jpg");
                    File.Delete(AppLogic.GetImagePath("Variant", "large", true) + vID.ToString() + ".gif");
                    File.Delete(AppLogic.GetImagePath("Variant", "large", true) + vID.ToString() + ".png");
                }
                catch { }

                switch (Image3File.ContentType)
                {
                    case "image/gif":

                        TempImage3 = AppLogic.GetImagePath("Variant", "large", true) + "tmp_" + FN + ".gif";
                        Image3 = AppLogic.GetImagePath("Variant", "large", true) + FN + ".gif";
                        Image3File.SaveAs(TempImage3);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage3, Image3, "large", "image/gif");
                        AppLogic.CreateOthersFromLarge("Variant", TempImage3, FN, "image/gif");

                        break;
                    case "image/x-png":
                    case "image/png":

                        TempImage3 = AppLogic.GetImagePath("Variant", "large", true) + "tmp_" + FN + ".png";
                        Image3 = AppLogic.GetImagePath("Variant", "large", true) + FN + ".png";
                        Image3File.SaveAs(TempImage3);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage3, Image3, "large", "image/png");
                        AppLogic.CreateOthersFromLarge("Variant", TempImage3, FN, "image/png");

                        break;
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":

                        TempImage3 = AppLogic.GetImagePath("Variant", "large", true) + "tmp_" + FN + ".jpg";
                        Image3 = AppLogic.GetImagePath("Variant", "large", true) + FN + ".jpg";
                        Image3File.SaveAs(TempImage3);
                        AppLogic.ResizeEntityOrObject("Variant", TempImage3, Image3, "large", "image/jpeg");
                        AppLogic.CreateOthersFromLarge("Variant", TempImage3, FN, "image/jpeg");

                        break;
                }
            }
        }

        protected void btnDeleteVariant_Click(object sender, EventArgs e)
        {
            string sql = "if (select count(*) from dbo.ProductVariant where ProductID = " + pID.ToString() +
                         " and VariantID <> " + vID.ToString() + " and deleted = 0 ) = 0 \n";
            sql += "select 'This is the only Variant for this product and cannot be deleted' \n";
            sql += "else begin \n";
            sql +=
                    "update dbo.ProductVariant set Deleted = case deleted when 1 then 0 else 1 end, isdefault = 0 where VariantID = " +
                    vID.ToString() + "\n";
            sql += " if exists (select * from dbo.ProductVariant where ProductID = " + pID.ToString() +
                   " and published = 1 and deleted = 0 and isdefault = 1 ) declare @t tinyint \n";
            sql +=
                    " else update dbo.ProductVariant set isdefault = 1 where VariantID = (select top 1 VariantID from dbo.ProductVariant where ProductID = " +
                    pID.ToString() + " and published = 1 and deleted = 0)\n";
            sql += "select '' \n";
            sql += "end";


            string err = string.Empty;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader dr = DB.GetRS(sql, conn))
                {
                    dr.Read();
                    err = dr.GetString(0);
                }
            }

            lblerr.Text = err;
            LoadContent();
        }

        #region "field Properties"

        private System.Text.RegularExpressions.Regex rxForbiddenCharacters =
            new System.Text.RegularExpressions.Regex("[&#<>#@]");

        /// <summary>
        /// txtColor field with prohibited characters removed
        /// </summary>
        private string VariantColors
        {
            get
            {
                return rxForbiddenCharacters.Replace(txtColors.Text, " ");
            }
            set
            {
                txtColors.Text = rxForbiddenCharacters.Replace(value, " ");
            }
        }
        /// <summary>
        /// txtSizes field with prohibited characters removed
        /// </summary>
        private string VariantSizes
        {
            get
            {
                return rxForbiddenCharacters.Replace(txtSizes.Text, " ");
            }
            set
            {
                txtSizes.Text = rxForbiddenCharacters.Replace(value, " ");
            }
        }

        #endregion
    }
}
