<%@ Page Language="c#" Inherits="AspDotNetStorefront.lat_driver" CodeFile="lat_driver.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap affiliate-page">
            <h1>Affiliate Program</h1>

            <asp:Panel ID="pnlBeforeSignup" runat="server" Visible="true">
                <div class="page-links">
					<div class="page-link-wrap">
						<a href="lat_signout.aspx">Affiliate Sign Out</a>
					</div>
					<div class="page-link-wrap">
						<a href="lat_account.aspx">Account Home</a>
					</div>
					<div class="page-link-wrap">
						<a href="lat_driver.aspx?topic=affiliate_linking">Web Linking Instructions</a>
					</div>
					<div class="page-link-wrap">
						<a href="lat_driver.aspx?topic=affiliate_faq">FAQs</a>
					</div>
					<div class="page-link-wrap">
						<asp:HyperLink ID="AskAQuestion" runat="server" Text="Ask A Question"></asp:HyperLink>
					</div>
					<div class="page-link-wrap">
						<a href="lat_driver.aspx?topic=affiliate_terms">Terms &amp; Conditions</a>
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
							<asp:HyperLink ID="lnkAskAQustion" Text="Ask A Question" runat="server"></asp:HyperLink>
						</div>
						<div class="page-link-wrap">
							<a href="lat_driver.aspx?topic=affiliate_terms">Terms &amp; Conditions</a>
						</div>
                    </div>
                </div>
            </asp:Panel>


            <asp:Label ID="lblErrorMsg" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>


            <asp:Literal ID="PageTopic" runat="server" Mode="PassThrough"></asp:Literal>
        </div>
    </asp:Panel>
</asp:Content>



