<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AdminMenu.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.AdminMenu" %>

<asp:Panel runat="server" Style="z-index: 800; position: relative;">
<asp:Menu ID="mnuHorizontalNav" runat="server" CssClass="MenuGroup" StaticMenuItemStyle-BackColor="Transparent" 
    StaticMenuStyle-BackColor="Transparent" StaticEnableDefaultPopOutImage="false"
    StaticMenuItemStyle-CssClass="TopMenuItem" StaticHoverStyle-CssClass="TopMenuItemHover"
    DynamicPopOutImageUrl="~/App_Themes/Admin_Default/Images/Arrow.gif" DynamicMenuItemStyle-CssClass="MenuItem"
    DynamicHoverStyle-CssClass="MenuItemHover">
</asp:Menu>
</asp:Panel>