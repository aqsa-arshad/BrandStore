<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenEmptyTemplate.master" CodeFile="OrderReceipt.aspx.cs" Inherits="AspDotNetStorefront.OrderReceipt" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="content-box-03">
        <div class="row">
            <div class="col-md-6">
                <p class="label-text">
                    <span>
                        <font><asp:Label ID="lblOrderNumberCaption" runat="server" Text='<%$ Tokens:StringResource, orders.aspx.OrderNumber %>'/></font>
                        <asp:Label runat="server" ID="lblOrderNumber"></asp:Label>
                    </span>
                    <span>
                        <font><asp:Label ID="lblCustomerIDCaption" runat="server" Text="<%$ Tokens:StringResource,admin.common.CustomerID%>" /></font>
                        <asp:Label runat="server" ID="lblCustomerID"></asp:Label>
                    </span>
                    <span>
                        <font><asp:Label ID="lblOrderDateCaption" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>" /></font>
                        <asp:Label runat="server" ID="lblOrderDate"></asp:Label>
                    </span>

                    <asp:Label runat="server" ID="lblInfo">The title of this order will be shown as “CMD” in your credit card transaction history.</asp:Label>

                </p>
            </div>
        </div>

        <table class="table top-row-adjsut border-line">
            <tbody>
                <tr>
                    <td class="td-40-percent">
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

                    </td>
                    <td class="td-30-percent">
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
                    </td>
                    <td class="td-30-percent">
                        <span class="normal-heading black-color">Payment Method</span>
                        <p>
                            <asp:Label class="block-text" ID="lblPMCardInfo" runat="server"></asp:Label>

                            <asp:Label class="block-text" ID="lblPMExpireDate" runat="server"></asp:Label>

                            <asp:Label class="block-text" ID="lblPMCountry" runat="server"></asp:Label>
                        </p>
                    </td>
                </tr>
            </tbody>
        </table>

        <%--Items Detail--%>
        <div>
            <table class="table margin-none">
                <tbody>
                    <asp:Repeater ID="rptOrderItemsDetail" runat="server" OnItemDataBound="rptOrderItemsDetail_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <asp:HiddenField ID="hfRegularPrice" runat="server" Value='<%# Eval("RegularPrice") %>' />
                                <asp:HiddenField ID="hfBluBucks" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                <asp:HiddenField ID="hfDescription" runat="server" Value='<%# Eval("Description") %>' />
                                <asp:HiddenField ID="hfSKU" runat="server" Value='<%# Eval("SKU") %>' />
                                <asp:HiddenField ID="hfCategoryFundUsed" runat="server" Value='<%# Eval("CategoryFundUsed") %>' />
                                <asp:HiddenField ID="hfBluBucksUsed" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                <asp:HiddenField ID="hfChosenColor" runat="server" Value='<%# Eval("ChosenColor") %>' />
                                <asp:HiddenField ID="hfProductID" runat="server" Value='<%# Eval("ProductID") %>' />
                                <asp:HiddenField ID="hfImageFileNameOverride" runat="server" Value='<%# Eval("ImageFileNameOverride") %>' />

                                <td class="td-40-percent">
                                    <div class="row">
                                        <div class="col-md-5">
                                            <div class="primary-img-box">
                                                <asp:Image ID="ImgProduct" runat="server" class="img-responsive" />
                                            </div>
                                        </div>
                                        <div class="col-md-5">

                                            <span class="normal-heading black-color">
                                                <asp:Label ID="lblProductName" runat="server" Text='<%# Eval("OrderedProductName") %>'></asp:Label>
                                            </span>
                                            <span>
                                                <asp:Label ID="lblProductSKU" runat="server" />
                                            </span>
                                            <p>
                                                <asp:Label ID="lblDescription" runat="server" />
                                            </p>
                                        </div>
                                    </div>
                                </td>
                                <td class="td-30-percent">
                                    <span class="normal-heading black-color">
                                        <asp:Label ID="lblQuantityCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.quantity %>' /></span>
                                    <span>
                                        <asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>' />
                                    </span>
                                </td>

                                <td class="td-30-percent label-text">
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
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></FooterTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

        <%--Billing Amounts--%>
        <table class="table margin-none">
            <tbody>
                <tr>
                    <td class="td-40-percent"></td>
                    <td class="td-30-percent"></td>
                    <td class="td-30-percent border-none">
                        <p class="label-text">
                            <span>
                                <font><asp:Label ID="lblSubTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.96 %>'/> $</font><asp:Label runat="server" ID="lblSubTotal"></asp:Label>
                            </span>
                        </p>
                        <%--TODO: Commented due to unavailablity Blu Bucks--%>
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
                        <p class="label-text">
                            <span>Taxes: $<asp:Label runat="server" ID="lblTax"></asp:Label></span>
                            <span>
                                <asp:Label ID="lblShippingCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.aspx.12 %>' />
                                $<asp:Label runat="server" ID="lblShippingCost"></asp:Label>
                            </span>
                        </p>

                        <p class="label-text">
                            <span><font><asp:Label ID="lblTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.61 %>'/> </font>$<asp:Label runat="server" ID="lblTotalAmount"></asp:Label></span>
                        </p>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</asp:Content>
