<%@ Control Language="C#" AutoEventWireup="true" CodeFile="editAppConfigAtom.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.EditAppConfigAtom" %>
<%@ Register TagPrefix="aspdnsf" TagName="EditAppConfigInput" Src="EditAppConfigInput.ascx" %>

<% if (!this.HideTableNode)
   {%>
<table class='<%# this.CssClass %>'>
	<% } %>
	<tr class='tr<%# this.CssClass %>'>
		<td class="atomAppConfigEditCell">
			<div class="atomAppConfigEditWrap">
				<table width="100%">
					<tr>
						<td>
							<asp:Label runat="server" ID="lblTitleWrap" CssClass="configTitle">
								<b>
									<asp:Label ID="lblTitle" runat="server" />:</b>
							</asp:Label>
						</td>
						<td align="right">
							<a href="javascript:void(0);" onclick="ToggleAppConfigStores($(this).closest('.atomAppConfigEditCell'));" class="atomEditAppConfig" id="MoreStoreLink" runat="server">
								<asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.atom.moreStoreSettings %>" />
							</a>
						</td>
					</tr>
				</table>
				<div style="clear: both;"></div>
				<asp:Panel ID="pnlWrapper" runat="server" CssClass="atomMultiStoreConfigEdit">
					<table width="100%" class="storeAppConfigTable">
						<asp:Repeater ID="repVisibleStoreValues" runat="server" OnItemDataBound="StoreValues_ItemDataBound">
							<ItemTemplate>
								<tr class="storeVisibleRow" id="storeVisibleRow">
									<td class="storeExpandingLabel atomLabelCell">
										<span class="atomLabel">
											<asp:Literal ID="litStoreName" runat="server" />:
										</span>
									</td>
									<td>
										<div class="atomAppConfigInputWrap">
											<aspdnsf:EditAppConfigInput runat="server" ID="acStoreValue" />
											<asp:HiddenField ID="hfStoreId" runat="server" Value='<%# Eval("StoreID") %>' />
										</div>
									</td>
								</tr>
							</ItemTemplate>
						</asp:Repeater>
						<tr>
							<td class="storeExpandingLabel atomLabelCell">
								<span class="atomLabel">Default:</span>
							</td>
							<td class="atomAppConfigInputCell">
								<div class="atomAppConfigInputWrap">
									<aspdnsf:EditAppConfigInput runat="server" ID="acDefault" />
									<asp:HiddenField ID="hfAppConfigName" runat="server" />

								</div>
							</td>
						</tr>
						<asp:Repeater ID="repHiddenStoreValues" runat="server" OnItemDataBound="StoreValues_ItemDataBound">
							<ItemTemplate>
								<tr class="storeToggleRow" style="display: none;" id="storeToggleRow">
									<td class="storeExpandingLabel atomLabelCell">
										<span class="atomLabel">
											<asp:Literal ID="litStoreName" runat="server" />:
										</span>
									</td>
									<td>
										<div class="atomAppConfigInputWrap">
											<aspdnsf:EditAppConfigInput runat="server" ID="acStoreValue" />
											<asp:HiddenField ID="hfStoreId" runat="server" Value='<%# Eval("StoreID") %>' />
										</div>
									</td>
								</tr>
							</ItemTemplate>
						</asp:Repeater>
					</table>
				</asp:Panel>
				<%if (this.ShowSaveButton)
	  { %>
				<div class="atomSaveButtonCell">
					<asp:Button ID="btnSave" CssClass="defaultButton" runat="server" OnClick="btnSave_Click" Text="Save" />
				</div>
				<% } %>
				<div style="clear: both;"></div>
			</div>
		</td>
		<td class="atomInfoCell">
			<asp:Panel CssClass="atomDescriptionWrap" runat="server" ID="pnlDescription">
				<asp:Literal runat="server" ID="ltDescription" />
			</asp:Panel>
		</td>
	</tr>
	<% if (!this.HideTableNode)
	{ %>
</table>
<% } %>
