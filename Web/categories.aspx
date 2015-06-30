<%@ Page language="c#" Inherits="AspDotNetStorefront.categories" CodeFile="categories.aspx.cs" EnableEventValidation="false" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
        <aspdnsf:XmlPackage id="Package1" PackageName="entitygridpage.xml.config" runat="server" EnforceDisclaimer="true" EnforcePassword="true" EnforceSubscription="true" AllowSEPropogation="true" RuntimeParams="entity=Category"/>
    </asp:Panel>
</asp:Content>




