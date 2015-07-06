<%@ Page Language="C#" AutoEventWireup="true" CodeFile="mailingtest.aspx.cs" Inherits="AspDotNetStorefrontAdmin.mailingTest"
    MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="controls/StoreSelector.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal runat="server" ID="ltStyles"></asp:Literal>
    <div id="container" style="width: 100%;">
        <div id="div1" style="float: left;">
            <table border="0" cellpadding="1" cellspacing="0">
                <tr>
                    <td>
                        <div>
                            <table border="0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td>
                                        <b>
                                            <asp:Literal ID="ltPreEntity" runat="server" Visible="false"></asp:Literal></b>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>
                        <aspdnsf:StoreSelector runat="server" ID="ssOne" AutoPostBack="true" OnSelectedIndexChanged="ssOne_SelectedIndexChanged"
                            SelectMode="SingleDropDown" ShowDefaultForAllStores="true" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <div>
                            <span style="color: Red; font-weight: bold;">
                                <asp:Literal ID="ltError" runat="server"></asp:Literal></span>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <div style="clear: both; padding-bottom: 15px;">
        </div>
        <div id="content" style="margin-right: 10px;">
            <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                <tr>
                    <td align="left">
                        <table border="0" cellpadding="0" cellspacing="0" class="">
                            <tr>
                                <td>
                                    <div class="">
                                        <table cellpadding="0" cellspacing="1" border="0">
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerDNS %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtMailMe_Server" runat="server"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ControlToValidate="txtMailMe_Server" ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterDNS %>"
                                                        ID="RequiredFieldValidator3" ValidationGroup="MainAdvanced" EnableClientScript="true"
                                                        SetFocusOnError="true" runat="server" Display="Static"></asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">
                                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerUsername %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtMailServerUser" runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">
                                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerPassword %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtMailServerPwd" runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerTCP %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtMailServerPort" runat="server"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ControlToValidate="txtMailServerPort" ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterTCP %>"
                                                        ID="RequiredFieldValidator6" ValidationGroup="MainAdvanced" EnableClientScript="true"
                                                        SetFocusOnError="true" runat="server" Display="Static"></asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">
                                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.MailServerRequiresSSL %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:RadioButtonList ID="rblMailServerSSL" runat="server" RepeatColumns="2"
                                                        RepeatDirection="horizontal">
                                                        <asp:ListItem Value="1" Text="Yes"></asp:ListItem>
                                                        <asp:ListItem Value="0" Text="No" Selected="true"></asp:ListItem>
                                                    </asp:RadioButtonList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    &nbsp;
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.ReceiptEmailSendsFrom %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtReceiptFrom" runat="server"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ControlToValidate="txtReceiptFrom" ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterEmail %>"
                                                        ID="RequiredFieldValidator4" ValidationGroup="MainAdvanced" EnableClientScript="true"
                                                        SetFocusOnError="true" runat="server" Display="Static"></asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">
                                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.ReceiptEmailSendsFromName %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtReceiptFromName"
                                                        runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.ReceiptEmailWithXML %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddXmlPackageReceipt" Width="350" runat="Server">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.SendReceiptEmail %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:RadioButtonList ID="rblSendReceipts" runat="server" RepeatColumns="2"
                                                        RepeatDirection="horizontal">
                                                        <asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.Yes %>" Selected="true" />
                                                        <asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.No %>" />
                                                    </asp:RadioButtonList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    &nbsp;
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.NewOrderSendToEmail %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtOrderNotificationTo"
                                                        runat="server"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ControlToValidate="txtOrderNotificationTo" ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterAdminEmail %>"
                                                        ID="RequiredFieldValidator5" ValidationGroup="MainAdvanced" EnableClientScript="true"
                                                        SetFocusOnError="true" runat="server" Display="Static"></asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">
                                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.NewOrderSendToName %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtOrderNotificationToName"
                                                        runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">
                                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.NewOrderSendFromEmail %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtOrderNotificationFrom"
                                                        runat="server"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ControlToValidate="txtOrderNotificationFrom"
                                                        ErrorMessage="<%$Tokens:StringResource, admin.mailingtest.EnterAdminEmail %>"
                                                        ID="RequiredFieldValidator9" ValidationGroup="MainAdvanced" EnableClientScript="true"
                                                        SetFocusOnError="true" runat="server" Display="Static"></asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">
                                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.NewOrderSendFromName %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox Width="350" CssClass="singleNormal" ID="txtOrderNotificationFromName"
                                                        runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">
                                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.SendNewOrderWithXML %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddXmlPackageOrderNotifications" Width="350" runat="Server">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.SendNewOrderNotifications %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:RadioButtonList ID="rblSendOrderNotifications" runat="server" RepeatColumns="2"
                                                        RepeatDirection="horizontal">
                                                        <asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.Yes %>" Selected="true" />
                                                        <asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.No %>" />
                                                    </asp:RadioButtonList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    &nbsp;
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.ShippedEmailsWithXML %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddXmlPackageShipped" Width="350" runat="Server">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.mailingtest.SendShippedEmails %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:RadioButtonList ID="rblSendShippedNotifications" runat="server" RepeatColumns="2"
                                                        RepeatDirection="horizontal">
                                                        <asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.Yes %>" Selected="true" />
                                                        <asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.No %>" />
                                                    </asp:RadioButtonList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    &nbsp;
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <div style="width: 100%; text-align: left; padding-top: 10px;">
                                                        <table>
                                                            <tr>
                                                                <td>
                                                                    <asp:Button runat="server" ID="btnUpdateAppConfigs" Width="200" CssClass="normalButtons" Text="Save"
                                                                        ValidationGroup="MainAdvanced" OnClick="btnUpdateAppConfigs_Click" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Button runat="server" ID="btnSendTestReceipt" Width="200" CssClass="normalButtons"
                                                                        Text="<%$Tokens:StringResource, admin.mailingtest.SendTestReceipt %>" ValidationGroup="MainAdvanced"
                                                                        OnClick="btnSendTestReceipt_Click" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Button runat="server" ID="btnSendNewOrderNotification" Width="200" CssClass="normalButtons"
                                                                        Text="<%$Tokens:StringResource, admin.mailingtest.SendTestOrderNotification %>"
                                                                        ValidationGroup="MainAdvanced" OnClick="btnSendNewOrderNotification_Click" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Button runat="server" ID="btnSendTestShipped" Width="200" CssClass="normalButtons"
                                                                        Text="<%$Tokens:StringResource, admin.mailingtest.SendTestShippedEmail %>" ValidationGroup="MainAdvanced"
                                                                        OnClick="btnSendTestShipped_Click" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Button runat="server" ID="btnSendAll" Width="200" CssClass="normalButtons"
                                                                        Text="<%$Tokens:StringResource, admin.mailingtest.TestAll %>" ValidationGroup="MainAdvanced"
                                                                        OnClick="btnSendAll_Click" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td>
                    </td>
                </tr>
            </table>
            
            <asp:ValidationSummary ID="validationSummaryAdvanced" ValidationGroup="MainAdvanced"
                DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" runat="server" />
        </div>
    </div>
</asp:Content>
