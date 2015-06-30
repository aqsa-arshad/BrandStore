<%@ Page Language="c#" Inherits="AspDotNetStorefront.orderconfirmation" CodeFile="orderconfirmation.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap create-account-page">
            <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />
            <asp:Literal ID="litOutput" runat="server"></asp:Literal>
        </div>
    </asp:Panel>
</asp:Content>
