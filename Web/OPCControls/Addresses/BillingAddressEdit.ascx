<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BillingAddressEdit.ascx.cs"
    Inherits="VortxControls_BillingAddressEdit" %>
<asp:UpdatePanel runat="server" ID="UpdatePanelBillingAddressWrap" RenderMode="Block"
    UpdateMode="Conditional" ChildrenAsTriggers="false">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="BillZip" />
        <asp:AsyncPostBackTrigger ControlID="BillZipCityState" />
        <asp:AsyncPostBackTrigger ControlID="BillOtherCountry" />
    </Triggers>
    <ContentTemplate>
        <div id="BillAddressTable" class="form billing-address-form" runat="server">
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillFirstName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.13") %>' />*
                </label>
                <asp:TextBox runat="server" ID="BillFirstName" CssClass="form-control" MaxLength="49" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillFirstName"
                    ControlToValidate="BillFirstName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.27") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillLastName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.14") %>' />*
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillLastName" MaxLength="49" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillLastName"
                    ControlToValidate="BillLastName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.15") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelPhone" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.32") %>' />*
                </label>
                <asp:TextBox runat="server" ID="BillPhone" CssClass="form-control" MaxLength="25" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorPhone" ControlToValidate="BillPhone"
                    ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.33") %>' Display="Dynamic" EnableClientScript="true"
                    ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelCompany" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.16") %>' />*
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillCompany" MaxLength="99" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillAddress1" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.17") %>' />*
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillAddress1" MaxLength="99" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillAddress1"
                    ControlToValidate="BillAddress1" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.18") %>'
                    Display="Dynamic" EnableClientScript="true" ValidationGroup="VGBillingAddress"
                    Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillAddress2" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.19") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillAddress2" MaxLength="99" />
            </div>
            <div class="form-group" id="PanelDynamicZip" runat="server">
                <label class="addressLabelCell">
                    <asp:Label runat="server" ID="LabelBillZip" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.20") %>' />*
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillZip" OnTextChanged="BillZip_OnTextChanged" MaxLength="10" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredBillZip" ControlToValidate="BillZip"
                    ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.21") %>' Display="Dynamic" EnableClientScript="true"
                    ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group" id="PanelDynamicCityAndState" runat="server">
                <label>
                    <asp:Label ID="LabelCityAndState" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.22") %>' />
                </label>
                <asp:DropDownList runat="server" CssClass="form-control" ID="BillZipCityState" Visible="true" AutoPostBack="true"
                    OnSelectedIndexChanged="BillZipCityState_OnChanged" />
                <asp:Label runat="server" ID="EnterZip" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.23") %>' />
            </div>
            <div id="PanelOtherCityState" runat="server">
                <div class="form-group">
                    <label>
                        <asp:Label runat="server" ID="LabelBillOtherCountry" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.24") %>' />*
                    </label>
                    <asp:DropDownList runat="server" CssClass="form-control" ID="BillOtherCountry" Visible="true" AutoPostBack="true"
                        OnSelectedIndexChanged="BillOtherCountry_OnChanged" OnDataBound="BillOtherCountry_OnDataBound" />
                </div>
                <div class="form-group">
                    <label>
                        <asp:Label runat="server" ID="LabelBillOtherState" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.25") %>' />*
                    </label>
                    <asp:DropDownList runat="server" CssClass="form-control" ID="BillOtherState" Visible="true" OnDataBound="BillOtherState_OnDataBound" />
                </div>
                <div class="form-group">
                    <label>
                        <asp:Label runat="server" ID="LabelBillOtherCity" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.28") %>' />
                    </label>
                    <asp:TextBox runat="server" CssClass="form-control" ID="BillOtherCity" Visible="true" />
                    <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillOtherCity"
                        ControlToValidate="BillOtherCity" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.26") %>' Display="Dynamic"
                        EnableClientScript="true" ValidationGroup="VGBillingOtherAddress" Text="*" Enabled="true" />
                </div>
            </div>
        </div>
        <asp:ValidationSummary ID="VSBillingAddress" runat="server" EnableClientScript="true" CssClass="error-wrap"
            DisplayMode="List" ValidationGroup="VGBillingAddress" />
        <asp:ValidationSummary ID="VSBillingOtherAddress" runat="server" EnableClientScript="true" CssClass="error-wrap"
            DisplayMode="List" ValidationGroup="VGBillingOtherAddress" />
        <asp:Panel ID="PanelError" CssClass="error-wrap" runat="server" Visible="false">
            <asp:Label ID="LabelError" runat="server" CssClass="error-large" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
