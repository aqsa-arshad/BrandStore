<%@ Page Language="C#" AutoEventWireup="true" CodeFile="entityEditProductVariant.aspx.cs"
    Inherits="AspDotNetStorefrontAdmin.entityEditProductVariant" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>
<%@ OutputCache Duration="1" Location="none" %>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Entity Edit Product Variant</title>
    <asp:Literal runat="server" ID="ltStyles"></asp:Literal>

    <script type="text/javascript">
        function CloseAndRebind(productID) {
            var currentWindow = GetRadWindow();
            if (currentWindow) {
                var args = new Object();
                args = "Variant," + productID;
                currentWindow.argument = args;
            }
            GetRadWindow().BrowserWindow.refreshGrid(args);
            GetRadWindow().Close();
        }

        function GetRadWindow() {
            var oWindow = null;
            if (window.radWindow) oWindow = window.radWindow; //Will work in Moz in all cases, including clasic dialog
            else if (window.frameElement.radWindow) oWindow = window.frameElement.radWindow; //IE (and Moz as well)

            return oWindow;
        }

        function CancelEdit() {
            GetRadWindow().Close();
        }
    </script>

</head>
<body>
    <form id="frmEntityEdit" runat="server" enctype="multipart/form-data" method="post">
    <asp:ScriptManager ID="scrptMgr" runat="server">
    </asp:ScriptManager>
    <asp:Panel ID="pnlContent" runat="server">
        <asp:Literal runat="server" ID="Literal1"></asp:Literal>
        <div style="width: 100%;">
            <div style="float: left;">
                <table style="width: 100%">
                    <tr>
                        <td align="left" style="width: 70%" valign="top">
                            <div class="breadCrumb3">
                                <div style="float: left">
                                    &nbsp;<asp:Literal ID="ltProduct" runat="server"></asp:Literal>
                                </div>
                                <div style="float: left">
                                    |&nbsp;<asp:Literal ID="ltStatus" runat="server"></asp:Literal>&nbsp;|
                                    <asp:LinkButton ID="btnDeleteVariant" runat="server" Text="Delete this Variant"
                                                        OnClick="btnDeleteVariant_Click" />
                                    </div>
                            </div>
                            <asp:Panel ID="pnlUpdateMappedKitItem" runat="server" Visible="false">
                                <br />
                                This variant is mapped to one or more kit items:
                                <asp:HyperLink ID="lnkMappedKitItems" runat="server" NavigateUrl="javascript:void(0);"
                                    Text="View mapped items"></asp:HyperLink>
                                <%--spacer--%>
                                <div style="width: 100%; height: 10px">
                                </div>
                            </asp:Panel>
                        </td>
                        <td align="right" style="width: 30%" valign="top">
                            <div id="help" style="float: right; margin-right: 5px;">
                                <table border="0" cellpadding="1" cellspacing="0" class="outerTable" runat="server"
                                    id="tblLocale">
                                    <tr>
                                        <td>
                                            <div class="wrapper">
                                                <table border="0" cellpadding="0" cellspacing="0" class="innerTable">
                                                    <tr>
                                                        <td class="titleTable">
                                                            <font class="subTitle">Locale:</font>
                                                        </td>
                                                        <td class="contentTable">
                                                            <asp:DropDownList ID="ddLocale" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddLocale_SelectedIndexChanged">
                                                            </asp:DropDownList>
                                                        </td>
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
                        <td colspan="2">
                            <asp:Literal ID="ltError" runat="server"></asp:Literal>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:Label ID="lblerr" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>
                        </td>
                    </tr>
                </table>
                <div style="padding-top: 2px; clear: left;">
                    <AjaxToolkit:TabContainer ID="tabContainer" runat="server" CssClass="aspdnsf_Tab">
                        <AjaxToolkit:TabPanel ID="pnlMain" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Main %>'>
                            <ContentTemplate>
                                <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                    <tr>
                                        <td class="tabShaddow">
                                        </td>
                                    </tr>
                                </table>
                                <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                    <tr>
                                        <td align="left">
                                            <table border="0" cellpadding="0" cellspacing="0" class="">
                                                <tr>
                                                    <td>
                                                        <div class="">
                                                            <table cellpadding="0" cellspacing="1" border="0">
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Variant Name:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="singleNormal" ID="txtName" runat="Server"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">SKU Suffix:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="50" ID="txtSKU" runat="server" CssClass="singleShorter"></asp:TextBox>
                                                                        &nbsp; <font class="subTitleSmall">Manufacturer Part #:</font>
                                                                        <asp:TextBox MaxLength="50" ID="txtManufacturePartNumber" runat="server" CssClass="singleShorter"></asp:TextBox>
                                                                     &nbsp; <font class="subTitleSmall">GTIN:</font><asp:TextBox MaxLength="14" ID="txtGTIN" runat="server" Columns="14"></asp:TextBox>
																	</td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*Published:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:RadioButtonList ID="rblPublished" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                            <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                            <asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Restricted Quantities:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="250" ID="txtRestrictedQuantities" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgRestrictedQuantities" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.restrictedqty %>"
                                                                            style="cursor: normal;" />
                                                                        &nbsp; <font class="subTitleSmall">Minimum Quantity:</font>
                                                                        <asp:TextBox MaxLength="10" ID="txtMinimumQuantity" runat="server" CssClass="singleShortest"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgMinimumQuantity" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.minimumqty %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*Price:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="10" CssClass="singleShortest" ID="txtPrice" runat="Server"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ControlToValidate="txtPrice" ErrorMessage="Please enter the Price"
                                                                            ID="rfvName" ValidationGroup="Main" EnableClientScript="true" SetFocusOnError="true"
                                                                            runat="server" Display="Dynamic">!!</asp:RequiredFieldValidator>
                                                                        <asp:Image runat="server" id="imgPrice" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Price %>"
                                                                            style="cursor: normal;" />
                                                                        &nbsp; <font class="subTitleSmall">Sale Price:</font>
                                                                        <asp:TextBox MaxLength="10" CssClass="singleShortest" ID="txtSalePrice" runat="Server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgSalePrice" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.SalePrice %>"
                                                                            style="cursor: normal;" />
                                                                        &nbsp;&nbsp;
                                                                        <asp:Literal ID="ltExtendedPricing" runat="server"></asp:Literal>
                                                                    </td>
                                                                </tr>
                                                                <tr id="trCustomerEntersPrice1" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*Customer Enters Price:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:RadioButtonList ID="rblCustomerEntersPrice" runat="server" RepeatColumns="2"
                                                                            RepeatDirection="horizontal">
                                                                            <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                            <asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                          <asp:Image runat="server" id="Image1" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.CustomerEntersPrice %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr id="trCustomerEntersPricePrompt" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        Customer Enters Price Prompt:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtCustomerEntersPricePrompt" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                          <asp:Image runat="server" id="Image2" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.CustomerEntersPricePrompt %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">MSRP:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="10" CssClass="singleShortest" ID="txtMSRP" runat="Server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgMSRP" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.msrp %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Actual Cost:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="10" CssClass="singleShortest" ID="txtActualCost" runat="Server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgActualCost" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.actualCost %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*Is Taxable:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:RadioButtonList ID="rblTaxable" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                            <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                            <asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                          <asp:Image runat="server" id="Image3" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.IsTaxable %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr id="trShippingCost" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Shipping Cost:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:Literal ID="ltShippingCost" runat="server"></asp:Literal>
                                                                                                                                              <asp:Image runat="server" id="Image4" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.ShippingCost %>"
                                                                            style="cursor: normal;" />
                                                                    </td>

                                                                </tr>
                                                                <tr id="trShipSeparately" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*Is Ship Separately:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:RadioButtonList ID="rblShipSeparately" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                            <asp:ListItem Value="0" Text="No" Selected="true"></asp:ListItem>
                                                                            <asp:ListItem Value="1" Text="Yes"></asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                          <asp:Image runat="server" id="Image5" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.IsShipSeparately %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <!--MOD START GS - Added Requires Shipping radio button-->
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*Is Free Shipping:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:RadioButtonList ID="rblFreeShipping" runat="server" RepeatColumns="3" RepeatDirection="horizontal">
                                                                            <asp:ListItem Value="0" Text="No" Selected="true"></asp:ListItem>
                                                                            <asp:ListItem Value="1" Text="Yes"></asp:ListItem>
                                                                            <asp:ListItem Value="2" Text="No Shipping Required"></asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                        <asp:Image runat="server" id="Image6" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.FreeShipping %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr id="trISDownload" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*Is Download:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:RadioButtonList ID="rblDownload" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                            <asp:ListItem Value="0" Text="No" Selected="true"></asp:ListItem>
                                                                            <asp:ListItem Value="1" Text="Yes"></asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                        <asp:Image runat="server" id="Image" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Download %>"
                                                                            style="cursor: normal;" />
																		Download is Valid For Number Of Days:
																		 <asp:TextBox MaxLength="5" ID="txtValidForDays" runat="server" CssClass="singleShortest"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr id="trDownloadLoc" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        Download Location:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="250" ID="txtDownloadLocation" runat="server" CssClass="singleLonger"></asp:TextBox>
                                                                        <asp:Image runat="server" id="Image8" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.DownloadLocation %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr id="trCondition" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*Condition:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:RadioButtonList ID="rblCondition" runat="server" RepeatColumns="3" RepeatDirection="horizontal">
                                                                            <asp:ListItem Value="0" Text="New" Selected="true"></asp:ListItem>
                                                                            <asp:ListItem Value="1" Text="Used"></asp:ListItem>
                                                                            <asp:ListItem Value="2" Text="Refurbished"></asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                        <asp:Image runat="server" id="Image7" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Condition %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Weight:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="15" CssClass="singleShortest" ID="txtWeight" runat="Server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgWeight" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Weight %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Dimensions:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="50" CssClass="singleNormal" ID="txtDimensions" runat="Server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgDimensions" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Dimensions %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr id="trCurrentInventory" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Current Inventory:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="15" CssClass="singleShortest" ID="txtCurrentInventory" runat="Server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="Image9" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.CurrentInventory %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr id="trManageInventory" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Manage Inventory:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:Literal ID="ltManageInventory" runat="server"></asp:Literal>
                                                                    </td>
                                                                </tr>
                                                                <tr id="trSubscription" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <asp:Literal ID="ltNormalSubscription" runat="server"><font class="subTitleSmall">Subscription Interval:</font></asp:Literal>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="50" ID="txtSubscriptionInterval" runat="server" CssClass="singleShortest">1</asp:TextBox>
                                                                        &nbsp; <font class="subTitleSmall">*Interval Type:</font>
                                                                        <asp:DropDownList Style="font-size: 10px;" ID="rblSubscriptionIntervalType" runat="server">
                                                                        </asp:DropDownList>
                                                                        <asp:Image runat="server" id="Image10" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Subscription %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </AjaxToolkit:TabPanel>
                        <AjaxToolkit:TabPanel ID="pnlImages" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Images %>'>
                            <ContentTemplate>
                                <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                    <tr>
                                        <td class="tabShaddow">
                                        </td>
                                    </tr>
                                </table>
                                <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                    <tr>
                                        <td align="left">
                                            <table border="0" cellpadding="0" cellspacing="0" class="">
                                                <tr>
                                                    <td>
                                                        <div class="">
                                                            <table cellpadding="0" cellspacing="1" border="0">
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Image Filename Override:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="singleNormal" ID="txtImageOverride" runat="Server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgImageOverride" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.ImageFilenameOverride %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Alt Text:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="textbox" ID="txtSEAlt" runat="Server"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="top">
                                                                        <font class="subTitleSmall">Icon:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:FileUpload CssClass="fileUpload" ID="fuIcon" runat="Server" />
                                                                        <div>
                                                                            <asp:Literal ID="ltIcon" runat="server"></asp:Literal>
                                                                        </div>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="top">
                                                                        <font class="subTitleSmall">Medium:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:FileUpload CssClass="fileUpload" ID="fuMedium" runat="Server" />
                                                                        <div>
                                                                            <asp:Literal ID="ltMedium" runat="server"></asp:Literal>
                                                                        </div>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="top">
                                                                        <font class="subTitleSmall">Large:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:FileUpload CssClass="fileUpload" ID="fuLarge" runat="Server" />
                                                                        <div>
                                                                            <asp:Literal ID="ltLarge" runat="server"></asp:Literal>
                                                                        </div>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </AjaxToolkit:TabPanel>
                        <AjaxToolkit:TabPanel ID="pnlDescription" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Description %>'>
                            <ContentTemplate>
                                <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                    <tr>
                                        <td class="tabShaddow">
                                        </td>
                                    </tr>
                                </table>
                                <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                    <tr>
                                        <td align="left">
                                            <table border="0" cellpadding="0" cellspacing="0" class="" width="100%">
                                                <tr>
                                                    <td>
                                                        <div class="">
                                                            <table cellpadding="0" cellspacing="1" border="0" width="100%">
                                                                <tr>
                                                                    <td align="left" valign="middle" width="100%">
                                                                        <asp:Literal ID="ltDescription" runat="Server"></asp:Literal>
                                                                        <telerik:radeditor runat="server" id="radDescription">
                                                                            <ImageManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <DocumentManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <FlashManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <MediaManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <SilverlightManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <TemplateManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                        </telerik:radeditor>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </AjaxToolkit:TabPanel>
                        <AjaxToolkit:TabPanel ID="pnlProductFeeds" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.ProductFeeds %>'>
                            <ContentTemplate>
                                <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                    <tr>
                                        <td class="tabShaddow">
                                        </td>
                                    </tr>
                                </table>
                                <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                    <tr>
                                        <td align="left">
                                            <table border="0" cellpadding="0" cellspacing="0" class="">
                                                <tr>
                                                    <td>
                                                        <div class="">
                                                            <table cellpadding="0" cellspacing="1" border="0">
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Feed Desc (No HTML):</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="multiExtension" ID="txtFroogle" runat="Server" TextMode="multiLine"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </AjaxToolkit:TabPanel>
                        <AjaxToolkit:TabPanel ID="pnlAttributes" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Attributes %>'>
                            <ContentTemplate>
                                <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                    <tr>
                                        <td class="tabShaddow">
                                        </td>
                                    </tr>
                                </table>
                                <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                    <tr>
                                        <td align="left" width="100%">
                                            <table border="0" cellpadding="0" cellspacing="0" class="" width="100%">
                                                <tr>
                                                    <td width="100%">
                                                        <div class="">
                                                            <table cellpadding="0" cellspacing="1" border="0" width="100%">
                                                                <tr>
                                                                    <td align="right" valign="middle" style="width: 30%">
                                                                        Colors:
                                                                    </td>
                                                                    <td align="left" valign="middle" style="width: 70%">
                                                                        <asp:TextBox ID="txtColors" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgColors" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.Colors %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Color SKU Modifiers:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtColorSKUModifiers" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgColorSKUModifiers" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.ColorSkuMods %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Sizes:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtSizes" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgSizes" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.sizes %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">Size SKU Modifiers:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtSizeSKUModifiers" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgSizeSKUModifiers" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.sizeskumods %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr id="trColorSizeSummary" runat="server">
                                                                    <td align="right" valign="middle">
                                                                        <%--<font class="subTitleSmall">Color/Size Tables:<br />(summary)</font>--%>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:Literal ID="ltColorSizeSummary" runat="server"></asp:Literal>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </AjaxToolkit:TabPanel>
                        <AjaxToolkit:TabPanel ID="pnlMiscellaneous" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Miscellaneous %>'>
                            <ContentTemplate>
                                <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                    <tr>
                                        <td class="tabShaddow">
                                        </td>
                                    </tr>
                                </table>
                                <div>
                                    <table cellpadding="0" cellspacing="1" border="0" width="100%">
                                        <tr>
                                            <td align="right" valign="middle" style="width: 25%">
                                                Page BG Color:
                                            </td>
                                            <td align="left" valign="middle" style="width: 75%">
                                                <asp:TextBox CssClass="" ID="txtPageBG" runat="Server"></asp:TextBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" valign="middle">
                                                Contents BG Color:
                                            </td>
                                            <td align="left" valign="middle">
                                                <asp:TextBox CssClass="" ID="txtContentsBG" runat="Server"></asp:TextBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="right" valign="middle">
                                                Skin Graphics Color:
                                            </td>
                                            <td align="left" valign="middle">
                                                <asp:TextBox CssClass="" ID="txtSkinColor" runat="Server"></asp:TextBox>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </ContentTemplate>
                        </AjaxToolkit:TabPanel>
                        <AjaxToolkit:TabPanel ID="pnlRecurring" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Recurring %>'>
                            <ContentTemplate>
                                <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                    <tr>
                                        <td class="tabShaddow"></td>
                                    </tr>
                                </table>
                                <div>
                                    <table cellpadding="0" cellspacing="1" border="0" width="100%"> 
                                        <tr runat="server" id="trRecurring">
                                            <td style="width: 20%; text-align:right;">
                                                <font class="subTitleSmall">*Is Recurring:</font>
                                            </td>
                                            <td style="width: 80%; text-align: left;">
                                                <asp:RadioButtonList ID="rblRecurring" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                    <asp:ListItem Value="0" Text="No" Selected="true"></asp:ListItem>
                                                    <asp:ListItem Value="1" Text="Yes"></asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                        <tr runat="server" id="trRecurringInterval">
                                            <td style="width: 20%; text-align:right;">
                                                <font class="subTitleSmall">Recurring Interval:</font>
                                            </td>
                                            <td style="width: 80%; text-align: left;">
                                                <asp:TextBox MaxLength="50" ID="txtRecurringIntervalMultiplier" runat="server" CssClass="singleShortest" />
                                            </td>
                                        </tr>
                                        <tr runat="server" id="trRecurringType">
                                            <td style="width: 20%; text-align:right;">
                                                <font class="subTitleSmall">*Interval Type:</font>
                                            </td>
                                            <td style="width: 80%; text-align: left;">
                                                <asp:DropDownList Style="font-size: 10px;" ID="ddRecurringInterval" runat="server" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>

                                <table style="padding-left: 20px; padding-top: 40px; vertical-align: middle;">
                                    <tr>
                                        <td colspan="2" style="text-align: center">
                                            <asp:HyperLink Text="Get help with recurring products" NavigateUrl="http://manual.aspdotnetstorefront.com/p-1057-recurring-orders.aspx" runat="server" ID="hplManualLink" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Image runat="server" ID="imgAuthnet" ImageUrl="~/Admin/Images/authorizenet.gif" />
                                        </td>
                                        <td>
                                            <asp:Image runat="server" ID="imgPayPal" ImageUrl="~/Admin/Images/PayPal_PaymentsAccepted.gif" />
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </AjaxToolkit:TabPanel>
                    </AjaxToolkit:TabContainer>
                </div>
            </div>
            <div style="clear: both; padding-bottom: 15px;">
                <div style="width: 100%; height: 10px">
                </div>
                <asp:Button ID="btnSubmit" runat="server" CssClass="normalButtons" OnClick="btnSubmit_Click"
                    ValidationGroup="Main" /> &nbsp; &nbsp;
                    <asp:Button ID="btnClose" runat="server" CssClass="normalButtons" OnClick="btnClose_Click"
                    ValidationGroup="Main" Text="Save and Close" />
                    &nbsp; &nbsp;
                    <asp:Button ID="btnCancel" runat="server" CssClass="normalButtons" OnClick="btnCancel_Click"
                    ValidationGroup="Main" Text="Cancel" 
                    OnClientClick="return confirm('Are you sure you wish to cancel?  All unsaved information will be lost.');" />
                <asp:ValidationSummary ID="validationSummary" runat="Server" ValidationGroup="Main"
                    DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" />
            </div>
            <asp:Literal ID="ltScript" runat="server"></asp:Literal>
            <asp:Literal ID="ltScript2" runat="server"></asp:Literal>
        </div>
    </asp:Panel>
    </form>
</body>
</html>
