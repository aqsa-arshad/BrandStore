<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.phonesearch" CodeFile="phonesearch.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link rel="stylesheet" href="~/App_Themes/Admin_Default/StyleSheet.css" type="text/css" />
    <script type="text/javascript" src="jscripts/formValidate.js"></script>
    <script type="text/javascript">
        function SearchForm2_Validator(theForm) {
            if (theForm.SearchTerm.value.length < 3) {
                alert('Please enter at least 3 characters in the Search For field.');
                theForm.SearchTerm.focus();
                return (false);
            }
            return (true);
        }
    </script>
</head>
<body onload="self.focus()">
    <asp:Literal ID="ltContent" runat="server" />
</body>
</html>
