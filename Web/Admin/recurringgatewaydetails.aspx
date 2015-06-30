<%@ Page language="c#" Inherits="AspDotNetStorefrontAdmin.recurringgatewaydetails" CodeFile="recurringgatewaydetails.aspx.cs" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="content" style="margin-left: 5px">
        <asp:Panel ID="pnlInput1" runat="server" Width="100%" DefaultButton="btnOrderNumber">
            Original Order Number
                <asp:TextBox ID="txtOrderNumber" runat="server"></asp:TextBox>
                <asp:Button ID="btnOrderNumber" runat="server" Text="Submit" OnClick="btnOrderNumber_Click" /><br />
            <br />
        </asp:Panel>
        <asp:Panel ID="pnlInput2" runat="server" Width="100%" DefaultButton="btnSubscriptionID">
            Subscription ID
                <asp:TextBox ID="txtSubscriptionID" runat="server"></asp:TextBox>
                <asp:Button ID="btnSubscriptionID" runat="server" Text="Submit" OnClick="btnSubscriptionID_Click" /><br />
        </asp:Panel>
        <asp:Panel ID="pnlResults" runat="server" Width="100%">
            <br />
            <asp:Literal ID="ltResults" runat="server"></asp:Literal>
         </asp:Panel>
    </div>
</asp:Content>