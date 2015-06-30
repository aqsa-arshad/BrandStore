<%@ Page ClientTarget="UpLevel" Language="c#" Inherits="AspDotNetStorefront.mobilegetaddress" CodeFile="mobilegetaddress.aspx.cs" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <aspdnsf:Topic runat="server" ID="CreateAccountPageHeader" TopicName="CreateAccountPageHeader" />
        <asp:Panel ID="pnlCheckoutImage" runat="server" HorizontalAlign="Center" Visible="false">
            <asp:Literal runat="server" id="CheckoutHeader"></asp:Literal>
        </asp:Panel>
            <asp:Panel ID="pnlErrorMsg" runat="Server" HorizontalAlign="Left" Visible="false">
                <ul data-role="listview">
                    <li class="group" data-role="list-divider">
                            <asp:Label ID="ErrorMsgLabel" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>
                    </li>
                </ul>
            </asp:Panel>
            <asp:Panel ID="pnlAddressInfo" runat="server">
                <ul data-role="listview">
                    <li class="group" data-role="list-divider">
                        <asp:Literal runat="server" ID="MPAddressHeader" />
                    </li>
                    <li data-role="fieldcontain">
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
                            Width="100%" 
                            Visible="true"
                            FirstNameRequiredErrorMessage="FirstName Required!" 
                            LastNameRequiredErrorMessage="LastName Required!"
                            CityRequiredErrorMessage="City Required!" 
                            PhoneRequiredErrorMessage="Phone Number Required!"
                            Address1RequiredErrorMessage="Address1 Required!"                                                                                                       
                            FirstNameReqFieldValGrp="createacccount" 
                            LastNameReqFieldValGrp="createacccount"
                            PhoneNumberReqFieldValGrp="createacccount" 
                            CityReqFieldValGrp="createacccount" 
                            Address1ReqFieldValGrp="createacccount"
                            ZipCodeCustomValGrp="createacccount"
                            ShowValidatorsInline="true" />
                    </li>
                </ul>
            </asp:Panel>            
            <ul data-role="listview"><li>
            
                <asp:Button CssClass="fullwidthshortgreen action" data-icon="check" data-iconpos="right" ID="btnUseForBothAddressTypes" OnClientClick="Page_ValidationActive=true;" runat="server" CausesValidation="True" ValidationGroup="createaccount" OnClick="btnUseForBothAddressTypes_Click" />
                
                <asp:Button CssClass="fullwidthupdate action" ID="btnUseForOneAddress" OnClientClick="Page_ValidationActive=true;" runat="server" CausesValidation="True" ValidationGroup="createaccount" OnClick="btnUseForOneAddress_Click" />
                <asp:ValidationSummary ID="valSummary" DisplayMode="List" runat="server" ShowMessageBox="true" ShowSummary="false" ValidationGroup="createaccount" ForeColor="red" Font-Bold="true" />
            </li></ul>
    </asp:Panel>
</asp:Content>
