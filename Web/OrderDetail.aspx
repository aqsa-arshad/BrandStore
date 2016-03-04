<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="OrderDetail.aspx.cs" Inherits="AspDotNetStorefront.OrderDetail" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="content-box-03">
        <div class="row">
            <div class="col-md-12">
                <p class="label-text">
                    <span>
                        <font><asp:Label ID="lblOrderNumberCaption" runat="server" Text='<%$ Tokens:StringResource, orders.aspx.OrderNumber %>'/></font>
                        <asp:Label runat="server" ID="lblOrderNumber"></asp:Label>
                    </span>
                    <span>
                        <font><asp:Label ID="lblOrderDateCaption" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>" /></font>
                        <asp:Label runat="server" ID="lblOrderDate"></asp:Label>
                    </span>
                    <span>
                        <font><asp:Label ID="lblDeliveryStatus" runat="server" Text="Delivery Status" /></font>
                    </span>
                    <asp:Repeater ID="rptTrackingInformation" runat="server" OnItemDataBound="rptTrackingInformation_ItemDataBound">
                        <ItemTemplate>
                            <asp:HiddenField ID="hfShippingMethod" runat="server" Value='<%# Eval("ShippingMethod") %>' />
                            <asp:HiddenField ID="hfTrackingNumber" runat="server" Value='<%# Eval("TrackingNumber") %>' />
                            <asp:HiddenField ID="hfTrackingURL" runat="server" Value='<%# Eval("TrackingURL") %>' />
                            <asp:HiddenField ID="hfShippingStatus" runat="server" Value='<%# Eval("ShippingStatus") %>' />
                            <span>
                                <asp:Label ID="lblPackage" Text="Package" runat="server" />
                                <asp:Label ID="lblPackageNumber" runat="server" />
                                <%--<asp:Label ID="lblShippingMethod" runat="server" />--%>
                                <asp:Label ID="lblShippingStatus" runat="server" />
                                <asp:HyperLink ID="hlTrackItem" class="underline-link" Target="_blank" runat="server" />
                            </span>
                        </ItemTemplate>
                    </asp:Repeater>
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
                    <asp:Repeater ID="rptOrderItemsDetail" runat="server" OnItemDataBound="rptOrderItemsDetail_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <asp:HiddenField ID="hfRegularPrice" runat="server" Value='<%# Eval("RegularPrice") %>' />
                                <asp:HiddenField ID="hfBluBucks" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                <asp:HiddenField ID="hfSKU" runat="server" Value='<%# Eval("OrderedProductSKU") %>' />
                                <asp:HiddenField ID="hfCategoryFundUsed" runat="server" Value='<%# Eval("CategoryFundUsed") %>' />
                                <asp:HiddenField ID="hfBluBucksUsed" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                <asp:HiddenField ID="hfCreditPrice" runat="server" Value='<%# Eval("CreditPrice") %>' />
                                <asp:HiddenField ID="hfChosenColor" runat="server" Value='<%# Eval("ChosenColor") %>' />
                                <asp:HiddenField ID="hfProductID" runat="server" Value='<%# Eval("ProductID") %>' />
                                <asp:HiddenField ID="hfIsDownload" runat="server" Value='<%# Eval("IsDownload") %>' />
                                <asp:HiddenField ID="hfShippingTrackingNumber" runat="server" Value='<%# Eval("ShippingTrackingNumber") %>' />
                                <asp:HiddenField ID="hfShippingMethod" runat="server" Value='<%# Eval("ShippingMethod") %>' />
                                <asp:HiddenField ID="hfDownloadLocation" runat="server" Value='<%# Eval("DownloadLocation") %>' />
                                <asp:HiddenField ID="hfImageFileNameOverride" runat="server" Value='<%# Eval("ImageFileNameOverride") %>' />
                                <asp:HiddenField ID="hfFundName" runat="server" Value='<%# Eval("FundName") %>' />
                                 <asp:HiddenField ID="hfGLcode" runat="server" Value='<%# Eval("GLcode") %>' />
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
                                            <div class="shopping-cart-fix">
                                                <asp:Label ID="lblDescription" runat="server" Text='<%# Eval("Description") %>'></asp:Label>
                                            </div>
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

                                <td class="td-55-percent label-text">
                                    <span class="normal-heading black-color">
                                        <asp:Label ID="lblPaymentCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.payment %>' />
                                    </span>
                                    <span>
                                        <asp:Label ID="lblRegularPriceCaption" runat="server" Text='<%$ Tokens:StringResource,RegularPriceCaption  %>' /><asp:Label ID="lblRegularPrice" runat="server" />
                                    </span>
                                    <span>
                                        <asp:Label ID="lblBluBucksCaption" runat="server" Text='<%$ Tokens:StringResource,BluBucksCaption  %>' /><asp:Label ID="lblBluBuck" runat="server" />
                                    </span>
                                    <span>
                                        <asp:Label ID="lblCategoryFundCreditCaption" runat="server" /><asp:Label ID="lblCategoryFundCredit" runat="server" />
                                    </span>

                                     <span class="block-text">
                                        <asp:Label ID="lblSOFCodeCaption" runat="server" /><asp:Label runat="server" ID="lblSOFCode" Visible="false" Text='<%# Eval("SOFCode") %>'/>
                                    </span>

                                    <span>
                                        <asp:Label ID="lblCreditPriceCaption" runat="server" Text='<%$ Tokens:StringResource, CreditPriceCaption %>' /><asp:Label ID="lblCreditPrice" runat="server" />
                                    </span>
                                </td>
                                <td class="td-30-percent">
                                    <span class="normal-heading black-color">&nbsp;</span>
                                    <span>
                                        <asp:HyperLink ID="hlDelivery" Target="_blank" runat="server"></asp:HyperLink>
                                        <asp:Label ID="lblDelivery" runat="server"></asp:Label>
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
        <div>
            <div class="td-55-percent">
            </div>
            <div class="td-45-percent">
                <p>
                    <span class="black-blu-label">
                        <font><asp:Label ID="lblSubTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.96 %>'/>
                        </font>
                        <asp:Label runat="server" ID="lblSubTotal"></asp:Label>
                    </span>
                </p>

                <span class="normal-heading black-color">
                    <asp:Label ID="lblCreditsUsedCaption" runat="server" Text='<%$ Tokens:StringResource, CreditsUsedCaption %>' />
                </span>
                <p class="label-text">
                    <span class="block-text">
                        <asp:Label ID="lblBluBucksTotalCaption" Visible="False" Text='<%$ Tokens:StringResource, BluBucksCaption %>' runat="server" /><asp:Label runat="server" ID="lblBluBucksTotal" />
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblSOFFundsTotalCaption" Text='<%$ Tokens:StringResource, SOFFundsCaption %>' Visible="False" runat="server" /><asp:Label runat="server" ID="lblSOFFundsTotal" />
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblDirectMailFundsTotalCaption" Visible="False" Text="Direct Mail Funds: " runat="server" /><asp:Label runat="server" ID="lblDirectMailFundsTotal" />
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblDisplayFundsTotalCaption" Visible="False" Text="Display Funds: " runat="server" /><asp:Label runat="server" ID="lblDisplayFundsTotal" />
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblLiteratureFundsTotalCaption" Visible="False" Text="Literature Funds: " runat="server" /><asp:Label runat="server" ID="lblLiteratureFundsTotal" />
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblPOPFundsTotalCaption" Visible="False" Text="POP Funds: " runat="server" /><asp:Label runat="server" ID="lblPOPFundsTotal" />
                    </span>                    
                </p>

                <span class="normal-heading black-color">Charges</span>
                <p>
                    <span class="block-text">Taxes:
                        <asp:Label runat="server" ID="lblTax"></asp:Label></span>
                    <span class="block-text">
                        <asp:Label runat="server" ID="lblPurchasefee" Visible="false"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblShippingCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.aspx.12 %>' />
                        <asp:Label runat="server" ID="lblShippingCost"></asp:Label></span>
                </p>

                <p>
                    <span class="black-blu-label"><font><asp:Label ID="lblTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.61 %>'/></font>
                        <asp:Label runat="server" ID="lblTotalAmount"></asp:Label>
                    </span>
                </p>

                <div class="form-group">
                    <asp:HyperLink ID="hplPrintReceipt" runat="server" class="btn btn-primary btn-half" role="button" Text="Print Receipt" Target="_blank"></asp:HyperLink>
                    <asp:HyperLink ID="hplReOrder" runat="server" class="btn btn-primary btn-half" role="button" Text="Reorder"></asp:HyperLink>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnResendInfotoFulfillmentAPI" Text="Resend Info to Fulfillment API" OnClick="btnResendInfotoFulfillmentAPI_Click" OnClientClick='return confirm("Are you sure you want to resend Fulfillment API request?")' Visible="False" runat="server" class="btn btn-primary btn-block" />
                </div>
            </div>
            <div class="clearfix"></div>
        </div>
    </div>
</asp:Content>
