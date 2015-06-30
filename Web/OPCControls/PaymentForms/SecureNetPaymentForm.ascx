<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SecureNetPaymentForm.ascx.cs" Inherits="OPCControls_SecureNetPaymentPaymentForm" %>
<div class="paymentMethodContents">
	<p class="secureNetInstructions">
		<%# StringResourceProvider.GetString("smartcheckout.aspx.144") %>
	</p>

	<asp:Panel ID="PanelError" runat="server" CssClass="error-wrap" Visible="false">
		<asp:Label ID="ErrorMessage" runat="server" CssClass="error-large" />
	</asp:Panel>

    <asp:Panel ID="pnlSecureNetVaultPayment" runat="server" CssClass="info-message-box">
        <asp:Label ID="lblSecureNetMessage" Visible="false" runat="server" CssClass="error" />
        <asp:RadioButtonList ID="rblSecureNetVaultMethods" runat="server" AutoPostBack="true" OnSelectedIndexChanged="rblSecureNetVaultMethods_OnSelectedIndexChanged" />
    </asp:Panel>
</div>
