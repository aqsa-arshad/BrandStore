<%@ Page Language="c#" Inherits="AspDotNetStorefront.SecureAuthMoneybookers" CodeFile="SecureAuthMoneybookers.aspx.cs" EnableTheming="false" StylesheetTheme="" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<html>
	<head>
		<title>Title for Page</title>
	</head>
	<body onload="OnLoadEvent();">
		<form id="frmSecurePost" action='<%= FormPostUrl %>' method="post">
			<asp:Literal runat="server" ID="litFormFields" Mode="PassThrough" />
		
			<noscript>
				
				
				<center>
					<h1><%= AppLogic.GetString("secureauth.aspx.2", 1, Localization.GetDefaultLocale()) %></h1>
					<h2><%= AppLogic.GetString("secureauth.aspx.3", 1, Localization.GetDefaultLocale()) %></h2>
					<h3><%= AppLogic.GetString("secureauth.aspx.4", 1, Localization.GetDefaultLocale()) %></h3>
					<input type="submit" value='<%= AppLogic.GetString("secureauth.aspx.5", 1, Localization.GetDefaultLocale()) %>' />
				</center>
			</noscript>
		</form>

		<script language="Javascript" type="text/javascript">
			<!--
			function OnLoadEvent() {
				document.getElementById('frmSecurePost').submit();
			}
			//-->
		</script>
	</body>
</html>