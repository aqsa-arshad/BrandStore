<%@ Control Language="C#" AutoEventWireup="true" CodeFile="minicart.ascx.cs" Inherits="AspDotNetStorefront.MinicartControl" %>
<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

 <asp:ScriptManagerProxy ID="scrptProxy" runat="server">
 </asp:ScriptManagerProxy>
<asp:Panel ID="pnlMiniCart" runat="server">
   
    <div style="position: relative; padding-bottom: 25px; z-index: 9999" >
        <div style="background-color: #ffffff; position: absolute; top: 0; right: 0; z-index: 9999">
            <div id="divMiniCart" class="mini-cart-wrap">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server"
                    ChildrenAsTriggers="true" UpdateMode="Always">
                    <ContentTemplate>
                        <cc1:CollapsiblePanelExtender ID="extCollapseMinicart" runat="server" 
                            TargetControlID="pnlMinicartControl"
                            ExpandControlID="pnlMinicartHeader" 
                            CollapseControlID="pnlMinicartHeader" Collapsed="true"
                            TextLabelID="btnViewMiniCart" 
                            ExpandedText='<%$ Tokens:StringResource, minicart.cartCount %>'
                            CollapsedText='<%$ Tokens:StringResource, minicart.cartCount %>' 
                            SuppressPostBack="true" >
                        </cc1:CollapsiblePanelExtender>
                        <asp:Panel ID="pnlMinicartHeader" runat="server" CssClass="mini-cart-collapse-header">
                            <div style="padding-right: 15px;">
                                <div style="float: right">
                                    <asp:LinkButton ID="lnkViewMiniCart" runat="server" Text='<%$ Tokens:StringResource, minicart.cartCount %>'></asp:LinkButton>
                                    <asp:Label ID="lblItemCount" runat="server">(0)</asp:Label>
                                </div>
                                <div>
                                    <asp:Image ID="imgCartIcon2" runat="server" ImageUrl='<%$ Tokens:SkinImage, icons/cart.gif %>' />
                                </div>
                            </div>
                        </asp:Panel>
                        
                        <asp:Panel ID="pnlMinicartControl" runat="server" CssClass="mini-cart-collapse-body" Height="0" >
                            <aspdnsfc:ShoppingCartControl ID="ctrlMiniCart" runat="server" OnMiniCartItemUpdate="ctrlMiniCart_OnMiniCartItemUpdate"
                                OnItemDeleting="ctrlMiniCart_OnItemDeleting" 
                                ProductHeaderText='<%$ Tokens:StringResource, minicart.ProductHeader %>'
                                DisplayMode="MiniCart"
                                MaxMinicartLatestItems='<%$ Tokens:AppConfigUSInt, Minicart.MaxLatestCartItemsCount %>'
                                QuantityHeaderText='<%$ Tokens:StringResource, minicart.QuantityHeader %>' 
                                SubTotalHeaderText='<%$ Tokens:StringResource, minicart.SubTotalHeader %>'
                                StaticHeaderSeparatorImageUrl="~/images/tab-sep-minicart.gif">
                                
                                <LineItemSettings LinkToProductPageInCart='<%$ Tokens:AppConfigBool, LinkToProductPageInCart %>'
                                    SKUCaption='<%$ Tokens:StringResource, showproduct.aspx.21 %>' 
                                    GiftRegistryCaption='<%$ Tokens:StringResource, shoppingcart.cs.92 %>'
                                    ItemNotesCaption='<%$ Tokens:StringResource, shoppingcart.cs.86 %>' 
                                    ItemNotesColumns='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaCols %>'
                                    ItemNotesRows='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaRows %>'
                                    AllowShoppingCartItemNotes='<%$ Tokens:AppConfigBool, AllowShoppingCartItemNotes %>'
                                    ShowPicsInCart='<%$ Tokens:AppConfigBool, ShowPicsInMiniCart %>' 
                                    ShowEditButtonInCartForKitProducts='<%$ Tokens:AppConfigBool, ShowEditButtonInCartForKitProducts %>'
                                    ShowMultiShipAddressUnderItemDescription="true" 
                                    MinimumQuantityNotificationText='<%$ Tokens:StringResource, minicart.MinimumQuantity %>'/>
                                    
                                <MinicartSummarySetting CalculateShippingDuringCheckout="false" 
                                    CalculateTaxDuringCheckout="false"
                                    ShowTotal="true" 
                                    ShowShipping="false" 
                                    SkipShippingOnCheckout="true" 
                                    ShowGiftCardTotal="true"
                                    CalculateShippingDuringCheckoutText='<%$ Tokens:StringResource, shoppingcart.aspx.13 %>'
                                    CalculateTaxDuringCheckoutText='<%$ Tokens:StringResource, shoppingcart.aspx.15 %>'
                                    SubTotalCaption='<%$ Tokens:StringResource, shoppingcart.cs.96 %>' 
                                    SubTotalWithDiscountCaption='<%$ Tokens:StringResource, shoppingcart.cs.97 %>'
                                    ShippingCaption='<%$ Tokens:StringResource, shoppingcart.aspx.12 %>' 
                                    TaxCaption='<%$ Tokens:StringResource, shoppingcart.aspx.14 %>'
                                    ShippingVatExCaption='<%$ Tokens:StringResource, setvatsetting.aspx.7 %>' 
                                    TotalCaption='<%$ Tokens:StringResource, shoppingcart.cs.61 %>'
                                    MiniCartCheckoutText='<%$ Tokens:StringResource, minicart.checkout.1 %>' 
                                    MiniCartRemoveText='<%$ Tokens:StringResource, minicart.remove %>'
                                    MiniCartUndoText='<%$ Tokens:StringResource, minicart.undo %>' 
                                    MiniCartUpdateText='<%$ Tokens:StringResource, minicart.update %>' 
                                    GiftCardTotalCaption='<%$Tokens:StringResource, order.cs.83 %>' />
                                    
                            </aspdnsfc:ShoppingCartControl>    
                            
                            <%-- The link below's only purpose is to provide refresh capability --%>
                            <asp:LinkButton ID="lnkRefresh" runat="server" OnClick="lnkRefresh_Click" >x</asp:LinkButton>                        
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Panel>