<%@ Page Language="C#" AutoEventWireup="true" CodeFile="entityEditProducts.aspx.cs"
    Inherits="AspDotNetStorefrontAdmin.entityEditProducts" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="CSVHelper" Src="controls/CSVHelper.ascx" %>
<%@ OutputCache Duration="1" Location="none" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Entity Edit Product</title>
    <asp:Literal runat="server" ID="ltStyles"></asp:Literal>
    <script type="text/javascript" src="Scripts/jquery.min.js"></script>
    <script type="text/javascript">
        function calcHeight() {
            //find the height of the internal page
            var the_height = document.getElementById('variantFrame').contentWindow.document.body.scrollHeight + 5;

            //change the height of the iframe
            document.getElementById('variantFrame').height = the_height;
        }
    </script>

    <script type="text/javascript">
        function CloseAndRebind(productID) {
            var currentWindow = GetRadWindow();
            if (currentWindow) {
                var args = new Object();
                args = "Product," + productID;
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
    <style type="text/css">
        h2.head
        {
            margin:0 0 3px 0;
            font-size:12px;
            cursor:pointer;
        }
        .helpercontainer
        {
            border:solid 2px #B7CCFB;
            margin:5px;
            padding:4px;
        }
        .helperTT
        {
            float:right;
        }
    </style>
</head>
<body>
    <form id="frmEntityEdit" runat="server" enctype="multipart/form-data" method="post">
    <asp:ScriptManager ID="scrptMgr" runat="server" EnableScriptGlobalization="true" EnableScriptLocalization="true" >
    </asp:ScriptManager>
    <asp:Panel ID="pnlContent" runat="server">
        <div style="width: 100%;">
            <table border="0" style="width: 100%">
                <tr>
                    <td align="left" style="width: 70%" valign="top">
                        <div class="breadCrumb3">
                            <div style="float: left">
                                <asp:Label ID="Label2" runat="server" Font-Bold="True" Text="Status :"></asp:Label></div>
                            <div style="padding-right: 3px; padding-left: 3px; float: left; font-weight: normal;"
                                class="breadCrumbTitleText">
                                <asp:Literal ID="ltStatus" runat="server"></asp:Literal></div>
                            <div id="divltStatus2" runat="server" style="padding-right: 3px; float: left">
                                |
                                <asp:Literal ID="ltStatus2" runat="server"></asp:Literal>
                                |
                            </div>
                            <div  style="padding-right: 3px; float: left">
                                <asp:Panel ID="pnlDelete" runat="server">
                                    <asp:LinkButton ID="btnDeleteProduct" runat="server" Text="Delete this Product" OnClick="btnDeleteProduct_Click" /></asp:Panel>
                            </div>
                        </div>
                    </td>
                    <td align="right" style="width: 30%" valign="top">
                        <div id="" style="float: right; margin-right: 5px;">
                            <table border="0" cellpadding="1" cellspacing="0" class="outerTable" runat="server"
                                id="tblLocale">
                                <tr>
                                    <td style="height: 34px">
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
                    <td colspan="2" class="breadCrumb3">
                        <asp:Literal ID="ltError" runat="server"></asp:Literal>
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
                            <table border="0" cellpadding="1" cellspacing="1" width="100%">
                                <tr>
                                    <td align="left">
                                        <table border="0" cellpadding="1" cellspacing="1">
                                            <tr>
                                                <td>
                                                    <div>
                                                        <table cellpadding="0" cellspacing="1" border="0">
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Name:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox2" ID="txtName" runat="Server"></asp:TextBox>
                                                                    <asp:RequiredFieldValidator ControlToValidate="txtName" ErrorMessage="Please enter the Product Name (Main Tab)"
                                                                        ID="rfvName" ValidationGroup="Main" EnableClientScript="true" SetFocusOnError="true"
                                                                        runat="server" Display="Static">!!</asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Product Type:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList ID="ddType" CssClass="comboBox" runat="server">
                                                                    </asp:DropDownList>
                                                                    <asp:RequiredFieldValidator ControlToValidate="ddType" InitialValue="0" ErrorMessage="Please select a product type (Main Tab)"
                                                                        ID="RequiredFieldValidator3" ValidationGroup="Main" EnableClientScript="true"
                                                                        SetFocusOnError="true" runat="server" Display="Static">!!</asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Manufacturer:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList CssClass="comboBox" ID="ddManufacturer" runat="server">
                                                                    </asp:DropDownList>
                                                                    <asp:RequiredFieldValidator ControlToValidate="ddManufacturer" InitialValue="0" ErrorMessage="Please select a manufacturer (Main Tab)"
                                                                        ID="RequiredFieldValidator1" ValidationGroup="Main" EnableClientScript="true"
                                                                        SetFocusOnError="true" runat="server" Display="Static">!!</asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                            <tr runat="server" id="trDistributor">
                                                                <td align="right" valign="middle">
                                                                    Distributor:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList CssClass="comboBox" ID="ddDistributor" runat="server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    SKU:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox MaxLength="50" ID="txtSKU" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                    &nbsp; Manufacturer Part #:
                                                                    <asp:TextBox MaxLength="50" ID="txtManufacturePartNumber" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <asp:Label ID="lblPublished" runat="server">*Published:</asp:Label>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblPublished" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Display Format XmlPackage:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList CssClass="comboBox" ID="ddXmlPackage" runat="Server">
                                                                    </asp:DropDownList>
                                                                    <asp:Image runat="server" ID="imgXmlPackage" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.XMLPackage %>"
                                                                        Style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Fund Category:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList CssClass="comboBox" ID="ddFund" runat="Server">
                                                                    </asp:DropDownList>
                                                                    <asp:Image runat="server" ID="imgFund" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.Fund %>"
                                                                        Style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Page Size:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox MaxLength="2" ID="txtPageSize" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                    <asp:Image runat="server" ID="imgPageSize" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.PageSize %>"
                                                                        Style="cursor: normal;" />
                                                                    &nbsp; Column Width:
                                                                    <asp:TextBox MaxLength="2" ID="txtColumn" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                    <asp:Image runat="server" ID="imgColumn" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.ColumnWidth %>"
                                                                        Style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Tax Class:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList CssClass="comboBox" ID="ddTaxClass" runat="Server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Quantity Discount Table:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList CssClass="comboBox" ID="ddDiscountTable" runat="Server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Start Date:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                    <AjaxToolkit:CalendarExtender runat="server"
                                                                        TargetControlID="txtStartDate"
                                                                        Format="d" ID="calStartDate" />
                                                                    <asp:Literal ID="ltStartDate" runat="server"></asp:Literal>
                                                                    &nbsp; Stop Date:
                                                                    <asp:TextBox ID="txtStopDate" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                    <AjaxToolkit:CalendarExtender runat="server"
                                                                        TargetControlID="txtStopDate"
                                                                        Format="d" ID="calStopDate" />
                                                                    <asp:Literal ID="ltStopDate" runat="server"></asp:Literal>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" align="center" style="height: 10px;">
                                                                    <asp:CustomValidator ID="valDaterange" ControlToValidate="txtStartDate" EnableClientScript="true"
                                                                        SetFocusOnError="true" ValidationGroup="Main" Display="Static" ErrorMessage="Invalid Date Range for Start Date (Main Tab)"
                                                                        ForeColor="red" Font-Bold="true" runat="server" OnServerValidate="ValidateDateRange"></asp:CustomValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" style="height: 10px">
                                                                    *Show Buy Button:
                                                                </td>
                                                                <td align="left" valign="bottom">
                                                                    <asp:RadioButtonList ID="rblShowBuyButton" runat="server" RepeatColumns="2" RepeatDirection="horizontal"
                                                                        RepeatLayout="Flow">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="True"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                    <asp:Image runat="server" ID="imgddShowBuyButton" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.ShowBuyButton %>"
                                                                        Style="cursor: normal;" />
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Requires Registration To View:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblRequiresRegistrationToView" runat="server" RepeatColumns="2"
                                                                        RepeatDirection="horizontal" RepeatLayout="Flow">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="True"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Is Call to Order:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblIsCallToOrder" runat="server" RepeatColumns="2" RepeatDirection="horizontal"
                                                                        RepeatLayout="Flow">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="True"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                    <asp:Image runat="server" ID="imgddCallToOrder" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.CallToOrder %>"
                                                                        Style="cursor: normal;" />
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Hide Price Until Cart:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblHidePriceUntilCart" runat="server" RepeatColumns="2"
                                                                        RepeatDirection="horizontal" RepeatLayout="Flow">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="True"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                    <asp:Image runat="server" ID="imgdHidePriceUntilCart" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.HidePriceUntilCart %>"
                                                                        Style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Exclude from Product Feeds:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblExcludeFroogle" runat="server" RepeatColumns="2" RepeatDirection="horizontal"
                                                                        RepeatLayout="Flow">
                                                                        <asp:ListItem Value="0" Text="No" Selected="True"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </td>
                                                            </tr>
                                                            <tr id="trKit" runat="server">
                                                                <td align="right" valign="middle">
                                                                    *Is a Kit:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblIsKit" runat="server" RepeatColumns="2" RepeatDirection="horizontal"
                                                                        RepeatLayout="Flow">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="True"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                    <asp:Literal ID="ltKit" runat="server"></asp:Literal>
                                                                </td>
                                                            </tr>
                                                            <asp:Panel ID="PopularityRow" runat="server" Visible="false">
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        Popularity:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="4" ID="txtPopularity" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td colspan="2" style="height: 10px;">
                                                                    </td>
                                                                </tr>
                                                            </asp:Panel>
                                                            <tr runat="server" id="trInventory1">
                                                                <td align="right" valign="middle">
                                                                    *Track Inventory By Size and Color:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblTrackSizeColor" runat="server" RepeatColumns="2" RepeatDirection="horizontal"
                                                                        RepeatLayout="Flow">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="True"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                    <asp:Image runat="server" ID="Image1" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.TrackInventoryBySizeAndColor %>"
                                                                        Style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr runat="server" id="trInventory2">
                                                                <td align="right" valign="middle">
                                                                    Color Option Prompt:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox MaxLength="100" ID="txtColorOption" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr runat="server" id="trInventory3">
                                                                <td align="right" valign="middle">
                                                                    Size Option Prompt:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox MaxLength="100" ID="txtSizeOption" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Requires Text Field:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblRequiresTextField" runat="server" RepeatColumns="2" RepeatDirection="horizontal"
                                                                        RepeatLayout="Flow">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="True"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                    &nbsp; Field Prompt:
                                                                    <asp:TextBox MaxLength="100" ID="txtTextFieldPrompt" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                    &nbsp; Max Length:
                                                                    <asp:TextBox MaxLength="3" ID="txtTextOptionMax" runat="server" CssClass="textbox2"></asp:TextBox>
                                                                    <asp:Image runat="server" ID="imgddTextOptionMax" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.RequiresTextField %>"
                                                                        Style="cursor: normal;" />
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
                                                                    Image Filename Override:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="singleNormal" ID="txtImageOverride" runat="Server"></asp:TextBox>
                                                                    <asp:Image runat="server" ID="imgsImageOverride" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.tooltip.ImageFilenameOverride %>"
                                                                        Style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="top">
                                                                    Icon:
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
                                                                    Medium:
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
                                                                    Large:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:FileUpload CssClass="fileUpload" ID="fuLarge" runat="Server" />
                                                                    <div>
                                                                        <asp:Literal ID="ltLarge" runat="server"></asp:Literal>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="top">
                                                                    Color Swatch:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:FileUpload CssClass="fileUpload" ID="fuSwatch" runat="Server" />
                                                                    <div>
                                                                        <asp:Literal ID="ltSwatch" runat="server"></asp:Literal>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="top">
                                                                    Swatch Image Map:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox ID="txtSwatchImageMap" runat="server" CssClass="multiLong" TextMode="multiline"></asp:TextBox>
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
                    <AjaxToolkit:TabPanel ID="pnlSummary" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Summary %>'>
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
                                                                    <asp:Literal ID="ltSummary" runat="Server"></asp:Literal>
                                                                    <telerik:radeditor id="radSummary" runat="server" editable="true">
                                                                            <ImageManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <DocumentManager  MaxUploadFileSize="11000000"  UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <FlashManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <MediaManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <SilverlightManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <TemplateManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                    </telerik:radeditor>
                                                                    <br />
                                                                    <asp:Literal ID="ltSummaryAuto" runat="server"></asp:Literal>
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
                                                                    <asp:Literal ID="ltDescriptionFile1" runat="server"></asp:Literal>
                                                                    <asp:Button ID="btnDescription" runat="server" CssClass="normalButtons" OnClick="btnDescription_Click"
                                                                        Text="Delete" />
                                                                    <asp:Literal ID="ltDescriptionFile2" runat="server"></asp:Literal>
                                                                    <asp:Literal ID="ltDescription" runat="Server"></asp:Literal>
                                                                    <telerik:radeditor id="radDescription" runat="server" editable="true">
                                                                            <ImageManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <DocumentManager  MaxUploadFileSize="11000000"   UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
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
                    <AjaxToolkit:TabPanel ID="pnlSearchEngine" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.SearchEngine %>'>
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
                                                                <td align="right" valign="top" width="150px">
                                                                    Page Title:
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="textbox" ID="txtSETitle" runat="Server" Style="width: 600px"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle" width="150px">
                                                                    Keywords:
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="textbox" ID="txtSEKeywords" runat="Server" Style="width: 600px"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle" width="150px">
                                                                    Description:
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="textbox" ID="txtSEDescription" runat="Server" Style="width: 600px"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="top" width="150px">
                                                                    No Script:
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="textbox" ID="txtSENoScript" runat="Server" TextMode="multiLine"
                                                                        Rows="3" Style="width: 600px"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr id="Tr1" runat="server">
                                                                <td align="right" valign="middle" width="150px">
                                                                    Alt Text:
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="textbox" ID="txtSEAlt" runat="Server" Style="width: 600px"></asp:TextBox>
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
                            <table border="0" cellpadding="0" cellspacing="0" class="">
                                <tr>
                                    <td>
                                        <div class="">
                                            <table cellpadding="0" cellspacing="1" border="0">
                                                <tr>
                                                    <td align="right" valign="middle">
                                                        Feed Desc (No HTML):
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:TextBox CssClass="textboxMultiLine" ID="txtFroogle" runat="Server" TextMode="multiLine"></asp:TextBox>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </ContentTemplate>
                    </AjaxToolkit:TabPanel>
                    <AjaxToolkit:TabPanel ID="pnlMappings" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Mappings %>'>
                        <ContentTemplate>
                            <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                <tr>
                                    <td class="tabShaddow">
                                    </td>
                                </tr>
                            </table>
                            <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
                                <tr>
                                    <td align="left" width="100%">
                                        <table border="0" cellpadding="0" cellspacing="0" class="" width="100%">
                                            <tr>
                                                <td width="100%">
                                                    <div class="">
                                                        <table cellpadding="0" cellspacing="1" border="0" width="100%">
                                                            <tr>
                                                                <td colspan="2" align="left" valign="top">
                                                                    Product Mappings:
                                                                    <div style="padding-top: 10px; margin-bottom: 10px;">
                                                                        <div style="border-top: dashed 1px #909090; border-bottom: dashed 1px #909090; float: left;
                                                                            height: 350px; width: 25%; overflow: auto; font-size: 10px;">
                                                                            <b>Categories:</b>
                                                                            <br />
                                                                            <asp:CheckBoxList ID="cblCategory" runat="server" CellPadding="0" CellSpacing="0"
                                                                                RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow">
                                                                            </asp:CheckBoxList>
                                                                        </div>
                                                                        <div style="border-top: dashed 1px #909090; border-bottom: dashed 1px #909090; float: left;
                                                                            height: 350px; width: 25%; overflow: auto; font-size: 10px;">
                                                                            <b>Departments:</b>
                                                                            <br />
                                                                            <asp:CheckBoxList ID="cblDepartment" runat="server" CellPadding="0" CellSpacing="0"
                                                                                RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow">
                                                                            </asp:CheckBoxList>
                                                                        </div>
                                                                        <div runat="server" id="lblAffiliate" style="border-top: dashed 1px #909090; border-bottom: dashed 1px #909090;
                                                                            float: left; height: 350px; width: 24%; overflow: auto; font-size: 10px;">
                                                                            <b>Affiliates:</b>
                                                                            <br />
                                                                            <asp:CheckBoxList ID="cblAffiliates" runat="server" CellPadding="0" CellSpacing="0"
                                                                                RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow">
                                                                            </asp:CheckBoxList>
                                                                        </div>
                                                                        <div runat="server" id="lblCustLevels" style="border-top: dashed 1px #909090; border-bottom: dashed 1px #909090;
                                                                            float: left; height: 350px; width: 24%; overflow: auto; font-size: 10px;">
                                                                            <b>Customer Levels:</b>
                                                                            <br />
                                                                            <asp:CheckBoxList ID="cblCustomerLevels" runat="server" CellPadding="0" CellSpacing="0"
                                                                                RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow">
                                                                            </asp:CheckBoxList>
                                                                        </div>
                                                                        <div style="border-top: dashed 1px #909090; border-bottom: dashed 1px #909090; float: left;
                                                                            height: 350px; width: 24%; overflow: auto; font-size: 10px; visibility: hidden;">
                                                                            <b>Genres:</b>
                                                                            <br />
                                                                            <asp:CheckBoxList ID="cblGenres" runat="server" CellPadding="0" CellSpacing="0" RepeatColumns="1"
                                                                                RepeatDirection="Vertical" RepeatLayout="Flow">
                                                                            </asp:CheckBoxList>
                                                                        </div>
                                                                        <div style="border-top: dashed 1px #909090; border-bottom: dashed 1px #909090; float: left;
                                                                            height: 350px; width: 24%; overflow: auto; font-size: 10px; visibility: hidden;">
                                                                            <b>Vectors:</b>
                                                                            <br />
                                                                            <asp:CheckBoxList ID="cblVectors" runat="server" CellPadding="0" CellSpacing="0"
                                                                                RepeatColumns="1" RepeatDirection="Vertical" RepeatLayout="Flow">
                                                                            </asp:CheckBoxList>
                                                                        </div>
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
                    <AjaxToolkit:TabPanel ID="pnlStoreMappings" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.StoreMapping %>'>
                        <ContentTemplate>
                            <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                <tr>
                                    <td class="tabshaddow">
                                        <aspdnsf:EntityToStore ID="etsMapper" EntityType="Product" runat="server" />
                                    </td>
                                </tr>
                            </table>
                        </ContentTemplate>
                    </AjaxToolkit:TabPanel>
                    <AjaxToolkit:TabPanel ID="pnlOptions" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Options %>'>
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
                                                <td valign="top">
                                                    <div class="">
                                                        <table cellpadding="0" cellspacing="1" border="0" width="100%">
                                                            <tr>
                                                                <td align="right" valign="middle" style="width: 220px">
                                                                    Related Products:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox" ID="txtRelatedProducts" runat="Server"></asp:TextBox>
                                                                    <asp:Image runat="server" ID="imgRelatedProducts" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.RelatedProducts %>"
                                                                        Style="cursor: normal;" />
                                                                    <br />
                                                                    <asp:RegularExpressionValidator ID="valRelatedProducts" ControlToValidate="txtRelatedProducts"
                                                                        Display="None" runat="SERVER" ValidationGroup="Main" ValidationExpression="[0-9,]*"
                                                                        ErrorMessage="Related Products must be comma seperated product IDs. (Options Tab)"></asp:RegularExpressionValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Upsell Products:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox" ID="txtUpsellProducts" runat="Server"></asp:TextBox>
                                                                    <asp:Image runat="server" ID="imgUpsellProducts" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.UpsellProducts %>"
                                                                        Style="cursor: normal;" />
                                                                    <br />
                                                                    <asp:RegularExpressionValidator ID="valUpsellProducts" ControlToValidate="txtUpsellProducts"
                                                                        Display="None" runat="SERVER" ValidationGroup="Main" ValidationExpression="[0-9,]*"
                                                                        ErrorMessage="Upsell Products must be comma seperated product IDs. (Options Tab)"></asp:RegularExpressionValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Upsell Product Discount Percent:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox MaxLength="5" CssClass="textbox" ID="txtUpsellProductsDiscount" runat="Server"></asp:TextBox>
                                                                    <asp:Image runat="server" ID="imgUpsellProductsDiscount" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.UpsellProductDiscount %>"
                                                                        Style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Requires Products:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox" ID="txtRequiresProducts" runat="Server"></asp:TextBox>
                                                                    <asp:Image runat="server" ID="imgRequiresProducts" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.RequiresProducts %>"
                                                                        Style="cursor: normal;" />
                                                                    <br />
                                                                    <asp:RegularExpressionValidator ID="valRequiredProducts" ControlToValidate="txtRequiresProducts"
                                                                        Display="None" runat="SERVER" ValidationGroup="Main" ValidationExpression="[0-9,]*"
                                                                        ErrorMessage="Required Products must be comma seperated product IDs. (Options Tab)"></asp:RegularExpressionValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *'On Sale' Prompt:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList ID="ddOnSalePrompt" runat="server">
                                                                    </asp:DropDownList>
                                                                    <asp:RequiredFieldValidator ControlToValidate="ddOnSalePrompt" InitialValue="0" ErrorMessage="Please select an 'On Sale' prompt. (Options Tab)"
                                                                        ID="RequiredFieldValidator4" ValidationGroup="Main" EnableClientScript="true"
                                                                        SetFocusOnError="true" runat="server" Display="Static">!!</asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Spec Title:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox" ID="txtSpecTitle" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Spec Call:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox" ID="txtSpecCall" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    *Specs Inline:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblSpecsInline" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </td>
                                                            </tr>
                                                            <tr id="trSpecs" runat="server">
                                                                <td align="right" valign="middle">
                                                                    Specifications:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:Literal ID="ltSpecs1" runat="server"></asp:Literal>
                                                                    <asp:Button ID="btnSpecs" runat="server" OnClick="btnSpecs_Click" CssClass="normalButtons"
                                                                        Text="Delete" />&nbsp;
                                                                    <asp:Image ID="imgbtnSpecs" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                        ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.Specifications %>"
                                                                        Style="cursor: normal;" Visible="false" />
                                                                    <asp:Literal ID="ltSpecs2" runat="server"></asp:Literal>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="height: 10px;">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Page BG Color:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox" ID="txtPageBG" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Contents BG Color:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox" ID="txtContentsBG" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    Skin Graphics Color:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox" ID="txtSkinColor" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </td>
                                                <td width="500px" valign="top">
                                                    <div style="width:500px;">
                                                        
                                                        
                                                        <div id="CSVHelpers">
                                                            <div class="helpercontainer">
                                                                <h2 class="head">
                                                                    Related Products Helper
                                                                    <asp:Image runat="server" ID="ttRelatedHelper" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                            border="0" ToolTip="The Related Products Helper allows you to easily search for, add, and remove related product IDs. Click here to show or hide the helper."
                                                                            Style="cursor: normal;" CssClass="helperTT" />
                                                                </h2>
                                                                <div>
                                                                    <aspdnsf:CSVHelper ID="relatedHelper" runat="server" CSVTextBoxID="txtRelatedProducts" UniqueJSID="Related" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                                </div>
                                                            </div>
                                                            <div class="helpercontainer">
                                                                <h2 class="head">
                                                                    Upsell Products Helper
                                                                    <asp:Image runat="server" ID="ttUpsellHelper" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                            border="0" ToolTip="The Upsell Products Helper allows you to easily search for, add, and remove upsell product IDs. Click here to show or hide the helper."
                                                                            Style="cursor: normal;" CssClass="helperTT" />
                                                                </h2>
                                                                <div>
                                                                    <aspdnsf:CSVHelper ID="upsellHelper" runat="server" CSVTextBoxID="txtUpsellProducts" UniqueJSID="Upsell" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                                </div>
                                                            </div>
                                                             <div class="helpercontainer">
                                                                <h2 class="head">
                                                                    Required Products Helper
                                                                    <asp:Image runat="server" ID="ttRequiredHelper" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                            border="0" ToolTip="The Required Products Helper allows you to easily search for, add, and remove required product IDs. Click here to show or hide the helper."
                                                                            Style="cursor: normal;" CssClass="helperTT" />
                                                                </h2>
                                                                <div>
                                                                    <aspdnsf:CSVHelper ID="requiresHelper" runat="server" CSVTextBoxID="txtRequiresProducts" UniqueJSID="Requires" CSVSearchButtonText= "<%$Tokens:StringResource, admin.common.Search%>" />
                                                                </div>
                                                            </div>
                                                        </div>
                                                        
                                                        <script type="text/javascript">
                                                            jQuery(document).ready(function() {
                                                            $('#CSVHelpers .head').click(function() {
                                                                    $(this).next().toggle('fast');
                                                                    return false;
                                                                }).next().hide();
                                                            });                                                 
                                                        </script>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </ContentTemplate>
                    </AjaxToolkit:TabPanel>
                    <AjaxToolkit:TabPanel ID="pnlExtensionData" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.ExtensionData %>'>
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
                                                                <td align="right" valign="top">
                                                                    Extension Data (User Defined Data):
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textboxMultiLine" ID="txtExtensionData" runat="Server" TextMode="multiLine"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="top">
                                                                    Miscellaneous Text:
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textboxMultiLine" ID="txtMiscText" runat="Server" TextMode="multiLine"></asp:TextBox>
                                                                    <br />
                                                                    <asp:HyperLink ID="lnkAutoFill" runat="Server" Visible="False" NavigateUrl="" Target="_blank"
                                                                        Text="AutoFill Variants" />
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
                    <AjaxToolkit:TabPanel ID="pnlVariant" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Variant %>'>
                        <ContentTemplate>
                            <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                <tr>
                                    <td class="tabShaddow">
                                    </td>
                                </tr>
                            </table>
                            <asp:PlaceHolder runat="Server" ID="phAddVariant">
                                <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
                                    <tr>
                                        <td align="left" width="100%">
                                            <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                                                <tr>
                                                    <td width="100%">
                                                        <div class="wrapperExtraTop">
                                                            <table cellpadding="0" cellspacing="1" border="0" width="100%">
                                                                <tr>
                                                                    <td align="right" valign="top">
                                                                        Price:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="10" ID="txtPrice" runat="server" CssClass="singleShortest"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="top">
                                                                        Sale Price:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="10" ID="txtSalePrice" runat="server" CssClass="singleShortest"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        Weight:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="10" ID="txtWeight" runat="server" CssClass="single3chars"></asp:TextBox>
                                                                        <asp:Image runat="server" ID="imgWeight" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                            border="0" ToolTip="<%$ Tokens:StringResource, admin.entityeditproduct.ToolTip.Weight %>"
                                                                            Style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="top">
                                                                        Dimensions:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="10" ID="txtDimensions" runat="server" CssClass="singleShorter"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr id="trInventory" runat="server">
                                                                    <td align="right" valign="top">
                                                                        Current Inventory:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox MaxLength="10" ID="txtInventory" runat="server" CssClass="singleShorter"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        Colors:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtColors" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" ID="imColors" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                            border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.ToolTip.Colors %>"
                                                                            Style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        Color SKU Modifiers:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtColorSKUModifiers" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" ID="imgColorSKUModifiers" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                            border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.ToolTip.ColorSkuMods %>"
                                                                            Style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        Sizes:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtSizes" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" ID="imgSizes" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                            border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.ToolTip.Sizes %>"
                                                                            Style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        Size SKU Modifiers:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtSizeSKUModifiers" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                        <asp:Image runat="server" ID="imgSizeSKUModifiers" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                                                                            border="0" ToolTip="<%$ Tokens:StringResource, admin.editproductvariant.ToolTip.SizeSkuMods %>"
                                                                            Style="cursor: normal;" />
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
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="phAllVariants" runat="server">
                                <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
                                    <tr>
                                        <td align="left">
                                            <table border="0" cellpadding="0" cellspacing="0" class="innerTable">
                                                <tr>
                                                    <td>
                                                        <div class="wrapperExtraTop">
                                                            <table cellpadding="0" cellspacing="1" border="0" width="100%">
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        Action:
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:Literal ID="ltVariantsLinks" runat="server"></asp:Literal>
                                                                        |
                                                                        <asp:LinkButton ID="btnDeleteAll" runat="server" Text="Delete All Variants" OnClick="btnDeleteAll_Click" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle" colspan="2">
                                                                        <asp:Literal ID="ltIFrame" runat="server"></asp:Literal>
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
                            </asp:PlaceHolder>
                        </ContentTemplate>
                    </AjaxToolkit:TabPanel>
                </AjaxToolkit:TabContainer>
            </div>
            <div>
                <asp:Button ID="btnSubmit" runat="server" CssClass="normalButtons" OnClick="btnSubmit_Click"
                    ValidationGroup="Main" />
                &nbsp; &nbsp;
                <asp:Button ID="btnClose" runat="server" CssClass="normalButtons" OnClick="btnClose_Click"
                    ValidationGroup="Main" Text="Save and Close" />
                &nbsp; &nbsp;
                <asp:Button ID="btnCancel" runat="server" CssClass="normalButtons" OnClick="btnCancel_Click"
                    ValidationGroup="Main" Text="Cancel" OnClientClick="return confirm('Are you sure you wish to cancel?  All unsaved information will be lost.');" /></div>
        </div>
        <asp:ValidationSummary ID="validationSummary" runat="Server" ValidationGroup="Main"
            DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" />
        <br />
        <br />
        <asp:Literal ID="ltScript" runat="server"></asp:Literal>
        <asp:Literal ID="ltScript2" runat="server"></asp:Literal>
        <asp:Literal ID="ltScript3" runat="server"></asp:Literal>
    </asp:Panel>
    </form>
</body>
</html>
