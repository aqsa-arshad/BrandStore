<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenEmptyTemplate.master" CodeFile="OrderReceipt.aspx.cs" Inherits="AspDotNetStorefront.OrderReceipt" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />

    <table border="0" cellpadding="0" cellspacing="0" width="950px">
        <tbody>
            <tr>
                <td>

                    <table class="receipt-print" border="0" cellpadding="0" cellspacing="0" width="100%">
                        <tbody>
                            <tr>
                                <td height="30" width="32%">
                                    <span class="frut-roman-font black-color text-uppercase">
                                        <asp:Label ID="lblOrderNumberCaption" runat="server" Text='<%$ Tokens:StringResource, orders.aspx.OrderNumber %>' />
                                        <asp:Label runat="server" ID="lblOrderNumber"></asp:Label>
                                    </span>
                                </td>
                                <td width="2%"></td>
                                <td width="32%">
                                    <span class="frut-roman-font black-color text-uppercase">
                                        <asp:Label ID="lblCustomerIDCaption" runat="server" Text="<%$ Tokens:StringResource,admin.common.CustomerID%>" />
                                        <asp:Label runat="server" ID="lblCustomerID"></asp:Label>
                                    </span>
                                </td>
                                <td width="2%"></td>
                                <td width="32%">
                                    <span class="frut-roman-font black-color text-uppercase">
                                        <asp:Label ID="lblOrderDateCaption" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>" />
                                        <asp:Label runat="server" ID="lblOrderDate"></asp:Label>
                                    </span>
                                </td>
                            </tr>

                            <tr>
                                <td height="30" width="32%" valign="top">
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
                                <td width="2%"></td>
                                <td valign="top">
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
                                <td width="2%"></td>
                                <td width="32%" valign="top">
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
                </td>
            </tr>

            <%--Items Detail--%>
            <tr>
                <td>
                    <table class="receipt-print" border="0" cellpadding="0" cellspacing="0" width="100%">
                        <tbody>
                            <asp:Repeater ID="rptOrderItemsDetail" runat="server" OnItemDataBound="rptOrderItemsDetail_ItemDataBound">
                                <HeaderTemplate>
                                    <tr>
                                        <td height="30">
                                            <span class="frut-roman-font black-color text-uppercase">Description</span>
                                        </td>
                                        <td width="2%"></td>
                                        <td width="14%">
                                            <span class="frut-roman-font black-color text-uppercase">Quantity</span>
                                        </td>
                                        <td width="2%"></td>
                                        <td width="32%">
                                            <span class="frut-roman-font black-color text-uppercase">Payment</span>
                                        </td>
                                    </tr>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <asp:HiddenField ID="hfRegularPrice" runat="server" Value='<%# Eval("RegularPrice") %>' />
                                        <asp:HiddenField ID="hfBluBucks" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                        <asp:HiddenField ID="hfSKU" runat="server" Value='<%# Eval("SKU") %>' />
                                        <asp:HiddenField ID="hfCategoryFundUsed" runat="server" Value='<%# Eval("CategoryFundUsed") %>' />
                                        <asp:HiddenField ID="hfBluBucksUsed" runat="server" Value='<%# Eval("BluBucksUsed") %>' />
                                        <asp:HiddenField ID="hfChosenColor" runat="server" Value='<%# Eval("ChosenColor") %>' />
                                        <asp:HiddenField ID="hfCreditPrice" runat="server" Value='<%# Eval("CreditPrice") %>' />
                                        <asp:HiddenField ID="hfProductID" runat="server" Value='<%# Eval("ProductID") %>' />
                                        <asp:HiddenField ID="hfImageFileNameOverride" runat="server" Value='<%# Eval("ImageFileNameOverride") %>' />

                                        <td width="50%" valign="top">
                                            <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                                <tbody>
                                                    <asp:HiddenField ID="hfDescription" runat="server" Value='<%# Eval("Description") %>' />
                                                    <tr>
                                                        <td width="40%" valign="top">
                                                            <div class="primary-img-box">
                                                                <asp:Image ID="ImgProduct" runat="server" class="img-responsive" />
                                                            </div>
                                                        </td>
                                                        <td width="60%" valign="top" valign="top">
                                                            <span class="normal-heading black-color">
                                                                <asp:Label ID="lblProductName" runat="server" Text='<%# Eval("OrderedProductName") %>'></asp:Label>
                                                            </span>
                                                            <span>
                                                                <asp:Label ID="lblProductSKU" runat="server" />
                                                            </span>
                                                            <div class="shopping-cart-fix">
                                                                <asp:Label ID="lblDescription" runat="server"></asp:Label>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>
                                        <td width="2%"></td>
                                        <td width="14%" valign="top">
                                            <asp:Label ID="lblQuantity" runat="server" Text='<%# Eval("Quantity") %>' />
                                        </td>
                                        <td width="2%"></td>

                                        <td width="32%" valign="top">
                                            <span class="block-text">
                                                <asp:Label ID="lblRegularPriceCaption" runat="server" Text='<%$ Tokens:StringResource,RegularPriceCaption  %>' /><asp:Label ID="lblRegularPrice" runat="server" />
                                            </span>
                                            <span class="block-text">
                                                <asp:Label ID="lblCategoryFundCreditCaption" runat="server" Text='<%$ Tokens:StringResource,CategoryFundCreditCaption  %>' /><asp:Label ID="lblCategoryFundCredit" runat="server" />
                                            </span>
                                            <span class="block-text">
                                                <asp:Label ID="lblBluBucksCaption" runat="server" Text='<%$ Tokens:StringResource,BluBucksCaption  %>' /><asp:Label ID="lblBluBuck" runat="server" />
                                            </span>
                                            <span class="block-text">
                                                <asp:Label ID="lblCreditPriceCaption" runat="server" Text='<%$ Tokens:StringResource, CreditPriceCaption %>' /><asp:Label ID="lblCreditPrice" runat="server" />
                                            </span></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </td>
            </tr>
            <tr>
                <td>
                    <%--Billing Amounts--%>
                    <table class="receipt-print" border="0" cellpadding="0" cellspacing="0" width="100%">
                        <tbody>
                            <tr class="border-none">

                                <td width="50%"></td>
                                <td width="2%"></td>
                                <td width="14%"></td>
                                <td width="2%"></td>
                                <td width="32%">
                                    <p class="label-text">
                                        <span>
                                            <font><asp:Label ID="lblSubTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.96 %>'/></font>
                                            <asp:Label runat="server" ID="lblSubTotal"></asp:Label>
                                        </span>
                                    </p>
                                    <span class="normal-heading black-color">
                                        <asp:Label ID="lblCreditsUsedCaption" runat="server" Text='<%$ Tokens:StringResource, CreditsUsedCaption %>' />
                                    </span>
                                    <p class="label-text">
                                        <span class="block-text">
                                            <asp:Label ID="lblSOFFundsTotalCaption" Text="SOF Fund: " Visible="False" runat="server" /><asp:Label runat="server" ID="lblSOFFundsTotal" Visible="False"></asp:Label>
                                        </span>
                                        <span class="block-text">
                                            <asp:Label ID="lblDirectMailFundsTotalCaption" Visible="False" Text="Direct Mail Funds: " runat="server" /><asp:Label runat="server" ID="lblDirectMailFundsTotal" Visible="False"></asp:Label>
                                        </span>
                                        <span class="block-text">
                                            <asp:Label ID="lblDisplayFundsTotalCaption" Visible="False" Text="Display Funds: " runat="server" /><asp:Label runat="server" ID="lblDisplayFundsTotal" Visible="False"></asp:Label>
                                        </span>
                                        <span class="block-text">
                                            <asp:Label ID="lblLiteratureFundsTotalCaption" Visible="False" Text="Literature Funds: " runat="server" /><asp:Label runat="server" ID="lblLiteratureFundsTotal" Visible="False"></asp:Label>
                                        </span>
                                        <span class="block-text">
                                            <asp:Label ID="lblPOPFundsTotalCaption" Visible="False" Text="POP Funds: " runat="server" /><asp:Label runat="server" ID="lblPOPFundsTotal" Visible="False"></asp:Label>
                                        </span>
                                        <span class="block-text">
                                            <asp:Label ID="lblBluBucksTotalCaption" Visible="False" Text="BLU Bucks: " runat="server" /><asp:Label runat="server" ID="lblBluBucksTotal" Visible="False"></asp:Label>
                                        </span>
                                    </p>

                                    <span class="normal-heading black-color">Charges</span>
                                    <p class="label-text">
                                        <span>Taxes:
                                            <asp:Label runat="server" ID="lblTax"></asp:Label></span>
                                        <span>
                                            <asp:Label ID="lblShippingCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.aspx.12 %>' />
                                            <asp:Label runat="server" ID="lblShippingCost"></asp:Label>
                                        </span>
                                    </p>

                                    <p class="label-text">
                                        <span><font><asp:Label ID="lblTotalCaption" runat="server" Text='<%$ Tokens:StringResource, shoppingcart.cs.61 %>'/> </font>
                                            <asp:Label runat="server" ID="lblTotalAmount"></asp:Label>
                                        </span>
                                    </p>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>
</asp:Content>
