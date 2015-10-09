<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TrueBlueUserInfo.ascx.cs" Inherits="controls_TrueBlueUserInfo" %>
<h4 id="WelcomeHeadingAfterUserLogin" runat="server">TRUEBLU MODULE</h4>
<p>Sales Operations Funds = $$$$</p>
<a href="#"><u>Request sales operations funds</u></a>
<button class="btn btn-md btn-primary btn-block" type="button" id="btnViewAccount">VIEW MY ACCOUNT</button>
<script type="text/javascript">
    $(document).ready(function () {      
        $("#btnViewAccount").click(function () {
            window.open("Account.aspx", '_self');
        });
    });
</script>