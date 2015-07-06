<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LayoutItem.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.LayoutItem" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="editlayout" src="editlayout.ascx" %>

<div id="pnlLoadingGeneral" runat="server" style="float:left;margin-left:400px;position:absolute;"> 
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
            <ProgressTemplate>                    
                <img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-1.gif" />                 
            </ProgressTemplate>
        </asp:UpdateProgress>
</div>
      
<asp:Panel ID="pnlEditLayout" runat="server" DefaultButton="btnSaveEditedLayout" CssClass="modal_popup" >    
    <div class="modal_popup_Header" id="modaldiv" runat="server">Edit Layout</div>
    <aspdnsf:editlayout id="ctrlEditLayout" runat="server" 
        CssClass="modal_popup_Content"         
        AddMode="false"/>
    <div align="center" class="modal_popup_Footer" >
        <asp:Button ID="btnSaveEditedLayout" runat="server" 
            UseSubmitBehavior="false"  
            Text="Save" onclick="btnSaveEditedLayout_Click" />
        <asp:Button ID="btnCancelEditLayout" runat="server" Text="Cancel" />
        
        <div id="divProgressAdd" runat="server" style="display:none" >
            <p style="color:#3DDB0B;font-style:italic;">Saving in progress...<img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-1.gif" />  </p> 
        </div>
    </div>
    
    <%--
        We'll use animation extender as update progress notification instead
        since it reacts faster than the update progress and we can control
        which particular sections we want to display the progress template 
        based on which button clicked
    --%>    
    <ajax:AnimationExtender ID="extSaveLayoutAdd" runat="server" TargetControlID="btnSaveEditedLayout" Enabled="true" >
         <Animations >                                                     
             <OnClick>
                <Sequence>
                    <EnableAction AnimationTarget="btnSaveEditedLayout" Enabled="false" />
                    <EnableAction AnimationTarget="btnCancelEditLayout" Enabled="false" />
                    <%--Hide the main progress template since we have our own specialized--%>
                    <HideAction AnimationTarget="pnlLoadingGeneral" />
                    <StyleAction AnimationTarget="divProgressAdd" Attribute="display" Value=""/>
                </Sequence>
            </OnClick>
         </Animations>
    </ajax:AnimationExtender>
</asp:Panel>

<ajax:ModalPopupExtender ID="extLayoutPanel" runat="server" 
    PopupControlID="pnlEditLayout" 
    TargetControlID="imgbtnEdit" 
    BackgroundCssClass="modal_popup_background" 
    CancelControlID="btnCancelEditLayout"
    PopupDragHandleControlID="modaldiv" >
</ajax:ModalPopupExtender>

<asp:Panel ID="pnlLayoutItem" Width="200" Height="300" runat="server" BorderColor="Gray" BorderStyle="Dotted" BorderWidth="1">
    <div id="Div1" style="text-align:center;" runat="server">
    <br />
    <asp:Image ID="imgLayoutIcon" Width="160" Height="160" runat="server" />
    <br />
    <br />
    <asp:Literal ID="Literal1" runat="server" Text="Layout ID:" />&nbsp;<asp:Literal ID="litLayoutID" runat="server" Text="#" />
    <br />
    <asp:Literal ID="litLayoutName" runat="server" Text="Layout Name" />
    <br />
    <asp:Literal ID="Literal3" runat="server" Text="Actively Mapped:" />&nbsp;<asp:Literal ID="litLayoutMapped" runat="server" Text="No" />
    <br />
    <br />
    <br />
    <asp:ImageButton ID="imgbtnEdit" ImageUrl="~/App_Themes/Admin_Default/images/edit.png" runat="server" />
    &nbsp;&nbsp;&nbsp;
    <asp:ImageButton ID="imgbtnClone" ImageUrl="~/App_Themes/Admin_Default/images/clone.png" 
            runat="server" onclick="imgbtnClone_Click" OnClientClick="return confirm('Are you sure you want to clone this layout?');"  />
    &nbsp;&nbsp;&nbsp;
    <asp:ImageButton ID="imgbtnDelete" ImageUrl="~/App_Themes/Admin_Default/images/delete.png" OnClick="imgbtnDelete_Click" OnClientClick="return confirm('Are you sure you want to delete this layout?  This action cannot be undone.');" runat="server" />
    <br />
    </div>
</asp:Panel>

<ajax:HoverMenuExtender ID="hmeLarge" runat="Server"
    TargetControlID="imgLayoutIcon"
    PopupControlID="pnlLargePopup"
    HoverCssClass="popupHover"
    PopupPosition="Top"
    OffsetX="80"
    OffsetY="10"
    PopDelay="0" />
    
<asp:Panel BorderStyle="None" ID="pnlLargePopup" runat="server">
    <asp:Image ID="imgLarge" runat="server" />
</asp:Panel>