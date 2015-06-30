<%@ Control Language="C#" AutoEventWireup="true" CodeFile="eCheckPaymentForm.ascx.cs"
    Inherits="OPCControls_eCheckPaymentForm" %>
<asp:UpdatePanel UpdateMode="Conditional" ChildrenAsTriggers="false" ID="UpdatePanelECheck"
    runat="server">
    <ContentTemplate>
        <asp:ValidationSummary CssClass="error-wrap" ID="VSECheckPayment" runat="server" EnableClientScript="true" DisplayMode="List" ValidationGroup="VGECheckPayment" />
        <asp:Panel ID="PanelError" CssClass="error-wrap" runat="server" Visible="false">
            <asp:Label ID="ErrorMessage" runat="server" CssClass="error-large" />
        </asp:Panel>
        <div class="paymentMethodContents">
            <asp:Panel ID="PanelECheckDetails" runat="server" DefaultButton="BtnSaveECheckPaymentForm">

                <div class="form e-check-form">
                    <div class="form-group">
                        <label>
                            <asp:Label runat="server" ID="LabelNameOnAccount" Text='<%# StringResourceProvider.GetString("address.cs.36") %>' />
                        </label>

                        <asp:TextBox runat="server" ID="NameOnAccount" CssClass="form-control" MaxLength="100" />
                        <asp:RequiredFieldValidator runat="server" ID="RFNameOnAccount" ControlToValidate="NameOnAccount"
                            ErrorMessage='<%# StringResourceProvider.GetString("address.cs.37") %>' Display="Dynamic" EnableClientScript="true"
                            ValidationGroup="VGECheckPayment" Text="*" Enabled="true" />

                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Label runat="server" ID="LabelBankName" Text='<%# StringResourceProvider.GetString("address.cs.38") %>' />
                        </label>
                        <asp:TextBox runat="server" ID="BankName" CssClass="form-control" MaxLength="100" />
                        <asp:RequiredFieldValidator runat="server" ID="RFBankName" ControlToValidate="BankName"
                            ErrorMessage='<%# StringResourceProvider.GetString("address.cs.39") %>' Display="Dynamic" EnableClientScript="true"
                            ValidationGroup="VGECheckPayment" Text="*" Enabled="true" />
                        <div>
                            <asp:Label runat="server" ID="LabelBankName2" Text='<%# StringResourceProvider.GetString("address.cs.40") %>' />
                        </div>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Label runat="server" ID="LabelBankAba" Text='<%# StringResourceProvider.GetString("address.cs.41") %>' />
                        </label>
                        <asp:Image ID="ImgECheckBankABAImage1" runat="server" BorderStyle="None" BorderWidth="0" />
                        <asp:TextBox runat="server" ID="BankABA" CssClass="form-control" MaxLength="100" />
                        <asp:Image ID="ImgECheckBankABAImage2" runat="server" BorderStyle="None" BorderWidth="0" />
                        <asp:RequiredFieldValidator runat="server" ID="RFBankABA" ControlToValidate="BankABA"
                            ErrorMessage='<%# StringResourceProvider.GetString("address.cs.43") %>' Display="Dynamic" EnableClientScript="true"
                            ValidationGroup="VGECheckPayment" Text="*" Enabled="true" />
                        <div class="form-text">
                            <asp:Label runat="server" ID="LabelBankAba2" Text='<%# StringResourceProvider.GetString("address.cs.42") %>' />
                        </div>

                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Label runat="server" ID="LabelAccountNumber" Text='<%# StringResourceProvider.GetString("address.cs.44") %>' />
                        </label>

                        <asp:TextBox runat="server" ID="AccountNumber" CssClass="form-control" MaxLength="100" />
                        <asp:Image ID="ImgECheckBankAccountImage" runat="server" BorderStyle="None" BorderWidth="0" />
                        <asp:RequiredFieldValidator runat="server" ID="RFAccountNumber" ControlToValidate="AccountNumber"
                            ErrorMessage='<%# StringResourceProvider.GetString("address.cs.46") %>' Display="Dynamic" EnableClientScript="true"
                            ValidationGroup="VGECheckPayment" Text="*" Enabled="true" />
                        <div class="form-text">
                            <asp:Label runat="server" ID="Label1" Text='<%# StringResourceProvider.GetString("address.cs.45") %>' />
                        </div>
                    </div>
                    <div class="form-group">
                        <label>
                            <asp:Label runat="server" ID="LabelAccountType" Text='<%# StringResourceProvider.GetString("address.cs.47") %>' />
                        </label>

                        <asp:DropDownList ID="DDLAccountType" CssClass="form-control" runat="server" AutoPostBack="false">
                            <asp:ListItem Text="Checking" Value="CHECKING" />
                            <asp:ListItem Text="Savings" Value="SAVINGS" />
                            <asp:ListItem Text="Business Checking" Value="BUSINESS CHECKING" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ID="RFDDLAccountType" ControlToValidate="DDLAccountType"
                            ErrorMessage='<%# StringResourceProvider.GetString("address.cs.47") %>' Display="Dynamic" EnableClientScript="true"
                            ValidationGroup="VGECheckPayment" Text="*" Enabled="true" />
                    </div>
                    <div class="form-text">
                        <asp:Label runat="server" ID="LabelNotes" Text='<%# StringResourceProvider.GetString("address.cs.48") %>' />
                    </div>
                    <div class="form-submit-wrap">
                        <asp:Button ID="BtnSaveECheckPaymentForm" Text='<%# StringResourceProvider.GetString("smartcheckout.aspx.120") %>' runat="server" CssClass="OPButton" OnClick="BtnSaveECheckPaymentForm_Click" />
                    </div>

                </div>

            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
