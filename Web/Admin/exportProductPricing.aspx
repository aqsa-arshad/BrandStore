<%@ Page Language="C#" CodeFile="exportProductPricing.aspx.cs" Inherits="AspDotNetStorefrontAdmin.exportProductPricing" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">   
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <asp:Label ID="lblBreadcrumb" runat="server" />
                                </td>
                            </tr>

                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.ExportPrices %>" />:</font>
                                </td>
                            </tr>
                        
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <div id="divMain" runat="server">
                                            <span class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterCategory %>" />:</span>
                                            <br /><asp:DropDownList ID="ddCategory" runat="server"></asp:DropDownList>
                                            <br />
                                            <span class="subTitleSmall">
                                                <br />
                                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterSection %>" />:</span>
                                            <br /><asp:DropDownList ID="ddSection" runat="server"></asp:DropDownList>
                                            <br />
                                            <span class="subTitleSmall">
                                                <br />
                                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterManufacturer %>" />:</span>
                                            <br /><asp:DropDownList ID="ddManufacturer" runat="server"></asp:DropDownList>
                                            <div id="divDistributor" runat="server">
                                                <span class="subTitleSmall">
                                                    <br />
                                                    <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterDistributor %>" />:</span>
                                                <br /><asp:DropDownList ID="ddDistributor" runat="server"></asp:DropDownList>
                                            </div>
                                            <div id="divGenre" runat="server">
                                                <span class="subTitleSmall">
                                                    <br />
                                                    <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterGenre %>" /></span>
                                                <br /><asp:DropDownList ID="ddGenre" runat="server"></asp:DropDownList>
                                            </div>
                                            <div id="divVector" runat="server">
                                                <span class="subTitleSmall">
                                                    <br />
                                                    <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.FilterVector %>" />:</span>
                                                <br /><asp:DropDownList ID="ddVector" runat="server"></asp:DropDownList>
                                            </div>
                                            <br />
                                            <br />
                                            <span class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.exportProductPricing.SelectFormat %>" />:</span>
                                            <br />
                                            <asp:RadioButtonList ID="rblExport" runat="server">
                                                <asp:ListItem Value="xls" Text="<%$Tokens:StringResource, admin.exportProductPricing.Excel %>" Selected="true"></asp:ListItem>
                                                <asp:ListItem Value="xml" Text="<%$Tokens:StringResource, admin.exportProductPricing.XML %>"></asp:ListItem>
                                                <asp:ListItem Value="csv" Text="<%$Tokens:StringResource, admin.exportProductPricing.CSV %>"></asp:ListItem>
                                            </asp:RadioButtonList>
                                            <br />
                                            <br />
                                            <asp:Button ID="btnUpload" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Submit %>" OnClick="btnUpload_Click" />
                                        </div>
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