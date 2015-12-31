<%@ Page Language="c#" Inherits="AspDotNetStorefront.checkoutreview" CodeFile="checkoutreview.aspx.cs" MasterPageFile="~/App_Templates/skin_3/JeldWenTemplate.master" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BuySafeKicker" Src="~/controls/BuySafeKicker.ascx" %>
<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel ID="pnlContent" runat="server">
        <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
        <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />        
        <div class="content-box-03">
            <h2>
                <asp:Literal ID="Literal2" Text="<%$ Tokens:StringResource,checkoutreview.aspx.1 %>" runat="server" />
            </h2>
            <aspdnsf:Topic runat="server" ID="CheckoutReviewPageHeader" TopicName="CheckoutReviewPageHeader" />
            <asp:Literal ID="XmlPackage_CheckoutReviewPageHeader" runat="server" Mode="PassThrough"></asp:Literal>

            <asp:Literal ID="checkoutreviewaspx6" Mode="PassThrough" runat="server" Text="<%$ Tokens:StringResource,checkoutreview.aspx.6 %>"></asp:Literal>
           <%-- <div class="">
                <asp:Button ID="btnContinueCheckout1" Text="<%$ Tokens:StringResource,checkoutreview.aspx.7 %>" CssClass="btn btn-primary btn-block" runat="server" OnClick="btnContinueCheckout1_Click" />
            </div>--%>
            <asp:Panel ID="pnlAmazonAddressWidget" runat="server" Visible="false">
                <div class="page-row">
                    <%= new GatewayCheckoutByAmazon.CheckoutByAmazon().RenderAddressWidget("CBAAddressWidgetContainer", true, String.Empty, new Guid(ThisCustomer.CustomerGUID)) %>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlAmazonWalletWidget" runat="server" Visible="false">
                <div class="page-row">
                    <%= new GatewayCheckoutByAmazon.CheckoutByAmazon().RenderWalletWidget("CBAWalletWidgetContainer", true)%>
                </div>
            </asp:Panel>

            <%--  <div class="page-row order-summary">
                <div class="one-third">
                    <asp:Label ID="checkoutreviewaspx8" CssClass="bold review-header" Text="<%$ Tokens:StringResource,checkoutreview.aspx.8 %>" runat="server"></asp:Label>
                    <span class="review-edit-link">[<asp:HyperLink ID="HyperLink1" runat="server" Text="edit" />]</span>
                    <asp:Literal ID="litBillingAddress" runat="server" Mode="PassThrough"></asp:Literal>
                </div>
                <div class="one-third">
                    <asp:Label ID="checkoutreviewaspx9" CssClass="bold review-header" Text="<%$ Tokens:StringResource,checkoutreview.aspx.9 %>" runat="server"></asp:Label>
                    <span class="review-edit-link">[<asp:HyperLink ID="HyperLink2" runat="server" Text="edit" NavigateUrl="~/checkoutpayment.aspx" />]</span>
                    <asp:Literal ID="litPaymentMethod" runat="server" Mode="PassThrough"></asp:Literal>
                </div>
                <div class="one-third">
                    <asp:Label ID="ordercs57" CssClass="bold review-header" Text="<%$ Tokens:StringResource,order.cs.57 %>" runat="server"></asp:Label>
                    <span class="review-edit-link">[<asp:HyperLink ID="HyperLink3" runat="server" Text="edit" />]</span>
                    <asp:Literal ID="litShippingAddress" runat="server" Mode="PassThrough"></asp:Literal>
                </div>
            </div>--%>

            <div class="page-row">
                <aspdnsf:BuySafeKicker ID="buySAFEKicker" WrapperClass="reviewKicker" runat="server" />
            </div>

            <div class="group-header checkout-header hide-element">
                <asp:Literal ID="Literal1" Text="<%$ Tokens:StringResource,Header.ShoppingCart %>" runat="server" />
            </div>
            <div class="page-row">
                <table class="table">
                    <tbody>
                        <%--Shopping cart control--%>
                        <aspdnsfc:ShoppingCartControl ID="ctrlShoppingCart"
                            ProductHeaderText=''
                            QuantityHeaderText=''
                            SubTotalHeaderText=''
                            runat="server" AllowEdit="false">
                            <LineItemSettings
                                LinkToProductPageInCart='<%$ Tokens:AppConfigBool, LinkToProductPageInCart %>'
                                SKUCaption='<%$ Tokens:StringResource, showproduct.aspx.21 %>'
                                GiftRegistryCaption='<%$ Tokens:StringResource, shoppingcart.cs.92 %>'
                                ItemNotesCaption='<%$ Tokens:StringResource, shoppingcart.cs.86 %>'
                                ItemNotesColumns='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaCols %>'
                                ItemNotesRows='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaRows %>'
                                AllowShoppingCartItemNotes="false"
                                ShowPicsInCart="true" />
                        </aspdnsfc:ShoppingCartControl>

                    </tbody>
                </table>
                <asp:Literal ID="CartSummary" Mode="PassThrough" runat="server"></asp:Literal>

                <aspdnsf:OrderOption ID="ctrlOrderOption" runat="server" EditMode="false" />
            </div>
            <div>
                <div class="td-45-percent pull-left">                    
                        <div class="one-third">
                            <asp:Label ID="checkoutreviewaspx8" CssClass="frut-roman-font black-color" Text="<%$ Tokens:StringResource,checkoutreview.aspx.8 %>" runat="server"></asp:Label>
                            <span class="review-edit-link">[<asp:HyperLink ID="HyperLink1" runat="server" Text="edit" />]</span>
                            <p><asp:Literal ID="litBillingAddress" runat="server" Mode="PassThrough"></asp:Literal></p>
                        </div>
                </div>
                        <div class="one-third hide-element">
                            <asp:Label ID="checkoutreviewaspx9" CssClass="bold review-header" Text="<%$ Tokens:StringResource,checkoutreview.aspx.999 %>" runat="server"></asp:Label>
                            <span class="review-edit-link">[<asp:HyperLink ID="HyperLink2" runat="server" Text="edit" NavigateUrl="~/checkoutpayment.aspx" />]</span>
                            <asp:Literal ID="litPaymentMethod" runat="server" Mode="PassThrough"></asp:Literal>
                        </div>
                <div class="td-30-percent pull-left"> 
                       
                            <asp:Label ID="ordercs57" CssClass="frut-roman-font black-color" Text="<%$ Tokens:StringResource,order.cs.57 %>" runat="server"></asp:Label>
                            <span class="review-edit-link" runat="server" id="spn3">[<asp:HyperLink ID="HyperLink3" runat="server" Text="edit" />]</span>
                            <p><asp:Literal ID="litShippingAddress" runat="server" Mode="PassThrough"></asp:Literal></p>
                      
                    </div>
                <%-- <div class="col-md-4">
                </div>--%>
                <div class="td-25-percent pull-left">
                    <asp:Panel ID="pnlSubTotals" runat="server">
                       <%-- <div class="page-row row-sub-totals">--%>
                            <%--Total Summary--%>
                            <aspdnsfc:CartSummary ID="ctrlCartSummary" runat="server"
                                SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>'
                                SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
                                ShippingCaption='<%$Tokens:StringResource, shoppingcart.aspx.12 %>'
                                ShippingVatExCaption='<%$Tokens:StringResource, setvatsetting.aspx.7 %>'
                                ShippingVatInCaption='<%$Tokens:StringResource, setvatsetting.aspx.6 %>'
                                TaxCaption='<%$Tokens:StringResource, shoppingcart.aspx.14 %>'
                                TotalCaption='<%$Tokens:StringResource, shoppingcart.cs.61 %>'
                                GiftCardTotalCaption='<%$Tokens:StringResource, order.cs.83 %>'
                                ShowGiftCardTotal="true"
                                IncludeTaxInSubtotal="false"
                                LineItemDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.200 %>" OrderDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.201 %>" />
                        <%--</div>--%>
                         </asp:Panel>
                    <asp:button type="submit" id="btnback" class="btn btn-primary btn-block" runat="server" onclick="btnBack_Click" Text="Back"></asp:button>
                                        <div class="clearfix"></div>
        <asp:Button ID="btnContinueCheckout2" Text="<%$ Tokens:StringResource,checkoutreview.aspx.777 %>" CssClass="btn btn-primary btn-block" runat="server" OnClick="btnContinueCheckout2_Click" />
                </div>
                <div class="clearfix"></div>
                </div>
    </asp:Panel>
    
 

    <aspdnsf:Topic runat="server" ID="CheckoutReviewPageFooter" TopicName="CheckoutReviewPageFooter" />
    <asp:Literal ID="XmlPackage_CheckoutReviewPageFooter" runat="server" Mode="PassThrough"></asp:Literal>

    </div>
        <asp:Literal ID="ltPayPalIntegratedCheckout" runat="server" />
    </asp:Panel>
    
</asp:Content>
