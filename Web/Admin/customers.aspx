<%@ Page Language="C#" AutoEventWireup="true" CodeFile="customers.aspx.cs" Inherits="AspDotNetStorefrontAdmin.customers" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="customergrid" Src="Controls/CustomerGrid.ascx" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
<style type="text/css">
.RadCalendarPopup
{
     z-index: 150000  !important;
}
</style>
</asp:Content>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:UpdatePanel ID="upCustomers" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <aspdnsf:customergrid ID="grd" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

