<%@ Page Language="C#" AutoEventWireup="true" CodeFile="mailingmgr.aspx.cs" Inherits="AspDotNetStorefrontAdmin.mailingmgr"
	MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="head">

	<script type="text/javascript">
		function EnableButtons() {
			setTimeout("document.getElementById('<%=btnSend.ClientID%>').disabled=false;", 60000);
			setTimeout("document.getElementById('<%=btnRemoveEmail.ClientID%>').disabled=false;", 60000);
		}
	</script>

</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<cc1:ConfirmButtonExtender ID="cbxSendMail" runat="server" TargetControlID="btnSend"
		ConfirmText="<%$ Tokens:StringResource, admin.mailingmgr.Warning %>" />
	<div runat="server" id="divStatus" style="margin: 5px;" align="center">
		<iframe runat="server" id="ifrStatus" width="95%" height="70px" frameborder="1" scrolling="no"
			visible="false" />
		<br />
		<asp:Literal ID="ltError" runat="server"></asp:Literal>
	</div>
	<asp:Panel ID="pnlSendMail" DefaultButton="btnSend" runat="server">
		<div id="co">
			<table border="0" cellpadding="0" cellspacing="0" class="outerTable" width="100%">
				<tr>
					<td>
						<p>
							<asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.mailingmgr.Info %>" />
						</p>
					</td>
				</tr>
			</table>
			<table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
				<tr>
					<td width="260" align="right" valign="top">
						<font class="subTitleSmall">*<asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.mailingmgr.Subject %>" />:</font>
					</td>
					<td align="left" valign="middle" class="style1">
						<asp:TextBox runat="server" ID="txtSubject" Width="294px"></asp:TextBox>
					</td>
				</tr>

				<tr id="trMessageBox" runat="server">
					<td align="right" valign="top">
						<font class="subTitleSmall">
							<asp:Literal ID="Literal9" runat="server" Text="<%$Tokens:StringResource, admin.systemlog.Message %>" /></font>
					</td>
					<td align="left" valign="top" class="style1">
						<asp:Literal ID="ltMessage" runat="server"></asp:Literal>
						<div>
							<telerik:RadEditor runat="server" ID="radDescription">
								<ImageManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
								<DocumentManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
								<FlashManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
								<MediaManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
								<SilverlightManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
								<TemplateManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
							</telerik:RadEditor>
						</div>
					</td>
				</tr>
				<tr id="trMessageFooter" runat="server">
					<td align="right" valign="top">
						<font class="subTitleSmall">
							<asp:Literal ID="Literal10" runat="server" Text="<%$Tokens:StringResource, admin.mailingmgr.MessageFooter %>" /></font>
					</td>
					<td align="left" valign="middle" class="style1">
						<asp:Literal ID="Literal11" runat="server" Text="<%$Tokens:Topic, MailFooter %>" />
					</td>
				</tr>
				<tr>
					<td>&nbsp;
					</td>
					<td class="style1">
						<asp:Label ID="lblFooter" runat="server"></asp:Label>
					</td>
				</tr>
				<tr>
					<td colspan="2">&nbsp;
					</td>
				</tr>
				<tr id="trCustomersWithOrder" runat="server">
					<td>&nbsp;
					</td>
					<td>
						<asp:CheckBox ID="chkCustomersWithOrdersOnly" runat="server" Text="<%$Tokens:StringResource, admin.mailingmgr.CustomersWithOrders %>" />
					</td>
				</tr>
				<tr>
					<td colspan="2">&nbsp;
					</td>
				</tr>
				<tr>
					<td>&nbsp;
					</td>
					<td>
						<asp:RadioButtonList runat="server" ID="rbCustomerListType" RepeatDirection="Horizontal" RepeatLayout="Table">
							<asp:ListItem Text="<%$Tokens:StringResource, admin.mailingmgr.newslettersemails %>" Value="Newsletter" Selected="True" />
							<asp:ListItem Text="<%$Tokens:StringResource, admin.mailingmgr.customeremails %>" Value="Customers" />
						</asp:RadioButtonList>
					</td>
				</tr>
				<tr>
					<td colspan="2">&nbsp;
					</td>
				</tr>
				<tr>
					<td>&nbsp;
					</td>
					<td>
						<asp:Button ID="btnSend" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.mailingmgr.SendEMail %>"
							OnClick="btnSend_Click" />
						<asp:Button ID="btnExport" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.mailingmgr.ExportEMail %>"
							OnClick="btnExport_Click" />
						<asp:Button ID="btnSendTestEmailToAdmin" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.mailingmgr.SendTestEmailToAdmin %>"
							OnClick="btnSendTestEmailToAdmin_Click" />
					</td>
				</tr>
			</table>
		</div>
	</asp:Panel>
	<br />
	<hr />
	<asp:Panel ID="pnlRemove" DefaultButton="btnRemoveEmail" runat="server">
		<table class="style2">
			<tr>
				<td>
					<b>
						<asp:Literal ID="Literal12" runat="server" Text="<%$Tokens:StringResource, admin.mailingmgr.UseForm %>" /></b>
				</td>
			</tr>
			<tr>
				<td>
					<table class="style2">
						<tr>
							<td class="style3">
								<asp:Literal ID="Literal13" runat="server" Text="<%$Tokens:StringResource, admin.mailingmgr.RemoveCustomerEmail %>" />:
							</td>
							<td class="style4">
								<asp:TextBox ID="txtRemoveEmail" runat="server" Width="400px"></asp:TextBox>
							</td>
							<td class="style5">
								<asp:Button ID="btnRemoveEmail" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.mailingmgr.RemoveEmail %>"
									OnClick="btnRemoveEmail_Click" />
							</td>
							<td>&nbsp;
							</td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
	</asp:Panel>

	<script type="text/javascript">
		EnableButtons();
	</script>

</asp:Content>
