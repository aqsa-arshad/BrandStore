<%@ Page Language="c#" Inherits="AspDotNetStorefront.sitemap2" CodeFile="sitemap2.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    
    <ComponentArt:SiteMap ID="SiteMap1" runat="server" LeafNodeCssClass="SiteMapLeafNode"
            ParentNodeCssClass="SiteMapParentNode" RootNodeCssClass="SiteMapRootNode" CssClass="SiteMap"
            TreeLineImageHeight="20" TreeLineImageWidth="19" TreeLineImagesFolderUrl="images/lines2"
            TreeShowLines="true" SiteMapLayout="Tree" Width="100%">
            <Table>
                <ComponentArt:SiteMapTableRow>
                    <ComponentArt:SiteMapTableCell RootNodes="1" valign="top" Width="25%"></ComponentArt:SiteMapTableCell>
                    <ComponentArt:SiteMapTableCell RootNodes="1" valign="top" Width="25%"></ComponentArt:SiteMapTableCell>
                    <ComponentArt:SiteMapTableCell RootNodes="1" valign="top" Width="25%"></ComponentArt:SiteMapTableCell>
                    <ComponentArt:SiteMapTableCell valign="top" Width="25%"></ComponentArt:SiteMapTableCell>
                </ComponentArt:SiteMapTableRow>
            </Table>
        </ComponentArt:SiteMap>
        
        
    </asp:Panel>
</asp:Content>
