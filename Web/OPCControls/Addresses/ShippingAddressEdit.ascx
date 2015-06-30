<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShippingAddressEdit.ascx.cs" Inherits="VortxControls_ShippingAddressEdit" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls.Validators" Assembly="AspDotNetStorefrontControls" %>

<asp:UpdatePanel runat="server" ID="UpdatePanelShippingAddressWrap" RenderMode="Block"
    UpdateMode="Conditional" ChildrenAsTriggers="false">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ButtonSaveAddress" />
        <asp:AsyncPostBackTrigger ControlID="ShipZip" />
        <asp:AsyncPostBackTrigger ControlID="ShipZipCityState" />
        <asp:AsyncPostBackTrigger ControlID="ShipOtherCountry" />
    </Triggers>
    <ContentTemplate>
        <div id="ShipAddressTable" class="form shipping-address-form" runat="server">
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipFirstName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.13") %>' />*
                </label>
                <asp:TextBox runat="server" ID="ShipFirstName" CssClass="form-control" MaxLength="49" TabIndex="1" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorShipFirstName"
                    ControlToValidate="ShipFirstName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.27") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGShippingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipLastName" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.14") %>' />*
                </label>
                <asp:TextBox runat="server" ID="ShipLastName" CssClass="form-control" MaxLength="49" TabIndex="2" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorShipLastName"
                    ControlToValidate="ShipLastName" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.15") %>' Display="Dynamic"
                    EnableClientScript="true" ValidationGroup="VGShippingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelPhone" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.32") %>' />*
                </label>
				<a runat="server" onclick="adnsf$('.why-phone-info').toggle();" class="why-required-label" title='<%# StringResourceProvider.GetString("smartcheckout.aspx.167") %>'>
					<asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.168") %>' />
				</a>
				<div class="why-phone-info notice-wrap">
					<asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.167") %>' />
				</div>
                <asp:TextBox runat="server" ID="ShipPhone" CssClass="form-control" MaxLength="25" TabIndex="3" />
                <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorPhone" ControlToValidate="ShipPhone"
                    ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.33") %>' Display="Dynamic" EnableClientScript="true"
                    ValidationGroup="VGShippingAddress" Text="*" Enabled="true" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipCompany" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.16") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="Company" MaxLength="99" TabIndex="4" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label runat="server" ID="LabelShipAddress1" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.17") %>' />*
                </label>
                <asp:TextBox runat="server" ID="ShipAddress1" CssClass="form-control" MaxLength="99" TabIndex="5" />
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
                <asp:TextBox runat="server" ID="ShipAddress2" CssClass="form-control" MaxLength="99" TabIndex="6" />
                <asp:CustomValidator ID="Address2POBoxNotAllowed" EnableClientScript="false" runat="server"
                    Text="*" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.34") %>'
                    Display="Dynamic" ControlToValidate="ShipAddress2" ValidateEmptyText="false"
                    ValidationGroup="VGShippingAddress" Enabled="true" OnServerValidate="Address1POBoxNotAllowed_ServerValidate" />
            </div>
            <div class="form-group" id="PanelDynamicZip" runat="server">
                <label>
                    <asp:Label runat="server" ID="LabelShipZip" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.36") %>' />*
                </label>
                <asp:TextBox runat="server" ID="ShipZip" OnTextChanged="ShipZip_OnTextChanged" TabIndex="7" CssClass="form-control zip-code" MaxLength="10" ValidationGroup="VGShippingOtherAddress" />
                <aspdnsf:ZipCodeValidator runat="server" ID="ZipCodeValidator" ControlToValidate="ShipZip" ValidationGroup="VGShippingAddress"
                    EnableClientScript="true" Display="Dynamic" Text="*" />
                <asp:LinkButton runat="server" ID="LnkBtnNonUsAddress" TabIndex="8" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.156") %>' OnClick="LnkBtnNonUsAddress_Click" />
            </div>
            <div class="form-group" id="PanelDynamicCityAndState" runat="server">
                <label>
                    <asp:Label ID="Label1" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.22") %>' />
                </label>
                <asp:DropDownList runat="server" ID="ShipZipCityState" CssClass="form-control" TabIndex="9" Visible="true" AutoPostBack="true"
                    OnSelectedIndexChanged="ShipZipCityState_OnChanged" />
                <div class="form-text">
                    <asp:Label runat="server" ID="EnterZip" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.23") %>' />
                </div>
            </div>
            <div class="form-group" id="PanelOtherCityState" runat="server">
                <div class="form-group">
                    <label>
                        <asp:Label runat="server" ID="LabelShipOtherCity" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.28") %>' />*
                    </label>
                    <asp:TextBox runat="server" ID="ShipOtherCity" CssClass="form-control" TabIndex="10" Visible="true" MaxLength="99" ValidationGroup="VGShippingOtherAddress" />
                    <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidatorShipOtherCity"
                        ControlToValidate="ShipOtherCity" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.26") %>' Display="Dynamic"
                        EnableClientScript="true" ValidationGroup="VGShippingOtherAddress" Text="*" Enabled="true" />
                </div>
                <div class="form-group">
                    <label>
                        <asp:Label runat="server" ID="LabelShipOtherCountry" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.24") %>' />*
                    </label>
                    <asp:DropDownList runat="server" ID="ShipOtherCountry" CssClass="form-control" TabIndex="11" Visible="true" AutoPostBack="true" ValidationGroup="VGShippingOtherAddress"
                        OnSelectedIndexChanged="ShipOtherCountry_OnChanged" OnDataBound="ShipOtherCountry_OnDataBound" />
                </div>
                <div class="form-group">
                    <label>
                        <asp:Label runat="server" ID="LabelShipOtherState" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.25") %>' />*
                    </label>
                    <asp:DropDownList runat="server" CssClass="form-control" ID="ShipOtherState" TabIndex="12" Visible="true" OnDataBound="ShipOtherState_OnDataBound" ValidationGroup="VGShippingOtherAddress"
                        MaxLength="99" />
                </div>
            </div>
            <div class="form-group" runat="server" id="PanelShipComments">
                <label>
                    <asp:Label runat="server" ID="LabelShipComments" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.37") %>' />
                </label>
                <asp:TextBox runat="server" CssClass="form-control" ID="ShipComments" TabIndex="13" TextMode="MultiLine" Rows="2" />
                <div class="form-text">
                    <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.38") %>' />
                </div>
            </div>
            <div class="form-group" runat="server" id="PanelResidential">
                <label>
                    <asp:Label runat="server" ID="LabelCommercial" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.39") %>' />
                </label>
                <asp:CheckBox runat="server" ID="CheckBoxCommercial" TabIndex="14" Text="" />
                <div class="form-text">
                    <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.40") %>' />
                </div>
            </div>
        </div>
        <div class="next-step-wrap">
            <asp:Panel ID="PanelError" runat="server" CssClass="error-wrap" Visible="false">
                <asp:Label runat="server" ID="LabelError" CssClass="error-large" />
            </asp:Panel>
            <asp:ValidationSummary ID="VSShippingAddress" runat="server" CssClass="error-wrap"
                EnableClientScript="true" DisplayMode="List" ValidationGroup="VGShippingAddress" />
            <asp:ValidationSummary ID="VSShppingOtherAddress" runat="server" CssClass="error-wrap"
                EnableClientScript="true" DisplayMode="List" ValidationGroup="VGShippingOtherAddress" />
            <asp:Button ID="ButtonSaveAddress" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.117") %>' TabIndex="15" runat="server" OnClick="SaveAddress_Click"
                CssClass="button opc-button" />
            <div class="clear">
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
