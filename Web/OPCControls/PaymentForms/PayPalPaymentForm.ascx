<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PayPalPaymentForm.ascx.cs"
	Inherits="OPCControls_PayPalPaymentForm" %>	
<asp:Panel runat="server" ID="PayPalWarningPanel" CssClass="paymentMethodContents">
	<asp:Panel runat="server" ID="PanelPayPalDetails" CssClass="checkout-block">
        <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.54") %>' />
		<div id="paypalWarning">
            <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.55") %>' />
		</div>
	</asp:Panel>
</asp:Panel>
