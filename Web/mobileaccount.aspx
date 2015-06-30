<%@ Page ClientTarget="UpLevel" language="c#" Inherits="AspDotNetStorefront.mobileaccount" CodeFile="mobileaccount.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master"   %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Import namespace="AspDotNetStorefrontCore" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >            
     
    <asp:Textbox ID="OriginalEMail" runat="server" Visible="false" />
        <asp:Panel ID="pnlCheckoutImage" runat="server" HorizontalAlign="Center" Visible="false">
            <asp:Literal runat="server" id="CheckoutHeader"></asp:Literal>
        </asp:Panel>
		<ul data-role="listview">
			<li class="group" data-role="list-divider" style="text-align:center;">
				<asp:Label ID="unknownerrormsg" runat="server" Style="color: #FF0000;"></asp:Label>
				<asp:Label ID="ErrorMsgLabel" runat="server" Style="color: #FF0000;"></asp:Label>
				<asp:Panel ID="pnlAccountUpdated" runat="server" HorizontalAlign="center">
					<asp:Label ID="lblAcctUpdateMsg" runat="server" Style="font-weight: bold; color: #FF0000;"></asp:Label>
				</asp:Panel>
				<asp:ValidationSummary DisplayMode="List" ID="ValSummary" ShowMessageBox="false" runat="server" ShowSummary="true" ValidationGroup="account" ForeColor="red" Font-Bold="true"/>
			</li>
			<li>
			    <a href="signout.aspx"><asp:Label ID="Label3" runat="server" Text="<%$ Tokens:StringResource,Mobile.Account.SignOut %>" /></a>
			</li>
		</ul>
        
        <div data-role="collapsible-set" id="mobileaccount-accordian">
            <asp:PlaceHolder runat="server" id="pnlAccountInfoMP">
	            <div data-role="collapsible" data-collapsed="false">
	                <h3><asp:Label ID="Label4" runat="server" Text="<%$ Tokens:StringResource,Mobile.Account.InfoHeader %>" /></h3>
	                <div class="accordion_child">
                        <table border="0" cellpadding="4" cellspacing="0" class="MobileAccountInfoTable">
                        <aspdnsf:Account ID="ctrlAccount" runat="server"
                            EmailCaption="<%$ Tokens:StringResource, account.aspx.15 %>" 
							EmailReenterCaption="<%$ Tokens:StringResource, createaccount.aspx.91 %>"
                            FirstNameCaption="<%$ Tokens:StringResource, account.aspx.13 %>" 
                            LastNameCaption="<%$ Tokens:StringResource, account.aspx.14 %>" 
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
							EmailCompareFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.93 %>"
                            FirstNameReqFieldErrorMessage="<%$ Tokens:StringResource, account.aspx.75 %>"
                            LastNameReqFieldErrorMessage="<%$ Tokens:StringResource, account.aspx.76 %>"
                            PhoneReqFieldErrorMessage="<%$ Tokens:StringResource, account.aspx.79 %>"
                            EmailRegExValGrp="account" EmailReqFieldValGrp="account"
                            FirstNameReqFieldValGrp="account" LastNameReqFieldValGrp="account"
                            PhoneReqFieldValGrp="account"
                            PasswordValidateValGrp="account" Over13ValGrp="account"
                            ShowVATRegistrationID="<%$ Tokens:AppConfigBool, VAT.Enabled %>"
                            ShowOver13="<%$ Tokens:AppConfigBool, RequireOver13Checked %>"
                            ShowSaveCC="<%$ Tokens:AppConfigBool, StoreCCInDB %>"                               
                            />
                        </table>
                        <p>
                            <asp:Button ID="btnUpdateAccount" Text="<%$ Tokens:StringResource,account.aspx.28 %>" runat="server" CausesValidation="true" ValidationGroup="account" OnClick="btnUpdateAccount_Click"  />
                        </p>
                    </div>
	            </div>
            </asp:PlaceHolder>
	
            <asp:PlaceHolder runat="server" id="pnlShippingAddressMP">
	            <div data-role="collapsible">
	                <h3>
                        <asp:Label ID="Label5" runat="server" Text="<%$ Tokens:StringResource,Mobile.Account.ShippingHeader %>" />
                        &nbsp;<span class="teaser"><asp:Literal ID="litShipTeaser"  runat="server"/></span>
                    </h3>
	                <div class="accordion_child">
                        <asp:Panel ID="pnlShipping" runat="server">
                            <asp:Literal ID="litShippingAddress" runat="server"></asp:Literal>
                           </asp:Panel>
                           <b><asp:HyperLink ID="lnkAddShippingAddress" runat="server"></asp:HyperLink></b>
                    </div>
	            </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" id="pnlBillingAddressMP">
                <div data-role="collapsible">
	                <h3>
                        <asp:Label ID="Label6" runat="server" Text="<%$ Tokens:StringResource,Mobile.Account.BillingHeader %>" />
                        &nbsp;<span class="teaser"><asp:Literal ID="litBillTeaser"  runat="server"/></span>
                    </h3>
	                <div class="accordion_child">
                        <asp:Panel ID="pnlBilling" runat="server">
                            <asp:Literal ID="litBillingAddress" runat="server"></asp:Literal>
                        </asp:Panel>
                        <b><asp:HyperLink ID="lnkAddBillingAddress" runat="server"></asp:HyperLink></b>
                    </div>
	            </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" id="pnlOrderHistoryMP">
                <div data-role="collapsible">
	                <h3><asp:Label ID="Label7" runat="server" Text="<%$ Tokens:StringResource,Mobile.Account.HistoryHeader %>" /></h3>
	                <div class="accordion_child">
                        <asp:Panel ID="pnlOrderHistory" runat="server">
                            <table width="100%" cellpadding="2" cellspacing="0" border="1">
                                <asp:Repeater ID="orderhistorylist" runat="server">
                                    <HeaderTemplate>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td align="left" valign="top" colspan="2">
                                                <a target="_blank" href='<%#m_StoreLoc + "receipt.aspx?ordernumber=" + DataBinder.Eval(Container.DataItem, "OrderNumber") %>'>
                                                    <%# DataBinder.Eval(Container.DataItem, "OrderNumber").ToString() %>
                                                </a>:
                                                <%#GetOrderTotal(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "QuoteCheckout").ToString()), DataBinder.Eval(Container.DataItem, "PaymentMethod").ToString(), DataBinder.Eval(Container.DataItem, "OrderTotal").ToString(), Convert.ToInt32(DataBinder.Eval(Container.DataItem, "CouponType").ToString()), DataBinder.Eval(Container.DataItem, "CouponDiscountAmount"))%>
                                                from <%#AspDotNetStorefrontCore.Localization.ConvertLocaleDateTime(DataBinder.Eval(Container.DataItem, "OrderDate").ToString(), Localization.GetDefaultLocale(), ThisCustomer.LocaleSetting).Substring(0, 10)%>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td  align="left" valign="top" colspan="2">
                                                 <%# GetShippingStatus(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "OrderNumber").ToString()), DataBinder.Eval(Container.DataItem, "ShippedOn").ToString(), DataBinder.Eval(Container.DataItem, "ShippedVIA").ToString(), DataBinder.Eval(Container.DataItem, "ShippingTrackingNumber").ToString(), DataBinder.Eval(Container.DataItem, "TransactionState").ToString(), DataBinder.Eval(Container.DataItem, "DownloadEMailSentOn").ToString()) + ".&nbsp;"%>
                                            Notes:
                                               <%#"&nbsp;" + GetCustSvcNotes(DataBinder.Eval(Container.DataItem, "CustomerServiceNotes").ToString()) + "&nbsp;"%>
                                               <%#GetReorder(DataBinder.Eval(Container.DataItem, "OrderNumber").ToString(), DataBinder.Eval(Container.DataItem, "RecurringSubscriptionID").ToString())%>
                                            &nbsp;&nbsp;&nbsp;&nbsp;
                                                <a target="_blank" href='<%#m_StoreLoc + "receipt.aspx?ordernumber=" + DataBinder.Eval(Container.DataItem, "OrderNumber") %>' class="mobileViewReceipt">
                                                    View Receipt
                                                </a>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate></FooterTemplate>
                                    <SeparatorTemplate>
                                    <tr>
                                        <td colspan="2" style="height:25px;vertical-align:middle;" valign="top"><div style="border-top:solid 2px black;"></div></td>
                                    </tr>
                                    </SeparatorTemplate>
                                </asp:Repeater>
                            </table>
                        </asp:Panel>
                        <asp:Label ID="accountaspx55" runat="server" Text="<%$ Tokens:StringResource,account.aspx.55 %>"></asp:Label>
                    </div>
	            </div>
            </asp:PlaceHolder>
	
        </div>

        <asp:PlaceHolder ID="phContinueCheckout" runat="server" Visible="false">
            <ul data-role="listview">
                <li>
                    <asp:Button ID="btnContinueToCheckOut" Text="<%$ Tokens:StringResource,account.aspx.60 %>" runat="server" CausesValidation="false" OnClick="btnContinueToCheckOut_Click" />
                </li>
            </ul>
        </asp:PlaceHolder>
        
</asp:Panel>
</asp:Content>
