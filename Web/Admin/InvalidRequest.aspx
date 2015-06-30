<%@ Page Language="C#" AutoEventWireup="true" CodeFile="InvalidRequest.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Admin_InvalidRequest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link rel="stylesheet" href="~/App_Themes/Admin_Default/StyleSheet.css" type="text/css" />
</head>
<body onload="self.focus()">
    <div style="height: 100%; width: 100%">
        <table style="height: 100%; width: 100%">
            <tr style="vertical-align: middle;">
                <td align="center">
                    <a href="systemlog.aspx" target="_blank">
                        <img style="border: none" src="~/App_Themes/Admin_Default/images/buttonalert.png"
                            runat="server" />
                    </a>
                    <p>
                        <b>
                            <asp:Label ID="Label2" runat="server" Text="<%$ Tokens:StringResource, admin.InvalidRequest.aspx.1 %>" /></b>
                    </p>
                </td>
            </tr>
        </table>
    </div>
</body>
</html>
