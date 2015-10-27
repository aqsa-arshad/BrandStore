<%@ Page Language="c#" Inherits="AspDotNetStorefront.showcategory" CodeFile="showcategory.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel runat="server">
        <span class="paypalAd">
            <asp:Literal ID="ltPayPalAd" runat="server" />
        </span>
        <asp:Literal ID="litOutput" runat="server"></asp:Literal>
    </asp:Panel>

    <script type="text/javascript">
        $(document).ready(function () {
            debugger;
            if ('<%=CategoryTypeFlag%>' == "true") {
                $("#divbeforelogin").addClass("sub-category-right");
                $("#ctl00_divafterlogin").addClass("sub-category-right");
            } else {
                $("#divbeforelogin").addClass("category-right");
                $("#ctl00_divafterlogin").addClass("category-right");
            }
            $("#beforelogindiv").hide();
            $(".beforelogin").hide();

            $("#headerlogo").click(function () {
                $("#headerlogo").attr("href", "home.aspx");
            });
        });
    </script>
</asp:Content>
