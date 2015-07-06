<%@ Page Language="C#" AutoEventWireup="true" CodeFile="appconfig.aspx.cs" Inherits="AspDotNetStorefrontAdmin.AppConfigPage" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="AppConfigList" Src="controls/appconfiglist.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">

    <script type="text/javascript" src="../jscripts/core.js"></script>
    
            <div class="pnlMain">
                <aspdnsf:AppConfigList ID="ctrlAppConfigList" runat="server" />
            </div>
    <br />
    <br />
    <br />
</asp:Content>
