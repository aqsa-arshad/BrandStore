// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using GatewayAuthorizeNet;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using System.Web;
using AspDotNetStorefront;

public partial class CIM_CreditCardEditor : System.Web.UI.UserControl
{
	public event EventHandler CardEditComplete;

	public long PaymentProfileId
	{
		get { return ((long?)Session["PaymentProfileId"] ?? 0); }
		set { Session["PaymentProfileId"] = value; }
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (!Page.IsPostBack)
			BindPage(0);
	}

	protected void ButtonSave_Click(object sender, EventArgs e)
	{
		if(!Page.IsValid)
			return;

		int selectedAddressId = int.Parse(BillingAddresses.SelectedValue);
        SkinBase page = HttpContext.Current.Handler as SkinBase;        

		AspDotNetStorefrontCore.Customer adnsfCustomer = AspDotNetStorefrontCore.Customer.Current;

		string errorMessage, errorCode;
		this.PaymentProfileId = ProcessTools.SaveProfileAndPaymentProfile(adnsfCustomer.CustomerID, adnsfCustomer.EMail, AspDotNetStorefrontCore.AppLogic.AppConfig("StoreName"), this.PaymentProfileId, selectedAddressId, TextCreditCard.Text, TextCardSecurity.Text, ExpirationMonth.SelectedValue, ExpirationYear.SelectedValue, out errorMessage, out errorCode);
		
		if (PaymentProfileId <= 0)
		{
			ShowError(String.Format("{0} {1}", AspDotNetStorefrontCore.AppLogic.GetString("AspDotNetStorefrontGateways.AuthorizeNet.Cim.ErrorMessage", adnsfCustomer.SkinID, adnsfCustomer.LocaleSetting), errorMessage));
			return;
		}
			
		if (CBMakePrimaryCard.Checked)
		{
			AspDotNetStorefrontCore.Address adnsfAddress = new AspDotNetStorefrontCore.Address();
			adnsfAddress.LoadFromDB(selectedAddressId);
			adnsfAddress.MakeCustomersPrimaryAddress(AspDotNetStorefrontCore.AddressTypes.Billing);
			DataUtility.SetPrimaryPaymentProfile(adnsfCustomer.CustomerID, this.PaymentProfileId);
		}

		BindPage(this.PaymentProfileId);

		FireCardEditComplete();
	}

	protected void ButtonCancel_Click(object sender, EventArgs e)
	{
		FireCardEditComplete();
	}

	public void BindPage(Int64 paymentProfileId)
	{
		DisableButtonOnClick(ButtonSave);

		ErrorMessage.Text = string.Empty;
		TextCardSecurity.Text = string.Empty;
		TextCreditCard.Text = string.Empty;

		AspDotNetStorefrontCore.Customer adnsfCustomer = AspDotNetStorefrontCore.Customer.Current;
		int customerId = adnsfCustomer.CustomerID;

		PanelError.Visible = false;
		PopulateExpirationDates();
		PopulateAddresses(customerId);

		this.PaymentProfileId = 0; // initially zero

		if(paymentProfileId <= 0)
			return;

		Int64 profileId = DataUtility.GetProfileId(customerId);
		if (profileId <= 0)
			return;

		var profileMgr = new ProfileManager(customerId, adnsfCustomer.EMail, profileId);
		var paymentProfile = profileMgr.GetPaymentProfile(paymentProfileId);

		if(paymentProfile == null)
			return;

		this.PaymentProfileId = paymentProfileId;

		var ccPayment = (GatewayAuthorizeNet.AuthorizeNetApi.CreditCardMaskedType)paymentProfile.payment.Item;
		var dataPaymentProfile = DataUtility.GetPaymentProfile(adnsfCustomer.CustomerID, this.PaymentProfileId);

		TextCreditCard.Text = ccPayment.cardNumber;
		TextCardSecurity.Text = string.Empty;

		ExpirationMonth.ClearSelection();
		var foundItem = ExpirationMonth.Items.FindByValue(dataPaymentProfile.ExpirationMonth);
		if (foundItem != null)
			foundItem.Selected = true;

		ExpirationYear.ClearSelection();
		foundItem = ExpirationYear.Items.FindByValue(dataPaymentProfile.ExpirationYear);
		if (foundItem != null)
			foundItem.Selected = true;

		var address = DataUtility.GetPaymentProfile(customerId, this.PaymentProfileId);
		if (address != null)
		{
			BillingAddresses.ClearSelection();
			BillingAddresses.Items.FindByValue(address.AddressId.ToString()).Selected = true;
		}
	}

	public void ShowError(string message)
	{
		PanelError.Visible = true;
		ErrorMessage.Text = message;
	}

	public void PopulateAddresses(int customerId)
	{
		BillingAddresses.Items.Clear();

		using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
		{
			connection.Open();

			using(SqlCommand command = new SqlCommand("select AddressID, Address1 + ' ' + City + ', ' + State + ', ' + Zip + ' ' + Country [DisplayValue] from Address where CustomerID = @customerId and Deleted = 0", connection))
			{
				command.Parameters.AddRange(new[] {
						new SqlParameter("@customerId", customerId),
					});

				SqlDataAdapter adapter = new SqlDataAdapter(command);
				System.Data.DataSet dataSet = new System.Data.DataSet();
				adapter.Fill(dataSet);

				BillingAddresses.DataSource = dataSet;
				BillingAddresses.DataTextField = "DisplayValue";
				BillingAddresses.DataValueField = "AddressID";
				BillingAddresses.DataBind();
			}
		}
	}

	public void PopulateExpirationDates()
	{
		int currentYear = DateTime.Now.Date.Year;

		ExpirationMonth.Items.Clear();
		ExpirationMonth.Items.Add(new ListItem("--"));
		for (int i = 1; i <= 12; i++)
		{
			ExpirationMonth.Items.Add(new ListItem(i.ToString(), i.ToString("D2")));
		}

		int lastYear = 10 + currentYear;
		ExpirationYear.Items.Clear();
		ExpirationYear.Items.Add(new ListItem("--"));
		for (int i = currentYear; i <= lastYear; i++)
		{
			ExpirationYear.Items.Add(new ListItem(i.ToString(), i.ToString()));
		}
	}

	private void FireCardEditComplete()
	{
		if (CardEditComplete != null)
			CardEditComplete(this, EventArgs.Empty);
	}

	//
	// Disable button with no secondary JavaScript function call.
	//
	public static void DisableButtonOnClick(Button ButtonControl)
	{
		DisableButtonOnClick(ButtonControl, string.Empty);
	}

	//
	// Disable button with a JavaScript function call.
	//
	public static void DisableButtonOnClick(Button ButtonControl, string ClientFunction)
	{
		StringBuilder sb = new StringBuilder(128);

		// If the page has ASP.NET validators on it, this code ensures the
		// page validates before continuing.
		sb.Append("if ( typeof( Page_ClientValidate ) == 'function' ) { ");
		sb.Append("if ( ! Page_ClientValidate('CIMCCEditor') ) { return false; } } ");

		// Disable this button.
		sb.Append("this.disabled = true;");

		// If a secondary JavaScript function has been provided, and if it can be found,
		// call it. Note the name of the JavaScript function to call should be passed without
		// parens.
		if (!String.IsNullOrEmpty(ClientFunction))
		{
			sb.AppendFormat("if ( typeof( {0} ) == 'function' ) {{ {0}() }};", ClientFunction);
		}

		// GetPostBackEventReference() obtains a reference to a client-side script function 
		// that causes the server to post back to the page (ie this causes the server-side part 
		// of the "click" to be performed).
		sb.Append(ButtonControl.Page.ClientScript.GetPostBackEventReference(ButtonControl, string.Empty) + ";");

		// Add the JavaScript created a code to be executed when the button is clicked.
		ButtonControl.UseSubmitBehavior = false;
		ButtonControl.Attributes.Add("onclick", sb.ToString());
	}
}
