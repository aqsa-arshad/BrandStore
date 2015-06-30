<%@ Page Language="C#" AutoEventWireup="true" CodeFile="sitemap2.aspx.cs" Inherits="AspDotNetStorefrontAdmin.sitemap2" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI"%>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div id="content" style="height: 390px">
		<ComponentArt:SiteMap id="SiteMap1" runat="server" LeafNodeCssClass="SiteMapLeafNode" ParentNodeCssClass="SiteMapParentNode"
			RootNodeCssClass="SiteMapRootNode" CssClass="SiteMap" TreeLineImageHeight="20" TreeLineImageWidth="19" TreeLineImagesFolderUrl="images/lines2"
			TreeShowLines="true" SiteMapLayout="Tree" Height="1000" Width="100%">
			<table>
				<ComponentArt:SiteMapTableRow>
					<ComponentArt:SiteMapTableCell RootNodes="1" valign="top" width="20%"></ComponentArt:SiteMapTableCell>
					<ComponentArt:SiteMapTableCell RootNodes="1" valign="top" width="20%"></ComponentArt:SiteMapTableCell>
					<ComponentArt:SiteMapTableCell RootNodes="1" valign="top" width="20%"></ComponentArt:SiteMapTableCell>
					<ComponentArt:SiteMapTableCell RootNodes="1" valign="top" width="20%"></ComponentArt:SiteMapTableCell>
					<ComponentArt:SiteMapTableCell valign="top" width="20%"></ComponentArt:SiteMapTableCell>
				</ComponentArt:SiteMapTableRow>
			</table>
		</ComponentArt:SiteMap>
	</div>
</asp:Content>
