<%@ Page Language="C#" AutoEventWireup="true" CodeFile="runsql.aspx.cs" Inherits="AspDotNetStorefrontAdmin.runsql" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    Enter SQL Command Text Below:
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapperTop">
                                        <asp:TextBox runat="Server" ID="txtQuery" TextMode="multiline" CssClass="default" Height="200px" Width="100%"></asp:TextBox>
                                        <br />
                                        <asp:Button runat="server" ID="btnSubmit" Text="Submit" CssClass="normalButtons" OnClick="btnSubmit_Click" />
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <asp:Literal ID="ltMiscellaneous" runat="server"></asp:Literal>
</asp:Content>