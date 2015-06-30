<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmailProduct.ascx.cs" Inherits="AspDotNetStorefront.EmailProduct" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>

<asp:UpdatePanel ID="upEmailProduct" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="page-wrap email-product-page">
            <h1 class="email-product-page-header">
                <asp:Label ID="emailproduct_aspx_4" runat="server"></asp:Label>
            </h1>
            <div class="page-row back-link">
                <asp:HyperLink ID="ProductNavLink" CssClass="ProductNavLink" runat="server"></asp:HyperLink>
            </div>
            <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                <div class="page-row">

                    <asp:Label ID="emailproduct_aspx_8" Font-Bold="true" runat="server"></asp:Label>

                    <asp:HyperLink ID="ReturnToProduct" Font-Bold="true" runat="server"></asp:HyperLink>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlRequireReg" runat="server" Visible="false">
                <div class="page-row require-registration">
                    <asp:Literal ID="emailproduct_aspx_1" Mode="PassThrough" runat="server"></asp:Literal>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlEmailToFriend" runat="server" Visible="false" DefaultButton="btnSubmit">
                <div class="row row-email-friend">
                    <div class="one-third">
                        <asp:Image ID="imgProduct" runat="server" />
                    </div>
                    <div class="two-thirds">
                        <div class="form form-email-friend">
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="emailproduct_aspx_11" runat="server"></asp:Literal>
                                </label>
                                <asp:TextBox ID="txtToAddress" runat="server" MaxLength="75" CssClass="form-control" CausesValidation="true"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="reqToAddress" ControlToValidate="txtToAddress" runat="server" Display="Dynamic"></asp:RequiredFieldValidator>
								<aspdnsf:EmailValidator ID="regexToAddress" ControlToValidate="txtToAddress" Display="Dynamic" runat="server" />
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="emailproduct_aspx_22" runat="server"></asp:Literal></label>
                                <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" CssClass="form-control" Rows="7"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="emailproduct_aspx_15" runat="server"></asp:Literal>
                                </label>
                                <asp:TextBox ID="txtFromAddress" MaxLength="75" runat="server" CssClass="form-control" CausesValidation="true"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="reqFromAddress" ControlToValidate="txtFromAddress" runat="server" Display="Dynamic"></asp:RequiredFieldValidator>
                                <aspdnsf:EmailValidator ID="regexFromAddress" ControlToValidate="txtFromAddress" Display="Dynamic" runat="server"></aspdnsf:EmailValidator>
                            </div>

                            <div class="form-text">
                                <span>
                                    <asp:Label ID="emailproduct_aspx_18" runat="server"></asp:Label></span>
                                <asp:Label ID="emailproduct_aspx_19" runat="server"></asp:Label>
                            </div>

                            <div class="form-submit-wrap">
                                <asp:Button ID="btnSubmit" runat="server" CssClass="button call-to-action" CausesValidation="true" OnClick="btnSubmit_Click" />
                            </div>

                            <AjaxToolkit:TextBoxWatermarkExtender ID="tbweRecipient" runat="server"
                                TargetControlID="txtToAddress"
                                WatermarkCssClass="form-control watermarked"
                                WatermarkText='<%$ Tokens:StringResource, emailproduct.aspx.12 %>' />

                            <AjaxToolkit:TextBoxWatermarkExtender ID="tbweMessage" runat="server"
                                TargetControlID="txtMessage"
                                WatermarkCssClass="form-control watermarked"
                                WatermarkText='<%$ Tokens:StringResource, emailproduct.aspx.23 %>' />

                            <AjaxToolkit:TextBoxWatermarkExtender ID="tbweYourEmail" runat="server"
                                TargetControlID="txtFromAddress"
                                WatermarkCssClass="form-control watermarked"
                                WatermarkText='<%$ Tokens:StringResource, emailproduct.aspx.25 %>' />


                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
