<%@ Page Language="c#" Inherits="AspDotNetStorefront._home" CodeFile="home.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/empty.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="custom" TagName="Search" Src="Controls/Search.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="JWBBrandMerchandiseExampleExample" Src="~/Controls/BrandMerchandiseExample.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="JWBBrandAssetExample" Src="~/Controls/BrandAssetExample.ascx" %>
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
            <aspdnsf:Topic ID="Topic2" runat="server" TopicName="HomePageTopic" />
            <!-- Contect Box 02  -->
            <div class="row">
                <div class="col-md-6">
                    <aspdnsf:JWBBrandAssetExample runat="server" ID="JWBBrandAssetExample" />
                </div>
                <div class="col-md-6">
                    <aspdnsf:JWBBrandMerchandiseExampleExample runat="server" ID="JWBBrandMerchandiseExampleExample" />
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



