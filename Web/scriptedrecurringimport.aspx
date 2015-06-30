<%@ Page language="c#" Inherits="AspDotNetStorefront.scriptedrecurringimport" CodeFile="scriptedrecurringimport.aspx.cs" Theme="Admin_Default" EnableTheming="false" StylesheetTheme="" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ OutputCache  Duration="1"  Location="none" %>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Recurring Import</title>
</head>

<body>
    <form id="frmRecurringImport" runat="server">   
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <div id="help">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable">
            <tr>
                <td>
                    <div class="wrapper">
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable">
                            <tr>
                                <td class="contentTable">
                                    <font class="title">
                                        Import Recurring Order Status From Gateway
                                    </font>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <div id="content">
        <asp:Literal ID="litResults" runat="server"></asp:Literal>
    </div>
    </form>
</body>
</html>
