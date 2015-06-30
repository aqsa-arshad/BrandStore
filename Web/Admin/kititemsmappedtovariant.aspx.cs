// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.Linq;
using AspDotNetStorefrontControls;
using System.Web.Script.Serialization;
using System.Collections.Generic;
namespace AspDotNetStorefrontAdmin
{
    public partial class kititemsmappedtovariant : AdminPageBase
    {
        private int m_variantid;
        public int VariantId
        {
            get { return m_variantid; }
            set { m_variantid = value; }
        }
        private string m_localesetting;
        public new string LocaleSetting
        {
            get { return m_localesetting; }
            set { m_localesetting = value; }
        }
        private ProductVariantForKit m_variant;
        public ProductVariantForKit Variant
        {
            get { return m_variant; }
            set { m_variant = value; }
        }
        private Customer m_thiscustomer;
        public new Customer ThisCustomer
        {
            get { return m_thiscustomer; }
            private set { m_thiscustomer = value; }
        }
        private List<KitProductData> m_mappedkitproducts;
        public List<KitProductData> MappedKitProducts
        {
            get { return m_mappedkitproducts; }
            private set { m_mappedkitproducts = value; }
        }
        private const string EMPTY_NULL_DISPLAY = "-";

        protected override void OnInit(EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            ThisCustomer = Customer.Current;            
            VariantId = Request.QueryStringNativeInt("variantId");
            Variant = ProductVariantForKit.Find(ThisCustomer, VariantId);
            LocaleSetting = Request.QueryStringCanBeDangerousContent("locale");
            MappedKitProducts = KitProductData.GetProductsThatHasKitItemsMappedToVariant(VariantId, ThisCustomer, LocaleSetting);
            BindData();
            base.OnInit(e);
        }

        private void BindData()
        {
            dlMappedKits.DataSource = MappedKitProducts;
            dlMappedKits.DataBind();
        }

        protected string FormatCurrencyDisplay(decimal amount)
        {
            if (amount == decimal.Zero)
            {
                return EMPTY_NULL_DISPLAY;
            }
            return Localization.CurrencyStringForDisplayWithoutExchangeRate(amount, ThisCustomer.CurrencySetting);
        }

        protected string FormatWeightDisplay(decimal weight)
        {
            if (weight == decimal.Zero)
            {
                return EMPTY_NULL_DISPLAY;
            }
            else
            {
                return weight.ToString("0.00");
            }
        }

        protected void btnUpdateAll_Click(object sender, EventArgs e)
        {
            UpdateKitItems(true);
        }

        protected void btnUpdateNameAndDescription_Click(object sender, EventArgs e)
        {
            UpdateKitItems(false);
        }

        private void UpdateKitItems(bool updateAll)
        {
            foreach (RepeaterItem rptItemKit in dlMappedKits.Items)
            {
                if (rptItemKit.ItemType == ListItemType.Item || 
                    rptItemKit.ItemType == ListItemType.AlternatingItem)
                {
                    HiddenField hdfKitProductId = rptItemKit.FindControl<HiddenField>("hdfKitProductId");
                    int kitId = hdfKitProductId.Value.ToNativeInt();

                    KitProductData kit = MappedKitProducts.Find(k => k.Id == kitId);
                    if (kit != null)
                    {
                        Repeater rptKitGroups = rptItemKit.FindControl<Repeater>("rptKitGroups");
                        foreach (RepeaterItem rptItemGroup in rptKitGroups.Items)
                        {
                            if (rptItemGroup.ItemType == ListItemType.Item || 
                                rptItemGroup.ItemType == ListItemType.AlternatingItem)
                            {
                                HiddenField hdfGroupId = rptItemGroup.FindControl<HiddenField>("hdfGroupId");
                                int groupId = hdfGroupId.Value.ToNativeInt();

                                KitGroupData group = kit.GetGroup(groupId);
                                if (group != null)
                                {
                                    Repeater rptKitItems = rptItemGroup.FindControl<Repeater>("rptKitItems");
                                    foreach (RepeaterItem rptKitItem in rptKitItems.Items)
                                    {
                                        if (rptKitItem.ItemType == ListItemType.Item || 
                                            rptKitItem.ItemType == ListItemType.AlternatingItem)
                                        {
                                            HiddenField hdfKitItemtId = rptKitItem.FindControl<HiddenField>("hdfKitItemtId");
                                            int kitItemId = hdfKitItemtId.Value.ToNativeInt();

                                            CheckBox chkUpdate = rptKitItem.FindControl<CheckBox>("chkUpdate");
                                            if (chkUpdate.Checked)
                                            {
                                                KitItemData kitItem = group.GetItem(kitItemId);
                                                if (kitItem != null)
                                                {
                                                    kitItem.Name = XmlCommon.GetLocaleEntry(Variant.Name, LocaleSetting, false);
                                                    kitItem.Description = XmlCommon.GetLocaleEntry(Variant.Description, LocaleSetting, false);

                                                    if (updateAll)
                                                    {
                                                        kitItem.PriceDelta = Variant.SalePrice > decimal.Zero ? Variant.SalePrice : Variant.Price;
                                                        kitItem.WeightDelta = Variant.Weight;
                                                    }
                                                    kitItem.Save(LocaleSetting);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            BindData();
        }
    }
}



