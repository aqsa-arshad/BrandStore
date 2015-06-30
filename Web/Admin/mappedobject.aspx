<%@ Page Language="C#" AutoEventWireup="true" CodeFile="mappedobject.aspx.cs" Inherits="AspDotNetStorefrontAdmin.MappedObjectPage" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityObjectMap" Src="controls/EntityObjectMap.ascx" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="~/App_Themes/Admin_Default/Stylesheet.css" type="text/css" />
    <script type="text/javascript">
        function mapAll(map) {
            var chks = document.getElementsByTagName('input');
            for (var ctr = 0; ctr < chks.length; ctr++) {
                var chk = chks[ctr];
                if (chk.className = 'map_check') {
                    chk.checked = map;
                }
            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="scrptMgr" runat="server" />
        <div id="pnlMain" runat="server" class="content_area">
            <aspdnsf:EntityObjectMap ID="ctrlMap" runat="server" />
        </div>
    </form>
</body>
</html>

