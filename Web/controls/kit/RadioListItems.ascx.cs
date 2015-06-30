// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;
using System;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront.Kit
{
    public partial class RadioListItems : BaseKitGroupControl
    {
        protected void rptItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                RadioButton rbItem = e.Item.FindControl<RadioButton>("rbItem");
                KitItemData kitItem = e.Item.DataItemAs<KitItemData>();

				rbItem.Attributes["onclick"] = string.Format("SetUniqueRadioButton('{0}', '{1}',this)", rptItems.UniqueID, rbItem.GroupName);
                if (kitItem.HasMappedVariant && !kitItem.VariantHasStock && AppLogic.AppConfigBool("KitInventory.DisableItemSelection"))
				{
                    rbItem.Checked = false;
					rbItem.Enabled = false;
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
                rbItem.Checked = kitItem.IsSelected;
            }
        }

        public override void ResolveSelection()
        {
            rptItems.ForEachItemTemplate(RepeaterItemIterator);
        }

        private void RepeaterItemIterator(RepeaterItem item)
        {
            HiddenField hdfKitItemId = item.FindControl<HiddenField>("hdfKitItemId");
            RadioButton rbItem = item.FindControl<RadioButton>("rbItem");

            int kitItemId = hdfKitItemId.Value.ToNativeInt();
            KitItemData kitItem = KitGroup.GetItem(kitItemId);

            kitItem.IsSelected = rbItem.Checked;
        }
    }
}


