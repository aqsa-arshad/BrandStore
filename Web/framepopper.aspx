<%@ Page language="c#" Inherits="AspDotNetStorefront.framepopper" CodeFile="framepopper.aspx.cs" EnableTheming="false" StylesheetTheme="" %>
<html>
    <head>
        <title>You are being redirected.</title>
    </head>
    <body>
        <form runat="server">
            <p>Please wait while you are being redirected. If you are not redirected in 5 seconds please 
            <asp:Literal ID="litLink" runat="server" />
            
            .</p>
        </form>
        <asp:Literal ID="litJScript" runat="server" />
    </body>
</html>
