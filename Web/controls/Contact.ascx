<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Contact.ascx.cs" Inherits="AspDotNetStorefront.Contact" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>
<div class="page-wrap create-account-page">
    <h1>
        <asp:Literal ID="ltPageTitle" Text='<%$ Tokens:StringResource, ContactUs.PageTitle %>' runat="server"></asp:Literal></h1>
    <asp:Literal ID="ltError" runat="server" />
    <div class="page-row">
        <asp:Literal ID='litContentTopic' runat="server" Text='<%$ Tokens:Topic, contact %>' />
    </div>
    <asp:Panel ID="pnlContactForm" runat="server">
        <div class="form contact-form">
            <div class="form-group">
                <label>
                    <asp:Label ID="Label1" runat="server" Text='<%$ Tokens:StringResource, ContactUs.Name %>' EnableViewState="false" />
                    <asp:Label runat="server" ID="lblNameError" ForeColor="Red" />
                </label>
                <asp:TextBox ID='txtName' runat="server" MaxLength="50" CssClass="form-control" />
                <AjaxToolkit:TextBoxWatermarkExtender ID="tbweName" runat="server"
                    TargetControlID="txtName"
                    WatermarkText='<%$ Tokens:StringResource, ContactUs.YourName %>'
                    WatermarkCssClass="form-control watermarked" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label ID="Label2" runat="server" Text='<%$ Tokens:StringResource, ContactUs.PhoneNumber %>' EnableViewState="false" />
                    <asp:Label runat="server" ID="lblPhoneError" ForeColor="Red" />
                </label>
                <asp:TextBox ID='txtPhone' runat="server" MaxLength="27" CssClass="form-control" />
                <AjaxToolkit:TextBoxWatermarkExtender ID="tbwePhone" runat="server"
                    TargetControlID="txtPhone"
                    WatermarkText='<%$ Tokens:StringResource, ContactUs.YourPhoneNumber %>'
                    WatermarkCssClass="form-control watermarked" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label ID="Label3" runat="server" Text='<%$ Tokens:StringResource, ContactUs.EmailAddress %>' EnableViewState="false" />
                    <asp:Label runat="server" ID="lblEmailError" ForeColor="Red" />
                </label>
                <asp:TextBox ID='txtEmailAddress' runat="server" MaxLength="50" CssClass="form-control" />
                <AjaxToolkit:TextBoxWatermarkExtender ID="tbweEmail" runat="server"
                    TargetControlID="txtEmailAddress"
                    WatermarkText='<%$ Tokens:StringResource, ContactUs.YourEmailAddress %>'
                    WatermarkCssClass="form-control watermarked" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label ID="Label4" runat="server" Text='<%$ Tokens:StringResource, ContactUs.Subject %>' EnableViewState="false" />
                    <asp:Label runat="server" ID="lblSubjectError" ForeColor="Red" />
                </label>
                <asp:TextBox ID='txtSubject' runat="server" MaxLength="50" CssClass="form-control" />
                <AjaxToolkit:TextBoxWatermarkExtender ID="tbweSubject" runat="server"
                    TargetControlID="txtSubject"
                    WatermarkText='<%$ Tokens:StringResource, ContactUs.YourSubject %>'
                    WatermarkCssClass="form-control watermarked" />
            </div>
            <div class="form-group">
                <label>
                    <asp:Label ID="Label5" runat="server" Text='<%$ Tokens:StringResource, ContactUs.Message %>' EnableViewState="false" />
                    <asp:Label runat="server" ID="lblMessageError" ForeColor="Red" />
                </label>
                <asp:TextBox runat="server" ID='txtMessage' Rows='11' TextMode="MultiLine" CssClass="form-control" />
                <AjaxToolkit:TextBoxWatermarkExtender ID="tbweMessage" runat="server"
                    TargetControlID="txtMessage"
                    WatermarkText='<%$ Tokens:StringResource, ContactUs.YourMessage %>'
                    WatermarkCssClass="form-control watermarked" />
            </div>
            <asp:PlaceHolder ID="phCaptchaPane" runat="server">
                <div class="form-group">
                    <label>
                        <asp:Label runat="server" ID='lblCaptchaError' ForeColor="Red" />
                        <asp:Image ImageUrl="../Captcha.ashx?id=1" Height='50px' Width='175px' runat="server" />
                    </label>
                        <asp:TextBox runat="server" ID="txtCaptchaText" CssClass="form-control" MaxLength="6" />
                        <AjaxToolkit:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender1" runat="server"
                            TargetControlID="txtCaptchaText"
                            WatermarkText='<%$ Tokens:StringResource, Global.CaptchaText %>'
                            WatermarkCssClass="form-control watermarked" />
                </div>
            </asp:PlaceHolder>

            <div class="form-submit-wrap">
                <asp:Button ID='cmdSubmit' CssClass="button call-to-action" runat="server" Text="Submit" OnClick="cmdSubmit_Click" />
            </div>
        </div>
    </asp:Panel>
</div>
