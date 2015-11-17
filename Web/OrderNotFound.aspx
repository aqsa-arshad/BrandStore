<%@ Page Language="C#" AutoEventWireup="true" CodeFile="OrderNotFound.aspx.cs" Inherits="AspDotNetStorefront.OrderNotFound" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<asp:Content  runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <html lang="en">
    <head>
        <meta charset="utf-8">
        <meta content="IE=edge" http-equiv="X-UA-Compatible">
        <meta content="width=device-width, initial-scale=1" name="viewport">
        <meta content="" name="description">
        <meta content="" name="author">
        <title>Order Not Found</title>
    </head>
        <body>
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js"></script>
        <script src="../../dist/js/bootstrap.min.js"></script>
        <script src="../../assets/js/ie10-viewport-bug-workaround.js"></script>
        <script src="offcanvas.js"></script>
        </body>
        </html>
     <aspdnsf:Topic ID="FooterAboutTrueBlue" runat="server" TopicName="ordernotfound" />
</asp:Content>

