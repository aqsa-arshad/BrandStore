<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWDealersOrderHistory.aspx.cs" Inherits="AspDotNetStorefront.JWDealersOrderHistory" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel ID="pnlOrderHistory" runat="server">
        <div class="content-box-03">
            <asp:Panel ID="pnlFundsInformation" runat="server" Visible="false">
                <div class="row">
                    <div class="col-md-6">
                        <p class="label-text">
                            <span>
                                <font><asp:Label ID="lblBluBucksHeading" runat="server" Text="<%$ Tokens:StringResource,BluBucksHeading%>"/></font>
                                <asp:Label runat="server" ID="lblBluBucks"></asp:Label>
                            </span>
                            <span>
                                <font><asp:Label ID="lblCustomerLevelHeading" runat="server" Text="<%$ Tokens:StringResource,CustomerLevelHeading%>"/></font>
                                <asp:Label runat="server" ID="lblTierLevel"></asp:Label>
                            </span>
                        </p>
                    </div>
                    <div class="col-md-6">
                        <p class="label-text">
                            <span>
                                <font><asp:Label ID="lblDirectMailFundsHeading" runat="server" Text="<%$ Tokens:StringResource,DirectMailFundsHeading%>"></asp:Label></font>
                                <asp:Label ID="lblDirectMailFunds" runat="server"></asp:Label>
                            </span>
                            <span>
                                <font><asp:Label ID="lblDisplayFundsHeading" runat="server" Text="<%$ Tokens:StringResource,DisplayFundsHeading%>"></asp:Label></font>
                                <asp:Label ID="lblDisplayFunds" runat="server"></asp:Label>
                            </span>
                            <span>
                                <font><asp:Label ID="lblLiteratureFundsHeading" runat="server" Text="<%$ Tokens:StringResource,LiteratureFundsHeading%>"></asp:Label></font>
                                <asp:Label ID="lblLiteratureFunds" runat="server"></asp:Label>
                            </span>
                            <span>
                                <font><asp:Label ID="lblPOPFundsHeading" runat="server" Text="<%$ Tokens:StringResource,POPFundsHeading%>"></asp:Label></font>
                                <asp:Label ID="lblPOPFunds" runat="server"></asp:Label>
                            </span>
                        </p>
                    </div>
                </div>
                <div class="border-line"></div>
            </asp:Panel>
            <div>
                <asp:HiddenField ID="hfAccountId" runat="server" />
                <asp:HiddenField ID="hfCustomerID" runat="server" />
                <table class="table">
                    <tbody>
                        <asp:Repeater ID="rptOrderhistory" runat="server">
                            <HeaderTemplate>
                                <tr class="table-row">
                                    <td class="td-20-percent">
                                        <asp:LinkButton runat="server" OnClick="lblOrderNumber_Click" CssClass="link-no">
                                            <asp:Label ID="lblOrderNumber" runat="server" Text="<%$ Tokens:StringResource,account.aspx.36 %>" CssClass="normal-heading black-color" />
                                        </asp:LinkButton>
                                    </td>
                                    <td class="td-25-percent">
                                       <asp:LinkButton runat="server" OnClick="lblOrderDate_Click" CssClass="link-no">
                                            <asp:Label ID="lblOrderDate" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>" CssClass="normal-heading black-color" />
                                        </asp:LinkButton>
                                    </td>
                                    <td class="td-25-percent">
                                        <asp:LinkButton runat="server" OnClick="lblPayment_Click" CssClass="link-no">
                                            <asp:Label ID="lblPayment" runat="server" Text="<%$ Tokens:StringResource,account.aspx.39%>" CssClass="normal-heading black-color" />
                                        </asp:LinkButton>
                                    </td>
                                    <td class="td-30-percent">
                                        <asp:LinkButton runat="server" OnClick="lblStatus_Click" CssClass="link-no">
                                            <asp:Label ID="lblStatus" runat="server" Text="<%$ Tokens:StringResource,account.aspx.106 %>" CssClass="normal-heading black-color" />
                                        </asp:LinkButton>
                                    </td>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="table-row">
                                    <td class="td-20-percent">
                                        <%# DataBinder.Eval(Container.DataItem, "OrderNumber").ToString() %>
                                        <a class="underline-link" href='<%#m_StoreLoc + "OrderDetail.aspx?ordernumber=" + DataBinder.Eval(Container.DataItem, "OrderNumber") + "&AccountId=" + hfAccountId.Value %>'>View Detail</a>
                                    </td>
                                    <td class="td-25-percent">
                                        <%#Localization.ConvertLocaleDate(DataBinder.Eval(Container.DataItem, "OrderDate").ToString(), Localization.GetDefaultLocale(), ThisCustomer.LocaleSetting)%>
                                    </td>
                                    <td class="td-25-percent">
                                        Total: $<%# Math.Round(Convert.ToDecimal(DataBinder.Eval(Container.DataItem, "OrderTotal")), 2).ToString() %>
                                    </td>
                                    <td class="td-30-percent">
                                        <asp:Label ID="lblUsername" runat="server" Text='<%# Eval("Username") %>'></asp:Label>
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


