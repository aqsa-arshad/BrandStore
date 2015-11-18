<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" CodeFile="orderconfirmation.aspx.cs" Inherits="AspDotNetStorefront.orderconfirmation" %>
<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content runat="server" ContentPlaceHolderID="PageContent">
     <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="content-box-03">
        <div class="row">
            <div class="col-md-8">
                
                 <h4 class="black-color">ORDER CONFIRMATION</h4>                       
                
                
                        <p class="frut-roman-font" style="word-wrap:break-word">
                            Thank you! Your order was successfully completed. Your Order Number is
                            <asp:Label runat="server" ID="lblOrderNumber"></asp:Label>
                            
                           <asp:Label runat="server" ID="lblreceipt" class="block-text">For a printable receipt, <a id="lnkreceipt" target="_blank" runat="server" class="underline-link">click here</a>.</asp:Label>
                         <asp:Label runat="server" ID="Label1" class="block-text">The title of this order will be shown as “CMD” in your credit card transaction history.</asp:Label>
                             </p>
                <p class="frut-roman-font">
                 Tracking numbers will be on your order history page when your items are ready to ship.                       
                </p>
            </div>
        </div>     
            <div class="col-md-4 hide-element">
                <span class="normal-heading black-color">Payment Method</span>
                <p>
                    <asp:Label class="block-text" ID="lblPMCardInfo" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblPMExpireDate" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblPMCountry" runat="server"></asp:Label>
                </p>
            </div>    

        <%--Items Detail--%>
        <div>
            <table class="table top-row-adjsut border-line">
                <tbody>
                    <asp:Repeater ID="rptOrderItemsDetail" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td class="td-45-percent">                                    
                                    <div class="row">
                                        <div class="col-md-4">
                                            <div class="primary-img-box">
                                                <asp:Image ID="Image1" runat="server" class="img-responsive"
                                                    ImageUrl='<%# AspDotNetStorefrontCore.AppLogic.LookupImage("Product", int.Parse(Eval("ProductID").ToString()), Eval("ImageFileNameOverride").ToString(), Eval("SKU").ToString(), "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)%>' />
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <span class="normal-heading blue-color">
                                                <asp:Label ID="lblProductName" runat="server" Text='<%# Eval("OrderedProductName") %>'></asp:Label>
                                            </span>
                                            <span>
                                                <asp:Label ID="lblProductID" runat="server" Text='<%# Eval("ProductID") %>'></asp:Label>
                                            </span>
                                            <p>
                                                <asp:Label ID="lblDescription" runat="server" Text='<%# Eval("Description").ToString().Take(100).Aggregate("", (x,y) => x + y) %>'></asp:Label>
                                            </p>
                                        </div>
                                    </div>
                                </td>                                
                                <td class="td-30-percent">
                                    <span class="normal-heading black-color">Payment</span>                                  
                                    <span>Price:    $<%#Math.Round(Convert.ToDecimal(Eval("OrderedProductPrice")), 2).ToString() %></span>                                    
                                </td>
                                <td class="td-25-percent">
                                    <span class="normal-heading black-color">Quantity</span>
                                    <span>
                                        <asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>'></asp:Label>
                                    </span>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></FooterTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

        <%--Billing Amounts--%>
        <div class="top-row-adjsut">
             <div class="td-45-percent pull-left ">
                <span class="normal-heading black-color">Billed to</span>
                <p>
                    <asp:Label class="block-text" ID="lblBAFullName" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblBACompany" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblBAAddress1" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblBAAddress2" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblBASuite" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblBACityStateZip" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblBACountry" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblBAPhone" runat="server"></asp:Label>
                </p>
            </div>
            <div class="td-30-percent pull-left">
                <span class="normal-heading black-color">Shipped to</span>
                <p>
                    <asp:Label class="block-text" ID="lblSAFullName" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblSACompany" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblSAAddress1" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblSAAddress2" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblSASuite" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblSACityStateZip" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblSACountry" runat="server"></asp:Label>

                    <asp:Label class="block-text" ID="lblSAPhone" runat="server"></asp:Label>
                </p>
            </div>
            <div  class="td-25-percent pull-left">
                <p>
                    <span class="black-blu-label">
                        <font>Subtotal: $</font><asp:Label runat="server" ID="lblSubTotal"></asp:Label>
                    </span>
                </p>              

                <span class="normal-heading black-color">Charges</span>
                <p>
                    <span class="block-text">Taxes: $<asp:Label runat="server" ID="lblTax"></asp:Label></span>
                    <span class="block-text">Shipping:  $<asp:Label runat="server" ID="lblShippingCost"></asp:Label></span>
                </p>

                <p>
                    <span class="black-blu-label"><font>Total:</font> $<asp:Label runat="server" ID="lblTotalAmount"></asp:Label></span>
                </p>
            </div>
            <div class="clearfix"></div>
        </div>
    </div>    
</asp:Content>
