<%@ Page ClientTarget="UpLevel" Language="c#" Inherits="AspDotNetStorefront.createaccount"
    CodeFile="createaccount.aspx.cs" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel runat="server" DefaultButton="btnContinueCheckout">
        <div class="content-box-03 body-forms">
        <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />    
          <h5><asp:Label runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.1 %>"></asp:Label></h5> 
            <p><asp:Label runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.12 %>"></asp:Label></p>
       
        <asp:Panel ID="pnlErrorMsg" runat="Server" Visible="false">
            <div class="error-wrap">
                <asp:Label ID="lblErrorMessage" runat="server" CssClass="error-large"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Literal ID="ltSignin" runat="server" Mode="PassThrough"></asp:Literal>

        <asp:Panel ID="pnlCBAAddressWidget" runat="server" Visible="false">
            <div class="page-row row-cba-address">
                <asp:Literal ID="litCBAAddressWidget" runat="server" />
            </div>
            <div class="page-row row-cba-instructions">
                <asp:Literal ID="litCBAAddressWidgetInstruction" runat="server" />
            </div>
        </asp:Panel>
        <div class="row">

            <asp:Panel ID="pnlAccountInfo" runat="server" Visible="false">

                <aspdnsf:Account ID="ctrlAccount" runat="server" 
                    EmailCaption="<%$ Tokens:StringResource, createaccount.aspx.15 %>"
                    EmailReenterCaption="<%$ Tokens:StringResource, createaccount.aspx.91 %>"
                    FirstNameCaption="<%$ Tokens:StringResource, createaccount.aspx.13 %>"
                    LastNameCaption="<%$ Tokens:StringResource, createaccount.aspx.14 %>"
                    DateOfBirthCaption="<%$ Tokens:StringResource, createaccount.aspx.87 %>"
                    PasswordCaption="<%$ Tokens:StringResource, createaccount.aspx.18 %>"
                    PasswordConfirmCaption="<%$ Tokens:StringResource, createaccount.aspx.21 %>"
                    PhoneCaption="<%$ Tokens:StringResource, createaccount.aspx.23 %>"
                    SaveCCCaption="<%$ Tokens:StringResource, account.aspx.65 %>"
                    OKToEmailCaption="<%$ Tokens:StringResource, createaccount.aspx.26 %>"
                    OKToEmailNoCaption="<%$ Tokens:StringResource, createaccount.aspx.28 %>"
                    OKToEmailYesCaption="<%$ Tokens:StringResource, createaccount.aspx.27 %>"
                    Over13Caption="<%$ Tokens:StringResource, createaccount.aspx.78 %>"
                    VATRegistrationIDCaption="<%$ Tokens:StringResource, account.aspx.71 %>"
                    PasswordNote="<%$ Tokens:StringResource, account.aspx.19 %>"
                    OKToEmailNote="<%$ Tokens:StringResource, createaccount.aspx.29 %>"
                    SaveCCNote="<%$ Tokens:StringResource, account.aspx.70 %>"
                    SecurityCodeCaption="<%$ Tokens:StringResource, signin.aspx.21 %>"
                    FirstNameReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.82 %>"
                    LastNameReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.83 %>"
                    DayOfBirthReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.88 %>"
                    MonthOfBirthReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.89 %>"
                    YearOfBirthReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.90 %>"
                    EmailReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.16 %>"
                    PhoneReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.24 %>"
                    PhoneRegExErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.25 %>"
                    SecurityCodeReqFieldErrorMessage="<%$ Tokens:StringResource, signin.aspx.20 %>"
                    EmailRegExErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.17 %>"
                    ReEnterEmailReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.94 %>"
                    EmailCompareFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.93 %>"
                    PasswordReqFieldErrorMessage="<%$ Tokens:StringResource, createaccount.aspx.20 %>"
                    RequireOver13ErrorMessage="<%$ Tokens:StringResource, Over13OnCreateAccount %>"
                    EmailRegExValGrp="registration"
                    EmailReqFieldValGrp="registration"
                    ReEnterEmailReqFieldValGrp="registration"
                    EmailCompareFieldValGrp="registration"
                    FirstNameReqFieldValGrp="registration"
                    LastNameReqFieldValGrp="registration"
                    DayOfBirthReqFieldValGrp="registration"
                    MonthOfBirthReqFieldValGrp="registration"
                    YearOfBirthReqFieldValGrp="registration"
                    Over13ValGrp="registration"
                    PasswordValidateValGrp="registration"
                    PhoneReqFieldValGrp="registration"
                    PhoneRegExValGrp="registration"
                    ShowSecurityCode="true"
                    ShowPasswordReqVal="true"
                    ShowSaveCC="<%$ Tokens:AppConfigBool, StoreCCInDB %>"
                    ShowValidatorsInline="true"
                    ShowVATRegistrationID="<%$ Tokens:AppConfigBool, Vat.Enabled %>"
                    ShowOver13="<%$ Tokens:AppConfigBool, RequireOver13Checked %>"
                    DisablePasswordAutocomplete="<%$ Tokens:AppConfigBool, DisablePasswordAutocomplete %>" />


            </asp:Panel>

            <asp:Panel ID="pnlSkipReg" runat="server" Visible="false" HorizontalAlign="Left">
                <div class="form skip-reg-form">
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="createaccount_aspx_15_2" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.15 %>"></asp:Literal>
                        </label>
                        <asp:TextBox ID="txtSkipRegEmail" runat="server" CssClass="form-control" MaxLength="100" ValidationGroup="registration"></asp:TextBox>
                        <asp:Literal ID="Literal4" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.81 %>"></asp:Literal>
                        <asp:RequiredFieldValidator ID="valReqSkipRegEmail" runat="server" ControlToValidate="txtSkipRegEmail" Enabled="false" Display="none" ValidationGroup="registration" EnableClientScript="true" ErrorMessage="<%$ Tokens:StringResource,createaccount.aspx.81 %>"></asp:RequiredFieldValidator>
                        <aspdnsf:EmailValidator ID="valRegExSkipRegEmail" ControlToValidate="txtSkipRegEmail" ValidationGroup="registration" Display="none" runat="server" />
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal2" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.78 %>"></asp:Literal>
                        </label>
                        <asp:CheckBox ID="SkipRegOver13" Visible="true" runat="server" CausesValidation="True" ValidationGroup="registration" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlBillingInfo" runat="server">
                <div class="form account-form">
                    <div class="group-header form-header account-header">
                        <asp:Literal ID="Literal1" Text="<%$ Tokens:StringResource,Header.BillingInformation %>" runat="server" />
                    </div>
                    <div class="form-text">
                        <asp:Literal ID="createaccountaspx30" Mode="PassThrough" runat="Server"></asp:Literal>
                    </div>
                    <div class="checkbox">
                        <label>
                            <asp:CheckBox ID="BillingEqualsAccount" runat="server" />
                            <asp:Literal ID="createaccountaspx31" Mode="PassThrough" runat="server"></asp:Literal>
                        </label>
                    </div>

                    <aspdnsf:AddressControl ID="ctrlBillingAddress" runat="server"
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
                        Visible="true"
                        AddressType="Billing"
                        FirstNameRequiredErrorMessage="FirstName Required!"
                        LastNameRequiredErrorMessage="LastName Required!"
                        CityRequiredErrorMessage="City Required!"
                        ZipRequiredErrorMessage="Zip Required!"
                        PhoneRequiredErrorMessage="Phone Number Required!"
                        PhoneValidationErrorMessage="Phone Number is not valid!"
                        Address1RequiredErrorMessage="Address1 Required!"
                        Address1ValidationErrorMessage="<%$ Tokens:StringResource, createaccount_process.aspx.3 %>"
                        FirstNameReqFieldValGrp="registration"
                        LastNameReqFieldValGrp="registration"
                        PhoneNumberReqFieldValGrp="registration"
                        PhoneNumberRegxFieldValGrp="registration"
                        CityReqFieldValGrp="registration"
                        ZipReqFieldValGrp="registration"
                        Address1ReqFieldValGrp="registration"
                        ZipCodeCustomValGrp="registration"
                        Address1CustomValGrp="registration"
                        ShowValidatorsInline="true" />

                </div>
            </asp:Panel>

            <asp:Panel ID="pnlShippingInfo" runat="server" Visible="false">
                <div class="form account-form">
                    <div class="group-header form-header account-header">
                        <asp:Literal ID="Literal3" Text="<%$ Tokens:StringResource,Header.ShippingInformation %>" runat="server" />
                    </div>
                    <div class="form-submit-wrap">
                        <asp:Button ID="btnShppingEqBilling" CssClass="button" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.53 %>"
                            OnClick="btnShppingEqBilling_Click" ValidationGroup="none" />
                    </div>
                    <aspdnsf:AddressControl ID="ctrlShippingAddress" runat="server"
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
                        Visible="true"
                        AddressType="Shipping"
                        FirstNameRequiredErrorMessage="FirstName Required!"
                        LastNameRequiredErrorMessage="LastName Required!"
                        CityRequiredErrorMessage="City Required!"
                        ZipRequiredErrorMessage="Zip Required!"
                        PhoneRequiredErrorMessage="Phone Number Required!"
                        PhoneValidationErrorMessage="Phone Number is not valid!"
                        Address1RequiredErrorMessage="Address1 Required!"
                        Address1ValidationErrorMessage="<%$ Tokens:StringResource, createaccount_process.aspx.3 %>"
                        FirstNameReqFieldValGrp="registration"
                        LastNameReqFieldValGrp="registration"
                        PhoneNumberReqFieldValGrp="registration"
                        PhoneNumberRegxFieldValGrp="registration"
                        CityReqFieldValGrp="registration"
                        ZipReqFieldValGrp="registration"
                        Address1ReqFieldValGrp="registration"
                        ZipCodeCustomValGrp="registration"
                        Address1CustomValGrp="registration"
                        ShowValidatorsInline="true" />

                </div>
            </asp:Panel>

            <div class="clearfix"></div>
            <div class="col-md-5 pull-right">
                <div class="form-group">
                    <asp:Button runat="server" ID="btnContinueCheckout"
                        CssClass="btn btn-md btn-primary btn-block" Text="<%$ Tokens:StringResource,createaccount.aspx.76 %>"
                        ValidationGroup="registration" OnClick="btnContinueCheckout_Click" />
                </div>
            </div>
        </div>
    </div>
    </asp:Panel>
    <script type="text/javascript">
        $(document).ready(function () {
            var txtFirstNameClientId = '<%= ctrlAccount.txtFirstName.ClientID %>';           
            $("#" + txtFirstNameClientId).focus();
        });
    </script>
</asp:Content>
