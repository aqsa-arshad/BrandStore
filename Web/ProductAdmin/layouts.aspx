<%@ Page Language="C#" AutoEventWireup="true" CodeFile="layouts.aspx.cs" Inherits="AspDotNetStorefrontAdmin.layouts" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="layoutlist" src="controls/layoutlist.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="editlayout" src="controls/editlayout.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
    <script type="text/javascript" src="../jscripts/core.js"></script>
    <asp:UpdatePanel ID="pnlUpdateLayout" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional"  >
        <Triggers>
            <asp:PostBackTrigger ControlID="ctrlLayoutList$btnSaveNewLayout" />
        </Triggers>
        <ContentTemplate>
            <div class="pnlMain">                
                <aspdnsf:layoutlist id="ctrlLayoutList" runat="server" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <br />
    <br />
    <br />
</asp:Content>
