<%@ Control Language="C#" AutoEventWireup="true" CodeFile="QuantityDiscountControl.ascx.cs" Inherits="controls_QuantityDiscountControl" %>
<%@ Import Namespace="System" %>


<asp:Repeater ID="rptQuantityDiscountList" runat="server">
    <HeaderTemplate>
        <table class="quantitydiscountTable">
            <tr>
                <td>
                    <b><asp:Literal ID="litPerItemHeader" runat="server" Text='<%$ Tokens:StringResource, common.cs.34 %>'></asp:Literal></b>
                </td>
                <td>
                    <b><asp:Literal ID="litQuantityHeader" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.116 %>'></asp:Literal></b>
                </td>
            </tr>
    </HeaderTemplate>
    <ItemTemplate>
        <tr>
            <td>
                <asp:Literal ID="litDiscountQuantity" runat="server" Text='<%#ShowDiscountQuantity(Eval("LowQuantity"), Eval("HighQuantity")) %>'></asp:Literal>
            </td>
            <td>
                <%#ShowDiscountAmount(Eval("QuantityDiscountID"), Eval("DiscountPercent")) %>
            </td>
        </tr>
    </ItemTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
</asp:Repeater>
