<%@ Page Language="C#" AutoEventWireup="true" CodeFile="viewshipment.aspx.cs" Inherits="AspDotNetStorefrontAdmin.viewshipment" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">


<table style="width: 402px">
            <tr>
                <td style="width: 100px">
                    <asp:Literal ID="litImportErrors" runat="server" />
                </td>
            </tr>
            <tr>
                <td style="width: 100px">
                    <asp:GridView ID="dview" runat="server" AllowPaging="false" AllowSorting="false" HorizontalAlign="Left" Width="50%" >
                        <HeaderStyle CssClass="gridHeader" Font-Bold="True" />
                        <FooterStyle CssClass="gridFooter" />
                        <EditRowStyle CssClass="gridEdit2" />
                        <SelectedRowStyle CssClass="DataCellGrid" />
                        <RowStyle CssClass="gridRowPlain" />
                        <AlternatingRowStyle CssClass="gridAlternatingRowPlain" />
                    </asp:GridView>                     
                </td>
            </tr>
            <tr>
                <td style="width: 100px">
                </td>
            </tr>
        </table>
        
</asp:Content>
