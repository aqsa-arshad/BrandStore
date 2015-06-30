<%@ Page Language="c#" Inherits="AspDotNetStorefront.signout" CodeFile="signout.aspx.cs" EnableTheming="false" StylesheetTheme="" %>
<html>
<head runat="server">
    <title>Signout</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <link rel="stylesheet" href="~/App_Themes/Skin_1/style.css" type="text/css">
</head>
<body bgcolor="#FFFFFF">
    <form runat="server" id="form1">
    <table width="100%" height="100%" cellpadding="0" cellspacing="0">
        <tr>
            <td width="100%" height="100%" align="center" valign="middle">
                <asp:Literal ID="Literal1" runat="server" Text="<%$ Tokens:StringResource,signout.aspx.2 %>" />
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
