<%@ Page Language="C#" AutoEventWireup="true" CodeFile="entityEdit.aspx.cs" Inherits="AspDotNetStorefrontAdmin.entityEdit" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityProductMap" src="controls/EntityProductMap.ascx" %>
<%@ OutputCache Duration="1" Location="none" %>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <script type="text/javascript" src="Scripts/jquery.min.js"></script>
    <script type="text/javascript" src="Scripts/jquery-ui-1.8.17.custom.min.js"></script>
    <script type="text/javascript" src="Scripts/jquery.ui.selectmenu.js"></script>
    <link type="text/css" href="css/redmond/jquery-ui-1.8.17.custom.css" />

    <script type="text/javascript">
	$(function() {
	    $('#tabContainer_pnlMain_ddParent').selectmenu();
	});
    //BEGIN jquery ui compatibility fix for jquery 10.x
	$.curCSS = function (element, attrib, val) {
	    $(element).css(attrib, val);
	};
    //END jquery ui compatibility fix for jquery 10.x
    </script>

    <script type="text/javascript">
        function CloseAndRebind(entityID) {
            var currentWindow = GetRadWindow();
            if (currentWindow) {
                var args = new Object();
                args = entityID;
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

        function CancelEdit(entityID) {
            var currentWindow = GetRadWindow();
            if (currentWindow) {
                var args = new Object();
                args = entityID;
                currentWindow.argument = args;
            }
            GetRadWindow().BrowserWindow.refreshGrid(args);
            GetRadWindow().Close();
        }
    </script>

</head>

<body>
    <form runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <asp:Panel ID="pnlContent" runat="server">
        <asp:Literal runat="server" ID="Literal1"></asp:Literal>
        <div style="width: 100%;">
            <table style="width: 100%">
                <tr>
                    <td style="width: 70%" valign="top">
                        <div class="breadCrumb3">
                            <div style="float: left">
                                <asp:Literal ID="ltPreEntity" runat="server"></asp:Literal></div>
                            <div style="float: left">
                                &nbsp;<asp:Label ID="Label2" runat="server" Text="View :"></asp:Label>&nbsp;</div>
                            <div>
                                <asp:Literal ID="ltEntity" runat="server"></asp:Literal></div>
                        </div>
                        <div class="breadCrumbTitleText">
                            <div style="float: left" class="breadCrumb3">
                                <asp:Label ID="Label1" runat="server" Text="Status :"></asp:Label></div>
                            <div>
                                &nbsp;<asp:Literal ID="ltStatus" runat="server"></asp:Literal></div>
                        </div>
                    </td>
                    <td align="right" style="width: 30%" valign="top">
                        <div id="" style="float: right; margin-right: 5px;">
                            <table border="0" cellpadding="1" cellspacing="0" class="" runat="server" id="tblLocale">
                                <tr>
                                    <td>
                                        <div class="">
                                            <table border="0" cellpadding="0" cellspacing="0" class="">
                                                <tr>
                                                    <td class="titleTable">
                                                        <font class="">
                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Locale %>" /></font>
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
                                                                    <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Name %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox2" ID="txtName" runat="Server"></asp:TextBox>
                                                                    <asp:RequiredFieldValidator ControlToValidate="txtName" ID="rfvName" ValidationGroup="Main"
                                                                        EnableClientScript="true" SetFocusOnError="true" runat="server" Display="Static">!!</asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.currencies.Published %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblPublished" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </td>
                                                            </tr>
                                                            <tr id="trBrowser" runat="server">
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ShowProductBrowser %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblBrowser" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </td>
                                                            </tr>
                                                            <tr id="trParent" runat="server">
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Parent %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList ID="ddParent" Width="400px" runat="Server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.DisplayFormatXmlPackage %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList ID="ddXmlPackage" runat="Server">
                                                                    </asp:DropDownList>
                                                                    <asp:Image runat="server" id="imgXmlPackage" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.entityedit.tooltip.XMLPackage %>"
                                                                        style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.QuantityDiscountTable %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:DropDownList ID="ddDiscountTable" runat="Server">
                                                                    </asp:DropDownList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.PageSize %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox MaxLength="2" ID="txtPageSize" runat="server" CssClass="single3chars"></asp:TextBox>
                                                                    <asp:Image runat="server" id="imgPageSize" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.entityedit.tooltip.PageSize %>"
                                                                        style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ColumnWidth %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox MaxLength="2" ID="txtColumn" runat="server" CssClass="single3chars"></asp:TextBox>
                                                                    <asp:Image runat="server" id="imgColumn" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.entityedit.tooltip.ColumnWidth %>"
                                                                        style="cursor: normal;" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.OrderProductsByLooks %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:RadioButtonList ID="rblLooks" runat="server" RepeatColumns="2" RepeatDirection="horizontal">
                                                                        <asp:ListItem Value="0" Text="No"></asp:ListItem>
                                                                        <asp:ListItem Value="1" Text="Yes" Selected="true"></asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </td>
                                                            </tr>
                                                            <!-- Address -->
                                                            <asp:PlaceHolder ID="phAddress" runat="Server">
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Address %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="textbox2" ID="txtAddress1" runat="Server"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.AptSuite %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="singleShortest" ID="txtApt" runat="Server"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Address2 %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="textbox2" ID="txtAddress2" runat="Server"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.City %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="singleShorter" ID="txtCity" runat="Server"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.State %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:DropDownList ID="ddState" runat="server">
                                                                        </asp:DropDownList>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ZipCode %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox CssClass="singleShortest" ID="txtZip" runat="Server"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Country %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:DropDownList ID="ddCountry" runat="server">
                                                                        </asp:DropDownList>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.E-Mail %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>
                                                                        <aspdnsf:EmailValidator ErrorMessage="Invalid Email Address Format (Main Tab)" ID="valRegExValEmail"
                                                                            ControlToValidate="txtEmail" runat="server" ValidationGroup="Main"></aspdnsf:EmailValidator>
                                                                         <asp:RequiredFieldValidator ErrorMessage="Fill in E-Mail (Main Tab)" ControlToValidate="txtEmail"
                                                                            ID="rfvEmail" ValidationGroup="Main" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Website %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtURL" runat="server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgURL" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.entityedit.tooltip.Website %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Phone %>" /></font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtPhone" runat="server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgPhone" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.entityedit.tooltip.Phone %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="right" valign="middle">
                                                                        <font class="subTitleSmall">
                                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Fax %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:TextBox ID="txtFax" runat="server"></asp:TextBox>
                                                                        <asp:Image runat="server" id="imgFax" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" ToolTip="<%$ Tokens:StringResource, admin.entityedit.tooltip.Fax %>"
                                                                            style="cursor: normal;" />
                                                                    </td>
                                                                </tr>
                                                            </asp:PlaceHolder>
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
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ImageFilenameOverride %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="textbox2" ID="txtImageOverride" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="top">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Icon %>" />:</font>
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
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Medium %>" />:</font>
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
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Large %>" />:</font>
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
                                                                    <telerik:radeditor runat="server" id="radSummary">
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
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.ExtensionData %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="multiExtension" ID="txtExtensionData" runat="Server" TextMode="multiLine"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr id="Tr2" runat="server" visible="false">
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.UseSkinTemplate %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="singleShorter" ID="txtUseSkinTemplateFile" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr id="Tr1" runat="server" visible="false">
                                                                <td align="right" valign="middle">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.UseSkinID %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox CssClass="singleShorter" ID="txtUseSkinID" runat="Server"></asp:TextBox>
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
                                    </td>
                                </tr>
                            </table>
                            <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                    <tr>
                                        <td align="left" width="100%">
                                            <aspdnsf:EntityToStore ID="etsMapper" EntityType="Category" runat="server" />
                                            <asp:Panel ID="pnlStoreMapNotSupported" runat="server" Visible="false">
                                                <strong style="display:block;padding:10px 5px;">Store Mapping is not supported for this entity.</strong>
                                             </asp:Panel>
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
                                                                <td align="right" valign="top" width="150">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.PageTitle %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="singleAuto" ID="txtSETitle" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle" width="150">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.Keywords %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="singleAuto" ID="txtSEKeywords" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="middle" width="150">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Description %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="singleAuto" ID="txtSEDescription" runat="Server"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right" valign="top" width="150">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.NoScript %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="multiAutoNormal" ID="txtSENoScript" runat="Server" TextMode="multiLine"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr id="Tr3" runat="server">
                                                                <td align="right" valign="middle" width="150">
                                                                    <font class="subTitleSmall">
                                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.AltText %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle" width="*">
                                                                    <asp:TextBox CssClass="singleAuto" ID="txtSEAlt" runat="Server"></asp:TextBox>
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
                    <AjaxToolkit:TabPanel ID="pnlProducts" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.Products %>'>
                        <ContentTemplate>
                            <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                <tr>
                                    <td class="tabShaddow">
                                    </td>
                                </tr>
                            </table>
                            <asp:PlaceHolder runat="Server" ID="phProducts">
                                <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
                                    <tr>
                                        <td align="left" width="100%">
                                            <table border="0" cellpadding="0" cellspacing="0" class="" width="100%">
                                                <tr>
                                                    <td width="100%">
                                                        <div class="">
                                                            <table cellpadding="0" cellspacing="1" border="0" width="100%">
                                                                <tr style="padding-top: 10px;">
                                                                    <td align="right" valign="top">
                                                                        <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entity.Products %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:LinkButton ID="lnkProducts" runat="server" OnClick="lnkProducts_Click"></asp:LinkButton>
                                                                    </td>
                                                                </tr>
                                                                <tr style="padding-top: 10px;">
                                                                    <td align="right" valign="top">
                                                                        <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entityEdit.BulkProducts %>" />:</font>
                                                                    </td>
                                                                    <td align="left" valign="middle">
                                                                        <asp:LinkButton ID="lnkBulkDisplayOrder" runat="server" OnClick="lnkBulkDisplayOrder_Click">Display Order</asp:LinkButton>&nbsp;|&nbsp;
                                                                        <asp:LinkButton ID="lnkBulkInventory" runat="server" OnClick="lnkBulkInventory_Click">Inventory</asp:LinkButton>&nbsp;|&nbsp;
                                                                        <asp:LinkButton ID="lnkBulkSEFields" runat="server" OnClick="lnkBulkSEFields_Click">SE Fields</asp:LinkButton>&nbsp;|&nbsp;
                                                                        <asp:LinkButton ID="lnkBulkPrices" runat="server" OnClick="lnkBulkPrices_Click">Prices</asp:LinkButton>&nbsp;|&nbsp;
                                                                        <asp:LinkButton ID="lnkBulkDownloadFiles" runat="server" OnClick="lnkBulkDownloadFiles_Click">Download Files</asp:LinkButton>&nbsp;|&nbsp;
                                                                        <asp:LinkButton ID="lnkBulkShippingMethods" runat="server" OnClick="lnkBulkShippingMethods_Click">Shipping Costs</asp:LinkButton>
                                                                    </td>
                                                                </tr>
                                                                <tr id="trBulkFrame" runat="server" visible="false">
                                                                    <td colspan="2" width="100%">
                                                                        <iframe name="bulkFrame" id="bulkFrame" runat="server" frameborder="0" scrolling="auto" width="100%" marginheight="0" marginwidth="0" height="500"></iframe>
                                                                    </td>
                                                                </tr>
                                                                <tr id="trProductMap" runat="server">
                                                                    <td align="left" valign="middle" colspan="2">
                                                                        <aspdnsf:EntityProductMap id="ctrlEntityProductMap" runat="server" />
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
                    <AjaxToolkit:TabPanel ID="pnlDisplayOrder" runat="server" HeaderText='<%$ Tokens:StringResource, admin.tabs.DisplayOrder %>'>
                        <ContentTemplate>
                            <table cellpadding="0" cellspacing="0" border="0" style="width: 100%">
                                <tr>
                                    <td class="tabShaddow">
                                    </td>
                                </tr>
                            </table>
                            <table border="0" cellpadding="1" cellspacing="0" class="" style="width: 100%; background-color: #ffffff">
                                <tr>
                                    <td align="left" width="100%">
                                        <asp:Literal ID="ltIFrame" Mode="PassThrough" runat="server"></asp:Literal>
                                    </td>
                                </tr>
                            </table>
                        </ContentTemplate>
                    </AjaxToolkit:TabPanel>
                </AjaxToolkit:TabContainer>
            </div>
            <div style="width: 100%; text-align: left; padding-top: 10px;">
                &nbsp;&nbsp;<asp:Button ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" CssClass="normalButtons"
                    ValidationGroup="Main" />
                &nbsp; &nbsp;
                <asp:Button ID="btnClose" runat="server" CssClass="normalButtons" OnClick="btnClose_Click"
                    ValidationGroup="Main" Text="Save and Close" />
                &nbsp; &nbsp;
                <asp:Button ID="btnCancel" runat="server" CssClass="normalButtons" OnClick="btnCancel_Click"
                    ValidationGroup="Main" Text="Cancel" CausesValidation="false" OnClientClick="return confirm('Are you sure you wish to cancel?  All unsaved information will be lost.');" />
                <asp:ValidationSummary ID="validationSummary" runat="Server" ValidationGroup="Main"
                    DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" />
            </div>
        </div>
        <asp:Literal ID="ltScript" runat="server"></asp:Literal>
        <asp:Literal ID="ltScript2" runat="server"></asp:Literal>
    </asp:Panel>
    </form>
</body>
</html>
