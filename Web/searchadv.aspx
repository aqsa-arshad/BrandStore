<%@ Page language="c#" Inherits="AspDotNetStorefront.searchadv" CodeFile="searchadv.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >    
        <aspdnsf:XmlPackage id="searchPackage" PackageName="page.searchadv.xml.config" runat="server"/>
    </asp:Panel>
</asp:Content>
