<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TopicEditor.ascx.cs" Inherits="AspDotNetStorefront.TopicEditor" %>
<%@ Register TagPrefix="aspdnsf" TagName="TopicToStore" Src="StoreSelector.ascx" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="tcontrol" %>
<%@ Register TagPrefix="aspdnsf" TagName="TopicMapping" Src="TopicMapping.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel ID="pnlMapTopic" runat="server" DefaultButton="btnCancelMapTopic" CssClass="modal_popup" >    
            <div class="modal_popup_Header" id="modaldiv" runat="server"><asp:Literal ID="Literal30" runat="server" Text='<%$ Tokens:StringResource, admin.topic.map %>' /></div>
            <aspdnsf:TopicMapping ID="ctrlMapTopic" runat="server" CssClass="modal_popup_Content" />
            <div align="center" class="modal_popup_Footer" >
                <asp:Button ID="btnCancelMapTopic" runat="server" Text="Close" />
            
                <div id="divProgressAdd" runat="server" style="display:none" >
                    <p style="color:#3DDB0B;font-style:italic;">Saving in progress...<asp:Image ID="Image1" Tooltip="saving" ImageUrl="~/App_Themes/Admin_Default/images/ajax-loader-1.gif" runat="server" /> </p> 
                </div>
            </div>
      

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<ajax:ModalPopupExtender ID="extTopicMap" runat="server" 
    PopupControlID="pnlMapTopic" 
    TargetControlID="lbMapTopics" 
    BackgroundCssClass="modal_popup_background"
    CancelControlID="btnCancelMapTopic"
    PopupDragHandleControlID="modaldiv">
</ajax:ModalPopupExtender>

<asp:Literal ID="ltValid" runat="server"></asp:Literal>
<div id="titlediv" runat="server">
    <div style="margin-bottom: 5px; margin-top: 5px;">
        <asp:Literal ID="ltError" runat="server"></asp:Literal>
    </div>
</div> 

<asp:Panel ID="pnlTopicEditor" runat="server">


    <div style="width: 100% " runat ="server" id="div1" class="titleMessage">
        <asp:Literal runat="server" ID="ltMode"></asp:Literal>
    </div>  
    <p>
        <asp:Literal ID="Literal10" runat="server" Text='<%$ Tokens:StringResource, admin.common.RequiredFieldsPrompt %>' />
    </p>                                            
    <table width="100%" cellpadding="1" cellspacing="0" border="0">
        <tr>
            <td width="260" align="right" valign="top">
                <font class="subTitleSmall">*<asp:Literal ID="Literal11" runat="server" Text='<%$ Tokens:StringResource, admin.topic.name %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:TextBox runat="server" ID="ltName"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td align="right" valign="top">
                <font class="subTitleSmall">*<asp:Literal ID="Literal12" runat="server" Text='<%$ Tokens:StringResource, admin.topic.title %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:TextBox runat="server" ID="ltTitle"></asp:TextBox>
            </td>
        </tr>
        <tr runat="server" id="trMapTopics">
            <td align="right" valign="top" style="padding-top:3px;">
                <font class="subTitleSmall"><asp:Literal ID="Literal13" runat="server" Text='<%$ Tokens:StringResource, admin.topic.maptopics %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:LinkButton ID="lbMapTopics" runat="server"><asp:Literal ID="Literal29" runat="server" Text='<%$ Tokens:StringResource, admin.topic.map %>' /></asp:LinkButton>
            </td>
        </tr>
        <tr runat="server" visible="false" id="trCopyToStore">
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal1" runat="server" Text='<%$ Tokens:StringResource, admin.topic.copytostore %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:DropDownList ID="ddCopyToStore" runat="server" ></asp:DropDownList>
                <asp:Button ID="btnCopyToStore" runat="server" Text="<%$ Tokens:StringResource, admin.common.Submit %>" OnClick="btnCopyToStore_Click" />
            </td>
        </tr>
        <tr id="trSkinId" runat="server">
            <td align="right" valign="middle">
                <font class="subTitleSmall">*<asp:Literal ID="Literal14" runat="server" Text='<%$ Tokens:StringResource, admin.topic.appliestoskin %>' /></font>
            </td>
            <td align="left" valign="top">
                <asp:TextBox ID="txtSkin" runat="server" CssClass="singleShortest" />
                <asp:Image id="imgSkin" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="Leave blank to allow topic to apply to all skins!" runat="server" />
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal15" runat="server" Text='<%$ Tokens:StringResource, admin.topic.displayorder %>' /></font>
            </td>
            <td align="left" valign="top">
                <asp:TextBox ID="txtDspOrdr" runat="server" CssClass="singleShortest"></asp:TextBox>
            </td>
        </tr>     
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal2" runat="server" Text='<%$ Tokens:StringResource, admin.topic.publishedlabel %>' /></font>
            </td>
            <td align="left" valign="top">
                <asp:CheckBox ID="chkPublished" runat="server" />
            </td>
        </tr>                                             
        <tr>
            <td align="right" valign="top">
                <font class="subTitleSmall"><asp:Literal ID="Literal16" runat="server" Text='<%$ Tokens:StringResource, admin.topic.description %>' /></font>
            </td>
            <td align="left" valign="top">
                <asp:Literal ID="ltDescription" runat="server"></asp:Literal>
                <tcontrol:RadEditor runat="server" id="radDescription">
                                    <ImageManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                    <DocumentManager MaxUploadFileSize="11000000" UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                    <FlashManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                    <MediaManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                    <SilverlightManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                                    <TemplateManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                </tcontrol:RadEditor>
            </td>
        </tr>
        <tr>
            <td align="right" valign="top">
                <font class="subTitleSmall"><asp:Literal ID="Literal17" runat="server" Text='<%$ Tokens:StringResource, admin.topic.setitle %>' /></font>
            </td>
            <td align="left" valign="top">
                <asp:TextBox runat="server" ID="ltSETitle"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td align="right" valign="top">
                <font class="subTitleSmall"><asp:Literal ID="Literal18" runat="server" Text='<%$ Tokens:StringResource, admin.topic.sekeywords %>' /></font>
            </td>
            <td align="left" valign="top">
                <asp:TextBox runat="server" ID="ltSEKeywords"></asp:TextBox>                                                    
            </td>
        </tr>
        <tr>
            <td align="right" valign="top">
                <font class="subTitleSmall"><asp:Literal ID="Literal19" runat="server" Text='<%$ Tokens:StringResource, admin.topic.sedescription %>' /></font>
            </td>
            <td align="left" valign="top">
                <asp:TextBox runat="server" ID="ltSEDescription"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal20" runat="server" Text='<%$ Tokens:StringResource, admin.topic.password %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:TextBox ID="txtPassword" runat="server"></asp:TextBox>
                <asp:Image id="imgPassword" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="Only required if you want to protect this topic content by requiring a password to be entered." runat="server" />
            </td>
        </tr>
        <tr id="trRequireSubscription" runat="server">
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal21" runat="server" Text='<%$ Tokens:StringResource, admin.topic.subscription %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:RadioButtonList ID="rbSubscription" runat="server" RepeatDirection="horizontal">
                    <asp:ListItem Value="0" Selected="true" Text='<%$ Tokens:StringResource, admin.common.No %>' ></asp:ListItem>
                    <asp:ListItem Value="1" Text='<%$ Tokens:StringResource, admin.common.Yes %>' ></asp:ListItem>                             
                </asp:RadioButtonList>
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal22" runat="server" Text='<%$ Tokens:StringResource, admin.topic.HTML %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:RadioButtonList ID="rbHTML" runat="server" RepeatDirection="horizontal">
                    <asp:ListItem Value="0" Selected="true" Text='<%$ Tokens:StringResource, admin.common.No %>' ></asp:ListItem>
                    <asp:ListItem Value="1" Text='<%$ Tokens:StringResource, admin.common.Yes %>' ></asp:ListItem>                                                            
                </asp:RadioButtonList>
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal23" runat="server" Text='<%$ Tokens:StringResource, admin.topic.disclaimer %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:RadioButtonList ID="rbDisclaimer" runat="server" RepeatDirection="horizontal">
                    <asp:ListItem Value="0" Selected="true" Text='<%$ Tokens:StringResource, admin.common.No %>' ></asp:ListItem>
                    <asp:ListItem Value="1" Text='<%$ Tokens:StringResource, admin.common.Yes %>' ></asp:ListItem>                                                            
                </asp:RadioButtonList>
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal24" runat="server" Text='<%$ Tokens:StringResource, admin.topic.sitemap %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:RadioButtonList ID="rbPublish" runat="server" RepeatDirection="horizontal">
                    <asp:ListItem Value="0" Selected="true" Text='<%$ Tokens:StringResource, admin.common.No %>' ></asp:ListItem>
                    <asp:ListItem Value="1" Text='<%$ Tokens:StringResource, admin.common.Yes %>' ></asp:ListItem>                                                            
                </asp:RadioButtonList>
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal25" runat="server" Text='<%$ Tokens:StringResource, admin.topic.pagebgcolor %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:TextBox ID="txtPageBG" runat="server" CssClass="singleShorter"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal26" runat="server" Text='<%$ Tokens:StringResource, admin.topic.contentsbgcolor %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:TextBox ID="txtContentsBG" runat="server" CssClass="singleShorter"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <font class="subTitleSmall"><asp:Literal ID="Literal27" runat="server" Text='<%$ Tokens:StringResource, admin.topic.skingraphicscolor %>' /></font>
            </td>
            <td align="left" valign="middle">
                <asp:TextBox ID="txtSkinColor" runat="server" CssClass="singleShorter"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td valign="middle" align="right">
                <asp:Panel ID="litStoreMapperHdr" runat="server">
                    <font class="subTitleSmall">
                        <asp:Literal ID="Literal28" runat="server" Text='<%$ Tokens:StringResource, admin.topic.mapstores %>' />
                    </font>
                </asp:Panel>
            </td>
            <td>
            <asp:Panel ID="litStoreMapper" runat="server" >
                <aspdnsf:TopicToStore runat="server" ShowText="false" SelectMode="SingleDropDown" ShowDefaultForAllStores="true"  ID="ssOne" />
            </asp:Panel>                                                  
            </td>
        </tr>
        <tr>
            <td align="right" valign="middle">
                <asp:Literal ID="Literal31" runat="server" Text="<%$ Tokens:StringResource, admin.topic.isfrequent %>" /></td>
            <td>
                <asp:CheckBox ID="chkIsFrequent" Checked="true" runat="server" /></td>
        </tr>
    </table>
    <div style="width: 100%; text-align: center;">
        &nbsp;&nbsp;<asp:Button ID="btnSubmit" runat="server" CssClass="normalButtons" OnClick="btnSubmit_Click" />
        &nbsp;&nbsp;<asp:Button ID="btnDelete" runat="server" CssClass="normalButtons" OnClick="btnDelete_Click" Text='<%$ Tokens:StringResource, admin.topic.deletebutton %>' />
        &nbsp;&nbsp;<asp:Button ID="btnNuke" runat="server" CssClass="normalButtons" OnClick="btnNuke_Click" Text='Nuke Topic' />
    </div>
</asp:Panel>