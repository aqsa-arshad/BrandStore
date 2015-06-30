<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BillingAddressNoZipEdit.ascx.cs" Inherits="VortxControls_BillingAddressNoZipEdit" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls.Validators" Assembly="AspDotNetStorefrontControls" %>

<asp:UpdatePanel runat="server" ID="UpdatePanelBillingAddressWrap" RenderMode="Block"
    UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>
        <div id="BillAddressTable" class="form billing-address-form" runat="server">
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillFirstName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.13") %>' /><span style="color: red">*</span>
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillFirstName" MaxLength="49" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillFirstName"
                    ControlToValidate="BillFirstName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.27") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillLastName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.14") %>' /><span style="color: red">*</span>
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillLastName" MaxLength="49" />
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
                    <asp:Label runat="server" ID="LabelCompany" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.16") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillCompany" MaxLength="99" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillAddress1" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.17") %>' /><span style="color: red">*</span>
                </label>

                <asp:TextBox runat="server" CssClass="form-control" ID="BillAddress1" MaxLength="99" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillAddress1"
                    ControlToValidate="BillAddress1" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.18") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGBillingAddress" Text="*" Enabled="true" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillAddress2" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.19") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillAddress2" MaxLength="99" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillOtherCity" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.28") %>' /><span style="color: red">*</span>
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="BillOtherCity" Visible="true" MaxLength="99" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorBillOtherCity"
                    ControlToValidate="BillOtherCity" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.26") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGBillingOtherAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillOtherCountry" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.24") %>' /><span style="color: red">*</span>
                </label>

                <asp:DropDownList runat="server" CssClass="form-control" ID="BillOtherCountry" Visible="true" AutoPostBack="true"
                    OnSelectedIndexChanged="BillOtherCountry_OnChanged" OnDataBound="BillOtherCountry_OnDataBound" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelBillOtherState" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.25") %>' /><span style="color: red">*</span>
                </label>

                <asp:DropDownList runat="server" CssClass="form-control" ID="BillOtherState" Visible="true" OnDataBound="BillOtherState_OnDataBound" />

            </div>
            <div class="form-group" id="PanelZipCityState" runat="server">
                <label>
                    <asp:Label runat="server" ID="LabelBillZip" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.20") %>' /><span style="color: red">*</span>
                </label>

                <asp:TextBox runat="server" CssClass="form-control" ID="BillZip" MaxLength="10" />
                <aspdnsf:ZipCodeValidator runat="server" ID="ZipCodeValidator" ControlToValidate="BillZip" ValidationGroup="VGBillingOtherAddress"
                    EnableClientScript="true" Display="Dynamic" Text="*" />

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
