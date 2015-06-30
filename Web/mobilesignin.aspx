<%@ Page Language="c#" Inherits="AspDotNetStorefront.mobilesignin" CodeFile="mobilesignin.aspx.cs" MasterPageFile="~/App_Templates/skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="login" Src="~/Controls/Signin.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlLogin" runat="server">
        <asp:Panel ID="CheckoutPanel" runat="server">
            <div id="CheckoutSequence">
                <asp:Literal runat="server" id="CheckoutHeader"></asp:Literal>
            </div>
        </asp:Panel>
        <asp:Literal ID="ltlSigninHeader" runat="server" Text="<%$ Tokens:Topic, SigninPageHeader %>" />
        <ul data-role="listview">
            <li>
                <asp:HyperLink ID="mobileSignUpLink" runat="server" Text="<%$ Tokens:StringResource,signin.aspx.7 %>"></asp:HyperLink>
            </li>
            <li class="group" style="color:Red;" data-role="list-divider">
                <asp:Panel ID="ErrorPanel" runat="server" Visible="False" HorizontalAlign="Left">
                    <asp:Label CssClass="errorLg" ID="ErrorMsgLabel" runat="server"></asp:Label>
                </asp:Panel>
            </li>
        </ul>
        <asp:Login ID="ctrlLogin" runat="server" OnLoggingIn="ctrlLogin_LoggingIn" EnableViewState="true" Width="100%">
            <LayoutTemplate>
                <asp:Panel ID="FormPanel" runat="server" Width="100%">
                    <ul data-role="listview">
                        <li>
                            <table cellspacing="5" cellpadding="0" border="0">
                                <tbody>
                                    <tr valign="baseline">
                                        <td valign="baseline" align="right">
                                            <font class="LightCellText">*E-Mail:</font>
                                        </td>
                                        <td valign="baseline" align="left">
                                            <asp:TextBox ID="UserName" runat="server" ValidationGroup="Group1" Columns="30" MaxLength="100"
                                                CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ValidationGroup="Group1"
                                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="UserName" Display="Dynamic"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td valign="baseline" align="right">
                                            <font class="LightCellText">*Password:</font>
                                        </td>
                                        <td valign="baseline" align="left">
                                            <asp:TextBox ID="Password" runat="server" ValidationGroup="Group1" Columns="30" MaxLength="50"
                                                CausesValidation="True" TextMode="Password"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ValidationGroup="Group1"
                                                ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="Password" Display="Dynamic"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </li>
                        <li>
                            <p align="left">
                                <asp:Button ID="LoginButton" CssClass="fullwidthshortgreen" CommandName="Login" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.19 %>' ValidationGroup="Group1"></asp:Button>
                            </p>
                        </li>
                    </ul>
                </asp:Panel>
                <asp:CheckBox ID="DoingCheckout" runat="server" Visible="False" />
                <asp:Label ID="ReturnURL" runat="server" Text="default.aspx" Visible="False" />
                <asp:Panel ID="pnlChangePwd" runat="server" Visible="false" DefaultButton="btnChgPwd">
                        <ul data-role="listview">
                            <li class="group" data-role="list-divider">
                                <font class="LightCellText">
                                    <asp:Label ID="Label10" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.22 %>' Font-Bold="true" ForeColor="red"></asp:Label>
                                </font>
                                
                                <asp:Label ID="lblPwdChgErr" runat="server" Font-Bold="true" ForeColor="red" Visible="false"></asp:Label>
                            </li>
                            <li>
                                <table width="100%">
                                    <tbody>
                                        <tr class="MediumCell" valign="top">
                                            <td class="LightCell" align="left" width="90%">
                                                <table cellspacing="5" cellpadding="0" width="100%" border="0">
                                                    <tbody>
                                                        <tr valign="baseline">
                                                            <td colspan="2">
                                                                <b><font class="LightCellText">
                                                                    <asp:Label ID="Label13" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.23 %>'></asp:Label>
                                                                </font></b>
                                                            </td>
                                                        </tr>
                                                        <tr valign="baseline">
                                                            <td valign="middle" align="right">
                                                                <font class="LightCellText">
                                                                    <asp:Label ID="Label14" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.10 %>'></asp:Label></font>
                                                            </td>
                                                            <td valign="middle" align="left">
                                                                <asp:TextBox ID="CustomerEmail" runat="server" ValidationGroup="Group3" Columns="30"
                                                                    MaxLength="100" CausesValidation="True" AutoCompleteType="Email"></asp:TextBox>
                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ValidationGroup="Group3"
                                                                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ControlToValidate="CustomerEmail"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td valign="middle" align="right">
                                                                <font class="LightCellText">
                                                                    <asp:Label ID="Label15" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.33 %>'></asp:Label></font>
                                                            </td>
                                                            <td valign="middle" align="left">
                                                                <asp:TextBox ID="OldPassword" runat="server" ValidationGroup="Group3" Columns="30"
                                                                    MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ValidationGroup="Group3"
                                                                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="OldPassword"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td valign="middle" align="right">
                                                                <font class="LightCellText">
                                                                    <asp:Label ID="Label16" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.34 %>'></asp:Label></font>
                                                            </td>
                                                            <td valign="middle" align="left">
                                                                <asp:TextBox ID="NewPassword" runat="server" ValidationGroup="Group3" Columns="30"
                                                                    MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ValidationGroup="Group3"
                                                                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="NewPassword"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td valign="middle" align="right">
                                                                <font class="LightCellText">
                                                                    <asp:Label ID="Label17" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.35 %>'></asp:Label></font>
                                                            </td>
                                                            <td valign="middle" align="left">
                                                                <asp:TextBox ID="NewPassword2" runat="server" ValidationGroup="Group3" Columns="30"
                                                                    MaxLength="50" CausesValidation="True" TextMode="Password"></asp:TextBox>
                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ValidationGroup="Group3"
                                                                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.4 %>' ControlToValidate="NewPassword2"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr valign="baseline">
                                                            <td valign="middle" align="right">
                                                                <font class="LightCellText">
                                                                    <asp:Label ID="Label18" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.21 %>'
                                                                        Visible="False"></asp:Label></font>
                                                            </td>
                                                            <td valign="middle" align="left">
                                                                <asp:TextBox ID="SecurityCode2" runat="server" Visible="False" ValidationGroup="Group3"
                                                                    CausesValidation="True" Width="73px" EnableViewState="False"></asp:TextBox>
                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ControlToValidate="SecurityCode2"
                                                                    ErrorMessage='<%$ Tokens:StringResource,signin.aspx.20 %>' ValidationGroup="Group3"
                                                                    Enabled="False"></asp:RequiredFieldValidator>
                                                            </td>
                                                        </tr>
                                                        <tr valign="baseline">
                                                            <td valign="middle" align="center" colspan="2">
                                                                <asp:Image ID="Image1" runat="server" Visible="False"></asp:Image>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr valign="top">
                                            <td align="left" width="90%">
                                                <p align="left">
                                                    <asp:Button ID="btnChgPwd" CssClass="fullwidthshortgreen" OnClick="btnChgPwd_Click" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.36 %>'
                                                        ValidationGroup="Group3"></asp:Button>
                                                </p>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </li>
                        </ul>
                    </asp:Panel>
            </LayoutTemplate>
        </asp:Login>
        
        
        
        <asp:Panel ID="ExecutePanel" runat="server" Width="100%">
            <ul data-role="listview">
                <li>
                    <asp:Literal ID="SignInExecuteLabel" runat="server"></asp:Literal>
                </li>
            </ul>
        </asp:Panel>
        
        <asp:PasswordRecovery ID="ctrlRecoverPassword" runat="server" OnVerifyingUser="ctrlRecoverPassword_VerifyingUser" Width="100%">
            <UserNameTemplate>
            
                
                <ul data-role="listview">
                    <li class="group" data-role="list-divider">
                        <asp:Label ID="Label6" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.15 %>'></asp:Label>
                    </li>
                    <li>
                        <table>
                            <tbody>
                                <tr valign="baseline">
                                    <td align="right" valign="middle">
                                        <font class="LightCellText">*E-Mail:</font>
                                    </td>
                                    <td valign="middle">
                                        <%--<asp:TextBox ID="ForgotEMail" runat="server" ValidationGroup="Group2" CausesValidation="True"
                                            Columns="30"></asp:TextBox>--%>
                                        <asp:TextBox ID="UserName" runat="server" ValidationGroup="Group2" CausesValidation="True"
                                            Columns="30"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="UserName"
                                            ErrorMessage='<%$ Tokens:StringResource,signin.aspx.3 %>' ValidationGroup="Group2"
                                            Display="Dynamic"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </li>
                    <li>
                        <p align="left">
                            <asp:Button ID="btnRequestNewPassword" CssClass="fullwidthshortgreen" CommandName="Submit" runat="server" Text='<%$ Tokens:StringResource,signin.aspx.18 %>' ValidationGroup="Group2"></asp:Button>
                        </p>
                    </li>
                </ul>
                
            </UserNameTemplate>
        </asp:PasswordRecovery> 
    </asp:Panel>
</asp:Content>
