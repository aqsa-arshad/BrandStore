<%@ Page Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" AutoEventWireup="true" CodeFile="customeralerts.aspx.cs" Inherits="AspDotNetStorefrontAdmin.customeralerts" MaintainScrollPositionOnPostback="true" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

    <table border="0" cellpadding="0" cellspacing="0" width="100%">
        <asp:Repeater ID="rptCustomerAlerts" runat="server" OnItemCommand="rptCustomerAlerts_ItemCommand">
            <HeaderTemplate>
                <tr class="table-header">
                    <td style="width:5%">ID</td>
                    <td style="width:10%">Customer Level ID</td>
                    <td style="width:15%">Customer Level Name</td>
                    <td style="width:10%">Title</td>
                    <td style="width:25%">Description</td>
                    <td style="width:6%">Alert Date</td>
                    <td style="width:7%">Created Date</td>
                    <td style="width:7%">Modified Date</td>
                    <td style="width:5%">IsDeleted</td>
                    <td style="width:5%">Edit</td>
                    <td style="width:5%">Delete</td>
                </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr class="table-row2">
                    <td><%# Eval("CustomerAlertID") %></td>
                    <td><%# Eval("CustomerLevelID") %></td>
                    <td><%# Eval("Name") %></td>
                    <td><%# Eval("Title") %></td>
                    <td><%# Eval("Description") %></td>
                    <td><%# Eval("AlertDate") %></td>
                    <td><%# Eval("CreatedDate") %></td>
                    <td><%# Eval("ModifiedDate") %></td>
                    <td><%# Eval("IsDeleted") %></td>
                    <td>
                        <asp:Button ID="btnEdit" CssClass="normalButtons" runat="server" Text="Edit" CommandName="Edit" CommandArgument='<%# Eval("CustomerAlertID") %>'></asp:Button>
                    </td>
                    <td>
                        <asp:Button ID="btnDelete" CssClass="normalButtons" runat="server" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("CustomerAlertID") %>'></asp:Button>
                    </td>
                </tr>
            </ItemTemplate>
            <AlternatingItemTemplate>
                <tr class="table-alternatingrow2">
                    <td><%# Eval("CustomerAlertID") %></td>
                    <td><%# Eval("CustomerLevelID") %></td>
                    <td><%# Eval("Name") %></td>
                    <td><%# Eval("Title") %></td>
                    <td><%# Eval("Description") %></td>
                    <td><%# Eval("AlertDate") %></td>
                    <td><%# Eval("CreatedDate") %></td>
                    <td><%# Eval("ModifiedDate") %></td>
                    <td><%# Eval("IsDeleted") %></td>
                    <td>
                        <asp:Button ID="btnEdit" CssClass="normalButtons" runat="server" Text="Edit" CommandName="Edit" CommandArgument='<%# Eval("CustomerAlertID") %>'></asp:Button>
                    </td>
                    <td>
                        <asp:Button ID="btnDelete" CssClass="normalButtons" runat="server" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("CustomerAlertID") %>'></asp:Button>
                    </td>
                </tr>
            </AlternatingItemTemplate>
        </asp:Repeater>
    </table>

    <asp:Repeater ID="rptPager" runat="server">
        <ItemTemplate>
            <asp:LinkButton ID="lnkPage" runat="server" Text='<%#Eval("Text") %>' CommandArgument='<%# Eval("Value") %>'
                OnClick="Page_Changed" OnClientClick='<%# !Convert.ToBoolean(Eval("Enabled")) ? "return false;" : "" %>'></asp:LinkButton>
        </ItemTemplate>
    </asp:Repeater>

    <br /><br />
    <asp:Button ID="btnAddNewAlert" runat="server" CssClass="normalButtons" Text="Add New Alert" OnClick="btnAddNewAlert_Click"></asp:Button>

</asp:Content>

