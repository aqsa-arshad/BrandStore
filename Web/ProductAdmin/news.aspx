<%@ Page Language="C#" AutoEventWireup="true" CodeFile="news.aspx.cs" Inherits="AspDotNetStorefrontAdmin.news" MaintainScrollPositionOnPostback="true"
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %> 

<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

    <asp:Literal runat="server" ID="ltStyles"></asp:Literal>
    
    <div class="breadCrumb3">
    <asp:Literal ID="ltScript1" runat="server"></asp:Literal></div>
    
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;" class="breadCrumb3">
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
                                <td class="titleTable" width="150">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.news.NewsList %>" /></font>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #EBEBEB;" />
                                <td style="width: 5px;" />
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.news.NewsDetails %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="150">
				                    <div class="wrapperTopBottom">
					                    <asp:Button runat="server" ID="btnAdd" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.AddNewUC %>" OnClick="btnAdd_Click" />
				                    </div>
                                    <div class="wrapperTopBottom">
                                        <asp:TreeView ID="treeMain" runat="server" OnSelectedNodeChanged="treeMain_SelectedNodeChanged">
                                        </asp:TreeView>
                                    </div>                                    
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #EBEBEB;" />
                                <td style="width: 5px;" />
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">
                                        <asp:PlaceHolder ID="phMain" runat="server">
                                            <div style="width: 100% " runat ="server" id="div1" class="titleMessage">
                                                <asp:Literal runat="server" ID="ltMode"></asp:Literal><asp:Literal runat="server" />
                                            </div>
                                            
                                            <div style="width: 100%" runat ="server" id="divPageLocale" class="titleMessage">
                                                &nbsp;&nbsp;<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.SelectLocale %>" /><asp:DropDownList ID="ddlPageLocales" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlPageLocales_SelectedIndexChanged"></asp:DropDownList>
                                            </div>
                                            
                                            <p>
                                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editnews.NewsInfo %>" />
                                            </p>
                                            <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                                <tr>
                                                    <td align="right" valign="top">
                                                        <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editnews.Headline %>" /></font>
                                                    </td>
                                                    <td align="left" valign="top">
                                                        <asp:TextBox runat="server" ID="ltHeadline" Columns="30"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td align="right" valign="top">
                                                        <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editnews.NewsCopy %>" /></font>
                                                    </td>
                                                    <td align="left" valign="top">
                                                        <asp:Literal ID="ltCopy" runat="server"></asp:Literal>
                                                        <telerik:RadEditor id="radCopy" runat="server">
                                                                            <ImageManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <DocumentManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <FlashManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <MediaManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <SilverlightManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                                            <TemplateManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                                        </telerik:RadEditor>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td align="right" valign="middle">
                                                        <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editnews.ExpirationDate %>" /></font>
                                                    </td>
                                                    <td align="left" valign="top">
					                                    <telerik:RadDatePicker ID="txtDate" runat="server" Style="z-index: 150000;">
															<Calendar UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
															<DatePopupButton HoverImageUrl="" ImageUrl="" />
														</telerik:RadDatePicker>
                                                        <asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinExpirationDate %>" ControlToValidate="txtDate" ID="RequiredFieldValidator1" SetFocusOnError="true" runat="server" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td align="right" valign="middle">
                                                        <font class="subTitleSmall"><asp:Literal runat="server" Text='<%$Tokens:StringResource, admin.common.Published %>' /></font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:RadioButtonList ID="rbPublished" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>" />
                                                            <asp:ListItem Value="1" Selected="true" Text="<%$Tokens:StringResource, admin.common.Yes %>" />
                                                        </asp:RadioButtonList>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td valign="middle" align="right">
                                                        <asp:Panel ID="litStoreMapperHdr" runat="server">
                                                            <font class="subTitleSmall">
                                                            <asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" />
                                                            </font>
                                                        </asp:Panel>
                                                    </td>
                                                    <td>
                                                        <asp:Panel ID="litStoreMapper" runat="server" >
                                                            <aspdnsf:EntityToStore ID="etsMapper" runat="server"
                                                                EntityType="News" 
                                                                Text=""
                                                                />
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:ValidationSummary ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                                    </td>
                                                </tr>
                                            </table>
                                            <div style="width: 100%; text-align: center;">
                                                &nbsp;&nbsp;<asp:Button ID="btnSubmit" runat="server" CssClass="normalButtons" OnClick="btnSubmit_Click" />
                                                &nbsp;&nbsp;<asp:Button ID="btnDelete" runat="server" CssClass="normalButtons" OnClick="btnDelete_Click" Text="<%$Tokens:StringResource, admin.news.DeleteNewsItem %>" />
                                            </div>
                                        </asp:PlaceHolder>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <asp:Literal ID="ltScript" runat="server"></asp:Literal>
    
</asp:Content>