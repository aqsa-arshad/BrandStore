<%@ Page language="c#" Inherits="AspDotNetStorefront.mobilecheckoutshipping" CodeFile="mobilecheckoutshipping.aspx.cs" MasterPageFile="~/App_Templates/skin_1/template.master" %>

<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsfc" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    <asp:Literal ID="CheckoutValidationScript" runat="server" Mode="PassThrough"></asp:Literal>
    <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
    <asp:Literal runat="server" id="CheckoutHeader"></asp:Literal>
    
    <ul data-role="listview">
        <li class="group" data-role="list-divider">
            <asp:Panel ID="pnlErrorMsg" runat="server" Visible="false">
                <asp:Label ID="ErrorMsgLabel" CssClass="errorLg" runat="server"></asp:Label>
            </asp:Panel>
        </li>
    </ul>
    
        <asp:Panel runat="server" ID="pnlSelectShipping">
            <asp:Panel runat="server" ID="pnlSelectAddress">
                <ul data-role="listview">
                    <li>
                        <asp:Label ID="lblChooseShippingAddr" Text="<%$ Tokens:StringResource,mobile.checkoutselectaddress %>" Font-Bold="true"  runat="server"></asp:Label>
                        <asp:DropDownList ID="ddlChooseShippingAddr" runat="server" OnSelectedIndexChanged="ddlChooseShippingAddr_SelectedIndexChanged"></asp:DropDownList>
                    </li>
                </ul>
            </asp:Panel>
            <ul data-role="listview">
                <li>
                    <div class="checkoutWrap">
                        <asp:Panel ID="pnlGetFreeShippingMsg" CssClass="FreeShippingThresholdPrompt" Visible="false" runat="server">
                            <asp:Literal ID="GetFreeShippingMsg" runat="server" Mode="PassThrough"></asp:Literal>
                        </asp:Panel>
                        <asp:Panel ID="pnlFreeShippingMsg" CssClass="FreeShippingThresholdPrompt" runat="server">
                            <asp:Label ID="FreeShippingMsg" Visible="false" runat="server"></asp:Label>
                        </asp:Panel>
                        <asp:Panel ID="pnlCartAllowsShippingMethodSelection" runat="server">
                            <asp:Label ID="ShipSelectionMsg" runat="server"></asp:Label>
                            <div class="shippingOptionWrap">
                                <aspdnsfc:ShippingMethodControl ID="ctrlShippingMethods" runat="server"/>
                            </div>
                        </asp:Panel>
                    </div>
                </li>
                <li>
                    <asp:Button ID="btnContinueCheckout" runat="server" Text="<%$ Tokens:StringResource,checkoutshipping.aspx.13 %>" CssClass="ShippingPageContinueCheckoutButton fullwidthshortgreen action" data-icon="check" data-iconpos="right" Visible="false" onclick="btnContinueCheckout_Click" />
                </li>
            </ul>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlNewShipAddr" Visible="false">
            <ul data-role="listview">
                <li>
                    <asp:Table ID="tblShippingInfoBox" CellSpacing="0" CellPadding="2" Width="100%" runat="server">
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="left" VerticalAlign="top">
                                <table border="0" cellpadding="0" cellspacing="0" width="100%">
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx55" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.55 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingFirstName" Columns="20" MaxLength="50" runat="server" CausesValidation="true"
                                                ValidationGroup="shipping1" />
                                            <asp:RequiredFieldValidator ID="valReqShipFName" ErrorMessage="<%$ Tokens:StringResource,address.cs.13 %>"
                                                ControlToValidate="ShippingFirstName" EnableClientScript="false" runat="server"
                                                ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx57" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.57 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingLastName" Columns="20" MaxLength="50" runat="server" CausesValidation="true"
                                                ValidationGroup="shipping1" />
                                            <asp:RequiredFieldValidator ID="valReqShipLName" ErrorMessage="<%$ Tokens:StringResource,address.cs.14 %>"
                                                ControlToValidate="ShippingLastName" EnableClientScript="false" runat="server"
                                                ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx59" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.59 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingPhone" Columns="20" MaxLength="25" runat="server" CausesValidation="true"
                                                ValidationGroup="shipping1" />
                                            <asp:RequiredFieldValidator ID="valReqShipPhone" ErrorMessage="<%$ Tokens:StringResource,address.cs.15 %>"
                                                ControlToValidate="ShippingPhone" EnableClientScript="false" runat="server" ValidationGroup="shipping1"
                                                Display="None"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx62" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.62 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingCompany" Columns="25" MaxLength="100" runat="server" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="addresscs58_2" runat="server" Text="<%$ Tokens:StringResource,address.cs.58 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:DropDownList ID="ShippingResidenceType" runat="server">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx63" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.63 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingAddress1" Columns="25" MaxLength="100" runat="server" CausesValidation="true"
                                                ValidationGroup="shipping1" />
                                            <asp:RequiredFieldValidator ID="valReqShipAddr1" ErrorMessage="<%$ Tokens:StringResource,address.cs.16 %>"
                                                ControlToValidate="ShippingAddress1" EnableClientScript="false" runat="server"
                                                ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx65" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.65 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingAddress2" Columns="25" MaxLength="100" runat="server" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx66" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.66 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingSuite" Columns="25" MaxLength="50" runat="server" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx67" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.67 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingCity" Columns="25" MaxLength="50" runat="server" CausesValidation="true"
                                                ValidationGroup="shipping1" />
                                            <asp:RequiredFieldValidator ID="valReqShipCity" ErrorMessage="<%$ Tokens:StringResource,address.cs.17 %>"
                                                ControlToValidate="ShippingCity" EnableClientScript="false" runat="server" ValidationGroup="shipping1"
                                                Display="None"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx73" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.73 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:DropDownList ID="ShippingCountry" Style="width: 175px;" runat="server" OnSelectedIndexChanged="ShippingCountry_Change"
                                                AutoPostBack="True">
                                            </asp:DropDownList>
                                            <asp:RequiredFieldValidator ID="valReqShipCountry" ErrorMessage="<%$ Tokens:StringResource,createaccount.aspx.11 %>"
                                                ControlToValidate="ShippingCountry" EnableClientScript="false" runat="server"
                                                ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx69" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.69 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:DropDownList ID="ShippingState" Style="width: 175px;" runat="server">
                                            </asp:DropDownList>
                                            <asp:RequiredFieldValidator ID="valReqShipState" ErrorMessage="<%$ Tokens:StringResource,address.cs.1 %>"
                                                ControlToValidate="ShippingState" EnableClientScript="false" runat="server" ValidationGroup="shipping1"
                                                Display="None"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td width="25%">
                                            <asp:Literal ID="Checkout1aspx70" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.70 %>"></asp:Literal>
                                        </td>
                                        <td width="75%">
                                            <asp:TextBox ID="ShippingZip" Columns="14" MaxLength="10" runat="server" CausesValidation="true"
                                                ValidationGroup="shipping1" />
                                            <asp:RequiredFieldValidator ID="valReqShipZip" ErrorMessage="<%$ Tokens:StringResource,address.cs.18 %>" ControlToValidate="ShippingZip"
                                                EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                </table>
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </li>
                <li>
                    <asp:Button ID="btnNewShipAddr" runat="server" Text="<%$ Tokens:StringResource,checkoutshipping.aspx.21 %>" CssClass="ShippingPageContinueCheckoutButton fullwidthshortgreen" Visible="true" ValidationGroup="shipping1" OnClick="btnNewShipAddr_OnClick" />
                </li>
            </ul>
        </asp:Panel>
</asp:Panel>
</asp:Content>
