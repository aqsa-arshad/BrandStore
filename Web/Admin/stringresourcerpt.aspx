<%@ Page Language="C#" AutoEventWireup="true" CodeFile="stringresourcerpt.aspx.cs" Inherits="AspDotNetStorefrontAdmin.stringresourcerpt" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

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
                                    <font class="subTitle"><asp:Label ID="ReportLabel" runat="server">Missing Strings:</asp:Label></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%" style="height: 81px">
                                    <div class="wrapper">
                                        <asp:Literal ID="ltLocale" runat="server"></asp:Literal>
                                        <br /><asp:Button ID="btnExportExcel" runat="server" CssClass="normalButtons" Text="Export to Excel" OnClick="btnExportExcel_Click"/><br />
                                        &nbsp;<asp:Literal ID="ltData" runat="server"></asp:Literal>
                                        
                                        <br />
                                        <asp:Button ID="btnSubmit" runat="server" CssClass="normalButtons" Text="Add/Update" OnClick="btnSubmit_Click" />
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