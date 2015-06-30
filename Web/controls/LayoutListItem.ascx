<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LayoutListItem.ascx.cs" Inherits="AspDotNetStorefront.LayoutListItem" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<asp:Panel ID="pnlMain" Width="180" Height="30" runat="server">
<div style="vertical-align:middle;height:30px;text-align:left;float:left;" runat="server">
<asp:Image ID="imgThumb" Height="30" Width="30" ImageUrl="~/images/nopicture.gif" runat="server"  />
</div>
<div style="vertical-align:middle;height:20px;text-align:left;float:left;margin-left:10px;margin-top:7px;">
<asp:Literal ID="litName" runat="server" Text="Literal: litName" />
</div>
</asp:Panel>

<ajax:HoverMenuExtender ID="hmeLarge" runat="server"
    TargetControlID="imgThumb"
    PopupControlID="pnlLargePopup"
    HoverCssClass="popupHover"
    PopupPosition="Left"
    OffsetX="0"
    OffsetY="0"
    PopDelay="0" />
    
<asp:Panel BorderStyle="None" ID="pnlLargePopup" runat="server">
    <asp:Image ID="imgLarge" runat="server" />
</asp:Panel>