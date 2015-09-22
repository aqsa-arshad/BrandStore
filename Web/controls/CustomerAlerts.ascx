<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomerAlerts.ascx.cs" Inherits="CustomerAlerts" %>
<b><u>Customer Alerts</u></b>
<br />
<asp:Label ID="lblCustomerID" runat="server"></asp:Label>
<br />
<asp:Label ID="lblCustomerName" runat="server"></asp:Label>
<br /><br />
<table border="0" cellpadding="0" cellspacing="0" style="font-size:smaller">
    <asp:Repeater ID="rptCustomerAlerts" runat="server" OnItemCommand="rptCustomerAlerts_ItemCommand">
        <ItemTemplate>
            <tr>
                <td>
                    <asp:Label ID="lblAlertTitle" runat="server" Text='<%# Eval("Title") %>' Font-Bold='<%# bool.Parse(Eval("IsRead").ToString()) ? false : true %>'></asp:Label>
                    <asp:Label ID="lblSeprator" runat="server" Text=" - " Font-Bold='<%# bool.Parse(Eval("IsRead").ToString()) ? false : true %>'></asp:Label>
                    <asp:Label ID="lblAlertDescription" runat="server" ForeColor="GrayText" Text='<%# Eval("Description") %>' Font-Bold='<%# bool.Parse(Eval("IsRead").ToString()) ? false : true %>'></asp:Label>
                </td>
                <td>&nbsp;&nbsp;&nbsp;</td>
                <td>
                    <asp:LinkButton ID="btnRead" runat="server" Text="Read" CommandName="Read" CommandArgument='<%# Eval("CustomerAlertStatusID") %>' Visible='<%# bool.Parse(Eval("IsRead").ToString()) ? false : true %>' ></asp:LinkButton>
                </td>
                <td>&nbsp;&nbsp;&nbsp;</td>
                <td>                    
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("CustomerAlertStatusID") %>' ></asp:LinkButton>
                </td>
            </tr>
        </ItemTemplate>
    </asp:Repeater>
</table>

