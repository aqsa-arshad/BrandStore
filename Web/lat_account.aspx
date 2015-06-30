<%@ Page Language="c#" Inherits="AspDotNetStorefront.lat_account" CodeFile="lat_account.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap affiliate-page">
            <h1>Affiliate Program</h1>
            <asp:Panel ID="pnlMain" runat="server">

                <asp:Panel ID="pnlBeforeSignup" runat="server" Visible="true">
                    <div class="page-links">
						<div class="page-link-wrap">
							<a href="lat_signout.aspx">Affiliate Sign Out</a>
						</div>
						<div class="page-link-wrap">
							<a href="lat_account.aspx">
								<asp:Literal ID="Literal1" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.27 %>" />
							</a>
						</div>
						<div class="page-link-wrap">
							<a href="lat_driver.aspx?topic=affiliate_linking">
								<asp:Literal ID="Literal2" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.26 %>" />
							</a>
						</div>
						<div class="page-link-wrap">
							<a href="lat_driver.aspx?topic=affiliate_faq">
								<asp:Literal ID="Literal3" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.25 %>" />
							</a>
						</div>
						<div class="page-link-wrap">
							<asp:HyperLink ID="AskAQuestion" runat="server" Text="<%$ Tokens:StringResource,lataccount.aspx.24 %>"></asp:HyperLink>
						</div>
                        <div class="page-link-wrap">
							<a href="lat_driver.aspx?topic=affiliate_terms">
								<asp:Literal ID="Literal4" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.23 %>" />
							</a>
						</div>
                    </div>
                </asp:Panel>


                <asp:Panel ID="pnlAfterSignup" runat="server" Visible="false">
                    <div class="page-row">
                        <asp:Literal ID="Literal6" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.22 %>" />
                    </div>
                </asp:Panel>

                <div class="form affiliate-form">
                    <div class="group-header form-header affiliate-header">
                        <asp:Label runat="server" ID="AppConfigAffiliateProgramName3" Font-Bold="true"></asp:Label>
                    </div>
                    <div class="form-text">
                        <asp:Label ID="lblErrMsg" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>
                        <asp:Label ID="lblNote" runat="server" Font-Bold="true" ForeColor="blue"></asp:Label>
                        <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>
                    </div>
                    <div class="form-text">
                        <asp:Label runat="server" ID="AppConfig_AffiliateProgramName4" Font-Bold="true"></asp:Label>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal8" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.7 %>" />
                        </label>
                        <asp:TextBox ID="FirstName" runat="server" MaxLength="50" CssClass="form-control" TextMode="SingleLine" ValidationGroup="signup" CausesValidation="true" />
                        <asp:RequiredFieldValidator ID="valReqFName" ControlToValidate="FirstName" EnableClientScript="true" ErrorMessage="Please enter your first name" runat="server" SetFocusOnError="true" ValidationGroup="signup" Display="None"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal9" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.8 %>" /></label>
                        <asp:TextBox ID="LastName" runat="server" MaxLength="50" CssClass="form-control" TextMode="SingleLine" ValidationGroup="signup" CausesValidation="true" />
                        <asp:RequiredFieldValidator ID="valReqLName" ControlToValidate="LastName" EnableClientScript="true" ErrorMessage="Please enter your last name" runat="server" SetFocusOnError="true" ValidationGroup="signup" Display="None"></asp:RequiredFieldValidator>

                        <asp:Literal ID="Literal10" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.9 %>" />
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:TextBox ID="EMail" runat="server" MaxLength="100" CssClass="form-control" TextMode="SingleLine" CausesValidation="true" ValidationGroup="signup" />
                        </label>
                        <asp:RequiredFieldValidator ID="Reqpwd" ControlToValidate="EMail" EnableClientScript="true" ErrorMessage="Please enter your e-mail address" runat="server" SetFocusOnError="true" ValidationGroup="signup" Display="None"></asp:RequiredFieldValidator>
                        <aspdnsf:EmailValidator ID="RegExValEmail" runat="SERVER" Display="None" ControlToValidate="EMail" EnableClientScript="true" ErrorMessage="Please enter a valid e-mail address" ValidationGroup="signup"></aspdnsf:EmailValidator>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal11" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.10 %>" />
                        </label>

                        <asp:TextBox ID="AffPassword" TextMode="Password" CssClass="form-control" MaxLength="100" runat="server" ValidationGroup="signup" CausesValidation="true"></asp:TextBox><asp:Literal ID="Literal184" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.34 %>" />
                        <asp:CustomValidator ID="valPwd" runat="server" ControlToValidate="AffPassword" Display="none" EnableClientScript="false" ErrorMessage="" ValidationGroup="signup" SetFocusOnError="true" OnServerValidate="ValidatePassword"></asp:CustomValidator>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal12" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.11 %>" />
                        </label>

                        <asp:TextBox ID="AffPassword2" TextMode="Password" CssClass="form-control" MaxLength="100" runat="server" ValidationGroup="signup" CausesValidation="true"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal13" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.12 %>" />
                        </label>
                        <asp:TextBox ID="Company" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" CausesValidation="false" />
                    </div>

                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal14" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.13 %>" />
                        </label>

                        <asp:TextBox ID="Address1" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ID="ReqAddr1" runat="server" ControlToValidate="Address1" ValidationGroup="signup" Display="None" EnableClientScript="true" ErrorMessage="Please enter an address" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>

                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal141" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.14 %>" />
                        </label>
                        <asp:TextBox ID="Address2" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" />
                    </div>

                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal15" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.15 %>" />
                        </label>
                        <asp:TextBox ID="Suite" CssClass="form-control" MaxLength="100" TextMode="singleLine" runat="server" />
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal16" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.16 %>" />
                        </label>

                        <asp:TextBox ID="City" CssClass="form-control" MaxLength="50" TextMode="singleLine" runat="server" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ID="valReqCity" ControlToValidate="City" ErrorMessage="<%$ Tokens:StringResource,address.cs.17 %>" runat="server" ValidationGroup="signup" Display="None" EnableClientScript="true" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal17" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.17 %>" />
                        </label>

                        <asp:DropDownList ID="State" runat="server" CssClass="form-control" OnDataBound="State_DataBound"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valReqState" ControlToValidate="State" ErrorMessage="<%$ Tokens:StringResource,address.cs.17 %>" runat="server" ValidationGroup="signup" Display="None" EnableClientScript="true" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal39" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.18 %>" />
                        </label>

                        <asp:TextBox ID="Zip" CssClass="form-control" MaxLength="10" TextMode="singleLine" runat="server" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ControlToValidate="Zip" ErrorMessage="<%$ Tokens:StringResource,address.cs.18 %>" ID="RequiredFieldValidatorZip" runat="server" ValidationGroup="signup" Display="None" EnableClientScript="true" SetFocusOnError="true"></asp:RequiredFieldValidator>

                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal40" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.19 %>" />
                        </label>

                        <asp:DropDownList ID="Country" runat="server" CssClass="form-control" OnDataBound="Country_DataBound"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valReqCountry" ErrorMessage="<%$ Tokens:StringResource,address.cs.17a %>" runat="server" ControlToValidate="City" ValidationGroup="signup" Display="None" EnableClientScript="true" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>

                            <asp:Literal ID="Literal41" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.20 %>" />
                        </label>

                        <asp:TextBox ID="Phone" CssClass="form-control" MaxLength="20" TextMode="singleLine" runat="server" CausesValidation="true" ValidationGroup="signup" />
                        <asp:RequiredFieldValidator ControlToValidate="Phone" ErrorMessage="<%$ Tokens:StringResource,address.cs.15 %>" ID="RequiredFieldValidatorPhone" runat="server" ValidationGroup="signup" Display="None" EnableClientScript="true" SetFocusOnError="true"></asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal42" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.21 %>" />
                        </label>

                        <asp:TextBox ID="DOBTxt" CssClass="form-control" MaxLength="10" TextMode="singleLine" runat="server" />

                    </div>

                    <div class="form-submit-wrap">
                        <asp:Button ID="btnUpdate1" CssClass="button call-to-action" runat="server" Text="<%$ Tokens:StringResource,lataccount.aspx.1 %>" CausesValidation="true" ValidationGroup="signup" OnClick="btnUpdate1_Click" />
                    </div>
                </div>

                <div class="form affiliate-web-form">
                    <div class="form-text">
                        <asp:Literal ID="Literal22" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.5 %>" />
                        <asp:Literal ID="Literal23" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.6 %>" />
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal24" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.4 %>" />
                        </label>
                        <asp:TextBox ID="WebSiteName" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal25" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.3 %>" />
                        </label>
                        <asp:TextBox ID="WebSiteDescription" runat="server" CssClass="form-control" MaxLength="500"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Literal ID="Literal26" runat="server" Mode="PassThrough" Text="<%$ Tokens:StringResource,lataccount.aspx.2 %>" />
                        </label>
                        <asp:TextBox ID="URL" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                    </div>
                    <div class="form-submit-wrap">
                        <asp:Button ID="btnUpdate2" CssClass="button call-to-action" runat="server" Text="<%$ Tokens:StringResource,lataccount.aspx.1 %>" CausesValidation="true" ValidationGroup="signup" OnClick="btnUpdate2_Click" />
                    </div>
                </div>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>

