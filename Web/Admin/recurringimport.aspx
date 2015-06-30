<%@ Page Language="C#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.recurringimport" CodeFile="recurringimport.aspx.cs" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"%>

<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <div id="">
        <div class="errorMsg" style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
    <asp:Panel ID="pnlMain" runat="server" Width="100%" DefaultButton="btnProcessFile">
        <asp:Label ID="lblLastRun" runat="server"></asp:Label><asp:Button ID="btnGetGatewayStatus" runat="server" Text="<%$Tokens:StringResource, admin.recurringimport.GetTodaysStatusFile %>" OnClick="btnGetGatewayStatus_Click" />
        <asp:Button ID="btnProcessFile" CssClass="normalButtons" runat="server" Text="<%$Tokens:StringResource, admin.recurringimport.ProcessRecords %>" OnClick="btnProcessFile_Click" Visible="True" />
        <br />
        <br />
        <asp:Label ID="PastePromptLabel" runat="server" Text="<%$Tokens:StringResource, admin.recurringimport.PasteGatewayAutobill %>"></asp:Label><br />
        <asp:TextBox ID="txtInputFile" runat="server" Height="600px" TextMode="MultiLine"
            Width="45%" Font-Size="Small"></asp:TextBox>&nbsp;
        <asp:TextBox ID="txtResults" runat="server" Height="600px" TextMode="MultiLine"
            Visible="False" Width="50%" Font-Size="Small" /><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.recurringimport.ProcessingResultsWillGoHere %>" />
     </asp:Panel>
    <asp:Panel ID="pnlNotSupported" runat="server" Width="100%">
            <asp:Label ID="lblNotSupported" runat="server" Text="<%$Tokens:StringResource, admin.recurringimport.NotSupported %>" /><br />
    </asp:Panel>
    </div>
</asp:Content>
