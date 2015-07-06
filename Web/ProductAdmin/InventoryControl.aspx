<%@ Page Language="C#" AutoEventWireup="true" CodeFile="InventoryControl.aspx.cs" Inherits="AspDotNetStorefrontAdmin._InventoryControl" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>
<%@ Register TagPrefix="aspdnsf" TagName="AppConfigInfo" Src="controls/appconfiginfo.ascx" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div id="container">
		<table cellpadding="0" cellspacing="0" style="width: 100%; border-style: none; padding: 20px;">
			<tr>
				<td style="font-weight: bold;">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.wizard.appconfigwarning %>" />
				</td>
			</tr>
			<tr>
				<td id="tdLocale" runat="server" height="25px" align="right" valign="middle">
					<span class="subTitle">
						<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Locale %>" />
					</span>
					<asp:DropDownList ID="ddlLocale" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLocale_SelectedIndexChanged" />
				</td>
			</tr>
			<tr>
				<td align="left" height="25px" valign="middle">
					<asp:CheckBox ID="chkLimitCartToQuantityOnHand" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.LimitCartInQuantityInHand %>" />
					<aspdnsf:AppConfigInfo AppConfigName="Inventory.LimitCartToQuantityOnHand" runat="server" />
				</td>
			</tr>
			<tr>
				<td align="left" height="25px" valign="middle">
					<asp:CheckBox ID="chkShowInventoryTable" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ShowInventoryTable %>" />
					<aspdnsf:AppConfigInfo AppConfigName="ShowInventoryTable" runat="server" />
				</td>
			</tr>
			<tr>
				<td>
					<hr style="width: 80%; float: left;" />
				</td>
			</tr>
			<tr>
				<td align="left" valign="middle" style="height: 25px">
					<asp:Label runat="server" AssociatedControlID="txtHideProductsWithLessThanThisInventoryLevel" Text="<% $Tokens:StringResource, admin.InventoryControl.HideProductsWithLessThanThisInventoryLevel %>" />
					<asp:TextBox ID="txtHideProductsWithLessThanThisInventoryLevel" runat="server" MaxLength="4" />
					<aspdnsf:AppConfigInfo AppConfigName="HideProductsWithLessThanThisInventoryLevel" runat="server" />
					
					<asp:CompareValidator runat="server" ErrorMessage="<%$Tokens:StringResource, admin.editquantitydiscounttable.EnterInteger %>" ControlToValidate="txtHideProductsWithLessThanThisInventoryLevel" Operator="DataTypeCheck" Type="Integer" />
					<asp:RequiredFieldValidator runat="server" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.InventoryControl.EnterGreaterThan0 %>" ControlToValidate="txtHideProductsWithLessThanThisInventoryLevel" />
				</td>
			</tr>
			<tr>
				<td align="left" height="25px" valign="middle">
					<asp:CheckBox ID="chkProductPageOutOfStockRedirect" runat="server"  Text="<% $Tokens:StringResource, admin.InventoryControl.ProductPageOutOfStockRedirect %>"/>
					<aspdnsf:AppConfigInfo AppConfigName="ProductPageOutOfStockRedirect" runat="server" />
				</td>
			</tr>
			<tr>
				<td align="left" height="25px" valign="middle">
					<asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.OutOfStockMessageOptions %>" />
				</td>
			</tr>
			<tr>
				<td>
					<div class="DisplayOutOfStockGroup">
						<table>
							<tr>
								<td align="left" valign="middle">
									<asp:Label runat="server" AssociatedControlID="chkDisplayOutOfStockMessage" Text="<% $Tokens:StringResource, admin.InventoryControl.DisplayOutOfStockProducts %>" />
								</td>
								<td align="left" valign="middle">
									<asp:CheckBox ID="chkDisplayOutOfStockMessage" runat="server" />
									<aspdnsf:AppConfigInfo AppConfigName="DisplayOutOfStockProducts" runat="server" />
								</td>
							</tr>
							<tr>
								<td align="left" valign="middle">
									<asp:Label ID="lblOutOfStockThreshold" runat="server" Text="<% $Tokens:StringResource, admin.InventoryControl.OutOfStockThreshold %>" />
								</td>
								<td align="left" valign="middle">
									<asp:TextBox ID="txtOutOfStockThreshold" runat="server" MaxLength="4" />
									<aspdnsf:AppConfigInfo AppConfigName="OutOfStockThreshold" runat="server" />
									<asp:CompareValidator runat="server" ControlToValidate="txtOutOfStockThreshold" CultureInvariantValues="True" ErrorMessage="<%$Tokens:StringResource, admin.InventoryControl.InputNumber %>" Display="Dynamic" Operator="DataTypeCheck" Type="Integer" />
									<asp:RequiredFieldValidator runat="server" Display="Dynamic" ErrorMessage="<%$Tokens:StringResource, admin.InventoryControl.InputNumber %>" ControlToValidate="txtOutOfStockThreshold" />
								</td>
							</tr>
							<tr>
								<td>
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ProductOutOfStockMessage %>" />
								</td>
								<td align="left" valign="middle">
									<asp:TextBox ID="txtProductOutOfStockMessage" runat="server" />
									<asp:Image runat="server" ID="imgDisplayOFSOnProductPage" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource, admin.InventoryControl.Tooltip.ProductOutOfStockMessage %>" />
								</td>
							</tr>
							<tr>
								<td align="left" valign="middle">
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.EntityOutOfStockMessage %>" />
								</td>
								<td align="left" valign="middle">
									<asp:TextBox ID="txtEntityOutOfStockMessage" runat="server" />
									<asp:Image runat="server" ID="imgDisplayOFSOnEntityPage" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource, admin.InventoryControl.Tooltip.EntityOutOfStockMessage %>" />
								</td>
							</tr>
							<tr>
								<td align="left" valign="middle">
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ProductInStockMessage %>" />
								</td>
								<td align="left" valign="middle">
									<asp:TextBox ID="txtProductInStockMessage" runat="server" />
									<asp:Image runat="server" ID="imgDisplayISOnEntityPage" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource, admin.InventoryControl.Tooltip.ProductInStockMessage %>" />
								</td>
							</tr>
							<tr>
								<td align="left" valign="middle">
									<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.EntityInStockMessage %>" />
								</td>
								<td align="left" valign="middle">
									<asp:TextBox ID="txtEntityInStockMessage" runat="server" />
									<asp:Image runat="server" ID="imgDisplayISOnProductPage" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource, admin.InventoryControl.Tooltip.EntityInStockMessage %>" />
								</td>
							</tr>
							<tr>
								<td align="left" valign="middle">
									<asp:CheckBox ID="chkShowOutOfStockMessageOnProductPages" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ShowInProductPages %>" />
									<aspdnsf:AppConfigInfo AppConfigName="DisplayOutOfStockOnProductPages" runat="server" />
								</td>
								<td align="left" valign="middle">
								</td>
							</tr>
							<tr>
								<td align="left" valign="middle">
									<asp:CheckBox ID="chkShowOutOfStockMessageOnEntityPages" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.ShowInEntityPages %>" />
									<aspdnsf:AppConfigInfo AppConfigName="DisplayOutOfStockOnEntityPages" runat="server" />
								</td>
								<td align="left" valign="middle">
								</td>
							</tr>
						</table>
					</div>
				</td>
			</tr>
			<tr>
				<td>
					<hr style="width: 80%; float: left;" />
				</td>
			</tr>
			<tr>
				<td>
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.KitInventoryOptions %>" />
					<div class="DisplayOutOfStockGroup">
						<asp:CheckBox ID="chkEnableKitItemStockHints" runat="server" Text="<%$Tokens:StringResource, admin.InventoryControl.Kits.EnableStockHints %>" />
						<aspdnsf:AppConfigInfo AppConfigName="KitInventory.ShowStockHint" runat="server" />
						<br />
						<asp:RadioButton ID="rbDisableOutOfStockKitItem" runat="server" GroupName="KitDisableOption" Text="<%$Tokens:StringResource, admin.InventoryControl.Kits.DisableOutOfStockItemSelection %>" />
						<aspdnsf:AppConfigInfo AppConfigName="KitInventory.DisableItemSelection" runat="server" />
						<br />
						<asp:RadioButton ID="rbHideOutOfStockKitItems" runat="server" GroupName="KitDisableOption" Text="<%$Tokens:StringResource, admin.InventoryControl.Kits.HideOutOfStockOptions %>" />
						<aspdnsf:AppConfigInfo AppConfigName="KitInventory.HideOutOfStock" runat="server" />
						<br />
						<asp:RadioButton ID="rbNoInventoryControl" runat="server" GroupName="KitDisableOption" Text="<%$Tokens:StringResource, admin.common.None %>" />
						<br />
					</div>
				</td>
			</tr>
			<tr>
				<td>
					<hr style="width: 80%; float: left;" />
				</td>
			</tr>
			<tr>
				<td align="left" height="25px" valign="middle">
					<asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.InventoryControl.Update %>" />
				</td>
			</tr>
		</table>
	</div>
</asp:Content>
