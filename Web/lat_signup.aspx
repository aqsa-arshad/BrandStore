<%@ Page Language="c#" Inherits="AspDotNetStorefront.lat_signup" CodeFile="lat_signup.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>


<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap affiliate-page">
            <h1>Affiliate Program</h1>
            <asp:Literal ID="JSPopupRoutines" runat="server" Mode="PassThrough"></asp:Literal>

            <asp:Panel ID="pnlBeforeSignup" runat="server" Visible="true">
                <div class="form-text">
                    <a href="lat_signin.aspx">Already a Member?</a>
                </div>
                <div class="page-links">
					<div class="page-link-wrap">
						<asp:HyperLink runat="server" NavigateUrl="<%$ Tokens:TopicLink, affiliate %>" Text="Learn More" />
					</div>
					<div class="page-link-wrap">
						<asp:HyperLink runat="server" NavigateUrl="<%$ Tokens:TopicLink, affiliate_faq %>" Text="<%$ Tokens:TopicTitle, affiliate_faq %>" />
					</div>
					<div class="page-link-wrap">
						<asp:HyperLink ID="CustSvcEmailLink" runat="server" Text="Customer Service"></asp:HyperLink>
					</div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlAfterSignup" runat="server" Visible="false">
                <div class="group-header affiliate-header">
                    <asp:Label ID="AppConfigAffiliateProgramName4" runat="server"></asp:Label>
                </div>
                <div class="page-row">
                    <div class="page-links">
						<div class="page-link-wrap">
							<a href="lat_signout.aspx">Affiliate Logout</a>
						</div>
						<div class="page-link-wrap">
							<a href="lat_account.aspx">Account Home</a>
						</div>
						<div class="page-link-wrap">
							<a href="lat_getlinking.aspx">Web Linking Instructions</a>
						</div>
						<div class="page-link-wrap">
							<a href="lat_driver.aspx?topic=affiliate_faq">FAQs</a>
						</div>
						<div class="page-link-wrap">
							<asp:HyperLink ID="lnkAskAQuestion" Text="Ask A Question" runat="server"></asp:HyperLink>
						</div>
						<div class="page-link-wrap">
							<a href="lat_driver.aspx?topic=affiliate_terms">Terms &amp; Conditions</a>
						</div>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlSignupSuccess" runat="server" Visible="false">
                <div class="page-row">
                    <asp:Label ID="lblSignupSuccess" runat="server" Font-Bold="true"></asp:Label>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlSignedInMsg" runat="server" Visible="false">
                You're already signed in...please <a href="lat_signout.aspx">sign out</a> here
            </asp:Panel>
            <asp:Panel ID="pnlSignUpForm" runat="server" Visible="false">
                <aspdnsf:Topic runat="server" ID="AffiliateTeaser" TopicName="AffiliateTeaser" />

                <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>


                <asp:ValidationSummary DisplayMode="BulletList" ID="ValSummary" ShowMessageBox="false" runat="server" ShowSummary="true" ValidationGroup="signup" EnableClientScript="false" />

                <div class="form affiliate-signup-form">
                    <div class="group-header form-header affiliate-header">
                        <asp:Label runat="server" ID="AppConfigAffiliateProgramName3" Font-Bold="true"></asp:Label>
                    </div>
                    <div class="form-group">
                        <label>*Your First Name:</label>
                        <asp:TextBox ID="FirstName" CssClass="form-control" runat="server" MaxLength="50" TextMode="SingleLine" CausesValidation="false" />
                        <asp:RequiredFieldValidator ID="valReqFName" ControlToValidate="FirstName" EnableClientScript="false" ErrorMessage="Please enter your first name" runat="server" SetFocusOnError="true" ValidationGroup="signup" Display="None"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>*Your Last Name:</label>

                        <asp:TextBox ID="LastName" CssClass="form-control" runat="server" MaxLength="50" TextMode="SingleLine" CausesValidation="false" />
                        <asp:RequiredFieldValidator ID="valReqLName" ControlToValidate="LastName" EnableClientScript="false" ErrorMessage="Please enter your last name" runat="server" SetFocusOnError="true" ValidationGroup="signup" Display="None"></asp:RequiredFieldValidator>

                    </div>
                    <div class="form-group">
                        <label>*Your E-Mail:</label>

                        <asp:TextBox ID="EMail" CssClass="form-control" runat="server" MaxLength="100" TextMode="SingleLine" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ID="Reqpwd" ControlToValidate="EMail" EnableClientScript="false" ErrorMessage="Please enter your e-mail address" runat="server" SetFocusOnError="true" ValidationGroup="signup" Display="None"></asp:RequiredFieldValidator>
                        <aspdnsf:EmailValidator ID="rgxvEmail" runat="server" Display="None" ControlToValidate="EMail" EnableClientScript="false" ErrorMessage="Please enter a valid e-mail address" ValidationGroup="signup"></aspdnsf:EmailValidator>
                    </div>
                    <div class="form-group">
                        <label>*Password</label>

                        <asp:TextBox ID="AffPassword" TextMode="Password" CssClass="form-control" MaxLength="100" runat="server" ValidationGroup="signup" CausesValidation="true"></asp:TextBox>
                        (at least 5 chars long)
                                                                <asp:RequiredFieldValidator ID="reqAffPwd" runat="server" Display="None" ControlToValidate="AffPassword" EnableClientScript="false" ValidationGroup="signup" SetFocusOnError="true"></asp:RequiredFieldValidator>
                        <asp:CustomValidator ID="valPwd" runat="server" ControlToValidate="AffPassword" Display="none" EnableClientScript="false" ErrorMessage="" ValidationGroup="signup" SetFocusOnError="true" OnServerValidate="ValidatePassword"></asp:CustomValidator>
                    </div>
                    <div class="form-group">
                        <label>*Repeat Password:</label>
                        <asp:TextBox ID="AffPassword2" TextMode="Password" CssClass="form-control" MaxLength="100" runat="server" ValidationGroup="signup" CausesValidation="true"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Company:</label>

                        <asp:TextBox ID="Company" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" CausesValidation="false" />
                    </div>
                    <div class="form-group">
                        <label>*Address1:</label>
                        <asp:TextBox ID="Address1" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ID="ReqAddr1" runat="server" ControlToValidate="Address1" ValidationGroup="signup" Display="None" EnableClientScript="false" ErrorMessage="Please enter an address" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>Address2:</label>
                        <asp:TextBox ID="Address2" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" />
                    </div>
                    <div class="form-group">
                        <label>Suite:</label>
                        <asp:TextBox ID="Suite" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" />
                    </div>
                    <div class="form-group">
                        <label>*City:</label>
                        <asp:TextBox ID="City" CssClass="form-control" MaxLength="50" TextMode="singleLine" runat="server" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidatorCity" ErrorMessage="Please enter a city" runat="server" ControlToValidate="City" ValidationGroup="signup" Display="None" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>*State/Province:</label>
                        <asp:DropDownList ID="State" CssClass="form-control" runat="server" Width="250"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valReqState" ControlToValidate="State" ErrorMessage="Please select a state" runat="server" ValidationGroup="signup" Display="None" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>*Zip:</label>
                        <asp:TextBox ID="Zip" CssClass="form-control" MaxLength="10" TextMode="singleLine" runat="server" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ControlToValidate="Zip" ErrorMessage="Please enter the zipcode" ID="RequiredFieldValidatorZip" runat="server" ValidationGroup="signup" Display="None" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>*Country:</label>

                        <asp:DropDownList ID="Country" runat="server" CssClass="form-control">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valReqCountry" ControlToValidate="Country" ErrorMessage="Please select a country" runat="server" ValidationGroup="signup" Display="None" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>

                    </div>
                    <div class="form-group">
                        <label>*Phone:</label>

                        <asp:TextBox ID="Phone" CssClass="form-control" MaxLength="20" TextMode="singleLine" runat="server" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ControlToValidate="Phone" ErrorMessage="Please enter the phone number" ID="RequiredFieldValidatorPhone" runat="server" ValidationGroup="signup" Display="None" EnableClientScript="false" SetFocusOnError="true"></asp:RequiredFieldValidator>

                    </div>
                    <div class="form-group">
                        <label>Birthday:</label>

                        <asp:TextBox ID="DateOfBirth" runat="server" CssClass="form-control" TextMode="singleLine" Columns="14"></asp:TextBox>
                    </div>
                </div>
                <div id="tblAffWebInfo" class="form affiliate-web-form" runat="server">
                    <div class="group-header form-header affiliate-web-header">Website Information</div>
                    <div id="tblWebSiteInfoBox" runat="server">
                        <div class="form-text">
                            Online affiliates must also complete the following fields.
                        You only need to enter these fields if you will be using a web site to link to us.
                        </div>
                        <div class="form-group">
                            <label>Your Web Site Name:</label>
                            <asp:TextBox ID="WebSiteName" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" />
                        </div>
                        <div class="form-group">
                            <label>Your Web Site Description:</label>
                            <asp:TextBox ID="WebSiteDescription" CssClass="form-control" MaxLength="500" TextMode="singleLine" runat="server" />
                        </div>
                        <div class="form-group">
                            <label>Your Web Site URL:</label>

                            <asp:TextBox ID="URL" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" />
                        </div>
                    </div>
                </div>
                <div>
                    <asp:CheckBox ID="cbkAgreeToTermsAndConditions" runat="server" CausesValidation="true" ValidationGroup="signup" />By selecting this box and pressing the "Join" button, I agree to these
                                            <asp:Literal ID="TermsLink" runat="server" Mode="PassThrough"></asp:Literal>
                    <asp:Button ID="btnJoin" CssClass="button call-to-action" Text="Join" runat="server" CausesValidation="true" ValidationGroup="signup" Enabled="false" OnClick="btnJoin_Click" />
                    <asp:CustomValidator ID="ValTerms" runat="server" ErrorMessage="Please select the Terms and Conditions checkbox to indicate that you agree with the Terms and Conditions." Display="None" ClientValidationFunction="AgreeToTerms" ValidationGroup="signup" OnServerValidate="ValTerms_ServerValidate"></asp:CustomValidator>
                </div>

            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>


