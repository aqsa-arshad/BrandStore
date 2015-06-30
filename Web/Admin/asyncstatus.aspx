<%@ Page Language="C#" AutoEventWireup="true" CodeFile="asyncstatus.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Admin_asyncstatus" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div align="center">
        <asp:Image runat="server" ID="imgWaiting" ImageUrl="~/App_Themes/Admin_Default/images/waiting.gif" Height="8px" /><br />
        <asp:Literal runat="server" ID="ltlNumeric" /><br />
        <asp:Literal runat="server" ID="ltlPercent" /><br />
        <asp:Literal runat="server" ID="ltlEstRemaining" Text="<%$ Tokens:StringResource, admin.asyncstatus.EstimatedTimeRemaining %>" /> 
        <b><asp:Literal runat="server" ID="ltlTimeRemaining" /></b><br />
    </div>
    </form>
</body>
</html>
