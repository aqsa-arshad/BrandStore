<%@ Page Language="C#" AutoEventWireup="true" CodeFile="default.aspx.cs" Inherits="AspDotNetStorefrontAdmin._default"
    MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"
    Theme="Admin_Default" Title="<%$Tokens:StringResource, admin.title.default %>" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="Controls/XmlPackageControl.ascx" %>
<%@ OutputCache Duration="1" Location="none" %>
<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
    <%-- PANEL EXTENDERS TO ALLOW COLLAPSIBLE DIVS --%>
    <cc1:CollapsiblePanelExtender ID="cpxSecurity" runat="server" TargetControlID="pnlSecurity"
        Collapsed="false" CollapsedSize="60" CollapsedImage="~/App_Themes/Admin_Default/images/icon_restore.jpg"
        ExpandedImage="~/App_Themes/Admin_Default/images/icon_minimize.jpg" ExpandDirection="Vertical"
        ExpandControlID="imgMinimize_SecurityAudit" CollapseControlID="imgMinimize_SecurityAudit"
        ImageControlID="imgMinimize_SecurityAudit" />
    <cc1:CollapsiblePanelExtender ID="cpxLowStock" runat="server" TargetControlID="pnlLowStock"
        Collapsed="false" CollapsedSize="60" CollapsedImage="~/App_Themes/Admin_Default/images/icon_restore.jpg"
        ExpandedImage="~/App_Themes/Admin_Default/images/icon_minimize.jpg" ExpandDirection="Vertical"
        ExpandControlID="imgMinimize_LowStock" CollapseControlID="imgMinimize_LowStock"
        ImageControlID="imgMinimize_LowStock" />
    <cc1:CollapsiblePanelExtender ID="cpxStats" runat="server" TargetControlID="pnlStatistics"
        Collapsed="false" CollapsedSize="60" CollapsedImage="~/App_Themes/Admin_Default/images/icon_restore.jpg"
        ExpandedImage="~/App_Themes/Admin_Default/images/icon_minimize.jpg" ExpandDirection="Vertical"
        ExpandControlID="imgMinimize_Statistics" CollapseControlID="imgMinimize_Statistics"
        ImageControlID="imgMinimize_Statistics" />
    <cc1:CollapsiblePanelExtender ID="cpxFeeds" runat="server" TargetControlID="pnlFeeds"
        Collapsed="false" CollapsedSize="60" CollapsedImage="~/App_Themes/Admin_Default/images/icon_restore.jpg"
        ExpandedImage="~/App_Themes/Admin_Default/images/icon_minimize.jpg" ExpandDirection="Vertical"
        ExpandControlID="imgMinimize_Feeds" CollapseControlID="imgMinimize_Feeds" ImageControlID="imgMinimize_Feeds" />
    <cc1:CollapsiblePanelExtender ID="cpxOrders" runat="server" TargetControlID="pnlOrders"
        Collapsed="false" CollapsedSize="60" CollapsedImage="~/App_Themes/Admin_Default/images/icon_restore.jpg"
        ExpandedImage="~/App_Themes/Admin_Default/images/icon_minimize.jpg" ExpandDirection="Vertical"
        ExpandControlID="imgMinimize_LatestOrders" CollapseControlID="imgMinimize_LatestOrders"
        ImageControlID="imgMinimize_LatestOrders" />
    <cc1:CollapsiblePanelExtender ID="cpxCustomers" runat="server" TargetControlID="pnlCustomers"
        Collapsed="false" CollapsedSize="60" CollapsedImage="~/App_Themes/Admin_Default/images/icon_restore.jpg"
        ExpandedImage="~/App_Themes/Admin_Default/images/icon_minimize.jpg" ExpandDirection="Vertical"
        ExpandControlID="imgMinimize_LatestRegisteredCustomers" CollapseControlID="imgMinimize_LatestRegisteredCustomers"
        ImageControlID="imgMinimize_LatestRegisteredCustomers" />
    <%-- END PANEL EXTENDERS --%>
    <div>
        <table border="0" cellpadding="0" cellspacing="0" style="width: 100%">
            <tr>
                <td id="colLeft" align="center" valign="top" class="splash_LeftPaneBgColor" runat="server">
                    <input type="hidden" runat="server" id="colLeft_Visible" name="colLeft_Visible" value="1" />
                    <div class="splash_Breadcrumb" id="divLtError" runat="server" visible="false">
                        <asp:Literal ID="ltError" runat="server" />
                    </div>
                    <%-- LEFT MENU SPACER--%>
                    <div class="splash_LeftMenuSpacer">
                    </div>
                    <%-- END LEFT MENU SPACER--%>
                    <%-- COMMONLINKS --%>
                    <asp:Panel ID="pnlCommonLinks" runat="server">
                        <div id="" class="splash_GroupHeader">
                            <table style="width: 100%; height: 100%; border: 0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td style="width: 330px; text-align: center">
                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CommonLinks %>" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div class="splash_commonLinksBody">
                            <table style="width: 100%">
                                <tr>
                                    <td>
                                        <ul id="splash_linksMenu">
                                            <li runat="server" id="ChangeEncryptKeyReminder" visible="false"><a href="changeencryptkey.aspx">
                                                <font color="red"><b>
                                                    <asp:Label Font-Bold="true" runat="server" Text="<%$Tokens:StringResource, admin.default.ChangeEncryptKey %>" /></b></font></a></li>
                                            <li runat="server" id="WizardPrompt"><a href="wizard.aspx">
                                                <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.RunConfigurationWizard %>" /></a></li>
                                            <li runat="server" id="MonthlyMaintenancePrompt" visible="true"><a href="monthlymaintenance.aspx">
                                                <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.RunMonthlyMaintenance %>" /></a></li>
                                            <li><a href="orders.aspx">
                                                <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewManageOrders %>" /></a></li>
                                            <li><a href="newproducts.aspx">
                                                <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewManageProducts %>" /></a></li>
                                            <li><a href="customers.aspx">
                                                <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewManageCustomers %>" /></a></li>
                                            <li><a href="appconfig.aspx">
                                                <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewEditAppConfigs %>" /></a></li>
                                        </ul>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </asp:Panel>
                    <%-- END COMMONLINKS --%>
                    <%-- LEFT MENU SPACER--%>
                    <div class="splash_LeftMenuSpacer">
                    </div>
                    <%-- END LEFT MENU SPACER--%>
                    <%-- PATCH INFORMATION --%>
                    <asp:Panel runat="server" ID="pnlPatchInfo" Visible="false">
                        <div class="splash_GroupHeader">
                            <table style="width: 100%; height: 100%; border: 0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td style="width: 330px; text-align: center">
                                            <asp:Label ID="PatchInfoHeader" runat="server" Text="Patch Information" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                         <div class="splash_sysInfoBody">
                            <div style="padding:0 10px;">
                                <asp:Literal ID="litPatchInfo" runat="server" />
                            </div>
                         </div>
                         <div class="splash_LeftMenuSpacer"></div>
                    </asp:Panel>
                    <%-- END PATCH INFORMATION --%>
                    <%-- SYSTEM INFORMATION --%>
                    <div>
                        <div class="splash_GroupHeader">
                            <table style="width: 100%; height: 100%; border: 0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td style="width: 330px; text-align: center">
                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.SystemInformation %>" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div class="splash_sysInfoBody">
                            <table width="100%" cellpadding="1" cellspacing="1" border="0">
                                <tr>
                                    <td width="180" align="right" valign="top">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.Version %>" /></font>
                                    </td>
                                    <td valign="top">
                                        <asp:Literal ID="ltStoreVersion" runat="server"></asp:Literal><br />
                                        <a runat="server" id="VersionInfoDetails" href="versioninfo.aspx?excludeImages=true">Details</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CurrentServerDateTime %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltDateTime" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label ID="lbTrustLevel" runat="server" Text="<%$Tokens:StringResource, admin.default.TrustLevel %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltTrustLevel" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ExecutionMode %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltExecutionMode" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.UseSSL %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltUseSSL" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.OnLiveServer %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltOnLiveServer" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.IsSecureConnection %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltServerPortSecure" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CachingIs %>" /></font>
                                    </td>
                                    <td>
                                        <asp:LinkButton ID="lnkCacheSwitch" runat="server" OnClick="lnkCacheSwitch_Click"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.LinkButton %>" /></asp:LinkButton>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.PrimaryStoreLocaleSetting %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltWebConfigLocaleSetting" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.SQLLocaleSetting %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltSQLLocaleSetting" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CustomerLocaleSetting %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltCustomerLocaleSetting" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.PrimaryStoreCurrency %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="PrimaryCurrency" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentGateway %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltPaymentGateway" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.GatewayMode %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltUseLiveTransactions" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.TransactionMode %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltTransactionMode" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right" style="white-space: nowrap" valign="top">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentMethods %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltPaymentMethods" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr id="trMicropay" runat="server">
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.MicroPayEnabled %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="ltMicroPayEnabled" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr id="trCardinal" runat="server">
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CardinalEnabled %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="CardinalEnabled" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr id="trStoreCC" runat="server">
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.StoreCreditCards %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="StoreCC" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr id="trGatewayRec" runat="server">
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.default.UsingGatewayRecurringBilling %>" /></font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="GatewayRecurringBilling" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                                <tr id="trBuySafe" runat="server">
                                    <td align="right" style="white-space: nowrap">
                                        <font class="subTitleSmall">
                                            <asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.menu.BuySafe %>" />:</font>
                                    </td>
                                    <td>
                                        <asp:Literal ID="litBuySafeStatus" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        
                    </div>
                    <%-- END SYSTEM INFORMATION --%>
                </td>
                <td id="colMain" valign="top" class="splash_RightPaneBgColor">
                    <div id="pnlShowLeftPanel" runat="server" style="float: left; display: none;">
                        <asp:Image ID="imgShowLeftPanel" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/splash_showLeft.jpg"
                            hover="~/App_Themes/Admin_Default/images/splash_showLeft_hover.jpg" class="splash_ImageButton" />
                    </div>
                    <table>
                        <tr>
                            <td width="50%" valign="top">
                                <%--SECURITY AUDIT RESULTS--%>
                                <asp:Panel ID="pnlSecurity" runat="server">
                                    <div id="divSecurityAudit" runat="server">
                                        <div style="padding-left: 30px; padding-top: 20px">
                                            <table style="width: 100%" border="0" cellpadding="0" cellspacing="0">
                                                <tr>
                                                    <td class="splash_securityAuditTitleHeadIcon">
                                                    </td>
                                                    <td>
                                                        <div style="height: 10px">
                                                        </div>
                                                        <div style="float: left; width: 24px; height: 24px;">
                                                            <asp:Image ID="imgMinimize_SecurityAudit" runat="server" class="splash_ImageButton"
                                                                ImageUrl="~/App_Themes/Admin_Default/images/icon_minimize.jpg" min="~/App_Themes/Admin_Default/images/icon_minimize.jpg"
                                                                min_hover="~/App_Themes/Admin_Default/images/icon_minimize_hover.jpg" restore="~/App_Themes/Admin_Default/images/icon_restore.jpg"
                                                                restore_hover="~/App_Themes/Admin_Default/images/icon_restore_hover.jpg" />
                                                        </div>
                                                        <div class="splash_groupsTitleHead">
                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.SecurityAudit %>" />
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                        <input type="hidden" runat="server" id="divSecurityAudit_Content_Visible" name="divSecurityAudit_Content_Visible"
                                            value="1" />
                                        <div id="divSecurityAudit_Content" class="splash_divSecurityAudit" runat="server"
                                            visible="true">
                                            <asp:Table runat="server" ID="tblSecurityAudit" Width="100%" CellPadding="1" CellSpacing="0" />
                                        </div>
                                    </div>
                                </asp:Panel>
                                <%--END SECURITY AUDIT RESULTS--%>
                            </td>
                            <td width="50%" valign="top">
                                <%--FEEDS--%>
                                <asp:Panel runat="server" ID="pnlFeeds">
                                    <div id="divFeeds" runat="server" style="padding-left: 30px; padding-top: 20px">
                                        <table style="width: 100%" border="0" cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td class="splash_NewsTitleHeadIcon">
                                                </td>
                                                <td>
                                                    <div style="height: 10px">
                                                    </div>
                                                    <div style="float: left; width: 24px; height: 24px">
                                                        <asp:Image ID="imgMinimize_Feeds" runat="server" class="splash_ImageButton" ImageUrl="~/App_Themes/Admin_Default/images/icon_minimize.jpg"
                                                            min="~/App_Themes/Admin_Default/images/icon_minimize.jpg" min_hover="~/App_Themes/Admin_Default/images/icon_minimize_hover.jpg"
                                                            restore="~/App_Themes/Admin_Default/images/icon_restore.jpg" restore_hover="~/App_Themes/Admin_Default/images/icon_restore_hover.jpg" />
                                                    </div>
                                                    <div class="splash_groupsTitleHead">
                                                        <span>AspDotNetStorefront News</span>
                                                    </div>
                                                    <input type="hidden" runat="server" id="divFeeds_Content_Visible" name="divFeeds_Content_Visible" value="1" />
                                                    <div id="divFeeds_Content" runat="server">
                                                        <asp:Panel ID="pnlNewsFeed" runat="server" Visible="true" Width="100%">
                                                            <aspdnsf:XmlPackage ID="NewsFeed" runat="server" PackageName="rss.aspdnsfrssconsumer.xml.config"
                                                                RuntimeParams="channel=news&height=170&width=800" />
                                                        </asp:Panel>
                                                        <asp:Panel ID="pnlSponsorFeed" runat="server" Visible="true">
                                                            <aspdnsf:XmlPackage ID="SponsorFeed" runat="server" PackageName="rss.aspdnsfrssconsumer.xml.config"
                                                                RuntimeParams="channel=sponsors&height=200&width=830" />
                                                        </asp:Panel>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </asp:Panel>
                                <%--END FEEDS--%>
                            </td>
                        </tr>
                    </table>

                <%--LOW STOCK WARNING--%>
                <asp:Panel ID="pnlLowStock" runat="server">
                    <div id="divLowStock" runat="server">
                        <div style="padding-left: 30px; padding-top: 20px">
                            <table style="width: 100%" border="0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td class="splash_latestOrdersTitleHeadIcon">
                                    </td>
                                    <td>
                                        <div style="height: 10px">
                                        </div>
                                        <div style="float: left; width: 24px; height: 24px;">
                                            <asp:Image ID="imgMinimize_LowStock" runat="server" class="splash_ImageButton" ImageUrl="~/App_Themes/Admin_Default/images/icon_minimize.jpg"
                                                min="~/App_Themes/Admin_Default/images/icon_minimize.jpg" min_hover="~/App_Themes/Admin_Default/images/icon_minimize_hover.jpg"
                                                restore="~/App_Themes/Admin_Default/images/icon_restore.jpg" restore_hover="~/App_Themes/Admin_Default/images/icon_restore_hover.jpg" />
                                        </div>
                                        <div class="splash_groupsTitleHead">
                                            <asp:Label ID="Label3" runat="server" Text="<%$Tokens:StringResource, admin.LowStockWarningTitle %>" />
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <input type="hidden" runat="server" id="divLowStock_Content_Visible" name="divLowStock_Content_Visible"
                            value="1" />
                        <div id="divLowStock_Content" class="splash_divSecurityAudit" runat="server" visible="true">
                            <p>
                                <asp:Literal ID="lowStockHeader" runat="server" Text="<%$Tokens:StringResource, admin.LowStockHeader%>" /></p>
                            <asp:Table runat="server" ID="tblLowStock" Width="100%" CellPadding="1" CellSpacing="0"
                                border="1">
                                <asp:TableHeaderRow CssClass="gridHeader">
                                    <asp:TableHeaderCell>
                                        <asp:Literal ID="lowStockLit1" runat="server" Text="<%$Tokens:StringResource, admin.common.ProductName%>" /></asp:TableHeaderCell>
                                    <asp:TableHeaderCell>
                                        <asp:Literal ID="lowStockLit2" runat="server" Text="<%$Tokens:StringResource, admin.common.VariantName%>" /></asp:TableHeaderCell>
                                    <asp:TableHeaderCell>
                                        <asp:Literal ID="lowStockLit3" runat="server" Text="<%$Tokens:StringResource, admin.common.Size%>" /></asp:TableHeaderCell>
                                    <asp:TableHeaderCell>
                                        <asp:Literal ID="lowStockLit4" runat="server" Text="<%$Tokens:StringResource, admin.common.Color%>" /></asp:TableHeaderCell>
                                    <asp:TableHeaderCell>
                                        <asp:Literal ID="lowStockLit5" runat="server" Text="<%$Tokens:StringResource, admin.common.Quantity%>" /></asp:TableHeaderCell>
                                </asp:TableHeaderRow>
                            </asp:Table>
                        </div>
                    </div>
                </asp:Panel>
                <%--END LOW STOCK WARNING--%>

                    
                    <%-- STATISTICS --%>
                    <asp:Panel runat="server" ID="pnlStatistics">
                        <div id="divStats" runat="server" style="padding-left: 30px; padding-top: 20px">
                            <table style="width: 100%" border="0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td class="splash_StatisticsTitleHeadIcon">
                                    </td>
                                    <td>
                                        <div style="height: 10px">
                                        </div>
                                        <div style="float: left; width: 24px; height: 24px">
                                            <asp:Image ID="imgMinimize_Statistics" runat="server" class="splash_ImageButton"
                                                ImageUrl="~/App_Themes/Admin_Default/images/icon_minimize.jpg" min="~/App_Themes/Admin_Default/images/icon_minimize.jpg"
                                                min_hover="~/App_Themes/Admin_Default/images/icon_minimize_hover.jpg" restore="~/App_Themes/Admin_Default/images/icon_restore.jpg"
                                                restore_hover="~/App_Themes/Admin_Default/images/icon_restore_hover.jpg" />
                                        </div>
                                        <div class="splash_groupsTitleHead">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.Statistics %>" />
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            <input type="hidden" runat="server" id="divStatistics_Content_Visible" name="divStatistics_Content_Visible"
                                value="1" />
                            <div id="divStatistics_Content" runat="server">
                                <table id="StatsTable" runat="server" border="0" cellpadding="1" cellspacing="0"
                                    class="outerTable" width="100%">
                                    <tr>
                                        <td style="padding-left: 50px; padding-right: 60px">
                                            <div style="margin-bottom: 0px; text-align: left; margin-right: 2px;">
                                                <table style="width: 100%" cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td style="width: 100%">
                                                            <asp:GridView ID="gridCustomerStats" runat="server" AutoGenerateColumns="False" Width="100%"
                                                                GridLines="None" ShowFooter="True" CellPadding="0">
                                                                <Columns>
                                                                    <asp:TemplateField>
                                                                        <HeaderStyle Width="1%" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.CustomerStatHeader %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "CustomerType") %></ItemTemplate>
                                                                        <HeaderStyle Width="23%" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.CustomerStatToday %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "Today") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.CustomerStatThisWeek %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisWeek") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.CustomerStatThisMonth %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisMonth") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.CustomerStatThisYear %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisYear") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.CustomerStatAllTime %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "AllTime") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField>
                                                                        <HeaderStyle Width="1%" />
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                                <FooterStyle CssClass="gridFooter" />
                                                                <RowStyle CssClass="gridRow" />
                                                                <HeaderStyle CssClass="gridHeader" />
                                                                <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                                            </asp:GridView>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="width: 100%; height: 21px;">
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:GridView ID="gridAuthorized" runat="server" AutoGenerateColumns="False" Width="100%"
                                                                GridLines="None" ShowFooter="True" CellPadding="0">
                                                                <Columns>
                                                                    <asp:TemplateField>
                                                                        <HeaderStyle Width="1%" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersAuthorizedState %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "OrderSummaryName")%></ItemTemplate>
                                                                        <HeaderStyle Width="23%" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersAuthorizedStateToday %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "Today") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersAuthorizedStateThisWeek %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisWeek") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersAuthorizedStateThisMonth %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisMonth") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersAuthorizedStateThisYear %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisYear") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersAuthorizedStateAllTime %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "AllTime") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField>
                                                                        <HeaderStyle Width="1%" />
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                                <RowStyle CssClass="gridRow" />
                                                                <HeaderStyle CssClass="gridHeader" />
                                                                <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                                                <FooterStyle CssClass="gridFooter" />
                                                            </asp:GridView>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="height: 21px">
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:GridView ID="gridCaptured" runat="server" AutoGenerateColumns="False" Width="100%"
                                                                GridLines="None" ShowFooter="True" CellPadding="0">
                                                                <Columns>
                                                                    <asp:TemplateField>
                                                                        <HeaderStyle Width="1%" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersCapturedState %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "OrderSummaryName")%></ItemTemplate>
                                                                        <HeaderStyle Width="23%" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersCapturedStateToday %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "Today") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersCapturedStateThisWeek %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisWeek") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersCapturedStateThisMonth %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisMonth") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersCapturedStateThisYear %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisYear") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersCapturedStateAllTime %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "AllTime") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField>
                                                                        <HeaderStyle Width="1%" />
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                                <RowStyle CssClass="gridRow" />
                                                                <HeaderStyle CssClass="gridHeader" />
                                                                <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                                                <FooterStyle CssClass="gridFooter" />
                                                            </asp:GridView>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="height: 21px">
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:GridView ID="gridVoided" runat="server" AutoGenerateColumns="False" Width="100%"
                                                                GridLines="None" ShowFooter="True" CellPadding="0">
                                                                <Columns>
                                                                    <asp:TemplateField>
                                                                        <HeaderStyle Width="1%" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.Voided %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "OrderSummaryName")%></ItemTemplate>
                                                                        <HeaderStyle Width="23%" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.VoidedToday %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "Today") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.VoidedThisWeek %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisWeek") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.VoidedThisMonth %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisMonth") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.VoidedThisYear %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "ThisYear") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.VoidedAllTime %>">
                                                                        <ItemTemplate>
                                                                            <%# DataBinder.Eval(Container.DataItem, "AllTime") %></ItemTemplate>
                                                                        <ItemStyle HorizontalAlign="Right" />
                                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField>
                                                                        <HeaderStyle Width="1%" />
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                                <RowStyle CssClass="gridRow" />
                                                                <HeaderStyle CssClass="gridHeader" />
                                                                <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                                                <FooterStyle CssClass="gridFooter" />
                                                            </asp:GridView>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="height: 15px">
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                            <asp:GridView ID="gridRefunded" runat="server" AutoGenerateColumns="False" Width="100%"
                                                GridLines="None" ShowFooter="True" CellPadding="0">
                                                <Columns>
                                                    <asp:TemplateField>
                                                        <HeaderStyle Width="1%" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersRefundedState %>">
                                                        <ItemTemplate>
                                                            <%# DataBinder.Eval(Container.DataItem, "OrderSummaryName")%></ItemTemplate>
                                                        <HeaderStyle Width="23%" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersRefundedStateToday %>">
                                                        <ItemTemplate>
                                                            <%# DataBinder.Eval(Container.DataItem, "Today") %></ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Right" />
                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersRefundedStateThisWeek %>">
                                                        <ItemTemplate>
                                                            <%# DataBinder.Eval(Container.DataItem, "ThisWeek") %></ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Right" />
                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersRefundedStateThisMonth %>">
                                                        <ItemTemplate>
                                                            <%# DataBinder.Eval(Container.DataItem, "ThisMonth") %></ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Right" />
                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersRefundedStateThisYear %>">
                                                        <ItemTemplate>
                                                            <%# DataBinder.Eval(Container.DataItem, "ThisYear") %></ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Right" />
                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.OrdersRefundedStateAllTime %>">
                                                        <ItemTemplate>
                                                            <%# DataBinder.Eval(Container.DataItem, "AllTime") %></ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Right" />
                                                        <HeaderStyle HorizontalAlign="Right" Width="15%" Font-Bold="False" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField>
                                                        <HeaderStyle Width="1%" />
                                                    </asp:TemplateField>
                                                </Columns>
                                                <RowStyle CssClass="gridRow" />
                                                <HeaderStyle CssClass="gridHeader" />
                                                <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                                <FooterStyle CssClass="gridFooter" />
                                            </asp:GridView>
                                            &nbsp;
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </asp:Panel>
                    <%-- END STATISTICS --%>
                    
                    <%-- Security Alert --%>
                    <%--<div style="padding-left: 30px; padding-top : 20px">
                        <table style="width: 100%" border="0" cellpadding="0" cellspacing="0">
                            <tr>
                                <td class="splash_NewsTitleHeadIcon"></td>
                                <td>
                                    <div style="height: 10px"></div>
                                    <div style="float: left; width : 24px; height: 24px">
                                        <img id="imgMinimize_SecurityAlert" runat="server"  class="splash_ImageButton"
                                            src="~/App_Themes/Admin_Default/images/icon_minimize.jpg" 
                                            src_hover="~/App_Themes/Admin_Default/images/icon_minimize_hover.jpg"
                                            restore="~/App_Themes/Admin_Default/images/icon_restore.jpg"
                                            restore_hover="~/App_Themes/Admin_Default/images/icon_restore_hover.jpg"/>
                                    </div>
                                    <div class="splash_groupsTitleHead">                                                                                
                                        Security Alert
                                    </div>     
                                    <div id="divSecurityAlert_Contents" runat="server">
                                        <asp:Panel ID="pnlSecurityFeed" runat="server" Visible="true" Width="348px" BorderColor="#b7ccfb" >   
                                        <aspdnsf:XmlPackage id="SecurityFeed" runat="server" PackageName="rss.aspdnsfrssconsumer.xml.config" RunTimeParams="channel=security"/>
                                    </asp:Panel>                                     
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>--%>
                    <%-- End Security Alert --%>
                    <%-- LATEST ORDERS --%>
                    <asp:Panel runat="server" ID="pnlOrders">
                        <div style="padding-left: 30px; padding-top: 20px">
                            <table style="width: 100%" border="0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td class="splash_latestOrdersTitleHeadIcon">
                                    </td>
                                    <td>
                                        <div style="height: 10px">
                                        </div>
                                        <div style="float: left; width: 24px; height: 24px">
                                            <asp:Image ID="imgMinimize_LatestOrders" runat="server" class="splash_ImageButton"
                                                ImageUrl="~/App_Themes/Admin_Default/images/icon_minimize.jpg" min="~/App_Themes/Admin_Default/images/icon_minimize.jpg"
                                                min_hover="~/App_Themes/Admin_Default/images/icon_minimize_hover.jpg" restore="~/App_Themes/Admin_Default/images/icon_restore.jpg"
                                                restore_hover="~/App_Themes/Admin_Default/images/icon_restore_hover.jpg" />
                                        </div>
                                        <div class="splash_groupsTitleHead">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.LatestOrders %>" />
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <input type="hidden" runat="server" id="divLatestOrders_Content_Visible" name="divLatestOrders_Content_Visible"
                            value="1" />
                        <div id="divLatestOrders_Content" runat="server">
                            <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                <tr>
                                    <td class="splash_divLatestOrders">
                                        <a href="orders.aspx">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewOrders %>" /></a>
                                        &nbsp;<a href="reports.aspx"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.OrderReports %>" /></a>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="splash_divLatestOrders">
                                        <asp:GridView ID="gOrders" AutoGenerateColumns="False" ShowFooter="True" runat="server"
                                            OnRowDataBound="gOrders_RowDataBound" Width="100%" GridLines="None" CellPadding="0">
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="DataCellGridEdit" />
                                            <PagerStyle CssClass="pagerGrid" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                            <Columns>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersOrder %>">
                                                    <ItemTemplate>
                                                        <%# "<a href=\"orders.aspx?ordernumber=" + DataBinder.Eval(Container.DataItem, "OrderNumber") + "\">" + DataBinder.Eval(Container.DataItem, "OrderNumber") + "</a>" %></ItemTemplate>
                                                    <ItemStyle CssClass="lighterData" Width="60px" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersDate %>">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "OrderDate") %></ItemTemplate>
                                                    <ItemStyle CssClass="lightData" Width="150px" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersCustomer %>">
                                                    <ItemTemplate>
                                                        <%# (DataBinder.Eval(Container.DataItem, "FirstName") + " " + DataBinder.Eval(Container.DataItem, "LastName")).Trim() %></ItemTemplate>
                                                    <ItemStyle CssClass="normalData" Width="160px" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersShipping %>">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "ShippingMethod") %></ItemTemplate>
                                                    <ItemStyle CssClass="normalData" Width="400px" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersTotal %>">
                                                    <ItemTemplate>
                                                        <%# AspDotNetStorefrontCore.Localization.CurrencyStringForDisplayWithoutExchangeRate((decimal)DataBinder.Eval(Container.DataItem, "OrderTotal"),false) %></ItemTemplate>
                                                    <ItemStyle CssClass="normalData" Width="80px" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestOrdersMaxMind %>">
                                                    <ItemTemplate>
                                                        <%# AspDotNetStorefrontCore.Localization.DecimalStringForDB((decimal)DataBinder.Eval(Container.DataItem, "MaxMindFraudScore")) %></ItemTemplate>
                                                    <ItemStyle CssClass="normalData" HorizontalAlign="Right" Width="50px" />
                                                    <HeaderStyle HorizontalAlign="Right" />
                                                </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </asp:Panel>
                    <%-- END LATEST ORDERS --%>
                    <%-- LATEST REGISTERED CUSTOMERS --%>
                    <asp:Panel runat="server" ID="pnlCustomers">
                        <div style="padding-left: 30px; padding-top: 20px">
                            <table style="width: 100%" border="0" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td class="splash_latestRegisteredTCustomersTitleHeadIcon">
                                    </td>
                                    <td>
                                        <div style="height: 10px">
                                        </div>
                                        <div style="float: left; width: 24px; height: 24px">
                                            <asp:Image ID="imgMinimize_LatestRegisteredCustomers" runat="server" class="splash_ImageButton"
                                                ImageUrl="~/App_Themes/Admin_Default/images/icon_minimize.jpg" min="~/App_Themes/Admin_Default/images/icon_minimize.jpg"
                                                min_hover="~/App_Themes/Admin_Default/images/icon_minimize_hover.jpg" restore="~/App_Themes/Admin_Default/images/icon_restore.jpg"
                                                restore_hover="~/App_Themes/Admin_Default/images/icon_restore_hover.jpg" />
                                        </div>
                                        <div class="splash_groupsTitleHead">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.LatestRegisteredCustomers %>" />
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <input type="hidden" runat="server" id="divLatestRegisteredCustomers_Content_Visible"
                            name="divLatestRegisteredCustomers_Content_Visible" value="1" />
                        <div id="divLatestRegisteredCustomers_Content" runat="server">
                            <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                <tr>
                                    <td class="splash_divLatestCustomers">
                                        <a href="customers.aspx">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ViewCustomers %>" /></a>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="splash_divLatestCustomers">
                                        <asp:GridView ID="gCustomers" AutoGenerateColumns="false" ShowHeader="true" ShowFooter="True"
                                            runat="server" AllowPaging="false" AllowSorting="false" Width="100%" GridLines="None"
                                            CellPadding="0">
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="DataCellGridEdit" />
                                            <PagerStyle CssClass="pagerGrid" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                            <Columns>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestCustomerCustomerID %>">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "CustomerID") %></ItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestCustomerDate %>">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "RegisterDate") %></ItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.default.LatestCustomerCustomer %>">
                                                    <ItemTemplate>
                                                        <%# (DataBinder.Eval(Container.DataItem, "FirstName") + " " + DataBinder.Eval(Container.DataItem, "LastName")).Trim() %></ItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </asp:Panel>
                    <%-- END LATEST REGISTERED CUSTOMERS --%>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
