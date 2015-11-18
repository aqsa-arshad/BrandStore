﻿<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenEmptyTemplate.master" CodeFile="OrderReceipt.aspx.cs" Inherits="AspDotNetStorefront.OrderReceipt" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="content-box-03">
        <div class="row">
            <div class="col-md-6">
                <p class="label-text">
                    <span>
                        <font>Order Number</font>
                        <asp:Label runat="server" ID="lblOrderNumber"></asp:Label>
                    </span>
                    <span>
                        <font>Customer ID</font>
                        <asp:Label runat="server" ID="lblCustomerID"></asp:Label>
                    </span>
                    <span>
                        <font>Date</font>
                        <asp:Label runat="server" ID="lblOrderDate"></asp:Label>
                    </span>

                    <asp:Label runat="server" ID="Label1">The title of this order will be shown as “CMD” in your credit card transaction history.</asp:Label>

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
                    <asp:Repeater ID="rptOrderItemsDetail" runat="server" OnItemDataBound="rptAddresses_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <td class="td-40-percent">
                                    <div class="row">
                                        <div class="col-md-5">
                                            <div class="primary-img-box">
                                                <asp:Image ID="Image1" runat="server" class="img-responsive"
                                                    ImageUrl='<%# AspDotNetStorefrontCore.AppLogic.LookupImage("Product", int.Parse(Eval("ProductID").ToString()), Eval("ImageFileNameOverride").ToString(), Eval("SKU").ToString(), "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)%>' />
                                            </div>
                                        </div>
                                        <div class="col-md-5">
                                            <asp:HiddenField ID="hfDescription" runat="server" Value='<%# Eval("Description") %>' />
                                            <asp:HiddenField ID="hfSKU" runat="server" Value='<%# Eval("SKU") %>' />
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
                                <td class="td-30-percent">
                                    <span class="normal-heading black-color">Quantity</span>
                                    <span>
                                        <asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>'></asp:Label>
                                    </span>
                                </td>

                                <td class="td-30-percent">
                                    <span class="normal-heading black-color">Price</span>
                                    <span>Price:     $<%#Math.Round(Convert.ToDecimal(Eval("OrderedProductRegularPrice")), 2).ToString() %></span>
                                    <span>Ext. Price:     $<%#Math.Round(Convert.ToDecimal(Eval("OrderedProductPrice")), 2).ToString() %></span>
                                    <%--TODO: Commented due to unavailablity Blu Bucks--%>
                                    <%--<span>Price with Sales Fund credit: $Y,YYY.YY</span>
                                    <span>GL Code: (GL CODE)</span>--%>
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
                                <font>Subtotal: $</font>
                                <asp:Label runat="server" ID="lblSubTotal"></asp:Label>
                            </span>
                        </p>
                        <%--TODO: Commented due to unavailablity Blu Bucks--%>
                        <%-- <span class="normal-heading black-color">Credits Used</span>
                <p>
                    <span class="block-text">Credit Name: $X,XXX.XX</span>
                    <span class="block-text">Credit Name: $XXX.XX</span>
                    <span class="block-text">BLU Bucks: $XXX.XX</span>
                </p>--%>

                        <span class="normal-heading black-color">Charges</span>
                        <p class="label-text">
                            <span>Taxes: $<asp:Label runat="server" ID="lblTax"></asp:Label></span>
                            <span>Shipping:  $<asp:Label runat="server" ID="lblShippingCost"></asp:Label></span>
                        </p>

                        <p class="label-text">
                            <span><font>Total:</font>$<asp:Label runat="server" ID="lblTotalAmount"></asp:Label></span>
                        </p>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</asp:Content>
