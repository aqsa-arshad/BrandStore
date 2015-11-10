<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWAddAddresses.aspx.cs" Inherits="AspDotNetStorefront.JWAddAddresses" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="content-box-03 body-forms">
        <p>Update your address by editing the fields below. <label>*Required field</label></p>
        <div class="row">
            <div class="col-md-5">
                <div class="form-group">
                    <asp:HiddenField ID="hfAddressID" runat="server" />
                    <asp:HiddenField ID="hfPreviousURL" runat="server" />
                    <label><asp:Label ID="lblNickName" runat="server" Text='<%$ Tokens:StringResource, address.cs.49 %>'></asp:Label></label>
                    <asp:TextBox ID="txtNickName" CssClass="form-control" runat="server" MaxLength="100"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="txtNickName" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.90 %>" ValidationExpression="^[a-zA-Z][0-9a-zA-Z .,'-]*$" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblFirstName" runat="server" Text='<%$ Tokens:StringResource, address.cs.2 %>'></asp:Label></label>
                    <asp:TextBox ID="txtFirstName" CssClass="form-control" runat="server" MaxLength="100"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtFirstName" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.13 %>" />
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="txtFirstName" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.91 %>" ValidationExpression="^[a-zA-Z][0-9a-zA-Z .,'-]*$" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblLastName" runat="server" Text='<%$ Tokens:StringResource, address.cs.3 %>'></asp:Label></label>
                    <asp:TextBox ID="txtLastName" CssClass="form-control" runat="server" MaxLength="100"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtLastName" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.14 %>" />
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="txtLastName" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.92 %>" ValidationExpression="^[a-zA-Z][0-9a-zA-Z .,'-]*$" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblPhoneNumber" runat="server" Text='<%$ Tokens:StringResource, address.cs.4 %>'></asp:Label></label>
                    <asp:TextBox ID="txtPhoneNumber" CssClass="form-control" runat="server" MaxLength="25"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtPhoneNumber" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.15 %>" />
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator4" runat="server" ControlToValidate="txtPhoneNumber" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.93 %>" ValidationExpression="\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblCompany" runat="server" Text='<%$ Tokens:StringResource, address.cs.5 %>'></asp:Label></label>
                    <asp:TextBox ID="txtCompany" CssClass="form-control" runat="server" MaxLength="100"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator5" runat="server" ControlToValidate="txtCompany" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.94 %>" ValidationExpression="^[a-zA-Z][0-9a-zA-Z .,'-]*$" />
                </div>
            </div>
            <div class="col-md-5 pull-desktop-right">
                <div class="form-group">
                    <label><asp:Label ID="lblAddress1" runat="server" Text='<%$ Tokens:StringResource, address.cs.6 %>'></asp:Label></label>
                    <asp:TextBox ID="txtAddress1" CssClass="form-control" runat="server" MaxLength="100"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="txtAddress1" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.16 %>" />
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator6" runat="server" ControlToValidate="txtAddress1" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.95 %>" ValidationExpression="[0-9a-zA-Z #.,-/():]+" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblAddress2" runat="server" Text='<%$ Tokens:StringResource, address.cs.7 %>'></asp:Label></label>
                    <asp:TextBox ID="txtAddress2" CssClass="form-control" runat="server" MaxLength="100"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator7" runat="server" ControlToValidate="txtAddress2" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.96 %>" ValidationExpression="[0-9a-zA-Z #.,-/():]+" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblSuite" runat="server" Text='<%$ Tokens:StringResource, address.cs.8 %>'></asp:Label></label>
                    <asp:TextBox ID="txtSuite" CssClass="form-control" runat="server" MaxLength="50"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator8" runat="server" ControlToValidate="txtSuite" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.97 %>" ValidationExpression="^[0-9a-zA-Z .,'-]*$" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblCity" runat="server" Text='<%$ Tokens:StringResource, address.cs.9 %>'></asp:Label></label>
                    <asp:TextBox ID="txtCity" CssClass="form-control" runat="server" MaxLength="100"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="txtCity" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.17 %>" />
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator9" runat="server" ControlToValidate="txtCity" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.98 %>" ValidationExpression="^[a-z A-Z]+$" />
                </div>
                <div class="form-group" hidden="hidden">
                    <label><asp:Label ID="lblCountry" runat="server" Text='<%$ Tokens:StringResource, address.cs.53 %>'></asp:Label></label>
                    <asp:DropDownList ID="ddlCountry" runat="server" OnSelectedIndexChanged="ddlCountry_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ddlCountry" Display="Dynamic" ValidationGroup="AddAddress" InitialValue="Please select" ErrorMessage="<%$ Tokens:StringResource, address.cs.102 %>" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblState" runat="server" Text='<%$ Tokens:StringResource, address.cs.10 %>'></asp:Label></label>
                    <asp:DropDownList ID="ddlState" runat="server"></asp:DropDownList>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="ddlState" Display="Dynamic" ValidationGroup="AddAddress" InitialValue="Please select" ErrorMessage="<%$ Tokens:StringResource, address.cs.1 %>" />
                </div>
                <div class="form-group">
                    <label><asp:Label ID="lblZip" runat="server" Text='<%$ Tokens:StringResource, address.cs.12 %>'></asp:Label></label>
                    <asp:TextBox ID="txtZip" CssClass="form-control" runat="server" MaxLength="10"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="txtZip" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.18 %>" />
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator10" runat="server" ControlToValidate="txtZip" Display="Dynamic" ValidationGroup="AddAddress" ErrorMessage="<%$ Tokens:StringResource, address.cs.100 %>" ValidationExpression="^\d{5}$|^\d{5}-\d{4}$" />
                </div>
                <div class="clearfix"></div>
                <div class="form-group">
                    <asp:Button ID="btnSave" runat="server" Visible="false" CssClass="btn btn-md btn-primary btn-half" Text="Save" ValidationGroup="AddAddress" OnClick="btnSave_Click" />
                    <asp:Button ID="btnUpdate" runat="server" Visible="false" CssClass="btn btn-md btn-primary btn-half" Text="Update" ValidationGroup="AddAddress" OnClick="btnUpdate_Click" />
                    <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-md btn-primary btn-half" Text="Cancel" OnClick="btnCancel_Click" />
                </div>
            </div>

        </div>
    </div>
</asp:Content>

