<%@ Control Language="C#" AutoEventWireup="true" CodeFile="editlayout.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.EditLayout" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<style type="text/css">
    .style1
    {
        width: 234px;
    }
</style>
<div class='<%# this.CssClass %>'>
    <asp:Panel ID="pnlLayoutEdit" runat="server" Width="95%" Height="550">
        <asp:Panel ID="pnlError" runat="server" Visible='<%# HasErrors %>' CssClass="config_error">
            <asp:Label ID="lblError" runat="server"></asp:Label>
        </asp:Panel>
        <table width="100%" runat="server">
            <tr valign="top">
                <td class="style1" width="400">
                    <table>
                        <tr align="left" valign="middle">
                            <td align="right" valign="middle" width="100">
                                <asp:Label ID="lblName" AssociatedControlID="txtName" Text="Name: " runat="server" />
                            </td>
                            <td align="left" valign="middle">
                                <asp:TextBox ID="txtName" Text="" Width="300" runat="server" />
                            </td>
                        </tr>
                        <tr align="left" valign="middle">
                            <td align="right" valign="top" width="100">
                                <asp:Label ID="lblDescription" runat="server" AssociatedControlID="txtDescription" Text="Description:" />
                            </td>
                            <td align="left" valign="middle">
                                <asp:TextBox ID="txtDescription" runat="server" Width="300" Height="200" TextMode="MultiLine" Text="" />
                                <br /><br />
                            </td>
                        </tr>
                        <tr>
                            <td align="right" valign="top" width="100">
                                <asp:Label ID="lblThumb" Text="Thumbnail:" runat="server" />
                            </td>
                            <td>
                                <asp:Image ID="imgThumb" Height="75" Width="75" runat="server" />
                                <br />
                                <asp:FileUpload ID="fuLayoutThumb" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right" valign="top" width="100">
                                <asp:Label ID="lblLarge" Text="Large:" runat="server" />
                            </td>
                            <td>
                                <asp:Image ID="imgLarge" Width="150" Height="150" runat="server" />
                                <br />
                                <asp:FileUpload ID="fuLayoutLarge" runat="server" />
                            </td>
                        </tr>
                    </table>
                </td>
                <td width="1" style="border-style: dotted; border-color: Gray; border-width: thin;">
                </td>
                <td width="500" align="left" valign="top">
                    <table width="500">
                        <tr>
                            <td align="left" valign="middle" colspan="2">
                                <asp:Label ID="lblLayout" runat="server" AssociatedControlID="txtLayout" Text="Layout Markup:" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox ID="txtLayout" TextMode="MultiLine" runat="server" Width="100%" Height="475" Text=""/>
                            </td>
                        </tr>
                        <tr>
                            <td align="right" valign="middle" width="100">
                                <asp:Label ID="lblLayoutFile" runat="server" AssociatedControlID="fuLayoutFile" Text="Layout File: " />
                            </td>
                            <td align="left" valign="middle">
                                <asp:FileUpload ID="fuLayoutFile" runat="server" />
                            </td>
                        </tr>
                    </table>
                    <br />
                    
                </td>
            </tr>
        </table>
    </asp:Panel>
</div>

<ajax:HoverMenuExtender ID="hmeThumb" runat="Server"
    TargetControlID="imgThumb"
    PopupControlID="pnlThumbPopup"
    HoverCssClass="popupHover"
    PopupPosition="Top"
    OffsetX="80"
    OffsetY="10"
    PopDelay="0" />
    
<asp:Panel BorderStyle="None" ID="pnlThumbPopup" runat="server">
    <asp:Image ID="imgPopThumb" runat="server" />
</asp:Panel>

<ajax:HoverMenuExtender ID="hmeLarge" runat="Server"
    TargetControlID="imgLarge"
    PopupControlID="pnlLargePopup"
    HoverCssClass="popupHover"
    PopupPosition="Top"
    OffsetX="80"
    OffsetY="10"
    PopDelay="0" />
    
<asp:Panel BorderStyle="None" ID="pnlLargePopup" runat="server">
    <asp:Image ID="imgPopLarge" runat="server" />
</asp:Panel>
