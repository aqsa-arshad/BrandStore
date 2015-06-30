<%@ Page Language="c#" Inherits="AspDotNetStorefront.editaddressrecurring" CodeFile="editaddressrecurring.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">

        <asp:Literal Visible="false" ID="ltOriginalRecurringOrderNumber" runat="server" />
        <asp:Panel ID="pnlErrorMsg" runat="Server" HorizontalAlign="Left" Style="margin-left: 20px;">
            <asp:Label ID="ErrorMsgLabel" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="pnlAddress" runat="server" Visible="true">
            <asp:Table ID="tblAddressList" CellSpacing="0" CellPadding="1" Width="100%" runat="server">
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="left" VerticalAlign="top">
                        <asp:Table ID="tblAddressListBox" CellSpacing="0" CellPadding="1" Width="100%" runat="server">
                            <asp:TableRow>
                                <asp:TableCell HorizontalAlign="left" VerticalAlign="top">

                                    <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="litAddressNickName" runat="server" Text="<%$ Tokens:StringResource,address.cs.49 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtAddressNickName" Columns="20" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx33" runat="server" Text="<%$ Tokens:StringResource,address.cs.2 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtFirstName" Columns="20" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                                <asp:RequiredFieldValidator ID="valReqFName" ControlToValidate="txtFirstName" Display="none" EnableClientScript="false" runat="server" ErrorMessage="<%$ Tokens:StringResource,address.cs.13 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx35" runat="server" Text="<%$ Tokens:StringResource,address.cs.3 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtLastName" Columns="20" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                                <asp:RequiredFieldValidator ID="valReqLName" ControlToValidate="txtLastName" Display="none" EnableClientScript="false" runat="server" ErrorMessage="<%$ Tokens:StringResource,address.cs.14 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx37" runat="server" Text="<%$ Tokens:StringResource,address.cs.4 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtPhone" Columns="20" MaxLength="25" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                                <asp:RequiredFieldValidator ID="valReqPhone" ControlToValidate="txtPhone" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" ErrorMessage="<%$ Tokens:StringResource,address.cs.15 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx40" runat="server" Text="<%$ Tokens:StringResource,address.cs.5 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtCompany" Columns="34" MaxLength="100" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="addresscs58" runat="server" Text="<%$ Tokens:StringResource,address.cs.58 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:DropDownList ID="ddlResidenceType" runat="server"></asp:DropDownList></td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx41" runat="server" Text="<%$ Tokens:StringResource,address.cs.6 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtAddress1" Columns="34" MaxLength="100" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                                <asp:RequiredFieldValidator ID="valReqAddr1" ControlToValidate="txtAddress1" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" ErrorMessage="<%$ Tokens:StringResource,address.cs.16 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx43" runat="server" Text="<%$ Tokens:StringResource,address.cs.7 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtAddress2" Columns="34" MaxLength="100" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx44" runat="server" Text="<%$ Tokens:StringResource,address.cs.8 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtSuite" Columns="34" MaxLength="50" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx45" runat="server" Text="<%$ Tokens:StringResource,address.cs.9 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtCity" Columns="34" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                                <asp:RequiredFieldValidator ID="valReqCity" ControlToValidate="txtCity" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" ErrorMessage="<%$ Tokens:StringResource,address.cs.17 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx52" runat="server" Text="<%$ Tokens:StringResource,address.cs.53 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:DropDownList ID="ddlCountry" runat="server" OnSelectedIndexChanged="ddlCountry_OnChange" AutoPostBack="True"></asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx47" runat="server" Text="<%$ Tokens:StringResource,address.cs.10 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:DropDownList ID="ddlState" runat="server"></asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%">
                                                <asp:Literal ID="createaccountaspx49" runat="server" Text="<%$ Tokens:StringResource,address.cs.12 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtZip" Columns="14" MaxLength="10" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                                <asp:RequiredFieldValidator ID="valReqZip" ControlToValidate="txtZip" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" ErrorMessage="<%$ Tokens:StringResource,address.cs.18 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                    </table>
                                    <asp:Literal ID="litCCForm" runat="server"></asp:Literal>
                                </asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="center">
                        <asp:Button ID="btnSaveAddress" runat="server" CssClass="EditAddressButton" OnClick="btnSaveAddress_Click" />
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </asp:Panel>

    </asp:Panel>
</asp:Content>

