// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using AspDotNetStorefrontCore;
using System.IO;

namespace AspDotNetStorefrontAdmin
{
    public partial class _InventoryControl : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Form.DefaultButton = btnUpdate.UniqueID;

            if (!Page.IsPostBack)
            {
                LoadLocaleContent();
                LoadPageContent();
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string locale = Localization.GetDefaultLocale();
            if (ddlLocale.SelectedValue != null)
                locale = ddlLocale.SelectedValue;

            UpdateStringResources(locale);
            UpdateAppConfigs();
            LoadPageContent();
        }

        /// <summary>
        /// Change the locale
        /// </summary>
        protected void ddlLocale_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPageContent();
        }

        /// <summary>
        /// Get all the localesetting
        /// </summary>
        private void LoadLocaleContent()
        {
            if (!Page.IsPostBack)
            {
                ddlLocale.Items.Clear();
                //Populate the dropdowlist for localesetting
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader localeReader = DB.GetRS("SELECT * FROM LocaleSetting  with (NOLOCK)  ORDER BY DisplayOrder,Name", conn))
                    {
                        while (localeReader.Read())
                        {
                            ddlLocale.Items.Add(DB.RSField(localeReader, "Name"));
                        }
                    }
                }
                if (ddlLocale.Items.Count < 2)//If only have 1 locale dont show the dropdown
                {
                    ddlLocale.Visible = false;
                    tdLocale.Visible = false;
                }
                else
                {
                    ddlLocale.SelectedValue = LocaleSetting;
                }
            }
        }

        /// <summary>
        /// Set the initial value
        /// </summary>
        private void LoadPageContent()
        {
            string locale = ddlLocale.SelectedValue ?? Localization.GetDefaultLocale();

            LoadAppConfigs();
            LoadStringResources(locale);
        }

        private void LoadAppConfigs()
        {
            txtHideProductsWithLessThanThisInventoryLevel.Text = AppLogic.AppConfig("HideProductsWithLessThanThisInventoryLevel", 0, false);
            txtOutOfStockThreshold.Text = AppLogic.AppConfig("OutOfStockThreshold", 0, false);

            chkDisplayOutOfStockMessage.Checked = AppLogic.AppConfigBool("DisplayOutOfStockProducts", 0, true);
            chkLimitCartToQuantityOnHand.Checked = AppLogic.AppConfigBool("Inventory.LimitCartToQuantityOnHand", 0, true);
            chkShowInventoryTable.Checked = AppLogic.AppConfigBool("ShowInventoryTable", 0, true);
            chkShowOutOfStockMessageOnProductPages.Checked = AppLogic.AppConfigBool("DisplayOutOfStockOnProductPages", 0, true);
            chkShowOutOfStockMessageOnEntityPages.Checked = AppLogic.AppConfigBool("DisplayOutOfStockOnEntityPages", 0, true);

            // Kit Inventory Control
            chkEnableKitItemStockHints.Checked = AppLogic.AppConfigBool("KitInventory.ShowStockHint", 0, true);

            rbDisableOutOfStockKitItem.Checked = AppLogic.AppConfigBool("KitInventory.DisableItemSelection", 0, true);
            rbHideOutOfStockKitItems.Checked = AppLogic.AppConfigBool("KitInventory.HideOutOfStock", 0, true);
            rbNoInventoryControl.Checked = !rbDisableOutOfStockKitItem.Checked && !rbHideOutOfStockKitItems.Checked;

            chkProductPageOutOfStockRedirect.Checked = AppLogic.AppConfigBool("ProductPageOutOfStockRedirect", 0, true);
        }

        private void UpdateAppConfigs()
        {
            AppConfigManager.SetAppConfigDBAndCache(0, "Inventory.LimitCartToQuantityOnHand", chkLimitCartToQuantityOnHand.Checked.ToString());
            AppConfigManager.SetAppConfigDBAndCache(0, "ShowInventoryTable", chkShowInventoryTable.Checked.ToString());

            AppConfigManager.SetAppConfigDBAndCache(0, "HideProductsWithLessThanThisInventoryLevel", txtHideProductsWithLessThanThisInventoryLevel.Text);

            AppConfigManager.SetAppConfigDBAndCache(0, "DisplayOutOfStockProducts", chkDisplayOutOfStockMessage.Checked.ToString());
            AppConfigManager.SetAppConfigDBAndCache(0, "OutOfStockThreshold", txtOutOfStockThreshold.Text);
            AppConfigManager.SetAppConfigDBAndCache(0, "DisplayOutOfStockOnProductPages", chkShowOutOfStockMessageOnProductPages.Checked.ToString());
            AppConfigManager.SetAppConfigDBAndCache(0, "DisplayOutOfStockOnEntityPages", chkShowOutOfStockMessageOnEntityPages.Checked.ToString());

            AppConfigManager.SetAppConfigDBAndCache(0, "KitInventory.ShowStockHint", chkEnableKitItemStockHints.Checked.ToString());

            AppConfigManager.SetAppConfigDBAndCache(0, "KitInventory.DisableItemSelection", rbDisableOutOfStockKitItem.Checked.ToString());
            AppConfigManager.SetAppConfigDBAndCache(0, "KitInventory.HideOutOfStock", rbHideOutOfStockKitItems.Checked.ToString());

            AppConfigManager.SetAppConfigDBAndCache(0, "ProductPageOutOfStockRedirect", chkProductPageOutOfStockRedirect.Checked.ToString().ToLower());

        }

        private void LoadStringResources(string locale)
        {
            //Set initial value, this is from stringresource 
            txtProductOutOfStockMessage.Text = AppLogic.GetString("OutOfStock.DisplayOutOfStockOnProductPage", SkinID, locale);
            txtEntityOutOfStockMessage.Text = AppLogic.GetString("OutOfStock.DisplayOutOfStockOnEntityPage", SkinID, locale);
            txtProductInStockMessage.Text = AppLogic.GetString("OutOfStock.DisplayInStockOnProductPage", SkinID, locale);
            txtEntityInStockMessage.Text = AppLogic.GetString("OutOfStock.DisplayInStockOnEntityPage", SkinID, locale);
        }

        private void UpdateStringResources(string locale)
        {
            var displayOutOfStockOnProduct = StringResourceManager.GetStringResource(AppLogic.StoreID(), locale, "OutofStock.DisplayOutOfStockOnProductPage");
            if (displayOutOfStockOnProduct != null)
                displayOutOfStockOnProduct.Update("OutofStock.DisplayOutOfStockOnProductPage", locale, txtProductOutOfStockMessage.Text);

            var displayOutOfStockOnEntity = StringResourceManager.GetStringResource(AppLogic.StoreID(), locale, "OutofStock.DisplayOutOfStockOnEntityPage");
            if (displayOutOfStockOnEntity != null)
                displayOutOfStockOnEntity.Update("OutofStock.DisplayOutOfStockOnEntityPage", locale, txtEntityOutOfStockMessage.Text);

            var displayInStockOnProduct = StringResourceManager.GetStringResource(AppLogic.StoreID(), locale, "OutofStock.DisplayInStockOnProductPage");
            if (displayInStockOnProduct != null)
                displayInStockOnProduct.Update("OutofStock.DisplayInStockOnProductPage", locale, txtProductInStockMessage.Text);

            var displayInStockOnEntity = StringResourceManager.GetStringResource(AppLogic.StoreID(), locale, "OutofStock.DisplayInStockOnEntityPage");
            if (displayInStockOnEntity != null)
                displayInStockOnEntity.Update("OutofStock.DisplayInStockOnEntityPage", locale, txtEntityInStockMessage.Text);
        }

        protected void btnRemoveSpaces_Click(object sender, EventArgs e)
        {
            UpdateDB();
            UpdateFileName(new DirectoryInfo(Server.MapPath("~/images/product/icon/")));
            UpdateFileName(new DirectoryInfo(Server.MapPath("~/images/product/medium/")));
            UpdateFileName(new DirectoryInfo(Server.MapPath("~/images/product/large/")));
        }

        private void UpdateDB()
        {
            using (var conn = DB.dbConn())
            {
                conn.Open();
                var query = "UPDATE Product SET ImageFilenameOverride = CAST(REPLACE(LTRIM(RTRIM(CAST(ImageFilenameOverride as NVarchar(4000)))), ' ','') AS NText)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateFileName(DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles())
            {
                try
                {

                    if (file.Name.Contains(" "))
                        File.Move(file.FullName, file.FullName.Replace(" ", ""));

                }
                catch (Exception ex)
                {
                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }
            }
        }
    }
}
