<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PayPalEmbeddedCheckoutPaymentForm.ascx.cs"
    Inherits="OPCControls_PaymentForms_PayPalEmbeddedCheckoutPaymentForm" %>
<div class="paymentMethodContents">
	<asp:Panel ID="PanelError" runat="server" Visible="false" CssClass="error-wrap">
		<asp:Label ID="ErrorMessage" runat="server" CssClass="error-large" />
	</asp:Panel>
    <asp:Literal ID="litPayPalEmbeddedCheckoutFrame" runat="server" />
</div>
