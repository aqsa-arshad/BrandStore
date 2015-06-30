<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckoutSteps.ascx.cs" Inherits="AspDotNetStorefront.CheckoutSteps" %>
<div class="checkout-steps-wrap">
    <ul class="checkout-tabs">
        <li class="no-tab secure-checkout">
            <asp:Literal ID="ltCheckoutTitle" runat="server" Text='<%$ Tokens:StringResource,Checkout.Progress.Title %>' />
        </li>
        <li ID="liAccount" runat="server">
            <asp:HyperLink ID="lnkAccount" NavigateUrl="~/account.aspx?checkout=true" runat="server" Text='<%$ Tokens:StringResource,Checkout.Progress.Account %>'></asp:HyperLink>
        </li>
        <li ID="liShipping" runat="server">
            <asp:HyperLink ID="lnkShipping" NavigateUrl="~/checkoutshipping.aspx" runat="server" Text='<%$ Tokens:StringResource,Checkout.Progress.Shipping %>'></asp:HyperLink>
        </li>
        <li ID="liPayment" runat="server">
            <asp:HyperLink ID="lnkPayment" NavigateUrl="~/checkoutpayment.aspx" runat="server" Text='<%$ Tokens:StringResource,Checkout.Progress.Payment %>'></asp:HyperLink>
        </li>
        <li ID="liReview" runat="server">
            <asp:HyperLink ID="lnkReview" NavigateUrl="~/checkoutreview.aspx" runat="server" Text='<%$ Tokens:StringResource,Checkout.Progress.Review %>'></asp:HyperLink>
        </li>
        <li ID="liComplete" runat="server">
            <asp:HyperLink ID="lnkComplete" NavigateUrl="~/orderconfirmation.aspx" runat="server" Text='<%$ Tokens:StringResource,Checkout.Progress.Complete %>'></asp:HyperLink>
        </li>
    </ul>
</div>

