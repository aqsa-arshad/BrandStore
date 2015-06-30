<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CartSummary.ascx.cs" Inherits="OPCControls_CartSummary" %>
<asp:UpdatePanel ID="UpdatePanelCartSummary" runat="server" UpdateMode="Conditional"
    RenderMode="Block" ChildrenAsTriggers="false">
    <ContentTemplate>
        <div class="opc-subtotals-wrap sub-total">
            <!-- Subtotal -->
            <div class="page-row">
                <div class="one-half label text-left">
                    <asp:Label runat="server" ID="LabelSubTotal" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.60") %>' />
                </div>
                <div class="one-half value text-right">
                    <asp:Label runat="server" ID="SubTotal" />
                </div>
            </div>

            <!-- Line Item Discounts -->
            <div id="LineItemDiscountRow" runat="server" class="page-row item-discount">
                <div class="one-half label text-left">
                    <asp:Label ID="LineItemDiscountLabel" runat="server" />
                </div>
                <div class="one-half value text-right">
                    <asp:Label ID="LineItemDiscount" runat="server" />
                </div>
            </div>

            <!-- Quantity Discounts -->
            <div id="QuantityDiscountRow" runat="server" class="page-row quantity-discount">
                <div class="one-half label text-left">
                    <asp:Label runat="server" ID="LabelQuantityDiscount" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.62") %>' />
                </div>
                <div class="one-half value text-right">
                    <asp:Label runat="server" ID="LabelQuantityDiscountAmount" />
                </div>
            </div>

            <!-- Shipping Cost -->
            <div class="page-row shipping-total">
                <div class="one-half label text-left">
                    <asp:Label runat="server" ID="LabelShipMethodAmounts" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.63") %>' />
                </div>
                <div class="one-half value text-right">
                    <asp:Label runat="server" ID="ShipMethodAmount" />
                </div>
            </div>

            <!-- Tax -->
            <div id="trTaxAmounts" runat="server" class="page-row tax-total">
                <div class="one-half label text-left">
                    <asp:Label runat="server" ID="LabelTaxAmounts" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.64") %>' />
                </div>
                <div class="one-half value text-right">
                    <asp:Label runat="server" ID="TaxAmount" />
                </div>
            </div>

            <!-- Order Item Discount -->
            <div id="OrderItemDiscountRow" runat="server" class="page-row item-discount">
                <div class="one-half label text-left">
                    <asp:Label ID="OrderItemDiscountLabel" runat="server" />
                </div>
                <div class="one-half value text-right">
                    <asp:Label ID="OrderItemDiscount" runat="server" />
                </div>
            </div>

            <!-- Gift Card Discount -->
            <div id="GiftCardRow" runat="server" class="page-row gift-card">
                <div class="one-half label text-left">
                    <asp:Label ID="GiftCardLabel" runat="server" Text='<%# StringResourceProvider.GetString("order.cs.83") %>' />
                </div>
                <div class="one-half value text-right">
                    <asp:Label ID="GiftCardAmount" runat="server" />
                </div>
            </div>

            <!-- Order Total Discount -->
            <div class="page-row order-total">
                <div class="one-half label text-left">
                    <asp:Label runat="server" ID="LabelTotal" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.65") %>' />
                </div>
                <div class="one-half value text-right">
                    <asp:Label runat="server" ID="Total" />
                </div>
            </div>

        </div>
    </ContentTemplate>
</asp:UpdatePanel>
