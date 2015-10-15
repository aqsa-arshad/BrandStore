<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TrueBlueUserInfo.ascx.cs" Inherits="controls_TrueBlueUserInfo" %>
<h2 id="WelcomeHeadingAfterUserLogin" runat="server">Hi, Johnny Appleseed</h2>
<div class="right-logo-box">
    <img src="../App_Themes/Skin_3/images/true-blue-logo.png" />
    <span>Elite Dealer</span>
</div>
<h4>BLU Bucks</h4>
<div class="blu-bucks">
    <p>Level: Elite<br>
        BLU Bucks = XXXX</p>
    <a href="#">Other Credits </a>
</div>
<button class="btn btn-md btn-primary btn-block" type="button" id="btnViewAccount">VIEW MY ACCOUNT</button>
<a href="#" class="sm-link">Learn more about TrueBLU ></a><br>
<br>
<script type="text/javascript">
    $(document).ready(function () {
        $("#btnViewAccount").click(function () {
            window.open("JWMyAccount.aspx", '_self');
        });
    });
</script>
