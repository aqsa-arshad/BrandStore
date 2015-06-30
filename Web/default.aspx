<%@ Page Language="c#" Inherits="AspDotNetStorefront._default" CodeFile="default.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
		<asp:Literal ID="ltPayPalAd" runat="server" />
        <aspdnsf:XmlPackage id="Package1" runat="server" EnforceDisclaimer="true" EnforcePassword="true" EnforceSubscription="True" AllowSEPropogation="True"/>
    </asp:Panel>
</asp:Content>
