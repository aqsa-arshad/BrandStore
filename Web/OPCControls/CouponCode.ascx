<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CouponCode.ascx.cs" Inherits="OPCControls_CouponCode" %>
<asp:Panel ID="pnlGiftCard" runat="server" DefaultButton="btnAddGiftCard" CssClass="opcCouponCodeWrapper">
    <div class="opc-container-header">
        <asp:Label ID="LbLPromotionHeader" CssClass="opc-container-inner" runat="server" Text="<%$ Tokens:StringResource,smartcheckout.aspx.161 %>"></asp:Label>
    </div>
    <div class="opc-container-body">
        <div class="opc-container-inner">
            <div class="form opc-promotions">
                <div class="form-group">
                    <label>
                        <asp:Literal ID="shoppingcartcs31" runat="server" Text="<%$ Tokens:StringResource,smartcheckout.aspx.162 %>"></asp:Literal></label>
                    <asp:TextBox ID="CouponCode" CssClass="form-control" MaxLength="50" runat="server" ValidationGroup="AddGiftCard"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RFVCouponCode" runat="server" ControlToValidate="CouponCode" ValidationGroup="AddGiftCard" ErrorMessage="*" Display="Dynamic" />
                    <asp:Button ID="btnAddGiftCard" CssClass="button opc-button btn-add-gift-card" runat="server" Text="Add" OnClick="btnAddGiftCard_Click" ValidationGroup="AddGiftCard" />
                </div>
                <div class="form-text">
                    <asp:LinkButton ID="btnRemoveGiftCard" CssClass="button opc-button btn-remove-gift-card" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.aspx.removecoupon %>"
                        Visible="false" OnClick="btnRemoveGiftCard_Click" />
                    <asp:Label ID="lblCouponError" runat="Server" CssClass="error" />
                </div>
            </div>
        </div>
    </div>
</asp:Panel>
