<%@ Page ClientTarget="UpLevel" Language="c#" Inherits="AspDotNetStorefront.account" CodeFile="account.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="SecureNetVault" Src="~/Controls/SecureNetVaultPanel.ascx" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Register Src="CIM/Wallet.ascx" TagName="Wallet" TagPrefix="uc1" %>
<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel ID="pnlContent" runat="server">
        <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />
         <%--Hidden Variables Regions--%>
        <asp:Label ID="hdnCustomerLevel1" name="hdnCustomerLevel1" runat="server" ClientIDMode="Static" Style="display: none" />
        <%--End Hidden Variables Region--%>
        <div class="content-box-03">

            <%--<h1 class="account-page-header">
				<asp:Literal ID="ltAccount" Text="<%$ Tokens:StringResource,Header.AccountInformation %>" runat="server" />
			</h1>--%>
            <asp:Panel ID="pnlErrorMsg" runat="server" Visible="false">
                <div class="error-wrap">
                    <asp:Label ID="lblErrorMessage" runat="server" CssClass="error-large"></asp:Label>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlCBAAddressWidget" runat="server" Visible="false">
                <div class="page-row row-cba-address">
                    <asp:Literal ID="litCBAAddressWidget" runat="server" />
                </div>
                <div class="page-row row-cba-instructions">
                    <asp:Literal ID="litCBAAddressWidgetInstruction" runat="server" />
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlAccountUpdated" runat="server">
                <div class="notice-wrap account-update-notice-wrap">
                    <asp:Label CssClass="notice-large" ID="lblAcctUpdateMsg" runat="server"></asp:Label>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlNotCheckOutButtons" runat="server" HorizontalAlign="left" CssClass="hide-element">
                <div class="page-row page-links">
                    <asp:HyperLink runat="server" ID="accountaspx4" NavigateUrl="#OrderHistory" Text="<%$ Tokens:StringResource,account.aspx.4 %>"></asp:HyperLink>
                    <asp:Panel ID="pnlShowWishButton" runat="server">
                        <asp:HyperLink runat="server" ID="ShowWishButtons" NavigateUrl="~/wishlist.aspx" Text="<%$ Tokens:StringResource,account.aspx.58 %>"></asp:HyperLink>
                    </asp:Panel>
                    <asp:Panel ID="pnlShowGiftRegistryButtons" runat="server">
                        <asp:HyperLink runat="server" ID="HyperLink1" NavigateUrl="~/giftregistry.aspx" Text="<%$ Tokens:StringResource,account.aspx.59 %>"></asp:HyperLink>
                    </asp:Panel>
                    <asp:Panel ID="pnlSubscriptionExpiresOn" runat="server">
                        <asp:Label runat="server" ID="lblSubscriptionExpiresOn"></asp:Label>
                    </asp:Panel>
                </div>
            </asp:Panel>

            <aspdnsf:Topic runat="server" ID="HeaderMsg" TopicName="AccountPageHeader" />

            <asp:TextBox ID="OriginalEMail" runat="server" Visible="false" />

            <asp:Panel runat="server" ID="pnlCustomerLevel" Visible="false">
                <div class="notice-wrap customer-level-notice-wrap">
                    <asp:Label ID="lblCustomerLevel" runat="server"></asp:Label>
                </div>
            </asp:Panel>

            <asp:Panel runat="server" ID="pnlMicroPayEnabled" Visible="false">
                <div class="notice-wrap micropay-notice-wrap">
                    <asp:Label CssClass="notice-large" ID="lblMicroPayEnabled" runat="server"></asp:Label>
                </div>
            </asp:Panel>


            <asp:ValidationSummary DisplayMode="List" ID="ValSummary" ShowMessageBox="false" runat="server" ShowSummary="true" CssClass="error-wrap validation-summary" ValidationGroup="account" />

            <div class="row">
            <div class="col-md-4" id="divAccountInfo">               
                <h4 class="normal-heading black-color">
                    <asp:Label ID="accountaspx12" runat="server" Text="CONTACT"></asp:Label>
                </h4>
                 <p>
                    <asp:Literal ID="Literal1" Text="<span class='block-text'>Please enter your contact </span> information. *Required Fields." runat="server" />
                </p>
                <aspdnsf:Accounttab ID="ctrlAccount" runat="server"
                    EmailCaption="<%$ Tokens:StringResource, account.aspx.15 %>"
                    EmailReenterCaption="<%$ Tokens:StringResource, createaccount.aspx.91 %>"
                    FirstNameCaption="<%$ Tokens:StringResource, account.aspx.13 %>"
                    LastNameCaption="<%$ Tokens:StringResource, account.aspx.14 %>"
                    DateOfBirthCaption="<%$ Tokens:StringResource, createaccount.aspx.87 %>"
                    PasswordCaption="<%$ Tokens:StringResource, account.aspx.66 %>"
                    PasswordConfirmCaption="<%$ Tokens:StringResource, account.aspx.67 %>"
                    PhoneCaption="<%$ Tokens:StringResource, account.aspx.23 %>"
                    SaveCCCaption="<%$ Tokens:StringResource, account.aspx.65 %>"
                    OKToEmailCaption="<%$ Tokens:StringResource, account.aspx.24 %>"
                    OKToEmailNoCaption="<%$ Tokens:StringResource, account.aspx.26 %>"
                    OKToEmailYesCaption="<%$ Tokens:StringResource, account.aspx.25 %>"
                    Over13Caption="<%$ Tokens:StringResource, createaccount.aspx.78 %>"
                    VATRegistrationIDCaption="<%$ Tokens:StringResource, account.aspx.71 %>"
                    PasswordNote="<%$ Tokens:StringResource, account.aspx.19 %>"
                    OKToEmailNote="<%$ Tokens:StringResource, account.aspx.27 %>"
                    SaveCCNote="<%$ Tokens:StringResource, account.aspx.70 %>"
                    VATRegistrationIDInvalidCaption="<%$ Tokens:StringResource, account.aspx.72 %>"
                    SecurityCodeCaption="<%$ Tokens:StringResource, signin.aspx.21 %>"
                    EmailRegExErrorMessage="<%$ Tokens:StringResource, account.aspx.78 %>"
                    EmailReqFieldErrorMessage="<%$ Tokens:StringResource, account.aspx.77 %>"
                    ReEnterEmailReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.94 %>"
                    FirstNameReqFieldErrorMessage="<%$ Tokens:StringResource, account.aspx.75 %>"
                    LastNameReqFieldErrorMessage="<%$ Tokens:StringResource, account.aspx.76 %>"
                    DayOfBirthReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.88 %>"
                    MonthOfBirthReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.89 %>"
                    YearOfBirthReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.90 %>"
                    EmailCompareFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.93 %>"
                    DayOfBirthReqFieldValGrp="account"
                    MonthOfBirthReqFieldValGrp="account"
                    YearOfBirthReqFieldValGrp="account"
                    PhoneReqFieldErrorMessage="<%$ Tokens:StringResource, account.aspx.79 %>"
                    EmailRegExValGrp="account"
                    EmailReqFieldValGrp="account"
                    EmailCompareFieldValGrp="account"
                    ReEnterEmailReqFieldValGrp="account"
                    FirstNameReqFieldValGrp="account"
                    LastNameReqFieldValGrp="account"
                    PhoneReqFieldValGrp="account"
                    PasswordValidateValGrp="account" Over13ValGrp="account"
                    ShowVATRegistrationID="<%$ Tokens:AppConfigBool, VAT.Enabled %>"
                    ShowOver13="<%$ Tokens:AppConfigBool, RequireOver13Checked %>"
                    ShowSaveCC="<%$ Tokens:AppConfigBool, StoreCCInDB %>"
                    ShowPassword="false"
                    DisablePasswordAutocomplete="<%$ Tokens:AppConfigBool, DisablePasswordAutocomplete %>" />
                <%--<div class="form-submit-wrap">
					<asp:Button ID="btnUpdateAccount" CssClass="button update-account-button" Text="<%$ Tokens:StringResource,account.aspx.28 %>" runat="server" CausesValidation="true" ValidationGroup="account" OnClick="btnUpdateAccount_Click" />
					<asp:Button ID="btnContinueToCheckOut" CssClass="button call-to-action" Text="<%$ Tokens:StringResource,account.aspx.60 %>" runat="server" CausesValidation="false" OnClick="btnContinueToCheckOut_Click" />
				</div>--%>
            </div>
            <asp:Panel ID="pnlAddress" runat="server">
                <%--<div class="group-header account-header address-header">
					<asp:Literal ID="Literal2" Text="<%$ Tokens:StringResource,Header.AddressBook %>" runat="server" />
				</div>--%>
                <%--<div class="page-row form-text address-text">
					<asp:Label ID="accountaspx29" runat="server" Text="<%$ Tokens:StringResource,account.aspx.29 %>"></asp:Label>
				</div>--%>
                <div class="col-md-4 checkout-field-adjust"> 
                   <h4 class="normal-heading black-color">
                     <asp:Label ID="Label1" runat="server" Text="PRIMARY BILLING ADDRESS"></asp:Label>
                   </h4>
                    <asp:Panel ID="pnlBilling" runat="server">
                        <%-- <div class="address-header">
                            <asp:Label ID="accountaspx30" runat="server" Text="<%$ Tokens:StringResource,account.aspx.30 %>"></asp:Label>
                        </div>--%>                           
                        <div class="address-view">
                            <asp:Literal ID="litBillingAddress" runat="server"></asp:Literal>
                        </div>
                            <div class="address-edit">
                            <asp:HyperLink ID="lnkChangeBilling" Text="Change Billing Address" runat="server" CssClass="underline-link"></asp:HyperLink>
                        </div>
                        <%-- <div class="address-new">
                            <asp:HyperLink ID="lnkAddBillingAddress" runat="server"></asp:HyperLink>
                        </div>--%>
                    </asp:Panel>
                </div>
                <div class="col-md-3"> 
                   <h4 class="normal-heading black-color">
                        <asp:Label ID="Label2" runat="server" Text="PRIMARY SHIPPING ADDRESS"></asp:Label>
                </h4>
                    <asp:Panel ID="pnlShipping" runat="server">
                        <%--<div class="address-header">
                            <asp:Label ID="accountaspx32" runat="server" Text="<%$ Tokens:StringResource,account.aspx.32 %>"></asp:Label>
                        </div>--%>                           
                        <div class="address-view">
                            <asp:Literal ID="litShippingAddress" runat="server"></asp:Literal>
                        </div>
                            <div class="address-edit">
                            <asp:HyperLink ID="lnkChangeShipping" Text="Change Shipping Address" runat="Server" CssClass="underline-link"></asp:HyperLink>
                        </div>
                        <%-- <div class="address-new">
                            <asp:HyperLink ID="lnkAddShippingAddress" runat="server"></asp:HyperLink>
                        </div>--%>
                    </asp:Panel>
                </div>
            </asp:Panel>
             </div>
            <div class="row">
            <div class="col-md-4"> </div>
            <div class="col-md-4 checkout-field-adjust">
               <%-- <asp:Button ID="btnUpdateAccount" CssClass="btn btn-primary btn-block margin-none" Text="<%$ Tokens:StringResource,account.aspx.28 %>" runat="server" CausesValidation="true" ValidationGroup="account" OnClick="btnUpdateAccount_Click" />--%>
                <asp:Button ID="btnContinueToCheckOut" CssClass="btn btn-primary btn-block" Text="<%$ Tokens:StringResource,account.aspx.60 %>" runat="server" CausesValidation="false" OnClick="btnContinueToCheckOut_Click" />
            </div>
            </div>
            <asp:Panel ID="pnlPaymentMethods" runat="server" Visible="false" CssClass="hide-element">
                <div class="page-row row-payment-methods">
                    <asp:Panel ID="pnlSecureNetPaymentMethods" runat="server" Visible="false">
                        <aspdnsf:SecureNetVault runat="server" />
                    </asp:Panel>
                    <asp:Label runat="server" ID="LabelPanelMessage" Visible="false" />
                    <asp:Panel ID="PanelWallet" runat="server" Visible="false">
                        <div id="pnlCimWallet">
                            <asp:ScriptManagerProxy ID="SMProxy" runat="server">
                                <Scripts>
                                    <asp:ScriptReference Path="~/CIM/scripts/ajaxHelpers.js" />
                                    <asp:ScriptReference Path="~/jscripts/tooltip.js" />
                                </Scripts>
                            </asp:ScriptManagerProxy>
                            <uc1:Wallet ID="Wallet" runat="server" />
                            <script type="text/javascript">
                                Sys.Application.add_load(function () { var toolTip = new ToolTip('aCardCodeToolTip', 'card-code-tooltip', '<iframe width="400" height="370" frameborder="0" marginheight="2" marginwidth="2" left="-50" top="-50" scrolling="no" src="App_Themes/skin_1/images/verificationnumber.gif"></iframe>'); });
                            </script>
                        </div>
                    </asp:Panel>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlGiftCards" runat="server" Visible="false" CssClass="hide-element">
                <div class="group-header account-header gift-card-header">
                    <asp:Literal ID="Literal3" Text="<%$ Tokens:StringResource,Header.GiftCards %>" runat="server" />
                </div>
                <div class="page-row row-gift-card-headers">
                    <div class="one-half">Gift Card Serial #</div>
                    <div class="one-half">Balance</div>
                </div>
                <asp:Repeater ID="rptrGiftCards" runat="server">
                    <ItemTemplate>
                        <div class="page-row row-gift-card">
                            <div class="one-half"><%# ((AspDotNetStorefrontCore.GiftCard)Container.DataItem).SerialNumber %></div>
                            <div class="one-half"><%# ThisCustomer.CurrencyString(((AspDotNetStorefrontCore.GiftCard)Container.DataItem).Balance) %></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>



            <asp:Panel ID="pnlOrderHistory" runat="server" CssClass="hide-element">
                <a name="OrderHistory"></a>
                <div class="group-header account-header order-history-header">
                    <asp:Literal ID="Literal4" Text="<%$ Tokens:StringResource,Header.OrderHistory %>" runat="server" />
                </div>
                <div class="page-row order-history">
                    <table class="table table-striped order-table">
                        <tr class="table-header">
                            <th>
                                <asp:Label ID="accountaspx36" runat="server" Text="<%$ Tokens:StringResource,account.aspx.36 %>"></asp:Label>
                                <asp:Label ID="accountaspx37" runat="server" Text="<%$ Tokens:StringResource,account.aspx.37 %>"></asp:Label></th>
                            <th>
                                <asp:Label ID="accountaspx38" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>"></asp:Label></th>
                            <th>
                                <asp:Label ID="accountaspx39" runat="server" Text="<%$ Tokens:StringResource,account.aspx.39%>"></asp:Label></th>
                            <th>
                                <asp:Label ID="accountaspx40" runat="server" Text="<%$ Tokens:StringResource,account.aspx.40 %>"></asp:Label></th>

                            <th>
                                <asp:Label ID="accountaspx41" runat="server" Text="<%$ Tokens:StringResource,account.aspx.41 %>"></asp:Label></th>
                            <th>
                                <asp:Label ID="accountaspx42" runat="server" Text="<%$ Tokens:StringResource,account.aspx.42 %>"></asp:Label></th>
                        </tr>
                        <asp:Repeater ID="orderhistorylist" runat="server">
                            <HeaderTemplate>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="table-row">
                                    <td>
                                        <a target="_blank" href='<%#m_StoreLoc + "receipt.aspx?ordernumber=" + DataBinder.Eval(Container.DataItem, "OrderNumber") %>'><%# DataBinder.Eval(Container.DataItem, "OrderNumber").ToString() %></a>
                                        <%#GetReorder(DataBinder.Eval(Container.DataItem, "OrderNumber").ToString(), DataBinder.Eval(Container.DataItem, "RecurringSubscriptionID").ToString())%>
                                    </td>
                                    <td><%#AspDotNetStorefrontCore.Localization.ConvertLocaleDateTime(DataBinder.Eval(Container.DataItem, "OrderDate").ToString(), Localization.GetDefaultLocale(), ThisCustomer.LocaleSetting)%></td>
                                    <td><%#GetPaymentStatus(DataBinder.Eval(Container.DataItem, "PaymentMethod").ToString(), DataBinder.Eval(Container.DataItem, "CardNumber").ToString(), DataBinder.Eval(Container.DataItem, "TransactionState").ToString(), DataBinder.Eval(Container.DataItem, "OrderTotal").ToString())%></td>
                                    <td><%#"&nbsp;" + GetShippingStatus(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "OrderNumber").ToString()), DataBinder.Eval(Container.DataItem, "ShippedOn").ToString(), DataBinder.Eval(Container.DataItem, "ShippedVIA").ToString(), DataBinder.Eval(Container.DataItem, "ShippingTrackingNumber").ToString(), DataBinder.Eval(Container.DataItem, "TransactionState").ToString(), DataBinder.Eval(Container.DataItem, "DownloadEMailSentOn").ToString()) + "&nbsp;"%></td>
                                    <td><%#GetOrderTotal(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "QuoteCheckout").ToString()), DataBinder.Eval(Container.DataItem, "PaymentMethod").ToString(), DataBinder.Eval(Container.DataItem, "OrderTotal").ToString(), Convert.ToInt32(DataBinder.Eval(Container.DataItem, "CouponType").ToString()), DataBinder.Eval(Container.DataItem, "CouponDiscountAmount"))%></td>
                                    <td><%#"&nbsp;" + GetCustSvcNotes(DataBinder.Eval(Container.DataItem, "CustomerServiceNotes").ToString()) + "&nbsp;"%></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate></FooterTemplate>
                        </asp:Repeater>
                    </table>
                    <asp:Label ID="accountaspx55" runat="server" Text="<%$ Tokens:StringResource,account.aspx.55 %>"></asp:Label>
                </div>
                <div class="page-row recurring-history">
                    <asp:Literal ID="ltRecurringOrders" runat="Server"></asp:Literal>
                </div>
            </asp:Panel>
        </div>
    </asp:Panel>
    <script type="text/javascript">
        $(document).ready(function () {
            var CustomerLevel = GetCustomerLevel();
            if (CustomerLevel == "8") {
                $("#divAccountInfo").removeClass("hide-element");
            }
            else {
                $("#divAccountInfo").addClass("hide-element");
            }
           

            function GetCustomerLevel() {
                var CustomerLevelElemment;
                if (document.getElementById('hdnCustomerLevel1')) {
                    CustomerLevelElemment = document.getElementById('hdnCustomerLevel1');
                }
                else if (document.all) {
                    CustomerLevelElemment = document.all['hdnCustomerLevel1'];
                }
                else {
                    CustomerLevelElemment = document.layers['hdnCustomerLevel1'];
                }

                return CustomerLevelElemment.innerHTML;
            }
        });

    </script>
</asp:Content>

