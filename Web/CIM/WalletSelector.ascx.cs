// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using GatewayAuthorizeNet;
using AspDotNetStorefront;
using System.Web;

public partial class CIM_WalletSelector : System.Web.UI.UserControl
{
	public delegate void PaymentProfileSelectedHandler(object sender, long paymentProfileId);
	public event PaymentProfileSelectedHandler PaymentProfileSelected;

	public long SelectedPaymentProfileId
	{
		get
		{
			foreach(RepeaterItem item in ListViewCreditCards.Items)
			{
				var button = ((RadioButton)item.FindControl("ButtonBillCard"));

				if(button.Checked)
				{
					string profileId = button.Attributes["value"];
					return long.Parse(profileId);
				}
			}

			return 0;
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
        if (!Page.IsPostBack)
        {
            AspDotNetStorefrontGateways.Processors.AuthorizeNet authorizeNet = new AspDotNetStorefrontGateways.Processors.AuthorizeNet();
            if (authorizeNet.IsCimEnabled)
            {
                BindPage();
            }
        }
	}

	public void BindPage()
	{
        SkinBase page = HttpContext.Current.Handler as SkinBase;
		AspDotNetStorefrontCore.Customer adnsfCustomer;

        if (page != null)
            adnsfCustomer = page.ThisCustomer;
        else
            adnsfCustomer = AspDotNetStorefrontCore.Customer.Current;

		ListViewCreditCards.DataSource = DataUtility.GetPaymentProfiles(adnsfCustomer.CustomerID, adnsfCustomer.EMail);
		ListViewCreditCards.DataBind();

		PanelNoSavedCards.Visible = ListViewCreditCards.Items.Count <= 0;
	}


	protected void ListViewCreditCards_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{
		if(e.Item.ItemType != ListItemType.AlternatingItem && e.Item.ItemType != ListItemType.Item)
			return;

		var radioButton = (RadioButton)e.Item.FindControl("ButtonBillCard");
		PaymentProfileWrapper dataItem = (PaymentProfileWrapper)e.Item.DataItem;
		radioButton.Checked = false;
		radioButton.Attributes.Add("value", dataItem.ProfileId.ToString());

		if (dataItem.ProfileId.ToString() == AspDotNetStorefrontCore.Customer.Current.ThisCustomerSession["ActivePaymentProfileId"])
		    radioButton.Checked = true;
	}

	protected void BillThisCard_CheckChanged(object sender, EventArgs e)
	{
		RadioButton button = (RadioButton)sender;
		string profileId = button.Attributes["value"];

		AspDotNetStorefrontCore.Customer.Current.ThisCustomerSession["ActivePaymentProfileId"] = profileId;

		if (PaymentProfileSelected != null)
			PaymentProfileSelected(this, long.Parse((string)profileId));

		BindPage();
	}

	public void ClearSelection()
	{
		foreach(RepeaterItem item in ListViewCreditCards.Items)
		{
			((RadioButton)item.FindControl("ButtonBillCard")).Checked = false;
		}

		AspDotNetStorefrontCore.Customer.Current.ThisCustomerSession["ActivePaymentProfileId"] = String.Empty;
	}
}
