// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;


namespace AspDotNetStorefrontCore
{
    public class PayPalAd
    {
		public enum TargetPage
		{
			Home,
			Cart,
			Product,
			Entity
		}

		public bool Show { get; private set; }
		public string ImageDimensions { get; private set; }
		public string ImageScript { get; private set; }

		public PayPalAd(TargetPage placement)
		{
			string dimensionsConfigName = string.Format("PayPal.Ads.{0}PageDimensions", placement);
			string displayConfigName = string.Format("PayPal.Ads.ShowOn{0}Page", placement);

			string dimensionsConfig = AppLogic.AppConfig(dimensionsConfigName);
			string publisherId = AppLogic.AppConfig("PayPal.Ads.PublisherId");

			Show = AppLogic.AppConfigBool(displayConfigName) && AppLogic.AppConfigBool("PayPal.Ads.TermsAndConditionsAgreement");
			ImageDimensions = String.IsNullOrEmpty(dimensionsConfig) ? string.Empty : dimensionsConfig;
			ImageScript = !String.IsNullOrEmpty(dimensionsConfig) && ImageDimensions.Length > 0 ? GetAdScript(ImageDimensions, publisherId, dimensionsConfig) : string.Empty;
		}

		private string GetAdScript(string dimensions, string publisherId, string dimensionsConfig)
		{
			if(String.IsNullOrEmpty(dimensionsConfig) || !dimensionsConfig.Contains(dimensions))
				throw new ArgumentException("Ad dimensions must be in the list of allowable dimensions.");

			StringBuilder bannerScript = new StringBuilder();

			bannerScript.Append("<div class=\"paypal-banner-wrap\" >");
			bannerScript.Append(string.Format("<script type=\"text/javascript\" data-pp-pubid=\"{0}\" data-pp-placementtype=\"{1}\" data-pp-channel=\"vortx\">", publisherId , dimensions));
			bannerScript.Append(" (function (d, t) {\"use strict\";");
			bannerScript.Append("var s = d.getElementsByTagName(t)[0], n = d.createElement(t);");
			bannerScript.Append("n.src = \"//paypal.adtag.where.com/merchant.js\";");
			bannerScript.Append("s.parentNode.insertBefore(n, s);");
			bannerScript.Append("}(document, \"script\"));");
			bannerScript.Append("</script>");
			bannerScript.Append("</div>");

			return bannerScript.ToString();
		}
    }

}
