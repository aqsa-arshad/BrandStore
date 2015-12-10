<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingServicesAfterLogin.ascx.cs" Inherits="controls_MarketingServicesAfterLogin" %>

<div id="MarketingServiceSection" runat="server">
</div>

<script type="text/javascript">
    $(document).ready(function () {
        $("#btnShowMarketingServicesDetail").click(function () {
            window.open("MarketingServicesDetailPage.aspx", '_self');
        });
    });
</script>
