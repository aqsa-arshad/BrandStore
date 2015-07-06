<%@ Control Language="C#" AutoEventWireup="true" CodeFile="editcustomer.ascx.cs"
    Inherits="AspDotNetStorefrontControls.Admin_Controls_editcustomer" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls"
    TagPrefix="aspdnsf" %>
<%@ Register TagPrefix="aspdnsfs" TagName="CustomerToStore" Src="StoreSelector.ascx" %>
<%@ Register Src="addressedit.ascx" TagName="addressedit" TagPrefix="aspdnsf" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<asp:UpdatePanel ID="updCustomerEdit" runat="server" UpdateMode="Conditional">
    <%-- TRIGGERS --%>
    <%-- END TRIGGERS --%>
    <ContentTemplate>
        <asp:Panel ID="pnlCustomerEdit" runat="server">
            <%-- AJAX EXTENDERS --%>
            <ajax:ConfirmButtonExtender ID="cbeFailedTransactions" runat="server" TargetControlID="btnClearFailedTransactions"
                ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmFailedTransaction %>" />
            <ajax:ConfirmButtonExtender ID="cbeBlockIP" runat="server" TargetControlID="btnBlockIP"
                ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmBanIP %>" />
            <ajax:ConfirmButtonExtender ID="cbeManualPassword" runat="server" TargetControlID="btnManualPassword"
                ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmManualPassword %>" />
            <ajax:ConfirmButtonExtender ID="cbeRandomPassword" runat="server" TargetControlID="btnRandomPassword"
                ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmRandomPassword %>" />
            <ajax:ConfirmButtonExtender ID="cbeClearSession" runat="server" TargetControlID="btnClearSession"
                ConfirmText="<%$ Tokens:StringResource, admin.customer.confirmClearSession %>" />
            <%-- END AJAX EXTENDERS --%>
            <%-- CONTROL CONTENTS --%>
            <div id="divWrapper" runat="server" style="border:solid 1px white;background:white;">
                <table width="700px" align="center" cellpadding="0" cellspacing="0">
                    <tr>
                        <asp:Panel ID="pnlMessage" runat="server">
                            <asp:Literal ID="ltlMessage" runat="server" />
                        </asp:Panel>
                    </tr>
                </table>
                <table id="tblDetails" runat="server" align="left" width="700px" cellpadding="0"
                    cellspacing="0">
                        <tr runat="server" id="trCustomerID">
                            <td align="right">
                                <asp:Literal ID="Literal1" runat="server" Text="<%$ Tokens:StringResource, admin.customer.CustomerID %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:Literal ID="ltlCustomerID" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr id="trCreatedOn" runat="server">
                            <td align="right">
                                <asp:Literal ID="Literal2" runat="server" Text="<%$ Tokens:StringResource, admin.customer.CreatedOn %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:Literal ID="ltlCreatedOn" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr id="trGuid" runat="server">
                            <td align="right">
                                <asp:Literal ID="Literal3" runat="server" Text="<%$ Tokens:StringResource, admin.customer.Guid %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:Literal ID="ltlCustomerGuid" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr id="trIP" runat="server">
                            <td align="right">
                                <asp:Literal ID="Literal4" runat="server" Text="<%$ Tokens:StringResource, admin.customer.IPAddress %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:Literal ID="ltlIPAddress" runat="server"></asp:Literal>
                                <asp:Button ID="btnBlockIP" OnClick="btnBlockIP_OnClick" runat="server" Text="Ban/Unban IP Button"
                                    class="normalButtons" CausesValidation="false" />
                            </td>
                        </tr>
                        <tr id="trFailedTransactions" runat="server">
                            <td align="right">
                                <asp:Literal ID="Literal5" runat="server" Text="<%$ Tokens:StringResource, admin.customer.FailedTransactions %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:Literal ID="ltlFailedTransactions" runat="server"></asp:Literal>
                                <asp:Button ID="btnClearFailedTransactions" OnClick="btnClearFailedTransactions_OnClick"
                                    runat="server" Text="<%$ Tokens:StringResource, admin.customer.ClearFailedTransactions %>"
                                    class="normalButtons" CausesValidation="false" />
                            </td>
                        </tr>
                        <tr runat="server" id="trClearSession">
                            <td align="right">
                                <asp:Literal ID="Literal6" runat="server" Text="<%$ Tokens:StringResource, admin.customer.CustomerSession %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:Button ID="btnClearSession" OnClick="btnClearSession_OnClick" runat="server"
                                    Text="<%$ Tokens:StringResource, admin.customer.ClearSession2 %>" class="normalButtons"
                                    CausesValidation="false" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:RequiredFieldValidator ID="rfvFirstName" runat="server" ControlToValidate="txtFirstName"
                                    ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
                                    ValidationGroup="vldMain" CssClass="ErrorMsg"></asp:RequiredFieldValidator>
                                <asp:Literal ID="Literal7" runat="server" Text="<%$ Tokens:StringResource, admin.customer.FirstName %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:TextBox ID="txtFirstName" runat="server" MaxLength="100"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:RequiredFieldValidator ID="rfvLastName" runat="server" ControlToValidate="txtLastName"
                                    ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
                                    ValidationGroup="vldMain" CssClass="ErrorMsg"></asp:RequiredFieldValidator>
                                <asp:Literal ID="Literal8" runat="server" Text="<%$ Tokens:StringResource, admin.customer.LastName %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:TextBox ID="txtLastName" runat="server" MaxLength="100"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal9" runat="server" Text="<%$ Tokens:StringResource, admin.customer.LocaleSetting %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:DropDownList ID="ddlCustomerLocaleSetting" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right" style="padding-left: 4px;">
                                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                                    ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
                                    ValidationGroup="vldMain" CssClass="ErrorMsg"></asp:RequiredFieldValidator>
                                <asp:Literal ID="Literal10" runat="server" Text="<%$ Tokens:StringResource, admin.customer.Email %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:TextBox ID="txtEmail" runat="server" MaxLength="100"></asp:TextBox>
								<aspdnsf:EmailValidator ID="rgxvEmail" runat="server" ErrorMessage="<%$ Tokens:StringResource, admin.customer.EmailValidationFailed %>"
                                    ControlToValidate="txtEmail" ValidationGroup="vldMain" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone"
                                    CssClass="ErrorMsg" ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
                                    ValidationGroup="vldMain"></asp:RequiredFieldValidator>
                                <asp:Literal ID="Literal17" runat="server" Text="<%$ Tokens:StringResource, admin.customer.Phone %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:TextBox ID="txtPhone" runat="server" MaxLength="20"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtManualPassword"
                                    ErrorMessage="<%$ Tokens:StringResource, admin.common.FieldRequiredToLeft %>"
                                    ValidationGroup="vldPassword" CssClass="ErrorMsg"></asp:RequiredFieldValidator>
                                <asp:Literal ID="ltlResetPasswordLabel" runat="server" Text="<%$ Tokens:StringResource, admin.customer.ResetPassword %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:Button ID="btnRandomPassword" OnClick="btnRandomPassword_OnClick" runat="server"
                                    Text="<%$ Tokens:StringResource, admin.customer.ResetToRandomPassword %>" class="normalButtons"
                                    CausesValidation="false" />
                                <asp:TextBox ID="txtManualPassword" runat="server"></asp:TextBox>
                                <asp:Button ID="btnManualPassword" OnClick="btnManualPassword_OnClick" runat="server"
                                    Text="<%$ Tokens:StringResource, admin.customer.ManuallySetPassword %>" class="normalButtons"
                                    ValidationGroup="vldPassword" CausesValidation="true" />
                                <asp:RegularExpressionValidator ID="rgxvPassword" runat="server" ErrorMessage="<%$ Tokens:StringResource, admin.customer.PasswordValidationFailed %>"
                                    ControlToValidate="txtManualPassword" ValidationGroup="vldPassword" ValidationExpression="\S{5,}" />
                            </td>
                        </tr>
                        <tr runat="server" id="trIsRegistered">
                            <td align="right">
                                <asp:Literal ID="Literal11" runat="server" Text="<%$ Tokens:StringResource, admin.customer.IsRegistered %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:Literal ID="ltlIsRegistered" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr runat="server" id="trAssignedToStore">
                            <td align="right">
                                <asp:Literal ID="Literal12" runat="server" Text="<%$ Tokens:StringResource, admin.customer.AssignedToStore %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                            <aspdnsfs:CustomerToStore runat="server" ShowText="false" SelectMode="SingleDropDown" ShowDefaultForAllStores="false"  ID="ssOne" />
<%--                                <asp:Literal ID="ltlCustomerStore" runat="server"></asp:Literal>--%>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal13" runat="server" Text="<%$ Tokens:StringResource, admin.customer.AccountLocked %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:CheckBox ID="chkAccountLocked" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal14" runat="server" Text="<%$ Tokens:StringResource, admin.customer.Over13 %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:CheckBox ID="chkOver13" runat="server" />
                            </td>
                        </tr>
                        <tr runat="server" id="trCanViewCC" visible="false">
                            <td align="right">
                                <asp:Literal ID="Literal15" runat="server" Text="<%$ Tokens:StringResource, admin.customer.CanViewCC %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:CheckBox ID="chkCanViewCC" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal16" runat="server" Text="<%$ Tokens:StringResource, admin.customer.SubscriptionExpires %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <%-- Wrapped date-picker in a div to fix IE8 rendering issue --%>
                                <div style="width: 250px">
                                    <telerik:RadDatePicker ID="txtSubscriptionExpires" runat="server"
                                        Style="z-index: 150000;">
                                        <Calendar UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False"
                                            ViewSelectorText="x">
                                        </Calendar>
                                        <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                    </telerik:RadDatePicker>
                                </div>
                            </td>
                        </tr>
                        <tr runat="server" id="trVATRegID">
                            <td align="right">
                                <asp:Literal ID="Literal18" runat="server" Text="<%$ Tokens:StringResource, admin.customer.VATRegistrationID %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:TextBox ID="txtVATRegID" runat="server" MaxLength="15"></asp:TextBox>
                                <asp:RegularExpressionValidator ID="rgxvVATRegID" runat="server" ErrorMessage="<%$ Tokens:StringResource, admin.customer.VATValidationFailed %>"
                                    ControlToValidate="txtVATRegID" ValidationExpression="\B|^[0-9a-zA-Z]{8,12}"
                                    ValidationGroup="vldMain" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal20" runat="server" Text="<%$ Tokens:StringResource, admin.customer.DateOfBirth %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:TextBox ID="txtDOB" runat="server" MaxLength="12"></asp:TextBox>
                            </td>
                        </tr>
                        <tr runat="server" visible="<%# AspDotNetStorefrontCore.AppLogic.MicropayIsEnabled() %>">
                          <td align="right" >
                            <asp:Literal ID="Literal29" runat="server" Text="<%$ Tokens:StringResource, account.aspx.11 %>" /> Balance:
                          </td>
                          <td align="left" style="padding-left: 4px;">
                            <asp:TextBox ID="txtMicroPay"  runat="server" />
                          </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal21" runat="server" Text="<%$ Tokens:StringResource, admin.customer.OKToEmail %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:CheckBox ID="chkOkToEmail" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right" style="padding-left: 4px;">
                                <asp:Literal ID="Literal22" runat="server" Text="<%$ Tokens:StringResource, admin.customer.CODAllowed %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:CheckBox ID="chkCODAllowed" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal23" runat="server" Text="<%$ Tokens:StringResource, admin.customer.NetTermsAllowed %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:CheckBox ID="chkNetTermsAllowed" runat="server" />
                            </td>
                        </tr>
                        <tr runat="server" id="trCustomerLevel">
                            <td align="right">
                                <asp:Literal ID="Literal24" runat="server" Text="<%$ Tokens:StringResource, admin.customer.CustomerLevel %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:DropDownList ID="ddlCustomerLevel" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal25" runat="server" Text="<%$ Tokens:StringResource, admin.customer.Affiliate %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:DropDownList ID="ddlCustomerAffiliate" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td align="right">
                                <asp:Literal ID="Literal26" runat="server" Text="<%$ Tokens:StringResource, admin.customer.Notes %>" />
                            </td>
                            <td align="left" style="padding-left: 4px;">
                                <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Width="450" Rows="10"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" align="center">
                                <br />
                                <asp:Button ID="btnSubmit" runat="server" Text="<%$ Tokens:StringResource, admin.customer.CreateNew %>"
                                    class="normalButtons" OnClick="btnSubmit_OnClick" CausesValidation="true" ValidationGroup="vldMain" />
                                <br />
                                <br />
                            </td>
                        </tr>
                        <tr align="center">
                            <td colspan="2" align="center">
                                <asp:Panel ID="pnlAddressEdit" runat="server">
                                    <asp:UpdatePanel runat="server" ID="updAddressTable" UpdateMode="Conditional">
                                        <ContentTemplate>
                                            <table runat="server" id="tblAddresses" width="100%" align="center"> 
                                                    <tr runat="server" id="trAddressesHeader" class="gridHeader">
                                                        <td runat="server" id="tdAddressesHeader" colspan="2" align="center">
                                                            <asp:Literal ID="Literal27" runat="server" Text="<%$ Tokens:StringResource, admin.customer.editcustomeraddresses %>" />
                                                        </td>
                                                    </tr>
                                                    <tr runat="server" id="trAddressesBody">
                                                        <td>
                                                            <%-- BILLING ADDRESS --%>
                                                            <table align="center">
                                                                    <tr>
                                                                        <td class="gridHeaderSmall">
                                                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.PrimaryBillingAddress %>" />
                                                                        </td>
                                                                        <td class="gridHeaderSmall">
                                                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.customer.PrimaryShippingAddress %>" />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td runat="server" id="tdBillingAddress">
                                                                            <asp:Literal ID="ltlBillingName" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlBillingCompany" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlBillingAddress1" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlBillingAddress2" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlBillingSuite" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlBillingCityStateZip" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlBillingCountry" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlBillingPhone" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlBillingEmail" runat="server"></asp:Literal>
                                                                        </td>
                                                                        <%-- SHIPPING ADDRESS --%>
                                                                        <td runat="server" id="tdShippingAddress">
                                                                            <asp:Literal ID="ltlShippingName" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlShippingCompany" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlShippingAddress1" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlShippingAddress2" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlShippingSuite" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlShippingCityStateZip" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlShippingCountry" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlShippingPhone" runat="server"></asp:Literal><br />
                                                                            <asp:Literal ID="ltlShippingEmail" runat="server"></asp:Literal>
                                                                        </td>
                                                                    </tr>
                                                            </table>
                                                        </td>
                                                    </tr>
                                            </table>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td colspan="2" align="center">
                                                    <br />
                                                    <br />
                                                    <div align="center">
                                                        <asp:Button runat="server" ID="btnViewEditAddresses" OnClick="btnViewEditAddresses_Click"
                                                            CssClass="normalButtons" Text="<%$ Tokens:StringResource, admin.editaddress.ViewEditAddresses %>" />
                                                    </div>
                                                    <br />
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </asp:Panel>
                            </td>
                        </tr>
                </table>
                <br clear="all" />
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlAddresses" Visible="false" runat="server">
            <table runat="server" id="Table1" width="100%" align="center">
                    <tr runat="server" id="tr1" class="gridHeader">
                        <td runat="server" id="td1" colspan="2" align="center">
                            <asp:Literal ID="Literal28" runat="server" Text="<%$ Tokens:StringResource, admin.customer.editcustomeraddresses %>" />
                        </td>
                    </tr>
            </table>
            <div align="center">
                <aspdnsf:addressedit ID="adrlCustomerAddresses" runat="server" />
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
