<%@ Page Language="c#" Inherits="AspDotNetStorefront.mobilecheckoutreview" CodeFile="mobilecheckoutreview.aspx.cs" MasterPageFile="~/App_Templates/skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
	<asp:Panel ID="pnlContent" runat="server">
		<asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
		<div>
			<asp:Literal runat="server" ID="CheckoutHeader"></asp:Literal>
			<ul data-role="listview">
				<li class="group" data-role="list-divider"></li>
				<li>
					<asp:Button ID="btnContinueCheckout1" Text="<%$ Tokens:StringResource,checkoutreview.aspx.7 %>" CssClass="ReviewPageContinueCheckoutButton fullwidthshortgreen action" runat="server" OnClick="btnContinueCheckout1_Click" data-icon="check" data-iconpos="right" />
				</li>
			</ul>
			<div data-role="collapsible-set">
				<div data-role="collapsible" data-collapsed="false">
					<h3>
						<asp:Label ID="paymentHeader" Text="<%$ Tokens:StringResource,Mobile.checkoutReview.PaymentHeader %>" runat="server"></asp:Label></h3>
					<div class="accordion_child">
						<asp:Label ID="checkoutreviewaspx9" Text="<%$ Tokens:StringResource,checkoutreview.aspx.9 %>" Font-Bold="true"
							runat="server"></asp:Label>

						<asp:Literal ID="litPaymentMethod" runat="server" Mode="PassThrough"></asp:Literal>
					</div>
				</div>
				<div data-role="collapsible">
					<h3>
						<asp:Label ID="Label1" Text="<%$ Tokens:StringResource,Mobile.checkoutReview.BillingHeader %>" runat="server"></asp:Label></h3>
					<div class="accordion_child">
						<asp:Literal ID="litBillingAddress" runat="server" Mode="PassThrough"></asp:Literal>
					</div>
				</div>
				<div data-role="collapsible">
					<h3>
						<asp:Label ID="shippingHeader" Text="<%$ Tokens:StringResource,Mobile.checkoutReview.ShippingHeader %>" runat="server"></asp:Label></h3>
					<div class="accordion_child" style="text-align: left;">
						<asp:Literal ID="litShippingAddress" runat="server" Mode="PassThrough"></asp:Literal>
					</div>
				</div>
				<div data-role="collapsible">
					<h3>
						<asp:Label ID="orderHeader" Text="<%$ Tokens:StringResource,Mobile.checkoutReview.OrderHeader %>" runat="server"></asp:Label></h3>
					<div id="pnlOrderSummary" style="text-align: left;">
						<%--Shopping cart control--%>
						<aspdnsfc:ShoppingCartControl ID="ctrlShoppingCart"
							ProductHeaderText='<%$ Tokens:StringResource, shoppingcart.product %>'
							QuantityHeaderText='<%$ Tokens:StringResource, shoppingcart.quantity %>'
							SubTotalHeaderText='<%$ Tokens:StringResource, shoppingcart.subtotal %>'
							runat="server" AllowEdit="false">
							<LineItemSettings
								LinkToProductPageInCart='<%$ Tokens:AppConfigBool, LinkToProductPageInCart %>'
								SKUCaption='<%$ Tokens:StringResource, showproduct.aspx.21 %>'
								GiftRegistryCaption='<%$ Tokens:StringResource, shoppingcart.cs.92 %>'
								ItemNotesCaption='<%$ Tokens:StringResource, shoppingcart.cs.86 %>'
								ItemNotesColumns='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaCols %>'
								ItemNotesRows='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaRows %>'
								AllowShoppingCartItemNotes="false" />
						</aspdnsfc:ShoppingCartControl>

						<asp:Literal ID="CartSummary" Mode="PassThrough" runat="server"></asp:Literal>

						<aspdnsf:OrderOption ID="ctrlOrderOption" runat="server" EditMode="false" />
						<div class="final-total">
							<%--Total Summary--%>
							<aspdnsfc:CartSummary ID="ctrlCartSummary" runat="server"
								SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>'
								SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
								ShippingCaption='<%$Tokens:StringResource, shoppingcart.aspx.12 %>'
								ShippingVatExCaption='<%$Tokens:StringResource, setvatsetting.aspx.7 %>'
								ShippingVatInCaption='<%$Tokens:StringResource, setvatsetting.aspx.6 %>'
								TaxCaption='<%$Tokens:StringResource, shoppingcart.aspx.14 %>'
								TotalCaption='<%$Tokens:StringResource, shoppingcart.cs.61 %>'
								GiftCardTotalCaption='<%$Tokens:StringResource, order.cs.83 %>'
								ShowGiftCardTotal="true"
								IncludeTaxInSubtotal="false" />
						</div>
					</div>
				</div>
			</div>
			<ul data-role="listview">
				<li>
					<asp:Button ID="btnContinueCheckout2" Text="<%$ Tokens:StringResource,checkoutreview.aspx.7 %>" CssClass="ReviewPageContinueCheckoutButton fullwidthshortgreen action" runat="server" OnClick="btnContinueCheckout2_Click" data-icon="check" data-iconpos="right" />
				</li>
			</ul>
		</div>
	</asp:Panel>
</asp:Content>
