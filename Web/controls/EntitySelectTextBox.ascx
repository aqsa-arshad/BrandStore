<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntitySelectTextBox.ascx.cs" Inherits="AspDotNetStorefront.EntitySelectTextBoxControl" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>


<asp:TextBox ID="txtEntityID" runat="server" Columns="10" ></asp:TextBox>
            
<telerik:RadWindow ID="rwEntityList" runat="server"
        Skin="Default2006" 
        VisibleOnPageLoad="false" 
        ShowContentDuringLoad="false" 
        VisibleStatusbar="false"      
        Behaviors="Maximize, Move, Close, Resize"
        Width="530px"
        Height="380px">
    </telerik:RadWindow>



