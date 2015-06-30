<%@ Page Language="c#" Inherits="AspDotNetStorefront.sitemap" CodeFile="sitemap.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
        
        <asp:Panel runat="server" ID="PackagePanel" Visible="True">
            
        </asp:Panel>
        <asp:Panel runat="server" ID="EntityPanel" Visible="False">
            <asp:Literal runat="Server" ID="Literal1" />
        </asp:Panel>
        
    </asp:Panel>
</asp:Content>
