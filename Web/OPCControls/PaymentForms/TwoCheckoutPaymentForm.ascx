<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TwoCheckoutPaymentForm.ascx.cs"
    Inherits="OPCControls_PaymentForms_TwoCheckoutPaymentForm" %>
<div class="paymentMethodContents">
    <asp:Panel runat="server" ID="PanelTwoCheckoutInfo" CssClass="checkout-block">
        <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("checkouttwocheckout.aspx.2") %>'/>
    </asp:Panel>
</div>
