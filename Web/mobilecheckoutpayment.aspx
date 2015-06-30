<%@ Page language="c#" Inherits="AspDotNetStorefront.mobilecheckoutpayment" CodeFile="mobilecheckoutpayment.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>
<%@ Register Src="CIM/WalletSelector.ascx" TagName="WalletSelector" TagPrefix="uc1" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
    <asp:Panel ID="pnlHeaderGraphic" runat="server" HorizontalAlign="center">
        <asp:Literal runat="server" id="CheckoutHeader"></asp:Literal>
    </asp:Panel>
    <ul data-role="listview">
        <li class="group" data-role="list-divider">
            <asp:Panel ID="pnlErrorMsg" runat="server" Visible="false" CssClass="checkoutErrorWrap">
                <asp:Label ID="ErrorMsgLabel" CssClass="error" runat="server"></asp:Label>
            </asp:Panel>
            <asp:Label ID="lblNetaxeptErrorMsg" style="font-weight: bold;" Visible="false" runat="server"></asp:Label>
            <asp:ValidationSummary DisplayMode="List" ID="valsumCreditCard" ShowMessageBox="false" runat="server" ShowSummary="true" ValidationGroup="creditcard" Font-Bold="true" CssClass="error-wrap error-large"/>
            <asp:ValidationSummary DisplayMode="List" ID="valsumEcheck" ShowMessageBox="false" runat="server" ShowSummary="true" ValidationGroup="echeck" Font-Bold="true" CssClass="error-wrap error-large"/>
            <asp:Panel ID="pnlCCTypeErrorMsg" runat="server" Visible="false">
                <asp:Label ID="CCTypeErrorMsgLabel" style="font-weight: bold;" runat="server" Text="<%$ Tokens:StringResource, address.cs.19 %>">
                    
                </asp:Label>
            </asp:Panel>
            <asp:Panel ID="pnlCCExpDtErrorMsg" runat="server" Visible="false">
                <asp:Label ID="CCExpDtErrorMsg" style="font-weight: bold;" runat="server" Text="<%$ Tokens:StringResource, checkoutcard_process.aspx.2 %>">
                
                </asp:Label>
            </asp:Panel>
            <asp:Label ID="Label1" style="font-weight: bold;" runat="server"></asp:Label>
        </li>
    </ul>
        <asp:Panel ID="pnlNoPaymentRequired" runat="server" HorizontalAlign="Center" Visible="false">
            <ul data-role="listview">
                <li>
                    <asp:Label ID="NoPaymentRequired" runat="server" Font-Bold="true" ForeColor="blue"></asp:Label>
                    
                    <asp:Literal ID="Finalization" runat="server" Mode="PassThrough"></asp:Literal>
                    <asp:Button ID="btnContinueCheckOut1" runat="server" Text="<%$ Tokens:StringResource,checkoutpayment.aspx.18 %>" CssClass="PaymentPageContinueCheckoutButton action" data-icon="check" data-iconpos="right" />
                </li>
            </ul>    
        </asp:Panel>
        <asp:Panel ID="pnlPaymentOptions" runat="server" HorizontalAlign="left" Visible="true">
            <ul data-role="listview"><li>
                <aspdnsfc:PaymentMethod ID="ctrlPaymentMethod" runat="server" 
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
                                PAYPALImage="<%$ Tokens:AppConfig, PayPal.PaymentIcon %>" 
								MONEYBOOKERSQUICKCHECKOUTImage="<%$ Tokens:AppConfig, Moneybookers.QuickCheckout.PaymentIcon %>" />
                            
                <asp:Panel ID="pnlFinalization" runat="server" Visible="false" CssClass ="info-message-box">
                    <asp:Literal ID="PMFinalization" Mode="PassThrough" runat="server"></asp:Literal>
                </asp:Panel>
            </li>
				<li>
					<asp:Panel ID="PanelWallet" runat="server" Visible="false">
					    <asp:ScriptManagerProxy ID="SMCIM" runat="server"></asp:ScriptManagerProxy>
					    <uc1:WalletSelector ID="CimWalletSelector" runat="server" />
					</asp:Panel>
				</li>
			</ul>
        </asp:Panel>
        <ul data-role="listview">
        <li class="group" data-role="list-divider"></li>
        <li>
            <asp:Panel ID="pnlCCPane" runat="server" Visible="false" CssClass ="info-message-box">
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
             </asp:Panel>            
            <asp:Panel ID="pnlEcheckPane" runat="server" Visible="false" CssClass ="info-message-box">
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
           <asp:Panel ID="pnlPOPane" runat="server" Visible="false" CssClass="info-message-box">
                <div><table><tr><td><b>
                    <asp:Label ID="lblPOHeader" runat="server" 
                        Text="<%$ Tokens:StringResource, checkoutpo.aspx.3 %>"></asp:Label>
                </b></td></tr><tr><td>
                    <asp:Label ID="lblPO" runat="server" 
                            Text="<%$ Tokens:StringResource, checkoutpo.aspx.4 %>"></asp:Label>
                    <asp:TextBox ID="txtPO" runat="server"></asp:TextBox>
                </td></tr></table></div>
            </asp:Panel>

            <asp:Panel ID="pnlSecureNetVaultPayment" runat="server" CssClass="info-message-box" Visible="false">
                <asp:Label ID="lblSecureNetMessage" Visible="false" runat="server" CssClass="error" />
                <asp:RadioButtonList ID="rblSecureNetVaultMethods" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlCardinaleCheckTopic" runat="server" CssClass ="info-message-box" >
                <aspdnsf:Topic runat="server" ID="CardinaleCheckTopic" TopicName="CardinalMyECheckPageHeader" />
            </asp:Panel>
            
            <asp:Panel ID="pnlCCPaneInfo" runat="server" CssClass ="info-message-box InfoMessage">
                <asp:Literal ID="CCPaneInfo" Mode="PassThrough" runat="server"></asp:Literal>
            </asp:Panel>
            
			<asp:Panel ID="pnlExternalPaymentMethod" runat="server" CssClass="info-message-box" Visible="false">
				<iframe runat="server" id="ExternalPaymentMethodFrame" class="ExternalPaymentMethodFrame" />
			</asp:Panel>
            <asp:Panel ID="pnlRequireTerms" runat="server" Visible="<%$ Tokens:AppConfigBool, RequireTermsAndConditionsAtCheckout %>" style="width: 99%; border: 0px; border-style: solid; padding-left: 0px; padding-top: 0px; padding-right: 0px; padding-bottom: 0px;">
                <asp:Literal ID="RequireTermsandConditions" runat="server" ></asp:Literal>
            </asp:Panel>
            
            <asp:Panel ID="pnlTerms" runat="server" Visible="false">
                <asp:Literal ID="terms" Mode="PassThrough" runat="server"></asp:Literal>
            </asp:Panel>
            <asp:Panel ID="pnlAmazonContCheckout" runat="server" Visible="false" HorizontalAlign="Center">
                <asp:ImageButton ID="ibAmazonSimplePay" runat="server" OnClick="btnAmazonCheckout_Click" />
            </asp:Panel>        
            <asp:Panel ID="pnlContCheckout" runat="server" Visible="true" HorizontalAlign="Center">
                    <asp:Button ID="btnContCheckout" runat="server" class="PaymentPageContinueCheckoutButton action"
                        onclick="btnContCheckout_Click"  data-icon="check" data-iconpos="right"
                        Text="<%$ Tokens:StringResource, checkoutpayment.aspx.18 %>" />
            </asp:Panel> 
        </li>
       
        </ul>
    </asp:Panel>
</asp:Content>
