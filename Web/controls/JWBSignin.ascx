<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JWBSignin.ascx.cs" Inherits="controls_JWBSignin" %>
<asp:Label Visible="false" runat="server" ID="HiddenLabel" Text="false"></asp:Label>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" DefaultButton="LoginButton">
            <div id="LoginPanel" runat="server">
                <h4>Login</h4>
                <label>Email address</label>
                <asp:TextBox ID="EmailTextField" CssClass="form-control" runat="server" ValidationGroup="LoginGroup" MaxLength="100"
                    CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                <asp:RequiredFieldValidator Display="Dynamic" ID="LoginRequiredFieldValidator1" runat="server" ValidationGroup="LoginGroup"
                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="EmailTextField"></asp:RequiredFieldValidator>
                <br />
                <label>Password</label>
                <asp:TextBox runat="server" class="form-control" ID="PasswordTextField" ValidationGroup="LoginGroup" MaxLength="50"
                    CausesValidation="True" TextMode="Password" AutoCompleteType="None"></asp:TextBox>

                <asp:RequiredFieldValidator Display="Dynamic" ID="LoginRequiredFieldValidator2" runat="server" ValidationGroup="LoginGroup"
                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="PasswordTextField"></asp:RequiredFieldValidator>
                <div class="checkbox">
                    <asp:LinkButton runat="server" OnClick="forgotpasswordLink_Click" CausesValidation="false">Forgot Password?</asp:LinkButton>
                    <label>
                        <asp:CheckBox ID="RememberMe" runat="server"></asp:CheckBox>Remember me
                    </label>
                </div>
                <br />
                <br />
                <asp:Panel ID="ExecutePanel" runat="server" Visible="false">
                    <div class="notice-wrap signin-executing-text">
                        <asp:Literal ID="SignInExecuteLabel" runat="server"></asp:Literal>
                    </div>
                </asp:Panel>
                <asp:Panel ID="ErrorPanel" runat="server" Visible="False" HorizontalAlign="Left">
                    <div class="error-wrap">
                        <asp:Label CssClass="error-large" ID="ErrorMsgLabel" runat="server"></asp:Label>
                    </div>
                </asp:Panel>
                <asp:Button type="submit" ID="LoginButton" CssClass="btn btn-md btn-primary btn-block" runat="server" ValidationGroup="LoginGroup" OnClick="submitButton_Click" Text="Sign in" />
                <a href="createaccount.aspx" class="account-link">Why do I have to create an account?</a>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" DefaultButton="ForgotPasswordButton">
            <div id="ForgotPasswordPanel" runat="server">
                <h4>Forgot your password?</h4>
                <label>If you forgot your password, to request a new ONE TIME use only password via e-mail, please enter your e-mail address below, and click the 'Request A New Password' button.</label>
                <br />
                <br />
                <label>Email address</label>
                <asp:TextBox ID="ForgotPasswordEmailTextField" CssClass="form-control" runat="server" ValidationGroup="ForgotPasswordGroup" MaxLength="100"
                    CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                <asp:RequiredFieldValidator Display="Dynamic" ID="LoginRequiredFieldValidator3" runat="server" ValidationGroup="ForgotPasswordGroup"
                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="ForgotPasswordEmailTextField"></asp:RequiredFieldValidator>
                <br />
                <asp:Panel ID="ForgotPasswordExecutepanel" runat="server" Visible="false">
                    <div class="notice-wrap signin-executing-text">
                        <asp:Literal ID="ForgotPaswwordSuccessMessage" runat="server"></asp:Literal>
                    </div>
                </asp:Panel>
                <asp:Panel ID="ForgotPasswordErrorPanel" runat="server" Visible="False" HorizontalAlign="Left">
                    <div class="error-wrap">
                        <asp:Label CssClass="error-large" ID="ForgotPasswordErrorMsgLabel" runat="server"></asp:Label>
                    </div>
                </asp:Panel>
                <asp:Button type="submit" ID="ForgotPasswordButton" CssClass="btn btn-md btn-primary btn-block" runat="server" ValidationGroup="ForgotPasswordGroup" OnClick="forgotpasswordButton_Click" Text="Request A New Password" />
                <asp:LinkButton runat="server" OnClick="GoBackToLoginLink_Click" CausesValidation="false">Go Back to Login?</asp:LinkButton>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>




