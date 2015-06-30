<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShippingAddressStatic.ascx.cs"
    Inherits="OPCControls_ShippingAddressStatic" %>
<asp:UpdatePanel runat="server" ID="UpdatePanelStaticAddress" UpdateMode="Conditional"
    ChildrenAsTriggers="false">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="EditAddress" />
    </Triggers>
    <ContentTemplate>
        <div id="staticAddressLayoutTable" class="page-row edit-address-row">
            <div class="one-fourth">
                <asp:Button ID="EditAddress" runat="server" Text="Edit" OnClick="EditAddress_Click"
                    CssClass="button opc-button" />
            </div>
            <div id="staticAddressWrap" class="three-fourths">
                <asp:Panel ID="PanelAddressLine1" runat="server">
                    <asp:Label runat="server" ID="FirstName" CssClass="addressName" />
                    <asp:Label runat="server" ID="LastName" CssClass="addressName" />
                    <asp:Panel ID="PanelPhone" CssClass="addressStaticLine" runat="server">
                        <asp:Label runat="server" ID="Phone" />
                    </asp:Panel>
                    <asp:Panel ID="PanelCompany" CssClass="addressStaticLine" runat="server">
                        <asp:Label runat="server" ID="Company" />
                    </asp:Panel>
                    <asp:Label runat="server" ID="Address1" />
                </asp:Panel>
                <asp:Panel ID="PanelAddressLine2" CssClass="addressStaticLine" runat="server">
                    <asp:Label runat="server" ID="Address2" />
                </asp:Panel>
                <asp:Panel ID="PanelApartment" CssClass="addressStaticLine" runat="server">
                    <asp:Label runat="server" ID="Apartment" />
                </asp:Panel>

                <asp:Panel ID="PanelCityStateZip" CssClass="addressStaticLine" runat="server">
                    <asp:Label runat="server" ID="City" />,
                            <asp:Label runat="server" ID="State" />
                    <asp:Label runat="server" ID="Zip" />
                </asp:Panel>
                <asp:Panel ID="PanelCountry" CssClass="addressStaticLine" runat="server">
                    <asp:Label runat="server" ID="Country" />
                </asp:Panel>

                <asp:Panel ID="PanelNotes" CssClass="addressStaticLine" runat="server">
                    <asp:Label runat="server" ID="Notes" />
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
