<%@ Page Language="C#" AutoEventWireup="true" CodeFile="editfeed.aspx.cs" Inherits="AspDotNetStorefrontAdmin.editfeed"
    MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div style="margin-bottom: 5px; margin-top: 5px;">
        <asp:Literal ID="ltError" runat="server"></asp:Literal>
    </div>
    <p>
        <b>
            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.header %>" />
            <br />
            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.fieldsmarked %>" />
        </b>
    </p>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle">
                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.ProductFeeds %>" />
                                    </font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapperLeft">
                                        <table>
                                            <tr>
                                                <td>
                                                    <asp:Label ID="Label1" Font-Bold="true" Text="Editing Feed:" runat="server"></asp:Label>&nbsp;<asp:Label ID="pageheading" Font-Bold="true" ForeColor="blue" runat="server"></asp:Label>
                                                </td>
                                            </tr>
                                        </table>
                                        <table>
                                            <tr>
                                                <td></td>
                                                <td>
                                                    <asp:Label runat="server" ID="lblMessage" ForeColor="Red"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.store %>" /></font></td>
                                                <td>
                                                    <asp:DropDownList ID="cboStore" runat="server" CausesValidation="true"></asp:DropDownList>
                                                    <asp:Image ID="imgStoreID" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editfeed.tooltip.imgStoreID %>" />
                                                    <asp:CustomValidator ID="reqStore" runat="server" Display="Dynamic" OnServerValidate="ValidateStoreID" ErrorMessage="<%$Tokens:StringResource,admin.editfeed.msgselectstore %>"></asp:CustomValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.Feedname %>" /></font></td>
                                                <td>
                                                    <asp:TextBox ID="txtFeedName" Columns="30" MaxLength="100" runat="server" CausesValidation="true"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="reqFeedName" ControlToValidate="txtFeedName" Display="Dynamic" EnableClientScript="true" ErrorMessage="<%$Tokens:StringResource,admin.editfeed.msgEnterFeedName %>" runat="server">!!</asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.XmlPackage %>" /></font></td>
                                                <td>
                                                    <asp:DropDownList ID="XmlPackage" runat="server" CausesValidation="true">
                                                        <asp:ListItem Text="Select a package" Value="" Selected="True"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <asp:CustomValidator ID="reqXmlPackage" runat="server" Display="Dynamic" OnServerValidate="ValidateXmlPackage" ErrorMessage="<%$Tokens:StringResource,admin.editfeed.tooltip.imgFtpUserName %>"></asp:CustomValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editfeed.AutoFTP %>" /></font></td>
                                                <td>
                                                    <asp:RadioButtonList ID="CanAutoFtp" runat="server" RepeatDirection="Horizontal">
                                                        <asp:ListItem Text="Yes" Value="1" Selected="True"></asp:ListItem>
                                                        <asp:ListItem Text="No" Value="0"></asp:ListItem>
                                                    </asp:RadioButtonList></td>
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPUsername %>" /></font></td>
                                                <td>
                                                    <asp:TextBox ID="txtFtpUserName" Columns="30" MaxLength="100" runat="server"></asp:TextBox>
                                                    <asp:Image ID="imgFtpUserName" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editfeed.tooltip.imgFtpUserName %>" />
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label ID="Label3" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPPassword %>" /></font></td>
                                                <td>
                                                    <asp:TextBox ID="txtFtpPwd" Columns="30" MaxLength="100" runat="server"></asp:TextBox>
                                                    <asp:Image ID="imgFtpPwd" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editfeed.tooltip.imgFtpPwd %>" />
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label ID="Label4" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPserver %>" /></font></td>
                                                <td>
                                                    <asp:TextBox ID="txtFtpServer" Columns="30" MaxLength="100" runat="server" CausesValidation="true"></asp:TextBox>
                                                    <asp:Image ID="imgFtpServer" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editfeed.tooltip.imgFtpServer %>" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label ID="Label5" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPPort %>" /></font></td>
                                                <td>
                                                    <asp:TextBox ID="txtFtpPort" Columns="30" MaxLength="5" Text="21" runat="server"></asp:TextBox>
                                                    <asp:Image ID="imgFtpPort" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editfeed.tooltip.imgFtpPort %>" />
                                                    <asp:CustomValidator ID="PortIsNumber" Display="Dynamic" OnServerValidate="ValidatePort" runat="server" ErrorMessage="<%$Tokens:StringResource,admin.editfeed.msgPortNumber %>"></asp:CustomValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><font class="subTitleSmall">
                                                    <asp:Label ID="Label6" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.FTPFilename %>" /></font></td>
                                                <td>
                                                    <asp:TextBox ID="txtFtpFileName" Columns="30" MaxLength="1000" runat="server" CausesValidation="true"></asp:TextBox>
                                                    <asp:Image ID="imgFtpFileName" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editfeed.tooltip.imgFtpFileName %>" />
                                                </td>
                                            </tr>
                                        </table>
                                        <br />
                                        <br />
                                        <asp:Button ID="btnSubmit" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.buttonCreateFeed %>" CssClass="normalButtons" OnClick="btnSubmit_OnClick" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                        <asp:Button ID="btnExecFeed" runat="server" Text="<%$Tokens:StringResource, admin.editfeed.buttonExecuteFeed %>" CssClass="normalButtons" OnClick="btnExecFeed_OnClick" /><br />
                                        <br />
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
