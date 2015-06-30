<%@ Page Language="C#" AutoEventWireup="true" CodeFile="orderoption.aspx.cs" Inherits="AspDotNetStorefrontAdmin.orderOption" 
 MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"%>
<%@ Register TagPrefix="aspdnsf" TagName="Store" src="controls/StoreSelector.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal runat="server" ID="ltStyles"></asp:Literal>    
    <asp:Literal ID="ltScript" runat="server"></asp:Literal>
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;" class="breadCrumb3">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
            <asp:Label ID="lblMsg" runat="server"></asp:Label>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable" width="185">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderOption.OptionList %>" /></font>:
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #EBEBEB;" />
                                <td style="width: 5px;" />
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderOption.OptionDetails %>" /></font>:
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="185">
                                    <div style="width: 100%" runat ="server" id="divLocale" class="wrapperTop">
                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderOption.ShowLocaleSetting %>" />:
                                        <asp:DropDownList ID="ddLocales" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddLocales_SelectedIndexChanged"></asp:DropDownList>
                                    </div>
                                    <div class="wrapperTop">
                                        <asp:Button runat="server" ID="btnAdd" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.AddNewUC %>" OnClick="btnAdd_Click" />                                   
                                    </div>                                    
                                    <div class="wrapperTop">                                    
                                            <asp:DataList ID="dlOrderOptions" runat="server" onitemcommand="ViewOrderOption" DataKeyField="Id">                                            
                                                <HeaderTemplate>[Display Order]&nbsp;<b>Options</b></HeaderTemplate>
                                                <ItemTemplate>
                                                        <asp:Image ID="Img1" ImageUrl="~/App_Themes/Admin_Default/images/icons/dot.gif" runat="server" />
                                                        <asp:TextBox ID="txtDisplayOrder" runat="server"  Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>' Width="24px">
                                                        </asp:TextBox>                                       
                                                        <asp:LinkButton ID="lnkOrderOptionName" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'>
                                                        </asp:LinkButton>                                                    
                                                </ItemTemplate>
                                            </asp:DataList>                                      
                                    </div>    
                                    <div class="wrapperTop">
                                        <asp:Button id="btnUpdate" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.orderOption.UpdateDisplayOrder %>" OnClick="btnUpdate_Click" />
                                    </div>                      
                                </td>                                
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #EBEBEB;" />
                                <td style="width: 5px;" />
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">
                                        <asp:PlaceHolder ID="phMain" runat="server">
                                            <font class="titleMessage">
                                                <asp:Literal runat="server" ID="ltMode"></asp:Literal><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.OrderOption %>" />
                                            </font>
                                            <div style="margin-top: 10px;"></div>
                                            <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                                <tr>
                                                    <td style="width: 260px"  align="right" valign="top">
                                                        <div id="divlblLocale" class="subTitleSmall" runat="server">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Locale %>" />:
                                                        </div>                                                        
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <div id="divPageLocale" runat="server">
                                                            <asp:DropDownList ID="ddlPageLocales" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlPageLocales_SelectedIndexChanged"></asp:DropDownList>
                                                        </div>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 260px"  align="right" valign="middle">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" /></td>
                                                    <td align="left" valign="top">
                                                        <aspdnsf:EntityToStore runat="server" ID="StoresMapping" EntityType="OrderOption" ShowText = "false"/>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td align="right" valign="top" width="260">
                                                        <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderOption.OptionName %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:TextBox runat="server" ID="ltName"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td align="right" valign="top">
                                                        <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Description %>" /></font>
                                                    </td>
                                                    <td align="left" valign="top">
                                                        <asp:Literal ID="ltDescription" runat="server"></asp:Literal>
                                                        <telerik:RadEditor id="radDescription" runat="server">
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
                                                        <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.editorderoption.DefaultIsChecked %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:RadioButtonList ID="rbIsChecked" runat="server" RepeatDirection="horizontal">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>" />
                                                            <asp:ListItem Value="1" Selected="true" Text="<%$Tokens:StringResource, admin.common.Yes %>" />
                                                        </asp:RadioButtonList>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td align="right" valign="middle">
                                                        <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Cost %>" /></font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:TextBox ID="txtCost" runat="server" CssClass="singleShortest"></asp:TextBox>
                                                        <asp:Image ID="imgCost" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" Tooltip="<%$Tokens:StringResource,admin.orderOption.tooltip.imgCost %>" runat="server" />
                                                    </td>
                                                </tr>  
                                                    <tr>
                                                        <td align="right" valign="middle">
                                                            <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.orderOption.TaxClass %>" />:</font>
                                                        </td>
                                                        <td align="left" valign="middle">
                                                            <asp:DropDownList ID="ddTaxClass" runat="Server"></asp:DropDownList>
                                                        </td>
                                                    </tr>
                                                <tr>
                                                    <td align="right" valign="top">
                                                        <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Icon %>" /></font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:FileUpload CssClass="fileUpload" ID="fuIcon" runat="Server" />
                                                        <div>
                                                            <asp:Literal ID="ltIcon" runat="server"></asp:Literal>
                                                        </div>
                                                    </td>
                                                </tr>                                              
                                            </table>
                                            <div style="width: 100%; text-align: center; padding-top: 10px;">
                                                &nbsp;&nbsp;<asp:Button ID="btnSubmit" runat="server" CssClass="normalButtons" OnClick="btnSubmit_Click"  OnClientClick="return validate();" />
                                                &nbsp;&nbsp;<asp:Button ID="btnDelete" runat="server" CssClass="normalButtons" OnClick="btnDelete_Click" Text="<%$Tokens:StringResource, admin.orderOption.DeleteOrderOption %>" />
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
        <asp:Literal ID="ltScript2" runat="server"></asp:Literal>
    </div>
</asp:Content>