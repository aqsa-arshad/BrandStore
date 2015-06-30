<%@ Page ClientTarget="UpLevel" Language="c#" Inherits="AspDotNetStorefront.mobilecreateaccount"
    CodeFile="mobilecreateaccount.aspx.cs" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <asp:Panel ID="pnlCheckoutImage" runat="server" HorizontalAlign="Center" Visible="false">
            <asp:Literal runat="server" id="CheckoutHeader"></asp:Literal>
        </asp:Panel>
            <asp:Panel ID="pnlErrorMsg" runat="Server" HorizontalAlign="Left" Visible="false">
                <ul data-role="listview">
                    <li class="group" data-role="list-divider">
                            <asp:Label ID="ErrorMsgLabel" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>
                    </li>
                </ul>
            </asp:Panel>
            <asp:Panel ID="pnlAccountInfo" runat="server" Visible="false">
                <asp:Literal ID="Signin" runat="server" Mode="PassThrough"></asp:Literal>
                <ul data-role="listview">
                    <li class="group" data-role="list-divider">
                       <asp:Literal ID="Literal2" runat="server" Text="<%$ Tokens:StringResource,Mobile.CreateAccount.ContactInfo %>" />
                    </li>
                    <li>
                            <table border="0" cellpadding="4" cellspacing="0" width="100%">
                                <aspdnsf:Account ID="ctrlAccount" runat="server" 
                                    EmailCaption="<%$ Tokens:StringResource, createaccount.aspx.15 %>"
									EmailReenterCaption="<%$ Tokens:StringResource, createaccount.aspx.91 %>"
                                    FirstNameCaption="<%$ Tokens:StringResource, createaccount.aspx.13 %>" 
                                    LastNameCaption="<%$ Tokens:StringResource, createaccount.aspx.14 %>"
                                    PasswordCaption="<%$ Tokens:StringResource, createaccount.aspx.18 %>" 
                                    PasswordConfirmCaption="<%$ Tokens:StringResource, createaccount.aspx.21 %>"
                                    PhoneCaption="<%$ Tokens:StringResource, createaccount.aspx.23 %>" 
                                    SaveCCCaption="<%$ Tokens:StringResource, account.aspx.65 %>"
                                    OKToEmailCaption="<%$ Tokens:StringResource, createaccount.aspx.26 %>" 
                                    OKToEmailNoCaption="<%$ Tokens:StringResource, createaccount.aspx.28 %>"
                                    OKToEmailYesCaption="<%$ Tokens:StringResource, createaccount.aspx.27 %>" 
                                    Over13Caption="<%$ Tokens:StringResource, createaccount.aspx.78 %>"
                                    VATRegistrationIDCaption="<%$ Tokens:StringResource, account.aspx.71 %>" 
                                    PasswordNote="<%$ Tokens:StringResource, account.aspx.19 %>"
                                    OKToEmailNote="<%$ Tokens:StringResource, createaccount.aspx.29 %>" 
                                    SaveCCNote="<%$ Tokens:StringResource, account.aspx.70 %>"
                                    SecurityCodeCaption="<%$ Tokens:StringResource, signin.aspx.21 %>" 
                                    FirstNameReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.82 %>"
                                    LastNameReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.83 %>"
                                    EmailReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.16 %>"
                                    PhoneReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.24 %>"
                                    SecurityCodeReqFieldErrorMessage="<%$ Tokens:StringResource, signin.aspx.20 %>"
                                    EmailRegExErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.17 %>"
                                    ReEnterEmailReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.94 %>"
                                    EmailCompareFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.93 %>"
                                    PasswordReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.20 %>"
                                    RequireOver13ErrorMessage="<%$ Tokens:StringResource, Over13OnCreateAccount %>"
                                    EmailRegExValGrp="registration"
                                    EmailReqFieldValGrp="registration" 
                                    FirstNameReqFieldValGrp="registration" 
                                    LastNameReqFieldValGrp="registration"
                                    Over13ValGrp="registration" 
                                    PasswordValidateValGrp="registration" 
                                    PhoneReqFieldValGrp="registration"
                                    ShowSecurityCode="true" 
                                    ShowPasswordReqVal="true" 
                                    ShowSaveCC="false"
                                    ShowValidatorsInline="true" 
                                    ShowVATRegistrationID="<%$ Tokens:AppConfigBool, Vat.Enabled %>"
                                    ShowOver13="<%$ Tokens:AppConfigBool, RequireOver13Checked %>" />
                            </table>
                    </li>
                </ul>
            </asp:Panel>
            <ul data-role="listview">
                <li class="group" data-role="list-divider"></li>
                <li>
                    <asp:Button ID="btnContinueCheckout"  data-icon="check" data-iconpos="right" CssClass="ContinueCheckoutButton fullwidthshortgreen action" OnClientClick="Page_ValidationActive=true;" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.76 %>" CausesValidation="True" ValidationGroup="registration" OnClick="btnContinueCheckout_Click" />
                <asp:ValidationSummary ID="valSummary" DisplayMode="List" runat="server" ShowMessageBox="true" ShowSummary="false" ValidationGroup="createaccount" ForeColor="red" Font-Bold="true" />
                </li>
            </ul>
    
    </asp:Panel>
</asp:Content>
