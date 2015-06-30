<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Signin.ascx.cs" Inherits="AspDotNetStorefront.Signin" %>

<asp:Panel ID="pnlLogin" runat="server">
    <div class="page-wrap signin-page">
        <h1>
            <asp:Label ID="Label11" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.6 %>'></asp:Label>
        </h1>
        <asp:Panel ID="ErrorPanel" runat="server" Visible="False" HorizontalAlign="Left">
            <div class="error-wrap">
                <asp:Label CssClass="error-large" ID="ErrorMsgLabel" runat="server"></asp:Label>
            </div>
        </asp:Panel>
        <asp:Login ID="ctrlLogin" Width="100%" runat="server" OnLoggingIn="ctrlLogin_LoggingIn" EnableViewState="true" CssClass="login-layout-table">
            <LayoutTemplate>
                <asp:Panel ID="FormPanel" runat="server" DefaultButton="LoginButton">
                    <div class="form login-form">


                        <div class="form-text signin-text">
                            <asp:HyperLink ID="SignUpLink" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.7 %>'></asp:HyperLink>
                        </div>
                        <div class="group-header form-header signin-text">
                            <asp:Literal ID="ltlSigninHeader" runat="server" Text="<%$ Tokens:Topic, SigninPageHeader %>" />
                        </div>
                        <div class="form-group">
                            <label>
                                <asp:Label ID="Label3" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.10 %>'></asp:Label></label>
                            <asp:TextBox ID="UserName" CssClass="form-control" runat="server" ValidationGroup="Group1" MaxLength="100"
                                CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator3" runat="server" ValidationGroup="Group1"
                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="UserName"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <label>
                                <asp:Label ID="Label2" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.12 %>'></asp:Label></label>
                            <asp:TextBox ID="Password" runat="server" CssClass="form-control" ValidationGroup="Group1" MaxLength="50"
                                CausesValidation="True" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator2" runat="server" ValidationGroup="Group1"
                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="Password"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <label>
                                <asp:Label ID="Label1" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.21 %>' Visible="False"></asp:Label></label>
                            <asp:TextBox ID="SecurityCode" runat="server" CssClass="form-control" Visible="False" ValidationGroup="Group1"
                                CausesValidation="True" EnableViewState="False"></asp:TextBox>
                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator4" runat="server" ControlToValidate="SecurityCode"
                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.20 %>' ValidationGroup="Group1"
                                Enabled="False"></asp:RequiredFieldValidator>
                            <asp:Image ID="SecurityImage" runat="server" Visible="False"></asp:Image>
                        </div>
                        <div class="form-group checkbox">
                            <label>
                                <asp:CheckBox ID="RememberMe" runat="server"></asp:CheckBox><asp:Literal ID="ltRememberPassword" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.13 %>'></asp:Literal></label>
                        </div>
                        <div class="form-submit-wrap">
                            <asp:Button ID="LoginButton" CssClass="button call-to-action login-button" CommandName="Login" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.19 %>'
                                ValidationGroup="Group1"></asp:Button>
                        </div>
                    </div>
                </asp:Panel>
                <asp:CheckBox ID="DoingCheckout" runat="server" Visible="False" />
                <asp:Label ID="ReturnURL" runat="server" Text="default.aspx" Visible="False" />
                <asp:Panel ID="pnlChangePwd" runat="server" Visible="false" DefaultButton="btnChgPwd">
                    <div class="form login-form">
						<asp:Panel runat="server" ID="pnlPasswordChangeError" Visble="false">
							<div class="error-wrap change-password-error">
								<asp:Label ID="lblPwdChgErr" runat="server" CssClass="error-large" Visible="false"></asp:Label>
							</div>
						</asp:Panel>
                        <div class="error-wrap change-password-prompt">
                            <asp:Label ID="Label10" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.22 %>' CssClass="error-large"></asp:Label>
                        </div>
                        <div class="form-text signin-text">
                            <asp:Label ID="Label19" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.8 %>'></asp:Label>
                        </div>
                        <div class="form-text signin-text">
                            <asp:Label ID="Label13" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.23 %>'></asp:Label>
                        </div>
                        <div class="form-group">
                            <label>
                                <asp:Label ID="Label14" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.10 %>'></asp:Label></label>
                            <asp:TextBox CssClass="form-control" ID="CustomerEmail" runat="server" ValidationGroup="Group3"
                                MaxLength="100" CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator5" runat="server" ValidationGroup="Group3"
                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="CustomerEmail"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <label>
                                <asp:Label ID="Label15" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.33 %>'></asp:Label></label>
                            <asp:TextBox CssClass="form-control" ID="OldPassword" runat="server" ValidationGroup="Group3"
                                MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator6" runat="server" ValidationGroup="Group3"
                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="OldPassword"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <label>
                                <asp:Label ID="Label16" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.34 %>'></asp:Label>
                            </label>
                            <asp:TextBox CssClass="form-control" ID="NewPassword" runat="server" ValidationGroup="Group3"
                                MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator7" runat="server" ValidationGroup="Group3"
                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="NewPassword"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <label>
                                <asp:Label ID="Label17" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.35 %>'></asp:Label></label>
                            <asp:TextBox CssClass="form-control" ID="NewPassword2" runat="server" ValidationGroup="Group3"
                                MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator8" runat="server" ValidationGroup="Group3"
                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="NewPassword2"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <label>
                                <asp:Label ID="Label18" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.21 %>' Visible="False"></asp:Label>
                            </label>
                            <asp:TextBox CssClass="form-control" ID="SecurityCode2" runat="server" Visible="False" ValidationGroup="Group3"
                                CausesValidation="True" Width="73px" EnableViewState="False"></asp:TextBox>
                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator9" runat="server" ControlToValidate="SecurityCode2"
                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.20 %>' ValidationGroup="Group3"
                                Enabled="False"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-submit-wrap">
                            <asp:Button ID="btnChgPwd" CssClass="button change-password-button" OnClick="btnChgPwd_Click" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.36 %>'
                                ValidationGroup="Group3"></asp:Button>
                        </div>
                    </div>
                </asp:Panel>
            </LayoutTemplate>
        </asp:Login>
        <asp:Panel ID="ExecutePanel" runat="server" Visible="false">
            <div class="notice-wrap signin-executing-text">
                <asp:Literal ID="SignInExecuteLabel" runat="server"></asp:Literal>
            </div>
        </asp:Panel>
        <asp:PasswordRecovery ID="ctrlRecoverPassword" runat="server" OnVerifyingUser="ctrlRecoverPassword_VerifyingUser" CssClass="forgot-password-layout-table">
            <UserNameTemplate>
                <asp:Panel ID="Panel1" runat="server" DefaultButton="btnRequestNewPassword">
                <div class="form password-recovery-form">
                    <div class="group-header form-header forgot-password-header">
                        <asp:Label ID="lblForgotPasswrod" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.15 %>'></asp:Label>
                    </div>
                    <div class="form-text forgot-password-instructions">
                        <asp:Label ID="lblForgotPasswordInstructions" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.16 %>'></asp:Label>
                    </div>
                    <div class="form-group forgot-password-email">
                        <label>
                            <asp:Label ID="lblUserName" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.10 %>'></asp:Label></label>
                        <asp:TextBox CssClass="form-control" ID="UserName" runat="server" ValidationGroup="Group2" CausesValidation="True"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="UserName"
                            ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ValidationGroup="Group2"
                            Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-submit-wrap forgot-password-submit-wrap">
                        <asp:Button CssClass="button request-password-button" ID="btnRequestNewPassword" CommandName="Submit" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.18 %>'
                            ValidationGroup="Group2"></asp:Button>
                    </div>
                </div>
                </asp:Panel>
            </UserNameTemplate>
        </asp:PasswordRecovery>
    </div>
</asp:Panel>
