<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" CodeFile="MarketingServicesDetailPage.aspx.cs" Inherits="MarketingServicesDetailPage" %>

<%@ Register TagPrefix="aspdnsf" TagName="MarketingServicesDetail" Src="~/Controls/MarketingServicesDetailControl.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />

    <aspdnsf:MarketingServicesDetail runat="server" />
</asp:Content>
