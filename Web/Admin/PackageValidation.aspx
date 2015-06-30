<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PackageValidation.aspx.cs" Inherits="AspDotNetStorefrontAdmin.PackageValidation" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
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
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.PackageValidation.Validation %>" />:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td align="left" valign="middle" width="165">
                                                    <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.PackageValidation.SelectFile %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:FileUpload CssClass="fileUpload" ID="xmlpackage" runat="server" Width="400px" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="left" colspan="2">
                                                    <div style="margin-top: 15px; margin-bottom: 15px;">
                                                        <b>-<asp:Literal ID="Literal1" runat="server" Text=" <%$Tokens:StringResource, admin.common.Or %> " />-</b>
                                                    </div>
                                                    <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.PackageValidation.Paste %>" /></font><br/>
                                                    <asp:TextBox ID="xmltext" runat="server" TextMode="MultiLine" Columns="80" Rows="20" Wrap="false" CssClass="default"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <asp:button runat="server" id="validatepackage" CssClass="normalButtons" text="<%$Tokens:StringResource, admin.PackageValidation.Validate %>" OnClick="validatepackage_Click" />
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>