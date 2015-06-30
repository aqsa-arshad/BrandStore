// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for inventory
    /// </summary>
    public partial class _RTShippingLocalPickup : AdminPageBase
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            Response.CacheControl = "private";
            Response.Expires = -1;
            Response.AddHeader("pragma", "no-cache");

            Page.Form.DefaultButton = btnUpdate.UniqueID;

            if (!Page.IsPostBack)
            {
                LoadLocaleContent();
                InitializePageContent();
            }

            InitializePanels();
        }

        /// <summary>
        /// Update the appconfig and stringresource 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateAppConfig();
            UpdateRestrictions();
            InitializePageContent();
            resetError("In-Store Pickup Updated", false);
        }
        /// <summary>
        /// Change the appconfig value
        /// </summary>
        /// <param name="Appconfig"></param>
        private string GetAppconfigValue(string Appconfig)
        {
            AppConfig config = AppLogic.GetAppConfigRouted(Appconfig);
            if (config != null)
            {
                return config.Description.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Change the locale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlLocale_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitializePageContent();
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
        private void InitializePageContent()
        {
            string locale = Localization.GetDefaultLocale();
            // for multilocale
            if (ddlLocale.SelectedValue != null)
            {
                locale = ddlLocale.SelectedValue;
            }

            AppConfig allowLocalPickup = AppLogic.GetAppConfigRouted("RTShipping.AllowLocalPickup");
            if (allowLocalPickup != null)
                imgAllowLocalPickup.ToolTip = allowLocalPickup.Description;

            AppConfig localPickupCost = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupCost");
            if (localPickupCost != null)
                imgRTShippingLocalPickupHandlingFee.ToolTip = localPickupCost.Description;

            AppConfig localPickupRestrictionType = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionType");
            if (localPickupRestrictionType != null)
                imgRestrictionType.ToolTip = localPickupRestrictionType.Description;

            AppConfig localPickupRestrictionStates = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionStates");
            if (localPickupRestrictionStates != null)
                imgRestrictionAllowedStates.ToolTip = localPickupRestrictionStates.Description;

            AppConfig localPickupRestrictionZips = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionZips");
            if (localPickupRestrictionZips != null)
                imgRestrictionAllowedZips.ToolTip = localPickupRestrictionZips.Description;

            AppConfig localPickupRestrictionZones = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionZones");
            if (localPickupRestrictionZones != null)
                imgRestrictionAllowedZones.ToolTip = localPickupRestrictionZones.Description;

            //Set initial value, this is from stringresource
            lblrtshippinglocalpickupheader.Text = AppLogic.GetString("RTShipping.LocalPickup.Header", SkinID, locale);
            lblrtshippinglocalpickupbreadcrumb.Text = AppLogic.GetString("RTShipping.LocalPickup.Breadcrumb", SkinID, locale);

            cbxAllowLocalPickup.Text = AppLogic.GetString("RTShipping.CheckBox.AllowLocalPickup", SkinID, locale);
            
            lblrestrictiontype.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionTypeLabel", SkinID, locale);
            
            liUnrestricted.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionType.Unrestricted", SkinID, locale);
            liState.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionType.State", SkinID, locale);
            liZip.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionType.Zip", SkinID, locale);
            liZone.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionType.Zone", SkinID, locale);

            lblRTShippingLocalPickupHandlingFee.Text = AppLogic.GetString("RTShipping.LocalPickup.HandlingFeeLabel", SkinID, locale);
            txtRTShippingLocalPickupHandlingFee.Text = AppLogic.AppConfig("RTShipping.LocalPickupCost");

            btnUpdate.Text = AppLogic.GetString("RTShipping.LocalPickup.Button.Update", SkinID, locale);
            lblTitle.Text = AppLogic.GetString("RTShipping.LocalPickup.TitleMessage", SkinID, locale);
            lblRestrictionsTitle.Text = AppLogic.GetString("RTShipping.LocalPickup.RestrictionsMessage", SkinID, locale);
            lblRestrictionAllowedZones.Text = AppLogic.GetString("RTShipping.LocalPickup.Restriction.Zones", SkinID, LocaleSetting);
            lblRestrictionAllowedZips.Text = AppLogic.GetString("RTShipping.LocalPickup.Restriction.Zips", SkinID, LocaleSetting);
            lblRestrictionAllowedStates.Text = AppLogic.GetString("RTShipping.LocalPickup.Restriction.States", SkinID, LocaleSetting);

            InitializeSelectedValue();
        }

        /// <summary>
        /// Get the initial selected value
        /// </summary>
        private void InitializeSelectedValue()
        {
            if (AppLogic.AppConfigBool("RTShipping.AllowLocalPickup"))
            {
                cbxAllowLocalPickup.Checked = true;
            }

            String sRestrictionType = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionType");

            if (sRestrictionType.Equals("state", StringComparison.InvariantCultureIgnoreCase))
            {
                liState.Selected = true;
            }
            else if (sRestrictionType.Equals("zip", StringComparison.InvariantCultureIgnoreCase))
            {
                liZip.Selected = true;

                String allowedzips = AppLogic.AppConfig("RTShipping.LocalPickupRestrictionZips").Trim();

                txtRestrictionAllowedZips.Text = HttpUtility.HtmlEncode(allowedzips);
            }
            else if (sRestrictionType.Equals("zone", StringComparison.InvariantCultureIgnoreCase))
            {
                liZone.Selected = true;
            }
            else
            {
                liUnrestricted.Selected = true;
            }
        }


        /// <summary>
        /// Update appconfig
        /// </summary>
        private void UpdateAppConfig()
        {
            //Update the appconfigs through checkbox
            AppConfig config = AppLogic.GetAppConfigRouted("RTShipping.AllowLocalPickup");
            if (config != null)
            {
                if (cbxAllowLocalPickup.Checked)
                {
                    config.ConfigValue = "true";
                }
                else
                {
                    config.ConfigValue = "false";
                }
            }

            //Update the appconfig through text box
            config = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupCost");
            if (config != null)
            {
                if (txtRTShippingLocalPickupHandlingFee.Text.Trim().Length == 0)
                {
                    config.ConfigValue = "0.00";
                }
                else
                {
                    config.ConfigValue = txtRTShippingLocalPickupHandlingFee.Text;
                }
            }

            //Update the appconfig through radio button
            config = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionType");
            if (config != null)
            {
                if (liState.Selected)
                {
                    config.ConfigValue = "state";
                }
                else if (liZip.Selected)
                {
                    config.ConfigValue = "zip";
                }
                else if (liZone.Selected)
                {
                    config.ConfigValue = "zone";
                }
                else
                {
                    config.ConfigValue = "unrestricted";
                }
            }
        }

        /// <summary>
        /// Update the restrictions
        /// </summary>
        private void UpdateRestrictions()
        {
            // Unrestricted
            // Do nothing...restrictions are ignored

            // States
            if (liState.Selected)
            {
                String allowedstateids = String.Empty;
                foreach (Control ctrl in pnlStateSelect.Controls)
                {
                    string Type = ctrl.GetType().ToString();
                    if (Type == "System.Web.UI.WebControls.CheckBox")
                    {
                        CheckBox cb = (CheckBox)ctrl;
                        if (cb.Checked)
                        {
                            allowedstateids += cb.ID.ToString().Remove(0, 4) + ",";
                        }
                    }
                }

                AppConfig config = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionStates");
                if (config != null)
                {
                    config.ConfigValue = allowedstateids.TrimEnd(',').Trim();
                }
            }

            // Zones
            if (liZone.Selected)
            {
                var allowedZoneIds = new List<string>();
                String shippingZoneId = String.Empty;

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();

					using (IDataReader rs = DB.GetRS("select ShippingZoneID from ShippingZone with (NOLOCK)", dbconn))
					{
						while (rs.Read())
						{
                            shippingZoneId = DB.RSFieldInt(rs, "ShippingZoneID").ToString();
                            String cbxId = HttpContext.Current.Request.Form.AllKeys.FirstOrDefault(x => x.EndsWith(String.Format(CultureInfo.InvariantCulture,"ckb_{0}", shippingZoneId)));
                            if (CommonLogic.FormCanBeDangerousContent(cbxId).Equals("on", StringComparison.InvariantCultureIgnoreCase))
							{
                                allowedZoneIds.Add(shippingZoneId);
							}
						}
					}
				}

                AppConfig config = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionZones");
                if (config != null)
                {
                    config.ConfigValue = String.Join(",", allowedZoneIds.ToArray());
                }

            }

            // Zips
            if (liZip.Selected)
            {
                AppConfig config = AppLogic.GetAppConfigRouted("RTShipping.LocalPickupRestrictionZips");
                if (config != null)
                {
                    config.ConfigValue = HttpUtility.HtmlEncode(txtRestrictionAllowedZips.Text).Trim().TrimEnd(',');
                }
            }
        }

        /// <summary>
        /// Initialize Panels for restrictions based on restriction type setting
        /// </summary>
        private void InitializePanels()
        {
            // If unrestricted...no panels are visible
			pnlStateSelect.Visible = pnlZipSelect.Visible = pnlZoneSelect.Visible = false;
            // If state...get states from database
            if (liState.Selected)
            {
                pnlStateSelect.Visible = true;
                String[] allowedstateids = AppLogic.AppConfig("RTShipping.LocalpickupRestrictionStates").Trim().Split(',');
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select * from State with (NOLOCK)", dbconn))
                    {
                        while (rs.Read())
                        {
                            CheckBox ckb = new CheckBox();
                            ckb.Text = DB.RSField(rs, "Name") + "<br/>";
                            ckb.ID = "ckb_" + DB.RSFieldInt(rs, "StateID").ToString();

                            foreach (String allowedstateid in allowedstateids)
                            {
                                if (allowedstateid.Length > 0)
                                {                                    
                                    if (Int32.Parse(allowedstateid) == (DB.RSFieldInt(rs, "StateID")))
                                    {
                                        ckb.Checked = true;
                                    }
                                }
                            }
                            pnlStateSelect.Controls.Add(ckb);
                        }
                    }
                }
            }

            // If zone...get zones from database
            if (liZone.Selected)
            {
                pnlZoneSelect.Visible = true;

                int cntZones = DB.GetSqlN("select count(*) as N from ShippingZone with (NOLOCK) where deleted <> 1");
                String shippingZoneId = String.Empty;
                if (cntZones > 0)
                {
                    String[] allowedZoneIds = AppLogic.AppConfig("RTShipping.LocalpickupRestrictionZones").Trim().Split(',');

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select * from ShippingZone with (NOLOCK) where deleted <> 1", dbconn))
                        while (rs.Read())
                        {
                            shippingZoneId = DB.RSFieldInt(rs, "ShippingZoneID").ToString();

                            CheckBox ckb = new CheckBox();
                            ckb.Text = String.Format(CultureInfo.InvariantCulture, "{0}<br/>", DB.RSField(rs, "Name"));
                            ckb.ID = String.Format(CultureInfo.InvariantCulture, "ckb_{0}", shippingZoneId);
                            ckb.CssClass = String.Format(CultureInfo.InvariantCulture, "ckb_{0}", shippingZoneId);
                            ckb.Checked = allowedZoneIds.Any(s => s.Equals(shippingZoneId, StringComparison.Ordinal));

                            pnlZoneSelect.Controls.Add(ckb);
                        }
                    }
                }
            }

            // If zip...populate text box with comma separated zips
            if (liZip.Selected)
            {
                pnlZipSelect.Visible = true;
            }
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

            ltError.Text = "<font class=\"noticeMsg\">" + str + "</font>";
        }

    }
}
