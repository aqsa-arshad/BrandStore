<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MiniCartControl.ascx.cs"
    Inherits="MiniCartControl" %>
<%@ Import Namespace="Vortx.OnePageCheckout.Models" %>
<asp:UpdatePanel ID="UpdatePanelMiniCartControl" runat="server" UpdateMode="Conditional"
    ChildrenAsTriggers="false">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="RepeaterCartItems" />
    </Triggers>
    <ContentTemplate>
        <asp:Repeater ID="RepeaterCartItems" runat="server" Visible="true" OnItemCommand="RepeaterCartItems_OnItemCommand"
            OnItemDataBound="RepeaterCartItems_OnItemDataBound">
            <ItemTemplate>
                <div class="mini-cart-item-row">
                    <div class="cart-item-image-wrap">
                        <asp:Image runat="server" ID="ProductImage" />
                    </div>
                    <div class="cart-item-name">
                        <asp:HyperLink runat="server" ID="ItemLink" NavigateUrl='<%# ((IShoppingCartItem) Container.DataItem).ProductUrl %>'>
                            <asp:Label ID="ItemName" runat="server" Text='<%# ((IShoppingCartItem) Container.DataItem).Name %>' />
                        </asp:HyperLink>
                    </div>
                    <div class="cart-item-number">
                        <asp:Literal ID="LitItemNumber" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.97") %>' />
                        <asp:Label ID="LabelSKU" runat="server" Text='<%# ((IShoppingCartItem) Container.DataItem).Sku %>' />
                    </div>
                    <div class="clear"></div>
                    <div class="cart-item-details">
                        <div class="page-row">
                            <asp:Panel ID="PanelSize" runat="server" Visible="false">
                                <asp:Label ID="ProductSizeLabel" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.95") %>'></asp:Label><asp:Label ID="ProductSize" runat="server" Text='<%# ((Vortx.OnePageCheckout.Models.IShoppingCartItem)Container.DataItem).Size%>'></asp:Label>
                            </asp:Panel>
                            <asp:Panel ID="PanelColor" runat="server" Visible="false">
                                <asp:Label ID="ProductColorLabel" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.96") %>'></asp:Label><asp:Label ID="ProductColor" runat="server" Text='<%# ((Vortx.OnePageCheckout.Models.IShoppingCartItem)Container.DataItem).Color%>'></asp:Label>
                            </asp:Panel>
                            <div class="cartItemPrice">
                                <asp:Label ID="CartItemPriceLabel" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.94") %>' runat="server" />
                                <asp:Label ID="CartItemPriceValue" Text='<%# ((IShoppingCartItem) Container.DataItem).Price.ToString("C") %>'
                                    runat="server" />
                                <asp:Label ID="CartItemPriceVat" Text='<%# StringResourceProvider.GetString("setvatsetting.aspx.6") %>' runat="server" />
                            </div>
                        </div>
                        <div class="page-row">
                            <div class="one-half text-left">
                                <asp:Label ID="CartItemQuantityLabel" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.98") %>' runat="server" />
                                <asp:DropDownList ID="CartItemRestrictedQuantity" runat="server" Visible="false"></asp:DropDownList>
                                <asp:TextBox ID="CartItemQuantityValue" CssClass="form-control quantity-box" runat="server"
                                    Text='<%# ((IShoppingCartItem) Container.DataItem).Quantity %>' />
                                <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="CartItemQuantityValue"
                                    ErrorMessage='<%# StringResourceProvider.GetString("common.cs.80") %>' ValidationExpression="^\d+$" ValidationGroup="val" Display="Dynamic"></asp:RegularExpressionValidator>
                            </div>
                            <div class="one-half text-right">
                                <asp:Button runat="server" ID="ButtonCartItemUpdate" CssClass="button opc-button opc-update-button" ValidationGroup="val"
                                    Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.112") %>' CommandName="Update" CommandArgument='<%# ((IShoppingCartItem) Container.DataItem).ItemCartId %>' />
                                <asp:Button runat="server" ID="ButtonCartItemRemove" CssClass="button opc-button opc-remove-button"
                                    Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.113") %>' CommandName="Remove" CommandArgument='<%# ((IShoppingCartItem) Container.DataItem).ItemCartId %>' />
                            </div>
                        </div>
                    </div>
                    <div class="clear">
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:Repeater ID="RepeaterOrderOptions" runat="server" OnItemCommand="RepeaterOrderOptions_OnItemCommand">
            <HeaderTemplate>
                <div class="page-row">
                    <div class="one-half">
                        <span class="OrderOptionsTitle">
                            <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.99") %>' />
                        </span>
                    </div>
                    <div class="one-fourth">
                        <span class="OrderOptionsRowHeader">
                            <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.100") %>' /></span>
                    </div>
                    <div class="one-fourth">
                        <span class="OrderOptionsRowHeader">
                            <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.101") %>' /></span>
                    </div>
                </div>
            </HeaderTemplate>
            <ItemTemplate>
                <div class="page-row">
                    <div class="one-half">
                        <img src="images/OrderOption/icon/<%# ((IOrderOptionItem)Container.DataItem).OrderOptionID %>.jpg"
                            class="order-options-image" border="0">
                        <span class="OrderOptionsName"><%# ((IOrderOptionItem)Container.DataItem).Name %></span>
                    </div>
                    <div class="one-fourth">
                        <span class="OrderOptionsPrice">
                            <%# ((IOrderOptionItem)Container.DataItem).Cost.ToString("C") %></span>
                    </div>
                    <div class="one-fourth">
                        <asp:Image ID="OrderOptionSelectedImage" runat="server" ImageUrl="images/Selected.gif" />
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>
