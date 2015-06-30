// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore.Validation;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Settings;
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.WebUtility;

public partial class OPCControls_AddressBook : 
    OPCUserControl<IAccountModel>,
    IAccountView
{
	#region Page Events

	protected void DataListAddressBook_OnItemCommand(object source, DataListCommandEventArgs e)
	{
		if(e.CommandName == "UseSelected")
		{
			string addressId = (string)e.CommandArgument;

			if(AddressType == Vortx.OnePageCheckout.Models.AddressType.Billing)
			{                
				this.Model.PrimaryBillingAddressId = addressId;
			}
            if (AddressType == Vortx.OnePageCheckout.Models.AddressType.Shipping)
			{
				this.Model.PrimaryShippingAddressId = addressId;
			}
		}
	}

	protected void DataListAddressBook_OnItemDataBound(object source, DataListItemEventArgs args)
	{
		if((args.Item.ItemType == ListItemType.Item) ||
			(args.Item.ItemType == ListItemType.AlternatingItem))
		{
			IAddressModel model = (IAddressModel)args.Item.DataItem;
			var addressView = (OPCUserControl<IAddressModel>)args.Item.FindControl("AddressStaticItem");
			
            addressView.SetModel(model, StringResourceProvider);
            addressView.Enable();
            addressView.Show();
            addressView.BindView();
		}
	}

	#endregion

	#region Private Methods

	private void BindPage(IEnumerable<IAddressModel> addressBook)
	{
		if(!PageUtility.IsAsyncPostBackForControl(this, ConfigurationProvider.DefaultProvider.ScriptManagerId))
		{
			this.DataListAddressBook.DataSource = addressBook;
			this.DataListAddressBook.DataBind();
		}
	}

	#endregion 

	#region IAccountView Members
    
    public Vortx.OnePageCheckout.Models.AddressType AddressType { get; private set; }
    public void SetAddressType(Vortx.OnePageCheckout.Models.AddressType addressType)
	{
		this.AddressType = addressType;	
	}

	public override void BindView()
	{
		IAddressValidator validator = new POBoxAddressValidator();
		IEnumerable<IAddressModel> addresses = null;
		if (ConfigurationProvider.DefaultProvider.DisallowShippingToPOBoxes)
			addresses = this.Model.AddressBook.Where(f => validator.IsValid(f.Address1));
		else
			addresses = this.Model.AddressBook;

		this.BindPage(addresses);
	}

	public override void BindView(object id)
	{
		BindView();
	}

	public override void SaveViewToModel()
	{

	}

	public override void Initialize()
	{
	}

	public override void Disable()
	{
	}

	public override void Enable()
	{
	}

	public override void Show()
	{
	}

	public override void Hide()
	{
	}

	public override void ShowError(string message)
	{

	}
	#endregion
}

