<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Wishlist.ascx.cs" Inherits="AspDotNetStorefront.WishlistControl" %>
<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<div class="page-wrap wishlist-page">
    <h1>
        <asp:Literal ID="ltWishlistHeader" Text="<%$ Tokens:StringResource,Header.Wishlist %>" runat="server" />
    </h1>

    <div class="wish-list-warning">
        <asp:Label ID="lblWishlistMessage" runat="server" Text="">
        </asp:Label>
    </div>

    <asp:Literal ID="ltlTopicWishListPageHeader" runat="server" Text="<%$ Tokens:Topic, WishListPageHeader %>" />
    <asp:Literal ID="XmlPackage_WishListPageHeader" runat="server" Mode="PassThrough"></asp:Literal>

    <asp:Literal ID="XmlPackage_WishListPageTopControlLines" runat="server" Mode="PassThrough" Visible="false"></asp:Literal>

    <asp:Panel ID="pnlTopControlLines" runat="server" Visible="false">
        <div class="form-submit-wrap">
            <asp:Button ID="btnContinueShopping1" runat="server" CssClass="button continue-button" OnClick="btnContinueShopping1_Click" />
            <asp:Button ID="btnUpdateWishList1" runat="server" CssClass="button update-button" OnClick="btnUpdateWishList1_Click" />
        </div>
    </asp:Panel>


    <asp:Panel ID="Panel1" runat="server" DefaultButton="btnUpdateWishList1" CssClass="pnlWishlist">
        <aspdnsfc:ShoppingCartControl ID="ctrlShoppingCart" runat="server"
            ProductHeaderText='<%$ Tokens:StringResource, shoppingcart.product %>'
            QuantityHeaderText='<%$ Tokens:StringResource, shoppingcart.quantity %>'
            OnItemDeleting="ctrlShoppingCart_ItemDeleting" DisplayMode="WishList"
            OnMoveToShoppingCartInvoked="ctrlShoppingCart_MoveToShoppingCartInvoked"
            EmptyCartTopicName="EmptyWishListText">
            <LineItemSettings
                LinkToProductPageInCart='<%$ Tokens:AppConfigBool, LinkToProductPageInCart %>'
                ImageLabelText='<%$ Tokens:StringResource, shoppingcart.cs.1000 %>'
                SKUCaption='<%$ Tokens:StringResource, showproduct.aspx.21 %>'
                GiftRegistryCaption='<%$ Tokens:StringResource, shoppingcart.cs.92 %>'
                ItemNotesCaption='<%$ Tokens:StringResource, shoppingcart.cs.86 %>'
                ItemNotesColumns='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaCols %>'
                ItemNotesRows='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaRows %>'
                AllowShoppingCartItemNotes='<%$ Tokens:AppConfigBool, AllowShoppingCartItemNotes %>'
                ShowPicsInCart='<%$ Tokens:AppConfigBool, ShowPicsInCart %>'
                ShowEditButtonInCartForKitProducts='<%$ Tokens:AppConfigBool, ShowEditButtonInCartForKitProducts %>'
                ShowMultiShipAddressUnderItemDescription="true"
                MoveFromWishListText='<%$ Tokens:StringResourceFormat, shoppingcart.cs.5, AppConfig.CartPrompt %>' />
        </aspdnsfc:ShoppingCartControl>
    </asp:Panel>

    <asp:Literal ID="Xml_WishListPageBottomControlLines" runat="server" Mode="PassThrough" Visible="false"></asp:Literal>

    <asp:Panel ID="pnlBottomControlLines" runat="server">
        <div class="form-submit-wrap">
            <asp:Button ID="btnContinueShopping2" runat="server" CssClass="button continue-button" OnClick="btnContinueShopping2_Click" />
            <asp:Button ID="btnUpdateWishList2" runat="server" CssClass="button update-button" OnClick="btnUpdateWishList2_Click" />
        </div>

    </asp:Panel>

    <asp:Literal ID="ltlWishListPageFooter" runat="server" Text="<%$ Tokens:Topic, WishListPageFooter %>" />
    <asp:Literal ID="Xml_WishListPageFooter" runat="server" Mode="PassThrough"></asp:Literal>
</div>
