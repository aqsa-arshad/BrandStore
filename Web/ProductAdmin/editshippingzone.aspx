<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.editshippingzone" CodeFile="editshippingzone.aspx.cs"
    MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">

    <script type="text/javascript">
        function ShippingZoneForm_Validator(theForm) {
            var hfAddressCountry = document.getElementById("hfAddressCountry");
            var ddlAddressCountry = document.getElementById("AddressCountry");
            hfAddressCountry.value = ddlAddressCountry.options[ddlAddressCountry.selectedIndex].value;
        }
    </script>

</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltContent" runat="server" />
</asp:Content>
