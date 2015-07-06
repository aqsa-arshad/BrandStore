<%@ Page Language="C#" AutoEventWireup="true" CodeFile="NewsletterMailingListManager.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Admin_NewsletterMailingListManager" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <table align="left">
        <tr>
            <td>
                <asp:TextBox ID="txtSearch" runat = "server" /> &nbsp;
            </td>
            <td>
                <asp:Button ID="cmdSearchName" runat="server" Text="<%$Tokens:StringResource, admin.NewsletterMailingListManager.SearchName %>" onclick="cmdSearch_Click" />
                    <asp:Literal ID='litMultiButtonBreak' runat="server"><br /></asp:Literal>
                <asp:Button ID="cmdSearchEmail" runat="server" Text="<%$Tokens:StringResource, admin.NewsletterMailingListManager.SearchEmail %>" onclick="cmdSearch_Click" />
            </td>
        </tr>
    </table>
    <br /><br /><br />
    <br /><br /><br />
    <asp:GridView ID="grdData" runat="server" onrowcommand="grdData_RowCommand" 
        onselectedindexchanged="grdData_SelectedIndexChanged" AllowPaging="True" 
        onpageindexchanging="grdData_PageIndexChanging" PageSize="5">
        <Columns>
            <asp:TemplateField ShowHeader="true" HeaderText="<%$Tokens:StringResource, admin.common.Unsubscribe %>">
                <ItemTemplate>
                    <asp:Button runat="server" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "EmailAddress") %>' CommandName="Unsubscribe" Text="<%$Tokens:StringResource, admin.common.Unsubscribe %>" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
            &nbsp;
        </EmptyDataTemplate>
    </asp:GridView>
    <asp:Literal ID='litNoRecords' runat="server" />
</asp:Content>
