<%@ Page Language="c#" Inherits="AspDotNetStorefront.ShoppingCartPage" CodeFile="ShoppingCart.aspx.cs" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BuySafeKicker" Src="~/controls/BuySafeKicker.ascx" %>
<asp:Content ID="PageContent" runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <%--<asp:Panel ID="pnlContent" runat="server">--%>
    <div class="content-box-03">
        <%--<h1>
				<asp:Literal ID="Literal1" Text="<%$ Tokens:StringResource,Header.ShoppingCart %>" runat="server" />
			</h1>--%>
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
                <div class="td-45-percent pull-left"></div>
                <div class="td-30-percent cart-btn-adjust pull-left">
                    <asp:Button ID="btnContinueShoppingTop" Text="<%$ Tokens:StringResource,shoppingcart.cs.62 %>"
                        CssClass="btn btn-primary btn-block margin-none" runat="server" />
                </div>
                <div class="td-25-percent pull-left">
                    <asp:Button ID="btnCheckOutNowTop" Text="<%$ Tokens:StringResource,shoppingcart.cs.111 %>"
                        runat="server" CssClass="btn btn-primary btn-block margin-none" />
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
                <div class="td-25-percent pull-right">
                    <asp:Panel ID="pnlSubTotals" runat="server">
                        <div class="page-row row-sub-totals">
                            <aspdnsf:CartSummary ID="ctrlCartSummary" runat="server" CalculateShippingDuringCheckout="false"
                                CalculateTaxDuringCheckout="false" CalculateShippingDuringCheckoutText='<%$Tokens:StringResource, shoppingcart.aspx.13 %>'
                                CalculateTaxDuringCheckoutText='<%$Tokens:StringResource, shoppingcart.aspx.15 %>'
                                SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>' SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
                                ShippingCaption='<%$Tokens:StringResource, shoppingcart.aspx.12 %>' SkipShippingOnCheckout='<%$Tokens:AppConfigBool, SkipShippingOnCheckout %>'
                                TaxCaption='<%$Tokens:StringResource, shoppingcart.aspx.14 %>' ShowTotal="true" TotalCaption='<%$Tokens:StringResource, admin.common.Total %>'
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
                        <asp:Button ID="btnUpdateShoppingCart" CssClass="btn btn-primary btn-block" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>"
                            runat="server" ValidationGroup="val" />
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
</asp:Content>
