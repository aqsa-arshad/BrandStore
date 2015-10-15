<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWMyAddresses.aspx.cs" Inherits="AspDotNetStorefront.JWMyAddresses" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />

    <div class="content-box-03">
        <div class="row address">

            <asp:Repeater ID="rptAddresses" runat="server" OnItemCommand="rptAddresses_ItemCommand">
                <ItemTemplate>
                    <div class="col-md-6">
                        <ul>
                            <li><asp:Label ID="lblFullName" runat="server" Text='<%# Eval("FirstName") + " " + Eval("LastName") %>'></asp:Label></li>
                            <li><asp:Label ID="lblAddress1" runat="server" Text='<%# Eval("Address1") %>'></asp:Label></li>
                            <li><asp:Label ID="lblAddress2" runat="server" Text='<%# Eval("Address2") %>'></asp:Label></li>
                            <li><asp:Label ID="lblStateZip" runat="server" Text='<%# string.IsNullOrEmpty(Eval("Zip").ToString()) ? Eval("State") : Eval("State") + ", " + Eval("Zip") %>'></asp:Label></li>
                            <li><asp:Label ID="lblCountry" runat="server" Text='<%# Eval("Country") %>'></asp:Label></li>
                            <li><asp:Label ID="lblPhone" runat="server" Text='<%# Eval("Phone") %>'></asp:Label></li>
                        </ul>
                        <div class="clearfix"></div>
                        <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-md btn-primary" CommandName="Edit" CommandArgument='<%# Eval("AddressID") %>' />
                        <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-md btn-primary" CommandName="Delete" CommandArgument='<%# Eval("AddressID") %>' OnClientClick='return confirm("Are you sure you want to delete this?")' />
                    </div>
                </ItemTemplate>
            </asp:Repeater>

        </div>
        <div class="btn-center">
            <asp:Button ID="btnAddAddresses" runat="server" CssClass="btn btn-md btn-primary" Text="Add an address" OnClick="btnAddAddresses_Click" />
        </div>
    </div>
</asp:Content>

