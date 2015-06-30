<%@ Page Language="c#" Inherits="AspDotNetStorefront.lat_signin" CodeFile="lat_signin.aspx.cs"
    MasterPageFile="~/App_Templates/skin_1/template.master" MaintainScrollPositionOnPostback="true" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap affiliate-page">
            <h1>
                <asp:Literal ID="AppConfig_AffiliateProgramName2" runat="server" Mode="PassThrough"></asp:Literal></h1>

            <div class="form affiliate-signin-form">
                <asp:TextBox ID="ReturnURL" runat="server" TextMode="SingleLine" Visible="false"></asp:TextBox>
                <div class="group-header form-header signin-header">
                    Login Information
                </div>
                <div class="form-text">Don't have an account? <a href="lat_signup.aspx">Click here</a> to sign up.</div>
                <div class="form-text">
                    <asp:Label ID="lblErrMsg" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>
                    <asp:Label ID="lblNote" runat="server" Font-Bold="true" ForeColor="blue"></asp:Label>
                    <asp:Label ID="lblReqPwdErr" runat="server" Font-Bold="true" ForeColor="red" Visible="true"></asp:Label>
                </div>

                <div class="form-text">
                    Enter your
                    <asp:Literal ID="AppConfig_AffiliateProgramName3" runat="server" Mode="PassThrough"></asp:Literal>
                    e-mail address and password below:
                </div>

                <div class="form-group">
                    <label>
                        My e-mail address is:
                    </label>

                    <asp:TextBox ID="EMail" CssClass="form-control" runat="server" MaxLength="100" ValidationGroup="sigin" CausesValidation="true"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="ReqValEmail" runat="server" ValidationGroup="signin" Display="none" ControlToValidate="EMail" EnableClientScript="true" ErrorMessage="Please enter your e-mail address"></asp:RequiredFieldValidator>
                    <aspdnsf:EmailValidator ID="RegExValEmail" runat="server" ValidationGroup="signin" Display="none" ControlToValidate="EMail" EnableClientScript="true" ErrorMessage="E-Mail address is invalid, please enter a valid e-mail address"></aspdnsf:EmailValidator>

                </div>
                <div class="form-group">
                    <label>My password is:</label>
                    <asp:TextBox ID="Password" runat="server" TextMode="Password" CssClass="form-control" ValidationGroup="sigin" CausesValidation="true"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ValidationGroup="signin" Display="none" ControlToValidate="Password" EnableClientScript="true" ErrorMessage="Please enter your password"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    Remember Password:<asp:CheckBox ID="PersistLogin" Checked="true" runat="server" />
                </div>
                <div class="form-submit-wrap">
                    <asp:Button ID="btnSignIn" runat="server" CssClass="button call-to-action" OnClientClick="Page_ValidationActive=true;" Text="Sign in" CausesValidation="true" ValidationGroup="signin" OnClick="btnSignIn_Click" />
                    <asp:ValidationSummary DisplayMode="BulletList" ID="ValSummary" ShowMessageBox="true" runat="server" ShowSummary="false" ValidationGroup="signin" EnableClientScript="true" />
                </div>
            </div>
            <div class="form forgot-password">
                <div class="group-header form-header forgot-password-header">
                    Forgot Your Password?
                </div>
                <div class="form-group">
                    <label>My e-mail address is:</label>
                    <asp:TextBox ID="ResetPwdEMail" CssClass="form-control" runat="server" Columns="35"></asp:TextBox>
                </div>
                <div class="form-submit-wrap">
                    <asp:Button ID="btnLostPassword" CssClass="button" runat="server" Text="Request Password" OnClick="btnLostPassword_Click" />
                </div>
            </div>
            <asp:Panel ID="pnlSigninSuccess" runat="server" Visible="false" Style="margin-top: 100px;" HorizontalAlign="Center">
                <asp:Label ID="lblSigninSuccess" runat="server" Font-Bold="true"></asp:Label>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>

