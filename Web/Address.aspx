<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Address.aspx.cs" Inherits="AspDotNetStorefront._Address"
    MasterPageFile="~/App_Templates/Skin_1/template.master" MaintainScrollPositionOnPostback="false" %>

<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap create-account-page">
            <h1>
                <asp:Literal ID="Literal5" Text="<%$ Tokens:StringResource,Header.AddressBook %>" runat="server" />
            </h1>

            <asp:Label ID="ErrorMsgLabel" CssClass="errorLg" runat="Server" Visible="false" />

            <asp:Label ID="lblPOBoxError" Visible="false" runat="server" ForeColor="Red" Font-Bold="true" />
            <asp:ValidationSummary DisplayMode="List" ID="vsEditAddress" ShowMessageBox="false"
                runat="server" ShowSummary="true" ValidationGroup="EditAddress" ForeColor="red"
                Font-Bold="true" />

            <asp:Panel ID="pnlCCTypeErrorMsg" CssClass="error-wrap" runat="server" Visible="false">
                <asp:Label ID="CCTypeErrorMsgLabel" CssClass="error-large" runat="server"
                    Text="<%$ Tokens:StringResource, address.cs.19 %>"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlCCExpMonthErrorMsg" CssClass="error-wrap" runat="server" Visible="false">
                <asp:Label ID="CCExpMonthErrorMsg" CssClass="error-large" runat="server"
                    Text="<%$ Tokens:StringResource, address.cs.20 %>"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlCCExpYrErrorMsg" CssClass="error-wrap" runat="server" Visible="false">
                <asp:Label ID="CCExpYrErrorMsg" CssClass="error-large" runat="server"
                    Text="<%$ Tokens:StringResource, address.cs.21 %>"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlCCNumberErrorMsg" CssClass="error-wrap" runat="server" Visible="false">
                <asp:Label ID="CCNumberErrorMsg" CssClass="error-large" runat="server"
                    Text="<%$ Tokens:StringResource, checkoutcard_process.aspx.3 %>"></asp:Label>
            </asp:Panel>

            <asp:ValidationSummary DisplayMode="List" ID="vsAddAddress" ShowMessageBox="false"
                runat="server" ShowSummary="true" ValidationGroup="AddAddress" ForeColor="red"
                Font-Bold="true" />


            <asp:Panel ID="pnlNewAddress" runat="server" Visible="false">
                <aspdnsf:AddressControl ID="ctrlNewAddress" runat="server" OnSelectedCountryChanged="ctrlAddress_SelectedCountryChanged"
                    NickNameCaption='<%$ Tokens:StringResource, address.cs.49 %>' FirstNameCaption='<%$ Tokens:StringResource, address.cs.2 %>'
                    LastNameCaption='<%$ Tokens:StringResource, address.cs.3 %>' PhoneNumberCaption='<%$ Tokens:StringResource, address.cs.4 %>'
                    CompanyCaption='<%$ Tokens:StringResource, address.cs.5 %>' ResidenceTypeCaption='<%$ Tokens:StringResource, address.cs.58 %>'
                    Address1Caption='<%$ Tokens:StringResource, address.cs.6 %>' Address2Caption='<%$ Tokens:StringResource, address.cs.7 %>'
                    SuiteCaption='<%$ Tokens:StringResource, address.cs.8 %>' CityCaption='<%$ Tokens:StringResource, address.cs.9 %>'
                    StateCaption='<%$ Tokens:StringResource, address.cs.10 %>' CountryCaption='<%$ Tokens:StringResource, address.cs.53 %>'
                    ZipCaption='<%$ Tokens:StringResource, address.cs.12 %>' FirstNameRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.13 %>'
                    LastNameRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.14 %>' CityRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.17 %>'
                    PhoneRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.15 %>' Address1RequiredErrorMessage='<%$ Tokens:StringResource, address.cs.16 %>'
                    FirstNameReqFieldValGrp="AddAddress" LastNameReqFieldValGrp="AddAddress"
                    PhoneNumberReqFieldValGrp="AddAddress" CityReqFieldValGrp="AddAddress" Address1ReqFieldValGrp="AddAddress"
                    ShowValidatorsInline="true" Address1CustomValGrp="AddAddress"
                    ZipCodeCustomValGrp="AddAddress" Address1ValidationErrorMessage="<%$ Tokens:StringResource, createaccount_process.aspx.3 %>" />

                <asp:Button ID="btnNewAddress" CssClass="button new-address-button" runat="server" Text='<%$ Tokens:StringResource, address.cs.65 %>'
                    ValidationGroup="AddAddress" OnClick="btnNewAddress_Click" />

                <asp:Button ID="btnCancelAddNew" CssClass="button cancel-button" runat="server" Text='<%$ Tokens:StringResource, address.cs.68 %>' OnClick="btnCancelAddNew_OnClick" />
            </asp:Panel>



            <asp:DataList ID="dlAddress" runat="server" Width="100%" OnEditCommand="dlAddress_EditCommand"
                OnItemDataBound="dlAddress_ItemDataBound" DataKeyField="AddressID" OnUpdateCommand="dlAddress_UpdateCommand"
                OnDeleteCommand="dlAddress_DeleteCommand" OnCancelCommand="dlAddress_CancelCommand"
                OnItemCommand="dlAddress_ItemCommand" AlternatingItemStyle-CssClass="alternating-data-item"
                ItemStyle-CssClass="data-item" EditItemStyle-CssClass="edit-item">
                <ItemTemplate>
                    <div class="page-row address-row">
                        <div class="one-half">
                            <asp:Label ID="lblIndexOrder" runat="server" />
                            <asp:ImageButton ID="ibMakePrimary" runat="server" CommandArgument="MakePrimaryAddress"
                                AlternateText="MakePrimaryAddress" />
                            <asp:ImageButton ID="ibEdit" runat="server" CommandName="Edit" ToolTip='<%$ Tokens:StringResource,admin.common.Edit %>'
                                ImageUrl="~/App_Themes/Skin_1/images/icons/edit.png" AlternateText="Edit" />
                            <asp:ImageButton ID="ibDelete" runat="server" CommandName="Delete" ToolTip='<%$ Tokens:StringResource,admin.common.Delete %>'
                                ImageUrl="~/App_Themes/Skin_1/images/icons/delete.png" AlternateText="Delete"
                                OnClientClick='return confirm("Are you sure you want to delete this?")' />
                        </div>
                        <div class="one-half">
                            <asp:HiddenField ID="hfAddressID" runat="server" Value='<%# DataBinder.Eval(Container.DataItem,"AddressID") %>' />
                            <asp:Label ID="lblAddressHTML" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"AsHTML") %>' />
                        </div>
                    </div>
                </ItemTemplate>
                <EditItemTemplate>

                    <aspdnsf:AddressControl ID="ctrlAddress" runat="server"
                        OnSelectedCountryChanged="ctrlAddress_SelectedCountryChanged"
                        NickNameCaption='<%$ Tokens:StringResource, address.cs.49 %>'
                        FirstNameCaption='<%$ Tokens:StringResource, address.cs.2 %>'
                        LastNameCaption='<%$ Tokens:StringResource, address.cs.3 %>'
                        PhoneNumberCaption='<%$ Tokens:StringResource, address.cs.4 %>'
                        CompanyCaption='<%$ Tokens:StringResource, address.cs.5 %>'
                        ResidenceTypeCaption='<%$ Tokens:StringResource, address.cs.58 %>'
                        Address1Caption='<%$ Tokens:StringResource, address.cs.6 %>'
                        Address2Caption='<%$ Tokens:StringResource, address.cs.7 %>'
                        SuiteCaption='<%$ Tokens:StringResource, address.cs.8 %>'
                        CityCaption='<%$ Tokens:StringResource, address.cs.9 %>'
                        StateCaption='<%$ Tokens:StringResource, address.cs.10 %>'
                        CountryCaption='<%$ Tokens:StringResource, address.cs.53 %>'
                        ZipCaption='<%$ Tokens:StringResource, address.cs.12 %>'
                        Width="100%" Visible="true"
                        FirstNameRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.13 %>'
                        LastNameRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.14 %>'
                        CityRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.17 %>'
                        PhoneRequiredErrorMessage='<%$ Tokens:StringResource, address.cs.15 %>'
                        Address1RequiredErrorMessage='<%$ Tokens:StringResource, address.cs.16 %>'
                        Address1ValidationErrorMessage="<%$ Tokens:StringResource, createaccount_process.aspx.3 %>"
                        FirstNameReqFieldValGrp="EditAddress"
                        LastNameReqFieldValGrp="EditAddress"
                        PhoneNumberReqFieldValGrp="EditAddress"
                        CityReqFieldValGrp="EditAddress"
                        Address1ReqFieldValGrp="EditAddress"
                        ShowValidatorsInline="false"
                        Address1CustomValGrp="EditAddress"
                        ZipCodeCustomValGrp="EditAddress" />
                    <asp:HiddenField ID="hfAddressID" runat="server" Value='<%# DataBinder.Eval(Container.DataItem,"AddressID") %>' />

                    <asp:Label ID="lblPreferredPayment" runat="server" Text='<%$ Tokens:StringResource, editaddress.aspx.4 %>' />

                    <asp:RadioButtonList ID="rblPaymentMethodInfo" runat="server" RepeatDirection="Horizontal"
                        OnSelectedIndexChanged="rblSelectedIndexChanged" AutoPostBack="true">
                        <asp:ListItem Selected="True" Text='<%$ Tokens:StringResource, editaddress.aspx.5 %>'
                            Value="CreditCard" />
                        <asp:ListItem Text='<%$ Tokens:StringResource, editaddress.aspx.6 %>' Value="ECheck" />
                    </asp:RadioButtonList>

                    <asp:Panel ID="pnlCCData" runat="server" Visible="false">
                        <aspdnsf:CreditCardPanel ID="ctrlCreditCard" runat="server" CreditCardExpDtCaption="<%$ Tokens:StringResource, address.cs.33 %>"
                            CreditCardNameCaption="<%$ Tokens:StringResource, address.cs.23 %>" CreditCardNoSpacesCaption="<%$ Tokens:StringResource, shoppingcart.cs.106 %>"
                            CreditCardNumberCaption="<%$ Tokens:StringResource, address.cs.25 %>" CreditCardTypeCaption="<%$ Tokens:StringResource, address.cs.31 %>"
                            CreditCardVerCdCaption="<%$ Tokens:StringResource, address.cs.28 %>" HeaderCaption="<%$ Tokens:StringResource, checkoutcard.aspx.6 %>"
                            WhatIsThis="<%$ Tokens:StringResource, address.cs.50 %>" CCNameReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.24 %>"
                            CreditCardStartDtCaption="<%$ Tokens:StringResource, address.cs.59 %>" CreditCardIssueNumCaption="<%$ Tokens:StringResource, address.cs.61 %>"
                            CreditCardIssueNumNote="<%$ Tokens:StringResource, address.cs.63 %>" CCNameValGrp="EditAddress"
                            CCNumberReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.26 %>" CCNumberValGrp="EditAddress"
                            CCVerCdReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.29 %>" CCVerCdValGrp="EditAddress"
                            ShowCCVerCd="false" ShowCCStartDtFields="<%$ Tokens:AppConfigBool, Misc.ShowCardStartDateFields %>"
                            ShowCCVerCdReqVal="<%$ Tokens:AppConfigBool, General.CardExtraCodeIsOptional %>" ShowValidatorsInline="False" ShowCimSaveCard="false" />
                    </asp:Panel>
                    <asp:Panel ID="pnlECData" runat="server" Visible="false">
                        <aspdnsf:Echeck ID="ctrlECheck" runat="server" ECheckBankABACodeLabel1="<%$ Tokens:StringResource, address.cs.41 %>"
                            ECheckBankABACodeLabel2="<%$ Tokens:StringResource, address.cs.42 %>" ECheckBankAccountNameLabel="<%$ Tokens:StringResource, address.cs.36 %>"
                            ECheckBankAccountNumberLabel1="<%$ Tokens:StringResource, address.cs.44 %>" ECheckBankAccountNumberLabel2="<%$ Tokens:StringResource, address.cs.45 %>"
                            ECheckBankAccountTypeLabel="<%$ Tokens:StringResource, address.cs.47 %>" ECheckBankNameLabel1="<%$ Tokens:StringResource, address.cs.38 %>"
                            ECheckBankNameLabel2="<%$ Tokens:StringResource, address.cs.40 %>" ECheckNoteLabel="<%$ Tokens:StringResource, address.cs.48 %>"
                            BankAccountNameReqFieldErrorMessage="<%$ Tokens:StringResource,address.cs.37 %>"
                            BankNameReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.39 %>" BankABACodeReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.43 %>"
                            BankAccountNumberReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.46 %>"
                            BankAccountNameReqFieldValGrp="EditAddress" BankNameReqFieldValGrp="EditAddress"
                            BankABACodeReqFieldValGrp="EditAddress" BankAccountNumberReqFieldValGrp="EditAddress" />
                    </asp:Panel>

                    <asp:Button ID="btnUpdate" CssClass="button update-button" runat="server" Text='<%$ Tokens:StringResource, address.cs.66 %>'
                        CommandArgument="ID" CommandName="Update" />
                    <asp:Button ID="btnDelete" CssClass="button delete-button" runat="server" Text='<%$ Tokens:StringResource, address.cs.67 %>'
                        CommandName="Delete" OnClientClick='return confirm("Are you sure you want to delete this data?")'
                        ValidationGroup="none" />
                    <asp:Button ID="btnCancel" CssClass="button cancel-button" runat="server" Text='<%$ Tokens:StringResource, address.cs.68 %>'
                        CommandName="Cancel" ValidationGroup="none" />

                </EditItemTemplate>
                <FooterTemplate>

                    <asp:ImageButton ID="ibAddNewAddress" OnClick="AddNewAddress" runat="server" ValidationGroup="none"
                        ImageUrl="~/App_Themes/Skin_1/images/icons/add_address.png" />
                    <asp:LinkButton ID="lbAddNewAddress" OnClick="AddNewAddress" runat="server" ValidationGroup="none" />

                </FooterTemplate>
            </asp:DataList>

            <asp:Button ID="btnReturnUrl" CssClass="button call-to-action" runat="server" Text="<%$ Tokens:StringResource,account.aspx.61 %>"
                OnClick="btnReturnUrlClick" ValidationGroup="none" />
        </div>
    </asp:Panel>
</asp:Content>
