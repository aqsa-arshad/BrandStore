<%@ Control Language="C#" AutoEventWireup="true" CodeFile="layoutlist.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.LayoutList" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="editlayout" src="editlayout.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="layoutitem" Src="LayoutItem.ascx" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="AspDotNetStorefrontLayout" %>
<%@ Import Namespace="System.Linq" %>   


     
<div id="pnlLoadingGeneral" runat="server" style="float:left;margin-left:400px;position:absolute;"> 
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
            <ProgressTemplate>                    
                <img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-2.gif" />                    
            </ProgressTemplate>
        </asp:UpdateProgress>
</div>

<asp:Button ID="btnAddLayout" runat="server" Text="Add Layout" />
          
<asp:Panel ID="pnlAddLayout" runat="server" DefaultButton="btnSaveNewLayout" CssClass="modal_popup" >    
    <div class="modal_popup_Header" id="modaldiv" runat="server">Add Layout</div>
    <aspdnsf:editlayout id="ctrlAddLayout" runat="server" 
        CssClass="modal_popup_Content"         
        AddMode="true"/>
    <div align="center" class="modal_popup_Footer" >
        <asp:Button ID="btnSaveNewLayout" runat="server" 
            UseSubmitBehavior="false"  
            Text="Save" onclick="btnSaveNewLayout_Click" />
        <asp:Button ID="btnCancelAddLayout" runat="server" Text="Cancel" />
        
        <div id="divProgressAdd" runat="server" style="display:none" >
            <p style="color:#3DDB0B;font-style:italic;">Saving in progress...<img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-1.gif" /></p> 
        </div>
    </div>
    
    <%--
        We'll use animation extender as update progress notification instead
        since it reacts faster than the update progress and we can control
        which particular sections we want to display the progress template 
        based on which button clicked
    --%>    
    <ajax:AnimationExtender ID="extSaveLayoutAdd" runat="server" TargetControlID="btnSaveNewLayout" Enabled="true" >
         <Animations >                                                     
             <OnClick>
                <Sequence>
                    <EnableAction AnimationTarget="btnSaveNewLayout" Enabled="false" />
                    <EnableAction AnimationTarget="btnCancelAddLayout" Enabled="false" />
                    <%--Hide the main progress template since we have our own specialized--%>
                    <HideAction AnimationTarget="pnlLoadingGeneral" />
                    <StyleAction AnimationTarget="divProgressAdd" Attribute="display" Value=""/>
                </Sequence>
            </OnClick>
         </Animations>
    </ajax:AnimationExtender>
</asp:Panel>

<ajax:ModalPopupExtender ID="extLayoutPanel" runat="server" 
    PopupControlID="pnlAddLayout" 
    TargetControlID="btnAddLayout" 
    BackgroundCssClass="modal_popup_background" 
    CancelControlID="btnCancelAddLayout"
    PopupDragHandleControlID="modaldiv" >
</ajax:ModalPopupExtender>

<asp:UpdatePanel runat="server" ID="updatePanelSearch" UpdateMode="Conditional">
<Triggers>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$tblListItems" />
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_All"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_#"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_A"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_B"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_C"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_D"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_E"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_F"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_G"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_H"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_I"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_J"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_K"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_L"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_M"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_N"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_O"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_P"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Q"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_R"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_S"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_T"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_U"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_V"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_W"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_X"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Y"/>
<asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Z"/>

</Triggers>
<ContentTemplate>

<aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch"  OnFilter="ctrlSearch_Filter"
                OnContentCreated="ctrlSearch_ContentCreated"  >
                            
                    <Search runat="server" SearchButtonCaption="Go" 
                        SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>" 
                        SearchTextMinLength="3" 
                        SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>" 
                                    ValidateInputLength="false" 
                                    ShowValidationMessageBox="false" 
                                    ShowValidationSummary="false" 
                                    UseLandingPageBehavior="false" 
                                    WillRenderInUpdatePanel="true"  />
                                    
                    <ContentTemplate>
                    
                    <asp:Panel ID="pnlListItems" runat="server">
                    <%--<asp:UpdatePanel ID="upListItems" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>--%>
                            <asp:Table ID="tblListItems" cellpadding="10" runat="server" />
                        <%--</ContentTemplate>
                    </asp:UpdatePanel>--%>
                    </asp:Panel>
                  </ContentTemplate>
</aspdnsf:SearcheableTemplate>

</ContentTemplate>
</asp:UpdatePanel>






