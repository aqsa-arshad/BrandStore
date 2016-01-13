<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserInfoAfterLogin.ascx.cs" Inherits="controls_UserInfoAfterLogin" %>
<h3 id="WelcomeHeadingAfterUserLogin" runat="server"></h3>

<p class="hideforpublicuser"><asp:Label ID="lblSOF" runat="server" /></p>
<button class="btn btn-md btn-primary btn-block tablet-btn" type="button" id="btnViewAccountofuser">VIEW MY ACCOUNT</button>

<script type="text/javascript">
    $(document).ready(function () {      
        $("#btnViewAccountofuser").click(function () {
            window.open("JWMyAccount.aspx", '_self');
        });
    });
</script>
