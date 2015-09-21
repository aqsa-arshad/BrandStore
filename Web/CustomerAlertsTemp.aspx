<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CustomerAlertsTemp.aspx.cs" Inherits="CustomerAlertsTemp" %>
<%@ Register TagPrefix="aspdnsf" TagName="CustomerAlerts" Src="~/controls/CustomerAlerts.ascx" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <aspdnsf:CustomerAlerts ID="UC_CustomerAlerts" runat="server" />
    </div>
    </form>
</body>
</html>
