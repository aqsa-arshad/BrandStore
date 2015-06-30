// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GatewayAuthorizeNet;
using AspDotNetStorefrontCore;

public partial class CIM_Wallet : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
		CreditCardEditor1.CardEditComplete += CreditCardEditor1_CardEditComplete;

		if (!Page.IsPostBack)
		{
			BindPage();
		}
    }

	public void BindPage()
	{
		PanelAddPaymentType.Visible = false;
		ButtonAddPaymentType.Visible = true;

		AspDotNetStorefrontCore.Customer adnsfCustomer = AspDotNetStorefrontCore.Customer.Current;

		ListViewCreditCards.DataSource = DataUtility.GetPaymentProfiles(adnsfCustomer.CustomerID, adnsfCustomer.EMail);
		ListViewCreditCards.DataBind();
	}

	protected void ListViewCreditCards_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
		{

		}
	}

	protected void ListViewCreditCards_ItemCommand(object source, RepeaterCommandEventArgs e)
	{
		if (e.CommandName == "Delete")
		{
			AspDotNetStorefrontCore.Customer adnsfCustomer = AspDotNetStorefrontCore.Customer.Current;

			Int64 profileId = DataUtility.GetProfileId(adnsfCustomer.CustomerID);
			Int64 paymentProfileId = long.Parse((string)e.CommandArgument);

			var profileMgr = new ProfileManager(adnsfCustomer.CustomerID, adnsfCustomer.EMail, profileId);
			profileMgr.DeletePaymentProfile(paymentProfileId);
			DataUtility.DeletePaymentProfile(adnsfCustomer.CustomerID, paymentProfileId);

			this.BindPage();
		}
		else if (e.CommandName == "Edit")
		{
			PanelAddPaymentType.Visible = true;
			ButtonAddPaymentType.Visible = false;
			CreditCardEditor1.BindPage(long.Parse((string)e.CommandArgument));
		}
	}

	protected void CreditCardEditor1_CardEditComplete(object sender, EventArgs e)
	{
		this.BindPage();
	}
	
	protected void ButtonAddPaymentType_Click(object sender, EventArgs e)
	{
		PanelAddPaymentType.Visible = true;
		ButtonAddPaymentType.Visible = false;
		CreditCardEditor1.BindPage(0);
	}
}
