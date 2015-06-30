<%@ Page Language="c#" Inherits="AspDotNetStorefront.checkoutshippingmult2" CodeFile="checkoutshippingmult2.aspx.cs"
    MasterPageFile="~/App_Templates/skin_1/template.master" MaintainScrollPositionOnPostback="true" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <div class="page-wrap multi-ship-page">
        <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />

        <h1>
            <asp:Literal ID="ltShippingOptions" Text="<%$ Tokens:StringResource,Header.ShippingOptions %>" runat="server" />
        </h1>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>

            <asp:Panel ID="pnlErrorMsg" runat="server" Visible="false">
                <asp:Label ID="ErrorMsgLabel" CssClass="errorLg" runat="server"></asp:Label>
            </asp:Panel>

            <aspdnsf:Topic runat="server" ID="CheckoutShippingMult2PageHeader" TopicName="CheckoutShippingMult2PageHeader" />
            <asp:Literal ID="XmlPackage_CheckoutShippingMult2PageHeader" runat="server" Mode="PassThrough"></asp:Literal>

            <asp:Panel ID="pnlGetFreeShipping" runat="server" CssClass="FreeShippingThresholdPrompt" Visible="false">
                <asp:Literal ID="GetFreeShipping" runat="server" Mode="PassThrough"></asp:Literal>
            </asp:Panel>

            <asp:Panel ID="pnlIsFreeShipping" runat="server" CssClass="FreeShippingThresholdPrompt" Visible="false">
                <asp:Literal ID="IsFreeShipping" runat="server" Mode="PassThrough"></asp:Literal>
            </asp:Panel>

            <asp:Literal ID="checkoutshippingmult2aspx16" Mode="PassThrough" runat="server"></asp:Literal>

            <asp:Literal ID="CartItems" Mode="PassThrough" runat="server"></asp:Literal>

            <aspdnsf:Topic runat="server" ID="CheckoutShippingMult2PageFooter" TopicName="CheckoutShippingMult2PageFooter" />
            <asp:Literal ID="XmlPackage_CheckoutShippingMult2PageFooter" runat="server" Mode="PassThrough"></asp:Literal>

        </asp:Panel>
    </div>
</asp:Content>
