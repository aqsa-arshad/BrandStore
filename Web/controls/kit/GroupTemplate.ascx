<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTemplate.ascx.cs" Inherits="AspDotNetStorefront.Kit.GroupTemplate" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<div class="kit-group">

    <asp:HiddenField ID="hdfGroupID" runat="server" Value='<%# KitGroup.Id %>' />

    <asp:Panel ID="pnlGroupName" runat="server" CssClass="kit-group-name">

        <asp:ImageButton ID="imgToggleDescription" runat="server"
            ImageUrl="~/images/collapse_blue.jpg"
            Visible='<%# KitGroup.HasDescription || KitGroup.HasImage %>' />
        <asp:Label ID="Label1" runat="server"
            Text='<%# KitGroup.Name %>'>
        </asp:Label>
        <asp:Panel ID="pnlGroupSummary" runat="server">
            <asp:Literal ID="litGroupSummary" runat="server" Text='<%# KitGroup.Summary %>'></asp:Literal>
        </asp:Panel>
    </asp:Panel>

    <asp:Panel ID="pnlGroupContent" runat="server" CssClass="kit-group-content">

        <asp:Panel ID="pnlGroupDescription" runat="server" CssClass="kit-group-description"
            Visible='<%# KitGroup.HasDescription || KitGroup.HasImage %>'
            Height="0">

            <asp:Image ID="imgGroup" runat="server" CssClass="kit-group-image"
                Visible='<%# KitGroup.HasImage %>'
                ImageUrl='<%# KitGroup.ImagePath %>' />

            <asp:Literal ID="Literal2" runat="server" Text='<%# KitGroup.Description %>'></asp:Literal>

        </asp:Panel>

        <ajax:CollapsiblePanelExtender ID="extX" runat="server"
            Enabled='<%# KitGroup.HasDescription || KitGroup.HasImage %>'
            TargetControlID="pnlGroupDescription"
            ExpandControlID="pnlGroupName"
            CollapseControlID="pnlGroupName"
            Collapsed='<%# KitGroup.HasImage ? false : true %>'
            ImageControlID="imgToggleDescription"
            CollapsedImage="~/images/expand_blue.jpg"
            ExpandedImage="~/images/collapse_blue.jpg"
            SuppressPostBack="true">
        </ajax:CollapsiblePanelExtender>

        <asp:UpdatePanel ID="pnlUpdateKitItems" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:PlaceHolder ID="plhKitItemTemplate" runat="server"></asp:PlaceHolder>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</div>

