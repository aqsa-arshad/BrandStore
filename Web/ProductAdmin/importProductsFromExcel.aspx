<%@ Page Language="C#" CodeFile="importProductsFromExcel.aspx.cs" Inherits="AspDotNetStorefrontAdmin.importProductsFromExcel" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
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
        <p><b><font color="red"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.BackupDBBeforeImports %>" /></font></b><br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.UndoImportDisclaimer %>" /></p>                    
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.ImportProductsFromExcel %>" />:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <div id="divMain" runat="server">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.importProductsFromExcel.ExcelInfo %>" />:
                                            <asp:FileUpload ID="fuFile" runat="server" CssClass="fileUpload"  />
                                            <br /><br />
                                            <asp:Button ID="btnUpload" runat="server" Text="<%$Tokens:StringResource, admin.common.Upload %>" OnClick="btnUpload_Click" CssClass="normalButtons" />
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="wrapper" runat="server" id="divReview">
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ReviewUpload %>" />:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <b><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ReviewLogStatus %>" /></b>
                                        <br /><br />
                                        <asp:Button ID="btnAccept" runat="server" Text="<%$Tokens:StringResource, admin.common.ClickHereToAcceptImportUC %>" CssClass="normalButtons" OnClick="btnAccept_Click" />
                                        &nbsp;
                                        <asp:Button ID="btnUndo" runat="server" Text="<%$Tokens:StringResource, admin.common.ClickHereToUndoImportUC %>" CssClass="normalButtons" OnClick="btnUndo_Click" />
                                        <hr noshade="noshade" />
                                        <asp:Literal ID="ltResults" runat="server"></asp:Literal>
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
