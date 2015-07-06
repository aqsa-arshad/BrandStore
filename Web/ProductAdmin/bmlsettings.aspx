<%@ Page Language="C#" AutoEventWireup="true" CodeFile="bmlsettings.aspx.cs" Inherits="AspDotNetStorefrontAdmin.bmlsettings" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" TagName="editAppConfigAtom" Src="Controls/editAppConfigAtom.ascx" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Panel ID="pnlBillMeLater" runat="server" CssClass="bml-panel" DefaultButton="btnSaveConfiguration">
        <script src="Scripts/jquery.min.js" type="text/javascript"></script>
        <script src="Scripts/jquery-ui-1.8.17.custom.min.js" type="text/javascript"></script>
        <script src="Scripts/jqueryextensions.js" type="text/javascript"></script>
        <script type="text/javascript">
            $(function () {
                $(".paypal-link").click(function () {
                    window.open('https://financing.paypal.com/ppfinportal/cart/index?dcp=3dc7b71e28f27588a17f4d973724bba4f19bb55d', 'PayPal Publisher ID', 'width=520px,height=640px,location=no,menubar=no,toolbar=no,scrollbars=no,resizable=no');
                });
            });
        </script>
        <script type="text/javascript">
            window.onload = function () {
                var scrollY = parseInt('<%=Request.Form["scrollY"] %>');
                if (!isNaN(scrollY)) {
                    window.scrollTo(0, scrollY);
                }
            };
            window.onscroll = function () {
                var scrollY = document.body.scrollTop;
                if (scrollY == 0) {
                    if (window.pageYOffset) {
                        scrollY = window.pageYOffset;
                    }
                    else {
                        scrollY = (document.body.parentElement) ? document.body.parentElement.scrollTop : 0;
                    }
                }
                if (scrollY > 0) {
                    var input = document.getElementById("scrollY");
                    if (input == null) {
                        input = document.createElement("input");
                        input.setAttribute("type", "hidden");
                        input.setAttribute("id", "scrollY");
                        input.setAttribute("name", "scrollY");
                        document.forms[0].appendChild(input);
                    }
                    input.value = scrollY;
                }
            };
        </script>
        <div class="configurationWrapper atomModalContent">
            <div class="modalTable modalHeader">
                <table width="100%" cellpadding="4" cellspacing="0">
                    <tbody>
                        <tr>
                            <td width="225px">
                                <img src="images/PayPal_OnBoarding_BillMeLat.png" border="0" />
                            </td>
                            <td>
                                <h1>Advertise Bill Me Later Financing 
                                </h1>
                                <p><a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=billmelater&amp;type=info" target="_blank">More info</a></p>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div class="configTitle"><b>1. Enable PayPal Payment Methods </b></div>
            <div class="sub">
                <asp:Panel ID="pnlPayPalEnabled" runat="server">
                     PayPal is enabled.  
                </asp:Panel>
                <asp:Panel ID="pnlPayPalNotEnabled" runat="server">
                     PayPal is not enabled.  <a href="wizard.aspx#payments">Click here</a> to enable PayPal.
                </asp:Panel>
            </div>
            <div class="configTitle"><b>2. Agree to Terms & Conditions</b></div>
            <div class="sub">
                <asp:Panel ID="pnlTermsAndConditions" runat="server">
                    <div class="termsWrap">
                        <asp:CheckBox ID="chkTermsAndConditions" runat="server" OnCheckedChanged="chkTermsAndConditions_CheckedChanged" AutoPostBack="true" />
                       I agree to the <a href="//www.aspdotnetstorefront.com/linkmanager.aspx?topic=billmelater&type=terms"  target="_blank">terms and conditions</a> for PayPal Banners
                    </div>
                </asp:Panel>
            </div>
            <div class="configTitle"><b>3. Enter your PayPal Publisher ID</b></div>
            <div class="sub">

                <div class="publisherIdWrap">
                    <aspdnsf:editAppConfigAtom runat="server" ID="AtomPublisherId" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.PublisherId" FriendlyName="PayPal Publisher ID" />
                </div>
                <a href="javascript:void(0);" class="paypal-link emphasisButton">Get Your PayPal Publisher ID</a>
                <asp:Button ID="btnSavePublisherId" runat="server" CssClass="defaultButton" OnClick="btnSavePublisherId_Click" Text="Save Your Publisher ID" />
            </div>
            <div class="configTitle"><b>4. Configure Your Banners</b></div>
            <div class="wizardButtonWrap">
                <asp:Button ID="btnSaveConfigurationTop" runat="server" CssClass="defaultButton" OnClick="btnSaveConfiguration_Click" Text="Save Configuration" />
            </div>
            <div class="bannerWrapper sub">
                <table class="bannerTable">
                    <tr>
                        <td valign="top">
                            <div class="bannerHeader">Home Page Banner</div>
                            <img src="images/paypal_ad_homepage.png" alt="Home Page" />
                        </td>
                        <td valign="bottom">
                            <div>
                                <aspdnsf:editAppConfigAtom runat="server" ID="AtomShowAdsOnHomePage" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.ShowOnHomePage" FriendlyName="Show PayPal Banners on My Home Page" />
                                <aspdnsf:editAppConfigAtom runat="server" ID="AtomHomePageDimensions" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.HomePageDimensions" FriendlyName="Home Page Banners Dimensions" />
                                <asp:Button ID="btnSaveConfiguration" runat="server" CssClass="defaultButton" OnClick="btnSaveConfiguration_Click" Text="Update &amp; Save" />
                            </div>
                        </td>
                    </tr>
                </table>

                <asp:Panel ID="pnlHomeScript" runat="server">
                    <div class="scriptWrap">
                        <asp:Literal ID="ltHomeScript" runat="server" />
                    </div>
                </asp:Panel>
            </div>
            <div class="bannerWrapper sub">
                <table class="bannerTable">
                    <tr>
                        <td valign="top">
                            <div class="bannerHeader">Shopping Cart Page Banner</div>
                            <img src="images/paypal_ad_cartpage.png" alt="Home Page" />
                        </td>
                        <td valign="bottom">
                            <div>
                                <aspdnsf:editAppConfigAtom runat="server" ID="AtomShowAdsOnCartPage" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.ShowOnCartPage" FriendlyName="Show PayPal Banners on My Shopping Cart Page" />
                                <aspdnsf:editAppConfigAtom runat="server" ID="AtomCartPageDimensions" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.CartPageDimensions" FriendlyName="Cart Page Banner Dimensions" />
                                <asp:Button ID="btnUpdateShoppingCart" runat="server" CssClass="defaultButton" OnClick="btnSaveConfiguration_Click" Text="Update &amp; Save" />
                            </div>
                        </td>
                    </tr>
                </table>
                <asp:Panel ID="pnlCartScript" runat="server">
                    <div class="scriptWrap">
                        <asp:Literal ID="ltCartScript" runat="server" />
                    </div>
                </asp:Panel>
            </div>
            <div class="bannerWrapper sub">
                <table class="bannerTable">
                    <tr>
                        <td valign="top">
                            <div class="bannerHeader">Product Page Banner</div>
                            <img src="images/paypal_ad_productpage.png" alt="Home Page" />
                        </td>
                        <td valign="bottom">
                            <div>
                                <aspdnsf:editAppConfigAtom runat="server" ID="AtomShowAdsOnProductPage" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.ShowOnProductPage" FriendlyName="Show PayPal Banners on My Product Page" />
                                <aspdnsf:editAppConfigAtom runat="server" ID="AtomProductPageDimensions" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.ProductPageDimensions" FriendlyName="Product Page Banner Dimensions" />
                                <asp:Button ID="btnUpdateProductPage" runat="server" CssClass="defaultButton" OnClick="btnSaveConfiguration_Click" Text="Update &amp; Save" />
                            </div>
                        </td>
                    </tr>
                </table>
                <asp:Panel ID="pnlProductScript" runat="server">
                    <div class="scriptWrap">
                        <asp:Literal ID="ltProductScript" runat="server" />
                    </div>
                </asp:Panel>
            </div>
            <div class="bannerWrapper sub">
                <table class="bannerTable">
                    <tr>
                        <td valign="top">
                            <div class="bannerHeader">Entity Page Banner</div>
                            <img src="images/paypal_ad_entitypage.png" alt="Home Page" />
                        </td>
                        <td valign="bottom">
                            <div>
                                <aspdnsf:editAppConfigAtom runat="server" ID="AtomShowAdsOnEntityPage" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.ShowOnEntityPage" FriendlyName="Show PayPal Banners on My Entity Page" />
                                <aspdnsf:editAppConfigAtom runat="server" ID="AtomEntityPageDimensions" HideTableNode="false" ShowSaveButton="false" AppConfig="PayPal.Ads.EntityPageDimensions" FriendlyName="Entity Page Banner Dimensions" />
                                <asp:Button ID="btUpdateEntity" runat="server" CssClass="defaultButton" OnClick="btnSaveConfiguration_Click" Text="Update &amp; Save" />
                            </div>
                        </td>
                    </tr>
                </table>
                <asp:Panel ID="pnlEntityScript" runat="server">
                    <div class="scriptWrap">
                        <asp:Literal ID="ltEntityScript" runat="server" />
                    </div>
                </asp:Panel>
            </div>
            <div class="wizardButtonWrap">
                <asp:Button ID="btnSaveConfigurationBottom" runat="server" CssClass="defaultButton" OnClick="btnSaveConfiguration_Click" Text="Save Configuration" />
            </div>
            <asp:Panel ID="pnlDisableConfiguration" runat="server">
                <script type="text/javascript">
                    $(".bannerWrapper :input").attr("disabled", true);
                    $(".bannerWrapper").css("opacity", 0.3);
                    $(".bannerWrapper").css("filter", "alpha(opacity = 30)");
                    $(".wizardButtonWrap :input").attr("disabled", true);
                    $(".wizardButtonWrap").css("opacity", 0.3);
                    $(".wizardButtonWrap").css("filter", "alpha(opacity = 30)");
                </script>
            </asp:Panel>
        </div>
    </asp:Panel>
</asp:Content>
