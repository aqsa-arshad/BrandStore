<%@ Page language="c#" AutoEventWireup="true" CodeFile="BadWord.aspx.cs" Inherits="AspDotNetStorefrontAdmin.BadWord" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %> 
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <asp:Literal ID="ltError" runat="server"></asp:Literal>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.BadWord.EnterBadWordBelow %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapperTop">
                                        <asp:TextBox runat="Server" ID="txtWord" CssClass="default" Height="41px" Width="400px"></asp:TextBox>
                                        <br />
                                        <asp:Button runat="server" ID="btnSubmit" Text="<%$Tokens:StringResource, admin.common.Submit %>" CssClass="normalButtons" OnClick="btnSubmit_Click" />
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <asp:CheckBox ID="chkBoxNewComments" runat="server" Text="<%$Tokens:StringResource, admin.BadWord.OnlyForNewComments %>" Checked="True" /></td>
            </tr>
        </table>
    </div>
</asp:Content>
