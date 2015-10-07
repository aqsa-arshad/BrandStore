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
        <meta content="" name="description">
        <meta content="" name="author">
        <title>Home</title>
    </head>
    <body>
        <div class="left-side">
            <%--Calling topic defind through admin console.--%>
            <aspdnsf:Topic ID="Topic2" runat="server" TopicName="LandingPageTopic" />
            <div class="content-box-02">
                <div class="row">
                    <%--calling xml package thst bringd featured products--%>
                    <asp:Literal ID="FeaturedProducts" runat="server" Text='<%$ Tokens:XMLPACKAGE, featuredproducts.xml.config, featuredentityid=11&featuredentitytype=category&headertext=Featured Products&numberofitems=4&columns=4&showprice=true %>' />
                </div>
            </div>
        </div>
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js"></script>
        <script src="../../dist/js/bootstrap.min.js"></script>
        <script src="../../assets/js/ie10-viewport-bug-workaround.js"></script>
        <script src="offcanvas.js"></script>
    </body>
    </html>
</asp:Content>



