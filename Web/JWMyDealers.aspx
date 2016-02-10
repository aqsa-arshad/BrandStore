<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWMyDealers.aspx.cs" Inherits="AspDotNetStorefront.JWMyDealers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="search-filters" id="myDealersHeader">
        <div class="row tablet-view">
            <div class="col-md-6 pull-left-md pull-sm-no">
                <label>Sort By: </label>
                <asp:DropDownList ID="OrderBylist" runat="server" AutoPostBack="true" OnSelectedIndexChanged="OrderBylist_SelectedIndexChanged">
                    <asp:ListItem Value="nameAsc">Name A-Z</asp:ListItem>
                    <asp:ListItem Value="nameDsc">Name Z-A</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-md-6 pull-left-md pull-sm-no">
                <label>Items per page: </label>
                <asp:DropDownList ID="PageSizeList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="PageSizelist_SelectedIndexChanged">
                    <asp:ListItem Value="10">10</asp:ListItem>
                    <asp:ListItem Value="25">25</asp:ListItem>
                    <asp:ListItem Value="10000">View All</asp:ListItem>
                </asp:DropDownList>
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
                                <asp:LinkButton runat="server" OnClick="DealersHeader_Click" CssClass="link-no">
                                    <asp:Label ID="lblDealerNameHeading" runat="server" Text="<%$ Tokens:StringResource,mydealers.aspx.1 %>" CssClass="normal-heading black-color" />
                                </asp:LinkButton>
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
    <script>

        $("#myDealersHeader")
    </script>
</asp:Content>

