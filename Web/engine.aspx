<%@ Page language="c#" Inherits="AspDotNetStorefront.engine" CodeFile="engine.aspx.cs" EnableEventValidation="false" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
        <aspdnsf:XmlPackage id="Package1" runat="server" EnforceDisclaimer="true" EnforcePassword="true" EnforceSubscription="true" AllowSEPropogation="true" ReplaceTokens="true"/>
    </asp:Panel>
</asp:Content>

