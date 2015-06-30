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
	public partial class bmlsettings : AdminPageBase
	{

		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			Page.MaintainScrollPositionOnPostBack = true;
		}


		protected override void OnPreRender(EventArgs e)
		{
			CreatePreviewBanners();
			InitializeTermsAndConditions();
			base.OnPreRender(e);
		}

		protected void chkTermsAndConditions_CheckedChanged(object sender, EventArgs e)
		{
			AppLogic.SetAppConfig("PayPal.Ads.TermsAndConditionsAgreement", chkTermsAndConditions.Checked.ToString());
			InitializeTermsAndConditions();
		}

		private void InitializeTermsAndConditions()
		{
			bool agreedToTerms = AppLogic.AppConfigBool("PayPal.Ads.TermsAndConditionsAgreement", ThisCustomer.StoreID, true);
			bool enabledPayPal = AppLogic.AppConfig("PaymentMethods", ThisCustomer.StoreID, true).ContainsIgnoreCase("PayPal");

			pnlDisableConfiguration.Visible = !AllowConfiguration();
			pnlPayPalEnabled.Visible = enabledPayPal;
			pnlPayPalNotEnabled.Visible = !enabledPayPal;

			if(!Page.IsPostBack)
			{
				chkTermsAndConditions.Checked = agreedToTerms;
			}
			else
			{
				AtomCartPageDimensions.DataBind();
				AtomEntityPageDimensions.DataBind();
				AtomHomePageDimensions.DataBind();
				AtomProductPageDimensions.DataBind();
				AtomPublisherId.DataBind();
				AtomShowAdsOnCartPage.DataBind();
				AtomShowAdsOnEntityPage.DataBind();
				AtomShowAdsOnHomePage.DataBind();
				AtomShowAdsOnProductPage.DataBind();
			}
		}

		private void CreatePreviewBanners()
		{
			PayPalAd homePageAd = new PayPalAd(PayPalAd.TargetPage.Home);
			PayPalAd cartPageAd = new PayPalAd(PayPalAd.TargetPage.Cart);
			PayPalAd productPageAd = new PayPalAd(PayPalAd.TargetPage.Product);
			PayPalAd entityPageAd = new PayPalAd(PayPalAd.TargetPage.Entity);

			ltHomeScript.Text = homePageAd.Show ? string.Format("<div>Your Home Page Banner ({0}):</div>{1}", homePageAd.ImageDimensions, homePageAd.ImageScript) : string.Empty;
			ltCartScript.Text = cartPageAd.Show ? string.Format("<div>Your Shopping Cart Page Banner ({0}):</div>{1}", cartPageAd.ImageDimensions, cartPageAd.ImageScript) : string.Empty;
			ltProductScript.Text = productPageAd.Show ? string.Format("<div>Your Product Page Banner ({0}):</div>{1}", productPageAd.ImageDimensions, productPageAd.ImageScript) : string.Empty;
			ltEntityScript.Text = entityPageAd.Show ? string.Format("<div>Your Entity Page Banner ({0}):</div>{1}", entityPageAd.ImageDimensions, entityPageAd.ImageScript) : string.Empty;

			pnlHomeScript.Visible = AppLogic.AppConfigBool("PayPal.Ads.ShowOnHomePage");
			pnlCartScript.Visible = AppLogic.AppConfigBool("PayPal.Ads.ShowOnCartPage");
			pnlProductScript.Visible = AppLogic.AppConfigBool("PayPal.Ads.ShowOnProductPage");
			pnlEntityScript.Visible = AppLogic.AppConfigBool("PayPal.Ads.ShowOnEntityPage");
		}

		protected void btnSavePublisherId_Click(object sender, EventArgs e)
		{
			AtomPublisherId.Save();
		}
		protected void btnSaveConfiguration_Click(object sender, EventArgs e)
		{
			AtomPublisherId.Save();

			if(AllowConfiguration())
			{
				AtomCartPageDimensions.Save();
				AtomEntityPageDimensions.Save();
				AtomHomePageDimensions.Save();
				AtomProductPageDimensions.Save();
				AtomShowAdsOnCartPage.Save();
				AtomShowAdsOnEntityPage.Save();
				AtomShowAdsOnHomePage.Save();
				AtomShowAdsOnProductPage.Save();
			}
		}

		protected bool AllowConfiguration()
		{
			bool agreedToTerms = AppLogic.AppConfigBool("PayPal.Ads.TermsAndConditionsAgreement", ThisCustomer.StoreID, true);
			bool enteredPublisherId = !String.IsNullOrEmpty(AppLogic.AppConfig("PayPal.Ads.PublisherId", ThisCustomer.StoreID, true));
			bool enabledPayPal = AppLogic.AppConfig("PaymentMethods", ThisCustomer.StoreID, true).ContainsIgnoreCase("PayPal");

			return enabledPayPal && agreedToTerms && enteredPublisherId;
		}
	}
}
