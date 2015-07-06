<%@ Page language="c#" Inherits="AspDotNetStorefrontAdmin.auditlog" CodeFile="auditlog.aspx.cs" EnableEventValidation="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

    <p>
        <asp:Label ID="lblCustomer" runat="server" Font-Bold="true"></asp:Label>
    </p>
    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" PageSize="100" Width="100%" OnPageIndexChanging="GridView1_PageIndexChanging">
        <HeaderStyle Font-Bold="True" Font-Size="XX-Small" HorizontalAlign="Left" VerticalAlign="Top" BackColor="#E0E0E0" />
        <RowStyle HorizontalAlign="Left" VerticalAlign="Top" />
    </asp:GridView>
</asp:Content>

