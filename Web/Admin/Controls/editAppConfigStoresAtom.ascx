<%@ Control Language="C#" AutoEventWireup="true" CodeFile="editAppConfigStoresAtom.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.EditAppConfigStoresAtom" %>
<%@ Register TagPrefix="aspdnsf" TagName="editAppConfigAtom" Src="editAppConfigAtom.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="GeneralInfo" Src="GeneralInfo.ascx" %>

<% if (!this.HideTableNode)
   {%>
<table  class='<%# this.CssClass %>'>
<% } %>
    <tr>
        <td style="text-align:right;">
            <div style="float:right;">
                <aspdnsf:GeneralInfo id="AppConfigInfo" runat="server" />
            </div>
            <div class="configTitle">
                <b><asp:Label ID="lblTitle" runat="server" /></b>
                &nbsp;:&nbsp;
            </div>
        </td>
        <td>
            <aspdnsf:editAppConfigAtom JustInput="true" id="eacaDefaultConfig" runat="server" />
        </td>
    </tr>
<% if (!this.HideTableNode)
   { %>
</table>
<% } %>