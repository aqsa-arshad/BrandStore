<%@ Page Language="C#" AutoEventWireup="true" CodeFile="mobilecheckoutgiftcard.aspx.cs" Inherits="AspDotNetStorefront.mobilecheckoutgiftcard" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
        <asp:Panel ID="pnlHeaderGraphic" runat="server" Visible="false">
            <asp:Literal runat="server" id="CheckoutHeader"></asp:Literal>
        </asp:Panel>
        <ul id="MPShoppingCartUL" data-role="listview">
        <li class="group" data-role="list-divider">
            <div id="pnlErrorMsg"><asp:Label ID="lblErrMsg" runat="server" Font-Bold="true" ForeColor="red" Visible="false"></asp:Label></div>
        </li>
        <li>
        <asp:Panel ID="pnlEmailGiftCards" runat="server" >
            
            <asp:Repeater ID="rptrEmailGiftCards" runat="server">
                <ItemTemplate>
                    <div class="giftCardOptions">
                        <div class="giftCardDetails">
                            <span class="giftCardName"><%# AspDotNetStorefrontCore.XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "ProductName").ToString(), ThisCustomer.LocaleSetting, true) + AspDotNetStorefrontCore.XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "VariantName").ToString(), ThisCustomer.LocaleSetting, true)%></span>
                            <span class="giftCardAmount"><%# AspDotNetStorefrontCore.CommonLogic.IIF(Convert.ToDecimal(DataBinder.Eval(Container.DataItem, "InitialAmount")) == 0, "", DataBinder.Eval(Container.DataItem, "InitialAmount", "{0:$#,##0.00}"))%></span>
                         </div>
                         <div class="recipsName">
                           <asp:Label ID="Label1" runat="server" Text="<%$ Tokens:StringResource,Mobile.GitfCard.NamePrompt %>" />
                         </div>
                         <div class="recipsNameBox">
                            <asp:TextBox ID="giftcardid" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "GiftCardID") %>' Visible="false"></asp:TextBox>
                            <asp:TextBox ID="EmailName" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "EmailName") %>' ></asp:TextBox>
                         </div>
                         <div class="recipsEmail">
                           <asp:Label ID="Label2" runat="server" Text="<%$ Tokens:StringResource,Mobile.GiftCard.EmailPrompt %>" />
                         </div>
                         <div class="recipsEmailBox">
                            <asp:TextBox ID="EmailTo" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "EmailTo") %>' ></asp:TextBox>
                          </div>
                          <div class="recipsMessage">
                           <asp:Label ID="Label3" runat="server" Text="<%$ Tokens:StringResource,Mobile.GiftCard.MessagePrompt %>" />
                          </div>
                          <div class="recipsMessageBox">
                            <asp:TextBox ID="EmailMessage" CssClass="recipsMessageArea" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "EmailMessage") %>' TextMode="MultiLine" Rows="4"></asp:TextBox>
                          </div>
                      </div>
                </ItemTemplate>
                
            </asp:Repeater>
                <asp:Button ID="btnContinueCheckout" CssClass="fullwidthshortgreen" runat="server" OnClick="btnContinue_Click" Text="Continue Checkout" />
        </asp:Panel>
        <asp:Literal ID="GiftCardXmlPackage" runat="Server" Mode="PassThrough"></asp:Literal>
        </li>
        </ul>
    </asp:Panel>
</asp:Content>