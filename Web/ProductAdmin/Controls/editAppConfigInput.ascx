<%@ Control Language="C#" AutoEventWireup="true" CodeFile="editAppConfigInput.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.EditAppConfigInput" %>

<div class="configValue">
    <asp:TextBox ID="tbxValue" runat="server" Visible="false" Width="250px" CssClass="appconfigTextBox" />
    <asp:RadioButtonList ID="rblValue" runat="server" Visible="false">
        <asp:ListItem Value="false" Text="No" />
        <asp:ListItem Value="true" Text="Yes" />
    </asp:RadioButtonList>
    <asp:DropDownList ID="ddValue" runat="server" Visible="false" />
    <asp:CheckBoxList ID="cblValue" runat="server" Visible="false" CssClass="antialternate" />
    <asp:PlaceHolder ID="phValidators" runat="server" />
    <asp:HiddenField ID="AppConfigId" runat="server" />
    <asp:HiddenField ID="StoreId" runat="server" />
    <asp:Panel ID="pnlInheritWarning" runat="server" Visible="false">
        <small class="atomInheritWarning">(Unassigned for <em><asp:Literal ID="litStoreName" runat="server" /></em>. Currently using default.)</small>
    </asp:Panel>
</div>
      