<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserInfoAfterLogin.ascx.cs" Inherits="controls_UserInfoAfterLogin" %>
<h4 id="WelcomeHeadingAfterUserLogin" runat="server"></h4>
<p class="hideforpublicuser">Sales Operations Funds = $$$$</p>
<a href="#" class="hideforpublicuser"><u>Request sales operations funds</u></a>
<button class="btn btn-md btn-primary btn-block" type="button" id="btnViewAccount">VIEW MY ACCOUNT</button>

<script type="text/javascript">
    $(document).ready(function () {
      
        $("#btnViewAccount").click(function () {
            window.open("Account.aspx", '_self');
        });
    });
</script>
