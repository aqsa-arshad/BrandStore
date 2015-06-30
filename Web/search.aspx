<%@ Page language="c#" Inherits="AspDotNetStorefront.search" CodeFile="search.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master"  %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register assembly="AspDotNetStorefrontControls" namespace="AspDotNetStorefrontControls" tagprefix="aspdnsf" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    
        <asp:Literal ID="litSearch" runat="server"></asp:Literal>       
        
    </asp:Panel>
</asp:Content>
