<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PromotionEditor.aspx.cs" Inherits="_PromotionEditor" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="GeneralInfo" Src="controls/GeneralInfo.ascx" %>
<%@ Register TagPrefix="ComponentArt" Namespace="ComponentArt.Web.UI" Assembly="ComponentArt.Web.UI" %>
<%@ Register TagPrefix="aspdnsf" TagName="CSVHelper" Src="controls/CSVHelper.ascx" %>
<%@ Register TagPrefix="aspdnsfs" TagName="StoreSelector" Src="Controls/StoreSelector.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="head">
    <link type="text/css" href="css/redmond/jquery-ui-1.8.17.custom.css" rel="stylesheet" />
    <link type="text/css" href="css/promotions.css" rel="stylesheet" />
    <script type="text/javascript" src="Scripts/jquery.min.js"></script>
    <script type="text/javascript" src="Scripts/jquery-ui-1.8.17.custom.min.js"></script>
    <script type="text/javascript" src="Scripts/promotions.js"></script>
</asp:Content>

<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div class="bodywrapper ui-widget">
        <asp:Panel ID="pnlView" runat="server" DefaultButton="btnSearch">
            <asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.Search%>" />
            <asp:TextBox ID="txtKeyword" runat="server" CssClass="textfield short" />
            <asp:Button ID="btnSearch" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.Search%>" OnClick="btnSearch_Click" CssClass="button" />
            <asp:CheckBox ID="chkActiveFilter" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.ActiveHeaderText%>" />
            <asp:CheckBox ID="chkAutoAssignedFilter" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.AutoAssignedHeaderText%>" />
            <br />
            <br />
            <asp:GridView ID="gridPromotions" runat="server" DataSourceID="PromotionDataSource"
                DataKeyNames="Id" AutoGenerateColumns="false" AllowPaging="true" AllowSorting="true"
                PageSize="10" OnPageIndexChanged="gridPromotions_PageChanged" OnSelectedIndexChanged="gridPromotions_SelectedIndexChanged" CellPadding="5"
                CellSpacing="0" AlternatingRowStyle-BackColor="#E2E8FA" AlternatingRowStyle-CssClass="promoGridAltRow" 
                RowStyle-CssClass="promoGridRow" BorderWidth="1" CssClass="promoGridTable">
                <Columns>
                    <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.NameHeaderText%>" DataField="Name" SortExpression="Name" HeaderStyle-HorizontalAlign="Left" HeaderStyle-Width="150" />
                    <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.CodeHeaderText%>" DataField="Code" SortExpression="Code" HeaderStyle-HorizontalAlign="Left" HeaderStyle-Width="55" />
                    <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.PriorityHeaderText%>" DataField="Priority" SortExpression="Priority" />
                    <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.ActiveHeaderText%>" DataField="Active" SortExpression="Active" />
                    <asp:BoundField HeaderText="<%$Tokens:StringResource, admin.PromotionEditor.AutoAssignedHeaderText%>" DataField="AutoAssigned" SortExpression="AutoAssigned" />
                    <asp:TemplateField ShowHeader="false">
                        <ItemTemplate>
                            <asp:Button ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Select" Text="<%$Tokens:StringResource, admin.PromotionEditor.EditButton%>" CssClass="button" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:LinqDataSource ID="PromotionDataSource" runat="server" ContextTypeName="AspDotNetStorefront.Promotions.Data.EntityContextDataContext, AspDotNetStorefrontPromotions" TableName="Promotions" AutoPage="true" AutoSort="true" EnableDelete="true" EnableInsert="true" EnableUpdate="true" />
            <br />
            <asp:Button ID="btnAdd" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.AddButton%>" OnClick="btnAdd_Click" CausesValidation="false" CssClass="button" />
        </asp:Panel>
        <asp:Panel ID="pnlUpdate" runat="server" Visible="false">
            <fieldset>
                <legend><asp:Literal ID="litPromotionLegend" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.PromotionLegend%>" /><asp:Label ID="lblTitle" runat="server" /></legend>
                <table width="100%">
                    <tr>
                        <td class="tdButtonRow" style="text-align:left;">
                            <asp:Button ID="btnUpdate" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.UpdateButton%>" OnClick="btnUpdate_Click" CssClass="button" />
                            <asp:Button ID="btnDelete" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.DeleteButton%>" OnClick="btnDelete_Click" CssClass="button" CausesValidation="false" OnClientClick="return confirm('Are you sure you want to delete this promotion?');" />
                            <asp:Button ID="btnCancel" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.CancelButton%>" OnClick="btnCancel_Click" CausesValidation="false" CssClass="button" />
                        </td>
                        <td class="tdButtonRow" style="text-align:left;">
							<input runat="server" type="button" value="<%$Tokens:StringResource, admin.PromotionEditor.ExpandAllButton%>" class="button expandAllExpandables" />
							<input runat="server" type="button" value="<%$Tokens:StringResource, admin.PromotionEditor.CollapseAllButton%>" class="button collapseAllExpandables" />
						</td>
                    </tr>
                    <tr>
                        <td valign="top" style="width:400px;">
                            <table class="promoFieldTable">
                                <tr>
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo6" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoTitle%>" />
                                    </td>
                                    <td class="tdLabel">                                        
                                        <asp:Label ID="lblText" Text="<%$Tokens:StringResource, admin.PromotionEditor.TitleLabel%>" AssociatedControlID="txtName" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <asp:TextBox ID="txtName" runat="server" CssClass="textfield" Columns="60" />
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtName" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationStar%>" Display="Dynamic" EnableClientScript="true" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo21" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoCode%>" />
                                    </td>
                                    <td class="tdLabel">
                                        <asp:Label ID="lblCode" Text="<%$Tokens:StringResource, admin.PromotionEditor.CodeLabel%>" AssociatedControlID="txtCode" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <asp:TextBox ID="txtCode" runat="server" CssClass="textfield" Columns="60" />
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCode" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationStar%>" Display="Dynamic" EnableClientScript="true" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo22" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoDescription%>" />
                                    </td>
                                    <td class="tdLabel">
                                        <asp:Label ID="lblDescription" Text="<%$Tokens:StringResource, admin.PromotionEditor.DescriptionLabel%>" AssociatedControlID="txtDescription" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <asp:TextBox ID="txtDescription" runat="server" CssClass="textfield" Columns="60" />
                                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDescription" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationStar%>" Display="Dynamic" EnableClientScript="true" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo23" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoPriority%>" />
                                    </td>
                                    <td class="tdLabel">
                                        <asp:Label ID="lblPriority" Text="<%$Tokens:StringResource, admin.PromotionEditor.PriorityLabel%>" AssociatedControlID="txtPriority" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <asp:TextBox ID="txtPriority" runat="server" CssClass="textfield" Columns="5" />                                        
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtPriority" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationStar%>" Display="Dynamic" EnableClientScript="true" />
                                        <asp:CompareValidator ID="cmpValTxtPriority" runat="server" ControlToValidate="txtPriority" ValueToCompare="0" Operator="GreaterThanEqual" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Display="Dynamic" Type="Integer" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo24" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoStatus%>" />
                                    </td>
                                    <td class="tdLabel">
                                        <asp:Label ID="lblChk" Text="<%$Tokens:StringResource, admin.PromotionEditor.ActiveLabel%>" AssociatedControlID="rblActive" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <asp:RadioButtonList runat="server" ID="rblActive" RepeatDirection="Horizontal" >
                                            <asp:ListItem Value="true" Text="Active" Selected="True"></asp:ListItem>
                                            <asp:ListItem Value="false" Text="Inactive"></asp:ListItem>
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo25" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoAutoAssigned%>" />
                                    </td>
                                    <td class="tdLabel">
                                        <asp:Label ID="lblAutoAssigned" Text="<%$Tokens:StringResource, admin.PromotionEditor.AutoAssignedLabel%>" AssociatedControlID="chkAutoAssigned" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <asp:CheckBox ID="chkAutoAssigned" runat="server" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo26" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoUsage%>" />
                                    </td>
                                    <td class="tdLabel">
                                        <asp:Label ID="lblUsage" Text="<%$Tokens:StringResource, admin.PromotionEditor.UsageLabel%>" AssociatedControlID="txtUsageText" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <asp:TextBox ID="txtUsageText" runat="server" CssClass="textfield" Columns="60" />                                        
                                    </td>
                                </tr>
                                <tr>
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo28" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoProductMessage%>" />
                                    </td>
                                    <td class="tdLabel">
                                        <asp:Label ID="lblCallToAction" Text="<%$Tokens:StringResource, admin.PromotionEditor.CallToActionLabel%>" AssociatedControlID="CallToActionTextbox" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <asp:TextBox ID="CallToActionTextbox" runat="server" CssClass="textfield" Columns="60" />
                                    </td>
                                </tr>
                                <tr runat="server" id="trStoreMapping">
                                    <td class="tdInfo">
                                        <aspdnsf:GeneralInfo ID="GeneralInfo29" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPromoStores%>" />
                                    </td>
                                    <td class="tdLabel">
                                        <asp:Label ID="lblStore" Text="<%$Tokens:StringResource, admin.PromotionEditor.StoreLabel%>" AssociatedControlID="ssPromotion" runat="server" />
                                    </td>
                                    <td class="tdValue">
                                        <aspdnsfs:StoreSelector ID="ssPromotion" runat="server" ShowText="false" SelectMode="MultiCheckList" ShowDefaultForAllStores="false" />
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="3">
                                        <asp:Label ID="lblError" runat="server" CssClass="error" />
                                    </td>
                                </tr>                                
                            </table>
                            <asp:HiddenField ID="txtId" runat="server" />
                        </td>
                        <td valign="top">
                            <asp:Panel ID="pnlRulesDiscounts" runat="server" Visible="false" CssClass="pnlRulesDiscounts">
                               
                            <h3 class="checkHeader expandable initiallyExpanded"><asp:Label CssClass="checkLabel" ID="litDiscount" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountLiteral%>" runat="server" /></h3>
                            <div class="checkTarget">
                                <table width="100%;">
                                    <tr id="trShippingDiscount" runat="server">
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <asp:Panel ID="pnlShippingDiscount" runat="server">
                                                <!-- Shipping Discount -->
                                                <h3 class="checkHeader">
                                                    <aspdnsf:GeneralInfo ID="GeneralInfo17" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShippingDiscount%>" /> 
                                                    <asp:CheckBox ID="chkRuleShippingDiscount" runat="server" CssClass="checkToggler" />
                                                    <asp:Label CssClass="checkLabel" ID="litDiscountShipping" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountShippingLiteral%>" runat="server" />
                                                </h3>
                                                <div class="checkTarget">
                                                    <table>
                                                        <tr>
                                                            <td class="tdLabel">
                                                             <asp:Label ID="Label1" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountTypeLabel%>" AssociatedControlID="ddlRuleShippingDiscountType" runat="server" />
                                                            </td>
                                                            <td>
                                                                <asp:DropDownList ID="ddlRuleShippingDiscountType" runat="server">
                                                                    <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesFixed%>" />
                                                                    <asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesPercentage%>" />
                                                                </asp:DropDownList>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tdLabel">
                                                             <asp:Label ID="Label2" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountAmountLabel%>" AssociatedControlID="txtRuleShippingDiscountAmount" runat="server" />
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtRuleShippingDiscountAmount" runat="server" />
                                                                <asp:RangeValidator runat="server" ControlToValidate="txtRuleShippingDiscountAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="multiLineLabel">
                                                             <asp:Label ID="Label3" Text="<%$Tokens:StringResource, admin.PromotionEditor.ShippingMethodsLabel%>" AssociatedControlID="txtRuleShippingMethodID" runat="server" />
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtRuleShippingMethodID" runat="server" style="display:none;" />
                                                                <aspdnsf:CSVHelper ID="CSVHelper2" runat="server" CSVTextBoxID="txtRuleShippingMethodID" UniqueJSID="txtRuleShippingMethodID" EntityType="ShippingMethod" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </div>
                                             </asp:Panel>
                                        </td>
                                    </tr>
                                    <tr id="trShippingOnlyDiscount" runat="server">
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <asp:Panel ID="pnlShippingOnlyDiscount" runat="server">
                                            <!-- Shipping Only Discount -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShipping%>" /> 
                                                <asp:RadioButton GroupName="rblDiscountList" ID="chkShippingOnlyDiscount" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal2" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountShippingOnlyLiteral%>" runat="server" />
                                            </h3>
                                            <div></div>
                                            </asp:Panel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Order Discount -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo18" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoEntireOrder%>" /> 
                                                <asp:RadioButton GroupName="rblDiscountList" ID="chkRuleOrderDiscount" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal3" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountOrderLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label44" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountTypeLabel%>" AssociatedControlID="ddlRuleOrderDiscountType" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="ddlRuleOrderDiscountType" runat="server">
                                                                <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesFixed%>" />
                                                                <asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesPercentage%>" />
                                                            </asp:DropDownList>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                         <asp:Label ID="Label4" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountAmountLabel%>" AssociatedControlID="txtRuleOrderDiscountAmount" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleOrderDiscountAmount" runat="server" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleOrderDiscountAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Line Item Discount -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo19" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoLineItems%>" /> 
                                                <asp:RadioButton GroupName="rblDiscountList" ID="chkRuleLineItemDiscount" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal4" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountApplicableLineItemLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                         <asp:Label ID="Label5" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountTypeLabel%>" AssociatedControlID="ddlRuleLineItemDiscountType" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="ddlRuleLineItemDiscountType" runat="server">
                                                                <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesFixed%>" />
                                                                <asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.PromotionEditor.DropDownValuesPercentage%>" />
                                                            </asp:DropDownList>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                         <asp:Label ID="Label6" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountAmountLabel%>" AssociatedControlID="txtRuleLineItemDiscountAmount" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleLineItemDiscountAmount" runat="server" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleLineItemDiscountAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Gift With Purchase -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo20" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoAutomaticallyAdd%>" /> 
                                                <asp:RadioButton GroupName="rblDiscountList" ID="chkRuleGiftWithPurchase" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal5" Text="<%$Tokens:StringResource, admin.PromotionEditor.GiftWithPurchaseLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel" valign="top">
                                                         <asp:Label ID="Label7" Text="<%$Tokens:StringResource, admin.PromotionEditor.ProductsLabel%>" AssociatedControlID="txtRuleGiftWithPurchaseProductId" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleGiftWithPurchaseProductId" runat="server" style="display:none;" />
                                                            <aspdnsf:CSVHelper ID="CSVHelper3" runat="server" CSVTextBoxID="txtRuleGiftWithPurchaseProductId" UniqueJSID="txtRuleGiftWithPurchaseProductId" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                        <asp:Label ID="Label8" Text="<%$Tokens:StringResource, admin.PromotionEditor.DiscountPercentageLabel%>" AssociatedControlID="txtGiftWithPurchaseDiscountAmount" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtGiftWithPurchaseDiscountAmount" runat="server" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtGiftWithPurchaseDiscountAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                        <asp:Label ID="Label9" Text="<%$Tokens:StringResource, admin.PromotionEditor.MatchQuantitesLabel%>" AssociatedControlID="chkMatchQuantites" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox ID="chkMatchQuantites" Checked="true" runat="server" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                               
                               <h3 class="checkHeader expandable"><asp:Label CssClass="checkLabel" ID="Literal28" Text="<%$Tokens:StringResource, admin.PromotionEditor.LimitsHeaderLiteral%>" runat="server" /></h3>
                               <div class="checkTarget">
                                <table width="100%;">
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- StartDateRules -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo id="AppConfigInfo" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoNotValidUntilStartDate%>" /> 
                                                <asp:CheckBox ID="optRuleStartDate" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal6" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableStartDateLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td class="tdLabel">
                                                        <asp:Label ID="Label10" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleStartDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDateTimePicker InputMode="DateTimePicker" ID="txtRuleStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDateTimePicker>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- ExpirationDateRule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoValidUntilExpiration%>" /> 
                                                <asp:CheckBox ID="optRuleExpirationDate" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal7" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableExpirationDateLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td class="tdLabel">
                                                        <asp:Label ID="Label11" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleExpirationDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDateTimePicker InputMode="DateTimePicker" ID="txtRuleExpirationDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDateTimePicker>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- ExpirationNumberOfUsesDateRule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo1" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoMaxUses%>" /> 
                                                <asp:CheckBox ID="optRuleExpirationNumberOfUses" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal8" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableMaximumNumberofUsesLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td class="tdLabel">
                                                        <asp:Label ID="Label12" Text="<%$Tokens:StringResource, admin.PromotionEditor.MaximumNumberofUsesLabel%>" AssociatedControlID="txtRuleExpirationNumberOfUses" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleExpirationNumberOfUses" runat="server" Width="100" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleExpirationNumberOfUses" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Integer" Display="Dynamic" MinimumValue="0" MaximumValue="99999999" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                        <asp:Literal ID="litPerCustomer2" Text="<%$Tokens:StringResource, admin.PromotionEditor.PerCustomerLiteral%>" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox ID="chkRuleExpirationNumberOfUsesPerCustomer" runat="server" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                                </div>
                                <h3 class="checkHeader expandable"><asp:Label CssClass="checkLabel" ID="Literal10" Text="<%$Tokens:StringResource, admin.PromotionEditor.RequirementsLiteral%>" runat="server" /></h3>
                                <div class="checkTarget">
                                <table width="100%;">
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Product ID Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo2" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoValidForProducts%>" /> 
                                                <asp:CheckBox ID="chkRuleProductId" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal11" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableProductRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
                                                <table>
                                                    <tr>
                                                        <td class="multiLineLabel">
                                                        <asp:Label ID="Label15" Text="<%$Tokens:StringResource, admin.PromotionEditor.ProductsLabel%>" AssociatedControlID="txtRuleProductIds" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleProductIds" runat="server" Width="450" style="display:none;" />
                                                            <aspdnsf:CSVHelper ID="relatedHelper" runat="server" CSVTextBoxID="txtRuleProductIds" UniqueJSID="txtRuleProductIds" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                        <asp:Label ID="Label16" Text="<%$Tokens:StringResource, admin.PromotionEditor.RequireLabel%>" AssociatedControlID="rblProductsAllOrAny" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:RadioButtonList ID="rblProductsAllOrAny" runat="server" RepeatDirection="Horizontal">
                                                                <asp:ListItem Text="All" Value="all" />
                                                                <asp:ListItem Text="Any" Value="any" />
                                                            </asp:RadioButtonList>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                        <asp:Label ID="Label17" Text="<%$Tokens:StringResource, admin.PromotionEditor.RequireQuantityLabel%>" AssociatedControlID="txtRuleProductIdsRequireQuantity" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleProductIdsRequireQuantity" runat="server" Width="50" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleProductIdsRequireQuantity" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Integer" Display="Dynamic" MinimumValue="0" MaximumValue="99999999" />
                                                        </td>
                                                    </tr>
                                                </table>
                                                
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Category Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo3" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoProductFromCategory%>" /> 
                                                <asp:CheckBox ID="chkRuleCategories" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal13" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableCategoryRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="multiLineLabel">
                                                             <asp:Label ID="Label21" Text="<%$Tokens:StringResource, admin.PromotionEditor.CategoriesLabel%>" AssociatedControlID="txtRuleCategories" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleCategories" runat="server" Width="450" style="display:none;" />
                                                            <aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleCategories" UniqueJSID="txtRuleCategories" EntityType="Category" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Section Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo4" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoProductFromSection%>" /> 
                                                <asp:CheckBox ID="chkRuleSections" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal14" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableSectionRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="multiLineLabel">
                                                             <asp:Label ID="Label22" Text="<%$Tokens:StringResource, admin.PromotionEditor.SectionsLabel%>" AssociatedControlID="txtRuleSections" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleSections" runat="server" Width="450" style="display:none;" />
                                                            <aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleSections" UniqueJSID="txtRuleSections" EntityType="Section" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Manufacturer Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo5" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoProductFromManufacturer%>" /> 
                                                <asp:CheckBox ID="chkRuleManufacturers" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal15" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableManufacturerRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="multiLineLabel">
                                                             <asp:Label ID="Label23" Text="<%$Tokens:StringResource, admin.PromotionEditor.ManufacturersLabel%>" AssociatedControlID="txtRuleManufacturers" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleManufacturers" runat="server" Width="450" style="display:none;" />
                                                            <aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleManufacturers" UniqueJSID="txtRuleManufacturers" EntityType="Manufacturer" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Minimum Cart Amount Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo7" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoSubtotalGreater%>" /> 
                                                <asp:CheckBox ID="chkRuleCartAmount" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal17" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableMinimumCartSubtotalRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td>
                                                        <asp:Literal ID="Literal18" Text="<%$Tokens:StringResource, admin.PromotionEditor.MinimumAmountLiteral%>" runat="server" />
                                                        <asp:TextBox ID="txtRuleCartAmount" runat="server" Width="50" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleCartAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Email Address Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo8" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoEmailAddressIncluded%>" /> 
                                                <asp:CheckBox ID="chkRuleEmail" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal19" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableEmailAddressRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label26" Text="<%$Tokens:StringResource, admin.PromotionEditor.EmailAddressesLabel%>" AssociatedControlID="txtRuleEmailAddresses" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleEmailAddresses" runat="server" Width="450" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td colspan="2">
                                                            <asp:Literal ID="Literal20" Text="<%$Tokens:StringResource, admin.PromotionEditor.UploadfromcsvfileLiteral%>" runat="server" />
                                                            <asp:FileUpload ID="fileUpload" runat="server" />
                                                            <asp:Button ID="btnUpload" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.UploadButton%>" OnClick="btnUpload_Click" CssClass="button" /><br />
                                                            <asp:Label ID="lblEmailUploadError" runat="server" CssClass="error" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Customer Level Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo9" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoCustomerLevels%>" /> 
                                                <asp:CheckBox ID="chkRuleCustomerLevel" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal21" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableCustomerLevelRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="multiLineLabel">
                                                             <asp:Label ID="Label27" Text="<%$Tokens:StringResource, admin.PromotionEditor.CustomerLevelLabel%>" AssociatedControlID="txtRuleCustomerLevels" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleCustomerLevels" runat="server" Width="450" style="display:none;" />
                                                            <aspdnsf:CSVHelper ID="CSVHelper1" runat="server" CSVTextBoxID="txtRuleCustomerLevels" UniqueJSID="txtRuleCustomerLevels" EntityType="CustomerLevel" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- State Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo10" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShippingAddressState%>" /> 
                                                <asp:CheckBox ID="chkRuleState" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal22" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableShippingStateRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label28" Text="<%$Tokens:StringResource, admin.PromotionEditor.StatesLabel%>" AssociatedControlID="txtRuleStates" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleStates" runat="server" Width="450" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- ZipCode Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo11" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShippingAddressZip%>" /> 
                                                <asp:CheckBox ID="chkRuleZipCode" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal23" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableShippingZipCodeRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
                                                <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label29" Text="<%$Tokens:StringResource, admin.PromotionEditor.ZipCodesLabel%>" AssociatedControlID="txtRuleZipCodes" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleZipCodes" runat="server" Width="450" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Country Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo12" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoShippingAddressCountry%>" /> 
                                                <asp:CheckBox ID="chkRuleCountryCodes" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal24" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableShippingCountryRequirementLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label30" Text="<%$Tokens:StringResource, admin.PromotionEditor.CountryNamesLabel%>" AssociatedControlID="txtRuleCountryCodes" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleCountryCodes" runat="server" Width="450" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <h3 class="checkHeader expandable"><asp:Label CssClass="checkLabel" ID="Literal30" Text="<%$Tokens:StringResource, admin.PromotionEditor.LoyaltyHeaderLiteral%>" runat="server" /></h3>
                            <div class="checkTarget">
                                <table width="100%;">
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Minimum Orders Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo13" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoMorePastOrders%>" /> 
                                                <asp:CheckBox ID="chkRuleMinimumOrders" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal25" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnableMinimumNumberofPastOrdersLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td>
                                                            <asp:Label ID="Label31" Text="<%$Tokens:StringResource, admin.PromotionEditor.MinimumNumberofPastOrdersLabel%>" AssociatedControlID="txtRuleMinimumOrders" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleMinimumOrders" runat="server" Width="50" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleMinimumOrders" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Integer" Display="Dynamic" MinimumValue="0" MaximumValue="99999999" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label32" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleMinimumOrdersCustomStartDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="txtRuleMinimumOrdersCustomStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label45" Text="<%$Tokens:StringResource, admin.PromotionEditor.EndDateLabel%>" AssociatedControlID="txtRuleMinimumOrdersCustomEndDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="txtRuleMinimumOrdersCustomEndDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Minimum Order Amount Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo14" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPastOrdersSum%>" /> 
                                                <asp:CheckBox ID="chkRuleMinimumOrderAmount" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal26" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnablePastOrdersMinimumAmountLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label46" Text="<%$Tokens:StringResource, admin.PromotionEditor.MinimumRequiredLabel%>" AssociatedControlID="txtRuleMinimumOrderAmount" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleMinimumOrderAmount" runat="server" Width="100" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleMinimumOrderAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label33" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleMinimumOrderAmountCustomStartDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="txtRuleMinimumOrderAmountCustomStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label47" Text="<%$Tokens:StringResource, admin.PromotionEditor.EndDateLabel%>" AssociatedControlID="txtRuleMinimumOrderAmountCustomEndDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="txtRuleMinimumOrderAmountCustomEndDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Minimum Products Ordered Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo15" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPastOrdersProducts%>" /> 
                                                <asp:CheckBox ID="chkRuleMinimumProductsOrdered" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal27" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnablePastOrdersMinimumNumberofProductsLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label53" Text="<%$Tokens:StringResource, admin.PromotionEditor.QuantityRequiredLabel%>" AssociatedControlID="txtRuleMinimumProductsOrdered" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleMinimumProductsOrdered" runat="server" Width="50" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleMinimumProductsOrdered" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Integer" Display="Dynamic" MinimumValue="0" MaximumValue="999999999" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label34" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedCustomStartDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="txtRuleMinimumProductsOrderedCustomStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label48" Text="<%$Tokens:StringResource, admin.PromotionEditor.EndDateLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedCustomEndDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="txtRuleMinimumProductsOrderedCustomEndDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel" valign="top">
                                                             <asp:Label ID="Label51" Text="<%$Tokens:StringResource, admin.PromotionEditor.ProductIdsLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedProductIds" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleMinimumProductsOrderedProductIds" runat="server" Width="450" style="display:none;" />
                                                            <aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleMinimumProductsOrderedProductIds" UniqueJSID="txtRuleMinimumProductsOrderedProductIds" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="emptyCell">&nbsp;</td>
                                        <td>
                                            <!-- Minimum Product Amount Ordered Rule -->
                                            <h3 class="checkHeader">
                                                <aspdnsf:GeneralInfo ID="GeneralInfo16" runat="server" DefaultText="<%$Tokens:StringResource, admin.PromotionEditor.GeneralInfoPastOrdersPRoductSum%>" /> 
                                                <asp:CheckBox ID="chkRuleMinimumProductsOrderedAmount" runat="server" CssClass="checkToggler" />
                                                <asp:Label CssClass="checkLabel" ID="Literal31" Text="<%$Tokens:StringResource, admin.PromotionEditor.EnablePastOrdersValueofProductsLiteral%>" runat="server" />
                                            </h3>
                                            <div class="checkTarget">
	                                            <table>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label52" Text="<%$Tokens:StringResource, admin.PromotionEditor.AmountRequiredLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedAmount" runat="server" />
                                                        </td>
                                                        <td>
                                                            $<asp:TextBox ID="txtRuleMinimumProductsOrderedAmount" runat="server" Width="100" />
                                                            <asp:RangeValidator runat="server" ControlToValidate="txtRuleMinimumProductsOrderedAmount" ErrorMessage="<%$Tokens:StringResource, admin.PromotionEditor.ValidationIncorrectNumberFormat%>" Type="Double" Display="Dynamic" MinimumValue="0" MaximumValue="999999999999999999" />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label35" Text="<%$Tokens:StringResource, admin.PromotionEditor.StartDateLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedAmountCustomStartDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="txtRuleMinimumProductsOrderedAmountCustomStartDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel">
                                                             <asp:Label ID="Label49" Text="<%$Tokens:StringResource, admin.PromotionEditor.EndDateLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedAmountCustomEndDate" runat="server" />
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="txtRuleMinimumProductsOrderedAmountCustomEndDate" runat="server" Style="z-index: 150000;" MaxDate="9999-12-31">
                                                                <Calendar runat="server" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
                                                                <DatePopupButton HoverImageUrl="" ImageUrl="" />
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tdLabel" valign="top">
                                                             <asp:Label ID="Label55" Text="<%$Tokens:StringResource, admin.PromotionEditor.ProductIdsLabel%>" AssociatedControlID="txtRuleMinimumProductsOrderedAmountProductIds" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtRuleMinimumProductsOrderedAmountProductIds" runat="server" Width="450" style="display:none;" />
                                                            <aspdnsf:CSVHelper runat="server" CSVTextBoxID="txtRuleMinimumProductsOrderedAmountProductIds" UniqueJSID="txtRuleMinimumProductsOrderedAmountProductIds" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            </asp:Panel>
                        </td>
                    </tr>
                    <tr>
                        <td class="tdButtonRow" style="text-align:left;">
                            <asp:Button ID="Button1" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.UpdateButton%>" OnClick="btnUpdate_Click" CssClass="button" />
                            <asp:Button ID="Button2" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.DeleteButton%>" OnClick="btnDelete_Click" CssClass="button" CausesValidation="false" OnClientClick="return confirm('Are you sure you want to delete this promotion?');" />
                            <asp:Button ID="Button3" runat="server" Text="<%$Tokens:StringResource, admin.PromotionEditor.CancelButton%>" OnClick="btnCancel_Click" CausesValidation="false" CssClass="button" />
                        </td>
                        <td class="tdButtonRow" style="text-align:left;">
							<input runat="server" type="button" value="<%$Tokens:StringResource, admin.PromotionEditor.ExpandAllButton%>" class="button expandAllExpandables" />
							<input runat="server" type="button" value="<%$Tokens:StringResource, admin.PromotionEditor.CollapseAllButton%>" class="button collapseAllExpandables" />
                        </td>
                    </tr>
                </table>
            </fieldset>
        </asp:Panel>
    </div>
</asp:Content>
