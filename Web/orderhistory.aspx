<%@ Page Language="c#" AutoEventWireup="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" Inherits="AspDotNetStorefront.orderhistory" CodeFile="orderhistory.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel ID="pnlOrderHistory" runat="server">
        <div class="content-box-03">
            <div>
                <asp:HiddenField ID="hfCustomerID" runat="server" />
                <table class="table">
                    <tbody>
                        <asp:Repeater ID="rptOrderhistory" runat="server">
                            <ItemTemplate>
                                <tr class="table-row">
                                    <td class="td-25-percent">
                                        <span class="normal-heading black-color">
                                            <asp:Label ID="lblOrderNumber" runat="server" Text="<%$ Tokens:StringResource,account.aspx.36 %>" />
                                        </span>
                                        <%# DataBinder.Eval(Container.DataItem, "OrderNumber").ToString() %>
                                        <a class="underline-link" href='<%#m_StoreLoc + "OrderDetail.aspx?ordernumber=" + DataBinder.Eval(Container.DataItem, "OrderNumber") %>'>View Detail</a>
                                    </td>
                                    <td class="td-25-percent">
                                        <span class="normal-heading black-color">
                                            <asp:Label ID="lblOrderDate" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>" />
                                        </span>
                                        <%#Localization.ConvertLocaleDate(DataBinder.Eval(Container.DataItem, "OrderDate").ToString(), Localization.GetDefaultLocale(), ThisCustomer.LocaleSetting)%>
                                    </td>
                                    <td class="td-25-percent">
                                        <span class="normal-heading black-color">
                                            <asp:Label ID="lblPayment" runat="server" Text="<%$ Tokens:StringResource,account.aspx.39%>" />
                                        </span>
                                        Total: $<%# Math.Round(Convert.ToDecimal(DataBinder.Eval(Container.DataItem, "OrderTotal")), 2).ToString() %>
                                        <%--<%#GetPaymentStatus(DataBinder.Eval(Container.DataItem, "PaymentMethod").ToString(), DataBinder.Eval(Container.DataItem, "CardNumber").ToString(), DataBinder.Eval(Container.DataItem, "TransactionState").ToString(), DataBinder.Eval(Container.DataItem, "OrderTotal").ToString())%>--%>
                                    </td>
                                    <td class="td-25-percent">
                                        <span class="normal-heading black-color">
                                            <asp:Label ID="lblStatus" runat="server" Text="<%$ Tokens:StringResource,account.aspx.40 %>" />
                                        </span>
                                        <%#GetShippingStatus(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "OrderNumber").ToString()), DataBinder.Eval(Container.DataItem, "ShippedOn").ToString()) + "&nbsp;"%>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
                <asp:Label ID="lblOrderNotFound" runat="server" Text="<%$ Tokens:StringResource,account.aspx.55 %>"></asp:Label>
            </div>
            <div class="pagerArea bottomPagerArea disabled-pageArea padding-none">
                <asp:Repeater ID="rptPager" runat="server">
                    <ItemTemplate>

                        <asp:LinkButton ID="lnkPage" runat="server" Text='<%#Eval("Text") %>' CommandArgument='<%# Eval("Value") %>' Enabled='<%# Convert.ToBoolean(Eval("Enabled")) %>'
                            OnClick="Page_Changed" />

                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </asp:Panel>

</asp:Content>
