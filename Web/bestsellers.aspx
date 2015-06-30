<%@ Page Language="c#" Inherits="AspDotNetStorefront.bestsellers" CodeFile="bestsellers.aspx.cs"  MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    <aspdnsf:XmlPackage ID="Package1" PackageName="page.bestsellers.xml.config" runat="server" EnforceDisclaimer="True" EnforcePassword="True" EnforceSubscription="True" AllowSEPropogation="true"/>
 </asp:Panel>
</asp:Content>
