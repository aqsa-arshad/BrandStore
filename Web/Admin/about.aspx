<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.about" CodeFile="about.aspx.cs" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"  %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
            <tr>
                <td class="contentTable">
                    <div>
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td style="height: 15px" width="100%">
                                </td>
                            </tr>
                            <tr>
                                <td class="titleTable" width="100%">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.SystemInformation %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" width="100%">
                                    <div class="divBox">
                                        <table width="100%" cellpadding="1" cellspacing="1" border="0">
                                            <tr>
                                                <td width="250" align="right" valign="top">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.VersionCodeDB %>" /></font></td>
                                                <td valign="top">
                                                    <asp:Literal ID="ltStoreVersion" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CurrentServerDateTime %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltDateTime" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.ExecutionMode %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltExecutionMode" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.OnLiveServer %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltOnLiveServer" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.UseSSL %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltUseSSL" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.IsSecureConnection %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltServerPortSecure" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.CachingIs %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltCaching" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.AdminDirectoryChanged %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltAdminDirChanged" runat="server"></asp:Literal></td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
            <tr>
                <td class="contentTable">
                    <div>
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td style="height: 15px" width="100%">
                                </td>
                            </tr>
                            <tr>
                                <td class="titleTable" width="100%">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.LicenseInformation %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" width="100%">
                                    <div class="divBox">
                                        <table width="100%" cellpadding="1" cellspacing="1" border="0">
                                           	<tr>
                                                <td width="250" align="right" valign="top"></td>
                                                <td valign="top">
                                                    <asp:Literal ID="pnlLicenseInformation" runat="server"></asp:Literal></td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
            <tr>
                <td class="contentTable">
                    <div>
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td style="height: 15px" width="100%">
                                </td>
                            </tr>
                            <tr>
                                <td class="titleTable" width="100%">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.LocalizationInformation %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" width="100%">
                                    <div class="divBox">
                                        <table width="100%" cellpadding="1" cellspacing="1" border="0">
                                            <tr>
                                                <td width="250" align="right" valign="top">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.PrimaryStoreLocaleSetting %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltWebConfigLocaleSetting" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.SQLLocaleSetting %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltSQLLocaleSetting" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.CustomerLocaleSetting %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltCustomerLocaleSetting" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.splash.aspx.sysinfo.PrimaryStoreCurrency %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltPrimaryCurrency" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.LocalizationStoreCurrency %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltLocalizationCurrencyCode" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.LocalizationStoreCurrencyCode %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltLocalizationCurrencyNumericCode" runat="server"></asp:Literal></td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
            <tr>
                <td class="contentTable">
                    <div>
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td style="height: 15px" width="100%">
                                </td>
                            </tr>
                            <tr>
                                <td class="titleTable" width="100%">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.GatewayInformation %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" width="100%">
                                    <div class="divBox">
                                        <table width="100%" cellpadding="1" cellspacing="1" border="0">
                                            <tr>
                                                <td width="250" align="right" valign="top">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentGateway %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltPaymentGateway" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.GatewayMode %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltUseLiveTransactions" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.TransactionMode %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltTransactionMode" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentMethods %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltPaymentMethods" runat="server"></asp:Literal></td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
            <tr>
                <td class="contentTable">
                    <div>
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td style="height: 15px" width="100%">
                                </td>
                            </tr>
                            <tr>
                                <td class="titleTable" width="100%">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.ShippingInformation %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" width="100%">
                                    <div class="divBox">
                                        <table width="100%" cellpadding="1" cellspacing="1" border="0">
                                            <tr>
                                                <td width="250" align="right" valign="top">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.ShippingCalculation %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltShippingCalculation" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.OriginState %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltOriginState" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.OriginZip %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltOriginZip" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.OriginCountry %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltOriginCountry" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.FreeShippingThreshold %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltFreeShippingThreshold" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.FreeShippingMethods %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltFreeShippingMethods" runat="server"></asp:Literal></td>
                                            </tr>
                                            <tr>
                                                <td align="right">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.about.FreeShippingRateSelection %>" /></font></td>
                                                <td>
                                                    <asp:Literal ID="ltFreeShippingRateSelection" runat="server"></asp:Literal></td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    
</asp:Content>