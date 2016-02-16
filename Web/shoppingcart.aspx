<%@ Page Language="c#" Inherits="AspDotNetStorefront.ShoppingCartPage" CodeFile="ShoppingCart.aspx.cs" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BuySafeKicker" Src="~/controls/BuySafeKicker.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="PageContent" runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <%--<asp:Panel ID="pnlContent" runat="server">--%>
    <div class="content-box-03">
        <%--<h1>
				<asp:Literal ID="Literal1" Text="<%$ Tokens:StringResource,Header.ShoppingCart %>" runat="server" />
			</h1>--%>
        <%--Hidden Variables Regions--%>


        <asp:Label ID="hdncustomerlevel" name="hdncustomerlevel" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdncurrentrecordid" name="hdncurrentrecordid" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdnpricewithcategoryfundapplied" name="hdnpricewithcategoryfundapplied" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdnBluBucktsPoints" name="hdnBluBucktsPoints" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
         <asp:Label ID="hdnaddblubucks" name="hdnaddblubucks" runat="server" ClientIDMode="Static" Style="display: none" Text="1" />
        <asp:Label ID="hdnProductFundID" name="hdnProductFundID" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdnProductFundAmount" name="hdnProductFundAmount" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdnBudgetPercentValue" name="hdnBudgetPercentValue" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdnProductFundAmountUsed" name="hdnProductFundAmountUsed" EnableViewState="true" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdnsoffundamount" name="hdnsoffundamount" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdndirectmailfundamount" name="hdndirectmailfundamount" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdndisplayfundamount" name="hdndisplayfundamount" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdnliteraturefundamount" name="hdndisplayfundamount" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdnpopfundamount" name="hdnpopfundamount" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
        <asp:Label ID="hdntoreplace" name="hdntoreplace" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />

        <%--End Hidden Variables Regions--%>
        <asp:Literal ID="ltValidationScript" runat="server"></asp:Literal>
        <asp:Literal ID="ltJsPopupRoutines" runat="server"></asp:Literal>
        <aspdnsf:Topic runat="server" ID="topicHeaderMessage" TopicName="CartPageHeader" />
        <asp:Literal ID="ltShoppingCartHeaderXmlPackage" runat="server"></asp:Literal>
        <div class="page-row row-checkout-controls">
            <%--<div class="one-half page-links">
					<div class="page-link-wrap">
						<aspdnsf:BuySafeKicker WrapperClass="buy-safe-kicker" ID="buySafeKicker" runat="server" />
					</div>
					<asp:Panel ID="pnlShippingInformation" runat="server" CssClass="page-link-wrap">
						<a onclick="popuptopicwh('Shipping+Information','shipping',650,550,'yes')" href="javascript:void(0);">
							<asp:Literal ID="shoppingcartaspx8" runat="server"></asp:Literal>
						</a>
					</asp:Panel>
					<div class="page-link-wrap">
						<a onclick="popuptopicwh('Return+Policy+Information','returns',650,550,'yes')" href="javascript:void(0);">
							<asp:Literal ID="shoppingcartaspx9" Text="<%$ Tokens:StringResource,shoppingcart.aspx.9 %>" runat="server"></asp:Literal>
						</a>
					</div>
					<div class="page-link-wrap">
						<a onclick="popuptopicwh('Privacy+Information','privacy',650,550,'yes')" href="javascript:void(0);">
							<asp:Literal ID="shoppingcartaspx10" Text="<%$ Tokens:StringResource,shoppingcart.aspx.10 %>" runat="server"></asp:Literal>
						</a>
					</div>
					<asp:Panel ID="pnlAddresBookLink" runat="server" CssClass="page-link-wrap">
						<a href="address.aspx?returnurl=shoppingcart.aspx&AddressType=Shipping">
							<asp:Literal ID="shoppingcartaspx11" Text="<%$ Tokens:StringResource,shoppingcart.aspx.11 %>" runat="server"></asp:Literal>
						</a>
					</asp:Panel>
				</div>--%>

            <div>
                <div class="td-40-percent pull-left"></div>
                <div class="td-35-percent cart-btn-adjust pull-left pull-sm-no">
                    <asp:Button ID="btnContinueShoppingTop" Text="<%$ Tokens:StringResource,shoppingcart.cs.62 %>"
                        CssClass="btn btn-primary btn-block margin-none" runat="server" />
                </div>
                <div class="td-25-percent pull-left pull-sm-no">
                    <asp:Button ID="btnCheckOutNowTop" Text="<%$ Tokens:StringResource,shoppingcart.cs.111 %>"
                        runat="server" CssClass="btn btn-primary btn-block margin-none margin-top-sm" />
                    <asp:Button ID="btnQuickCheckoutTop" Text="<%$ Tokens:StringResource,shoppingcart.cs.111a %>"
                        Visible="false" runat="server" CssClass="button call-to-action button-checkout-now" />
                    <asp:Button ID="btnInternationalCheckOutNowTop" Visible="false" Text="<%$ Tokens:StringResource,shoppingcart.cs.111b %>"
                        runat="server" CssClass="button call-to-action button-checkout-now  margin-none" />
                </div>
                <div class="clearfix"></div>
            </div>
        </div>
        <%--<div class="row-alt-checkouts" runat="server" id="divAltCheckoutsTop">
				<div class="page-row" runat="server" id="divAmazonCheckoutTop">
					<asp:Literal ID="ltAmazonCheckoutButtonTop" runat="server" />
				</div>
				<div class="page-row" runat="server" id="divPayPalExpressTop">
					<div class="paypal-buttons">
						<div class="paypal-button">
							<asp:ImageButton ImageAlign="Top" CssClass="button-paypal-express" ID="btnPayPalExpressCheckoutTop" runat="server" OnCommand="btnPayPalExpressCheckout_Click" CommandName="payPalCheckoutOptions" CommandArgument="ppec" />
						</div>
						<div class="paypal-button">
							<div class="paypal-bml">
								<asp:ImageButton ImageAlign="Top" CssClass="button-bill-me-later" ID="btnPayPalBillMeLaterTop" runat="server" OnCommand="btnPayPalExpressCheckout_Click" CommandName="payPalCheckoutOptions" CommandArgument="ppbml" />
							</div>
							<div class="clear"></div>
							<asp:Literal ID="ltBillMeLaterMessageTop" runat="server" />
						</div>
						<div class="clear"></div>
						<asp:Literal ID="ltPayPalRecurring" Text="<%$ Tokens:StringResource,shoppingcart.recurring.paypalworks %>" runat="server" />
						<asp:Literal ID="ltPayPalIntegratedCheckout" runat="server" />
					</div>
				</div>
			</div>--%>
        <%--<div class="page-row row-pay-pal-banner">
				<asp:Literal ID="ltPayPalAd" runat="server" />
			</div>--%>
        <%--<div class="page-row row-errors">
				<asp:Panel ID="pnlErrorMessage" runat="Server" Visible="false">
					<div class="error-wrap">
						<asp:Label ID="lblErrorMessage" CssClass="error-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
				<asp:Panel ID="pnlCouponError" runat="Server" Visible="false">
					<div class="error-wrap coupon-error-wrap">
						<asp:Label ID="lblCouponError" CssClass="error-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
				<asp:Panel ID="pnlPromoError" runat="Server" Visible="false">
					<div class="error-wrap promo-error-wrap">
						<asp:Label ID="lblPromotionError" runat="Server" CssClass="error-small" />
					</div>
				</asp:Panel>
				<asp:Panel ID="pnlInventoryTrimmedError" runat="Server" Visible="false">
					<div class="error-wrap inventory-error-wrap">
						<asp:Label ID="lblInventoryTrimmedError" CssClass="error-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
				<asp:Panel ID="pnlRecurringScheduleConflictError" runat="Server" Visible="false">
					<div class="error-wrap recurring-error-wrap">
						<asp:Label ID="lblRecurringScheduleConflictError" CssClass="error-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
				<asp:Panel ID="pnlMinimumQuantitiesUpdatedError" runat="Server" Visible="false">
					<div class="error-wrap min-quan-error-wrap">
						<asp:Label ID="lblMinimumQuantitiesUpdatedError" CssClass="error-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
				<asp:Panel ID="pnlMinimumOrderAmountError" runat="Server" Visible="false">
					<div class="error-wrap order-amount-error-wrap">
						<asp:Label ID="lblMinimumOrderAmountError" CssClass="error-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
				<asp:Panel ID="pnlMinimumOrderQuantityError" runat="Server" Visible="false">
					<div class="error-wrap order-quan-error-wrap">
						<asp:Label ID="lblMinimumOrderQuantityError" CssClass="error-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
				<asp:Panel ID="pnlMaximumOrderQuantityError" runat="Server" Visible="false">
					<div class="error-wrap order-quan-error-wrap">
						<asp:Label ID="lblMaximumOrderQuantityError" CssClass="error-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
			<asp:Panel ID="pnlMicropayEnabledNotice" runat="Server" Visible="false">
					<div class="notice-wrap micropay-notice-wrap">
						<asp:Label ID="lblMicropayEnabledNotice" CssClass="notice-large" runat="Server"></asp:Label>
					</div>
				</asp:Panel>
			</div>--%>
        <asp:Panel ID="pnlShoppingCart" runat="server" DefaultButton="btnUpdateShoppingCart">
            <table class="table">
                <tbody>
                    <aspdnsfc:ShoppingCartControl ID="ctrlShoppingCart" runat="server"
                        AllowEdit="true" ProductHeaderText=''
                        QuantityHeaderText='' SubTotalHeaderText=''
                        OnItemDeleting="ctrlShoppingCart_ItemDeleting">
                        <LineItemSettings LinkToProductPageInCart='<%$ Tokens:AppConfigBool, LinkToProductPageInCart %>'
                            ImageLabelText='<%$ Tokens:StringResource, shoppingcart.cs.1000 %>' SKUCaption='<%$ Tokens:StringResource, showproduct.aspx.21 %>'
                            GiftRegistryCaption='<%$ Tokens:StringResource, shoppingcart.cs.92 %>' ItemNotesCaption='<%$ Tokens:StringResource, shoppingcart.cs.86 %>'
                            ItemNotesColumns='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaCols %>'
                            ItemNotesRows='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaRows %>'
                            AllowShoppingCartItemNotes='<%$ Tokens:AppConfigBool, AllowShoppingCartItemNotes %>'
                            ShowPicsInCart='<%$ Tokens:AppConfigBool, ShowPicsInCart %>' ShowEditButtonInCartForKitProducts='<%$ Tokens:AppConfigBool, ShowEditButtonInCartForKitProducts %>'
                            ShowMultiShipAddressUnderItemDescription="true" />
                    </aspdnsfc:ShoppingCartControl>
                    <script type="text/javascript">
                        (function ($) {
                            $(".quantity-box").change(function () {
                                var quantity = $(this).val();
                                var inventory = $(this).parent().parent().parent().find("input[type=hidden]").val();
                                if (parseInt(quantity) > parseInt(inventory)) {
                                    alert("Your order may be delayed because you have selected more than what is currently in stock. Please select a smaller quantity in your cart if you want to avoid any shipping delays.");
                                }
                            });
                        })(jQuery);
                    </script>
                </tbody>
            </table>
            <div>
                <div class="td-25-percent pull-right pull-sm-no">
                    <asp:Panel ID="pnlSubTotals" runat="server">
                        <div class="page-row row-sub-totals">
                            <aspdnsf:CartSummary ID="ctrlCartSummary" runat="server" CalculateShippingDuringCheckout="false"
                                CalculateTaxDuringCheckout="false" CalculateShippingDuringCheckoutText='<%$Tokens:StringResource, shoppingcart.aspx.13 %>'
                                CalculateTaxDuringCheckoutText='<%$Tokens:StringResource, shoppingcart.aspx.15 %>'
                                SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>' SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
                                ShippingCaption='<%$Tokens:StringResource, shoppingcart.aspx.12 %>' SkipShippingOnCheckout='<%$Tokens:AppConfigBool, SkipShippingOnCheckout %>'
                                TaxCaption='<%$Tokens:StringResource, shoppingcart.aspx.14 %>' ShowTotal="true" TotalCaption='<%$Tokens:StringResource, shoppingcart.cs.61 %>'
                                LineItemDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.200 %>" OrderDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.201 %>"
                                IncludeTaxInSubtotal="False" />
                        </div>

                        <%--<div class="page-row row-shipping-estimator">
					<asp:Button ID="btnRemoveEstimator" CssClass="button button-remove-estimator" runat="server" OnClick="btnRemoveEstimator_Click"
						Text="<%$ Tokens:StringResource,checkoutshipping.estimator.control.remove %>"
						Visible="false" />
				</div>
				<div class="page-row text-right row-request-estimate">
					<asp:Button ID="btnRequestEstimates" CssClass="btn btn-primary btn-block col-md-4" runat="server" OnClick="btnRequestEstimates_Click"
						Visible="false" />
					<asp:Panel ID="pnlShippingAndTaxEstimator" runat="server" CssClass="shipping-estimator-wrap" Visible="false">
						<aspdnsfc:ShippingAndTaxEstimateTableControl ID="ctrlEstimate" runat="server" Visible="false" />
						<aspdnsfc:ShippingAndTaxEstimatorAddressControl ID="ctrlEstimateAddress" runat="server"
							Visible="false" OnRequestEstimateButtonClicked="EstimateAddressControl_RequestEstimateButtonClicked" />
					</asp:Panel>
				</div>--%>
                    </asp:Panel>

                    <div>
                        <asp:ValidationSummary ID="vsQuantity" runat="server" ValidationGroup="val" DisplayMode="List" ShowMessageBox="true" ShowSummary="false" />
                        <asp:Button ID="btnUpdateShoppingCart" CssClass="btn btn-primary btn-block hide-element" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>"
                            runat="server" ValidationGroup="val" />
                        <div class="clearfix"></div>
                        <%--<asp:Button ID="btnContinueShoppingBottom" Text="<%$ Tokens:StringResource,shoppingcart.cs.62 %>"
					CssClass="button button-continue-shopping" runat="server" />--%>
                        <asp:Button ID="btnCheckOutNowBottom" Text="<%$ Tokens:StringResource,shoppingcart.cs.111 %>"
                            runat="server" CssClass="btn btn-primary btn-block" />
                        <asp:Button ID="btnQuickCheckoutBottom" Text="<%$ Tokens:StringResource,shoppingcart.cs.111a %>"
                            runat="server" Visible="false" CssClass="button call-to-action button-checkout-now" />
                        <asp:Button ID="btnInternationalCheckOutNowBottom" Text="<%$ Tokens:StringResource,shoppingcart.cs.111b %>"
                            Visible="False" runat="server" CssClass="button call-to-action button-checkout-now" />
                    </div>
                </div>
                <div class="clearfix"></div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlOrderOptions" runat="server" Visible="false">
            <div class="group-header checkout-header order-options-header hide-element">
                <asp:Literal ID="ltHeaderOrderOptions" Text="<%$ Tokens:StringResource,Header.OrderOptions %>" runat="server" />
            </div>
            <div class="page-row row-order-options">
                <aspdnsf:OrderOption ID="ctrlOrderOption" runat="server" EditMode="true" />
            </div>
            <%--<div class="page-row text-right">
					<asp:Button ID="btnUpdateCartOrderOptions" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>"
						CssClass="button button-update-cart" />
				</div>--%>
        </asp:Panel>
        <%--<asp:Panel ID="pnlSubTotals" runat="server">
				<div class="page-row row-sub-totals">
					<aspdnsf:CartSummary ID="ctrlCartSummary" runat="server" CalculateShippingDuringCheckout="true"
						CalculateTaxDuringCheckout="true" CalculateShippingDuringCheckoutText='<%$Tokens:StringResource, shoppingcart.aspx.13 %>'
						CalculateTaxDuringCheckoutText='<%$Tokens:StringResource, shoppingcart.aspx.15 %>'
						SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>' SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
						ShippingCaption='<%$Tokens:StringResource, shoppingcart.aspx.12 %>' SkipShippingOnCheckout='<%$Tokens:AppConfigBool, SkipShippingOnCheckout %>'
						TaxCaption='<%$Tokens:StringResource, shoppingcart.aspx.14 %>' ShowTotal="False"
						LineItemDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.200 %>" OrderDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.201 %>"
						IncludeTaxInSubtotal="False" />
				</div>--%>
        <div class="page-row row-shipping-estimator hide-element">
            <asp:Button ID="btnRemoveEstimator" CssClass="btn btn-primary btn-block" runat="server" OnClick="btnRemoveEstimator_Click"
                Text="<%$ Tokens:StringResource,checkoutshipping.estimator.control.remove %>"
                Visible="false" />
        </div>
        <div class="page-row text-right row-request-estimate hide-element">
            <asp:Button ID="btnRequestEstimates" CssClass="btn btn-primary btn-block col-md-4" runat="server" OnClick="btnRequestEstimates_Click"
                Visible="false" />
            <asp:Panel ID="pnlShippingAndTaxEstimator" runat="server" CssClass="shipping-estimator-wrap" Visible="false">
                <aspdnsfc:ShippingAndTaxEstimateTableControl ID="ctrlEstimate" runat="server" Visible="false" />
                <aspdnsfc:ShippingAndTaxEstimatorAddressControl ID="ctrlEstimateAddress" runat="server"
                    Visible="false" OnRequestEstimateButtonClicked="EstimateAddressControl_RequestEstimateButtonClicked" />
            </asp:Panel>
        </div>
        <%--</asp:Panel>--%>
        <asp:Panel ID="pnlUpsellProducts" runat="server" Visible="false">
            <div class="page-row row-upsell">
                <asp:Literal ID="ltUpsellProducts" runat="server"></asp:Literal>
            </div>
            <div class="page-row text-right">
                <asp:Button ID="btnUpdateCartUpsells" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>"
                    CssClass="button button-update-cart" Visible="false" />
            </div>
        </asp:Panel>
        <%--<asp:Panel ID="pnlGiftCard" runat="server" Visible="false" DefaultButton="btnUpdateGiftCard">
				<div class="group-header checkout-header gift-card-header">
					<asp:Literal ID="ltGiftCardHeader" Text="<%$ Tokens:StringResource,Header.GiftCard %>" runat="server" />
				</div>
				<div class="page-row row-gift-card">
					<div class="form gift-card-form">
						<div class="form-group">
							<label>
								<asp:Label ID="shoppingcartcs31" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.117 %>"></asp:Label></label>
							<asp:TextBox ID="txtGiftCard" CssClass="form-control textbox-giftcard" MaxLength="50" runat="server"></asp:TextBox>
							<asp:LinkButton ID="btnRemoveGiftCard" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.aspx.removecoupon %>"
								Visible="false" OnClick="btnRemoveCoupon_Click" />
						</div>
					</div>
				</div>
				<div class="page-row text-right row-gift-card-button">
					<asp:Button CssClass="button button-update-cart" ID="btnUpdateGiftCard" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>" />
				</div>
			</asp:Panel>--%>
        <%--<asp:Panel ID="pnlPromotion" runat="server" DefaultButton="btnAddPromotion">
				<div class="group-header checkout-header promotions-header">
					<asp:Literal ID="ltPromotionCodeHeader" Text="<%$ Tokens:StringResource,Header.PromotionCode %>" runat="server" />
				</div>
				<div class="page-row row-promotions">
					<div class="form">
						<div class="form-group">
							<label>
								<asp:Label ID="shoppingcartcs118" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.118 %>"></asp:Label></label>
							<asp:TextBox ID="txtPromotionCode" CssClass="form-control textbox-promo-code" MaxLength="100" runat="server"></asp:TextBox>
							<asp:RequiredFieldValidator runat="server" ControlToValidate="txtPromotionCode" ValidationGroup="AddPromotion" ErrorMessage="*" Display="Dynamic" />
						</div>
						<div class="form-submit-wrap text-right">
							<asp:Button CssClass="button button-update-cart" ID="btnAddPromotion" runat="server" Text="Add" OnClick="btnAddPromotion_Click" ValidationGroup="AddPromotion" />
						</div>
					</div>
				</div>
				<div class="page-row row-promotions">
					<asp:Repeater ID="rptPromotions" runat="server" OnItemDataBound="rptPromotions_ItemDataBound" OnItemCommand="rptPromotions_ItemCommand">
						<ItemTemplate>
							<div class="notice-wrap promo-wrap">
								<span class="promo-code">
									<%# Eval("Code") %>
								</span>
								<span class="promo-description">
									<%# Eval("Description") %>
								</span>
								<span class="promo-remove">
									<asp:LinkButton ID="lnkRemovePromotion" runat="server" CommandArgument='<%# Eval("Code") %>' CommandName="RemovePromotion" Text="Remove" />
								</span>
							</div>
						</ItemTemplate>
					</asp:Repeater>
				</div>

			</asp:Panel>--%>
        <%--<asp:Panel ID="pnlOrderNotes" runat="server" Visible="false" DefaultButton="btnUpdateCartOrderNotes">
				<div class="group-header checkout-header order-notes-header">
					<asp:Literal ID="ltOrderNoteHeader" Text="<%$ Tokens:StringResource,Header.OrderNotes %>" runat="server" />
				</div>
				<div class="page-row row-order-notes">
					<div class="form">
						<div class="form-group">
							<label>
								<asp:Label ID="lblOrderNotes" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.66 %>"></asp:Label></label>
							<asp:TextBox ID="txtOrderNotes" CssClass="form-control textbox-order-notes" TextMode="MultiLine" runat="server"></asp:TextBox>
						</div>
					</div>
				</div>
				<div class="page-row text-right row-order-notes-button">
					<asp:Button ID="btnUpdateCartOrderNotes" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>"
						CssClass="button button-update-cart" />
				</div>
			</asp:Panel>--%>

        <div class="row-alt-checkouts" runat="server" id="divAltCheckoutsBottom">
            <div class="page-row" runat="server" id="divAmazonCheckoutBottom">
                <asp:Literal ID="ltAmazonCheckoutButtonBottom" runat="server" />
            </div>
            <div class="page-row" runat="server" id="divPayPalExpressBottom">
                <div class="paypal-buttons">
                    <div class="paypal-button">
                        <asp:ImageButton ImageAlign="Top" ID="btnPayPalExpressCheckoutBottom"
                            runat="server" OnCommand="btnPayPalExpressCheckout_Click" CommandArgument="ppec" />
                    </div>
                    <div class="paypal-button">
                        <div class="paypal-bml">
                            <asp:ImageButton ImageAlign="Top" ID="btnPayPalBillMeLaterBottom" Visible="false"
                                runat="server" OnCommand="btnPayPalExpressCheckout_Click" CommandArgument="ppbml" />
                        </div>
                        <asp:Literal ID="ltBillMeLaterMessageBottom" runat="server" />
                    </div>
                    <div class="clear"></div>
                </div>
            </div>
        </div>
    </div>

    <aspdnsf:Topic ID="topicFooterMessage" runat="server" TopicName="CartPageFooter" />
    <asp:Literal ID="ltShoppingCartFooterXmlPackage" runat="server"></asp:Literal>

    <%--</asp:Panel> --%>

    <%-- Region Open Pop Up for bucckts--%>
    <div class="modal fade" id="myModa2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
        <div class="modal-dialog modal-checkout" role="document">
            <div class="modal-content">
                <div class="modal-body">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <img src="App_Themes/Skin_3/images/close-popup.png" alt="Close"></button>
                    <h4 class="text-uppercase-no">APPLY BLU™ BUCKS</h4>
                    <p runat="server" id="ppointscount" clientidmode="Static">You have XXXXXX BLU™ Bucks you can use to purchase items.</p>
                    <p runat="server" id="ppercentage" clientidmode="Static">You can pay for up to XX% of this item's cost with BLU™ Bucks.</p>

                    <div class="form-group">
                        <div class="col-xs-12 padding-none">
                            <label class="roman-black">BLU™ Bucks to be applied:</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="txtBluBuksUsed" onpaste="return false" AutoCompleteType="Disabled" ClientIDMode="Static" MaxLength="10" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>

                        </div>
                        <div class="clearfix"></div>
                    </div>

                    <p class="label-text">
                        <span class="roman-black">Price using BLU™ Bucks:</span>
                        <span id="spprice" runat="server" clientidmode="Static">$0,000.00 </span>
                    </p>
                    <div class="buttons-group trueblue-popup">
                     
                            <asp:Button ID="btnaddtocart" ClientIDMode="Static" CssClass="btn btn-primary btn-block" Text="APPLY" runat="server" OnClick="btnaddtocart_Click" />
                           
                      
                    </div>
                </div>
            </div>
        </div>
    </div>
    <%--End Region Open Pop Up for bucckts--%>

    <%-- Region Open PopUp for SOF Funds--%>
  
        <!-- Modal -->
        <div class="modal fade" id="myModal1" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
            <div class="modal-dialog modal-checkout" role="document">
                <div class="modal-content">
                    <div class="modal-body">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <img src="App_Themes/Skin_3/images/close-popup.png" alt="Close"></button>
                    <h4 class="text-uppercase-no margin-top-none">APPLY SALES FUNDS</h4>
                    <p class="roman-black">Please indicate the amount of available sales funds you would like to apply to this item.</p>

                        <div class="form-group">
                            <div class="row">
                                <%--<div class="col-xs-6 col-sm-7">
                                    <label class="roman-black">GL Code:</label>                                  
                                <asp:TextBox ID="txtGLcode" MaxLength="12" ClientIDMode="Static" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                                </div>--%>
                           <div class="col-md-7">
                                <label class="roman-black">Amount:</label>
                                <asp:TextBox ID="txtproductcategoryfundusedforsalesrep" onpaste="return false" AutoCompleteType="Disabled" MaxLength="7" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <p class="roman-black">Please indicate if this purchase uses specially allocated funds from your vice president. </p>
                    <div class="row form-group">
                            <div class="col-xs-6 col-sm-4">
                                <label class="roman-black">
                                    YES:
                                    <input type="radio" runat="server" ClientIDMode="Static"   name="optionsRadio" id="optionsRadioYes" value="option1" class="radio-btn-group" />
                                </label>
                            </div>
                            <div class="col-xs-4 col-sm-4">
                                <label class="roman-black">
                                    NO:
                                    <input type="radio" runat="server" ClientIDMode="Static" name="optionsRadio" id="optionsRadioNo" value="option2" class="radio-btn-group" />

                                </label>
                            </div>
                    </div>
                        <p class="label-text">                           

                             <span class="roman-black">Total price using sales funds:</span>
                            <span id="sppriceforsalesrep" runat="server" clientidmode="Static">$0,000.00 </span>
                        </p>
                        <div class="buttons-group">
                              <asp:Button ID="btnaddtocartforsalesrep" ClientIDMode="Static" CssClass="btn btn-block btn-primary" Text="APPLY" runat="server" OnClick="btnaddtocartforsalesrep_Click" />
                            
                        </div>
                    </div>
                </div>
            </div>
        </div>
     
  
    <%--End Region Open Pop Up For SOF Funds--%>

    <script type="text/javascript">
        $(document).ready(function () {
            showhidepricelabels();

            $("#txtBluBuksUsed").attr("autocomplete","off");
            $("#txtproductcategoryfundusedforsalesrep").attr("autocomplete","off");

            function showhidepricelabels() {
                var customerlevel = GetCustomerLevel();
                if (customerlevel == 13 || customerlevel == 4 || customerlevel == 5 || customerlevel == 6) {
                    $(".spregularprice").removeClass("hide-element");
                    $(".spfunddiscountprice").removeClass("hide-element");
                    $(".spblubucksprice").removeClass("hide-element");
                }
                else if (customerlevel == 8 || customerlevel == 1) {
                    //all or hidden except price

                }
                else {
                    $(".spregularprice").removeClass("hide-element");
                    $(".spfunddiscountprice").removeClass("hide-element");

                }
            }

            function GetCustomerLevel() {
                var CustomerLevelElemment;
                if (document.getElementById('hdnCustomerLevel')) {
                    CustomerLevelElemment = document.getElementById('hdnCustomerLevel');
                }
                else if (document.all) {
                    CustomerLevelElemment = document.all['hdnCustomerLevel'];
                }
                else {
                    CustomerLevelElemment = document.layers['hdnCustomerLevel'];
                }

                return CustomerLevelElemment.innerHTML;
            }
               
            $("#btnaddtocartforsalesrep").click(function () {               
               
                if($("#txtproductcategoryfundusedforsalesrep").trigger("focusout"))
                    return true;
                    else
                    return false;
            });

            $("#txtproductcategoryfundusedforsalesrep").focusout(function () {

                $("#spprice").text("$" + $("#hdnpricewithcategoryfundapplied").text());
                
                var currentrecordid = $("#hdncurrentrecordid").text();
                var ItemOriginalPrice = $("#spItemPrice_" + currentrecordid).text().replace("$", "").replace("Item Price:", "");
                var quantityfieldid = "#" + $("#hdntoreplace").text() + "txtQuantity";
                var ItemQuantity = $(quantityfieldid).val().replace("$", "");

                var newpricetotal = (ItemOriginalPrice * ItemQuantity);// - $("#hdnProductFundAmountUsed").text();//$("#spprice").text().replace("$", "");// (ItemOriginalPrice * ItemQuantity) - $("#spregularprice_" + currentrecordid).text().replace("$", "").replace("Regular Price: ", "");
               
                var ProductCategoryID = $("#spItemProductCategoryId_" + currentrecordid).text().replace("$", "");
                var BluBucksPercentage = $("#spBluBucksPercentageUsed_" + currentrecordid).text().replace("$", "");
                var customerlevel = $("#hdncustomerlevel").text(); 
                BluBucksPercentage= GetPercentageRatio(customerlevel,ProductCategoryID);

                var spproductcategoryfund = $("#spfunddiscountprice_" + currentrecordid).text()//529;
                var toreplace = spproductcategoryfund.substr(0, spproductcategoryfund.lastIndexOf(":") + 1);
                spproductcategoryfund = spproductcategoryfund.replace(toreplace, "").replace("$", "");

                spproductcategoryfund = round($("#hdnsoffundamount").text(),2) + round(spproductcategoryfund,2)  ;          

                $("#spprice").text("$" +newpricetotal);
                $("#sppriceforsalesrep").text("$" + newpricetotal);

                newpricetotal = $("#spprice").text().replace("$","");
                var sofentered = round($("#txtproductcategoryfundusedforsalesrep").val(),2);

                if (applySOFValidation(newpricetotal, sofentered, spproductcategoryfund)) {
                    $("#spprice").text("$" + ItemQuantity * ItemOriginalPrice);
                    var updatedprice = round($("#spprice").text().replace("$", ""),2) - round($("#txtproductcategoryfundusedforsalesrep").val(),2);
                    $("#spprice").text("$" + round(updatedprice,2));
                    $("#sppriceforsalesrep").text("$" + round(updatedprice,2));
                    var ProductCategoryFundUsed = $("#txtproductcategoryfundusedforsalesrep").val();
                    var BluBucksUsed = 0;
                    PageMethods.SaveValuesInSession(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, onSucceed, onError);// onSucceed, onError
                    return true;
                }
                else {
                    PageMethods.SaveValuesInSession("0", "0", currentrecordid, onSucceed, onError);
                    var updatedprice = $("#spprice").text().replace("$", "")-$("#txtproductcategoryfundusedforsalesrep").val() ;//((ItemOriginalPrice * ItemQuantity) - round($("#hdnProductFundAmountUsed").val(),2));//$("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val();               
                  
                    $("#spprice").text("$" + round(updatedprice,2));                    
                    $("#sppriceforsalesrep").text("$" + round(updatedprice,2));
                    return false;
                }
            });

            function applySOFValidation(newpricetotal, sofentered, spproductcategoryfund) {

                if(spproductcategoryfund <=0)
                {
                $("#txtproductcategoryfundusedforsalesrep").val("0.00");
                  return true;
                }
                if ($("#txtproductcategoryfundusedforsalesrep").val() == "" || isNaN($("#txtproductcategoryfundusedforsalesrep").val())) {
                    $("#txtproductcategoryfundusedforsalesrep").val("0.00");
                    return true;
                  
                }
                else if (round(sofentered,2) > round(spproductcategoryfund,2)) {
                    alert("You exceed available SOF");
                    $("#txtproductcategoryfundusedforsalesrep").val(round($("#hdnProductFundAmountUsed").text(),2) );
                    return false;
                }
                else if (round(sofentered,2) >round(newpricetotal,2)) {
                    alert("You exceed price limit");
                    $("#txtproductcategoryfundusedforsalesrep").val(round($("#hdnProductFundAmountUsed").text(),2) );
                    return false;
                }
                else {
                    return true;
                }
            }

            $("#btnaddtocart").click(function () {              
                
                $("#hdncurrentrecordid").text();
                var currentrecordid = $("#hdncurrentrecordid").text();
                var ItemOriginalPrice = $("#spItemPrice_" + currentrecordid).text().replace("$", "").replace("Item Price:", "");
                var quantityfieldid = "#" + $("#hdntoreplace").text() + "txtQuantity";
                var ItemQuantity = $(quantityfieldid).val().replace("$", "");

                var spfunddiscountprice = $("#spfunddiscountprice_" + currentrecordid).text();
                var toreplace = spfunddiscountprice.substr(0, spfunddiscountprice.lastIndexOf(":") + 1);
                spfunddiscountprice = spfunddiscountprice.replace(toreplace, "").replace("$", "");

                var newpricetotal = (ItemOriginalPrice * ItemQuantity) - $("#hdnProductFundAmountUsed").text();//(ItemOriginalPrice * ItemQuantity) - spfunddiscountprice;


                $("#spprice").text("$" + (newpricetotal.toFixed(2)));
                var ProductCategoryID = $("#spItemProductCategoryId_" + currentrecordid).text().replace("$", "");
                var BluBucksPercentage = $("#spBluBucksPercentageUsed_" + currentrecordid).text().replace("$", "");
                var customerlevel = $("#hdncustomerlevel").text(); 
                BluBucksPercentage = GetPercentageRatio(customerlevel, ProductCategoryID);
              

                var spblubucksprice = $("#spblubucksprice_" + currentrecordid).text().replace("BLU Bucks used:", "").replace("BLU™ Bucks used:","");
                spblubucksprice = parseFloat($("#hdnBluBucktsPoints").text()) +parseFloat(spblubucksprice);
                var availableblubucksforthisitem = spblubucksprice;  
                
             
                if (applyblubuksvalidation(newpricetotal, ProductCategoryID, BluBucksPercentage, availableblubucksforthisitem)) {    
              
                    $("#spprice").text("$" + ($("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val()));
                    var ProductCategoryFundUsed = $("#hdnProductFundAmountUsed").text();
                    var BluBucksUsed = $("#txtBluBuksUsed").val();                   
                    PageMethods.SaveValuesInSession(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, onSucceed, onError);// onSucceed, onError
                    return true;
                }
                else {

                     var updatedprice = (ItemOriginalPrice * ItemQuantity) - $("#hdnProductFundAmountUsed").text();//$("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val();                 
                    $("#spprice").text("$" + round(updatedprice,2));

                    var ProductCategoryFundUsed = $("#hdnProductFundAmountUsed").text();
                    var BluBucksUsed = $("#txtBluBuksUsed").val();                   
                    PageMethods.SaveValuesInSession(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, onSucceed, onError);

                    return false;
                }
                   
            });
          
            $("#txtBluBuksUsed").focusout(function () {
               
                $("#hdncurrentrecordid").text();
                var currentrecordid = $("#hdncurrentrecordid").text();
                var ItemOriginalPrice = $("#spItemPrice_" + currentrecordid).text().replace("$", "").replace("Item Price:", "");
                var quantityfieldid = "#" + $("#hdntoreplace").text() + "txtQuantity";
                var ItemQuantity = $(quantityfieldid).val().replace("$", "");
                //var newpricetotal = $("#spprice").text().replace("$", "");// (ItemOriginalPrice * ItemQuantity) - $("#spregularprice_" + currentrecordid).text().replace("$", "").replace("Regular Price: ", "");

                var spfunddiscountprice = $("#spfunddiscountprice_" + currentrecordid).text();//.replace("(FUND) discount: $", "");
                var toreplace = spfunddiscountprice.substr(0, spfunddiscountprice.lastIndexOf(":") + 1);
                spfunddiscountprice = spfunddiscountprice.replace(toreplace, "").replace("$", "");

                var newpricetotal =  parseFloat(ItemOriginalPrice * ItemQuantity) - parseFloat($("#hdnProductFundAmountUsed").text());    
          
                $("#spprice").text("$" + round(newpricetotal,2));
                var ProductCategoryID = $("#spItemProductCategoryId_" + currentrecordid).text().replace("$", "");
                var BluBucksPercentage = $("#spBluBucksPercentageUsed_" + currentrecordid).text().replace("$", "");
                var customerlevel = $("#hdncustomerlevel").text(); 
                 BluBucksPercentage= GetPercentageRatio(customerlevel,ProductCategoryID);
                var spblubucksprice = $("#spblubucksprice_" + currentrecordid).text().replace("BLU Bucks used:", "").replace("BLU™ Bucks used:","");
                spblubucksprice = parseFloat($("#hdnBluBucktsPoints").text()) + parseFloat(spblubucksprice);
                var availableblubucksforthisitem = spblubucksprice;
                var maxfundlimit = ($("#spprice").text().replace("$", "") * ((BluBucksPercentage) / 100));
                //initially bellow line was commented
               // applyblubuksvalidation2(newpricetotal, ProductCategoryID, maxfundlimit, availableblubucksforthisitem)    ;    

                if (applyblubuksvalidation(newpricetotal, ProductCategoryID, BluBucksPercentage, availableblubucksforthisitem)) {                    
                    var updatedprice = $("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val();
                    $("#spprice").text("$" + round(updatedprice,2));
                    $("#sppriceforsalesrep").text("$" + round(updatedprice.toFixed(2),2));
                    var ProductCategoryFundUsed = $("#hdnProductFundAmountUsed").text();
                    var BluBucksUsed = $("#txtBluBuksUsed").val();                   
                    PageMethods.SaveValuesInSession(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, onSucceed, onError);// onSucceed, onError
                }
                else {  

                    var updatedprice =  $("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val();                 
                    $("#spprice").text("$" + round(updatedprice,2));
                    
                    var ProductCategoryFundUsed = $("#hdnProductFundAmountUsed").text();
                    var BluBucksUsed = $("#txtBluBuksUsed").val();                   
                    PageMethods.SaveValuesInSession(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, onSucceed, onError);

                }


            });

            // CallBack method when the page call success
            function onSucceed(results, currentContext, methodName) {
                
            }
            //CallBack method when the page call fails due to internal, server error 
            function onError(results, currentContext, methodName) {              
                
            }
           
            $('input').keypress(function (e) {
                debugger;
                var regex=new RegExp("(?!^ +$)^.+$");
                var id = $(this).attr('id');
                var toreplace = id.substr(0, id.lastIndexOf("_") + 1);
                id = id.replace(toreplace, "");
                if ($(this).attr('id') == "txtBluBuksUsed" || $(this).attr('id') == "txtproductcategoryfundusedforsalesrep") {
                    if ((e.which != 46 || $(this).val().indexOf('.') != -1) && ((e.which < 48 || e.which > 57) && (e.which != 0 && e.which != 8))) {
                        return false;
                        //event.preventDefault();
                    }

                    var text = $(this).val();

                    if ((text.indexOf('.') != -1) && (text.substring(text.indexOf('.')).length > 2) && (e.which != 0 && e.which != 8) && ($(this)[0].selectionStart >= text.length - 2)) {
                        return false;
                        //event.preventDefault();
                    }
                }
                else if (id == "txtQuantity") {
                    regex = new RegExp("^[0-9\b]+$");
                }

                var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);             
                if (regex !== "" && regex !== undefined && regex !== null) {
                    if (regex.test(str)) {
                        return true;
                    }

                    else {
                        e.preventDefault();
                        return false;
                    }
                }
                
            });

            $("#txtBluBuksUsed").keypress(function (evt) {
                var charCode = (evt.which) ? evt.which : event.keyCode
                if (charCode == 46)
                    return true;
                else if (charCode > 31 && (charCode < 48 || charCode > 57))
                    return false;
                else
                    return true;
            });

            $(".lnkUpdateItem").click(function () {
               
                var id = $(this).attr("id");
                var toreplace = id.substr(0, id.lastIndexOf("_") + 1);
                $("#hdntoreplace").text(toreplace);
                $("#hdncurrentrecordid").text(id.replace(toreplace, ""));
                var currentrecordid = id.replace(toreplace, "");
                var maxInventory = $("#spInventory_" + currentrecordid).text();
                var ItemOriginalPrice = $("#spItemPrice_" + currentrecordid).text().replace("$", "").replace("Item Price:", "");
                var Itemquantityfromcart = $("#spItemQuantity_" + currentrecordid).text();
                var quantityfieldid = "#" + toreplace + "txtQuantity";
                var ItemQuantity = $(quantityfieldid).val().replace("$", "");                
                var newpricetotal = (ItemOriginalPrice * ItemQuantity) //- $("#spregularprice_" + currentrecordid).text().replace("$", "").replace("Regular Price: ", "");
                var ProductCategoryID = $("#spItemProductCategoryId_" + currentrecordid).text().replace("$", "");
                var fuundcheckdecision=$("#spfundcheck_" + currentrecordid).text().replace("$", "");
                var customerlevel = $("#hdncustomerlevel").text();                
                
                var BluBucksPercentage = $("#spBluBucksPercentageUsed_" + currentrecordid).text().replace("$", "");
                BluBucksPercentage= GetPercentageRatio(customerlevel,ProductCategoryID);
                
                var blubuckspercent = "You can pay for up to " + round(BluBucksPercentage,2) + "% of this item's cost with BLU™ Bucks.";
                $("#ppercentage").text(blubuckspercent);                            
               
                
                if (ItemQuantity * 1 > maxInventory) {
                    alert("Your quantity exceeds stock on hand. The maximum quantity that can be added is " + maxInventory + ". Please contact us if you need more information.");
                    $(quantityfieldid).val(maxInventory);
                    return false;
                }
               
              
               
                applyproductcategoryfund(newpricetotal, currentrecordid, customerlevel)
                // $("#txtBluBuksUsed").val("0.00");
                $("#txtBluBuksUsed").val($("#spprice").text().replace("$", ""));
               
                var spblubucksprice = $("#spblubucksprice_" + currentrecordid).text().replace("BLU Bucks used:", "").replace("BLU™ Bucks used:","");
                
                spblubucksprice = parseFloat($("#hdnBluBucktsPoints").text()) + parseFloat(spblubucksprice);

                var availableblubucksforthisitem = spblubucksprice;

                var maxfundlimit = ($("#spprice").text().replace("$", "") * ((BluBucksPercentage) / 100));

                $("#txtBluBuksUsed").val((maxfundlimit));
                applyblubuksvalidation2($("#spprice").text().replace("$", ""), ProductCategoryID, maxfundlimit, availableblubucksforthisitem);
                applyblubuksvalidation($("#spprice").text().replace("$", ""), ProductCategoryID, BluBucksPercentage, availableblubucksforthisitem);

                var updatedprice = $("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val();

                //bellow two lines are important
                $("#spprice").text("$" + updatedprice.toFixed(2));
                $("#sppriceforsalesrep").text("$" + updatedprice.toFixed(2)); 
            
                if (customerlevel == 13 || customerlevel == 4 || customerlevel == 5 || customerlevel == 6) {
                    if (ItemQuantity == 0) {
                        $(".lnkUpdateItem").removeAttr("data-toggle", "modal");
                        $(".lnkUpdateItem").removeAttr("data-target", "#myModa2");
                        $(".lnkUpdateItem").removeAttr("data-target", "#myModal1");
                        PageMethods.Firebtnaddtocartclickevent("1", onSucceed, onError);//, onSucceed, onError
                       $("#btnaddtocart").trigger("click");
                       $
                    }
                    else {
                    
                        if ((availableblubucksforthisitem) > 0 &&  round($("#txtBluBuksUsed").val(),2) > 0) {//&& ($("#spprice").text().replace("$", "")) > 0 
                            //if user have blubucks then show popup otherwise not                             
                            $(".lnkUpdateItem").attr("data-toggle", "modal");
                            $(".lnkUpdateItem").attr("data-target", "#myModa2");
                        }
                        else {
                     
                            $(".lnkUpdateItem").removeAttr("data-toggle", "modal");
                            $(".lnkUpdateItem").removeAttr("data-target", "#myModa2");
                            $(".lnkUpdateItem").removeAttr("data-target", "#myModal1");
                            PageMethods.Firebtnaddtocartclickevent("1", onSucceed, onError);//, onSucceed, onError
                            $("#btnaddtocart").trigger("click");
                        }
                       
                    }
                }
                else if ((customerlevel == 3 || customerlevel == 7)) {

                    if(round($("#hdnProductFundAmountUsed").text(),2)>0)
                    {

                    //bind link update to sof fund opup/internal user

                    if(fuundcheckdecision=="Yes")                   
                     {      
                       $("#optionsRadioNo").prop('checked', false);                                   
                       $("#optionsRadioYes").prop('checked', true);                      
                    }
                    else
                    {  
                           $("#optionsRadioYes").prop('checked', false);  
                           $("#optionsRadioNo").prop('checked', true);
                    }

                    $("#txtproductcategoryfundusedforsalesrep").val(round($("#hdnProductFundAmountUsed").text(),2));
                    $("#sppriceforsalesrep").text("$" + round($("#spprice").text().replace("$", ""),2));
                    $(".lnkUpdateItem").attr("data-toggle", "modal");
                    $(".lnkUpdateItem").attr("data-target", "#myModal1");
                }
                    else
                    {

                        $(".lnkUpdateItem").removeAttr("data-toggle", "modal");
                        $(".lnkUpdateItem").removeAttr("data-target", "#myModa2");
                        $(".lnkUpdateItem").removeAttr("data-target", "#myModal1");
                        $("#txtproductcategoryfundusedforsalesrep").val("0.00");
                        PageMethods.Firebtnaddtocartclickevent("1", onSucceed, onError);//, onSucceed, onError
                        $("#btnaddtocartforsalesrep").trigger("click");
                    }
                }
                else {                       

                        $(".lnkUpdateItem").removeAttr("data-toggle", "modal");
                        $(".lnkUpdateItem").removeAttr("data-target", "#myModa2");
                        $(".lnkUpdateItem").removeAttr("data-target", "#myModal1");
                    PageMethods.Firebtnaddtocartclickevent("1", onSucceed, onError);//, onSucceed, onError
                    $("#btnaddtocart").trigger("click");
                }

            });
           
                function  GetPercentageRatio(customerlevel,ProductCategoryID)            {
                   
                    var percentageration=0;
                        $.ajax({
                        type: "post",
                        url: "Shoppingcart.aspx/GetPercentageRatio",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({
                            "CustomerLevelID": customerlevel,
                            "ProductCategoryID": ProductCategoryID                            
                        }),                        
                        async: false,
                        dataType: "json",
                        success: function (result) {                          
                            
                            percentageration=result.d.toString();
                        },
                        error: function (result) {                      
                               
                        }
            });
                   
                    return percentageration;
                }

            function applyblubuksvalidation(newpricetotal, ProductCategoryID, BluBucksPercentage, availableblubucksforthisitem) {
                
                var maxfundlimit = newpricetotal * ((BluBucksPercentage) / 100)

                 if(($("#spprice").text().replace("$", ""))<=0)
                {
                    $("#txtBluBuksUsed").val(0);
                    return true;
                }
                if ($("#txtBluBuksUsed").val() == "" || isNaN($("#txtBluBuksUsed").val())) {
                    $("#txtBluBuksUsed").val("0.00");                  
                    return true;
                }
                else if (($("#txtBluBuksUsed").val()) > (availableblubucksforthisitem)) {
                    alert("BLU BUCKS cannot be greater than allowed limit");
                  //  $("#txtBluBuksUsed").val(availableblubucksforthisitem);
                    applyblubuksvalidation2(newpricetotal, ProductCategoryID, (maxfundlimit), availableblubucksforthisitem);
                    
                    return false;
                }
                else if ((round($("#txtBluBuksUsed").val(),2)) > (round(maxfundlimit,2))) {
                    
                    alert("BLU™ BUCKS cannot be greater than allowed limit");
                   // $("#txtBluBuksUsed").val(Math.round(maxfundlimit));
                    applyblubuksvalidation2(newpricetotal, ProductCategoryID, (maxfundlimit), availableblubucksforthisitem);
                  
                    return false;
                }                
                else if ((round($("#txtBluBuksUsed").val(),2)) > (round($("#spprice").text().replace("$", ""),2))) {
                    <%--alert("BLU BUKS cannot be greater than allowed limit");--%>
                   // $("#txtBluBuksUsed").val($("#spprice").text().replace("$", "").toFixed(2));
                    applyblubuksvalidation2(newpricetotal, ProductCategoryID, (maxfundlimit), availableblubucksforthisitem);
                   
                    return false;
                }
                else
                {                  
                    return true;
                }
               
            }

            function applyblubuksvalidation2(newpricetotal, ProductCategoryID, maxfundlimit, availableblubucksforthisitem) {    
                
                var min=Math.min(newpricetotal,maxfundlimit,availableblubucksforthisitem);
                $("#txtBluBuksUsed").val((round(min,2)));  
              
           
            }


            function applyproductcategoryfund(newpricetotal, currentrecordid, customerlevel) {

                var ItemFundId = $("#spItemFundId_" + currentrecordid).text();
                var spfunddiscountprice = $("#spfunddiscountprice_" + currentrecordid).text();
                var toreplace = spfunddiscountprice.substr(0, spfunddiscountprice.lastIndexOf(":") + 1);
                spfunddiscountprice = spfunddiscountprice.replace(toreplace, "").replace("$", "");
                var spblubucksprice = $("#spblubucksprice_" + currentrecordid).text().replace("BLU Bucks used:", "").replace("BLU™ Bucks used:","");               
                spblubucksprice = (round($("#hdnBluBucktsPoints").text(),2)) + round((spblubucksprice),2);
              
                var blubucks="You have " + spblubucksprice + " BLU™ Bucks you can use to purchase items."               
                $("#ppointscount").text(blubucks);
              
                var fundamount = 0;
                if (customerlevel == 3) {
                    fundamount = $("#hdnsoffundamount").text();
                    fundamount = round(fundamount,2) + round(spfunddiscountprice,2)   
                    applyFund(newpricetotal, fundamount);
                }
                else if (ItemFundId == 2) {
                    fundamount = $("#hdnsoffundamount").text();
                    fundamount =round(fundamount,2) + round(spfunddiscountprice,2)   
                    applyFund(newpricetotal, fundamount);
                }

                else if (ItemFundId == 3) {
                    fundamount = $("#hdndirectmailfundamount").text();                   
                    fundamount = round(fundamount,2) + round(spfunddiscountprice,2)                                     
                    applyFund(newpricetotal, fundamount);
                }

                else if (ItemFundId == 4) {
                    fundamount = $("#hdndisplayfundamount").text();
                    fundamount =round(fundamount,2) + round(spfunddiscountprice,2) 
                    applyFund(newpricetotal, fundamount);
                }
                else if (ItemFundId == 5) {
                    fundamount = $("#hdnliteraturefundamount").text();
                    fundamount = round(fundamount,2) + round(spfunddiscountprice,2)   
                    applyFund(newpricetotal, fundamount);
                }
                else if (ItemFundId == 6) {
                    fundamount = $("#hdnpopfundamount").text();
                    fundamount = round(fundamount,2) + round(spfunddiscountprice,2)   
                    applyFund(newpricetotal, fundamount);
                }
                else if (ItemFundId == 0) {
                    fundamount = 0;
                    fundamount =round(fundamount,2) + round(spfunddiscountprice,2)   
                    applyFund(newpricetotal, fundamount);
                }

                var ProductCategoryFundUsed =($("#hdnProductFundAmountUsed").text());
                ProductCategoryFundUsed=round(ProductCategoryFundUsed,2);
                var BluBucksUsed = ($("#txtBluBuksUsed").val());
                BluBucksUsed=round(BluBucksUsed,2);

                PageMethods.SaveValuesInSession(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, onSucceed, onError);//, onSucceed, onError
            }

            function applyFund(newpricetotal, fundamount) {

                if (fundamount < newpricetotal) {
                    newpricetotal = newpricetotal - fundamount;
                    $("#hdnProductFundAmountUsed").text(fundamount);
                }
                else {
                    fundamount = fundamount - newpricetotal;
                    $("#hdnProductFundAmountUsed").text(newpricetotal);
                    newpricetotal = 0;

                }
                $("#spprice").text("$" + round(newpricetotal,2));//price to show on popup after deduction of category fund
                $("#sppriceforsalesrep").text("$" + round(newpricetotal,2));//price to show on popup after deduction of category fund for sales rep
                
                $("#hdnpricewithcategoryfundapplied").text(round(newpricetotal,2));
                
            }
            function round(value, decimals) 
            {
                    if (value == "" || isNaN(value))
                        return 0; 
        
                return Number(Math.round(value+'e'+decimals)+'e-'+decimals);
            }

        });


    </script>
</asp:Content>
