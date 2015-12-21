﻿<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="OrderDetail.aspx.cs" Inherits="AspDotNetStorefront.OrderDetail" %>

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
                        <font><asp:Label ID="lblOrderDateCaption" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>" /></font>
                        <asp:Label runat="server" ID="lblOrderDate"></asp:Label>
                    </span>
                </p>
            </div>
        </div>
        <%--Bill and Ship to Address Section--%>
        <div class="top-row-adjsut border-line">
            <div>
                <%--Bill to Address--%>
                <div class="td-55-percent">
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
                <%--Ship to Address--%>
                <div class="td-45-percent">
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
                <div class="td-55-percent">
                    <span class="normal-heading black-color">Payment Method</span>
                    <p>
                        <asp:Label class="block-text" ID="lblPMCardInfo" runat="server"></asp:Label>

                        <asp:Label class="block-text" ID="lblPMExpireDate" runat="server"></asp:Label>

                        <asp:Label class="block-text" ID="lblPMCountry" runat="server"></asp:Label>
                    </p>
                </div>
            </div>
        </div>

        <div>
            <div class="clearfix"></div>
            <div class="clearfix"></div>
            <div class="clearfix"></div>
            <table class="table order-detail border-line">
                <tbody>
                    <asp:Repeater ID="rptOrderItemsDetail" runat="server" OnItemDataBound="rptAddresses_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <asp:HiddenField ID="hfDescription" runat="server" Value='<%# Eval("Description") %>' />
                                <asp:HiddenField ID="hfSKU" runat="server" Value='<%# Eval("SKU") %>' />
                                <asp:HiddenField ID="hfCategoryFundUsed" runat="server" Value='<%# Eval("CategoryFundUsed") %>' />
                                <asp:HiddenField ID="hfBluBucksUsed" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                <asp:HiddenField ID="hfChosenColor" runat="server" Value='<%# Eval("ChosenColor") %>' />
                                <asp:HiddenField ID="hfProductID" runat="server" Value='<%# Eval("ProductID") %>' />
                                <asp:HiddenField ID="hfIsDownload" runat="server" Value='<%# Eval("IsDownload") %>' />
                                <asp:HiddenField ID="hfShippingTrackingNumber" runat="server" Value='<%# Eval("ShippingTrackingNumber") %>' />
                                <asp:HiddenField ID="hfShippingMethod" runat="server" Value='<%# Eval("ShippingMethod") %>' />
                                <asp:HiddenField ID="hfDownloadLocation" runat="server" Value='<%# Eval("DownloadLocation") %>' />
                                <asp:HiddenField ID="hfImageFileNameOverride" runat="server" Value='<%# Eval("ImageFileNameOverride") %>' />
                                <td class="td-55-percent">
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
                                                <asp:Label ID="lblProductSKU" runat="server"></asp:Label>
                                            </span>
                                            <p>
                                                <asp:Label ID="lblDescription" runat="server"></asp:Label>
                                            </p>
                                        </div>
                                    </div>
                                </td>
                                <td class="td-45-percent">
                                    <span class="normal-heading black-color">
                                        <asp:Label ID="lblQuantityCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.quantity %>' /></span>
                                    <span>
                                        <asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>'></asp:Label>
                                    </span>
                                </td>

                                <td class="td-55-percent">
                                    <span class="normal-heading black-color">
                                        <asp:Label ID="lblPaymentCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.payment %>' />
                                    </span>
                                    <span>Unit Price:     $<%#Math.Round(Convert.ToDecimal(Eval("OrderedProductRegularPrice")), 2).ToString() %></span>
                                    <span>Total Price:     $<%#Math.Round(Convert.ToDecimal(Eval("OrderedProductPrice")), 2).ToString() %></span>
                                </td>
                                <td class="td-30-percent">
                                    <span class="normal-heading black-color">
                                        <asp:Label ID="lblDeliveryCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.delivery %>' />
                                    </span>
                                    <span>
                                        <asp:HyperLink ID="hlDelivery" runat="server"></asp:HyperLink>
                                        <asp:Label ID="lblDelivery" runat="server"></asp:Label>
                                    </span>
                                </td>
                                <td class="td-15-percent">
                                    <span class="normal-heading black-color">&nbsp;</span>
                                    <asp:HyperLink ID="hlTrackItem" class="underline-link" runat="server">Track item &gt;</asp:HyperLink>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></FooterTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>

        <%--Billing Amounts--%>
        <div>
            <div class="td-55-percent">
            </div>
            <div class="td-45-percent">
                <p>
                    <span class="black-blu-label">
                        <font><asp:Label ID="lblSubTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.96 %>'/> $</font>
                        <asp:Label runat="server" ID="lblSubTotal"></asp:Label>
                    </span>
                </p>
                <%--TODO: Commented due to unavailablity Blu Bucks--%>
                <%--<span class="normal-heading black-color">Credits Used</span>
                <p>
                    <span class="block-text">Credit Name: $X,XXX.XX</span>
                    <span class="block-text">Credit Name: $XXX.XX</span>
                    <span class="block-text">BLU Bucks: $XXX.XX</span>
                </p>--%>

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

                <div class="form-group">
                    <a class="btn btn-primary btn-half" type="submit" role="button" target="_blank" href='<%=m_StoreLoc + "OrderReceipt.aspx?ordernumber=" + OrderNumber %>'>Print Receipt</a>
                    <asp:HyperLink ID="hplReOrder" runat="server" class="btn btn-primary btn-half" role="button" Text="Reorder"></asp:HyperLink>
                </div>
            </div>
            <div class="clearfix"></div>
        </div>
    </div>
</asp:Content>
