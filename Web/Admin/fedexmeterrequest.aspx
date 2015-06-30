<%@ Page Language="C#" AutoEventWireup="true" CodeFile="fedexmeterrequest.aspx.cs" Inherits="AspDotNetStorefrontAdmin.FedExMeterRequest"
    MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <table class="toppage" width="100%" cellspacing="0" cellpadding="0" border="0">
        <tbody>
            <tr>
                <td valign="middle" align="left" style="height: 36px;">
                    <h3>
                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Form %>" /></h3>
                    <table border="0" cellspacing="1" id="table1">
                        <tr>
                            <td colspan="2">
                                <h4>
                                    <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.AccountInfo %>" />:</h4>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.AccountInfo %>" />:</td>
                            <td>
                                <asp:TextBox ID="AccountNumber" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.FedexServer %>" />:</td>
                            <td>
                                <asp:TextBox ID="FedExServer" runat="server" Width="272px">https://gateway.fedex.com/GatewayDC</asp:TextBox></td>
                        </tr>
                        <tr>
                            <td colspan="2">&nbsp;
						<asp:Label ID="lblMeter" runat="server" Font-Bold="True" ForeColor="Blue" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.MeterDisplay %>"></asp:Label></td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <h4><b>
                                    <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.ContactInfo %>" /></b>:</h4>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.FullName %>" />:</td>
                            <td>
                                <asp:TextBox ID="FullName" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.CompanyName %>" />:</td>
                            <td>
                                <asp:TextBox ID="CompanyName" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Department %>" />:</td>
                            <td>
                                <asp:TextBox ID="Department" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Phone %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="PhoneNumber" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Pager %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="PagerNumber" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Fax %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="FaxNumber" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Email %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="EMail" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Address %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="Address" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.City %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="City" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.State %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="State" runat="server"></asp:TextBox>&nbsp;<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.2Chars %>" /></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Zip %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="Zip" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Country %>" />:
                            </td>
                            <td>
                                <asp:TextBox ID="Country" runat="server"></asp:TextBox>&nbsp;<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.2Chars %>" /></td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <p align="center">
                                    <asp:Button ID="btnSubmitRequest" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.Submit %>" OnClick="btnSubmitRequest_Click"></asp:Button>
                                </p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>
    <p>
        &nbsp;<asp:Literal ID="responseX" runat="server" Text="<%$Tokens:StringResource, admin.fedexmeterrequest.ResponseDisplay %>"></asp:Literal>
    </p>
    <p>&nbsp;</p>
</asp:Content>
