<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWMyAddresses.aspx.cs" Inherits="AspDotNetStorefront.JWMyAddresses" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />

    <div class="content-box-03">
        <div class="row address">

            <asp:Repeater ID="rptAddresses" runat="server" OnItemCommand="rptAddresses_ItemCommand" OnItemDataBound="rptAddresses_ItemDataBound">
                <ItemTemplate>
                    <div class="col-md-6">
                        <ul>
                            <li><asp:Label ID="lblFullName" runat="server" Text='<%# Eval("FirstName") + " " + Eval("LastName") %>'></asp:Label></li>
                            <li><asp:Label ID="lblAddress1" runat="server" Text='<%# Eval("Address1") %>'></asp:Label></li>
                            <li><asp:Label ID="lblAddress2" runat="server" Text='<%# Eval("Address2") %>'></asp:Label></li>
                            <li><asp:Label ID="lblSuite" runat="server" Text='<%# Eval("Suite") %>'></asp:Label></li>
                            <li>
                                <asp:HiddenField ID="hfCity" runat="server" Value='<%# Eval("City") %>' />
                                <asp:HiddenField ID="hfState" runat="server" Value='<%# Eval("State") %>' />
                                <asp:HiddenField ID="hfZip" runat="server" Value='<%# Eval("Zip") %>' />
                                <asp:Label ID="lblCityStateZip" runat="server"></asp:Label>
                            </li>
                            <li><asp:Label ID="lblCountry" runat="server" Text='<%# Eval("Country") %>' Visible="false"></asp:Label></li>
                            <li><asp:Label ID="lblPhone" runat="server" Text='<%# Eval("Phone") %>'></asp:Label></li>
                        </ul>
                        <asp:HiddenField ID="hfAddressID" runat="server" Value='<%# Eval("AddressID") %>' />
                        <div class="clearfix"></div>
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit Address" CssClass="underline-link" CommandName="Edit" CommandArgument='<%# Eval("AddressID") %>' />
                        <div class="clearfix"></div>
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete Address" CssClass="underline-link" CommandName="Delete" CommandArgument='<%# Eval("AddressID") %>' OnClientClick='return confirm("Are you sure you want to delete this?")' />
                        <div class="clearfix"></div>
                        <asp:Button ID="btnSelect" runat="server" Text="Select" CssClass="btn btn-md btn-primary btn-block" CommandName="Select" CommandArgument='<%# Eval("AddressID") %>' />
                    </div>
                </ItemTemplate>
            </asp:Repeater>

        </div>
        <div class="btn-center">
            <asp:Button ID="btnAddAddresses" runat="server" CssClass="btn btn-md btn-primary" Text="Add an address" OnClick="btnAddAddresses_Click" /> 
        </div>
        <div class="btn-center">
            <asp:Button ID="btnBack" runat="server" CssClass="btn btn-md btn-primary" Text="Back to Account" OnClick="btnBack_Click" />
        </div>
    </div>
</asp:Content>

