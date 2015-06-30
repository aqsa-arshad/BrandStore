<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EndPreview.ascx.cs" Inherits="AspDotNetStorefront.controls_EndPreview" %>
<asp:Panel runat="server" ID="PreviewPanel">
    <div class="preview-block">
        <div class="preview-block-right">
            <asp:Button runat="server" Text="End Preview" CssClass="preview-button" ID="EndPreviewButton" OnClick="EndPreviewButton_Click" />
        </div>
        <div class="preview-block-left">
            <asp:Label ID="litPreviewText" runat="server"  />
        </div>
        <div style="clear: both;"></div>
    </div>
</asp:Panel>
