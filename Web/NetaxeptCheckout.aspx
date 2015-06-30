<%@ Page Language="C#" AutoEventWireup="true" CodeFile="NetaxeptCheckout.aspx.cs" Inherits="AspDotNetStorefront.NetaxeptCheckout" EnableTheming="false" StylesheetTheme="" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body onload ="document.frmNetaxept.submit()">
    <form id="frmNetaxept" name="frmNetaxept" runat="server"  >
    <div align="center">
        <asp:Image ID="loadingshipping" AlternateText="" runat="server" ImageAlign="AbsMiddle" />
    </div>

   <div style="visibility: hidden">
    <asp:Button ID="btnNetaxept" runat="server" Text="Button"/>
   </div>
  
    <asp:Literal ID="ltlNexaxept" runat="server"></asp:Literal>
    </form>
</body>
</html>
