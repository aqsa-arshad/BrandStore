<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SecureNetVaultPanel.ascx.cs" Inherits="AspDotNetStorefront.SecureNetVaultPanel" %>
<asp:ScriptManagerProxy ID="scriptProxy" runat="server">
</asp:ScriptManagerProxy>
<script type="text/javascript" language="Javascript" src="jscripts/tooltip.js"></script>
<asp:UpdatePanel ID="pnlSecureNetVaultCards" runat="server" Visible="false" UpdateMode="Always" ChildrenAsTriggers="true">
    <%--<Triggers>
        <asp:AsyncPostBackTrigger ControlID="ibAddNewAddress" />
        <asp:AsyncPostBackTrigger ControlID="btnSecureNetSaveCard" />
    </Triggers>--%>
    <ContentTemplate>
        <asp:DataList ID="dlSecureNetVault" runat="server" Width="100%"
            OnItemDataBound="dlSecureNetVault_ItemDataBound" DataKeyField="PaymentId"
            OnDeleteCommand="dlSecureNetVault_DeleteCommand" AlternatingItemStyle-BackColor="WhiteSmoke"
            ItemStyle-BackColor="White" EditItemStyle-BackColor="White" RepeatLayout="Table" RepeatDirection="Vertical" RepeatColumns="1">
            <ItemTemplate>
                <table cellpadding="4" cellspacing="0">
                    <td class="addressImagebuttons">
                        <nobr>
                            <asp:ImageButton ID="ibDelete" runat="server" CommandName="Delete" ToolTip='<%$ Tokens:StringResource,admin.common.Delete %>'
                                ImageUrl="~/App_Themes/Skin_1/images/icons/delete.png" AlternateText="Delete"
                                OnClientClick='return confirm("Are you sure you want to delete this?")' />
                            </nobr>
                        <asp:HiddenField ID="hfVaultId" runat="server" Value='<%# DataBinder.Eval(Container.DataItem,"PaymentId") %>' Visible="false" />
                    </td>
                    <td valign="top">
                        <nobr>
                            <strong><asp:Literal runat="server" Text="<%$ Tokens:StringResource,account.creditcardprompt %>" /></strong>:&nbsp;
                            <asp:Label ID="lblAddressHTML" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"CardNumberPadded") %>' />
                            </nobr>
                    </td>
                    <td valign="top" width="100%">
                        <nobr>
                            <strong><asp:Literal runat="server" Text="<%$ Tokens:StringResource,account.expires %>" /></strong>:&nbsp;
                            <asp:Label ID="lblExpDate" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"ExpDateFormatted") %>' />
                            </nobr>
                    </td>
                </table>
            </ItemTemplate>
        </asp:DataList>
        <div class="addressfooter">
            <asp:Panel ID="pnlAddSecureNetCardPrompt" runat="server">
                <asp:ImageButton ID="ibAddNewAddress" OnClick="AddNewVaultCard" runat="server" ValidationGroup="none" ImageUrl="~/App_Themes/Skin_1/images/icons/add_address.png" />
                <asp:LinkButton ID="lbAddNewAddress" OnClick="AddNewVaultCard" runat="server" ValidationGroup="none" />
            </asp:Panel>
            <asp:Panel ID="pnlAddSecureNetCard" runat="server" Visible="false">
                <span class="error">
                    <asp:Literal ID="litSNCError" runat="server" /></span>
                <aspdnsf:CreditCardPanel ID="SecureNetCreditCardPanel" runat="server" CreditCardExpDtCaption="<%$ Tokens:StringResource, address.cs.33 %>"
                    CreditCardNameCaption="<%$ Tokens:StringResource, address.cs.23 %>" CreditCardNoSpacesCaption="<%$ Tokens:StringResource, shoppingcart.cs.106 %>"
                    CreditCardNumberCaption="<%$ Tokens:StringResource, address.cs.25 %>" CreditCardTypeCaption="<%$ Tokens:StringResource, address.cs.31 %>"
                    CreditCardVerCdCaption="<%$ Tokens:StringResource, address.cs.28 %>" HeaderCaption="<%$ Tokens:StringResource, checkoutcard.aspx.6 %>"
                    WhatIsThis="<%$ Tokens:StringResource, address.cs.50 %>" CCNameReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.24 %>"
                    CreditCardStartDtCaption="<%$ Tokens:StringResource, address.cs.59 %>" CreditCardIssueNumCaption="<%$ Tokens:StringResource, address.cs.61 %>"
                    CreditCardIssueNumNote="<%$ Tokens:StringResource, address.cs.63 %>" CCNameValGrp="SecureNetAddCard"
                    CCNumberReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.26 %>" CCNumberValGrp="SecureNetAddCard"
                    CCVerCdReqFieldErrorMessage="<%$ Tokens:StringResource, address.cs.29 %>" CCVerCdValGrp="SecureNetAddCard"
                    ShowCCVerCd="True" ShowCCStartDtFields="false"
                    ShowCCVerCdReqVal="<%$ Tokens:AppConfigBool, CardExtraCodeIsOptional %>" ShowValidatorsInline="true" ShowCimSaveCard="false" />
                <asp:Button runat="server" Text="Save Credit Card" ID="btnSecureNetSaveCard" ValidationGroup="SecureNetAddCard" OnClick="btnSecureNetSaveCard_Click" />
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
