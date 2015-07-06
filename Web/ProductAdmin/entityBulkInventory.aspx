<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.entityBulkInventory" CodeFile="entityBulkInventory.aspx.cs" MaintainScrollPositionOnPostback="true" %>
<%@ OutputCache Duration="1" Location="none" %>
<head runat="server">
</head>
<body>
    <form runat="server">
        <div style="float:right;padding:5px;">
            <asp:Button ID="topBtnInventoryUpdate" Text="Inventory Update" CssClass="normalButtons" runat="server" OnClick="BtnInventoryUpdate_click" />
        </div>
            <asp:Literal ID="ltBody" runat="server" />
        <div style="float:right;padding:5px;">
            <asp:Button ID="bottomBtnInventoryUpdate" Text="Inventory Update" CssClass="normalButtons" runat="server" OnClick="BtnInventoryUpdate_click" />
        </div>
    </form>
</body>
