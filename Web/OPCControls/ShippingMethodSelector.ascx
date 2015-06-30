<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShippingMethodSelector.ascx.cs"
    Inherits="VortxControls_ShippingMethodSelector" %>
<div>
    <asp:UpdatePanel runat="server" ID="UpdatePanelShipMethod" ChildrenAsTriggers="false"
        UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ShippingMethods" />
        </Triggers>
        <ContentTemplate>
            <asp:Panel ID="PanelError" runat="server" Visible="false">
                <asp:Label ID="LabelError" runat="server" />
            </asp:Panel>
            <asp:Panel ID="PanelFreeShippingThreshold" runat="server" Visible="false">
                <asp:Label ID="LblFreeShippingThreshold" runat="server" Text="" />
            </asp:Panel>
            <asp:Panel ID="PanelMultiShip" runat="server" Visible="false">
                <asp:Label ID="LabelMultiShip" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.139") %>' />
                <asp:HyperLink ID="HLMultiShip" runat="Server" NavigateUrl="~/checkoutshippingmult.aspx" ><asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.140") %>' /></asp:HyperLink>
            </asp:Panel>
            <div id="shippingMethodListWrap">
                <asp:RadioButtonList RepeatLayout="Flow" runat="server"  ID="ShippingMethods" AutoPostBack="true" OnSelectedIndexChanged="ShippingMethods_SelectedIndexChanged" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
