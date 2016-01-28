<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JWBSignin.ascx.cs" Inherits="controls_JWBSignin" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<asp:Label Visible="false" runat="server" ID="HiddenLabel" Text="false"></asp:Label>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" DefaultButton="LoginButton">
            <div id="LoginPanel" runat="server">
                <h4></h4>
                <asp:Label class="blue-color" runat="server"> <aspdnsf:Topic ID="SignInInstructions" runat="server" TopicName="signinheading"  Visible="false" /></asp:Label>
                <label>
                    <asp:Label runat="server" Text='<%$ Tokens:StringResource,signin.aspx.10 %>'></asp:Label></label>
                <asp:TextBox ID="EmailTextField" CssClass="form-control" runat="server" ValidationGroup="LoginGroup" MaxLength="100"
                    CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                <asp:RequiredFieldValidator Display="Dynamic" ID="LoginRequiredFieldValidator1" runat="server" ValidationGroup="LoginGroup"
                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="EmailTextField"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="EmailTextField" Display="Dynamic" 
                    ValidationGroup="LoginGroup" ErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.17 %>" ValidationExpression="^[a-zA-Z0-9][-\w\.\+]*@([a-zA-Z0-9][\w\-]*\.)+[a-zA-Z]{2,4}$" />
                <br />
                <label><asp:Label ID="Label2" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.12 %>'></asp:Label></label>
                <asp:TextBox runat="server" class="form-control" ID="PasswordTextField" ValidationGroup="LoginGroup" MaxLength="50"
                    CausesValidation="True" TextMode="Password" AutoCompleteType="None"></asp:TextBox>
                <asp:RequiredFieldValidator Display="Dynamic" ID="LoginRequiredFieldValidator2" runat="server" ValidationGroup="LoginGroup"
                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="PasswordTextField"></asp:RequiredFieldValidator>
                <div class="checkbox">
                    <asp:LinkButton runat="server" OnClick="forgotpasswordLink_Click" Font-Underline="true" CausesValidation="false" Text='<%$ Tokens:StringResource,signin.aspx.15 %>'></asp:LinkButton>
                    <div class="clearfix visible-xs"></div>
                    <label><asp:CheckBox ID="RememberMe" runat="server"></asp:CheckBox>Remember me</label>
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
                <asp:LinkButton ID="createAccountLink" runat="server" CssClass="account-link" CausesValidation="false" Text='<%$ Tokens:StringResource,signin.aspx.7 %>' OnClick="CreateAccountLink_Click"></asp:LinkButton>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:UpdatePanel runat="server">

    <ContentTemplate>
        <asp:Panel runat="server" DefaultButton="ForgotPasswordButton">
            <div id="ForgotPasswordPanel" runat="server">
                <h4></h4>
                <p><asp:Label ID="lblForgotPasswordInstructions" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.16 %>'></asp:Label></p>
                <label><asp:Label runat="server" Text='<%$ Tokens:StringResource,signin.aspx.11 %>'></asp:Label></label>
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
                <asp:Button type="submit" ID="ForgotPasswordButton" CssClass="btn btn-md btn-primary btn-block" runat="server" ValidationGroup="ForgotPasswordGroup" OnClick="forgotpasswordButton_Click" Text='<%$ Tokens:StringResource,signin.aspx.18 %>' />
                <asp:LinkButton runat="server" OnClick="GoBackToLoginLink_Click" CssClass="account-link" CausesValidation="false" Text='<%$ Tokens:StringResource,signin.aspx.13 %>'></asp:LinkButton>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>




