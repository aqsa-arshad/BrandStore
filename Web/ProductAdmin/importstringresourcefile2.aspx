<%@ Page Language="C#" AutoEventWireup="true" CodeFile="importstringresourcefile2.aspx.cs" Inherits="AspDotNetStorefrontAdmin.importstringresourcefile2" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="help">
        <table border="0" cellpadding="1" cellspacing="0" class="toppage">
            <tr>
                <td align="left" valign="middle" style="height: 36px">
                        <table border="0" cellpadding="5" cellspacing="0" class="breadCrumb3">
                            <tr>
                                <td align="left" valign="middle">
                                    <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.NowIn %>" />
                                </td>
                                <td align="left" valign="middle">
                                   <asp:Literal ID="litStage" runat="server"/>
                                </td>
                                <td align="left" valign="middle">
                                    <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.View %>" />:
                                </td>
                                <td align="left" valign="middle">
                                    <a href="default.aspx"><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Home %>" /></a>
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
    <div id="content">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="5" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"> <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.VerifyFile %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <asp:Literal ID="ltProcessing" runat="server"></asp:Literal>
                                        <p>
                                            <asp:Literal ID="ltVerify" runat="server"></asp:Literal>
                                            <br />
                                            <asp:LinkButton ID="btnProcessFile" runat="Server" OnClick="btnProcessFile_Click"></asp:LinkButton>
                                            <br />
                                            <asp:Literal ID="ltStrings" runat="server"></asp:Literal>
                                        </p>                                        
                                        <asp:Literal runat="server" ID="ltData"></asp:Literal>
                                        <br />
                                        <br />
                                        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" 
                                            CellPadding="4" ForeColor="#333333" GridLines="None">
                                            <RowStyle BackColor="#EFF3FB" />
                                            <Columns>
                                                <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.importstringresourcefile2.Row %>" />
                                                <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.importstringresourcefile2.Status %>" />
                                                <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.common.Name %>" />
                                                <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.common.LocaleSetting %>" />
                                                <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.importstringresourcefile2.StringValue %>" />
                                            </Columns>
                                            <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                            <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
                                            <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                                            <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                            <EditRowStyle BackColor="#2461BF" />
                                            <AlternatingRowStyle BackColor="White" />
                                        </asp:GridView>
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
