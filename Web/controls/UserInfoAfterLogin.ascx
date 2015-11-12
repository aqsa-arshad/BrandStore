<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserInfoAfterLogin.ascx.cs" Inherits="controls_UserInfoAfterLogin" %>
<h3 id="WelcomeHeadingAfterUserLogin" runat="server"></h3>
<%--Todo: True Blu Funtionality in Phase-2--%>
<%--<p class="hideforpublicuser">Sales Operations Funds = $$$$</p>
<a href="#" class="hideforpublicuser"><u>Request sales operations funds</u></a>--%>
<button class="btn btn-md btn-primary btn-block" type="button" id="btnViewAccountofuser">VIEW MY ACCOUNT</button>

<script type="text/javascript">
    $(document).ready(function () {      
        $("#btnViewAccountofuser").click(function () {
            window.open("JWMyAccount.aspx", '_self');

        });
    });
</script>
