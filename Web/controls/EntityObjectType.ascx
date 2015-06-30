<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityObjectType.ascx.cs" Inherits="AspDotNetStorefront.TemplateEditors.EntityObjectTypeControl" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register src="EntitySelectTextBox.ascx" tagname="EntitySelectTextBox" tagprefix="ctrl" %>

<table>
    <tr>
        <td align="right" valign="middle" >
            Entity Type:
        </td>
        <td align="left" valign="middle" >
            <asp:DropDownList ID="cboEntityType" runat="server" OnSelectedIndexChanged="cboEntityType_SelectedIndexChanged" AutoPostBack="true" >
            </asp:DropDownList>
        </td>
    </tr>
    <tr>
        <td align="right" valign="middle" >
            Entity ID:
        </td>
        <td align="left" valign="middle" >
            <ctrl:EntitySelectTextBox ID="ctrlSelectEntity" runat="server" />
        </td>
    </tr>
</table>
