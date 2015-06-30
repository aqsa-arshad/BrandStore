<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Wallet.ascx.cs" Inherits="CIM_Wallet" %>
<%@ Register Src="CreditCardEditor.ascx" TagName="CreditCardEditor" TagPrefix="uc1" %>

<div id="walletHeaderDescription">
    <span id="walletHeaderDescriptionText"></span>
</div>
<asp:UpdatePanel ID="PanelWallet" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Repeater ID="ListViewCreditCards" runat="server" OnItemCommand="ListViewCreditCards_ItemCommand"
            OnItemDataBound="ListViewCreditCards_ItemDataBound">
            <ItemTemplate>
                <div class="page-row">
                    <div class="one-third">
                        <asp:Image ID="ImageCard" CssClass="wallet-card-image" runat="server" 
                            ImageUrl='<%# GatewayAuthorizeNet.DisplayTools
                                .GetCardImage("~/App_Themes/Skin_1/images/", ((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).CardType) %>' />
                    </div>
                    <div class="one-third">
                        <div class="wallet-card-type">
                            <asp:Label ID="LabelCardType" runat="server" Text='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).CardType %>' /></div>
                        <div class="wallet-card-number">
                            <asp:Label ID="LabelCardNumber" runat="server" Text='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).CreditCardNumberMasked  %>' /></div>
                        <div class="wallet-exp-date">
                            Exp.
                            <asp:Label ID="LabelCardExpDate" runat="server" Text='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).ExpirationMonth  %>' />
                            /
                            <asp:Label ID="Label1" runat="server" Text='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).ExpirationYear %>' /></div>
                    </div>
                    <div class="one-third">
                        <div class="wallet-remove-button-wrap">
                            <asp:Button runat="server" ID="ButtonRemoveCard" Text="<%$ Tokens:StringResource, account.aspx.102%>" CommandName="Delete" CssClass="button wallet-button"
                                CommandArgument='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).ProfileId %>' /></div>
                        <div class="wallet-edit-button-wrap">
                            <asp:Button runat="server" ID="ButtonEditCard" CssClass="button wallet-button" Text="<%$ Tokens:StringResource, account.aspx.103%>" CommandName="Edit" CommandArgument='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).ProfileId %>' /></div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <div id="walletAddPaymentWrap">
            <asp:Button runat="server" Text="<%$ Tokens:StringResource, account.aspx.104%>" CssClass="button wallet-button" ID="ButtonAddPaymentType" OnClick="ButtonAddPaymentType_Click" />
            <asp:Panel runat="server" ID="PanelAddPaymentType">
                <uc1:CreditCardEditor ID="CreditCardEditor1" runat="server" />
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
