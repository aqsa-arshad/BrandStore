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
using Vortx.OnePageCheckout.UI;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Views;

public partial class VortxControls_CustomerService :
    OPCUserControl<IContentModel>,
	IContentView
{	
	#region IView Members

	public override void Initialize()
	{
		this.ContentDisplay.Text = string.Empty;
	}

	public override void Disable()
	{
		
	}

	public override void Enable()
	{
		
	}

	public override void Show()
	{
		this.Visible = true;
		this.ContentDisplay.Visible = true;
	}

	public override void Hide()
	{
		this.Visible = false;
		this.ContentDisplay.Visible = false;
	}

	public override void BindView()
	{
		this.ContentDisplay.Text = this.Model.Html;
	}

	public override void BindView(object identifier)
	{

	}

	public override void SaveViewToModel()
	{
	}

	public override void ShowError(string message)
	{
	}

	#endregion
}
