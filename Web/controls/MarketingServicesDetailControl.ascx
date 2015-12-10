<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingServicesDetailControl.ascx.cs" Inherits="controls_MarketingServicesDetailControl" %>
<%@ Import Namespace="AspDotNetStorefrontCore.dhl" %>

<div>
    <div class="content-box-03" id="ContentBox" runat="server">
    </div>
</div>

<script type="text/javascript">
    $(document).ready(function () {
        $("#btnShopMarketing").click(function () {
            window.open("c-6-marketing-services.aspx", '_self');
        });
    });
</script>
