<%@ Page Language="C#" AutoEventWireup="true" CodeFile="phonesitemap.aspx.cs" Inherits="AspDotNetStorefrontAdmin.phonesitemap" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <link rel="Stylesheet" type="text/css" href="~/App_Themes/Admin_Default/style.css" />
</head>
<body>
<form id="form1" method="post" runat="server">
    <div id="content" style="height: 390px">
		<ComponentArt:SiteMap id="SiteMap1" runat="server" LeafNodeCssClass="SiteMapLeafNode" ParentNodeCssClass="SiteMapParentNode"
			RootNodeCssClass="SiteMapRootNode" CssClass="SiteMap" TreeLineImageHeight="20" TreeLineImageWidth="19" TreeLineImagesFolderUrl="~/App_Themes/Admin_Default/images/lines2"
			TreeShowLines="true" SiteMapLayout="Tree" Height="1000" Width="100%">
			<table>
				<ComponentArt:SiteMapTableRow>
					<ComponentArt:SiteMapTableCell RootNodes="1" valign="top" width="33%"></ComponentArt:SiteMapTableCell>
					<ComponentArt:SiteMapTableCell RootNodes="1" valign="top" width="33%"></ComponentArt:SiteMapTableCell>
					<ComponentArt:SiteMapTableCell RootNodes="1" valign="top" width="34%"></ComponentArt:SiteMapTableCell>
				</ComponentArt:SiteMapTableRow>
			</table>
		</ComponentArt:SiteMap>
	</div>
</form>
</body>
</html>
