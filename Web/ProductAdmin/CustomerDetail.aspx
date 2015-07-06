<%@ Page Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"
    AutoEventWireup="true" CodeFile="CustomerDetail.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Admin_CustomerDetail" %>

<%@ Register src="Controls/editcustomer.ascx" tagname="editcustomer" tagprefix="aspdnsf" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContentPlaceholder" Runat="Server">
    <div id="container">
        <aspdnsf:editcustomer ID="editcustomer1" runat="server" />
    </div>  
</asp:Content>

