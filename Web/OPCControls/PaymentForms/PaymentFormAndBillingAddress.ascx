<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PaymentFormAndBillingAddress.ascx.cs" Inherits="OPCControls_PaymentFormAndBillingAddress" %>
<%@ Register Src="../Addresses/BillingAddressChoice.ascx" TagName="BillingAddressChoice" TagPrefix="uc1" %>
<%@ Register Src="CheckPaymentForm.ascx" TagName="CheckPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="PayPalPaymentForm.ascx" TagName="PayPalPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="PurchaseOrderPaymentForm.ascx" TagName="PurchaseOrderPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="PayPalExpressOPCPaymentForm.ascx" TagName="PayPalExpressOPCPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="RequestAQuotePaymentForm.ascx" TagName="RequestAQuoteForm" TagPrefix="uc1" %>
<%@ Register Src="MicroPayPaymentForm.ascx" TagName="MicroPayPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="CodPaymentForm.ascx" TagName="CodPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="CheckOutByAmazonPaymentForm.ascx" TagName="CheckOutByAmazonPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="PayPalEmbeddedCheckoutPaymentForm.ascx" TagName="PayPalEmbeddedCheckoutForm" TagPrefix="uc1" %>
<%@ Register Src="MoneybookersQuickCheckoutPaymentForm.ascx" TagName="MoneybookersQuickCheckoutPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="SecureNetPaymentForm.ascx" TagName="SecureNetPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="TwoCheckoutPaymentForm.ascx" TagName="TwoCheckoutPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="WorldPayPaymentForm.ascx" TagName="WorldPayPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="ECheckPaymentForm.ascx" TagName="ECheckPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="CreditCardPaymentForm.ascx" TagName="CreditCardPaymentForm" TagPrefix="uc1" %>
<%@ Register Src="FirstPayEmbeddedCheckoutPaymentForm.ascx" TagName="FirstPayForm" TagPrefix="uc1" %>

<asp:UpdatePanel UpdateMode="Conditional" ChildrenAsTriggers="false" ID="UpdatePanelPaymentFormAndBillingAddress"
    runat="server">
    <ContentTemplate>
        <div class="paymentMethodContents subSection">
            <asp:Panel ID="PnlBillingAddressChoice" runat="server">
                <uc1:BillingAddressChoice ID="BillingAddressChoice" runat="server" />
            </asp:Panel>
            <asp:Panel runat="server" ID="PanelPaymentMethod" CssClass="addACardWrap">
                <uc1:SecureNetPaymentForm ID="SecureNetPaymentView" runat="server" Visible="false" />
                <uc1:TwoCheckoutPaymentForm ID="TwoCheckoutPaymentView" runat="server" Visible="false" />
                <uc1:CreditCardPaymentForm ID="CreditCardPaymentForm" runat="server" Visible="false" />
                <uc1:FirstPayForm ID="FirstPayPaymentForm" runat="server" Visible="false" />
                <uc1:CheckPaymentForm ID="CheckPaymentView" runat="server" Visible="false" />
                <uc1:RequestAQuoteForm ID="RequestQuoteView" runat="server" Visible="false" />
                <uc1:PayPalPaymentForm ID="PayPalPaymentView" runat="server" Visible="false" />
                <uc1:PurchaseOrderPaymentForm ID="PurchaseOrderPaymentView" runat="server" Visible="false" />
                <uc1:PayPalExpressOPCPaymentForm ID="PayPalExpressView" runat="server" Visible="false" />
                <uc1:MicroPayPaymentForm ID="MicroPayPaymentView" runat="server" Visible="false" />
                <uc1:CodPaymentForm ID="CodPaymentView" runat="server" Visible="false" />
                <uc1:CheckOutByAmazonPaymentForm ID="CheckOutByAmazonPaymentView" runat="server" Visible="false" />
                <uc1:PayPalEmbeddedCheckoutForm ID="PayPalEmbeddedCheckoutPaymentView" runat="server" Visible="false" />
                <uc1:MoneybookersQuickCheckoutPaymentForm ID="MoneybookersQuickCheckoutPaymentView" runat="server" Visible="false" />
                <uc1:ECheckPaymentForm ID="ECheckPaymentView" runat="server" Visible="false" />
                <uc1:WorldPayPaymentForm ID="WorldPayPaymentView" runat="server" Visible="false" />
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
