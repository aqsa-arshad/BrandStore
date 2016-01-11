<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" CodeFile="orderconfirmation.aspx.cs" Inherits="AspDotNetStorefront.orderconfirmation" %>

<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="content-box-03">
        <div class="row">
            <div class="col-md-8">

                <h4 class="black-color">ORDER CONFIRMATION</h4>

                <p class="frut-roman-font" style="word-wrap: break-word">
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
                    <asp:Repeater ID="rptOrderItemsDetail" runat="server" OnItemDataBound="rptAddresses_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <asp:HiddenField ID="hfBluBucks" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                <asp:HiddenField ID="hfIsDownload" runat="server" Value='<%# Eval("IsDownload") %>' />
                                <asp:HiddenField ID="hfSKU" runat="server" Value='<%# Eval("SKU") %>' />
                                <asp:HiddenField ID="hfDescription" runat="server" Value='<%# Eval("Description") %>' />
                                <asp:HiddenField ID="hfDownloadLocation" runat="server" Value='<%# Eval("DownloadLocation") %>' />
                                <asp:HiddenField ID="hfCategoryFundUsed" runat="server" Value='<%# Eval("CategoryFundUsed") %>' />
                                <asp:HiddenField ID="hfBluBucksUsed" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                <asp:HiddenField ID="hfChosenColor" runat="server" Value='<%# Eval("ChosenColor") %>' />
                                <asp:HiddenField ID="hfProductID" runat="server" Value='<%# Eval("ProductID") %>' />
                                <asp:HiddenField ID="hfImageFileNameOverride" runat="server" Value='<%# Eval("ImageFileNameOverride") %>' />
                                <asp:HiddenField ID="hfRegularPrice" runat="server" Value='<%# Eval("RegularPrice") %>' />
                                <td class="td-40-percent">
                                    <div class="row tablet-view">
                                        <div class="col-md-4">
                                            <div class="primary-img-box pull-left-md">
                                                <asp:Image ID="ImgProduct" runat="server" class="img-responsive" />
                                            </div>
                                        </div>
                                        <div class="col-md-7 pull-left-md">
                                            <span class="normal-heading blue-color">
                                                <asp:Label ID="lblProductName" runat="server" Text='<%# Eval("OrderedProductName") %>'></asp:Label>
                                            </span>
                                            <span>
                                                <asp:Label ID="lblProductSKU" runat="server" />
                                            </span>
                                            <p>
                                                <asp:Label ID="lblDescription" runat="server"></asp:Label>
                                            </p>
                                        </div>
                                    </div>
                                </td>
                              <td class="td-35-percent label-text">
                                    <span class="normal-heading black-color">
                                        <asp:Label ID="lblPaymentCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.payment %>' />
                                    </span>
                                    <span>
                                        <asp:Label ID="lblRegularPriceCaption" runat="server" Text='<%$ Tokens:StringResource,RegularPriceCaption  %>' /><asp:Label ID="lblRegularPrice" runat="server" />
                                    </span>
                                    <span>
                                        <asp:Label ID="lblCategoryFundCreditCaption" runat="server" Text='<%$ Tokens:StringResource,CategoryFundCreditCaption  %>' /><asp:Label ID="lblCategoryFundCredit" runat="server" />
                                    </span>
                                    <span>
                                        <asp:Label ID="lblBluBucksCaption" runat="server" Text='<%$ Tokens:StringResource,BluBucksCaption  %>' /><asp:Label ID="lblBluBuck" runat="server" />
                                    </span>
                                    <span>
                                        <asp:Label ID="lblCreditPriceCaption" runat="server" Text='<%$ Tokens:StringResource, CreditPriceCaption %>' /><%#Math.Round(Convert.ToDecimal(Eval("CreditPrice")), 2).ToString() %>
                                    </span>
                                </td>
                                <td class="td-15-percent">
                                    <span class="normal-heading black-color">
                                        <asp:Label ID="lblQuantityCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.quantity %>' /></span>
                                    <span>
                                        <asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>'></asp:Label>
                                    </span>
                                </td>
                                <td class="td-10-percent">
                                    <span class="normal-heading black-color">&nbsp</span>
                                    <span>
                                        <asp:HyperLink ID="hlDelivery" runat="server"></asp:HyperLink>
                                        <asp:Label ID="lblDelivery" runat="server" Text='<%# Eval("ShippingMethod") %>'></asp:Label>
                                        <br />
                                        <asp:LinkButton runat="server" ID="hlLearnmore" OnClientClick="return showPopUp();" ClientIDMode="Static"></asp:LinkButton>
                                    </span>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></FooterTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

        <%--learn more POP UP Start here --%>
        <div id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" class="modal fade in" aria-hidden="false" style="display: none">
            <div class="modal-dialog modal-checkout" role="document">
                <div class="modal-content">
                    <div class="modal-body">
                        <button type="button" id="Closebtn" class="close" data-dismiss="modal" aria-label="Close">
                            <img src="App_Themes/Skin_3/images/close-popup.png" alt="Closs"></button>
                        <p>
                            <asp:Label runat="server" Text="<%$ Tokens:StringResource, learnMore.aspx.1 %>"></asp:Label>
                        </p>
                        <p>
                            <asp:Label runat="server" Text="<%$ Tokens:StringResource, learnMore.aspx.2 %>"></asp:Label>
                        </p>
                    </div>
                </div>
            </div>
        </div>
        <%--learn more POP UP ends here --%>

        <%--Billing Amounts--%>
        <div class="top-row-adjsut">
            <div class="td-40-percent pull-left ">
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
            <div class="td-35-percent pull-left">
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
            <div class="td-25-percent pull-left">
                <p>
                    <span class="black-blu-label">
                        <font><asp:Label ID="lblSubTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.96 %>'/> $</font><asp:Label runat="server" ID="lblSubTotal"></asp:Label>
                    </span>
                </p>
                <span class="normal-heading black-color">
                    <asp:Label ID="lblCreditsUsedCaption" runat="server" Text='<%$ Tokens:StringResource, CreditsUsedCaption %>' />
                </span>
                <p class="label-text">
                    <span class="block-text">
                        <asp:Label ID="lblSOFFundsTotalCaption" Text="SOF Fund: $" Visible="False" runat="server" /><asp:Label runat="server" ID="lblSOFFundsTotal" Visible="False"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblDirectMailFundsTotalCaption" Visible="False" Text="Direct Mail Funds: $" runat="server" /><asp:Label runat="server" ID="lblDirectMailFundsTotal" Visible="False"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblDisplayFundsTotalCaption" Visible="False" Text="Display Funds: $" runat="server" /><asp:Label runat="server" ID="lblDisplayFundsTotal" Visible="False"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblLiteratureFundsTotalCaption" Visible="False" Text="Literature Funds: $" runat="server" /><asp:Label runat="server" ID="lblLiteratureFundsTotal" Visible="False"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblPOPFundsTotalCaption" Visible="False" Text="POP Funds: $" runat="server" /><asp:Label runat="server" ID="lblPOPFundsTotal" Visible="False"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblBluBucksTotalCaption" Visible="False" Text="BLU Bucks: $" runat="server" /><asp:Label runat="server" ID="lblBluBucksTotal" Visible="False"></asp:Label>
                    </span>
                </p>
                <span class="normal-heading black-color">Charges</span>
                <p>
                    <span class="block-text">Taxes: $<asp:Label runat="server" ID="lblTax"></asp:Label></span>
                    <span class="block-text">
                        <asp:Label ID="lblShippingCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.aspx.12 %>' />
                        $<asp:Label runat="server" ID="lblShippingCost"></asp:Label></span>
                </p>
                <p>
                    <span class="black-blu-label"><font><asp:Label ID="lblTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.61 %>'/> </font>$<asp:Label runat="server" ID="lblTotalAmount"></asp:Label></span>
                </p>
            </div>
            <div class="clearfix"></div>
        </div>
    </div>
    <script type="text/javascript">
        $("#Closebtn").click(function () {
            document.getElementById('myModal').style.display = 'none';
        });
        function showPopUp(e) {
            document.getElementById('myModal').style.display = 'block';
            return false;
        }
    </script>
</asp:Content>
