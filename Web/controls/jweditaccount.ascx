<%@ Control Language="C#" AutoEventWireup="true" CodeFile="JWEditAccount.ascx.cs" Inherits="AspDotNetStorefront.JWEditAccount" %>


<div class="row">
    <h5>Update Your Account</h5>
    <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip.</p>
    <div class="col-md-6">
        <div class="form-group">
            <label><asp:Label ID="lblFirstName" runat="server" Text='<%$ Tokens:StringResource, createaccount.aspx.13 %>'></asp:Label></label>
            <asp:TextBox ID="txtFirstName" runat="server" MaxLength="100"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtFirstName" Display="Dynamic" ValidationGroup="UpdateAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.13 %>" />
            <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="txtFirstName" Display="Dynamic" ValidationGroup="UpdateAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.91 %>" ValidationExpression="^[a-zA-Z][0-9a-zA-Z .,'-]*$" />
        </div>
        <div class="form-group">
            <label><asp:Label ID="lblLastName" runat="server" Text='<%$ Tokens:StringResource, createaccount.aspx.14 %>'></asp:Label></label>
            <asp:TextBox ID="txtLastName" runat="server" MaxLength="100"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtLastName" Display="Dynamic" ValidationGroup="UpdateAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.14 %>" />
            <asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="txtLastName" Display="Dynamic" ValidationGroup="UpdateAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.92 %>" ValidationExpression="^[a-zA-Z][0-9a-zA-Z .,'-]*$" />
        </div>
        <div class="form-group">
            <label><asp:Label ID="lblEmail" runat="server" Text='<%$ Tokens:StringResource, createaccount.aspx.15 %>'></asp:Label></label>
            <asp:TextBox ID="txtEmail" runat="server" ReadOnly="true"></asp:TextBox>
        </div>
    </div>
    <div class="col-md-6 pull-right">
        <div class="form-group">
            <label><asp:Label ID="lblPhoneNumber" runat="server" Text='<%$ Tokens:StringResource, createaccount.aspx.23 %>'></asp:Label></label>
            <asp:TextBox ID="txtPhoneNumber" runat="server" MaxLength="25"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtPhoneNumber" Display="Dynamic" ValidationGroup="UpdateAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.15 %>" />
            <asp:RegularExpressionValidator ID="RegularExpressionValidator4" runat="server" ControlToValidate="txtPhoneNumber" Display="Dynamic" ValidationGroup="UpdateAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.93 %>" ValidationExpression="^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$" />
        </div>
        <div class="form-group">
            <label><asp:Label ID="lblPassword" runat="server" Text='<%$ Tokens:StringResource, createaccount.aspx.18 %>'></asp:Label></label>
            <asp:TextBox ID="txtPassword" CssClass="form-control" runat="server" MaxLength="50" TextMode="Password"></asp:TextBox>
        </div>
        <div class="form-group">
            <label><asp:Label ID="lblConfirmPassword" runat="server" Text='<%$ Tokens:StringResource, createaccount.aspx.21 %>'></asp:Label></label>
            <asp:TextBox ID="txtConfirmPassword" CssClass="form-control" runat="server" MaxLength="50" TextMode="Password"></asp:TextBox>
        </div>
        <div class="clearfix"></div>
        <div class="form-group">
            <asp:Button ID="btnUpdate" runat="server" CssClass="btn btn-md btn-primary btn-block" Text="Update" ValidationGroup="UpdateAddress" />
        </div>
    </div>
</div>
