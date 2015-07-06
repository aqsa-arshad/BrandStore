<%@ Page Language="C#" AutoEventWireup="true" CodeFile="signin.aspx.cs" Inherits="signin" Theme="Admin_Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>AspDotNetStorefront Admin for Store:<asp:Literal ID="ltStoreName" runat="server"></asp:Literal></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
       <asp:Literal runat="server" ID="literalSelfReloadScript"></asp:Literal> 
</head>
<body class="loginbody">

    <script type="text/javascript">
        if (top != self) {
            top.location = self.location;
        }
    </script>

    <form id="SigninForm" runat="server">
    <table cellpadding="0" cellspacing="0" height="100%" width="100%" border="0">
        <tr>
            <td height="600px" valign="middle" align="center" >
                <table border="0" width="100%" cellpadding="0" cellspacing="0">
                    <tr>
                        <td align="center">
                            <table width="513" cellpadding="0" cellspacing="0" border="0">
                                <tr>
                                    <td valign="bottom" height="140px" width="513px">
                                    <asp:Image ID="imglogo" ImageUrl="~/App_Themes/Admin_Default/images/logoblue.png" runat="server" />
                                    </td>
                                </tr>
                                <tr>
                                    <td align="left" class="loginbgmid">
                                        <table width="482" cellpadding="0" cellspacing="0" border="0">
                                            <tr>
                                                <td width="100%">
                                                    <asp:Panel ID="pnlSignIn" runat="server" HorizontalAlign="center" DefaultButton="btnSubmit" Visible="true">
                                                        <table cellpadding="0" cellspacing="0" width="100%" border="0">
                                                            <tr>
                                                                <td align="center">
                                                                    <asp:Literal ID="ltError" runat="Server"></asp:Literal>
                                                                    <table cellspacing="0" cellpadding="0" width="80%" border="0">
                                                                        <tr valign="top">
                                                                            <td align="left" height="18" valign="middle">
                                                                                <font class="DarkCellText" size="2"><b>&nbsp;<asp:Label ID="Label4" runat="server"
                                                                                    Text="<%$Tokens:StringResource, signin.aspx.8 %>"></asp:Label></b></font>
                                                                            </td>
                                                                        </tr>
                                                                        <tr valign="top">
                                                                            <td align="left" height="5px" valign="middle">
                                                                                &nbsp;
                                                                            </td>
                                                                        </tr>
                                                                        <tr valign="top">
                                                                            <td align="left" width="100%">
                                                                                <table cellspacing="0" cellpadding="5" width="100%" border="0">
                                                                                    <tr valign="baseline">
                                                                                        <td colspan="2">
                                                                                            <b>
                                                                                                <asp:Label ID="Label3" runat="server" Text="<%$Tokens:StringResource, signin.aspx.11 %>" ForeColor="#FFFFFF"></asp:Label>
                                                                                            </b>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td align="right" valign="middle">
                                                                                            <asp:Label ID="Label5" runat="server" Text="<%$Tokens:StringResource, signin.aspx.10 %>" ForeColor="#FFFFFF"></asp:Label>
                                                                                        </td>
                                                                                        <td class="logintext" valign="middle">
                                                                                            <asp:TextBox style="border: none; background-color: Transparent;padding-top:3px" ID="txtEMail" runat="server"
                                                                                                Height="15px" Width="200px" CausesValidation="True"></asp:TextBox>&nbsp;
                                                                                            &nbsp;<asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtEMail"
                                                                                                ErrorMessage="!!" Font-Bold="True" SetFocusOnError="True" ValidationGroup="LOGIN"></asp:RequiredFieldValidator>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr valign="baseline">
                                                                                        <td align="right" valign="middle">
                                                                                            <asp:Label ID="Label7" runat="server" Text="<%$Tokens:StringResource, signin.aspx.12 %>" ForeColor="#FFFFFF"></asp:Label>
                                                                                        </td>
                                                                                        <td class="logintext">
                                                                                            <asp:TextBox style="border: none; background-color: Transparent;padding-top:3px" ID="txtPassword"
                                                                                                TextMode="password" Height="15px" Width="200px" runat="server" EnableViewState="False"
                                                                                                CausesValidation="True"></asp:TextBox>&nbsp;
                                                                                            &nbsp;<asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="txtPassword"
                                                                                                ErrorMessage="!!" Font-Bold="True" SetFocusOnError="True" ValidationGroup="LOGIN"></asp:RequiredFieldValidator>
                                                                                        </td>
                                                                                    </tr>                                                                                    
                                                                                </table>
                                                                                <table>
                                                                                    <asp:Literal ID="ltSecurityCode" runat="server"></asp:Literal>
                                                                                    <tr valign="baseline" id="RowPersistLogin" runat="server" visible="false">
                                                                                        <td colspan="2" align="center">
                                                                                            <asp:CheckBox ID="cPersistLogin" runat="server" Visible="false" />
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr valign="baseline" id="RowSecurityCode" runat="server" visible="false">
                                                                                        <td valign="middle" align="right">
                                                                                            <asp:Label ID="Label9" runat="server" Text="" Visible="False" ForeColor="#FFFFFF"></asp:Label>
                                                                                        </td>
                                                                                        <td class="securitycodetext">
                                                                                            <asp:TextBox Style="border: none; background-color: Transparent;" ID="SecurityCode" runat="server" Visible="False" ValidationGroup="Group1"
                                                                                                CausesValidation="True" Height="15px" Width="65px" EnableViewState="False"></asp:TextBox>&nbsp;
                                                                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SecurityCode"
                                                                                                ErrorMessage="<%$Tokens:StringResource, signin.aspx.20 %>" ValidationGroup="Group1" Enabled="False"></asp:RequiredFieldValidator>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr valign="baseline" id="RowSecurityImage" runat="server" visible="false">
                                                                                        <td valign="middle" align="center" colspan="2">
                                                                                            <asp:Image ID="SecurityImage" runat="server" Visible="False"></asp:Image>
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                            </td>
                                                                        </tr>
                                                                        <tr valign="top">
                                                                            <td style="padding-right: 23px">
                                                                                <p align="right">
                                                                                    <asp:ImageButton ImageUrl="~/App_Themes/Admin_Default/images/signin.png" ID="btnSubmit" runat="Server"
                                                                                        OnClick="btnSubmit_Click" ValidationGroup="LOGIN"></asp:ImageButton>
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                    <table cellspacing="0" cellpadding="0" width="80%" border="0">
                                                                        <tr>
                                                                                <td>
                                                                                    <p align="left">
                                                                                        <asp:Label ID="Label6" runat="server" Text="<%$Tokens:StringResource, signin.aspx.15 %>" Font-Bold="true" ForeColor="#FFFFFF"></asp:Label>&nbsp;</p>
                                                                                </td>
                                                                            </tr>
                                                                        <tr valign="top">
                                                                            <td align="left" height="18" valign="middle">
                                                                                <font class="DarkCellText" size="2"><b>&nbsp;<asp:Label ID="Label2" runat="server"
                                                                                    Text="<%$Tokens:StringResource, signin.aspx.17 %>"></asp:Label></b></font>
                                                                            </td>
                                                                        </tr>
                                                                        <tr valign="top">
                                                                            <td align="left">
                                                                                <table cellspacing="0" cellpadding="5" width="100%" border="0">
                                                                                    <tr valign="baseline">
                                                                                        <td align="right" valign="middle">
                                                                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="txtEMailRemind"
                                                                                                ErrorMessage="!!" Font-Bold="True" SetFocusOnError="True" ValidationGroup="FORGOT"></asp:RequiredFieldValidator>
                                                                                            <asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, signin.aspx.10 %>" ForeColor="#FFFFFF"></asp:Label>
                                                                                        </td>
                                                                                        <td class="verificationtext" valign="middle" align="right">
                                                                                            <nobr>
                                                                                            <asp:TextBox  Style="border: none; background-color: Transparent;padding-top:3px;" ID="txtEMailRemind"
                                                                                                Height="15px" Width="160" runat="Server" CausesValidation="True"></asp:TextBox>&nbsp;
                                                                                            </nobr>
                                                                                        </td>
                                                                                        <td align="left" valign="middle">
                                                                                        <asp:Image ID="imgToolTipHelp" ImageUrl="~/App_Themes/Admin_Default/images/help.png" runat="server" style="cursor: normal;" />                                                                                            
                                                                                        </td>
                                                                                    </tr>
                                                                                </table>
                                                                            </td>
                                                                        </tr>
                                                                        <tr valign="top">
                                                                            <td align="left" style="padding-right: 13px;">
                                                                                <p align="right">
                                                                                    <asp:ImageButton ImageUrl="~/App_Themes/Admin_Default/images/requestpwd.png" ID="btnRequestNewPassword" 
                                                                                       OnClick="btnRequestNewPassword_Click" ValidationGroup="FORGOT" runat="Server"></asp:ImageButton>
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>                                                                
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnlChangePwd" runat="server" HorizontalAlign="center" Visible="false" DefaultButton="btnChangePwd">
                                                        <table cellpadding="0" cellspacing="0" width="100%" border="0">
                                                            <tr>
                                                                <td align="center">
                                                                    <table cellpadding="5" cellspacing="0" width="80%" border="0">
                                                                        <tr>
                                                                            <td colspan="2">
                                                                                <asp:Label ID="lblPwdChgErrMsg" runat="server" Font-Bold="true" ForeColor="red" Visible="false"></asp:Label><br />
                                                                                <br />
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td colspan="2">
                                                                                <asp:Label ID="lblChgPwdHeader" CssClass="DarkCellText" Text="Change Password" runat="server"
                                                                                    Font-Bold="true"></asp:Label>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td align="right" valign="middle">
                                                                                <asp:Label ID="lblChgPwdEmail" runat="server" Text="Email Address:" ForeColor="#FFFFFF"></asp:Label>
                                                                            </td>
                                                                            <td class="chgpwdtext">
                                                                                <asp:TextBox Style="border: none; background-color: Transparent;padding-top:3px" Width="200" Height="15" ID="txtEmailNewPwd" runat="server" Columns="30"></asp:TextBox>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td align="right" valign="middle">
                                                                                <asp:Label ID="lblOldPwd" runat="server" Text="Old Password:" ForeColor="#FFFFFF"></asp:Label>
                                                                            </td>
                                                                            <td class="chgpwdtext">
                                                                                <asp:TextBox  Style="border: none; background-color: Transparent;padding-top:3px" Width="200" Height="15" ID="txtOldPwd" runat="server" Columns="30" TextMode="Password"></asp:TextBox>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td align="right" valign="middle">
                                                                                <asp:Label ID="lblNewPwd" runat="server" Text="New Password:" ForeColor="#FFFFFF"></asp:Label>
                                                                            </td>
                                                                            <td class="chgpwdtext">
                                                                                <asp:TextBox Style="border: none; background-color: Transparent;padding-top:3px" Width="200" Height="15" ID="txtNewPwd" runat="server" Columns="30" TextMode="Password"></asp:TextBox>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td align="right" valign="middle">
                                                                                <asp:Label ID="lblConfirmPwd" runat="server" Text="Confirm Password:" ForeColor="#FFFFFF"></asp:Label>
                                                                            </td>
                                                                            <td class="chgpwdtext">
                                                                                <asp:TextBox Style="border: none; background-color: Transparent;padding-top:3px" Width="200" Height="15" ID="txtConfirmPwd" runat="server" Columns="30" TextMode="Password"></asp:TextBox>
                                                                            </td>
                                                                        </tr>
                                                                        <tr valign="top">
                                                                            <td align="right" colspan="2" style="padding-right: 30px">
                                                                                <p align="right">
                                                                                    <asp:ImageButton ImageUrl="~/App_Themes/Admin_Default/images/changepwd.png" ID="btnChangePwd" runat="Server"
                                                                                        OnClick="btnChangePwd_OnClick" ValidationGroup="LOGIN"></asp:ImageButton>
                                                                                </p>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="loginfooter" height="31px" width="482px">&nbsp;
                                    </td>
                                </tr>
                            </table>
                            <asp:ValidationSummary ID="validationSummary" runat="server" EnableClientScript="true"
                                ShowMessageBox="true" ShowSummary="false" Enabled="true" ValidationGroup="vgLogin" />
                            <asp:ValidationSummary ID="ValidationSummary1" runat="server" EnableClientScript="true"
                                ShowMessageBox="true" ShowSummary="false" Enabled="true" ValidationGroup="vgPassword" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
