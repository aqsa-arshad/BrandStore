<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.search" CodeFile="search.aspx.cs"
    MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
    <div align="center" style="padding-top: 10px; padding-bottom: 20px" >
        <asp:Label runat="server" Text="<%$ Tokens:StringResource, admin.common.Search %>" />:
        <asp:TextBox type="text" ID="txtSearchTerm" runat="server" />
        <asp:Button runat="server" ID="btnSubmit" Text="<%$ Tokens:StringResource, admin.common.Submit %>"
            OnClick="btnSubmit_OnClick" CssClass="normalButtons" />
    </div>
    <asp:Literal runat="server" ID="ltContents" />
</asp:Content>
