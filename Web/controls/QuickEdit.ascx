<%@ Control Language="C#" AutoEventWireup="true" CodeFile="QuickEdit.ascx.cs" Inherits="AspDotNetStorefront.QuickEdit" %>
<%@ Register Src="~/controls/QuickTopic.ascx" TagName="Topic" TagPrefix="QuickEdit" %>
<%@ Register Src="~/controls/MapLayout.ascx" TagName="MapLayout" TagPrefix="QuickEdit" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<%-- Map Layouts --%>
<ajax:ModalPopupExtender ID="extMapLayout" runat="server" 
    PopupControlID="pnlMapLayout" 
    TargetControlID="lbtnMapLayout"
    CancelControlID="btnCancelMapLayout" 
    BackgroundCssClass="modal_popup_background"
    PopupDragHandleControlID="modaldivmaplayout" >
</ajax:ModalPopupExtender>

<%-- Quick Topic Add --%>
<ajax:ModalPopupExtender ID="extQuickTopic" runat="server" 
    PopupControlID="pnlQuickTopic" 
    TargetControlID="lbtnQuickTopic"
    CancelControlID="btnCancelQuickTopic" 
    BackgroundCssClass="modal_popup_background"
    PopupDragHandleControlID="modaldiv" >
</ajax:ModalPopupExtender>

<div id="quickeditnav">
    <asp:LinkButton ID="lbtnMapLayout" Text="Map Layout" runat="server" Font-Underline="false"  />
    <img id="Img1" src="~/images/tab-sep.gif" runat="server" alt="" height="30" width="2" style="margin-left:10px;margin-bottom:2px;vertical-align:middle;"  />
    <asp:LinkButton ID="lbtnQuickTopic" Text="Quick Topic" runat="server" Font-Underline="false"  />
    <%  if (IsTopicPage) { %>
        <%-- Only render for topic pages --%>
        <ajax:ModalPopupExtender ID="extQuickEditTopic" runat="server"
            PopupControlID="pnlQuickEditTopic"
            TargetControlID="lbtnEditTopic"
            CancelControlID="btnCancelQuickEditTopic"
            BackgroundCssClass="modal_popup_background"
            PopupDragHandleControlID="modaldivedit" >
        </ajax:ModalPopupExtender>
        
        <asp:Panel ID="pnlQuickEditTopic" runat="server" DefaultButton="btnSubmitQuickEditTopic" CssClass="modal_popup" >
            <div class="modal_popup_Header" style="background:url(images/variantListHeader_background.jpg) repeat;" id="modaldivedit" runat="server">Edit Topic</div>
                <QuickEdit:Topic ID="quickEditTopic" AddMode="false" runat="server" />
                <div align="center" class="modal_popup_Footer" >
                    <asp:Button ID="btnSubmitQuickEditTopic" runat="server" CssClass="normalButtons" OnClick="btnSubmitQuickEditTopic_Click" Text="Update Topic" />&nbsp;&nbsp;
                    <asp:Button ID="btnCancelQuickEditTopic" runat="server" CssClass="normalButtons" OnClick="btnCancelQuickEditTopic_Click" Text="Cancel" />
                </div>
        </asp:Panel>
        
        <img id="Img2" src="~/images/tab-sep.gif" runat="server" alt="" height="30" width="2" style="margin-left:10px;vertical-align:middle;" />
        <asp:LinkButton ID="lbtnEditTopic" Text="Edit This Topic" runat="server" Font-Underline="false"   />
    <% } %>
    <%--<asp:Menu ID="quickEditMenu" runat="server" Orientation="Horizontal" StaticTopSeparatorImageUrl="~/images/tab-sep.gif"
        StaticEnableDefaultPopOutImage="False" StaticDisplayLevels="2" MaximumDynamicDisplayLevels="<%$ Tokens:AppConfigUSInt, MaxMenuLevel %>" >
        <StaticSelectedStyle CssClass="aspnetMenu_StaticSelectedStyle" />
        <LevelMenuItemStyles>
            <asp:MenuItemStyle CssClass="aspnetMenu_Level1" Font-Underline="False" />
            <asp:MenuItemStyle CssClass="aspnetMenu_Level2" Font-Underline="False" />
        </LevelMenuItemStyles>
        <StaticMenuItemStyle CssClass="aspnetMenu_StaticMenuItemStyle" />
        <DynamicSelectedStyle CssClass="aspnetMenu_DynamicSelectedStyle" />
        <DynamicMenuItemStyle CssClass="aspnetMenu_DynamicMenuItemStyle" />
        <DynamicMenuStyle CssClass="aspnetMenu_DynamicMenuStyle" />
        <Items>
        <asp:MenuItem Text="Map Layout"  ></asp:MenuItem>
        <asp:MenuItem Selectable="false"></asp:MenuItem>
        <asp:MenuItem  Text="Quick Topic"></asp:MenuItem>
        <asp:MenuItem Text="Quick Image"></asp:MenuItem>
        </Items>
        <%--
                These 2 styles below can't be added declaratively, it will be added later on the script
                But needs to follow the naming convention {MenuId}_WhatEverStyle
            
        <%--<DynamicHoverStyle CssClass="aspnetMenu_DynamicHoverStyle" />
        <%--<StaticHoverStyle  CssClass="aspnetMenu_StaticHoverStyle" />
    </asp:Menu>--%>
    </div>

<asp:Panel ID="pnlMapLayout" runat="server" DefaultButton="btnSubmitMapLayout" CssClass="modal_popup" >
<div class="modal_popup_Header" style="background:url(images/variantListHeader_background.jpg) repeat;" id="modaldivmaplayout" runat="server">Map Layout</div>
    <QuickEdit:MapLayout ID="quickMapLayout" AddMode="true" runat="server" />
    <div align="center" class="modal_popup_Footer" >
        <asp:Button ID="btnSubmitMapLayout" runat="server" CssClass="normalButtons" OnClick="btnSubmitMapLayout_Click" Text="Map This Page" />&nbsp;&nbsp;
        <asp:Button ID="btnCancelMapLayout" runat="server" CssClass="normalButtons" OnClick="btnCancelMapLayout_Click" Text="Cancel" />
    </div>
</asp:Panel>

<%--<asp:UpdatePanel ID="upQuickTopic" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
<ContentTemplate>--%>
<asp:Panel ID="pnlQuickTopic" runat="server" DefaultButton="btnSubmitQuickTopic" CssClass="modal_popup" >
<div class="modal_popup_Header" style="background:url(images/variantListHeader_background.jpg) repeat;" id="modaldiv" runat="server">Add Topic</div>
    <QuickEdit:Topic ID="quickTopic" AddMode="true" runat="server" />
    <div align="center" class="modal_popup_Footer" >
        <asp:Button ID="btnSubmitQuickTopic" runat="server" CssClass="normalButtons" OnClick="btnSubmitQuickTopic_Click" Text="Add Topic" />&nbsp;&nbsp;
        <asp:Button ID="btnCancelQuickTopic" runat="server" CssClass="normalButtons" OnClick="btnCancelQuickTopic_Click" Text="Cancel" />
    </div>
</asp:Panel>
<%--</ContentTemplate>
</asp:UpdatePanel>--%>