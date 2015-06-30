// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class shipwire : AdminPageBase
    {
        protected override void OnLoad(EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            // populate Configuration Section
            if (!IsPostBack)
                LoadData();

            if (txtUsername.Text.Length > 0)
            {
                SalesRow.Visible = false;
                TrackingRow.Visible = true;
                InventoryRow.Visible = true;
            }
            else
            {
                SalesRow.Visible = true;
                TrackingRow.Visible = false;
                InventoryRow.Visible = false;
            }
            base.OnLoad(e);
        }

        protected void LoadData()
        {
            // populate Configuration Section
            txtUsername.Text = AppLogic.AppConfig("Shipwire.Username");
            txtPassword.Text = string.Empty.PadLeft(AppLogic.AppConfig("Shipwire.Password").Length, '*');
        }

        protected void UpdateTracking_Click(object sender, EventArgs e)
        {
            UpdateTrackingStatus.Text = AspDotNetStorefrontCore.Shipwire.UpdateTracking();
            UpdateTrackingStatus.Visible = true;
        }
        protected void UpdateInventory_Click(object sender, EventArgs e)
        {
            UpdateInventoryStatus.Text = AspDotNetStorefrontCore.Shipwire.UpdateInventory();
            UpdateInventoryStatus.Visible = true;
        }
        protected void ConfigureShipwire_Click(object sender, EventArgs e)
        {
            AppLogic.SetAppConfig("Shipwire.Username", txtUsername.Text);
            if (!txtPassword.Text.StartsWith("*"))
                AppLogic.SetAppConfig("Shipwire.Password", txtPassword.Text);

            ConfigureShipwireStatus.Text = "Values stored in AppConfig table.";
            ConfigureShipwireStatus.Visible = true;
            LoadData();
        }
    }
}
