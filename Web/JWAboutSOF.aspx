<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWAboutSOF.aspx.cs" Inherits="AspDotNetStorefront.JWAboutSOF" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <aspdnsf:Topic ID="SalsRepsSupport" runat="server" TopicName="SalsRepsFooter.Support" />
</asp:Content>

