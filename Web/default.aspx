<%@ Page Language="c#" Inherits="AspDotNetStorefront._default" CodeFile="default.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/empty.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="custom" TagName="Search" Src="Controls/Search.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <html lang="en">
    <head>
        <meta charset="utf-8">
        <meta content="IE=edge" http-equiv="X-UA-Compatible">
        <meta content="width=device-width, initial-scale=1" name="viewport">
        <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
        <meta content="" name="description">
        <meta content="" name="author">
        <title>Home</title>
        <!-- Bootstrap core CSS -->
    </head>
    <body>

        <div class="left-side">
            <%--Calling topic defind through admin console.--%>
            <aspdnsf:Topic ID="Topic2" runat="server" TopicName="LandingPageTopic" />
            <!-- Contect Box 02  -->
            <div class="content-box-02">
                <div class="row">
                    <%--calling xml package thst bringd featured products--%>
                    <asp:Literal ID="FeaturedProducts" runat="server" Text='<%$ Tokens:XMLPACKAGE, featuredproducts.xml.config, featuredentityid=11&featuredentitytype=category&headertext=Featured Products&numberofitems=4&columns=4&showprice=true %>' />
                </div>
            </div>
        </div>
       <!-- Bootstrap core JavaScript
    ================================================== -->
        <!-- Placed at the end of the document so the pages load faster -->
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js"></script>
        <script src="../../dist/js/bootstrap.min.js"></script>

        <!-- IE10 viewport hack for Surface/desktop Windows 8 bug -->
        <script src="../../assets/js/ie10-viewport-bug-workaround.js"></script>

        <script src="offcanvas.js"></script>
    </body>
    </html>

    <script type="text/javascript">
        $(document).ready(function () {
            //Hide categories for prelogin page
            $("#Category1").hide();
            $("#Category2").hide();
            $("#Category4").hide();

            $("#MobileCategory1").hide();
            $("#MobileCategory2").hide();
            $("#MobileCategory4").hide();

            $(".prelogin").hide();
            $("#afterlogindiv").hide();
            $(".afterlogin").hide();

            $("#headerlogo").click(function () {
                $("#headerlogo").attr("href", "default.aspx");
            });
            

        });
    </script>
</asp:Content>



