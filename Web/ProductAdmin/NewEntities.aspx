<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminEntityEdit.master" Theme="Admin_Default" AutoEventWireup="true" CodeFile="NewEntities.aspx.cs" Inherits="AspDotNetStorefrontAdmin.NewEntities" %>
<%@ Register TagPrefix="aspdnsf" TagName="entitygrid" Src="Controls/EntityGrid.ascx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="entityMain" Runat="Server">
	<asp:Label ID="lblTitle" runat="server" CssClass="breadCrumb1" />
    <aspdnsf:entitygrid ID="grd" runat="server" />
</asp:Content>

