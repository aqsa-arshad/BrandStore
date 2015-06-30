<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BillingAddressUKEdit.ascx.cs"
    Inherits="VortxControls_BillingAddressUKEdit" %>
<asp:UpdatePanel runat="server" ID="UpdatePanelBillingAddressWrap" RenderMode="Block"
    UpdateMode="Conditional" ChildrenAsTriggers="false">
    <Triggers>
    </Triggers>
    <ContentTemplate>
        <div id="BillAddressTable" class="form billing-address-form" runat="server">
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillFirstName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.13") %>' />
                </label>

                <asp:TextBox runat="server" CssClass="form-control" ID="BillFirstName" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillFirstName"
                    ControlToValidate="BillFirstName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.27") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillLastName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.14") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillLastName" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillLastName"
                    ControlToValidate="BillLastName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.15") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelPhone" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.32") %>' /><span style="color: red">*</span>
                </label>
				<a runat="server" onclick="adnsf$('.why-phone-info').toggle();" class="why-required-label" title='<%# StringResourceProvider.GetString("smartcheckout.aspx.167") %>'>
					<asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.168") %>' />
				</a>
				<div class="why-phone-info notice-wrap">
					<asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.167") %>' />
				</div>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillPhone" MaxLength="25" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorPhone" ControlToValidate="BillPhone"
                    ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.33") %>' Display="Dynamic" EnableClientScript="true"
                    ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label ID="LabelBillAddress1" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.17") %>' />
                </label>

                <asp:TextBox ID="BillAddress1" CssClass="form-control" runat="server" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidatorBillAddress1"
                    runat="server" ControlToValidate="BillAddress1" Display="Dynamic"
                    EnableClientScript="true" Enabled="true"
                    ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.18") %>' Text="*"
                    ValidationGroup="VGBillpingAddress" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label ID="LabelBillAddress2" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.19") %>' />
                </label>
                <asp:TextBox ID="BillAddress2" CssClass="form-control" runat="server" />

            </div>
            <div class="form-group" id="PanelBillCityState" runat="server">
                <label>
                    <asp:Label runat="server" ID="LabelBillZip" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.29") %>' />
                </label>

                <asp:TextBox runat="server" CssClass="form-control" ID="BillZip" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredEmailAddress" ControlToValidate="BillZip"
                    ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.30") %>' Display="Dynamic" EnableClientScript="true"
                    ValidationGroup="VGBillpingAddress" Text="*" Enabled="true" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label ID="LabelBillCity" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.28") %>' />
                </label>

                <asp:TextBox ID="BillCity" CssClass="form-control" runat="server" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label ID="LabelBillCounty" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.31") %>' />
                </label>

                <asp:TextBox ID="BillCounty" CssClass="form-control" runat="server" />

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
