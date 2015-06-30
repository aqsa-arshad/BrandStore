<%@ Page Language="C#" AutoEventWireup="true" CodeFile="entityBody.aspx.cs" Inherits="entityBody" MasterPageFile="~/App_Templates/Admin_Default/AdminEntityEdit.master" Theme="Admin_Default" Title="<%$Tokens:StringResource, admin.title.entityBody %>"%>
<%@ OutputCache  Duration="1"  Location="none" %>

<asp:Content ContentPlaceHolderID="entityMain" runat="server">
    <div style="padding-top: 100px; text-align: center; width: 100%;">
        <div id="help" style="text-align: center; width: 100%;">
            <table border="0" cellpadding="1" cellspacing="0" class="outerTable">
                <tr>
                    <td align="center">
                        <div class="wrapper">
                            <table border="0" cellpadding="0" cellspacing="0" class="innerTable">
                                <tr>                            
                                    <td class="titleTable">
                                        <font class="subTitle">
                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.entities.StartSelect %>" />
                                        </font>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <asp:Literal ID="ltScript" runat="server"></asp:Literal>
</asp:Content>