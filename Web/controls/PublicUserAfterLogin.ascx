<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PublicUserAfterLogin.ascx.cs" Inherits="controls_PublicUserAfterLogin" %>
<h4 id="HeadingAfterPublicUserLogin" runat="server"></h4>
<div id="JeldWenProductSection" runat="server">
</div>
<button class="btn btn-md btn-primary btn-block" type="button" id="btnViewJeldwen">VISIT JELD-WEN.COM </button>

<script type="text/javascript">
    $(document).ready(function () {      
        $("#btnViewJeldwen").click(function () {
            window.open("https://www.jeld-wen.com");

        });
    });
</script>
