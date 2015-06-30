<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" AutoEventWireup="true" CodeFile="export.aspx.cs" Inherits="Admin_export" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="bodyContentPlaceholder" Runat="Server">

    <div id="container">
        <h1><asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.sectiontitle.export %>" /></h1>
        <p></p>
        <div>
            <asp:Button ID="SubmitButton" runat="server" Text="Export" OnClick="SubmitButton_Click" />
        </div>
    </div>

</asp:Content>

