<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConfigurationAtom.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.ConfigurationAtomControl" %>
<%@ Register TagPrefix="aspdnsf" TagName="editAppConfigAtom" Src="editAppConfigAtom.ascx" %>

<asp:Literal ID="litHTMLHeader" runat="server" />
<div>
    <asp:Repeater ID="repAppConfigs" runat="server" OnItemDataBound="repAppConfigs_ItemDataBound">
        <HeaderTemplate>
            <table width="100%">
        </HeaderTemplate>
        <ItemTemplate>
            <aspdnsf:editAppConfigAtom runat="server" id="AppConfigAtom" HideTableNode="true" ShowSaveButton="false" />
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <%if (this.ShowSaveButton){ %>
    <div style="text-align:right;">
        <asp:Button ID="btnSave" OnClick="btnSave_Click" runat="server" Text="Save" />
    </div>
    <% } %>
</div>