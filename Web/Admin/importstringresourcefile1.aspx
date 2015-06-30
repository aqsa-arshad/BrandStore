<%@ Page Language="C#" AutoEventWireup="true" CodeFile="importstringresourcefile1.aspx.cs" Inherits="AspDotNetStorefrontAdmin.importstringresourcefile1"
 MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal>
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="help">
        <table width="100%" border="0" cellspacing="0" cellpadding="0" class="toppage">
            <tr>
                <td>
                    <table border="0" cellpadding="5" cellspacing="0">
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.NowIn %>" />
                            </td>
                            <td>
                                <asp:Literal ID="litStage" runat="server" />
                            </td>
                            <td>
                                <td>
                                    <asp:Literal ID="Literal2" runat="server" Text="<%$ Tokens:StringResource, admin.common.View %>" />:
                                </td>
                                <td>
                                    <a href="default.aspx"><asp:Literal ID="Literal3" runat="server" Text="<%$ Tokens:StringResource, admin.common.Home %>" /></a>
                                </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <asp:Panel ID="pnlUpload" runat="server" Width="100%">
            <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
                <tr>
                    <td>
                        <div class="wrapper">
                            <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                                <tr>
                                    <td class="titleTable" style="width: 252px">
                                        <font class="subTitle"><asp:Literal ID="Literal4" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.UploadFile %>" /></font>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="contentTable" valign="top" style="width: 252px">
                                        <div class="wrapper">
                                            <asp:Literal ID="Literal1" runat="server" /><br />
                                            <br />
                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.UseBrowseButton %>" />
                                            <br />
                                            <br />
                                            *<asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.File %>" />
                                            <asp:FileUpload Width="250" ID="fuMain" CssClass="fileUpload" runat="server" /><br />
                                            <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator" ControlToValidate="fuMain"
                                                ErrorMessage="!"></asp:RequiredFieldValidator><br />
                                            <asp:CheckBox ID="chkReplaceExisting" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.ReplaceExisting %>" /><br />
                                            <asp:CheckBox ID="chkLeaveModified" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.LeaveModified %>"/><br />
                                            <br />
                                            <asp:Button ID="btnSubmit" CssClass="normalButtons" runat="server" Text="<%$Tokens:StringResource, admin.common.Upload %>"
                                                OnClick="btnSubmit_Click" />
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            <br />
                            <br />
                            <asp:HyperLink ID="lnkBack1" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.BackToManager %>"></asp:HyperLink>
                        </div>
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="pnlReload" runat="server" Width="100%">
            <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
                <tr>
                    <td>
                        <div class="wrapper">
                            <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                                <tr>
                                    <td class="titleTable">
                                        <font class="subTitle"><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.ReloadFromServer %>" /></font>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="contentTable" valign="top" width="100%">
                                        <div class="wrapper">
                                            <asp:CheckBox ID="chkReloadReplaceExisting" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.ReplaceExisting %>" /><br />
                                            <asp:CheckBox ID="chkReloadLeaveModified" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.LeaveModified %>" /><br />
                                            <br />
                                            <asp:Button ID="btnReload" CssClass="normalButtons" runat="server" Text="<%$Tokens:StringResource, admin.importstringresourcefile1.ReloadReview %>"
                                                OnClick="btnReload_Click" />
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            <br />
                            <br />
                            <asp:HyperLink ID="lnkBack2" runat="server" Text="<%$Tokens:StringResource, admin.stringresources.BackToManager %>"></asp:HyperLink>
                        </div>
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </div>
</asp:Content>
