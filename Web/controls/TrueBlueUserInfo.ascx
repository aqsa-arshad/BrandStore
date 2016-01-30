<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TrueBlueUserInfo.ascx.cs" Inherits="controls_TrueBlueUserInfo" %>
<h3 id="WelcomeHeadingAfterUserLogin" runat="server">Hi, Johnny Appleseed</h3>
<div class="right-logo-box" id="dLogoBox" runat="server">
    <img src="App_Themes/Skin_3/images/true-blue-logo.png" />
    <span>
        <asp:Label ID="lblDealerLevel" runat="server" />Dealer</span>
</div>
<h4 runat="server" id="hBluBucks"><asp:Label ID="BluBucks" runat="server" Text="BLU™ Bucks"></asp:Label></h4>
<div class="blu-bucks">
    <div>
        <asp:Label runat="server" ID="lblCustomerLevel" Text=""></asp:Label>
        <asp:Repeater ID="rptCustomerFunds" runat="server">
            <ItemTemplate>
                <span class="block-text">
                <asp:Label ID="lblCustomerFundName" runat="server" Text='<%# Eval("FundName") %>'></asp:Label>
                <asp:Label ID="lblSeprator" runat="server" Text="="></asp:Label>
                <asp:Label ID="lblCustomerFundAmount" runat="server" Text='<%# String.Format("{0:0.00}", Eval("AmountAvailable")) %>'></asp:Label>                  

                </span>
            </ItemTemplate>
        </asp:Repeater>
        <div class="collapse" id="collapseExample">
            <div class="well">
                <asp:Repeater ID="rptAllCustomerFunds" runat="server">
                    <ItemTemplate>
                        <span class="block-text">
                            <asp:Label runat="server" Text='<%# Eval("FundName") %>'></asp:Label>
                            <asp:Label runat="server" Text="="></asp:Label>
                            <asp:Label runat="server" Text='<%# String.Format("{0:C}", Eval("AmountAvailable")) %>'></asp:Label>
                        </span>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
    <asp:LinkButton runat="server" ID="ExpandFunds" class="blu-collapse-link" data-toggle="collapse" href="#collapseExample" aria-expanded="false" aria-controls="collapseExample" OnClientClick="expandFunds()" Text="Other Credits ˅"></asp:LinkButton>
    <asp:LinkButton runat="server" ID="lnkHideFunds" class="blu-collapse-link" data-toggle="collapse" href="#collapseExample" aria-expanded="false" aria-controls="collapseExample" OnClientClick="hideFunds()" Text="Close other credits ˄" style="display:none;"></asp:LinkButton>
</div>
<a href="JWMyAccount.aspx" class="btn btn-md btn-primary btn-block tablet-btn" type="button" id="btnViewAccount">VIEW MY ACCOUNT</a>
<br />
<a id="lnkLearnMoreAboutTruBlue" href="JWAboutTrueBlu.aspx" class="sm-link">Learn more about True BLU™ ></a>
<script>
    function expandFunds() {
        var clientID = '<%= ExpandFunds.ClientID %>';
        var lnkHideclientID = '<%= lnkHideFunds.ClientID %>'; 

        $("#" + clientID).css("display", "none");
        $("#" + lnkHideclientID).css("display", "block");

    }
    function hideFunds() {

        var clientID = '<%= ExpandFunds.ClientID %>';
        var lnkHideclientID = '<%= lnkHideFunds.ClientID %>';

        $("#" + clientID).css("display", "block");
        $("#" + lnkHideclientID).css("display", "none");


    }
</script>
