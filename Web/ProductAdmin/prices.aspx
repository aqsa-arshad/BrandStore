<%@ Page Language="C#" CodeFile="prices.aspx.cs" Inherits="AspDotNetStorefrontAdmin.prices"
    MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register Src="Controls/BulkEditVariants.ascx" TagName="BulkEditVariants" TagPrefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContentPlaceholder" runat="Server">
    <uc1:BulkEditVariants ID="BulkEditVariants1" runat="server" />
</asp:Content>
