<%@ Page Language="c#" Inherits="AspDotNetStorefront.smartcheckout" CodeFile="smartcheckout.aspx.cs"
    MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register Src="OPCControls/OnePageCheckout.ascx" TagName="OnePageCheckout" TagPrefix="uc1" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <uc1:OnePageCheckout ID="OnePageCheckout1" runat="server" />
	<asp:Literal ID="ltPayPalIntegratedCheckout" runat="server" />
</asp:Content>
