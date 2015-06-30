<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OnePageCheckout.ascx.cs"
    Inherits="Vortx.OnePageCheckout.OPCControls_OnePageCheckout" %>
<%@ Register Src="MiniCartControl.ascx" TagName="MiniCartControl" TagPrefix="opc" %>
<%@ Register Src="CartSummary.ascx" TagName="CartSummary" TagPrefix="opc" %>
<%@ Register Src="LoginPanel.ascx" TagName="LoginPanel" TagPrefix="opc" %>
<%@ Register Src="Addresses/ShippingAddressStatic.ascx" TagName="ShippingAddressStatic"
    TagPrefix="opc" %>
<%@ Register Src="Addresses/ShippingAddressEdit.ascx" TagName="ShippingAddressEdit"
    TagPrefix="opc" %>
<%@ Register Src="Addresses/ShippingAddressUKEdit.ascx" TagName="ShippingAddressUKEdit"
    TagPrefix="opc" %>
<%@ Register Src="Addresses/ShippingAddressNoZipEdit.ascx" TagName="ShippingAddressNoZipEdit"
    TagPrefix="opc" %>
<%@ Register Src="Addresses/BillingAddressEdit.ascx" TagName="BillingAddressEdit"
    TagPrefix="opc" %>
<%@ Register Src="Addresses/BillingAddressStatic.ascx" TagName="BillingAddressStatic"
    TagPrefix="opc" %>
<%@ Register Src="Addresses/AddressBook.ascx" TagName="AddressBook" TagPrefix="opc" %>
<%@ Register Src="ShippingMethodSelector.ascx" TagName="ShippingMethodSelector" TagPrefix="opc" %>
<%@ Register Src="PaymentMethodSelector.ascx" TagName="PaymentMethodSelector" TagPrefix="opc" %>
<%@ Register Src="CreateAccount.ascx" TagName="CreateAccount" TagPrefix="opc" %>
<%@ Register Src="CustomerService.ascx" TagName="CustomerService" TagPrefix="opc" %>
<%@ Register Src="Content/ContentPanel.ascx" TagName="ContentPanel" TagPrefix="opc" %>
<%@ Register Assembly="Vortx.OnePageCheckout" Namespace="Vortx.OnePageCheckout.WebUtility"
    TagPrefix="opc" %>
<%@ Register Src="Promotions.ascx" TagName="Promotions" TagPrefix="opc" %>
<%@ Register Src="CouponCode.ascx" TagName="CouponCode" TagPrefix="opc" %>

<%@ Register TagPrefix="aspdnsf" TagName="BuySafeKicker" Src="~/controls/BuySafeKicker.ascx" %>

<opc:OPCScriptManagerProxy ID="OPCScriptManagerProxy1" runat="server">
    <Scripts>
        <asp:ScriptReference Path="~/OPCControls/scripts/Address.js" />
        <asp:ScriptReference Path="~/OPCControls/scripts/creditCardTypeSelector.js" />
        <asp:ScriptReference Path="~/OPCControls/scripts/simpleLightBox.js" />
        <asp:ScriptReference Path="~/OPCControls/scripts/focus.js" />
    </Scripts>
</opc:OPCScriptManagerProxy>
<asp:Literal ID="StylesheetLiteral" runat="server"></asp:Literal>
<asp:UpdateProgress ID="OPCUpdateProgress" runat="server">
    <ProgressTemplate>
        <div id="updateProgressBackground" class="update-progress-bg">
        </div>
        <div id="updateProgress" class="update-progress">
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>
<asp:UpdatePanel runat="server" UpdateMode="Conditional" ID="UpdatePanelOnePageCheckoutMain"
    RenderMode="Block" ChildrenAsTriggers="false">
    <Triggers>
        <asp:PostBackTrigger ControlID="SubmitOrder" />
    </Triggers>
    <ContentTemplate>
        <div id="OnePageCheckoutWrap" class="opc-wrap">
            <div id="OPHeader" class="opc-page-header">
                <asp:Literal Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.1") %>' runat="server" />
                <img src="OPCControls/images/SecureLock.gif" alt="" />
                <asp:Literal Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.2") %>' runat="server" />
            </div>
            <div id="OPLeftCol" class="opc-page-left-column">
                <div class="opc-page-column-inner">
                    <asp:Panel runat="server" ID="PanelAccount" CssClass="checkout-block">
                        <opc:LoginPanel ID="LoginView" runat="server" />
                    </asp:Panel>
                    <asp:Panel ID="PanelShippingAddressWrap" runat="server" CssClass="checkout-block">
                        <asp:Panel runat="server" ID="PanelAddressBook" CssClass="modalAddressBookWindow">
                            <div id="ShippingAddressBookBox" class="address-book">
                                <opc:AddressBook ID="AddressBookView" runat="server" />

                                <asp:Button Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.134") %>' ID="ButtonAddressBookClose" CssClass="button opc-button" runat="server" OnClientClick="return hideForm('ShippingAddressBookBox');" />
                            </div>
                        </asp:Panel>
                        <asp:Panel runat="server" ID="PanelShippingAddressHeader">
                            <asp:Label runat="server" ID="ShipAddressHeader" CssClass="checkout-header">
                                <asp:Literal runat="server" ID="ShipAddressHeaderContent" />
                            </asp:Label>
                            <asp:LinkButton ID="HyperLinkShippingAddressBook" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.102") %>'
                                Visible="false" OnClientClick="return showForm('ShippingAddressBookBox','modal-address-background');" />
                        </asp:Panel>
                        <asp:Panel runat="server" ID="PanelShippingAddress" CssClass="subSection">
                            <opc:ShippingAddressEdit ID="ShippingAddressEditView" runat="server" />
                            <opc:ShippingAddressUKEdit ID="ShippingAddressEditUKView" runat="server" />
                            <opc:ShippingAddressNoZipEdit ID="ShippingAddressNoZipEditView" runat="server" />
                            <opc:ShippingAddressStatic ID="ShippingAddressStaticView" runat="server" />
                        </asp:Panel>
                    </asp:Panel>
                    <asp:Panel ID="PanelCheckOutByAmazonShipping" runat="server" CssClass="checkout-block" Visible="false">
                        <div class="form-group">
                            <asp:Literal ID="LitCheckOutByAmazoneShipping" runat="server" />
                            <div style="visibility: hidden;">
                                <asp:Button ID="btnRefreshCBAAddress" CssClass="button opc-button opc-refresh-address-button" runat="server" OnClick="btnRefreshCBAAddress_Click" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="PanelShippingMethod" runat="server" CssClass="checkout-block">
                        <div class="page-row">
                            <asp:Label runat="server" ID="lblShipMethodHeader" CssClass="checkout-header">
                                <asp:Literal Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.3") %>' runat="server" />
                            </asp:Label>
                            <asp:HyperLink ID="HyperLink1" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.135") %>' NavigateUrl="t-shipping.aspx"
                                Target="_blank" Visible="false" />
                        </div>
                        <div class="page-row shipping-methods-wrap">
                            <asp:Panel runat="server" ID="PanelShipMethods">
                                <opc:ShippingMethodSelector ID="ShipMethodView" runat="server" />
                            </asp:Panel>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="PanelCheckboxOver13" runat="server" Visible="false" CssClass="checkout-block">
                        <div class="form over-thirteen-form">
                            <div class="form-group">
                                <asp:CheckBox runat="server" ID="Over13Checkbox" AutoPostBack="true" OnCheckedChanged="Over13Checkbox_CheckedChanged" />
                                <asp:Literal ID="Literal1" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.9") %>' runat="server" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="PanelTerms" Visible="false" CssClass="checkout-block">
                        <div class="form terms-form">
                            <div class="form-group">
                                <asp:CheckBox runat="server" ID="TermsCheckbox" AutoPostBack="true" OnCheckedChanged="TermsCheckbox_CheckedChanged" />
                                <asp:Literal ID="Literal3" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.10") %>' runat="server" />
                            </div>
                            <div class="form-group">
                                <opc:ContentPanel ID="ContentPanelTerms" runat="server" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="PanelPaymentAndBilling" CssClass="checkout-block">
                        <opc:PaymentMethodSelector ID="PaymentView" runat="server" />
                        <asp:Literal ID="LitAmazonPaymentWidget" runat="server" />
                    </asp:Panel>
                    <asp:Panel runat="server" ID="PanelEmailOptIn" CssClass="checkout-block">
                        <div class="form email-opt-form">
                            <asp:Panel runat="server" ID="EmailOptInHeader" CssClass="checkout-header">                  
                                    <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.115") %>' />
                            </asp:Panel>
                            <div class="form-group">
                                <asp:RadioButton GroupName="EmailOptIn" ID="EmailOptInYes" runat="server" Text=""
                                    AutoPostBack="true" OnCheckedChanged="EmailOptInYes_CheckedChanged" />
                                <asp:Literal ID="litEmailPrefYes" runat="server" />
                            </div>
                            <div class="form-group">
                                <asp:RadioButton GroupName="EmailOptIn" ID="EmailOptInNo" runat="server" Text=""
                                    AutoPostBack="true" OnCheckedChanged="EmailOptInNo_CheckedChanged" />
                                <asp:Literal ID="litEmailPrefNo" runat="server" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel runat="server" ID="PanelCreateAccount" CssClass="checkout-block">
                        <opc:CreateAccount ID="CreateAccountView" runat="server" />
                    </asp:Panel>
                    <asp:Panel runat="server" ID="PanelSubmit" CssClass="checkout-block">
                        <asp:Panel runat="server" ID="PanelCompleteCheckout" CssClass="checkout-header">
                            <asp:Literal Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.6") %>' runat="server" />
                        </asp:Panel>
                        <asp:Panel runat="server" ID="PanelCompleteMoneybookersQuickCheckout" Visible="false">
                            <asp:Literal ID="Literal2" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.145") %>' runat="server" />
                        </asp:Panel>
                        <div class="form-submit-wrap">
                            <asp:Button ID="SubmitOrder" runat="server" CssClass="button call-to-action opc-button" OnClientClick="doSubmit(this); return false;" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.133") %>'
                                OnClick="SubmitOrder_OnClick" />
                        </div>
                    </asp:Panel>
                </div>
            </div>
            <div id="OPRightCol" class="opc-page-right-column">
                <div class="opc-page-column-inner">
                    <div id="OPC_CustomerServiceWrap">
                        <opc:CustomerService ID="CustomerServicePanel" runat="server" />
                    </div>
                    <div id="OPC_BuySafeWrap" class="buy-safe-wrap" runat="server">
                        <aspdnsf:BuySafeKicker ID="buySAFEKicker" WrapperClass="shoppingCartKicker" runat="server" />
                    </div>
                    <div id="OPC_PromotionsWrap" class="OPC_PromotionsWrap">
                        <opc:Promotions ID="Promotions" runat="server" />
                    </div>
                    <div id="OPC_CouponCodeWrap" class="OPC_CouponCodeWrap">
                        <opc:CouponCode ID="CouponCode" runat="server" />
                    </div>
                    <div class="opc-container-header">
                        <asp:Label CssClass="opc-container-inner" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.11") %>' runat="server" />
                    </div>
                    <div id="OPC_MiniCartWrap" class="opc-container-body">
                        <div class="opc-container-inner">
                            <asp:Panel runat="Server" ID="PanelMiniCart" CssClass="miniCart">
                                <opc:MiniCartControl ID="MiniCartView" runat="server" />
                            </asp:Panel>
                            <asp:Panel runat="Server" ID="PanelMiniCartSummary" CssClass="mini-cart-summary">
                                <opc:CartSummary ID="MiniCartCartSummary" runat="server" />
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>
            <div class="clear">
            </div>
        </div>
        <asp:Panel ID="pnlPayPalIntegratedCheckout" Visible="false" runat="server">
            <script>
                (function (d, s, id) {
                    var js, ref = d.getElementsByTagName(s)[0];
                    if (!d.getElementById(id)) {
                        js = d.createElement(s); js.id = id; js.async = true;
                        js.src = "//www.paypalobjects.com/js/external/paypal.js";
                        ref.parentNode.insertBefore(js, ref);
                    }
                }(document, "script", "paypal-js"));

                if (parent.location !== window.location) {
                    top.window.location = window.location;
                }
            </script>
        </asp:Panel>
        <script language="javascript" type="text/javascript">
            function doSubmit(btnSubmit) {
                btnSubmit.disabled = 'disabled';
                <%= Page.ClientScript.GetPostBackEventReference(SubmitOrder, string.Empty) %>;
            }

            Sys.Application.add_load(function () {
                if (typeof (cbaAddressWidget) != "undefined")
                    cbaAddressWidget.render('CBAAddressWidgetContainer');

                if (typeof (cbaWalletWidget) != "undefined")
                    cbaWalletWidget.render('CBAWalletWidgetContainer');

                // scroll to current window panel
                scrollToCurrentPanel();

                // set focus to the first input element
                setFirstInputFocus();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
