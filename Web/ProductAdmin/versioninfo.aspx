<%@ Page Language="C#" AutoEventWireup="true" Title="Version Info" CodeFile="versioninfo.aspx.cs" Inherits="AspDotNetStorefrontAdmin.versioninfo" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<div style="position:relative;">
    <div style="text-align:left;position:absolute;left:350px;top:-15px;">
        <asp:Button ID="btnExport" runat="server" OnClick="btnExport_Click" Text="Export Results" />
    </div>
    <div ID="divReports" runat="server">
        <table style="padding:0;margin:0;" cellpadding="0" cellspacing="0">
            <tr>
                <td class="splash_LeftPaneBgColor" style="padding:20px;padding-right:0;height:auto;" valign="top">
                    <%-- PATCH INFORMATION --%>
                            <asp:Panel runat="server" ID="pnlPatchInfo" Visible="false">
                                <div class="splash_GroupHeader">
                                    <table style="width: 100%; height: 100%; border: 0;" cellpadding="0" cellspacing="0">
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
                                                <asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.default.SystemInformation %>" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                                <div class="splash_sysInfoBody">
                                    <table width="100%" cellpadding="1" cellspacing="1" border="0">
                                        <tr>
                                            <td width="180" align="right" valign="top">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.default.Version %>" /></font>
                                            </td>
                                            <td valign="top">
                                                <asp:Literal ID="ltStoreVersion" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label3" runat="server" Text="<%$Tokens:StringResource, admin.default.CurrentServerDateTime %>" /></font>
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
                                                    <asp:Label ID="Label4" runat="server" Text="<%$Tokens:StringResource, admin.default.ExecutionMode %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltExecutionMode" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label5" runat="server" Text="<%$Tokens:StringResource, admin.default.UseSSL %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltUseSSL" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label6" runat="server" Text="<%$Tokens:StringResource, admin.default.OnLiveServer %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltOnLiveServer" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label7" runat="server" Text="<%$Tokens:StringResource, admin.default.IsSecureConnection %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltServerPortSecure" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label10" runat="server" Text="<%$Tokens:StringResource, admin.default.PrimaryStoreLocaleSetting %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltWebConfigLocaleSetting" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label11" runat="server" Text="<%$Tokens:StringResource, admin.default.SQLLocaleSetting %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltSQLLocaleSetting" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label12" runat="server" Text="<%$Tokens:StringResource, admin.default.CustomerLocaleSetting %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltCustomerLocaleSetting" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label13" runat="server" Text="<%$Tokens:StringResource, admin.default.PrimaryStoreCurrency %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="PrimaryCurrency" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label14" runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentGateway %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltPaymentGateway" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label15" runat="server" Text="<%$Tokens:StringResource, admin.default.GatewayMode %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltUseLiveTransactions" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label16" runat="server" Text="<%$Tokens:StringResource, admin.default.TransactionMode %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltTransactionMode" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" style="white-space: nowrap" valign="top">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label17" runat="server" Text="<%$Tokens:StringResource, admin.default.PaymentMethods %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltPaymentMethods" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr id="trMicropay" runat="server">
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label18" runat="server" Text="<%$Tokens:StringResource, admin.default.MicroPayEnabled %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="ltMicroPayEnabled" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr id="trCardinal" runat="server">
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label19" runat="server" Text="<%$Tokens:StringResource, admin.default.CardinalEnabled %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="CardinalEnabled" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr id="trStoreCC" runat="server">
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label20" runat="server" Text="<%$Tokens:StringResource, admin.default.StoreCreditCards %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="StoreCC" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr id="trGatewayRec" runat="server">
                                            <td align="right" style="white-space: nowrap">
                                                <font class="subTitleSmall">
                                                    <asp:Label ID="Label21" runat="server" Text="<%$Tokens:StringResource, admin.default.UsingGatewayRecurringBilling %>" /></font>
                                            </td>
                                            <td>
                                                <asp:Literal ID="GatewayRecurringBilling" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                                
                            </div>
                            <%-- END SYSTEM INFORMATION --%>
                </td>
                <td>
                    <asp:Literal runat="server" ID="litAssemblyInfo" />
                    <asp:Literal runat="server" ID="litFileInfo" />
                    <asp:Literal runat="server" ID="litConfigInfo" />
                </td>
            </tr>
        </table>
    </div>
    </div>
</asp:Content>