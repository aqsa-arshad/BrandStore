<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckPaymentForm.ascx.cs"
	Inherits="OPCControls_CheckPaymentForm" %>
<div class="paymentMethodContents">
	<asp:Label runat="server" ID="CheckPaymentMessage" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.41") %>' />
	<asp:Panel ID="PanelError" CssClass="error-wrap" runat="server" Visible="false">
		<asp:Label ID="ErrorMessage" runat="server" CssClass="error-large" />
	</asp:Panel>
</div>
