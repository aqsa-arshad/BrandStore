<%@ Page Language="C#" AutoEventWireup="true" CodeFile="checkoutgiftcard.aspx.cs" Inherits="AspDotNetStorefront.checkoutgiftcard" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register src="controls/CheckoutSteps.ascx" tagname="CheckoutSteps" tagprefix="checkout" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <div class="page-wrap checkout-giftcard-page">
            <checkout:checkoutsteps id="CheckoutSteps" runat="server" />
            <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
            <asp:Panel ID="pnlEmailGiftCards" runat="server">
                <h1>
                    <asp:Literal ID="ltAccount" Text="<%$ Tokens:StringResource,Header.GiftCards %>" runat="server" />
                </h1>

                <asp:Panel ID="pnlError" CssClass="error-wrap" runat="server" Visible="false">
                    <asp:Label ID="lblErrMsg" runat="server" CssClass="error-large"></asp:Label>
                </asp:Panel>

                <asp:Repeater ID="rptrEmailGiftCards" runat="server">
                    <HeaderTemplate>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="form giftcard-form">
                            <div class="group-header form-header giftcard-header">
                                <%# AspDotNetStorefrontCore.XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "ProductName").ToString(), ThisCustomer.LocaleSetting, true) + AspDotNetStorefrontCore.XmlCommon.GetLocaleEntry(DataBinder.Eval(Container.DataItem, "VariantName").ToString(), ThisCustomer.LocaleSetting, true)%>
                                <%# AspDotNetStorefrontCore.CommonLogic.IIF(Convert.ToDecimal(DataBinder.Eval(Container.DataItem, "InitialAmount")) == 0, "", DataBinder.Eval(Container.DataItem, "InitialAmount", "{0:$#,##0.00}"))%>
                            </div>

                            <div class="form-group">
                                <label>Recipient's Name:</label>
                                <asp:TextBox ID="giftcardid" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "GiftCardID") %>' Visible="false"></asp:TextBox>
                                <asp:TextBox ID="EmailName" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "EmailName") %>' CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Recipient's Email Address:</label>

                                <asp:TextBox ID="EmailTo" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "EmailTo") %>' CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Message for Recipient:</label>
                                <asp:TextBox ID="EmailMessage" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "EmailMessage") %>' TextMode="MultiLine" CssClass="form-control" Rows="4"></asp:TextBox>
                            </div>
                        </div>
                    </ItemTemplate>
                    <FooterTemplate>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Button ID="btnContinueCheckout" CssClass="button call-to-action gift-card-button" runat="server" OnClick="btnContinue_Click" Text="Continue Checkout" />
            </asp:Panel>
            <asp:Literal ID="GiftCardXmlPackage" runat="Server" Mode="PassThrough"></asp:Literal>
        </div>
    </asp:Panel>
</asp:Content>
