﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TextItems.ascx.cs" Inherits="AspDotNetStorefront.Kit.TextItems"  %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls.Extenders" Assembly="AspDotNetStorefrontControls" %>

 <asp:Repeater ID="rptItems" runat="server" DataSource='<%# KitGroup.SelectableItems %>'
    onitemdatabound="rptItems_ItemDataBound" >
         
    <ItemTemplate>
    
        <asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />        
        
		<div class="kit-group-item-wrap">
			<asp:Label ID="lblItemName" runat="server" 
				CssClass='<%# this.CssClassIf(Container.DataItemAs<KitItemData>().HasImage, "kit-group-item-name-label") %>' 
				Text='<%# KitItemDisplayText(Container.DataItemAs<KitItemData>()) %>'></asp:Label>
            
			<asp:Literal ID="litStockHint" runat="server" Text='<%# StockHint(KitGroup.FirstSelectedItem) %>' ></asp:Literal>
         </div>

        <asp:Panel ID="pnlCollapsible" runat="server">
            <asp:Panel ID="pnlKitItemImage" runat="server" Visible='<%# Container.DataItemAs<KitItemData>().HasImage %>' >        
                <asp:Image ID="imgKitItem" runat="server" CssClass="kit-group-item-image"            
                    ImageUrl='<%# Container.DataItemAs<KitItemData>().ImagePath %>' />            
            </asp:Panel>
            <asp:Panel ID="pnlKitItemDescription" runat="server" Visible='<%# Container.DataItemAs<KitItemData>().HasDescription %>'>
                <asp:Literal ID="ltKitItemDescription" runat="server" Text='<%# Container.DataItemAs<KitItemData>().Description %>' />
            </asp:Panel>
        </asp:Panel>
        
        <ajax:CollapsiblePanelExtender ID="extCollapseImage" runat="server" 
            TargetControlID="pnlCollapsible" 
            CollapseControlID="lblItemName" 
            ExpandControlID="lblItemName"
            Collapsed="true">
        </ajax:CollapsiblePanelExtender>
        
        
        
        <div class="kit-item-text">
            <asp:TextBox ID="txtKitItemText" runat="server" 
                AutoPostBack="true"
                CssClass="form-control"
                TextMode='<%# DetermineTextMode() %>'
                Columns='<%# DetermineTextColumns() %>'
                Text='<%# Container.DataItemAs<KitItemData>().TextOption %>'
                Enabled='<%# Container.DataItemAs<KitItemData>().CanBeSelected %>' >
            </asp:TextBox>
                
            <aspdnsf:TextStateChangePostbackBehavior ID="extTextOption" runat="server" TargetControlID="txtKitItemText" Timeout="50000" />
        </div>
                
    </ItemTemplate>    
</asp:Repeater>



