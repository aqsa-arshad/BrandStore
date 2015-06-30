<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TextPanel.ascx.cs" Inherits="AspDotNetStorefront.LayoutTextPanel" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" TagName="configuretext" src="ConfigureTextPanel.ascx" %>

<%--
      We enclose the progress bar inside a div panel 
      so that we can control it's visibility
      since by default, the progress template will trigger upon
      every asynchronous postback request.
--%>      
<div id="pnlLoadingGeneral" runat="server" style="float:left;margin-left:400px;position:absolute;"> 
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>                    
            <img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-2.gif" />                  
        </ProgressTemplate>
    </asp:UpdateProgress>
</div>

<asp:Panel ID="pnlConfigureText" runat="server" DefaultButton="btnSaveText" CssClass="modal_popup" >    
    <div class="modal_popup_Header" id="modaldiv" runat="server">Configure Text Panel</div>
    <aspdnsf:configuretext id="ctrlConfigureText" runat="server" 
        CssClass="modal_popup_Content"/>
    <div align="center" class="modal_popup_Footer" >
        <asp:Button ID="btnSaveText" runat="server" 
            UseSubmitBehavior="false"  
            Text="Save" onclick="btnSaveText_Click" />
        <asp:Button ID="btnCancelConfigure" runat="server" Text="Cancel" />
        
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
    <ajax:AnimationExtender ID="extSaveText" runat="server" TargetControlID="btnSaveText" Enabled="true" >
         <Animations >                                                     
             <OnClick>
                <Sequence>
                    <EnableAction AnimationTarget="btnSaveText" Enabled="false" />
                    <EnableAction AnimationTarget="btnCancelConfigure" Enabled="false" />
                    <%--Hide the main progress template since we have our own specialized--%>
                    <HideAction AnimationTarget="pnlLoadingGeneral" />
                    <StyleAction AnimationTarget="divProgressAdd" Attribute="display" Value=""/>
                </Sequence>
            </OnClick>
         </Animations>
    </ajax:AnimationExtender>
</asp:Panel>

<asp:Literal ID="litTextPanel" runat="server" Mode="Transform" />
<asp:LinkButton BorderStyle="Dashed" BorderColor="Gray" BorderWidth="1" ID="btnTextPanel" runat="server" ToolTip="Click here to edit">
<asp:Literal ID="litTextPanelAdmin" runat="server" Mode="Transform" />
</asp:LinkButton><ajax:ModalPopupExtender ID="extConfigureText" runat="server" 
    PopupControlID="pnlConfigureText" 
    TargetControlID="btnTextPanel" 
    BackgroundCssClass="modal_popup_background" 
    CancelControlID="btnCancelConfigure"
    PopupDragHandleControlID="modaldiv">
</ajax:ModalPopupExtender>