// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront.Kit
{
    public partial class DropDownListItems : BaseKitGroupControl
    {
        public override void DataBind()
        {
            base.DataBind();
            PopulateDropDown();
        }

        private void PopulateDropDown()
        {
            cboKitItems.Items.Clear();
            foreach (var kitItem in KitGroup.SelectableItems)
            {
                ListItem thisListItem = new ListItem();

                // truncate after 75 chars to maintain a proper width for the dropdown
                thisListItem.Text = KitItemDisplayText(kitItem, 60);
                thisListItem.Value = kitItem.Id.ToString();

                if (kitItem.HasMappedVariant && !kitItem.VariantHasStock && AppLogic.AppConfigBool("KitInventory.DisableItemSelection"))
                {
					thisListItem.Text += AppLogic.AppConfigBool("KitInventory.ShowStockHint") ? " - " + AppLogic.GetString("OutofStock.DisplayOutOfStockOnProductPage", ThisCustomer.LocaleSetting) : String.Empty;
                    thisListItem.Attributes.Add("disabled", "disabled");
                    thisListItem.Selected = false;
                    kitItem.IsSelected = false;
                }
                else
                {
                    bool isSetTrue = false;
                    foreach (var kitItem1 in KitGroup.SelectableItems)
                    {
                        if (kitItem1.IsSelected == true)
                        {
                            isSetTrue = true;
                        }
                    }
                    if (isSetTrue == false)
                    {
                        kitItem.IsSelected = true;
                    }
                }


                thisListItem.Selected = kitItem.IsSelected;
                cboKitItems.Items.Add(thisListItem);
            }
            litStockHint.Text = AppLogic.AppConfigBool("KitInventory.HideOutOfStock") ? StockHint(KitGroup.SelectableItems.FirstOrDefault()) : StockHint(KitGroup.Items[cboKitItems.SelectedIndex]);
        }

        public override void ResolveSelection()
        {
            foreach (ListItem lItem in cboKitItems.Items)
            {
                int kitId = lItem.Value.ToNativeInt();
                KitItemData kitItem = KitGroup.GetItem(kitId);

                kitItem.IsSelected = lItem.Selected;
            }
        }
    }
}


