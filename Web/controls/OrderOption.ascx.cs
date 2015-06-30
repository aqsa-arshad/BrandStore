// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Order option display control
    /// </summary>
    public partial class OrderOptionControl : BaseUserControl<ShoppingCart>
    {      

        /// <summary>
        /// Gets or sets if this control is in edit mode
        /// </summary>
        public bool EditMode
        {
            get
            {
                object booleanValue = ViewState["EditMode"];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["EditMode"] = value;
            }
        }
      
        /// <summary>
        /// Overrides the Databind event to initialize datasources
        /// </summary>
        public override void DataBind()
        {
            if (this.EditMode)
            {
                // display: The following Order Options are available
                lblNameHeader.Text = AppLogic.GetString("shoppingcart.cs.27", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);                

                // show all available options
                rptOrderOptions.DataSource = this.Datasource.AllOrderOptions;                
            }
            else
            {
                // display: You Have Selected The Following Order Options
                lblNameHeader.Text = AppLogic.GetString("order.cs.50", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);                

                // non-edit mode, show only selected options
                rptOrderOptions.DataSource = this.Datasource.OrderOptions;                
            }
            rptOrderOptions.DataBind();
			
            base.DataBind();
        }

        /// <summary>
        /// ItemDatabound event handler for rptOrderOptions control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptOrderOptions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || 
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var opt = e.Item.DataItemAs<OrderOption>();
                var helpcircle_gif = e.Item.FindControl<Image>("helpcircle_gif");
                if (helpcircle_gif != null)
                {
                    helpcircle_gif.Attributes.Add("onclick", "popuporderoptionwh('Order Option " + opt.ID.ToString() + "', " + opt.ID.ToString() + ",650,550,'yes');");
                }
            }
        }

        /// <summary>
        /// Determins if the current option should be checked
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        protected bool DetermineSelected(OrderOption opt)
        {
            if (this.Datasource.OrderOptions.Count == 0)
            {
                return opt.DefaultIsChecked;
            }
            else
            {
                return Datasource.OrderOptions.Any(xOpt => xOpt.Equals(opt));
            }
        }

        /// <summary>
        /// Resolves any selection changes made and updates the db accordingly
        /// </summary>
        /// <returns></returns>
        public override bool UpdateChanges()
        {
            List<OrderOption> updatedOptions = new List<OrderOption>();

            ForEachItemTemplate(rptOrderOptions, out updatedOptions);
			          

            // clear reset
            string clearSql = "Update Customer set OrderOptions = NULL where CustomerId = {0}".FormatWith(ThisCustomer.CustomerID);
            DB.ExecuteSQL(clearSql);

            if (updatedOptions.Count > 0)
            {
                string saveFormat = updatedOptions.Select(opt => opt.UniqueID.ToString()).ToList().ToCommaDelimitedString();
                string updateSql = "Update Customer set OrderOptions = {0} where CustomerId = {1}".FormatWith(saveFormat.DBQuote(), ThisCustomer.CustomerID);

                DB.ExecuteSQL(updateSql);
            }

            return true;
        }

        public void ForEachItemTemplate(Repeater rpt, out List<OrderOption> updatedOptions)
        {
            updatedOptions = new List<OrderOption>();
            foreach (RepeaterItem rptItem in rpt.Items)
            {
                if (rptItem.ItemType == ListItemType.Item ||
                    rptItem.ItemType == ListItemType.AlternatingItem)
                {
                    //                    itemAction(rptItem);

                    DataCheckBox chkSelected = rptItem.FindControl<DataCheckBox>("chkSelected");
                    if (chkSelected != null &&
                        chkSelected.Checked &&
                        chkSelected.Visible)
                    {
                        OrderOption xOpt = base.Datasource.AllOrderOptions.FirstOrDefault(opt => opt.UniqueID.ToString().EqualsIgnoreCase(chkSelected.Data.ToString()));
                        if (xOpt != null)
                        {
                            updatedOptions.Add(xOpt);
                        }
                    }

                }
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
        }

       

        /// <summary>
        /// Displays proper cost text
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        protected string DisplayCost(OrderOption opt)
        {
            string displayText = string.Empty;
            decimal cost = opt.Cost;

            if (cost == decimal.Zero)
            {
              
                    // display FREE
                    displayText = AppLogic.GetString("shoppingcart.aspx.16", this.ThisCustomer.SkinID, this.ThisCustomer.LocaleSetting);
            }
            else
            {
              
                    if (Datasource.VatEnabled)
                    {
                        if (Datasource.VatIsInclusive)
                        {
                            cost += opt.TaxRate;
                        }

                        // sample display format would be:
                        // $10.00 ex vat
                        //  VAT: $1.00 	
                        displayText = Localization.CurrencyStringForDisplayWithExchangeRate(cost, this.ThisCustomer.CurrencySetting);
                        displayText += "&nbsp;";
                        displayText += CommonLogic.IIF(Datasource.VatIsInclusive, AppLogic.GetString("setvatsetting.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("setvatsetting.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                        displayText += "VAT: " + ThisCustomer.CurrencyString(opt.TaxRate);
                    }
                    else
                    {
                        displayText = Localization.CurrencyStringForDisplayWithExchangeRate(cost, this.ThisCustomer.CurrencySetting);
                    }
                
            }


            return displayText;
        }
    }
}

