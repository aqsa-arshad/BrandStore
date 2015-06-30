<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LoginPanel.ascx.cs" Inherits="LoginPanel" %>
<asp:UpdatePanel runat="server" ID="UpdatePanelLogin" RenderMode="Block" UpdateMode="Conditional"
    ChildrenAsTriggers="true">
    <Triggers>
        <asp:PostBackTrigger ControlID="btnPasswordSubmit" />
    </Triggers>
    <ContentTemplate>
        <asp:Panel runat="server" ID="PanelLoginWrap">
            <asp:Panel ID="PanelUsername" runat="server">
                <div class="form login-form">
                    <div class="form-group">
                        <label>
                            <asp:Label runat="server" ID="lblEmailAddressHeader" CssClass="checkout-header">
                            <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.80") %>' />
                            </asp:Label></label>
                        <asp:TextBox runat="server" ID="txtEmailAddress" CssClass="form-control" ValidationGroup="VGAccount" TabIndex="1" />
                        <asp:RequiredFieldValidator runat="server" ID="RFVEmailAddress" ControlToValidate="txtEmailAddress"
                            ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.81") %>' Display="Dynamic"
                            EnableClientScript="true" ValidationGroup="VGAccount" Text="*" Enabled="true" />
                        <aspdnsf:EmailValidator runat="server" ID="REEmailAddress" ControlToValidate="txtEmailAddress"
                            ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.82") %>' Display="Dynamic"
                            EnableClientScript="true" ValidationGroup="VGAccount" Text="*" Enabled="true" />

                        <asp:LinkButton runat="server" ID="linkSwitchUser" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.84") %>' OnClick="linkSwitchUser_Click" />
                        <div class="form-text">
                            <asp:Label ID="EmailHelperText" runat="server">
                                <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.83") %>' />
                            </asp:Label>
                        </div>
                    </div>
                    <div class="form-group" runat="server" id="trConfirmEmail">
                        <label>
                            <asp:Label runat="server" ID="Label1" CssClass="checkout-header">
                                <asp:Literal ID="lblConfirmEmail" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.confirmemail") %>' />
                            </asp:Label></label>
                        <asp:TextBox runat="server" ID="txtConfirmEmailAddress" CssClass="form-control" ValidationGroup="VGAccount" TabIndex="1" />
                        <asp:RequiredFieldValidator runat="server" ID="RFVConfirmEmailAddress" ControlToValidate="txtConfirmEmailAddress"
                            ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.81") %>' Display="Dynamic"
                            EnableClientScript="true" ValidationGroup="VGAccount" Text="*" Enabled="true" />
						<aspdnsf:EmailValidator ID="REConfirmEmailAddress" ControlToValidate="txtConfirmEmailAddress" 
							ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.82") %>' 
							ValidationGroup="VGAccount" Display="Dynamic" runat="server" Enabled="true" Text="*" />
                        <asp:CompareValidator ID="CmpConfirmEmailAddress" runat="server" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.emailsdontmatch") %>'
                            ControlToValidate="txtConfirmEmailAddress" ControlToCompare="txtEmailAddress" Display="Dynamic"
                            EnableClientScript="true" ValidationGroup="VGAccount" Text="*" Enabled="true"></asp:CompareValidator>
                    </div>
                </div>
                <div class="next-step-wrap">
                    <asp:Button runat="server" ID="btnEmailSubmit" CssClass="button opc-button" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.85") %>' OnClick="btnEmailSubmit_Click"
                        ValidationGroup="VGAccount" TabIndex="2" />
                </div>
            </asp:Panel>
            <asp:Panel ID="PanelPassword" runat="server" DefaultButton="btnPasswordSubmit">
                <div class="form login-form">
                    <div class="form-group">
                        <label>
                            <asp:Label CssClass="checkout-header" ID="lblPassword" runat="server">
                			<asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.86") %>' />
                            </asp:Label></label>
                        <asp:TextBox runat="server" ID="txtPassword" TextMode="Password" CssClass="form-control"
                            ValidationGroup="VGAccount" TabIndex="2" />
                    </div>
                    <div class="form-text">
                        <asp:LinkButton ID="ButtonForgotPassword" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.87") %>'
                            OnClick="ButtonForgotPassword_Click" />
                    </div>
                </div>
                <div class="next-step-wrap">
                    <asp:Panel CssClass="error-wrap" ID="LostPasswordResults" runat="server" Visible="false">
                        <asp:Label ID="LabelForgotPasswordResults" CssClass="error-large" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.88") %>' Visible="false" />
                    </asp:Panel>
                    <asp:Button runat="server" ID="btnPasswordSubmit" CssClass="button opc-button" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.138") %>' OnClick="btnPasswordSubmit_Click"
                        ValidationGroup="VGAccount" TabIndex="3" />
                    <asp:Button runat="server" ID="btnSkipLogin" CssClass="button opc-button" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.155") %>' OnClick="btnSkipLogin_Click" TabIndex="4" />
                    <div class="clear">
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="PanelCreateNewPassword" runat="server" Visible="false">
                <div class="form login-form">
                    <div class="form-group opc-checkout-header">
                        <asp:Label ID="LabelCreateNewPasswordFirstName" runat="server" />
                        <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.89") %>' />
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.90") %>' />
                        </label>
                        <asp:TextBox runat="server" ID="TextBoxNewPassword1" TextMode="Password" CssClass="form-control"
                            ValidationGroup="VGCreateNewPassword" />
                        <asp:RequiredFieldValidator runat="server" ID="RFVCreateNewPassword" ControlToValidate="TextBoxNewPassword1"
                            ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.74") %>' Display="Dynamic" EnableClientScript="true"
                            ValidationGroup="VGCreateNewPassword" Text="*" Enabled="true" />
                        <asp:RegularExpressionValidator ID="valPasswordMinLength" runat="server" ControlToValidate="TextBoxNewPassword1"
                            Display="Dynamic" EnableClientScript="true" Text="*" ValidationGroup="VGCreateNewPassword"
                            ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.75") %>' ValidationExpression=".{5}.*" />
                    </div>
                    <div class="form-group">
                        <label>Re-enter Password: </label>
                        <asp:TextBox runat="server" ID="TextBoxNewPassword2" TextMode="Password" CssClass="form-control"
                            ValidationGroup="VGCreateNewPassword" />
                        <asp:RequiredFieldValidator runat="server" ID="RFVCreateNewPasswordConfirm" ControlToValidate="TextBoxNewPassword2"
                            ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.78") %>' Display="Dynamic" EnableClientScript="true"
                            ValidationGroup="VGCreateNewPassword" Text="*" Enabled="true" />
                        <asp:CompareValidator ID="CompareValidatorCreateAccountPassword" runat="server" ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.79") %>'
                            Text="*" ControlToValidate="TextBoxNewPassword2" ControlToCompare="TextBoxNewPassword1"
                            Display="Dynamic" EnableClientScript="true" ValidationGroup="VGCreateNewPassword"></asp:CompareValidator>
                    </div>
                </div>
                <div class="next-step-wrap">
                    <asp:ValidationSummary ID="ValidationSummaryCreateNewPassword" runat="server" EnableClientScript="true"
                        DisplayMode="List" ValidationGroup="VGCreateNewPassword" CssClass="error-wrap" />
                    <asp:Panel CssClass="error-wrap" ID="PanelCreateNewPasswordResults" runat="server" Visible="false">
                        <asp:Label ID="LabelCreateNewPasswordResults" CssClass="error-large" runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.91") %>' Visible="false" />
                    </asp:Panel>
                    <asp:Button runat="server" ID="ButtonCreateNewPassword" CssClass="button opc-button" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.92") %>'
                        OnClick="ButtonCreateNewPassword_Click" ValidationGroup="VGCreateNewPassword" />
                    <div class="clear">
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="PanelError" runat="server" CssClass="error-wrap" Visible="true">
                <asp:Label runat="server" CssClass="error-large" ID="lblError" />
            </asp:Panel>
            <asp:Panel ID="PanelNoAccount" runat="server" CssClass="no-account" Visible="false">
                <asp:Label runat="server" ID="lblNoAccount">
                    <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.93") %>' />
                </asp:Label>
            </asp:Panel>
            <asp:ValidationSummary ID="VSAccount" runat="server" EnableClientScript="true" DisplayMode="List"
                ValidationGroup="VGAccount" CssClass="error-wrap" />
            <div class="clear">
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
