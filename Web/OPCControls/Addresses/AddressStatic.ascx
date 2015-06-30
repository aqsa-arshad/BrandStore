<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddressStatic.ascx.cs"
	Inherits="OPCControls_AddressStatic" %>
<asp:UpdatePanel runat="server" ID="UpdatePanelStaticAddress" UpdateMode="Conditional"
	ChildrenAsTriggers="false">
	<ContentTemplate>		
		<asp:Panel ID="PanelAddressLine1" CssClass="addressStaticLine" runat="server">
			<asp:Label runat="server" ID="FirstName" /> <asp:Label runat="server" ID="LastName" />
			<asp:Label runat="server" ID="Address1" /></asp:Panel>
		<asp:Panel ID="PanelAddressLine2" CssClass="addressStaticLine" runat="server">
			<asp:Label runat="server" ID="Address2" /></asp:Panel>
		<asp:Panel ID="PanelApartment" CssClass="addressStaticLine" runat="server">
			<asp:Label runat="server" ID="Apartment" /></asp:Panel>
		<asp:Panel ID="PanelPhone" CssClass="addressStaticLine" runat="server">
			<asp:Label runat="server" ID="Phone" /></asp:Panel>
		<asp:Panel ID="PanelCityStateZip" CssClass="addressStaticLine" runat="server">
			<asp:Label runat="server" ID="City" />,
			<asp:Label runat="server" ID="State" />
			<asp:Label runat="server" ID="Zip" /></asp:Panel>
		<asp:Panel ID="PanelCountry" CssClass="addressStaticLine" runat="server">
			<asp:Label runat="server" ID="Country" /></asp:Panel>
	</ContentTemplate>
</asp:UpdatePanel>
