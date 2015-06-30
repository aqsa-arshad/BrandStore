<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BillingAddressChoice.ascx.cs" Inherits="OPCControls_Addresses_BillingAddressChoice" %>
<%@ Register Src="../Addresses/AddressBook.ascx" TagName="AddressBook" TagPrefix="uc1" %>
<%@ Register Src="../Addresses/BillingAddressNoZipEdit.ascx" TagName="BillingAddressNoZipEdit" TagPrefix="uc1" %>
<%@ Register Src="../Addresses/BillingAddressUKEdit.ascx" TagName="BillingAddressUKEdit" TagPrefix="uc1" %>
<%@ Register Src="../Addresses/BillingAddressStatic.ascx" TagName="BillingAddressStatic" TagPrefix="uc1" %>
<asp:UpdatePanel UpdateMode="Conditional" ChildrenAsTriggers="false" ID="UpdateBillingAddressChoice"
    runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="BillSameYes" />
        <asp:AsyncPostBackTrigger ControlID="BillSameNo" />
    </Triggers>
    <ContentTemplate>
        <asp:Panel runat="server" ID="PanelBillSame" Visible="true">
            <div class="page-row billing-same-row">
                <div>
                    <asp:Label runat="server" ID="LabelBillSamePrompt" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.43") %>' CssClass="OPEmphasize" />
                </div>
                <div>
                    <asp:RadioButton GroupName="BillSame" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.136") %>' ID="BillSameYes" runat="server" OnCheckedChanged="BillSameYes_CheckedChanged" AutoPostBack="true" />
                </div>
                <div>
                    <asp:RadioButton GroupName="BillSame" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.137") %>' ID="BillSameNo" runat="server" OnCheckedChanged="BillSameNo_CheckedChanged" AutoPostBack="true" />
                </div>
            </div>
        </asp:Panel>

        <asp:PlaceHolder ID="PHBillingAddressStatic" runat="server">
            <div class="page-row billing-row">
                <asp:Label runat="server" ID="BillAddressHeader" CssClass="OPEmphasize" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.116") %>' />
                <uc1:BillingAddressStatic ID="BillingAddressStaticViewControl" runat="server" Visible="false" />
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="PHBillingAddressEdit" runat="server">

            <asp:Panel runat="server" ID="PanelAddressBook" CssClass="modalAddressBookWindow">
                <div class="page-row address-book" id="BillingAddressBookBox">
                    <uc1:AddressBook ID="AddressBook1" runat="server" />

                    <asp:Button Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.134") %>' ID="ButtonAddressBookClose" runat="server" OnClientClick="return hideForm('BillingAddressBookBox');" />
                </div>
            </asp:Panel>

			<asp:Label runat="server" ID="Label1" CssClass="OPEmphasize">
				<asp:Literal runat="server"  Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.116") %>' />
			</asp:Label>
			&#32;
            <asp:LinkButton ID="HyperLinkBillingAddressBook" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.102") %>'
                Visible="true" OnClientClick="return showForm('BillingAddressBookBox','modal-address-background');" />
            <div class="billAddressWrap" id="BillAddressWrap" runat="server">
                <uc1:BillingAddressNoZipEdit ID="BillingAddressUSEditViewControl" runat="server" Visible="false" />
                <uc1:BillingAddressUKEdit ID="BillingAddressUKEditViewControl" runat="server" Visible="false" />
            </div>

        </asp:PlaceHolder>

    </ContentTemplate>
</asp:UpdatePanel>
