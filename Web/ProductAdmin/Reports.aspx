<%@ Page Language="C#" AutoEventWireup="true" Title="Version Info" CodeFile="Reports.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Reports" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="ew" Namespace="eWorld.UI" Assembly="eWorld.UI" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="aspdnsf" TagName="TopicToStore" Src="controls/StoreSelector.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="GeneralInfo" Src="controls/GeneralInfo.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable" width="300">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.type %>" /></font>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #EBEBEB;" />
                                <td style="width: 5px;" />
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.choosereport %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top">
                                    <div class="wrapperTopBottom" style="width:250px;">
                                       <!-- Report Controls -->
                                       <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                            <tr>
                                            <td>&nbsp;</td>
                                            <td>
                                                <br />
                                                <%--Report Step 1 - Add your report to the report list. --%>
                                               <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.builtin %>" /></font><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo1" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.abandonedcartdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.abandonedcartlabel %>" ID="rblAbandonedCart" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo2" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.inventorylevelsdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.inventorylevelslabel %>" ID="rblInventoryLevels" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo3" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.customersdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.customerslabel %>" ID="rblCustomers" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo4" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.promotionusagedescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.promotionsusagelabel %>" ID="rblPromotions" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo5" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.emptyentitiesdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.emptyentitieslabel %>" ID="rblEmptyEntities" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo6" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.unmappedproductsdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.unmappedproductslabel %>" ID="rblUnmappedProducts" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo7" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.affiliatesdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.affiliateslabel %>" ID="rblAffiliates" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo8" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.referralsdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.referralslabel %>" ID="rblReferrals" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo9" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.ordersbyentitydescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.ordersbyentitylabel %>" ID="rblOrdersByEntity" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo10" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.top10description %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.top10label %>" ID="rblBestsellers" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo11" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.customerswhoboughtdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.customerswhoboughtlabel %>" ID="rblCustomersByProduct" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo12" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.currentrecurringdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.currentrecurringlabel %>" ID="rblCurrentRecurring" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo13" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.cancelledrecurringdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.cancelledrecurringlabel %>" ID="rblCancelledRecurring" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo14" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.customerlevelproductsdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.customerlevelproductslabel %>" ID="rblProductsByCustomerLevel" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo15" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.ordersbydaterangedescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.ordersbydaterange %>" ID="rblOrdersByDateRange" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />
                                                <aspdnsf:GeneralInfo ID="GeneralInfo16" runat="server" DefaultText="<%$Tokens:StringResource, admin.reports.ordersbyitemdescription %>" />
                                                <asp:RadioButton runat="server" AutoPostBack="true" Text="<%$Tokens:StringResource, admin.reports.ordersbyitem %>" ID="rblOrdersByItem" OnCheckedChanged="BuiltInReport_Clicked" GroupName="rblReportType" /><br />

                                                <br />
                                                <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.custom %>" /></font><br />
                                                <asp:RadioButtonList runat="server" ID="RadioReportType" AutoPostBack="true" CellPadding="0" CellSpacing="0" RepeatColumns="1" OnSelectedIndexChanged="CustomReport_Clicked">
                                                    <%--This will be populated with custom reports (if any) from the DB --%>
                                                </asp:RadioButtonList>
                                                <br />
                                                <br />
                                            </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <asp:Panel ID="pnlStores" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable">
                                                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.forstore %>" /></font><br />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <aspdnsf:TopicToStore runat="server" ShowText="false" SelectMode="SingleDropDown" ShowDefaultForAllStores="true"  ID="ssOne" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnlDateSpecs" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable" colspan="2">
                                                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.specs %>" /></font>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" valign="top">
                                                                    <div class="wrapperBottom">
                                                                        <font class="titleMessage"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.selectdates %>" /></font>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.startdate %>" /></font>
                                                                </td>
                                                                <td align="left" valign="top">
                                                                    <telerik:RadDatePicker ID="dateStart" runat="server" Style="z-index: 150000;">
                                                                        <Calendar ID="Calendar1" runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                    </telerik:RadDatePicker>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.enddate %>" /></font>
                                                                </td>
                                                                <td align="left" valign="top">
                                                                    <telerik:RadDatePicker ID="dateEnd" runat="server" Style="z-index: 150000;">
                                                                        <Calendar ID="Calendar2" runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                    </telerik:RadDatePicker>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" valign="top">
                                                                    <div class="wrapperTopBottom">
                                                                        <font class="titleMessage"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.selectrange %>" /></font>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">&nbsp;</font>
                                                                </td>
                                                                <td align="left" valign="top">
                                                                    <asp:RadioButtonList runat="server" ID="rblRange" CellPadding="0" CellSpacing="0" RepeatColumns="1">
                                                                        <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.reports.datesabove %>" Selected="true" />
                                                                        <asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.reports.today %>" />
                                                                        <asp:ListItem Value="2" Text="<%$Tokens:StringResource, admin.reports.yesterday %>" />
                                                                        <asp:ListItem Value="3" Text="<%$Tokens:StringResource, admin.reports.thisweek %>" />
                                                                        <asp:ListItem Value="4" Text="<%$Tokens:StringResource, admin.reports.lastweek %>" />
                                                                        <asp:ListItem Value="5" Text="<%$Tokens:StringResource, admin.reports.thismonth %>" />
                                                                        <asp:ListItem Value="6" Text="<%$Tokens:StringResource, admin.reports.lastmonth %>" />
                                                                        <asp:ListItem Value="7" Text="<%$Tokens:StringResource, admin.reports.thisyear %>" />
                                                                        <asp:ListItem Value="8" Text="<%$Tokens:StringResource, admin.reports.lastyear %>" />
                                                                    </asp:RadioButtonList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" valign="top">
                                                                    <hr noshade="noshade" width="100%" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    <%--Input Parameter Step 1 - Add your panel options as an invisible panel.--%>
                                                    <asp:Panel ID="pnlProductId" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable">
                                                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.productID %>" /></font><br />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:TextBox ID="txtProductId" runat="server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnlQuantity" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable">
                                                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.inventorylevel %>" /></font><br />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:TextBox ID="txtQuantity" runat="server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnlCustomerType" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable">
                                                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.customertype %>" /></font><br />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:DropDownList ID="ddCustomerType" runat="server">
                                                                        <asp:ListItem Text="<%$Tokens:StringResource, admin.reports.both %>" Value="Both" Selected="True"></asp:ListItem>
                                                                        <asp:ListItem Text="<%$Tokens:StringResource, admin.reports.registered %>" Value="Registered"></asp:ListItem>
                                                                        <asp:ListItem Text="<%$Tokens:StringResource, admin.reports.unregistered %>" Value="Unregistered"></asp:ListItem>
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnlEntityTypes" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable">
                                                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.entitytype %>" /></font><br />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:DropDownList ID="ddEntity" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddEntity_SelectedIndexChanged">
                                                                        <asp:ListItem Text="<%$Tokens:StringResource, admin.reports.categories %>" Value="Category" Selected="True"></asp:ListItem>
                                                                        <asp:ListItem Text="<%$Tokens:StringResource, admin.reports.manufacturers %>" Value="Manufacturer"></asp:ListItem>
                                                                        <asp:ListItem Text="<%$Tokens:StringResource, admin.reports.sections %>" Value="Section"></asp:ListItem>
                                                                        <asp:ListItem Text="<%$Tokens:StringResource, admin.reports.stores %>" Value="Stores"></asp:ListItem>
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnlAffiliates" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable">
                                                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.affiliate %>" /></font><br />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:DropDownList ID="ddAffiliates" runat="server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    <asp:Panel ID="pnlEntities" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable">
                                                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.chooseentity %>" /></font><br />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                <asp:Literal ID="litCategoriesdd" runat="server" Text="Categories:"></asp:Literal><br />
                                                                    <asp:DropDownList ID="ddCategories" runat="server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                <asp:Literal ID="liteManufacturersdd" runat="server" Text="Manufacturers:"></asp:Literal><br />
                                                                    <asp:DropDownList ID="ddManufacturers" runat="server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                <asp:Literal ID="litSectionsdd" runat="server" Text="Departments:"></asp:Literal><br />
                                                                    <asp:DropDownList ID="ddSections" runat="server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>

                                                    <asp:Panel ID="pnlCustomerLevels" runat="server" Visible="false">
                                                        <table width="100%">
                                                            <tr>
                                                                <td class="titleTable">
                                                                    <font class="subTitle"><asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.reports.customerlevel %>" /></font><br />
                                                                </td>
                                                            </tr>
                                                           <tr>
                                                                <td>
                                                                    <asp:Literal ID="litCustomerLevelsdd" runat="server" Text="Customer Level:"></asp:Literal><br />
                                                                    <asp:DropDownList ID="ddCustomerLevel" runat="server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                    
                                                </td>
                                            </tr>
                                       </table>
                                    </div>
                                    <div class="wrapperTop">
                                        <asp:Button runat="server" ID="btnReport" CssClass="normalButtons" Visible="false" Text="<%$Tokens:StringResource, admin.reports.getreport %>" OnClick="BtnReport_Click" CausesValidation="true" />
                                    </div>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #EBEBEB;" />
                                <td style="width: 5px;" />
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft" id="divReportResults" runat="server">
                                        <asp:Button id="btnSaveReport" runat="server" OnClick="BtnSaveReport_click" Text="<%$Tokens:StringResource, admin.reports.saveexcel %>"  Visible="false"  CausesValidation="true"/>
                                        <asp:PlaceHolder ID="phReportResults" runat="server">
                                        </asp:PlaceHolder>
                                        <asp:Panel ID="pnlSummary" runat="server" Visible="false">
                                            <table width="500px;">
                                                <tr>
                                                    <td><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.registeredcustomers %>" /></td>
                                                    <td><asp:Literal ID="litRegisteredCustomers" Text="" runat="server"></asp:Literal></td>
                                                </tr>
                                                <tr style="background-color: #EBEBEB;">
                                                    <td><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.anoncustomers %>" /></td>
                                                    <td><asp:Literal ID="litAnonymousCustomers" Text="" runat="server"></asp:Literal></td>
                                                </tr>
                                                <tr>
                                                    <td><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.numorders %>" /></td>
                                                    <td><asp:Literal ID="litNumberOrders" Text="" runat="server"></asp:Literal></td>
                                                </tr>
                                                <tr style="background-color: #EBEBEB;">
                                                    <td><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.ordertotals %>" /></td>
                                                    <td><asp:Literal ID="litOrderTotals" Text="" runat="server"></asp:Literal></td>
                                                </tr>
                                                <tr>
                                                    <td><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.ordersubtotals %>" /></td>
                                                    <td><asp:Literal ID="litOrderSubtotals" Text="" runat="server"></asp:Literal></td>
                                                </tr>
                                                <tr style="background-color: #EBEBEB;">
                                                    <td><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.ordershipping %>" /></td>
                                                    <td><asp:Literal ID="litOrderShipping" Text="" runat="server"></asp:Literal></td>
                                                </tr>
                                                <tr>
                                                    <td><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.ordertax %>" /></td>
                                                    <td><asp:Literal ID="litOrderTax" Text="" runat="server"></asp:Literal></td>
                                                </tr>
                                                <tr style="background-color: #EBEBEB;">
                                                    <td><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.reports.averageorder %>" /></td>
                                                    <td><asp:Literal ID="litAverageOrder" Text="" runat="server"></asp:Literal></td>
                                                </tr>
                                            </table>
                                        </asp:Panel>
                                        
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