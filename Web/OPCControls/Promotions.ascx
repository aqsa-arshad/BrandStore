<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Promotions.ascx.cs" Inherits="OPCControls_Promotions" %>
<asp:Panel ID="pnlPromotion" runat="server" DefaultButton="btnAddPromotion" CssClass="opcPromotionWrapper">
    <div class="opc-container-header">
        <asp:Label ID="LbLPromotionHeader" CssClass="opc-container-inner" runat="server" Text="<%$ Tokens:StringResource,smartcheckout.aspx.160 %>"></asp:Label>
    </div>
    <div class="opc-container-body">
        <div class="opc-container-inner">
            <div class="form opc-promotions">
                <div class="form-group">
                    <label>
                        <asp:Label ID="shoppingcartcs118" runat="server" Text="<%$ Tokens:StringResource,smartcheckout.aspx.163 %>"></asp:Label>&#160;
                    </label>
                    <asp:TextBox ID="txtPromotionCode" CssClass="form-control" MaxLength="100" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RFVPromotionCode" runat="server" ControlToValidate="txtPromotionCode" ValidationGroup="AddPromotion" ErrorMessage="*" Display="Dynamic" />
                    <asp:Button ID="btnAddPromotion" runat="server" Text="Add" OnClick="btnAddPromotion_Click" CssClass="button opc-button add-promotion" ValidationGroup="AddPromotion" />
                </div>
                <div class="form-text">
                    <asp:Label ID="lblPromotionError" runat="Server" CssClass="error" />
                </div>
            </div>

            <div class="promotionlist">
                <asp:Repeater ID="repeatPromotions" runat="server" OnItemCommand="repeatPromotions_ItemCommand">
                    <ItemTemplate>
                        <div class="promotionlistitem notice-wrap">
                            <span class="promotion-list-item-code">
                                <%# Eval("Code") %>
                            </span>
                            <span class="promotion-list-item-description">
                                <%# Eval("Description") %>
                            </span>
                            <span class="promotioncodeentrylink">
                                <asp:LinkButton ID="lnkRemovePromotion" runat="server" CommandArgument='<%# Eval("Code") %>' CommandName="RemovePromotion" Text="Remove" />
                            </span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</asp:Panel>
