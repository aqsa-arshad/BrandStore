<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DealerInfoAfterLogin.ascx.cs" Inherits="controls_DealerInfoAfterLogin" %>
<h4>MY DEALERS</h4>
<div class="row">  
</div>
<p>View your dealers’ activity.</p>
<button class="btn btn-md btn-primary btn-block tablet-btn" type="button" id="btnViewReport">VIEW MY REPORTS</button>
<script type="text/javascript">
    $(document).ready(function () {      
        $("#btnViewReport").click(function () {
            window.open("JWMyDealers.aspx", '_self');
        });
    });
</script>

