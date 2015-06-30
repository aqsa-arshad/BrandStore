<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestAQuotePaymentForm.ascx.cs"
    Inherits="OPCControls_RequestAQuotePaymentForm" %>
<div class="paymentMethodContents">
	<asp:Label runat="server" ID="CheckPaymentMessage" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.59") %>' />
	<asp:Panel ID="PanelError" runat="server" Visible="false" CssClass="error-wrap">
		<asp:Label ID="ErrorMessage" runat="server" CssClass="error-large" />
	</asp:Panel>
</div>
