<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddressBook.ascx.cs" Inherits="OPCControls_AddressBook" %>
<%@ Register Src="AddressStatic.ascx" TagName="AddressStatic" TagPrefix="uc1" %>
<%@ Import Namespace="Vortx.OnePageCheckout.Models" %>
<asp:Panel ID="PanelAddressBook" runat="server">
    <h2>
        <asp:Literal Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.12") %>' runat="server" />
    </h2>
</asp:Panel>
<asp:DataList ID="DataListAddressBook" runat="server" CellPadding="4" ForeColor="#333333"
	RepeatColumns="2" RepeatDirection="Horizontal" DataKeyField="AddressId" 
	RepeatLayout="Table" OnItemDataBound="DataListAddressBook_OnItemDataBound" 
	 OnItemCommand="DataListAddressBook_OnItemCommand">
	<FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
	<AlternatingItemStyle  CssClass="address-book-item-alt" />
	<ItemStyle CssClass="address-book-item" />
	<SelectedItemStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
	<HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
	<ItemTemplate>
		<uc1:AddressStatic id="AddressStaticItem" runat="server" />
		
		<asp:LinkButton ID="ButtonUseAddress" CssClass="useThisAddress"  runat="server" Text="Use this address" CommandName="UseSelected" CommandArgument='<%# ((IAddressModel)Container.DataItem).AddressId %>' />
	</ItemTemplate>
</asp:DataList>
