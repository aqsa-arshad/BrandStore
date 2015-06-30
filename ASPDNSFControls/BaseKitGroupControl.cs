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
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Summary description for BaseKitGroupControl
    /// </summary>
    public class BaseKitGroupControl : UserControl
    {
        public const int SINGLE_SELECT_DROPDOWN_LIST = 1;
        public const int SINGLE_SELECT_RADIO_LIST = 2;
        public const int MULTI_SELECT_RADIO_LIST = 3;
        public const int TEXT_OPTION = 4;
        public const int TEXT_AREA = 5;
        public const int FILE_OPTION = 6;

        private Customer m_thiscustomer;

        public Customer ThisCustomer
        {
            get { return m_thiscustomer; }
            set { m_thiscustomer = value; }
        }
        private KitGroupData group = null;

        public event EventHandler CompositionChanged;
        public KitGroupData KitGroup
        {
            get { return group; }
            set
            {
                group = value;                
            }
        }

        protected override void OnInit(EventArgs e)
        {
            this.AppRelativeTemplateSourceDirectory = "~/";
            base.OnInit(e);
        }

        public virtual void ResolveSelection()
        {
        }

        public string KitItemDisplayText(KitItemData item)
        {
            return KitItemDisplayText(item, 999);
        }

        public string KitItemDisplayText(KitItemData item, int maxCharLength)
        {
            string text = string.Empty;

            string displayName = item.Name;
            if (displayName.Length > maxCharLength)
            {
                displayName = CommonLogic.Ellipses(displayName, maxCharLength, true);
            }

            if (item.IsSelected)
            {
                // just return the name
                text = displayName;
            }
            else
            {
                text = string.Format("{0} [{1}]", displayName, KitItemRelativePriceDeltaDisplayText(item));
            }

            return text;
        }

        public string KitItemRelativePriceDeltaDisplayText(KitItemData item)
        {
            if (item.IsSelected)
            {
                // Included in total price
                return AppLogic.GetString("kitproduct.aspx.10", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
            else
            {
                string deltaText = string.Empty;
                if (item.RelativePriceDeltaIsAdd)
                {
                    // Add
                    deltaText = AppLogic.GetString("kitproduct.aspx.11", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                else
                {
                    // Subtract
                    deltaText = AppLogic.GetString("kitproduct.aspx.12", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }

                String deltaDisplayFormat = String.Empty;

                if (item.Group.SelectionControl == MULTI_SELECT_RADIO_LIST
                    || item.Group.SelectionControl == TEXT_OPTION
                    || item.Group.SelectionControl == TEXT_AREA)
                {
                    deltaDisplayFormat = Localization.CurrencyStringForDisplayWithoutExchangeRate(item.PriceDelta, this.ThisCustomer.CurrencySetting);
                }
                else
                {
                    deltaDisplayFormat = Localization.CurrencyStringForDisplayWithoutExchangeRate(item.RelativePriceDelta, this.ThisCustomer.CurrencySetting);
                }
                return string.Format("{0} {1}", deltaText, deltaDisplayFormat.Replace("-",String.Empty));
            }
        }

        public string StockHint(KitItemData item)
        {
            string html = string.Empty;

            if (AppLogic.AppConfigBool("KitInventory.ShowStockHint"))
            {
                if (item.HasMappedVariant)
                {
                    bool hasStock = item.VariantHasStock;

                    string stockCssClass = string.Empty;
                    string stockHintMessage = string.Empty;

                    if (hasStock)
                    {
                        stockCssClass = "in-stock-hint";
                        stockHintMessage = AppLogic.GetString("OutofStock.DisplayInStockOnProductPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                    else
                    {
                        stockCssClass = "out-stock-hint";
                        stockHintMessage = AppLogic.GetString("OutofStock.DisplayOutOfStockOnProductPage", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }

                    html = string.Format(" <span class=\"{0}\">[{1}{2}]</span>", stockCssClass, stockHintMessage, ThisCustomer.IsAdminUser ? " (" + item.MappedVariant.InventoryCount.ToString() + ")" : string.Empty);
                }
            }

            return html;
        }

        public BaseKitGroupControl()
        {
        }

        protected virtual void OnCompositionChanged(EventArgs e)
        {
            if (CompositionChanged != null)
            {
                CompositionChanged(this, e);
            }
        }
    }
}

