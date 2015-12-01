<%@ Page Language="c#" Inherits="AspDotNetStorefront.showproduct" CodeFile="showproduct.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" EnableViewStateMac="false" %>

<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel runat="server">
        <asp:Literal ID="litOutput" runat="server"></asp:Literal>
    </asp:Panel>

    <script type="text/javascript">
        $(document).ready(function () {
            if ('<%=parentCategoryID%>' == "1") {
                $("#MCCategory1").addClass("active");
            }
            else if ('<%=parentCategoryID%>' == "2") {
                $("#MCCategory2").addClass("active");
            }
            else if ('<%=parentCategoryID%>' == "3") {
                $("#MCCategory3").addClass("active");
            }
            else if ('<%=parentCategoryID%>' == "4") {
                $("#MCCategory4").addClass("active");
            }
            else if ('<%=parentCategoryID%>' == "5") {
                $("#MCCategory5").addClass("active");
            }
            else {
                $("#MCCategory6").addClass("active");
            }
        });
    </script>
</asp:Content>

