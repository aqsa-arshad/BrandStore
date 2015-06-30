<%@ Page Language="c#" Inherits="AspDotNetStorefront.giftregistrysearch" CodeFile="giftregistrysearch.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap registry-search-page">
            <h1>
                <asp:Literal ID="Literal2" Text="<%$ Tokens:StringResource,Header.GiftRegistrySearch %>" runat="server" />
            </h1>

            <div class="form-text">
                <asp:Literal ID="Literal1" Text="<%$ Tokens:StringResource,giftregistrysearch.aspx.2 %>" runat="server" />
            </div>

            <div class="form registry-search-form">
                <div class="form-group">
                    <label>
                        <asp:Literal ID="giftregistrysearch_aspx_3" runat="server" Mode="PassThrough"></asp:Literal>
                    </label>
                    <asp:TextBox ID="txtSearchForName" CssClass="form-control" MaxLength="100" runat="server" TabIndex="1"></asp:TextBox>
                    <asp:Button ID="btnSearchForName" TabIndex="2" CssClass="button gift-registry-button" runat="server" OnClick="btnSearchForName_Click" />
                </div>

                <div class="form-group">
                    <label>
                        <asp:Literal ID="giftregistrysearch_aspx_4" runat="server" Mode="PassThrough"></asp:Literal>
                    </label>
                    <asp:TextBox ID="txtSearchForNickName" CssClass="form-control" MaxLength="100" runat="server" TabIndex="3"></asp:TextBox>
                    <asp:Button ID="btnSearchForNickName" TabIndex="4" CssClass="button gift-registry-button" runat="server" OnClick="btnSearchForNickName_Click" />
                </div>

                <div class="form-group">
                    <label>
                        <asp:Literal ID="giftregistrysearch_aspx_5" runat="server" Mode="PassThrough"></asp:Literal>
                    </label>
                    <asp:TextBox ID="txtSearchForEMail" CssClass="form-control" MaxLength="100" runat="server" TabIndex="5"></asp:TextBox>
                    <asp:Button ID="btnSearchForEMail" TabIndex="6" CssClass="button gift-registry-button" runat="server" OnClick="btnSearchForEMail_Click" />
                </div>
            </div>
            <asp:Literal ID="SearchResults" runat="server" Mode="PassThrough" Visible="true"></asp:Literal>
            <asp:Panel ID="pnlSearchResults" runat="server" Visible="false">
                <asp:Literal ID="giftregistry_aspx_16" runat="server" Mode="PassThrough"></asp:Literal>
                <asp:Button ID="btnSaveButton" runat="server" CssClass="GiftRegistrySaveButton" Visible="false" OnClick="btnSaveButton_Click" />

                <asp:Literal ID="ltGiftRegistryQuantityWarning" runat="server" Text='<%$ Tokens:StringResource, giftregistry.aspx.24 %>' />

                <aspdnsfc:ShoppingCartControl ID="ctrlTheirRegistry" runat="server"
                    ProductHeaderText='<%$ Tokens:StringResource, shoppingcart.product %>'
                    QuantityHeaderText='<%$ Tokens:StringResource, shoppingcart.quantity %>'
                    SubTotalHeaderText='<%$ Tokens:StringResource, shoppingcart.subtotal %>'
                    DisplayMode="GiftRegistry" AllowEdit="false"
                    OnMoveToShoppingCartInvoked="ctrlTheirRegistry_MoveToShoppingCartInvoked"
                    EmptyCartTopicName="EmptyGiftRegistryText">
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
                        MoveFromGiftRegistryText='<%$ Tokens:StringResource, giftregistry.aspx.14 %>' />
                </aspdnsfc:ShoppingCartControl>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>
