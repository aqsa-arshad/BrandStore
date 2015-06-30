<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileReadOnlyItems.ascx.cs" Inherits="AspDotNetStorefront.Kit.MobileReadOnlyItems" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<ul data-role="listview">
    <asp:Repeater ID="rptReadOnlyKitItems" runat="server"
        DataSource='<%# KitGroup.SelectableItems %>'>
        <ItemTemplate>
            <li>
                 <asp:Label ID="lblItemName" runat="server" 
                    CssClass='<%# this.CssClassIf(Container.DataItemAs<KitItemData>().HasImage, "kit-group-item-name-label") %>' 
                    Text='<%# KitItemDisplayText(Container.DataItemAs<KitItemData>()) %>'></asp:Label>
                    
                <asp:Literal ID="litStockHint" runat="server" Text='<%# StockHint(KitGroup.FirstSelectedItem) %>' ></asp:Literal>
                  
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
        
            </li>
        </ItemTemplate>
    </asp:Repeater>
</ul>
