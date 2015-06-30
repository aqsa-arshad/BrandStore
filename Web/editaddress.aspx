<%@ Page language="c#" Inherits="AspDotNetStorefront.editaddress" CodeFile="editaddress.aspx.cs" %>
<html>
<head>
</head>
<body>

    <form id="Form1" runat="server">
        <asp:Panel ID="pnlErrorMsg" runat="Server" HorizontalAlign="Left" style="margin-left:20px;">
            <asp:Label ID="ErrorMsgLabel" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>
        </asp:Panel>

        <asp:Panel ID="pnlAddress" runat="server" Visible="true">
            <asp:Table ID="tblAddressList" CellSpacing="0" CellPadding="2" Width="100%" runat="server">
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="left" VerticalAlign="top">
                        <asp:Image runat="server" ID="editaddress_gif" />
                        <asp:Table ID="tblAddressListBox" CellSpacing="0" CellPadding="2" Width="100%" runat="server">
                            <asp:TableRow>
                                <asp:TableCell HorizontalAlign="left" VerticalAlign="top">
                                    <asp:Literal ID="litAddressPrompt" runat="Server" Mode="PassThrough"></asp:Literal>

                                    <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                        <tr>
                                            <td width="100%" colspan="2">
                                                <hr />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="litAddressNickName" runat="server" Text="<%$ Tokens:StringResource,address.cs.49 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtAddressNickName" Columns="20" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx33" runat="server" Text="<%$ Tokens:StringResource,address.cs.2 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtFirstName" Columns="20" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                                <asp:RequiredFieldValidator ID="valReqFName" ControlToValidate="txtFirstName" ValidationGroup="createaccount" Display="none" EnableClientScript="false" runat="server" ErrorMessage="<%$ Tokens:StringResource,address.cs.13 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx35" runat="server" Text="<%$ Tokens:StringResource,address.cs.3 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtLastName" Columns="20" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="createaccount" /> 
                                                <asp:RequiredFieldValidator ID="valReqLName" ControlToValidate="txtLastName" ValidationGroup="createaccount" Display="none" EnableClientScript="false" runat="server" ErrorMessage="<%$ Tokens:StringResource,address.cs.14 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx37" runat="server" Text="<%$ Tokens:StringResource,address.cs.4 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtPhone" Columns="20" MaxLength="25" runat="server" CausesValidation="true" ValidationGroup="createaccount" /> 
                                                <asp:RequiredFieldValidator ID="valReqPhone" ControlToValidate="txtPhone" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" ErrorMessage="<%$ Tokens:StringResource,address.cs.15 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx40" runat="server" Text="<%$ Tokens:StringResource,address.cs.5 %>"></asp:Literal></td>
                                            <td width="65%"><asp:TextBox ID="txtCompany" Columns="34" MaxLength="100" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="addresscs58" runat="server" Text="<%$ Tokens:StringResource,address.cs.58 %>"></asp:Literal></td>
                                            <td width="65%"><asp:DropDownList ID="ddlResidenceType" runat="server" ></asp:DropDownList></td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx41" runat="server" Text="<%$ Tokens:StringResource,address.cs.6 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtAddress1" Columns="34" MaxLength="100" runat="server" CausesValidation="true" ValidationGroup="createaccount" /> 
                                                <asp:RequiredFieldValidator ID="valReqAddr1" ControlToValidate="txtAddress1" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" ErrorMessage="<%$ Tokens:StringResource,address.cs.16 %>"></asp:RequiredFieldValidator>
                                                <asp:CustomValidator ID="valAddressIsPOBox" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" OnServerValidate="ValidateAddress1" ></asp:CustomValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx43" runat="server" Text="<%$ Tokens:StringResource,address.cs.7 %>"></asp:Literal></td>
                                            <td width="65%"><asp:TextBox ID="txtAddress2" Columns="34" MaxLength="100" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx44" runat="server" Text="<%$ Tokens:StringResource,address.cs.8 %>"></asp:Literal></td>
                                            <td width="65%"><asp:TextBox ID="txtSuite" Columns="34" MaxLength="50" runat="server" /></td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx45" runat="server" Text="<%$ Tokens:StringResource,address.cs.9 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtCity" Columns="34" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="createaccount" /> 
                                                <asp:RequiredFieldValidator ID="valReqCity" ControlToValidate="txtCity" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" ErrorMessage="<%$ Tokens:StringResource,address.cs.17 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx52" runat="server" Text="<%$ Tokens:StringResource,address.cs.53 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:DropDownList ID="ddlCountry" runat="server" OnSelectedIndexChanged="ddlCountry_OnChange" AutoPostBack="True"></asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx47" runat="server" Text="<%$ Tokens:StringResource,address.cs.10 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:DropDownList ID="ddlState" runat="server"></asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td width="35%"><asp:Literal ID="createaccountaspx49" runat="server" Text="<%$ Tokens:StringResource,address.cs.12 %>"></asp:Literal></td>
                                            <td width="65%">
                                                <asp:TextBox ID="txtZip" Columns="14" MaxLength="10" runat="server" CausesValidation="true" ValidationGroup="createaccount" />
                                                <asp:RequiredFieldValidator ID="valReqZip" ControlToValidate="txtZip" EnableClientScript="false" runat="server" ValidationGroup="createaccount" Display="None" ErrorMessage="<%$ Tokens:StringResource,address.cs.18 %>"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                    </table>

                                    <asp:Panel ID="pnlBillingData" runat="server">
                                        <hr />
                                        <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                            <tr><td align="left" valign="bottom" style="width:35%;">
                                                    <asp:Literal ID="editaddress_aspx_4" runat="server" Mode="PassThrough"></asp:Literal>
                                                </td>
                                                <td align="left" valign="bottom">
                                                    <asp:Literal ID="editaddress_aspx_5" runat="server" Mode="PassThrough"></asp:Literal><asp:RadioButton ID="CreditCard" runat="server" GroupName="PaymentMethod" onclick="ShowPaymentInput(this)" />
                                                    <asp:Literal ID="editaddress_aspx_6" runat="server" Mode="PassThrough"></asp:Literal><asp:RadioButton ID="ECheck" runat="server" GroupName="PaymentMethod" onclick="ShowPaymentInput(this)" />
                                                </td>
                                            </tr>
                                        </table>
                                        <asp:Panel ID="pnlCCData" runat="server">
                                            <hr/>
                                            <p><asp:Label ID="editaddress_aspx_7" runat="server" Font-Bold="true"></asp:Label></p>
                                            <hr/>
                                            <asp:Literal ID="litCCForm" runat="server"></asp:Literal>
                                        </asp:Panel>
                                        
                                        <asp:Panel ID="pnlEcheckData" runat="server">
                                            <hr/>
                                            <p><asp:Label ID="editaddress_aspx_8" runat="server" Font-Bold="true"></asp:Label></p>
                                            <hr/>
                                            <asp:Literal ID="litECheckForm" runat="server"></asp:Literal>
                                        </asp:Panel>
                                    </asp:Panel>    
                              </asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                        
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="center">
                        <asp:Button ID="btnValidateAddress" runat="server" visible="false" CssClass="EditAddressButton" ValidationGroup="createaccount" OnClick="btnValidateAddress_Click" />
                        <asp:Label ID="lblValidateAddressSpacer" Text="&nbsp;&nbsp;&nbsp;&nbsp;" runat="server"></asp:Label>
                        <asp:Button ID="btnSaveAddress" runat="server" CssClass="EditAddressButton" ValidationGroup="createaccount" OnClick="btnSaveAddress_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:Button ID="btnDeleteAddress" runat="server" CssClass="EditAddressButton" ValidationGroup="createaccount" OnClick="btnDeleteAddress_Click" /></asp:TableCell> 
                </asp:TableRow>
            </asp:Table>
            
            
            <asp:Label ID="lblError" runat="server" Font-Bold="true" ForeColor="red"></asp:Label>

        </asp:Panel>
    </form>
</body>
</html>