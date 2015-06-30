<%@ Page Language="c#" Inherits="AspDotNetStorefront.giftregistry" CodeFile="giftregistry.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap registry-page">
            <h1>
                <asp:Literal ID="ltGiftRegistryHeader" Text="<%$ Tokens:StringResource,Header.GiftRegistry %>" runat="server" />
            </h1>

            <asp:Literal ID="giftregistry_aspx_10" Mode="PassThrough" runat="server"></asp:Literal>
            <script type="text/javascript">
                function GiftRegistryForm_Validator(theForm) {
                    return (true);
                }
            </script>

            <asp:Panel ID="pnlGiftRegistrySearches" runat="server" Visible="true">
                <asp:Repeater ID="rptrGiftRegistrySearches" runat="server">
                    <ItemTemplate>
                        <div class="page-row">
                            <asp:HyperLink ID="lnkGiftRegistry" runat="server" NavigateUrl='<%#"giftregistry.aspx?guid=" + DataBinder.Eval(Container.DataItem, "GiftRegistryGUID").ToString() %>' Text='<%#DataBinder.Eval(Container.DataItem, "CustomerName") %>'></asp:HyperLink>
                            <asp:Button ID="btnDelGiftRegSrch" CssClass="button update-button" Text='<%#DataBinder.Eval(Container.DataItem, "Label") %>' CommandArgument='<%#DataBinder.Eval(Container.DataItem, "CustomerGiftRegistrySearchesID") %>' CommandName="delete" runat="server" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>

            <asp:Panel ID="pnlNoCustomer" runat="server" Visible="false">
                <div class="page-row">
                    <asp:Label ID="giftregistry_aspx_2" runat="server"></asp:Label>
                    <asp:Button ID="btnContinueShopping1" runat="server" CssClass="button update-button" Text="<%$ Tokens:StringResource,giftregistry.aspx.3 %>" />
                </div>
            </asp:Panel>

            <div class="page-row">
                <asp:Literal ID="giftregistry_aspx_18" runat="server" Mode="PassThrough" Visible="false"></asp:Literal>
            </div>

            <asp:Panel ID="pnlGiftRegistrySettings" runat="server" Visible="false">
                <asp:Literal ID="giftregistry_aspx_17" runat="server" Mode="PassThrough" Visible="false"></asp:Literal>
                <div class="form registry-form">
                    <div class="form-group">
                        <label>
                            <asp:Label ID="giftregistry_aspx_5" runat="server" Font-Bold="true"></asp:Label></label>
                        <div>
                            <asp:RadioButton ID="GiftRegistryIsAnonymousYes" GroupName="GiftRegistryIsAnonymous" runat="server" /><asp:Literal ID="litYes1" Mode="PassThrough" runat="server"></asp:Literal>
                        </div>
                        <div>
                            <asp:RadioButton ID="GiftRegistryIsAnonymousNo" GroupName="GiftRegistryIsAnonymous" runat="server" /><asp:Literal ID="LitNo1" Mode="PassThrough" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Label ID="giftregistry_aspx_6" runat="server"></asp:Label>
                        </label>
                        <asp:TextBox ID="txtGiftRegistryNickName" CssClass="form-control" MaxLength="50" runat="server"></asp:TextBox>
                        <div class="form-text">
                            <asp:Literal ID="giftregistry_aspx_9" runat="server" Mode="PassThrough"></asp:Literal>
                        </div>
                        <div class="form-text">
                            <asp:Literal ID="giftregistry_aspx_19" runat="server" Mode="PassThrough" Visible="false"></asp:Literal>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Label ID="giftregistry_aspx_7" runat="server"></asp:Label></label>
                        <div>
                            <asp:RadioButton ID="GiftRegistryHideShippingAddressesYes" GroupName="GiftRegistryHideShippingAddresses" runat="server" /><asp:Literal ID="litYes2" Mode="PassThrough" runat="server"></asp:Literal>
                        </div>
                        <div>
                            <asp:RadioButton ID="GiftRegistryHideShippingAddressesNo" GroupName="GiftRegistryHideShippingAddresses" runat="server" /><asp:Literal ID="LitNo2" Mode="PassThrough" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Label ID="giftregistry_aspx_12" runat="server"></asp:Label></label>
                        <div>
                            <asp:RadioButton ID="GiftRegistryAllowSearchByOthersYes" GroupName="GiftRegistryAllowSearchByOthers" runat="server" /><asp:Literal ID="litYes3" Mode="PassThrough" runat="server"></asp:Literal>
                        </div>
                        <div>
                            <asp:RadioButton ID="GiftRegistryAllowSearchByOthersNo" GroupName="GiftRegistryAllowSearchByOthers" runat="server" /><asp:Literal ID="LitNo3" Mode="PassThrough" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div class="form-submit-wrap">
                        <asp:Button ID="btnUpdateGiftSettings" runat="server" CssClass="button update-button" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlRegistryLink" runat="server" Visible="false">
                <div class="form registry-link-form">
                    <div class="group-header form-header">
                        <asp:Literal ID="ltRegistryLinkHeader" runat="server" Text="<%$ Tokens:StringResource,Header.GiftRegistryLink %>" />
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="giftregistry_aspx_11" runat="server" Mode="PassThrough"></asp:Literal></label>
                        <asp:TextBox ID="txtMyyRegistryLink" TextMode="MultiLine" runat="server" Rows="5" ReadOnly="true" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlMyGiftRegistryItems" runat="server" Visible="false">
                <div class="group-header gift-registry-header">
                    <asp:Literal ID="ltRegistryListHeader" runat="server" Text="<%$ Tokens:StringResource,Header.GiftRegistryList %>" />
                </div>
                <aspdnsfc:ShoppingCartControl ID="ctrlMyRegistry" runat="server"
                    ProductHeaderText='<%$ Tokens:StringResource, shoppingcart.product %>'
                    QuantityHeaderText='<%$ Tokens:StringResource, shoppingcart.quantity %>'
                    SubTotalHeaderText='<%$ Tokens:StringResource, shoppingcart.subtotal %>'
                    OnItemDeleting="ctrlMyRegistry_ItemDeleting" DisplayMode="GiftRegistry"
                    OnMoveToShoppingCartInvoked="ctrlMyRegistry_MoveToShoppingCartInvoked"
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
                        MoveFromGiftRegistryText='<%$ Tokens:StringResourceFormat, shoppingcart.cs.5, AppConfig.CartPrompt %>' />
                </aspdnsfc:ShoppingCartControl>

                <asp:Button ID="btnUpdateGiftRegistry" CssClass="button update-button" OnClick="btnUpdateGiftRegistry_click" Text="<%$ Tokens:StringResource,giftregistry.aspx.update %>" runat="server" />

            </asp:Panel>
            <asp:Panel ID="pnlTheirGiftRegistry" runat="server" Visible="false">

                <asp:Literal ID="ltGiftRegistryQuantityWarning" runat="server" Text='<%$ Tokens:StringResource, giftregistry.aspx.24 %>' />

                <asp:Table ID="tblTheirGiftRegistryItems" CellSpacing="0" CellPadding="2" Width="100%" runat="server">
                    <asp:TableRow>
                        <asp:TableCell HorizontalAlign="left" VerticalAlign="top">
                            <aspdnsfc:ShoppingCartControl ID="ctrlTheirRegistry" runat="server"
                                ProductHeaderText='<%$ Tokens:StringResource, shoppingcart.product %>'
                                QuantityHeaderText='<%$ Tokens:StringResource, shoppingcart.quantity %>'
                                SubTotalHeaderText='<%$ Tokens:StringResource, shoppingcart.subtotal %>'
                                DisplayMode="GiftRegistry" AllowEdit="true"
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
                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
                <asp:Literal ID="giftregistry_aspx_13" runat="server" Mode="PassThrough" Visible="false"></asp:Literal>
            </asp:Panel>
        </div>
    </asp:Panel>

</asp:Content>

