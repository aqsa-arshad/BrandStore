<%@ Page Language="c#" Inherits="AspDotNetStorefront.search" CodeFile="search.aspx.cs" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel ID="pnlContent" runat="server">

        <asp:Literal ID="litSearch" runat="server"></asp:Literal>

    </asp:Panel>

    <script type="text/javascript">
        $(document).ready(function () {
            $("#beforelogindiv").hide();
            $(".beforelogin").hide();

            $("#headerlogo").click(function () {
                $("#headerlogo").attr("href", "home.aspx");
            });
        });
    </script>

</asp:Content>
