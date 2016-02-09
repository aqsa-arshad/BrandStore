<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWMyDealers.aspx.cs" Inherits="AspDotNetStorefront.JWMyDealers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="search-filters">
        <div class="row tablet-view">
            <div class="col-md-6 pull-left-md pull-sm-no" id="catSortByTop">
                <label>Sort By: </label>
                <select id="SelectSortTop" onchange="setParam('sortby', this.value)" name="SelectSort">
                    <option value="relevance" selected="">Relevance</option>
                    <option value="priceasc">Low Price</option>
                    <option value="pricedesc">High Price</option>
                    <option value="name">Name A-Z</option>
                </select>
            </div>
            <div class="col-md-6 pull-left-md pull-sm-no">
                <label>Items per page: </label>
                <select id="PageSizeTop" onchange="setParam('pagesize', this.value)" name="PageSize">
                    <option value="12">12</option>
                    <option value="48">48</option>
                    <option value="10000">View All</option>
                </select>
            </div>
            <div class="clearable"></div>
        </div>
    </div>
    <div class="content-box-03">
        <table class="table">
            <tbody>
                <asp:Repeater ID="rptMyDealers" runat="server">
                    <HeaderTemplate>
                        <tr class="table-row">
                            <td class="td-50-percent">
                                <span class="normal-heading black-color">
                                    <asp:Label ID="lblDealerNameHeading" runat="server" Text="<%$ Tokens:StringResource,mydealers.aspx.1 %>" />
                                </span>
                            </td>
                            <td class="td-50-percent"></td>
                        </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr class="table-row">
                            <td class="td-50-percent">
                                <span class="normal-heading black-color">
                                    <a class="underline-link" href="JWDealersOrderHistory.aspx?AccountId=<%# Eval("Id") %>"><%# Eval("Name") %></a>
                                </span>
                            </td>
                            <td class="td-50-percent"></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <asp:Label ID="lblDealerNotFound" runat="server" Visible="true" Text="<%$ Tokens:StringResource,mydealers.aspx.2 %>"></asp:Label>
    </div>
    <div class="pagerArea bottomPagerArea disabled-pageArea padding-none">
        <asp:Repeater ID="rptPager" runat="server">
            <ItemTemplate>
                <asp:LinkButton ID="lnkPage" runat="server" Text='<%#Eval("Text") %>' CommandArgument='<%# Eval("Value") %>' Enabled='<%# Convert.ToBoolean(Eval("Enabled")) %>'
                    OnClick="Page_Changed" />
            </ItemTemplate>
        </asp:Repeater>
    </div>
</asp:Content>

