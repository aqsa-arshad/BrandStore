<%@ Page Language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefront.checkoutpayment" CodeFile="checkoutpayment.aspx.cs" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BuySafeKicker" Src="~/controls/BuySafeKicker.ascx" %>
<%@ Register Src="CIM/WalletSelector.ascx" TagName="WalletSelector" TagPrefix="uc1" %>
<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Label ID="hdnPOChecked" name="hdnPOChecked" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnMinThreshHold" name="hdnMinThreshHold" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="lblInvoiceValue" name="lblInvoiceValue" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel ID="pnlContent" runat="server" CssClass="padding-none">
        <asp:Label ID="btnreqfrom" CssClass="hide-element" runat="server"></asp:Label>
        <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />
        <div class="content-box-03">
            <h4 class="black-color margin-top-none">Payment</h4>
            <div id="divmainrow">

                <%-- <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />--%>
                <%-- <h1>
                <asp:Literal ID="ltAccount" Text="<%$ Tokens:StringResource,Header.PaymentInformation %>" runat="server" />
            </h1>--%>
                <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
                <asp:Panel ID="pnlErrorMsg" runat="server" Visible="false">
                    <div class="error-wrap">
                        <asp:Label ID="ErrorMsgLabel" CssClass="error-large" runat="server"></asp:Label>
                    </div>
                </asp:Panel>
                <asp:Label ID="lblNetaxeptErrorMsg" CssClass="error-large" Visible="false" runat="server"></asp:Label>
                <asp:ValidationSummary DisplayMode="List" ID="valsumCreditCard" ShowMessageBox="false" runat="server" ShowSummary="true" ValidationGroup="creditcard" CssClass="error-wrap error-large" />
                <asp:ValidationSummary DisplayMode="List" ID="valsumEcheck" ShowMessageBox="false" runat="server" ShowSummary="true" ValidationGroup="echeck" ForeColor="red" Font-Bold="true" CssClass="error-wrap error-large" />
                <asp:Panel ID="pnlCCTypeErrorMsg" runat="server" Visible="false">
                    <div class="error-wrap">
                        <asp:Label ID="CCTypeErrorMsgLabel" CssClass="error-large" runat="server" Text="<%$ Tokens:StringResource, address.cs.19 %>"></asp:Label>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlCCExpDtErrorMsg" runat="server" Visible="false">
                    <div class="error-wrap">
                        <asp:Label ID="CCExpDtErrorMsg" CssClass="error-large" runat="server" Text="<%$ Tokens:StringResource, checkoutcard_process.aspx.2 %>"></asp:Label>
                    </div>
                </asp:Panel>

                <aspdnsf:Topic runat="server" ID="CheckoutPaymentPageHeader" TopicName="CheckoutPaymentPageHeader" />
                <asp:Literal ID="XmlPackage_CheckoutPaymentPageHeader" runat="server" Mode="PassThrough"></asp:Literal>

                <asp:Panel ID="pnlNoPaymentRequired" CssClass="col-md-12" runat="server" Visible="false">
                    <p></p>
                    <asp:Label ID="NoPaymentRequired" runat="server" CssClass="info-message" /></p>                       
                        <asp:Literal ID="Finalization" runat="server" Mode="PassThrough"></asp:Literal>
                    <div class="clearfix"></div>
                    <asp:Button ID="btnContinueCheckOut1" runat="server" Text="<%$ Tokens:StringResource,checkoutpayment.aspx.18 %>" CssClass="btn btn-primary td-30-percent" />


                </asp:Panel>

                <asp:Label ID="lbl1" ClientIDMode="Static" runat="server" Text="<%$ Tokens:StringResource,choosepaymentmethod%>"></asp:Label>
                <asp:Panel ID="pnlPaymentOptions" runat="server" HorizontalAlign="left" CssClass="checkout-tablet-view pull-left-md" Visible="true">
                    <div>
                        <aspdnsfc:PaymentMethod CssClass="payment-method" ID="ctrlPaymentMethod" runat="server"
                            OnPaymentMethodChanged="ctrlPaymentMethod_OnPaymentMethodChanged"
                            CARDINALMYECHECKCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.13 %>"
                            CHECKBYMAILCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.11 %>"
                            CODCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.12 %>"
                            CODCOMPANYCHECKCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.22 %>"
                            CODMONEYORDERCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.24 %>"
                            CODNET30Caption="<%$ Tokens:StringResource, checkoutpayment.aspx.23 %>"
                            CREDITCARDCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.7 %>"
                            ECHECKCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.13 %>"
                            MICROPAYCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.14 %>"
                            MICROPAYLabel="<%$ Tokens:StringResource, checkoutpayment.aspx.26 %>"
                            AMAZONSIMPLEPAYCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.31 %>"
                            PAYPALCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.9 %>"
                            PAYPALEXPRESSCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.25 %>"
                            PAYPALEXPRESSLabel="<%$ Tokens:StringResource, checkoutpaypal.aspx.2 %>"
                            PAYPALLabel="<%$ Tokens:StringResource, checkoutpaypal.aspx.2 %>"
                            PURCHASEORDERCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.8 %>"
                            REQUESTQUOTECaption="<%$ Tokens:StringResource, checkoutpayment.aspx.10 %>"
                            MONEYBOOKERSQUICKCHECKOUTCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.32 %>"
                            MONEYBOOKERSQUICKCHECKOUTLabel="<%$ Tokens:StringResource, checkoutpayment.aspx.33 %>"
                            SECURENETVAULTCaption="<%$ Tokens:StringResource, checkoutpayment.aspx.36 %>"
                            PAYPALHOSTEDCHECKOUTCaption='<%$ Tokens:StringResource, checkoutpayment.aspx.37 %>'
                            CREDITCARDImage_AmericanExpress="~/App_Themes/Skin_1/images/cc_americanexpress.jpg"
                            CREDITCARDImage_Discover="~/App_Themes/Skin_1/images/cc_discover.jpg"
                            CREDITCARDImage_MasterCard="~/App_Themes/Skin_1/images/cc_mastercard.jpg"
                            CREDITCARDImage_Visa="~/App_Themes/Skin_1/images/cc_visa.jpg"
                            CREDITCARDImage_Laser="~/App_Themes/Skin_1/images/cc_laser.gif"
                            CREDITCARDImage_Maestro="~/App_Themes/Skin_1/images/cc_maestro.jpg"
                            CREDITCARDImage_VisaDebit="~/App_Themes/Skin_1/images/cc_visadebit.gif"
                            CREDITCARDImage_VisaElectron="~/App_Themes/Skin_1/images/cc_visaelectron.png"
                            CREDITCARDImage_Jcb="~/App_Themes/Skin_1/images/cc_jcb.gif"
                            CREDITCARDImage_Diners="~/App_Themes/Skin_1/images/cc_diners.gif"
                            PAYPALEXPRESSImage="<%$ Tokens:AppConfig, PayPal.PaymentIcon %>"
                            PAYPALEMBEDDEDCHECKOUTImage="<%$ Tokens:AppConfig, PayPal.PaymentIcon %>"
                            PAYPALImage="<%$ Tokens:AppConfig, PayPal.PaymentIcon %>"
                            MONEYBOOKERSQUICKCHECKOUTImage="<%$ Tokens:AppConfig, Moneybookers.QuickCheckout.PaymentIcon %>" />

                        <aspdnsf:BuySafeKicker ID="buySAFEKicker" WrapperClass="paymentKicker" runat="server" />

                        <asp:Panel ID="pnlFinalization" runat="server" Visible="false" CssClass="page-row">
                            <asp:Literal ID="PMFinalization" Mode="PassThrough" runat="server"></asp:Literal>
                        </asp:Panel>

                        <%-- CIM --%>
                        <asp:Panel ID="PanelWallet" runat="server" Visible="false">
                            <asp:ScriptManagerProxy ID="SMCIM" runat="server"></asp:ScriptManagerProxy>
                            <uc1:WalletSelector ID="CimWalletSelector" runat="server" />
                        </asp:Panel>

                        <%-- CIM End --%>

                        <div id="dv_ctrlCreditCardPanel" class="checkout-tablet-view pull-left-md">
                            <%-- <asp:Panel ID="pnlCCPane" runat="server" Visible="false" CssClass="page-row">--%>

                            <aspdnsfc:CreditCardPanel ID="ctrlCreditCardPanel" runat="server"
                                CreditCardExpDtCaption="<%$ Tokens:StringResource, address.cs.33 %>"
                                CreditCardNameCaption="<%$ Tokens:StringResource, address.cs.23 %>"
                                CreditCardNoSpacesCaption="<%$ Tokens:StringResource, shoppingcart.cs.106 %>"
                                CreditCardNumberCaption="<%$ Tokens:StringResource, address.cs.25 %>"
                                CreditCardTypeCaption="<%$ Tokens:StringResource, address.cs.31 %>"
                                CreditCardVerCdCaption="<%$ Tokens:StringResource, address.cs.28 %>"
                                HeaderCaption="<%$ Tokens:StringResource, checkoutcard.aspx.6 %>"
                                WhatIsThis="<%$ Tokens:StringResource, address.cs.50 %>"
                                CCNameReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.24 %>"
                                CreditCardStartDtCaption="<%$ Tokens:StringResource, address.cs.59 %>"
                                CreditCardIssueNumCaption="<%$ Tokens:StringResource, address.cs.61 %>"
                                CreditCardIssueNumNote="<%$ Tokens:StringResource, address.cs.63 %>"
                                CCNameValGrp="creditcard" CCNumberReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.26 %>"
                                CCNumberValGrp="creditcard" CCVerCdReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.29 %>"
                                CCVerCdValGrp="creditcard" ShowCCVerCd="True" ShowCCStartDtFields="<%$ Tokens:AppConfigBool, ShowCardStartDateFields %>"
                                ShowCCVerCdReqVal="<%$ Tokens:AppConfigBool, CardExtraCodeIsOptional %>"
                                CimSaveCardCaption="<%$ Tokens:StringResource, address.cs.72 %>" />
                            <%-- </asp:Panel>--%>
                        </div>


                        <%--Whats this POP UP Start here --%>
                        <div id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" class="modal fade in" aria-hidden="false" style="display: none">
                            <div class="modal-dialog modal-checkout" role="document">
                                <div class="modal-content">
                                    <div class="modal-body">
                                        <button type="button" id="Closebtn" class="close" data-dismiss="modal" aria-label="Close">
                                            <img src="App_Themes/Skin_3/images/close-popup.png" alt="Closs"></button>
                                        <h4>What’s this</h4>
                                        <p>
                                            <asp:Label runat="server" Text="<%$ Tokens:StringResource, whatsthis.aspx.1 %>"></asp:Label>
                                        </p>
                                        <p>
                                            <asp:Label runat="server" Text="<%$ Tokens:StringResource, whatsthis.aspx.2 %>"></asp:Label>
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <%--Whats this POP UP Start here --%>


                        <asp:Panel ID="pnlEcheckPane" runat="server" Visible="false" CssClass="page-row">
                            <aspdnsfc:Echeck ID="ctrlEcheck" runat="server"
                                ECheckBankABACodeLabel1="<%$ Tokens:StringResource, address.cs.41 %>"
                                ECheckBankABACodeLabel2="<%$ Tokens:StringResource, address.cs.42 %>"
                                ECheckBankAccountNameLabel="<%$ Tokens:StringResource, address.cs.36 %>"
                                ECheckBankAccountNumberLabel1="<%$ Tokens:StringResource, address.cs.44 %>"
                                ECheckBankAccountNumberLabel2="<%$ Tokens:StringResource, address.cs.45 %>"
                                ECheckBankAccountTypeLabel="<%$ Tokens:StringResource, address.cs.47 %>"
                                ECheckBankNameLabel1="<%$ Tokens:StringResource, address.cs.38 %>"
                                ECheckBankNameLabel2="<%$ Tokens:StringResource, address.cs.40 %>"
                                ECheckNoteLabel="<%$ Tokens:StringResource, address.cs.48 %>"
                                BankAccountNameReqFieldErrorMessage="<%$ Tokens:StringResource,address.cs.37 %>"
                                BankNameReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.39 %>"
                                BankABACodeReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.43 %>"
                                BankAccountNumberReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.46 %>"
                                BankAccountNameReqFieldValGrp="echeck" BankNameReqFieldValGrp="echeck"
                                BankABACodeReqFieldValGrp="echeck" BankAccountNumberReqFieldValGrp="echeck" />
                        </asp:Panel>

                        <asp:Panel ID="pnlPOPane" runat="server" Visible="false" CssClass="page-row">

                            <p class="td-25-percent">
                                <asp:Label ID="lblPOHeader" runat="server" CssClass="block-text"
                                    Text="<%$ Tokens:StringResource, checkoutpo.aspx.3 %>"></asp:Label>
                            </p>
                            <div class="clearfix"></div>
                            <asp:Label ID="lblPO" runat="server" CssClass="block-text"
                                Text="<%$ Tokens:StringResource, checkoutpo.aspx.4 %>"></asp:Label>
                            <div class="td-25-percent">
                                <p class="item-space-right">
                                    <asp:TextBox ID="txtPO" AutoCompleteType="Disabled" onpaste="return false" MaxLength="20" ClientIDMode="Static" runat="server"></asp:TextBox>
                                    <%--<asp:RegularExpressionValidator ID="RegularExpressionValidator1" ControlToValidate="txtPO" runat="server" ErrorMessage="Special Characters are not allowed." ValidationExpression="^[0-9a-zA-Z-_\b]+$"></asp:RegularExpressionValidator>--%>
                                </p>
                                <div class="clearfix"></div>
                                <p>
                                    <asp:Label ID="lblPOFee" runat="server" CssClass="block-text black-color"
                                        Text="<%$ Tokens:StringResource, Invoice.Fee %>"></asp:Label>
                                </p>
                            </div>

                        </asp:Panel>

                        <asp:Panel ID="pnlSecureNetVaultPayment" runat="server" CssClass="page-row" Visible="false">
                            <asp:Label ID="lblSecureNetMessage" Visible="false" runat="server" CssClass="error" />
                            <asp:RadioButtonList ID="rblSecureNetVaultMethods" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlPayPalEmbeddedCheckout" runat="server" CssClass="page-row" Visible="false">
                            <asp:Literal ID="litPayPalEmbeddedCheckoutFrame" runat="server" />
                        </asp:Panel>

                        <asp:Panel ID="pnlCardinaleCheckTopic" runat="server" CssClass="page-row hide-element">
                            <aspdnsf:Topic runat="server" ID="CardinaleCheckTopic" TopicName="CardinalMyECheckPageHeader" />
                        </asp:Panel>

                        <asp:Panel ID="pnlCCPaneInfo" runat="server" CssClass="page-row info-message">
                            <asp:Literal ID="CCPaneInfo" Mode="PassThrough" runat="server"></asp:Literal>
                        </asp:Panel>

                        <asp:Panel ID="pnlExternalPaymentMethod" runat="server" CssClass="page-row" Visible="false">
                            <iframe runat="server" id="ExternalPaymentMethodFrame" class="ExternalPaymentMethodFrame" />
                        </asp:Panel>

                        <asp:Panel ID="pnlRequireTerms" runat="server" Visible="<%$ Tokens:AppConfigBool, RequireTermsAndConditionsAtCheckout %>">
                            <asp:Literal ID="RequireTermsandConditions" runat="server"></asp:Literal>
                        </asp:Panel>

                        <asp:Panel ID="pnlTerms" runat="server" Visible="false">
                            <asp:Literal ID="terms" Mode="PassThrough" runat="server"></asp:Literal>
                        </asp:Panel>
                        <asp:Panel ID="pnlAmazonContCheckout" runat="server" Visible="false" HorizontalAlign="Center">
                            <asp:ImageButton ID="ibAmazonSimplePay" runat="server" OnClick="btnAmazonCheckout_Click" />
                        </asp:Panel>


                        <%= new GatewayCheckoutByAmazon.CheckoutByAmazon().RenderWalletWidget("CBAAddressWidgetContainer", false)%>

                        <%-- <asp:Panel ID="pnlContCheckout" runat="server" Visible="true">
                                    <div class="col-md-4"></div>
                                    <div class="col-md-4 col-md-4 checkout-field-adjust">

                                        <button type="submit" id="btnback" class="btn btn-primary btn-block" onclick="open_win()">Back</button>
                                        <div class="clearfix"></div>
                                        <asp:Button ID="btnContCheckout" runat="server" CssClass="btn btn-primary btn-block"
                                            OnClick="btnContCheckout_Click"
                                            Text="<%$ Tokens:StringResource, checkoutpayment.aspx.18 %>" />
                                    </div>
                                </asp:Panel>--%>

                        <%--</div>--%><%--col-md-6--%>
                        <div class="checkout-tablet-view pull-right-md">
                            <div class="checkout-feilds pull-right pull-sm-no">

                                <aspdnsfc:CartSummary ID="ctrlCartSummary" runat="server"
                                    SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>'
                                    SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
                                    ShippingCaption='<%$Tokens:StringResource, shoppingcart.aspx.12 %>'
                                    ShippingVatExCaption='<%$Tokens:StringResource, setvatsetting.aspx.7 %>'
                                    ShippingVatInCaption='<%$Tokens:StringResource, setvatsetting.aspx.6 %>'
                                    TaxCaption='<%$Tokens:StringResource, shoppingcart.aspx.14 %>'
                                    TotalCaption='<%$Tokens:StringResource, shoppingcart.cs.61 %>'
                                    GiftCardTotalCaption='<%$Tokens:StringResource, order.cs.83 %>'
                                    LineItemDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.200 %>" OrderDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.201 %>"
                                    ShowGiftCardTotal="true"
                                    ShowShipping="true"
                                    ShowTax="true"
                                    IncludeTaxInSubtotal="false" />
                            </div>
                        </div>

                        <%--<asp:Panel ID="pnlContCheckout" runat="server" Visible="true">
                                 <div class="col-md-4"></div>
                                    <div class="col-md-4 col-md-4 checkout-field-adjust">

                                        <button type="submit" id="btnback" class="btn btn-primary btn-block" onclick="open_win()">Back</button>
                                        <div class="clearfix"></div>
                                        <asp:Button ID="btnContCheckout" runat="server" CssClass="btn btn-primary btn-block"
                                            OnClick="btnContCheckout_Click"
                                            Text="<%$ Tokens:StringResource, checkoutpayment.aspx.18 %>" />
                                    </div>
                                </asp:Panel>--%>
                    </div>
                </asp:Panel>


                <asp:Panel ID="pnlOrderSummary" runat="server">
                    <%--Style="display: none"--%>
                    <div class="group-header checkout-header" style="display: none">
                        <asp:Literal ID="Literal1" Text="<%$ Tokens:StringResource,Header.ShoppingCart %>" runat="server" />
                    </div>
                    <div class="page-row cart-row" style="display: none">
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
                                AllowShoppingCartItemNotes="false"
                                ShowPicsInCart="true" />
                        </aspdnsfc:ShoppingCartControl>
                    </div>

                    <div class="page-row options-row">
                        <asp:Literal ID="OrderSummary" Mode="PassThrough" runat="server"></asp:Literal>

                        <aspdnsf:OrderOption ID="ctrlOrderOption" runat="server" EditMode="false" />
                    </div>
                    <%-- <div class="page-row summary-row final-total">
                      
                                    <aspdnsfc:CartSummary ID="ctrlCartSummary" runat="server"
                                        SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>'
                                        SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
                                        ShippingCaption='<%$Tokens:StringResource, shoppingcart.aspx.12 %>'
                                        ShippingVatExCaption='<%$Tokens:StringResource, setvatsetting.aspx.7 %>'
                                        ShippingVatInCaption='<%$Tokens:StringResource, setvatsetting.aspx.6 %>'
                                        TaxCaption='<%$Tokens:StringResource, shoppingcart.aspx.14 %>'
                                        TotalCaption='<%$Tokens:StringResource, shoppingcart.cs.61 %>'
                                        GiftCardTotalCaption='<%$Tokens:StringResource, order.cs.83 %>'
                                        LineItemDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.200 %>" OrderDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.201 %>"
                                        ShowGiftCardTotal="true"
                                        ShowShipping="true"
                                        Showtax="true"
                                        IncludeTaxInSubtotal="false" />
                                </div>--%>
                </asp:Panel>

                <aspdnsf:Topic runat="server" ID="CheckoutPaymentPageFooter" TopicName="CheckoutPaymentPageFooter" />
                <asp:Literal ID="XmlPackage_CheckoutPaymentPageFooter" runat="server" Mode="PassThrough"></asp:Literal>
                <div class="col-md-3 hide-element">
                    <%-- <asp:Button ID="btnRequestEstimates" CssClass="btn btn-primary btn-block" Text="Request estimate" runat="server" OnClick="btnRequestEstimates_Click" />--%>
                    <asp:Panel ID="pnlShippingAndTaxEstimator" runat="server" CssClass="shipping-estimator-wrap" Visible="false">
                        <aspdnsfc:ShippingAndTaxEstimateTableControl ID="ctrlEstimate" runat="server" Visible="false" />
                        <aspdnsfc:ShippingAndTaxEstimatorAddressControl ID="ctrlEstimateAddress" runat="server"
                            Visible="false" OnRequestEstimateButtonClicked="EstimateAddressControl_RequestEstimateButtonClicked" />
                    </asp:Panel>
                </div>
                <asp:Panel ID="pnlContCheckout" runat="server" Visible="true" ChildrenAsTriggers="true">
                    <div class="checkout-buttons">
                        <asp:Button type="button" ID="Button1" class="btn btn-primary btn-block btn-success" runat="server" OnClick="btnback_Click" Text="<< Back"></asp:Button>
                    </div>
                    <div class="checkout-buttons pull-right">
                        <asp:Button ID="btnContCheckout" runat="server" CssClass="btn btn-primary btn-block"
                            OnClick="btnContCheckout_Click"
                            Text="<%$ Tokens:StringResource, checkoutpayment.aspx.18 %>" />
                    </div>
                    <div class="clearfix"></div>
                </asp:Panel>
            </div>
    </asp:Panel>



    <script type="text/javascript">
        function open_win() {
            window.history.back();
        }
        $(document).ready(function () {

            $("#divccpane1").unwrap();
            $("#divccpane2").unwrap();

            //if order total is 0 then hide payment label
            if ($('#ctl00_PageContent_pnlPaymentOptions').length <= 0) {
              
                $("#lbl1").hide();
            }
            else {
               
                $("#lbl1").show();
            }

            //disable auto complete of po number text box field
            $("#txtPO").attr("autocomplete", "off");

            //if purchase order is not selected then select credit card option
            SelectRespectivePaymentMethodonLoad();
            //Depending on min threshhold enable disable purchase order option
            EnableDisablePOOption();

            $("#txtPO").keypress(function (e) {
                debugger;
                var regex = new RegExp("^[0-9a-zA-Z-_\b]+$");                
                var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
                if (regex !== "" && regex !== undefined && regex !== null) {
                    if (regex.test(str)) {
                        return true;
                    }

                    else {
                        e.preventDefault();
                        return false;
                    }
                }
            });

        });

        function EnableDisablePOOption() {
            var OrderTotal = $("#ctl00_PageContent_ctrlCartSummary_lblTotal").text().replace("$", "").replace(",", "");
           
            if (OrderTotal < parseFloat($("#hdnMinThreshHold").text())) {
                $("#ctl00_PageContent_ctrlPaymentMethod_rbPURCHASEORDER").attr("Disabled", "Disabled");
                $("#ctl00_PageContent_ctrlPaymentMethod_rbCREDITCARD").trigger("click");
            }
            else
                $("#ctl00_PageContent_ctrlPaymentMethod_rbPURCHASEORDER").removeAttr("Disabled");
        }

        function SelectRespectivePaymentMethodonLoad() {
            if (!$('#ctl00_PageContent_ctrlPaymentMethod_rbPURCHASEORDER').is(':checked') && $("#hdnPOChecked").text() == "0") {

                $("#ctl00_PageContent_ctrlPaymentMethod_rbCREDITCARD").trigger("click");
            }
            else {
                $("#ctl00_PageContent_ctrlPaymentMethod_rbPURCHASEORDER").trigger("click");
                ShowInvoiceFee();
            }

        }
        // aqsa arshad code block
        function ShowInvoiceFee()
        {
            var Invoiceval=$("#lblInvoiceValue").text(); 
            var OrderTotal = $("#ctl00_PageContent_ctrlCartSummary_lblTotal").text().replace("$", "").replace(",", "");
            var total = round(OrderTotal,2)+ round(Invoiceval,2);
           $("#InvoiceFee").removeClass("hide-element");
            $("#ctl00_PageContent_ctrlCartSummary_lblTotal").text("$"+total);
        }
        function round(value, decimals) 
        {
                    if (value == "" || isNaN(value))
                        return 0; 
        
                return Number(Math.round(value+'e'+decimals)+'e-'+decimals);
        }
    </script>
    <script type="text/javascript">
        $(document).ready(function () {
            var lnkWatsThisClientId = '<%= ctrlCreditCardPanel.WhatsThisClientID %>';
            $("#" + lnkWatsThisClientId).click(function () {
                document.getElementById('myModal').style.display = 'block';
            });
            $("#Closebtn").click(function () {
                document.getElementById('myModal').style.display = 'none';
            });

        });
    </script>
</asp:Content>






