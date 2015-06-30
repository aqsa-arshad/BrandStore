<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WalletSelector.ascx.cs" Inherits="CIM_WalletSelector" %>
<asp:UpdatePanel ID="PanelWallet" runat="server" ChildrenAsTriggers="true" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Repeater ID="ListViewCreditCards" runat="server" OnItemDataBound="ListViewCreditCards_ItemDataBound">
            <ItemTemplate>
                <div class="page-row">
                    <div class="one-third">
                        <div id="walletUseCardWrap">
                            <asp:RadioButton runat="server" ID="ButtonBillCard" GroupName="BillCard" Text=""
                                CommandName="BillThisCard" AutoPostBack="true"
                                OnCheckedChanged="BillThisCard_CheckChanged" />
                        </div>
                    </div>
                    <div class="one-third">
                        <asp:Image ID="ImageCard" CssClass="wallet-card-image" runat="server"
                            ImageUrl='<%# GatewayAuthorizeNet.DisplayTools.GetCardImage("~/App_Themes/Skin_1/images/", ((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).CardType) %>' />
                    </div>
                    <div class="one-third">
                        <div id="wallet-card-type">
                            <asp:Label ID="LabelCardType" runat="server" Text='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).CardType %>' />
                        </div>
                        <div id="wallet-card-number">
                            <asp:Label ID="LabelCardNumber" runat="server" Text='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).CreditCardNumberMasked  %>' />
                        </div>
                        <div id="wallet-exp-date">
                            Exp.
                            <asp:Label ID="LabelCardExpDate" runat="server" Text='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).ExpirationMonth  %>' />
                            /
                            <asp:Label ID="Label1" runat="server" Text='<%#((GatewayAuthorizeNet.PaymentProfileWrapper)Container.DataItem).ExpirationYear %>' />
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:Panel ID="PanelNoSavedCards" runat="server" Visible="false">
            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, account.aspx.105%>" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
