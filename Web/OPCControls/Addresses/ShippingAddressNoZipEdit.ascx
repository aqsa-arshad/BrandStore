<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShippingAddressNoZipEdit.ascx.cs" Inherits="VortxControls_ShippingAddressNoZipEdit" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls.Validators" Assembly="AspDotNetStorefrontControls" %>

<asp:UpdatePanel runat="server" ID="UpdatePanelShippingAddressWrap" RenderMode="Block" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ButtonSaveAddress" />
    </Triggers>
    <ContentTemplate>
        <div id="ShipAddressTable" class="form shipping-address-form" runat="server">
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipFirstName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.13") %>' /><span style="color: red">*</span>
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipFirstName" MaxLength="49" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorShipFirstName"
                    ControlToValidate="ShipFirstName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.27") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGShippingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipLastName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.14") %>' /><span style="color: red">*</span>
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipLastName" MaxLength="49" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorShipLastName"
                    ControlToValidate="ShipLastName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.15") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGShippingAddress" Text="*" Enabled="true" />
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
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipPhone" MaxLength="25" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorPhone" ControlToValidate="ShipPhone"
                    ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.33") %>' Display="Dynamic" EnableClientScript="true"
                    ValidationGroup="VGShippingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipCompany" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.16") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="Company" MaxLength="99" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipAddress1" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.17") %>' /><span style="color: red">*</span>
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipAddress1" MaxLength="99" />
                <asp:CustomValidator ID="Address1POBoxNotAllowed" EnableClientScript="false" runat="server"
                    Text="*" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.34") %>'
                    Display="Dynamic" ControlToValidate="ShipAddress1" ValidateEmptyText="false"
                    ValidationGroup="VGShippingAddress" Enabled="true" OnServerValidate="Address1POBoxNotAllowed_ServerValidate" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorShipAddress1"
                    ControlToValidate="ShipAddress1" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.35") %>'
                    Display="Dynamic" EnableClientScript="true" ValidationGroup="VGShippingAddress"
                    Text="*" Enabled="true" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipAddress2" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.19") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipAddress2" MaxLength="99" />
                <asp:CustomValidator ID="Address2POBoxNotAllowed" EnableClientScript="false" runat="server"
                    Text="*" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.34") %>'
                    Display="Dynamic" ControlToValidate="ShipAddress2" ValidateEmptyText="false"
                    ValidationGroup="VGShippingAddress" Enabled="true" OnServerValidate="Address1POBoxNotAllowed_ServerValidate" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipOtherCity" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.28") %>' /><span style="color: red">*</span>
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipOtherCity" Visible="true" MaxLength="99" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorShipOtherCity"
                    ControlToValidate="ShipOtherCity" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.26") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGShippingOtherAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipOtherCountry" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.24") %>' /><span style="color: red">*</span>
                </label>
                <asp:DropDownList runat="server" CssClass="form-control" ID="ShipOtherCountry" Visible="true" AutoPostBack="true"
                    OnSelectedIndexChanged="ShipOtherCountry_OnChanged" OnDataBound="ShipOtherCountry_OnDataBound" />

            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipOtherState" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.25") %>' /><span style="color: red">*</span>
                </label>
                <asp:DropDownList runat="server" CssClass="form-control" ID="ShipOtherState" Visible="true" OnDataBound="ShipOtherState_OnDataBound" MaxLength="99" />

            </div>
            <div class="form-group" id="PanelZipCityState" runat="server">
                <label>
                    <asp:Label runat="server" ID="LabelShipZip" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.36") %>' /><span style="color: red">*</span>
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipZip" MaxLength="10" />
                <aspdnsf:ZipCodeValidator runat="server" ID="ZipCodeValidator" ControlToValidate="ShipZip" ValidationGroup="VGShippingAddress"
                    EnableClientScript="true" Display="Dynamic" Text="*" />

            </div>
            <div class="form-group" runat="server" id="PanelShipComments">
                <label>
                    <asp:Label runat="server" ID="LabelShipComments" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.37") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipComments" TextMode="MultiLine" Rows="2" />
                <div class="form-text">
                    <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.38") %>' />
                </div>
            </div>
            <div class="form-group" runat="server" id="PanelResidential">
                <label>
                    <asp:Label runat="server" ID="LabelCommercial" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.39") %>' />
                </label>
                <asp:CheckBox runat="server" ID="CheckBoxCommercial" Text="" />
                <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.40") %>' />
            </div>
        </div>
        <div class="next-step-wrap">
            <asp:Panel ID="PanelError" runat="server" CssClass="ErrorBox" Visible="false">
                <asp:Label runat="server" ID="LabelError" />
            </asp:Panel>
            <asp:ValidationSummary ID="VSShippingAddress" runat="server" CssClass="ErrorBox"
                EnableClientScript="true" DisplayMode="List" ValidationGroup="VGShippingAddress" />
            <asp:ValidationSummary ID="VSShppingOtherAddress" runat="server" CssClass="ErrorBox"
                EnableClientScript="true" DisplayMode="List" ValidationGroup="VGShippingOtherAddress" />
            <asp:Button ID="ButtonSaveAddress" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.118") %>' runat="server" OnClick="SaveAddress_Click"
                CssClass="button opc-button" />
            <div class="clear">
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
