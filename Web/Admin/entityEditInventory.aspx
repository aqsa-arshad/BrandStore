<%@ Page Language="c#" Theme="Admin_Default" Inherits="AspDotNetStorefrontAdmin.entityEditInventory" CodeFile="entityEditInventory.aspx.cs" %>

<html>
<head runat="server">
	<script type="text/javascript" src="./Scripts/FormValidate.js"></script>
	<title>Inventory Manager</title>
</head>
<body>
	<form id="Form1" runat="server">
		<div style="width: 800px; margin: 15px;">
			<div class="breadCrumb3">
				<div class="breadCrumbTitleText">
					<b>Status:</b> Editing Inventory For:
					<asp:HyperLink ID="lnkEditingInventoryFor" runat="server" />
				</div>
			</div>
			<div style="text-align: right; padding-bottom: 4px;">
				<asp:Button ID="Button1" CssClass="normalButtons" runat="server" Text="Save" OnClick="btnUpdate_Click" />
			</div>
			<div>
				<asp:GridView ID="grdInventory" Width="100%" runat="server" AutoGenerateColumns="false" GridLines="None" ShowFooter="true" CellPadding="4">
					<Columns>
						<asp:TemplateField HeaderText="Combinations">
							<ItemTemplate>
								<asp:HiddenField ID="hdnVariantId" runat="server" Value='<%# DataBinder.Eval(Container.DataItem, "VariantId") %>' />
								<asp:Literal ID="ltSize" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Size") %>' />,<asp:Literal ID="ltColor" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Color") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Inventory">
							<ItemTemplate>
								<asp:TextBox ID="txtInventory" runat="server" Columns="8" CausesValidation="true" Text='<%# DataBinder.Eval(Container.DataItem, "Inventory") %>' />
								<asp:RangeValidator ID="rvInventory" ControlToValidate="txtInventory" runat="server" MinimumValue="0" MaximumValue="99999999" ErrorMessage="*" />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="GTIN">
							<ItemTemplate>
								<asp:TextBox ID="txtGTIN" runat="server" MaxLength="14" Columns="14" Text='<%# DataBinder.Eval(Container.DataItem, "GTIN") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Warehouse Location">
							<ItemTemplate>
								<asp:TextBox ID="txtWarehouseLocation" runat="server" Columns="20" Text='<%# DataBinder.Eval(Container.DataItem, "WarehouseLocation") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="VendorId">
							<ItemTemplate>
								<asp:TextBox ID="txtVendorId" runat="server" Columns="8" Text='<%# DataBinder.Eval(Container.DataItem, "VendorId") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Full Vendor SKU">
							<ItemTemplate>
								<asp:TextBox ID="txtFullVendorSku" runat="server" Columns="14" Text='<%# DataBinder.Eval(Container.DataItem, "FullSku") %>' />
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField HeaderText="Weight Delta">
							<ItemTemplate>
								<asp:TextBox ID="txtWeightDelta" runat="server" Columns="8" Text='<%# DataBinder.Eval(Container.DataItem, "WeightDelta") %>' />
								<asp:RangeValidator ID="rvWeightDelta" ControlToValidate="txtWeightDelta" runat="server" MinimumValue="0" MaximumValue="99999999" ErrorMessage="*" />
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
					<AlternatingRowStyle BackColor="#C2DAFC" ForeColor="#00537E" />
					<RowStyle BackColor="#DFECFF" ForeColor="#00537E" />
					<HeaderStyle BackColor="#294167" ForeColor="#ffffff" Font-Bold="true" Font-Size="12px" />
					<FooterStyle BackColor="#294167" />
				</asp:GridView>
			</div>
			<div style="text-align: right; padding-top: 4px;">
				<asp:Button ID="btnUpdate" CssClass="normalButtons" runat="server" Text="Save" OnClick="btnUpdate_Click" />
			</div>
		</div>
	</form>
</body>
</html>
