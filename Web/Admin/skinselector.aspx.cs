// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using System.Web;

namespace AspDotNetStorefrontAdmin
{
	public partial class skinselector : AdminPageBase
	{
		ISkinProvider SkinProvider = new SkinProvider();
		Store selectedStore;
		Skin selectedStoreSkin;

		protected void Page_Load(object sender, EventArgs e)
		{
			if(!IsPostBack)
			{
				selectedStore = Store.GetDefaultStore();
				BindStores();
			}
			else
			{
				int storeId;
				if(!int.TryParse(StoreSelector.SelectedValue, out storeId))
					return;

				selectedStore = Store.GetStoreById(storeId);
			}
			Message.Visible = false;
			SetSelectedSkin(selectedStore.SkinID);
			BindSkins();
		}

		void SetSelectedSkin(int skinId)
		{
			selectedStoreSkin = SkinProvider.GetSkinById(skinId);
			SelectedStoreSkinName.Value = selectedStoreSkin.Name;
		}

		void BindStores()
		{
			List<Store> storeList = Store.GetStoreList();
			StoreSelector.DataSource = storeList;
			StoreSelector.DataTextField = "Name";
			StoreSelector.DataValueField = "StoreID";
			StoreSelector.DataBind();

			StoreSelector.SelectedValue = selectedStore.StoreID.ToString();
		}

		private void BindSkins()
		{
			var allSkins = SkinProvider.GetSkins();

			//exclude mobile skins from the options
			var skins = allSkins.Where(s => !s.IsMobile);

			SkinNavigationList.DataSource = skins;
			SkinNavigationList.DataBind();

			SkinInfo.DataSource = skins;
			SkinInfo.DataBind();
		}

		protected void SkinNavigation_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if(e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
				return;

			var skin = e.Item.DataItem as Skin;
			if(skin == null)
				return;

			HtmlImage skinImage = e.Item.FindControl("SkinImageIcon") as HtmlImage;
			if(skinImage == null)
				return;

			if(String.IsNullOrEmpty(skin.PreviewUrl))
				skinImage.Visible = false;

			Literal skinDisplayName = e.Item.FindControl("DisplayName") as Literal;
			if(skinDisplayName == null)
				return;

			if(String.IsNullOrEmpty(skin.DisplayName))
			{
				skinDisplayName.Text = skin.Name;
			}

			Label CurrentSkinIndicator = e.Item.FindControl("currentSkinIndicator") as Label;
			if(CurrentSkinIndicator == null)
				return;

			CurrentSkinIndicator.ToolTip = String.Format(AppLogic.GetString("admin.SkinManagement.SkinAppliedTitle", Localization.GetDefaultLocale()), selectedStore.Name);
			CurrentSkinIndicator.Visible = selectedStoreSkin.Name == skin.Name;

		}

		protected void SetSkin_Click(object sender, CommandEventArgs e)
		{
			var skinNameToSet = e.CommandArgument as String;
			if(String.IsNullOrEmpty(skinNameToSet))
				return;

			//clear the admin's skinid of of the profile table that would override the default and confuse the admin user
			HttpContext.Current.Profile.SetPropertyValue("SkinID", "");

			var skin = SkinProvider.GetSkinByName(skinNameToSet);
			selectedStore.SkinID = skin.Id;
			selectedStore.Save();
			SetSelectedSkin(skin.Id);
			BindSkins();
			Message.Visible = true;
		}

		protected void SkinInfo_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if(e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
				return;

			var skin = e.Item.DataItem as Skin;
			if(skin == null)
				return;

			HtmlImage skinImage = e.Item.FindControl("SkinImage") as HtmlImage;
			if(skinImage == null)
				return;

			Panel noPreview = e.Item.FindControl("NoPreviewAvailable") as Panel;
			if(noPreview == null)
				return;

			if(String.IsNullOrEmpty(skin.PreviewUrl))
			{
				skinImage.Visible = false;
				noPreview.Visible = true;
			}

			Literal skinDisplayName = e.Item.FindControl("DisplayName") as Literal;
			if(skinDisplayName == null)
				return;

			if(String.IsNullOrEmpty(skin.DisplayName))
			{
				skinDisplayName.Text = skin.Name;
			}

			HtmlGenericControl skinDescription = e.Item.FindControl("SkinDescriptionContainer") as HtmlGenericControl;
			if(skinDescription == null)
				return;

			skinDescription.Visible = String.IsNullOrEmpty(skin.Description) ? false : true;

			HyperLink previewLink = e.Item.FindControl("PreviewSkin") as HyperLink;
			if(previewLink == null)
				return;

            string previewUrl = BuildPreviewLink(skin.Id);

            if (!string.IsNullOrEmpty(previewUrl))
                previewLink.NavigateUrl = previewUrl;
            else
                DisablePreviewButton(previewLink);

			Button setSkinButton = e.Item.FindControl("SetSkin") as Button;
			if(setSkinButton == null)
				return;

			if(selectedStoreSkin.Name == skin.Name)
				setSkinButton.Enabled = false;
			
		}

		protected void StoreSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectedStoreId = 1;
			if(!int.TryParse(StoreSelector.SelectedValue, out selectedStoreId))
				return;

			selectedStore = Store.GetStoreById(selectedStoreId);
			selectedStoreSkin = SkinProvider.GetSkinById(selectedStore.SkinID);
			SelectedStoreSkinName.Value = selectedStoreSkin.Name;
		}

        protected string BuildPreviewLink(int skinId)
        {
            int storeId;
            if (!int.TryParse(StoreSelector.SelectedValue, out storeId))
                return string.Empty;
            
            //If on the current store, we can figure out the URL regardless of virtual directories.  If previewing for another store, this will only work
            //without a virtual directory.
            if (storeId == AppLogic.StoreID())
            {
                return AppLogic.ResolveUrl(string.Format("~/default.aspx?previewskinid={0}", skinId));
            }
            else
            {
                //Figure out what 'type' of store we're on right now
                StoreUrlType currentType = Store.DetermineCurrentUrlType();

                //Get the matching type URL for the chosen store
                string chosenStoreUrl = Store.GetStoreUrlByType(currentType, storeId);

                if (!String.IsNullOrEmpty(chosenStoreUrl))
                {
                    string previewUrl = "http://{0}/default.aspx?previewskinid={1}";
                    return string.Format(previewUrl, chosenStoreUrl, skinId);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private void DisablePreviewButton(HyperLink previewLink)
        {
            previewLink.Attributes.Add("disabled", "disabled");
            previewLink.CssClass = "previewLinkDisabled";
            previewLink.ToolTip = AppLogic.GetString("Admin.SkinSelector.Previewunavailable", ThisCustomer.LocaleSetting);
        }
}
}
