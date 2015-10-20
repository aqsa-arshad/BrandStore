<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Signin.ascx.cs" Inherits="AspDotNetStorefront.Signin" %>
<link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
<asp:Label Visible="false" runat="server" ID="HiddenLabel" Text="false"></asp:Label>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
            <div class="content-box-03" id="LoginPanel" runat="server">
                <asp:Panel ID="pnlLogin" runat="server">
                    <div class="page-wrap signin-page">
                        <asp:Panel ID="ErrorPanel" runat="server" Visible="False" HorizontalAlign="Left">
                            <div class="error-wrap">
                                <asp:Label CssClass="error-large" ID="ErrorMsgLabel" runat="server"></asp:Label>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="ExecutePanel" runat="server" Visible="false">
                            <div class="notice-wrap signin-executing-text">
                                <asp:Literal ID="SignInExecuteLabel" runat="server"></asp:Literal>
                            </div>
                        </asp:Panel>
                        <asp:Login ID="ctrlLogin" runat="server" OnLoggingIn="ctrlLogin_LoggingIn" EnableViewState="true">
                            <LayoutTemplate>
                                <asp:Panel ID="FormPanel" runat="server" DefaultButton="LoginButton">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <h5>SIGN IN</h5>
                                            <p>if you already have an account, please sign in using your email address and password</p>
                                            <h4>Login</h4>
                                            <label>Email address</label>
                                            <asp:TextBox ID="UserName" CssClass="form-control" runat="server" ValidationGroup="Group1" MaxLength="100"
                                                CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator3" runat="server" ValidationGroup="Group1"
                                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="UserName"></asp:RequiredFieldValidator>
                                            <br />
                                            <label>Password</label>
                                            <asp:TextBox ID="Password" runat="server" CssClass="form-control" ValidationGroup="Group1" MaxLength="50"
                                                CausesValidation="True" TextMode="Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator2" runat="server" ValidationGroup="Group1"
                                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="Password"></asp:RequiredFieldValidator>
                                            <label>
                                                <asp:Label ID="Label1" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.21 %>' Visible="False"></asp:Label>
                                            </label>
                                            <asp:TextBox ID="SecurityCode" runat="server" CssClass="form-control" Visible="False" ValidationGroup="Group1"
                                                CausesValidation="True" EnableViewState="False"></asp:TextBox>
                                            <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator4" runat="server" ControlToValidate="SecurityCode"
                                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.20 %>' ValidationGroup="Group1"
                                                Enabled="False"></asp:RequiredFieldValidator>
                                            <asp:Image ID="SecurityImage" runat="server" Visible="False"></asp:Image>

                                            <div class="checkbox">
                                                <asp:CheckBox ID="RememberMe" runat="server"></asp:CheckBox>Remember me
                           <asp:LinkButton runat="server" CssClass="pull right" OnClick="forgotpasswordLink_Click">Forgot Password?</asp:LinkButton>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <h5>DON'T HAVE AN ACCOUNT?</h5>
                                            <p>Create an account if you don't have one with the JELD WEN Brand store.</p>
                                            <asp:HyperLink ID="SignUpLink" class="account-link" runat="server" Text="Why do I have to create an account?"></asp:HyperLink>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col-md-6">
                                        </div>
                                        <div class="col-md-6">
                                            <asp:Button ID="LoginButton" CssClass="btn btn-md btn-primary  btn-block " CommandName="Login" runat="server" Text="SIGN IN"
                                                ValidationGroup="Group1"></asp:Button>
                                        </div>
                                    </div>
                                </asp:Panel>
                                <asp:CheckBox ID="DoingCheckout" runat="server" Visible="False" />
                                <asp:Label ID="ReturnURL" runat="server" Text="default.aspx" Visible="False" />
                                
                                <%-- change password panel for admin starts here--%>
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
                                        
                                        <label>
                                            <asp:Label ID="Label14" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.10 %>'></asp:Label></label>
                                        <asp:TextBox CssClass="form-control" ID="CustomerEmail" runat="server" ValidationGroup="Group3"
                                            MaxLength="100" CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                                        <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator5" runat="server" ValidationGroup="Group3"
                                            ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="CustomerEmail"></asp:RequiredFieldValidator>
                                       
                                        <label>
                                            <asp:Label ID="Label15" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.33 %>'></asp:Label></label>
                                        <asp:TextBox CssClass="form-control" ID="OldPassword" runat="server" ValidationGroup="Group3"
                                            MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator6" runat="server" ValidationGroup="Group3"
                                            ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="OldPassword"></asp:RequiredFieldValidator>
                                       
                                        <label>
                                            <asp:Label ID="Label16" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.34 %>'></asp:Label>
                                        </label>
                                        <asp:TextBox CssClass="form-control" ID="NewPassword" runat="server" ValidationGroup="Group3"
                                            MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator7" runat="server" ValidationGroup="Group3"
                                            ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="NewPassword"></asp:RequiredFieldValidator>
                                
                                        <label>
                                            <asp:Label ID="Label17" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.35 %>'></asp:Label></label>
                                        <asp:TextBox CssClass="form-control" ID="NewPassword2" runat="server" ValidationGroup="Group3"
                                            MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                                        <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator8" runat="server" ValidationGroup="Group3"
                                            ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="NewPassword2"></asp:RequiredFieldValidator>
                                      
                                        <label>
                                            <asp:Label ID="Label18" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.21 %>' Visible="False"></asp:Label>
                                        </label>
                                        <asp:TextBox CssClass="form-control" ID="SecurityCode2" runat="server" Visible="False" ValidationGroup="Group3"
                                            CausesValidation="True" Width="73px" EnableViewState="False"></asp:TextBox>
                                        <asp:RequiredFieldValidator Display="Dynamic" ID="RequiredFieldValidator9" runat="server" ControlToValidate="SecurityCode2"
                                            ErrorMessage='<%$ Tokens:StringResource,signin.aspx.20 %>' ValidationGroup="Group3"
                                            Enabled="False"></asp:RequiredFieldValidator>
                                   
                                        <div class="form-submit-wrap">
                                            <asp:Button ID="btnChgPwd" CssClass="btn btn-md btn-primary btn-block" OnClick="btnChgPwd_Click" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.36 %>'
                                                ValidationGroup="Group3"></asp:Button>
                                        </div>
                                    </div>
                                </asp:Panel>
                                <%--change password panel for admin ends here--%>
                            </LayoutTemplate>
                        </asp:Login>

                    </div>
                </asp:Panel>
            </div>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
     
            <div class="content-box-03" id="ForgotPasswordPanel" runat="server">
                 <asp:Panel ID="ForgotPasswordExecutepanel1" runat="server" Visible="false">
                                        <div class="notice-wrap signin-executing-text">
                                            <asp:Literal ID="ForgotPaswwordSuccessMessage1" runat="server"></asp:Literal>
                                        </div>
                                    </asp:Panel>
                                    <asp:Panel ID="ForgotPasswordErrorPanel1" runat="server" Visible="False" HorizontalAlign="Left">
                                        <div class="error-wrap">
                                            <asp:Label CssClass="error-large" ID="ForgotPasswordErrorMsgLabel1" runat="server"></asp:Label>
                                        </div>
                                    </asp:Panel>
                <h5>Forgot Your Password?</h5>
                <p>
                    <asp:Label ID="lblForgotPasswordInstructions" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.16 %>'></asp:Label>
                </p>
                <asp:PasswordRecovery ID="ctrlRecoverPassword" runat="server" OnVerifyingUser="ctrlRecoverPassword_VerifyingUser">
                    <UserNameTemplate>
                        <asp:Panel ID="Panel1" runat="server" DefaultButton="btnRequestNewPassword">
                            <div class="row" runat="server">
                                <div class="col-md-12" runat="server">
                                    <label>Email address</label>
                                    <asp:TextBox CssClass="form-control" ID="UserName" runat="server" ValidationGroup="Group2" CausesValidation="True"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="UserName"
                                        ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ValidationGroup="Group2"
                                        Display="Dynamic"></asp:RequiredFieldValidator>
                                    <br />
                                   
                                    <asp:Button class="form-control" CssClass="btn btn-md btn-primary btn-block" ID="btnRequestNewPassword" CommandName="Submit" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.18 %>'
                                        ValidationGroup="Group2"></asp:Button>
                                     <br />
                                    <asp:LinkButton runat="server" OnClick="GoBackToLoginLink_Click">Go Back to Login?</asp:LinkButton>
                                </div>
                            </div>
                        </asp:Panel>
                    </UserNameTemplate>
                </asp:PasswordRecovery>
            </div>
     
    </ContentTemplate>
</asp:UpdatePanel>

















