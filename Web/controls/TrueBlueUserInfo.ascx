<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TrueBlueUserInfo.ascx.cs" Inherits="controls_TrueBlueUserInfo" %>
<h3 id="WelcomeHeadingAfterUserLogin" runat="server">Hi, Johnny Appleseed</h3>
<%--Todo: True Blu Funtionality in Phase-2--%>

<div class="right-logo-box">
    <img src="App_Themes/Skin_3/images/true-blue-logo.png" />
    <span><asp:Label ID="lblDealerLevel" runat="server" />Dealer</span>
</div>
<h4>BLU Bucks</h4>
<div class="blu-bucks">
    <p>
        <asp:Label runat="server" ID="lblCustomerLevel" Text=""></asp:Label>
        <br>
        <asp:Repeater ID="rptCustomerFunds" runat="server">
            <ItemTemplate>
                <asp:Label ID="lblCustomerFundName" runat="server" Text='<%# Eval("FundName") %>'></asp:Label>
                <asp:Label ID="lblSeprator" runat="server" Text="="></asp:Label>
                <asp:Label ID="lblCustomerFundAmount" runat="server" Text='<%# String.Format("{0:0.00}", Eval("Amount")) %>'></asp:Label>
                <br />
            </ItemTemplate>
        </asp:Repeater>
    </p>
    <a href="#" id="ExpandFunds" style="display:none">Other Credits </a>
</div>

<button class="btn btn-md btn-primary btn-block" type="button" id="btnViewAccount">VIEW MY ACCOUNT</button>
<%--Todo: True Blu Funtionality in Phase-2--%>
<%--<a href="#" class="sm-link">Learn more about TrueBLU ></a><br>--%>
<br>
<script type="text/javascript">
    $(document).ready(function () {
        $("#btnViewAccount").click(function () {
            window.open("JWMyAccount.aspx", '_self');
        });
    });
</script>
