// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Xml.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using AspDotNetStorefront;
using ADNSF = AspDotNetStorefrontCore;
using AspDotNetStorefront.Promotions;
using AspDotNetStorefrontCore;

#endregion

public partial class _PromotionEditor : AspDotNetStorefrontAdmin.AdminPageBase
{
    #region Event Handlers
    protected void Page_Load(object sender, EventArgs e)
    {
        Title = "admin.PromotionEditor.cbTitle".StringResource();
        SectionTitle = "admin.PromotionEditor.cbSectionTitle".StringResource();
        trStoreMapping.Visible = AppLogic.GlobalConfigBool("AllowCouponFiltering");
    }

    protected void btnSearch_Click(Object sender, EventArgs e)
    {
        gridPromotions.PageIndex = 0;
        BindData();
    }

    protected void gridPromotions_SelectedIndexChanged(Object sender, EventArgs e)
    {
        Int32 id = (Int32)gridPromotions.SelectedValue;
        EditForm(id);
    }

    protected void gridPromotions_PageChanged(Object sender, EventArgs e)
    {
        BindData();
    }

    protected void btnAdd_Click(Object sender, EventArgs e)
    {
        ClearForm();
        AddForm();
    }

    protected void btnUpdate_Click(Object sender, EventArgs e)
    {
        Int32 id;
        Int32.TryParse(txtId.Value, out id);
        txtId.Value = UpdateForm(id).ToString();
    }

    protected void btnDelete_Click(Object sender, EventArgs e)
    {
        Int32 id;
        if (Int32.TryParse(txtId.Value, out id))
            DeleteForm(id);
    }

    protected void btnCancel_Click(Object sender, EventArgs e)
    {
        CancelForm();
    }

    protected void btnUpload_Click(Object sender, EventArgs e)
    {
        lblEmailUploadError.Text = String.Empty;

        if (fileUpload.PostedFile.ContentLength <= 0)
        {
            lblEmailUploadError.Text = "File did not contain any email addresses.";
            return;
        }

        try
        {
            String filename = GetFileName();
            SaveFile(fileUpload, filename);
            ImportFile(filename);
            return;
        }
        catch (Exception exception)
        {
            lblEmailUploadError.Text = exception.Message;
        }
    }
    
    #endregion

    #region Private Methods

    private void BindData()
    {
        String keyword = txtKeyword.Text;

        if (keyword == string.Empty) keyword = "*";

        StringBuilder whereString = new StringBuilder();
        whereString.Append("(");
        whereString.AppendFormat("(\"*\" = \"{0}\"", txtKeyword.Text);
        whereString.AppendFormat(" || Code.Contains(\"{0}\")", txtKeyword.Text);
        whereString.AppendFormat(" || Name.Contains(\"{0}\")", txtKeyword.Text);
        whereString.AppendFormat(" || Description.Contains(\"{0}\"))", txtKeyword.Text);
        if (chkActiveFilter.Checked)
            whereString.AppendFormat(" && Active");
        if (chkAutoAssignedFilter.Checked)
            whereString.AppendFormat(" && AutoAssigned");


        whereString.Append(")");

        PromotionDataSource.Where = whereString.ToString();
        PromotionDataSource.DataBind();
        gridPromotions.DataBind();
    }

    private void ToggleView(Boolean editing, Boolean isNewRecord)
    {
        pnlView.Visible = !editing;
		pnlUpdate.Visible = editing;
		pnlRulesDiscounts.Visible = editing;

		bool shippingDiscountsEnabled = !AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") && !AppLogic.AppConfigBool("ShowGiftRegistryButtons");
        pnlShippingDiscount.Enabled = shippingDiscountsEnabled;
		pnlShippingOnlyDiscount.Enabled = shippingDiscountsEnabled;
		CSVHelper2.Visible = shippingDiscountsEnabled;
    }

    private void AddForm()
    {
        EditForm(0);
    }

    private void EditForm(Int32 id)
    {
        ClearForm();
        AspDotNetStorefront.Promotions.Data.Promotion promotion = AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.Promotions.FirstOrDefault(l => l.Id == id);
        if (promotion == null)
        {
            ToggleView(true, true);
            return;
        }
        else
        {
            ToggleView(true, false);
        }

        txtId.Value = promotion.Id.ToString();
        lblTitle.Text =
            txtName.Text = promotion.Name;
        txtDescription.Text = promotion.Description;
        txtUsageText.Text = promotion.UsageText;
        txtCode.Text = promotion.Code;
        txtPriority.Text = promotion.Priority.ToString();
        rblActive.SelectedValue = promotion.Active.ToString().ToLower();
        chkAutoAssigned.Checked = promotion.AutoAssigned;
		CallToActionTextbox.Text = promotion.CallToAction;

        ssPromotion.SelectedStoreIDs = promotion.PromotionStores.Select(ps => ps.StoreID).ToArray();

        foreach (PromotionRuleBase rule in promotion.PromotionRules)
        {
            StartDatePromotionRule startDateRule = rule as StartDatePromotionRule;
            if (startDateRule != null)
            {
                optRuleStartDate.Checked = true;
                txtRuleStartDate.SelectedDate= startDateRule.StartDate;
            }

            ExpirationDatePromotionRule expirationDateRule = rule as ExpirationDatePromotionRule;
            if (expirationDateRule != null)
            {
                optRuleExpirationDate.Checked = true;
                txtRuleExpirationDate.SelectedDate = expirationDateRule.ExpirationDate;
            }

            ExpirationNumberOfUsesPromotionRule expirationDateNumberOfUsesRule = rule as ExpirationNumberOfUsesPromotionRule;
            if (expirationDateNumberOfUsesRule != null)
            {
                optRuleExpirationNumberOfUses.Checked = true;
                txtRuleExpirationNumberOfUses.Text = expirationDateNumberOfUsesRule.NumberOfUsesAllowed.ToString();
                chkRuleExpirationNumberOfUsesPerCustomer.Checked = false;
            }

            ExpirationNumberOfUsesPerCustomerPromotionRule expirationDateNumberOfUsesPerCustomerRule = rule as ExpirationNumberOfUsesPerCustomerPromotionRule;
            if (expirationDateNumberOfUsesPerCustomerRule != null)
            {
                optRuleExpirationNumberOfUses.Checked = true;
                txtRuleExpirationNumberOfUses.Text = expirationDateNumberOfUsesPerCustomerRule.NumberOfUsesAllowed.ToString();
                chkRuleExpirationNumberOfUsesPerCustomer.Checked = true;
            }

            EmailAddressPromotionRule emailAddressRule = rule as EmailAddressPromotionRule;
            if (emailAddressRule != null)
            {
                chkRuleEmail.Checked = true;
                txtRuleEmailAddresses.Text = String.Join(",", emailAddressRule.EmailAddresses);
                continue;
            }

            MinimumCartAmountPromotionRule cartAmountRule = rule as MinimumCartAmountPromotionRule;
            if (cartAmountRule != null)
            {
                chkRuleCartAmount.Checked = true;
                txtRuleCartAmount.Text = cartAmountRule.CartAmount.ToString();
                continue;
            }

            CustomerLevelPromotionRule customerLevelRule = rule as CustomerLevelPromotionRule;
            if (customerLevelRule != null)
            {
                chkRuleCustomerLevel.Checked = true;
                String[] customerLevels = Array.ConvertAll<int, string>(customerLevelRule.CustomerLevels, new Converter<int, string>(Convert.ToString));
                txtRuleCustomerLevels.Text = String.Join(",", customerLevels);
                continue;
            }

            CategoryPromotionRule categoryPromotionRule = rule as CategoryPromotionRule;
            if (categoryPromotionRule != null)
            {
                chkRuleCategories.Checked = true;
                String[] categories = Array.ConvertAll<int, string>(categoryPromotionRule.CategoryIds, new Converter<int, string>(Convert.ToString));
                txtRuleCategories.Text = String.Join(",", categories);
            }

            SectionPromotionRule sectionPromotionRule = rule as SectionPromotionRule;
            if (sectionPromotionRule != null)
            {
                chkRuleSections.Checked = true;
                String[] sections = Array.ConvertAll<int, string>(sectionPromotionRule.SectionIds, new Converter<int, string>(Convert.ToString));
                txtRuleSections.Text = String.Join(",", sections);
            }

            ManufacturerPromotionRule manufacturerPromotionRule = rule as ManufacturerPromotionRule;
            if (manufacturerPromotionRule != null)
            {
                chkRuleManufacturers.Checked = true;
                String[] manufacturers = Array.ConvertAll<int, string>(manufacturerPromotionRule.ManufacturerIds, new Converter<int, string>(Convert.ToString));
                txtRuleManufacturers.Text = String.Join(",", manufacturers);
            }

            ProductIdPromotionRule productIdRule = rule as ProductIdPromotionRule;
            if (productIdRule != null)
            {
                chkRuleProductId.Checked = true;
                String[] productIds = Array.ConvertAll<int, string>(productIdRule.ProductIds, new Converter<int, string>(Convert.ToString));
                txtRuleProductIds.Text = String.Join(",", productIds);
                rblProductsAllOrAny.SelectedIndex = CommonLogic.IIF(productIdRule.AndTogether, 0, 1);
                txtRuleProductIdsRequireQuantity.Text = productIdRule.Quantity.ToString();
                continue;
            }

            StatePromotionRule stateRule = rule as StatePromotionRule;
            if (stateRule != null)
            {
                chkRuleState.Checked = true;
                txtRuleStates.Text = String.Join(",", stateRule.States);
                continue;
            }

            ZipCodePromotionRule zipCodeRule = rule as ZipCodePromotionRule;
            if (zipCodeRule != null)
            {
                chkRuleZipCode.Checked = true;
                txtRuleZipCodes.Text = String.Join(",", zipCodeRule.ZipCodes);
                continue;
            }

            CountryPromotionRule countryRule = rule as CountryPromotionRule;
            if (countryRule != null)
            {
                chkRuleCountryCodes.Checked = true;
                txtRuleCountryCodes.Text = String.Join(",", countryRule.CountryCodes);
                continue;
            }

            MinimumOrdersPromotionRule minimumOrdersRule = rule as MinimumOrdersPromotionRule;
            if (minimumOrdersRule != null)
            {
                chkRuleMinimumOrders.Checked = true;
                txtRuleMinimumOrders.Text = minimumOrdersRule.MinimumOrdersAllowed.ToString();
                txtRuleMinimumOrdersCustomStartDate.SelectedDate = minimumOrdersRule.CustomStartDate;
                txtRuleMinimumOrdersCustomEndDate.SelectedDate = minimumOrdersRule.CustomEndDate;
            }

            MinimumOrderAmountPromotionRule minimumOrderAmountRule = rule as MinimumOrderAmountPromotionRule;
            if (minimumOrderAmountRule != null)
            {
                chkRuleMinimumOrderAmount.Checked = true;
                txtRuleMinimumOrderAmount.Text = minimumOrderAmountRule.MinimumOrderAmountAllowed.ToString();
                txtRuleMinimumOrderAmountCustomStartDate.SelectedDate = minimumOrderAmountRule.CustomStartDate;
                txtRuleMinimumOrderAmountCustomEndDate.SelectedDate = minimumOrderAmountRule.CustomEndDate;
            }

            MinimumProductsOrderedPromotionRule minimumProductsOrderedRule = rule as MinimumProductsOrderedPromotionRule;
            if (minimumProductsOrderedRule != null)
            {
                chkRuleMinimumProductsOrdered.Checked = true;
                txtRuleMinimumProductsOrdered.Text = minimumProductsOrderedRule.MinimumProductsOrderedAllowed.ToString();
                txtRuleMinimumProductsOrderedCustomStartDate.SelectedDate = minimumProductsOrderedRule.CustomStartDate;
                txtRuleMinimumProductsOrderedCustomEndDate.SelectedDate = minimumProductsOrderedRule.CustomEndDate;
                txtRuleMinimumProductsOrderedProductIds.Text = String.Join(",", Array.ConvertAll<int, string>(minimumProductsOrderedRule.ProductIds, i => i.ToString()));
            }

            MinimumProductAmountOrderedPromotionRule minimumProductAmountOrderedRule = rule as MinimumProductAmountOrderedPromotionRule;
            if (minimumProductAmountOrderedRule != null)
            {
                chkRuleMinimumProductsOrderedAmount.Checked = true;
                txtRuleMinimumProductsOrderedAmount.Text = minimumProductAmountOrderedRule.MinimumProductAmountOrderedAllowed.ToString();
                txtRuleMinimumProductsOrderedAmountCustomStartDate.SelectedDate = minimumProductAmountOrderedRule.CustomStartDate;
                txtRuleMinimumProductsOrderedAmountCustomEndDate.SelectedDate = minimumProductAmountOrderedRule.CustomEndDate;
                txtRuleMinimumProductsOrderedAmountProductIds.Text = String.Join(",", Array.ConvertAll<int, string>(minimumProductAmountOrderedRule.ProductIds, i => i.ToString()));
            }
        }

        foreach (PromotionDiscountBase discount in promotion.PromotionDiscounts)
        {
            ShippingPromotionDiscount shippingDiscount = discount as ShippingPromotionDiscount;
            if (shippingDiscount != null)
            {
                chkRuleShippingDiscount.Checked = true;
                ddlRuleShippingDiscountType.SelectedValue = ((Int32)shippingDiscount.DiscountType).ToString();

                if (shippingDiscount.DiscountType == AspDotNetStorefront.Promotions.DiscountType.Fixed)
                    txtRuleShippingDiscountAmount.Text = shippingDiscount.DiscountAmount.ToString("0.0000");
                else
                    txtRuleShippingDiscountAmount.Text = (shippingDiscount.DiscountAmount * 100).ToString("0.0000");

				String shipMethodIds = String.Join(",", shippingDiscount.ShippingMethodIds.Select(i => i.ToString()).ToArray());
				txtRuleShippingMethodID.Text = shipMethodIds;

				continue;
            }

            GiftProductPromotionDiscount giftProductDiscount = discount as GiftProductPromotionDiscount;
            if (giftProductDiscount != null && giftProductDiscount.GiftSkus != null)
            {

                chkRuleGiftWithPurchase.Checked = true;
                chkMatchQuantites.Checked = giftProductDiscount.MatchQuantities;
                txtGiftWithPurchaseDiscountAmount.Text = giftProductDiscount.GiftDiscountPercentage.ToString("0.0000");
            }

            if (giftProductDiscount != null && giftProductDiscount.GiftProductIds != null)
            {
                chkRuleGiftWithPurchase.Checked = true;
                txtRuleGiftWithPurchaseProductId.Text = String.Join(",", Array.ConvertAll<int, string>(giftProductDiscount.GiftProductIds, i => i.ToString()));
                chkMatchQuantites.Checked = giftProductDiscount.MatchQuantities;
                txtGiftWithPurchaseDiscountAmount.Text = giftProductDiscount.GiftDiscountPercentage.ToString("0.0000");
            }

            OrderPromotionDiscount orderDiscount = discount as OrderPromotionDiscount;
            if (orderDiscount != null)
            {
                chkRuleOrderDiscount.Checked = true;
                ddlRuleOrderDiscountType.SelectedValue = ((Int32)orderDiscount.DiscountType).ToString();
                if (orderDiscount.DiscountType == AspDotNetStorefront.Promotions.DiscountType.Fixed)
                    txtRuleOrderDiscountAmount.Text = orderDiscount.DiscountAmount.ToString("0.0000");
                else
                    txtRuleOrderDiscountAmount.Text = (orderDiscount.DiscountAmount * 100).ToString("0.0000");
                continue;
            }

            OrderItemPromotionDiscount orderItemDiscount = discount as OrderItemPromotionDiscount;
            if (orderItemDiscount != null)
            {
                chkRuleLineItemDiscount.Checked = true;
                ddlRuleLineItemDiscountType.SelectedValue = ((Int32)orderItemDiscount.DiscountType).ToString();
                if (orderItemDiscount.DiscountType == AspDotNetStorefront.Promotions.DiscountType.Fixed)
                    txtRuleLineItemDiscountAmount.Text = orderItemDiscount.DiscountAmount.ToString("0.0000");
                else
                    txtRuleLineItemDiscountAmount.Text = (orderItemDiscount.DiscountAmount * 100).ToString("0.0000");
                continue;
            }
        }
    }

    private Boolean ValidateForm(Boolean isNewRecord)
    {
        lblError.Text = String.Empty;

        if (isNewRecord)
        {
            String code = txtCode.Text.ToLower().Trim();
            if (AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.Promotions.Any(p => p.Code.ToLower() == code))
            {
                NotifyOfError("admin.PromotionEditor.ErrorCodeInUse".StringResource());
                return false;
            }
        }
        // Discount Rule
        if (chkRuleShippingDiscount.Checked && String.IsNullOrEmpty(txtRuleShippingDiscountAmount.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorDiscountShipping".StringResource());
            return false;
        }

        if (chkShippingOnlyDiscount.Checked && !chkRuleShippingDiscount.Checked)
        {
            chkRuleShippingDiscount.Checked = true;
            NotifyOfError("admin.PromotionEditor.ErrorDiscountShipping".StringResource());
            return false;
        }

        if (chkRuleOrderDiscount.Checked && String.IsNullOrEmpty(txtRuleOrderDiscountAmount.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorDiscountOrder".StringResource());
            return false;
        }

        if (chkRuleLineItemDiscount.Checked && String.IsNullOrEmpty(txtRuleLineItemDiscountAmount.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorDiscountApplicableLineItem".StringResource());
            return false;
        }

        if (chkRuleGiftWithPurchase.Checked && String.IsNullOrEmpty(txtGiftWithPurchaseDiscountAmount.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorGiftWithPurchase".StringResource());
            return false;
        }

        // Group One Rules
        if (optRuleStartDate.Checked && txtRuleStartDate.SelectedDate == null)
        {
            NotifyOfError("admin.PromotionEditor.ErrorStartOnADate".StringResource());
            return false;
        }

        if (optRuleExpirationDate.Checked && txtRuleExpirationDate.SelectedDate == null)
        {
            NotifyOfError("admin.PromotionEditor.ErrorExpirationRuleExpiresOnADate".StringResource());
            return false;
        }

        if (optRuleExpirationNumberOfUses.Checked && String.IsNullOrEmpty(txtRuleExpirationNumberOfUses.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorExpirationRuleExpiresAfterNumberOfUses".StringResource());
            return false;
        }
        
        // Group 2 Rules

        if (chkRuleProductId.Checked && String.IsNullOrEmpty(txtRuleProductIds.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableProductRequirement".StringResource());
            return false;
        }

        if (chkRuleCategories.Checked && String.IsNullOrEmpty(txtRuleCategories.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableCategoryRequirement".StringResource());
            return false;
        }

        if (chkRuleSections.Checked && String.IsNullOrEmpty(txtRuleSections.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableSectionRequirement".StringResource());
            return false;
        }

        if (chkRuleManufacturers.Checked && String.IsNullOrEmpty(txtRuleManufacturers.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableManufacturerRequirement".StringResource());
            return false;
        }

        if (chkRuleCartAmount.Checked && String.IsNullOrEmpty(txtRuleCartAmount.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableMinimumCartSubtotalRequirement".StringResource());
            return false;
        }

        if (chkRuleEmail.Checked && String.IsNullOrEmpty(txtRuleEmailAddresses.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableEmailAddressRequirement".StringResource());
            return false;
        }

        if (chkRuleCustomerLevel.Checked && String.IsNullOrEmpty(txtRuleCustomerLevels.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableEmailAddressRequirementCustLevel".StringResource());
            return false;
        }

        if (chkRuleState.Checked && String.IsNullOrEmpty(txtRuleStates.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableShippingStateRequirement".StringResource());
            return false;
        }

        if (chkRuleZipCode.Checked && String.IsNullOrEmpty(txtRuleZipCodes.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableShippingZipCodeRequirement".StringResource());
            return false;
        }
        
        if (chkRuleCountryCodes.Checked && String.IsNullOrEmpty(txtRuleCountryCodes.Text))
        {
            NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleEnableShippingCountryRequirement".StringResource());
            return false;
        }
        
        if (chkRuleMinimumOrders.Checked)
        {
            if (String.IsNullOrEmpty(txtRuleMinimumOrders.Text))
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofOrdersNeededMin".StringResource());
                return false;
            }

            if (txtRuleMinimumOrdersCustomStartDate.SelectedDate == null)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofOrdersNeededStartDate".StringResource());
                return false;
            }

            if (txtRuleMinimumOrdersCustomEndDate.SelectedDate == null)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofOrdersNeededEndDate".StringResource());
                return false;
            }

            if (txtRuleMinimumOrdersCustomStartDate.SelectedDate.Value.CompareTo(txtRuleMinimumOrdersCustomEndDate.SelectedDate.Value) > 0)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofOrdersStartDateAfterEndDate".StringResource());
                return false;
            }
        }

        if (chkRuleMinimumOrderAmount.Checked)
        {
            if (String.IsNullOrEmpty(txtRuleMinimumOrderAmount.Text))
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountNeededMin".StringResource());
                return false;
            }

            if (txtRuleMinimumOrderAmountCustomStartDate.SelectedDate == null)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountNeededStartDate".StringResource());
                return false;
            }

            if (txtRuleMinimumOrderAmountCustomEndDate.SelectedDate == null)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountNeededEndDate".StringResource());
                return false;
            }

            if (txtRuleMinimumOrderAmountCustomStartDate.SelectedDate.Value.CompareTo(txtRuleMinimumOrderAmountCustomEndDate.SelectedDate.Value) > 0)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountNeededStartDateAfterEndDate".StringResource());
                return false;
            }
        }

        if (chkRuleMinimumProductsOrdered.Checked)
        {
            if (String.IsNullOrEmpty(txtRuleMinimumProductsOrdered.Text))
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofProductsOrdered".StringResource());
                return false;
            }

            if (txtRuleMinimumProductsOrderedCustomStartDate.SelectedDate == null)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofProductsOrderedStartDate".StringResource());
                return false;
            }

            if (txtRuleMinimumProductsOrderedCustomEndDate.SelectedDate == null)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofProductsOrderedEndDate".StringResource());
                return false;
            }

            if (txtRuleMinimumProductsOrderedCustomStartDate.SelectedDate.Value.CompareTo(txtRuleMinimumProductsOrderedCustomEndDate.SelectedDate.Value) > 0)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofProductsOrderedStartDateAfterEndDate".StringResource());
                return false;
            }

            if (String.IsNullOrEmpty(txtRuleMinimumProductsOrderedProductIds.Text))
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumNumberofProductsOrderedProductIds".StringResource());
                return false;
            }
        }

        if (chkRuleMinimumProductsOrderedAmount.Checked)
        {
            if (String.IsNullOrEmpty(txtRuleMinimumProductsOrderedAmount.Text))
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountOfAProductNeededMin".StringResource());
                return false;
            }

            if (txtRuleMinimumProductsOrderedAmountCustomStartDate.SelectedDate == null)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountOfAProductNeededStartDate".StringResource());
                return false;
            }

            if (txtRuleMinimumProductsOrderedAmountCustomEndDate.SelectedDate == null)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountOfAProductNeededEndDate".StringResource());
                return false;
            }

            if (txtRuleMinimumProductsOrderedAmountCustomStartDate.SelectedDate.Value.CompareTo(txtRuleMinimumProductsOrderedAmountCustomEndDate.SelectedDate.Value) > 0)
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountOfAProductNeededStartDateAfterEndDate".StringResource());
                return false;
            }

            if (String.IsNullOrEmpty(txtRuleMinimumProductsOrderedAmountProductIds.Text))
            {
                NotifyOfError("admin.PromotionEditor.ErrorLoyaltyRuleMinimumOrderAmountOfAProductNeededProductIds".StringResource());
                return false;
            }
        }

        return true;
    }

    private Int32 UpdateForm(Int32 id)
    {
        if (!ValidateForm(id == 0))
            return id;

        AspDotNetStorefront.Promotions.Data.Promotion promotion = AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.Promotions.FirstOrDefault(l => l.Id == id);
        if (promotion == null)
        {
            promotion = new AspDotNetStorefront.Promotions.Data.Promotion()
            {
                PromotionGuid = Guid.NewGuid(),
                PromotionDiscountData = XElement.Parse("<root />"),
                PromotionRuleData = XElement.Parse("<root />")
            };
            AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.Promotions.InsertOnSubmit(promotion);
        }

        promotion.Name = txtName.Text;
        promotion.Description = txtDescription.Text;
        promotion.UsageText = txtUsageText.Text;
        promotion.EmailText = string.Empty;
        promotion.Code = txtCode.Text;
        promotion.Priority = Decimal.Parse(txtPriority.Text);
        promotion.Active = rblActive.SelectedValue.ToBool();
        promotion.AutoAssigned = chkAutoAssigned.Checked;
		promotion.CallToAction = CallToActionTextbox.Text;
        
        List<PromotionRuleBase> rules = new List<PromotionRuleBase>();

        if (optRuleStartDate.Checked)
        {
            rules.Add(new StartDatePromotionRule
            {
                StartDate = txtRuleStartDate.SelectedDate ?? DateTime.Now
            });
        }

        if (optRuleExpirationDate.Checked)
        {
            rules.Add(new ExpirationDatePromotionRule
            {
                ExpirationDate = txtRuleExpirationDate.SelectedDate ?? DateTime.Now
            });
        }

        if (optRuleExpirationNumberOfUses.Checked)
        {
            if (chkRuleExpirationNumberOfUsesPerCustomer.Checked)
                rules.Add(new ExpirationNumberOfUsesPerCustomerPromotionRule
                {
                    NumberOfUsesAllowed = Int32.Parse(txtRuleExpirationNumberOfUses.Text)
                });
            else
                rules.Add(new ExpirationNumberOfUsesPromotionRule
                {
                    NumberOfUsesAllowed = Int32.Parse(txtRuleExpirationNumberOfUses.Text)
                });
        }

        if (chkRuleEmail.Checked)
        {
            rules.Add(new EmailAddressPromotionRule
            {
                EmailAddresses = txtRuleEmailAddresses.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            });
        }

        if (chkRuleCartAmount.Checked)
        {
            rules.Add(new MinimumCartAmountPromotionRule
            {
                CartAmount = Convert.ToDecimal(txtRuleCartAmount.Text)
            });
        }

        if (chkRuleCustomerLevel.Checked)
        {
            rules.Add(new CustomerLevelPromotionRule
            {
                CustomerLevels = Array.ConvertAll<string, int>(txtRuleCustomerLevels.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString()))
            });
        }

        if (chkRuleProductId.Checked)
        {
            Int32 quantity = 0;
            Int32.TryParse(txtRuleProductIdsRequireQuantity.Text, out quantity);
            rules.Add(new ProductIdPromotionRule
            {
                AndTogether = rblProductsAllOrAny.SelectedIndex == 0,
                Quantity = quantity,
                RequireQuantity = quantity > 0,
                ProductIds = Array.ConvertAll<string, int>(txtRuleProductIds.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString()))
            });
        }

        if (chkRuleState.Checked)
        {
            rules.Add(new StatePromotionRule
            {
                States = txtRuleStates.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            });
        }

        if (chkRuleZipCode.Checked)
        {
            rules.Add(new ZipCodePromotionRule
            {
                ZipCodes = txtRuleZipCodes.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            });
        }

        if (chkRuleCountryCodes.Checked)
        {
            rules.Add(new CountryPromotionRule
            {
                CountryCodes = txtRuleCountryCodes.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray()
            });
        }

        if (chkRuleShippingDiscount.Checked && !String.IsNullOrEmpty(txtRuleShippingMethodID.Text))
        {
			rules.Add(new ShippingMethodIdPromotionRule());
        }

        if (chkRuleCategories.Checked && txtRuleCategories.Text.Length > 0)
        {
            String[] categoriesSplit = txtRuleCategories.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			int[] categories = Array.ConvertAll<string, int>(categoriesSplit, Convert.ToInt32);
            rules.Add(new CategoryPromotionRule
            {
                CategoryIds = categories
            });
        }

        if (chkRuleSections.Checked && txtRuleSections.Text.Length > 0)
        {
            String[] sectionsSplit = txtRuleSections.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			int[] sections = Array.ConvertAll<string, int>(sectionsSplit, Convert.ToInt32);
            rules.Add(new SectionPromotionRule
            {
                SectionIds = sections
            });
        }

        if (chkRuleManufacturers.Checked && txtRuleManufacturers.Text.Length > 0)
        {
            String[] manufacturersSplit = txtRuleManufacturers.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			int[] manufacturers = Array.ConvertAll<string, int>(manufacturersSplit, Convert.ToInt32);
            rules.Add(new ManufacturerPromotionRule
            {
                ManufacturerIds = manufacturers
            });
        }

        if (chkRuleMinimumOrders.Checked)
        {
            rules.Add(new MinimumOrdersPromotionRule
            {
                MinimumOrdersAllowed = Int32.Parse(txtRuleMinimumOrders.Text),
                StartDateType = DateType.CustomDate,
                CustomStartDate = txtRuleMinimumOrdersCustomStartDate.SelectedDate ?? DateTime.Now,
                EndDateType = DateType.CustomDate,
                CustomEndDate = txtRuleMinimumOrdersCustomEndDate.SelectedDate ?? DateTime.Now,
            });
        }

        if (chkRuleMinimumOrderAmount.Checked)
        {
            rules.Add(new MinimumOrderAmountPromotionRule
            {
                MinimumOrderAmountAllowed = Decimal.Parse(txtRuleMinimumOrderAmount.Text),
                StartDateType = DateType.CustomDate,
                CustomStartDate = txtRuleMinimumOrderAmountCustomStartDate.SelectedDate ?? DateTime.Now,
                EndDateType = DateType.CustomDate,
                CustomEndDate = txtRuleMinimumOrderAmountCustomEndDate.SelectedDate ?? DateTime.Now,
            });
        }

        if (chkRuleMinimumProductsOrdered.Checked)
        {
            rules.Add(new MinimumProductsOrderedPromotionRule
            {
                MinimumProductsOrderedAllowed = Int32.Parse(txtRuleMinimumProductsOrdered.Text),
                StartDateType = DateType.CustomDate,
                CustomStartDate = txtRuleMinimumProductsOrderedCustomStartDate.SelectedDate ?? DateTime.Now,
                EndDateType = DateType.CustomDate,
                CustomEndDate = txtRuleMinimumProductsOrderedCustomEndDate.SelectedDate ?? DateTime.Now,
                ProductIds = Array.ConvertAll<string, int>(txtRuleMinimumProductsOrderedProductIds.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString()))
            });
        }

        if (chkRuleMinimumProductsOrderedAmount.Checked)
        {
            rules.Add(new MinimumProductAmountOrderedPromotionRule
            {
                MinimumProductAmountOrderedAllowed = Decimal.Parse(txtRuleMinimumProductsOrderedAmount.Text),
                StartDateType = DateType.CustomDate,
                CustomStartDate = txtRuleMinimumProductsOrderedAmountCustomStartDate.SelectedDate ?? DateTime.Now,
                EndDateType = DateType.CustomDate,
                CustomEndDate = txtRuleMinimumProductsOrderedAmountCustomEndDate.SelectedDate ?? DateTime.Now,
                ProductIds = Array.ConvertAll<string, int>(txtRuleMinimumProductsOrderedAmountProductIds.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString()))
            });
        }
        
        List<PromotionDiscountBase> discounts = new List<PromotionDiscountBase>();
        if (chkRuleShippingDiscount.Checked && txtRuleShippingDiscountAmount.Text.Length > 0)
        {
            AspDotNetStorefront.Promotions.DiscountType discountType = (AspDotNetStorefront.Promotions.DiscountType)Int32.Parse(ddlRuleShippingDiscountType.SelectedValue);
            decimal discountAmount = decimal.Round(decimal.Parse(txtRuleShippingDiscountAmount.Text), 4);

            if (discountType == AspDotNetStorefront.Promotions.DiscountType.Percentage)
				discountAmount /= 100;

            String[] shipMethodsSplit = txtRuleShippingMethodID.Text
				.Replace(" ", "")
				.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                
			int[] shipMethods = Array.ConvertAll<string, int>(shipMethodsSplit, Convert.ToInt32);

            ShippingPromotionDiscount shippingPromotionDiscount = new ShippingPromotionDiscount(discountType, discountAmount, shipMethods);
            
			discounts.Add(shippingPromotionDiscount);
            rules.Add(new ShippingPromotionRule());
        }

        if (chkRuleGiftWithPurchase.Checked)
        {
            decimal giftDiscountPercentage = decimal.Round(decimal.Parse(txtGiftWithPurchaseDiscountAmount.Text), 4);
            GiftProductPromotionDiscount giftWithPurchaseDiscount = new GiftProductPromotionDiscount
            {
                GiftProductIds = Array.ConvertAll<string, int>(txtRuleGiftWithPurchaseProductId.Text.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i.ToString())),
                MatchQuantities = chkMatchQuantites.Checked,
                GiftDiscountPercentage = giftDiscountPercentage,
                PromotionId = promotion.Id
            };
            discounts.Add(giftWithPurchaseDiscount);
        }

        if (chkRuleOrderDiscount.Checked)
        {
            OrderPromotionDiscount orderPromotionDiscount = new OrderPromotionDiscount
            {
                DiscountType = (AspDotNetStorefront.Promotions.DiscountType)Int32.Parse(ddlRuleOrderDiscountType.SelectedValue)
            };
            if (orderPromotionDiscount.DiscountType == AspDotNetStorefront.Promotions.DiscountType.Fixed)
                orderPromotionDiscount.DiscountAmount = decimal.Round(decimal.Parse(txtRuleOrderDiscountAmount.Text), 4);
            else
                orderPromotionDiscount.DiscountAmount = decimal.Round(decimal.Parse(txtRuleOrderDiscountAmount.Text) / 100, 4);
            discounts.Add(orderPromotionDiscount);
        }

        if (chkRuleLineItemDiscount.Checked)
        {
            OrderItemPromotionDiscount orderItemPromotionDiscount = new OrderItemPromotionDiscount
            {
                DiscountType = (AspDotNetStorefront.Promotions.DiscountType)Int32.Parse(ddlRuleLineItemDiscountType.SelectedValue)
            };
            if (orderItemPromotionDiscount.DiscountType == AspDotNetStorefront.Promotions.DiscountType.Fixed)
                orderItemPromotionDiscount.DiscountAmount = decimal.Round(decimal.Parse(txtRuleLineItemDiscountAmount.Text), 4);
            else
                orderItemPromotionDiscount.DiscountAmount = decimal.Round(decimal.Parse(txtRuleLineItemDiscountAmount.Text) / 100, 4);
            discounts.Add(orderItemPromotionDiscount);
        }

        // This needs to be below the discount and rule creation because it serializes it to xml, it doesn't just store a reference.
        promotion.PromotionRules = rules;
        promotion.PromotionDiscounts = discounts;

        AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.SubmitChanges();

        foreach (var promotionStore in promotion.PromotionStores.ToList())
        {
            if (ssPromotion.SelectedStoreIDs.Contains(promotionStore.StoreID))
                continue;

            AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.PromotionStores.DeleteOnSubmit(promotionStore);
            promotion.PromotionStores.Remove(promotionStore);
        }
        
        foreach (var storeId in ssPromotion.SelectedStoreIDs)
        {
            if (promotion.PromotionStores.Any(ps => ps.StoreID == storeId))
                continue;

            var newPromotionStore = new AspDotNetStorefront.Promotions.Data.PromotionStore
            {
                CreatedOn = DateTime.Now,
                PromotionID = promotion.Id,
                StoreID = storeId
            };

            AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.PromotionStores.InsertOnSubmit(newPromotionStore);
            promotion.PromotionStores.Add(newPromotionStore);
        }
        AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.SubmitChanges();

        BindData();

        CancelForm();

        return promotion.Id;
    }

    private void DeleteForm(Int32 id)
    {
        AspDotNetStorefront.Promotions.Data.Promotion promotion = AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.Promotions.FirstOrDefault(l => l.Id == id);
        if (promotion == null)
            return;

        AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.PromotionUsages.DeleteAllOnSubmit(AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.PromotionUsages.Where(pu => pu.PromotionId == promotion.Id));
        AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.Promotions.DeleteOnSubmit(promotion);
        AspDotNetStorefront.Promotions.Data.DataContextProvider.Current.SubmitChanges();

        BindData();
        CancelForm();
    }

    private void CancelForm()
    {
        ClearForm();
        ToggleView(false, false);
    }

    private void ClearForm()
    {
        lblError.Text = String.Empty;

        txtId.Value =
            lblTitle.Text =
            txtName.Text =
            txtDescription.Text =
            txtUsageText.Text =
            txtCode.Text =
            lblEmailUploadError.Text =
			CallToActionTextbox.Text = String.Empty;

        txtPriority.Text = "1";

        ssPromotion.SelectedStoreIDs = new Int32[0];

        chkAutoAssigned.Checked = false;
        rblActive.SelectedValue = "true";

        //StartDatePromotionRule
        optRuleStartDate.Checked = false;
        txtRuleStartDate.SelectedDate = DateTime.Now;

        //ExpirationDatePromotionRule
        optRuleExpirationDate.Checked = false;
        txtRuleExpirationDate.SelectedDate = DateTime.Now;

        //ExpirationNumberOfUsesPromotionRule
		optRuleExpirationNumberOfUses.Checked = false;
        chkRuleExpirationNumberOfUsesPerCustomer.Checked = false;
        txtRuleExpirationNumberOfUses.Text = String.Empty;

        //ExpirationNumberOfUsesPerCustomerPromotionRule
		optRuleExpirationNumberOfUses.Checked = false;
        chkRuleExpirationNumberOfUsesPerCustomer.Checked = false;
        txtRuleExpirationNumberOfUses.Text = String.Empty;

        //ShippingMethodIdPromotionRule
        txtRuleShippingMethodID.Text = String.Empty;

        //EmailAddressPromotionRule
        chkRuleEmail.Checked = false;
        txtRuleEmailAddresses.Text = String.Empty;

        //MinimumCartAmountPromotionRule
        chkRuleCartAmount.Checked = false;
        txtRuleCartAmount.Text = String.Empty;

        //CustomerLevelPromotionRule
        chkRuleCustomerLevel.Checked = false;
        txtRuleCustomerLevels.Text = String.Empty;

        //CategoryPromotionRule
        chkRuleCategories.Checked = false;
        txtRuleCategories.Text = String.Empty;

        //SectionPromotionRule
        chkRuleSections.Checked = false;
        txtRuleSections.Text = String.Empty;

        //ManufacturerPromotionRule
        chkRuleManufacturers.Checked = false;
        txtRuleManufacturers.Text = String.Empty;

        //ProductIdPromotionRule
        chkRuleProductId.Checked = false;
        rblProductsAllOrAny.SelectedIndex = 0;
		txtRuleProductIds.Text = String.Empty;
        txtRuleProductIdsRequireQuantity.Text = String.Empty;

        //StatePromotionRule
        chkRuleState.Checked = false;
        txtRuleStates.Text = String.Empty;

        //ZipCodePromotionRule
        chkRuleZipCode.Checked = false;
        txtRuleZipCodes.Text = String.Empty;

        //CountryPromotionRule
        chkRuleCountryCodes.Checked = false;
        txtRuleCountryCodes.Text = String.Empty;

        //MinimumOrdersPromotionRule
        chkRuleMinimumOrders.Checked = false;
        txtRuleMinimumOrders.Text = String.Empty;
        txtRuleMinimumOrdersCustomEndDate.SelectedDate =
            txtRuleMinimumOrdersCustomStartDate.SelectedDate = DateTime.Now;

        //MinimumOrderAmountPromotionRule
        chkRuleMinimumOrderAmount.Checked = false;
        txtRuleMinimumOrderAmount.Text = String.Empty;
        txtRuleMinimumOrderAmountCustomStartDate.SelectedDate =
            txtRuleMinimumOrderAmountCustomEndDate.SelectedDate = DateTime.Now;

        //MinimumProductsOrderedPromotionRule
        chkRuleMinimumProductsOrdered.Checked = false;
        txtRuleMinimumProductsOrdered.Text =
            txtRuleMinimumProductsOrderedProductIds.Text = String.Empty;
        
        txtRuleMinimumProductsOrderedCustomStartDate.SelectedDate =
            txtRuleMinimumProductsOrderedCustomEndDate.SelectedDate = DateTime.Now;

        //MinimumProductAmountOrderedPromotionRule
        chkRuleMinimumProductsOrderedAmount.Checked = false;
        txtRuleMinimumProductsOrderedAmount.Text =
            txtRuleMinimumProductsOrderedAmountProductIds.Text = String.Empty;

        txtRuleMinimumProductsOrderedAmountCustomStartDate.SelectedDate =
            txtRuleMinimumProductsOrderedAmountCustomEndDate.SelectedDate = DateTime.Now;
            
        //ShippingPromotionDiscount
        chkRuleShippingDiscount.Checked = false;
        ddlRuleShippingDiscountType.ClearSelection();
        txtRuleShippingDiscountAmount.Text =
            txtRuleShippingDiscountAmount.Text = String.Empty;

        //FreeProductPromotionDiscount
        chkRuleGiftWithPurchase.Checked =
            chkMatchQuantites.Checked =
            chkRuleGiftWithPurchase.Checked =
            chkMatchQuantites.Checked = false;

        txtGiftWithPurchaseDiscountAmount.Text = 
            txtRuleGiftWithPurchaseProductId.Text =
            txtGiftWithPurchaseDiscountAmount.Text = String.Empty;

        //ShippingOnlyDiscount
        chkShippingOnlyDiscount.Checked = false;

        //OrderPromotionDiscount
        chkRuleOrderDiscount.Checked = false;
        ddlRuleOrderDiscountType.ClearSelection(); 
        txtRuleOrderDiscountAmount.Text =
            txtRuleOrderDiscountAmount.Text = String.Empty;

        //OrderItemPromotionDiscount
        chkRuleLineItemDiscount.Checked = false;
        ddlRuleLineItemDiscountType.ClearSelection();
        txtRuleLineItemDiscountAmount.Text =
            txtRuleLineItemDiscountAmount.Text = String.Empty;
    }

    private void NotifyOfError(String errorMessage)
    {
        lblError.Text += errorMessage + "<br />" + Environment.NewLine;
    }

    private String GetFileName()
    {
        return ADNSF.CommonLogic.SafeMapPath("../images/PromoEmailImport_" + ADNSF.Localization.ToNativeDateTimeString(System.DateTime.Now).Replace(" ", "").Replace("/", "").Replace(":", "").Replace(".", "") + ".csv");
    }

    private void SaveFile(FileUpload fileUpload, String filename)
    {
        if (fileUpload == null)
            throw new ArgumentNullException("fileUpload", "The file uploaded appeared to be empty.");

        if (String.IsNullOrEmpty(filename))
            throw new ArgumentException("filename", "The expected name of the file has been lost and cannot be imported.");

        if (fileUpload.PostedFile.ContentLength <= 0)
            return;

        fileUpload.PostedFile.SaveAs(filename);
        if (!File.Exists(filename))
            throw new InvalidOperationException("The file could not be saved to disk prior to importing.");
    }

    private void ImportFile(String filename)
    {
        if (!File.Exists(filename))
            throw new InvalidOperationException("The file to import no longer exists.");

        Int32 lineIdx = 0;
        Int32 emailColIdx = 0;


        String[] lines = File.ReadAllLines(filename);
        if (lines.Length < 2)
            throw new InvalidOperationException("The import file does not appear to have the first row as the header and at least one more row to import.");

        String[] header = lines[0].Split(new char[] { ',' });
        if (header.Length != 1)
            throw new InvalidOperationException("The import file requires an Email Address column.");

        StringBuilder emailAddresses = new StringBuilder();

        foreach (String line in lines)
        {
            lineIdx++;
            if (lineIdx == 1)
                continue;

            String[] columns = line.Split(new char[] { ',' });
            if (columns.Length != 1)
                throw new InvalidOperationException("The import file must have exactly 1 column: Email Address.");

            String emailAddress = columns[emailColIdx];
            emailAddresses.Append(emailAddress);
            if (lineIdx != lines.Count<String>())
            {
                emailAddresses.Append(",");
            }
        }
        txtRuleEmailAddresses.Text = emailAddresses.ToString();
    }

    #endregion
}
