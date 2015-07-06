<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminEntityEdit.master"
    Theme="Admin_Default" AutoEventWireup="true" CodeFile="NewProducts.aspx.cs" Inherits="AspDotNetStorefrontAdmin.NewProducts" %>

<%@ Register TagPrefix="aspdnsf" TagName="productgrid" Src="Controls/ProductsGrid.ascx" %>
<asp:Content ContentPlaceHolderID="head" runat="server">

    <script type="text/javascript">
            function calcHeight(elName) {
                //find the height of the internal page
                var the_height = document.getElementById(elName).contentWindow.document.body.scrollHeight + 5;

                //change the height of the iframe
                document.getElementById(elName).height = the_height;
            }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="entityMain" runat="Server">
    <aspdnsf:productgrid ID="grd" runat="server" />
</asp:Content>
