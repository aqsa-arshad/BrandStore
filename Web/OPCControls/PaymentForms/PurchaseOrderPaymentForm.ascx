<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PurchaseOrderPaymentForm.ascx.cs"
    Inherits="OPCControls_PurchaseOrderPaymentForm" %>
<div class="paymentMethodContents">
    <div class="form po-form">
        <div id="poInstructions" class="form-text">
            <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.56") %>' />
        </div>
        <div class="form-group">
            <label>
                <asp:Literal runat="server" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.57") %>' />
            </label>
            <asp:TextBox runat="server" CssClass="form-control" ID="TextBoxPONumber" MaxLength="50" />
            <asp:RequiredFieldValidator runat="server" ID="RequiredTextBoxPONumber" ControlToValidate="TextBoxPONumber"
                ErrorMessage='<%# StringResourceProvider.GetString("smartcheckout.aspx.58") %>' Display="Dynamic" EnableClientScript="true"
                ValidationGroup="VGPOPayment" Text="*" Enabled="true" />
        </div>
        <asp:ValidationSummary CssClass="error-wrap" ID="VSPOPayment" runat="server" EnableClientScript="true"
            DisplayMode="List" ValidationGroup="VGPOPayment" />
        <asp:Panel ID="PanelError" CssClass="error-wrap" runat="server" Visible="false">
            <asp:Label ID="ErrorMessage" runat="server" CssClass="error-large" />
        </asp:Panel>
        <asp:Button CssClass="OPButton" ID="ButtonPONext" runat="server" Visible="true" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.121") %>'
            OnClick="ButtonPONext_Click" />
    </div>
</div>
