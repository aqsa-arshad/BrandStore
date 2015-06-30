<%@ Page Language="c#" Inherits="AspDotNetStorefront.checkoutshippingmult" CodeFile="checkoutshippingmult.aspx.cs"
    MasterPageFile="~/App_Templates/skin_1/template.master" MaintainScrollPositionOnPostback="true" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap multi-ship-page">
            <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />

            <h1>
                <asp:Literal ID="ltShippingOptions" Text="<%$ Tokens:StringResource,Header.ShippingOptions %>" runat="server" />
            </h1>
            <%= new GatewayCheckoutByAmazon.CheckoutByAmazon().RenderJSLibrary() %>

            <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>

            <asp:Panel ID="pnlErrorMsg" CssClass="error-wrap" runat="server" Visible="false">
                <asp:Label ID="ErrorMsgLabel" CssClass="error-large" runat="server"></asp:Label>
            </asp:Panel>

            <aspdnsf:Topic runat="server" ID="CheckoutShippingMultPageHeader" TopicName="CheckoutShippingMultPageHeader" />
            <asp:Literal ID="XmlPackage_CheckoutShippingPageHeader" runat="server" Mode="PassThrough"></asp:Literal>
            <asp:Panel ID="pnlGetFreeShipping" runat="server" CssClass="FreeShippingThresholdPrompt" Visible="false">
                <asp:Literal ID="GetFreeShipping" runat="server" Mode="PassThrough"></asp:Literal>
            </asp:Panel>
            <asp:Panel ID="pnlIsFreeShipping" runat="server" CssClass="FreeShippingThresholdPrompt" Visible="false">
                <asp:Literal ID="IsFreeShipping" runat="server" Mode="PassThrough"></asp:Literal>
            </asp:Panel>
            <div class="group-header checkout-header">
                <asp:Literal ID="checkoutshippingmultaspx16" Mode="PassThrough" runat="server"></asp:Literal>
            </div>
            <div class="page-row">
                <asp:Literal ID="checkoutshippingmultaspx18" Mode="PassThrough" runat="server"></asp:Literal>
            </div>
            <asp:Repeater ID="ctrlCartItemAddresses" runat="server" OnItemCommand="ctrlCartItemAddresses_ItemCommand"
                OnItemCreated="ctrlCartItemAddresses_ItemCreated" OnItemDataBound="ctrlCartItemAddresses_ItemDataBound">
                <ItemTemplate>
                    <!-- Individual line item detail goes here -->
                    <div class="page-row mult-shipping-row">
                        <div class="one-half">
							<div class="group-header checkout-header mult-shipping-group-header">
								Product:
							</div>
                            <%-- NOTE: 
                        since repeater control doesn't maintain the current bound item after calling ItemCreated and ItemDataBound 
                        We stash here the ShoppingCartRecordID which we will use as lookup later
                            --%>
                            <input id="hdfShoppingCartRecordID" type="hidden" runat="server" value='<%# Bind("ShoppingCartRecordID") %>' />
                            <asp:Label ID="lblCartItemName" runat="server" Text='<%# Bind("ProductName") %>'></asp:Label>
                            <asp:Literal ID="litCartItemInfo" runat="server"></asp:Literal>
                        </div>
                        <div class="one-half">
							<div class="group-header checkout-header mult-shipping-group-header">
								Ship Item To:
							</div>
                            <input runat="server" type="hidden" name="ShipAddressOption" id="ShipAddressOption" value="true" />
                            <asp:Panel ID="pnlselectAddress" runat="server" CssClass="form-group multi-ship-form-group">
                                <asp:DropDownList ID="cboShipToAddress" runat="server" CssClass="form-control">
                                </asp:DropDownList>
                                <span class="gift-ship-type-wrap">
									<asp:Literal ID="giftShipType" runat="server" />
                                </span>
                                <asp:LinkButton ID="lnkUseGiftRefistryAddress" runat="server" CommandName="UseGiftRegistryAddress"
                                    Text="<%$Tokens:StringResource, checkoutshippingmult.aspx.15  %>" />
                                <asp:LinkButton ID="lnkUseExistingAddress" runat="server" CommandName="UseExistingAddress"
                                    Text="<%$Tokens:StringResource, checkoutshippingmult.aspx.13  %>" />
                                <asp:LinkButton ID="lnkAddNewAddress" runat="server" CommandName="AddNewAddress"
                                    ValidationGroup="SaveAddress" Text='<%$ Tokens:StringResource, address.cs.65 %>' />
                                <asp:Label ID="lblPOBoxError" Visible="true" runat="server" Font-Bold="true" ForeColor="Red" Text="" />
                            </asp:Panel>
                            <asp:Panel ID="pnlNoShipping" runat="server">
                                <asp:Label ID="lblNoShipping" runat="server" Text="Label"></asp:Label>
                            </asp:Panel>

                            <asp:Panel ID="pnlAddNewAddress" runat="server" Visible="false">

                                <aspdnsf:AddressControl ID="ctrlAddress" runat="server" NickNameCaption='<%$ Tokens:StringResource, address.cs.49 %>'
                                    FirstNameCaption='<%$ Tokens:StringResource, address.cs.2 %>' LastNameCaption='<%$ Tokens:StringResource, address.cs.3 %>'
                                    PhoneNumberCaption='<%$ Tokens:StringResource, address.cs.4 %>' CompanyCaption='<%$ Tokens:StringResource, address.cs.5 %>'
                                    ResidenceTypeCaption='<%$ Tokens:StringResource, address.cs.58 %>' Address1Caption='<%$ Tokens:StringResource, address.cs.6 %>'
                                    Address2Caption='<%$ Tokens:StringResource, address.cs.7 %>' SuiteCaption='<%$ Tokens:StringResource, address.cs.8 %>'
                                    CityCaption='<%$ Tokens:StringResource, address.cs.9 %>' StateCaption='<%$ Tokens:StringResource, address.cs.10 %>'
                                    CountryCaption='<%$ Tokens:StringResource, address.cs.53 %>' ZipCaption='<%$ Tokens:StringResource, address.cs.12 %>'
                                    FirstNameRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.13 %>' LastNameRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.14 %>'
                                    CityRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.17 %>' PhoneRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.15 %>'
                                    Address1RequiredErrorMessage='<%$ Tokens:StringResource, address.cs.16 %>'
                                    FirstNameReqFieldValGrp="SaveAddress"
                                    LastNameReqFieldValGrp="SaveAddress"
                                    PhoneNumberReqFieldValGrp="SaveAddress"
                                    CityReqFieldValGrp="SaveAddress"
                                    Address1ReqFieldValGrp="SaveAddress"
                                    Address1CustomValGrp="SaveAddress"
                                    ZipCodeCustomValGrp="SaveAddress"
                                    ShowValidatorsInline="true" Address1ValidationErrorMessage='<%$ Tokens:StringResource, createaccount_process.aspx.3 %>'
                                    Width="100%" Visible="false" />

                            </asp:Panel>
                            <asp:LinkButton ID="btnSaveNewAddress" runat="server" CommandName="SaveNewAddress"
                                Visible="false" ValidationGroup="SaveAddress" Text='<%$ Tokens:StringResource, address.cs.69 %>' />
                            &nbsp;
                        <asp:LinkButton ID="lnkCancelAddNew" runat="server" CommandName="CancelAddNew" Visible="false" Text='<%$ Tokens:StringResource, address.cs.68 %>' />

                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <div>
                <asp:Button ID="btnContinueCheckOut" runat="server" CssClass="button call-to-action" Text='<%$ Tokens:StringResource, checkoutshippingmult.aspx.20 %>' OnClick="btnContinueCheckOut_Click" />
            </div>
            <aspdnsf:Topic runat="server" ID="CheckoutShippingMultPageFooter" TopicName="CheckoutShippingMultPageFooter" />
            <asp:Literal ID="XmlPackage_CheckoutShippingMultPageFooter" runat="server" Mode="PassThrough"></asp:Literal>
        </div>
    </asp:Panel>
</asp:Content>
