<%@ Page Language="C#" CodeFile="importProductPricing.aspx.cs" Inherits="AspDotNetStorefrontAdmin.importProductPricing" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">   
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="">
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
                                    <asp:Label ID="lblbreadcrumb" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal Text="<%$Tokens:StringResource, admin.importProductPricing.ImportPrices %>" runat="server" />:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <div id="divMain" runat="server">
                                            <span class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.importProductPricing.FileToImport %>" />:</span>
                                            <br />
                                            <asp:FileUpload CssClass="fileUpload" ID="fuFile" runat="server" />
                                            <br />
                                            <br />
                                            <asp:Button ID="btnUpload" runat="server" Text="<%$Tokens:StringResource, admin.common.Submit %>" OnClick="btnUpload_Click" CssClass="normalButtons" />
                                        </div>
                                    </div>
                                </td>
                            </tr>
                            <tr runat="server" id="trResults">
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.importProductPricing.ImportPricesResult %>" />:</font>
                                </td>
                            </tr>

                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <asp:Literal ID="ltResult" runat="server"></asp:Literal>
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