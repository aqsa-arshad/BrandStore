<%@ Page Language="c#" Inherits="AspDotNetStorefront.checkoutanon" CodeFile="checkoutanon.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">

        <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
        <div class="page-wrap checkout-anon-page">
            <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />

            <asp:Panel ID="ErrorPanel" runat="server" Visible="False">
                <div class="error-wrap">
                    <asp:Label ID="ErrorMsgLabel" CssClass="error-large" runat="server"></asp:Label>
                </div>
            </asp:Panel>
            <asp:Panel ID="FormPanel" runat="server" Width="90%">
                <div class="page-row signin-row">

                    <div class="one-half signin-half">
                        <div class="form register-form">
                            <div class="group-header form-header">
                                <asp:Label ID="Label6" runat="server" Text="<%$ Tokens:StringResource,checkoutanon.aspx.12 %>"></asp:Label>
                            </div>
                            <div class="form-text">
                                <asp:Label ID="Label1" runat="server" Text="<%$ Tokens:StringResource,checkoutanon.aspx.3 %>" />
                            </div>

                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="ltEmail" runat="server" Text="<%$ Tokens:StringResource,checkoutanon.aspx.4 %>" />
                                </label>
                                <asp:TextBox ID="EMail" CssClass="form-control" runat="server" ValidationGroup="Group1" MaxLength="100" CausesValidation="True"
                                    AutoCompleteType="Email"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" ControlToValidate="EMail" runat="server" ValidationGroup="Group1" ErrorMessage="Required" Display="Dynamic" Font-Bold="True" SetFocusOnError="True"></asp:RequiredFieldValidator>
                            </div>

                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="ltPassword" runat="server" Text="<%$ Tokens:StringResource,checkoutanon.aspx.5 %>" />
                                </label>
                                <asp:TextBox ID="Password" CssClass="form-control" runat="server" ValidationGroup="Group1" MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                            </div>
                            <asp:Panel ID="pnlSecurity" runat="server" Visible="false">
                                <div class="form-group">
                                    <label>
                                        <asp:Literal ID="ltSecurity" runat="server" Text="<%$ Tokens:StringResource,signin.aspx.21 %>" Visible="False"></asp:Literal>
                                    </label>

                                    <asp:TextBox ID="SecurityCode2" CssClass="form-control" runat="server" Visible="False" ValidationGroup="Group3" CausesValidation="True" Width="157px" EnableViewState="False"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ControlToValidate="SecurityCode2" ErrorMessage="<%$ Tokens:StringResource,signin.aspx.20 %>" ValidationGroup="Group3" Enabled="False"></asp:RequiredFieldValidator>

                                    <asp:Image ID="SecurityImage" runat="server" Visible="False"></asp:Image>
                                </div>
                            </asp:Panel>
                            <div class="form-submit-wrap">
                                <asp:Button ID="btnSignInAndCheckout" CssClass="button" Text="<%$ Tokens:StringResource,checkoutanon.aspx.13 %>" runat="server" ValidationGroup="Group1" CausesValidation="true" OnClick="btnSignInAndCheckout_Click" />
                            </div>
                            <div class="form-text">
                                <asp:Label ID="Label5" runat="server" Text="<%$ Tokens:StringResource,checkoutanon.aspx.7 %>" />
                                <asp:HyperLink ID="HyperLink1" Text="<%$ Tokens:StringResource,checkoutanon.aspx.8 %>" runat="server" NavigateUrl="signin.aspx?checkout=true"></asp:HyperLink>
                            </div>
                        </div>
                    </div>
                    <div class="one-half register-half">
                        <div class="form register-form">
                            <div class="group-header form-header">
                                <asp:Literal ID="ltNewCustomers" runat="server" Text="<%$ Tokens:StringResource,checkoutanon.aspx.9 %>" />
                            </div>
                            <div class="form-controls">
                                <asp:Button ID="RegisterAndCheckoutButton" CssClass="button call-to-action" Text="<%$ Tokens:StringResource,checkoutanon.aspx.14 %>" runat="server" CausesValidation="false" OnClick="RegisterAndCheckoutButton_Click" />
                            </div>
                            <aspdnsf:Topic runat="server" ID="Teaser" TopicName="CheckoutAnonTeaser" />
                        </div>
                        <asp:Panel runat="Server" ID="PasswordOptionalPanel" Visible="false">
                            <div class="form anon-form">
                                <div class="group-header form-header">
                                    <asp:Label ID="Label8" runat="server" Text="<%$ Tokens:StringResource,checkoutanon.aspx.10 %>" />
                                </div>
                                <div class="form-text">
                                    <asp:Label ID="Label9" runat="server" Text="<%$ Tokens:StringResource,checkoutanon.aspx.11 %>" />
                                </div>
                                <div class="form-controls">
                                    <asp:Button ID="Skipregistration" CssClass="button anon-checkout-button" Text="<%$ Tokens:StringResource,checkoutanon.aspx.15 %>" runat="server" CausesValidation="false" OnClick="Skipregistration_Click" />
                                </div>
                            </div>
                        </asp:Panel>

                    </div>

                </div>
            </asp:Panel>
            <asp:Panel ID="ExecutePanel" runat="server" Width="90%" HorizontalAlign="center" Visible="false">
                <asp:Label ID="SignInExecuteLabel" runat="server"></asp:Label>
            </asp:Panel>
            <asp:Panel ID="pnlChangePwd" runat="server" Visible="false">
                <div class="form password-change-form">
                    <div class="group-header form-header">
                        <asp:Label ID="Label10" runat="server" Text="<%$ Tokens:StringResource,signin.aspx.22 %>"></asp:Label>
                    </div>
                    <div class="form-text">
                        <asp:Label ID="lblPwdChgErr" runat="server" Visible="false"></asp:Label>
                    </div>
                    <div class="form-text">
                        <asp:Label ID="Label19" runat="server" Text="<%$ Tokens:StringResource,signin.aspx.8 %>"></asp:Label>
                    </div>
                    <div class="form-text">
                        <asp:Label ID="Label13" runat="server" Text="<%$ Tokens:StringResource,signin.aspx.23 %>"></asp:Label>
                    </div>

                    <div class="form-group">
                        <label>
                            <asp:Label ID="Label14" runat="server" Text="<%$ Tokens:StringResource,signin.aspx.10 %>"></asp:Label></label>
                        <asp:TextBox ID="CustomerEmail" runat="server" ValidationGroup="Group3" CssClass="form-control" MaxLength="100" CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ValidationGroup="Group3" ErrorMessage="<%$ Tokens:StringResource,signin.aspx.3 %>" ControlToValidate="CustomerEmail" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>

                    <div class="form-group">
                        <label>
                            <asp:Label ID="Label15" runat="server" Text="Old Password"></asp:Label>
                        </label>
                        <asp:TextBox ID="OldPassword" runat="server" ValidationGroup="Group3" CssClass="form-control" MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ValidationGroup="Group3" ErrorMessage="<%$ Tokens:StringResource,signin.aspx.4 %>" ControlToValidate="OldPassword" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>

                    <div class="form-group">
                        <label>
                            <asp:Label ID="Label16" runat="server" Text="New Password"></asp:Label>
                        </label>
                        <asp:TextBox ID="NewPassword" runat="server" ValidationGroup="Group3" CssClass="form-control" MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ValidationGroup="Group3" ErrorMessage="<%$ Tokens:StringResource,signin.aspx.4 %>" ControlToValidate="NewPassword" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>

                    <div class="form-group">
                        <label>
                            <asp:Label ID="Label17" runat="server" Text="Confirm New Password"></asp:Label>
                        </label>
                        <asp:TextBox ID="NewPassword2" runat="server" ValidationGroup="Group3" CssClass="form-control" MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ValidationGroup="Group3" ErrorMessage="<%$ Tokens:StringResource,signin.aspx.4 %>" ControlToValidate="NewPassword2" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-submit-wrap">
                        <asp:Button ID="btnChgPwd" CssClass="button update-button" OnClick="btnChgPwd_Click" runat="server" Text="Change Password" ValidationGroup="Group1"></asp:Button>
                    </div>
                </div>
            </asp:Panel>

        </div>

    </asp:Panel>
</asp:Content>



