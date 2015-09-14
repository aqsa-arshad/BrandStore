<%@ Page Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" AutoEventWireup="true" CodeFile="customeralerts.aspx.cs" Inherits="AspDotNetStorefrontAdmin.customeralerts" MaintainScrollPositionOnPostback="true" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <table>
        <tr>
            <td>
                Add Alerts Form ...
            </td>
        </tr>
    </table>
    <table border="0" cellpadding="0" cellspacing="0" width="100%">
        <asp:Repeater ID="rptCustomerAlerts" runat="server">
            <HeaderTemplate>
                <tr class="table-header">
                    <td>CustomerAlertID</td>
                    <td>CustomerID</td>
                    <td>Customer Name</td>
                    <td>Title</td>
                    <td>Description</td>
                    <td>AlertDate</td>
                    <td>IsRead</td>
                    <td>IsDeleted</td>
                    <td>CreatedDate</td>
                    <td>ModifiedDate</td>
                    <td>CreatedBy</td>
                    <td>ModifiedBy</td>
                </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr class="table-row2">
                    <td><%# Eval("CustomerAlertID") %></td>
                    <td><%# Eval("CustomerID") %></td>
                    <td><%# Eval("CustomerName") %></td>
                    <td><%# Eval("Title") %></td>
                    <td><%# Eval("Description") %></td>
                    <td><%# Eval("AlertDate") %></td>
                    <td><%# Eval("IsRead") %></td>
                    <td><%# Eval("IsDeleted") %></td>
                    <td><%# Eval("CreatedDate") %></td>
                    <td><%# Eval("ModifiedDate") %></td>
                    <td><%# Eval("CreatedBy") %></td>
                    <td><%# Eval("ModifiedBy") %></td>
                </tr>
            </ItemTemplate>
            <AlternatingItemTemplate>
                <tr class="alternatingrow2">
                    <td><%# Eval("CustomerAlertID") %></td>
                    <td><%# Eval("CustomerID") %></td>
                    <td><%# Eval("CustomerName") %></td>
                    <td><%# Eval("Title") %></td>
                    <td><%# Eval("Description") %></td>
                    <td><%# Eval("AlertDate") %></td>
                    <td><%# Eval("IsRead") %></td>
                    <td><%# Eval("IsDeleted") %></td>
                    <td><%# Eval("CreatedDate") %></td>
                    <td><%# Eval("ModifiedDate") %></td>
                    <td><%# Eval("CreatedBy") %></td>
                    <td><%# Eval("ModifiedBy") %></td>
                </tr>
            </AlternatingItemTemplate>
        </asp:Repeater>
    </table>
</asp:Content>

