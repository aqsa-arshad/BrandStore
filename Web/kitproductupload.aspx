<%@ Page Language="C#" AutoEventWireup="true" CodeFile="kitproductupload.aspx.cs" Inherits="AspDotNetStorefront.kitproductupload" StylesheetTheme="" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
        <div align="center" >
            <asp:FileUpload ID="flpUpload" runat="server" />
            
            
            <asp:Button ID="btnUpload" runat="server" Text="Upload" 
                onclick="btnUpload_Click" />
            <asp:Literal ID="litScript" runat="server"></asp:Literal>
        </div>
    </form>
</body>
</html>
