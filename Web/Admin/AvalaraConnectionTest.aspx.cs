// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using AspDotNetStorefrontCore;

public partial class Admin_AvalaraConnectionTest : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		try
		{
			bool success = false;
			string reason = String.Empty;

			if(!AppLogic.AppConfigBool("AvalaraTax.Enabled"))
				ResultLabel.Text = "AvaTax is not enabled. Enable AvaTax by setting the AvalaraTax.Enabled AppConfig to True.";
			else
			{
				AvaTax avaTax = new AvaTax();
				success = avaTax.TestAddin(out reason);

				if(success)
					ResultLabel.Text = "Successfully connected to AvaTax.";
				else if(String.IsNullOrEmpty(reason))
					ResultLabel.Text = "AvaTax connection was not successful. Please review your configuration.";
				else
					ResultLabel.Text = String.Format("AvaTax connection was not successful. AvaTax returned the following message: {0}", reason);
			}
		}
		catch(Exception exception)
		{
			ResultLabel.Text = "Connection test failed due to an exception:<br />" + exception.ToString().Replace("\n", "<br />");
		}
    }
}
