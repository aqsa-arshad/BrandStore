<%@ Page Title="My Account" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWMyAccount.aspx.cs" Inherits="AspDotNetStorefront.JWMyAccount" %>

<%@ Register TagPrefix="aspdnsf" TagName="CustomerAlerts" Src="~/controls/JWEditAccount.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />

    <div class="content-box-03">
        <div class="row tablet-view">
            <div class="col-md-6 pull-left-md pull-sm-no">
                <h5>Current Order Status</h5>
                <asp:Label ID="accountaspx55" runat="server" Text="<%$ Tokens:StringResource,account.aspx.55 %>"></asp:Label>
                <ul id="ulLatestOrderStatus" runat="server">
                    <li>Order Number: <b runat="server" id="bOrderNumber"></b></li>
                    <li>Status: <b id="bStatus" runat="server"></b></li>
                    <a class="underline-link" id="aOrderDetail" runat="server" target="_blank">View Detail</a>
                </ul>
            </div>

            <div class="col-md-6 pull-left-md pull-sm-no">
                <h5>Downloads</h5>
                <ul>
                    <li><a href="MyDownloads.aspx" class="underline-link">View Your Downloads</a> </li>
                </ul>
            </div>
        </div>

        <div class="btn-center">
            <a href="OrderHistory.aspx" class="btn btn-md btn-primary" type="submit">View Order History</a>
        </div>
    </div>

    <div class="content-box-03">
        <div class="row address tablet-view">
            <div class="col-md-6 pull-left-md pull-sm-no">
                <h5>Contact Information</h5>
                <p>
                    <asp:Label ID="lblName" runat="server" CssClass="block-text"></asp:Label>
                    <asp:Label ID="lblmailId" runat="server" CssClass="block-text"></asp:Label>

                    <asp:Label ID="lblPhoneNumber" runat="server" CssClass="block-text"></asp:Label>
                    <br />
                    <asp:LinkButton runat="server" CssClass="underline-link block-text" ID="lnkEditAccountInfo" OnClick="UpdateAccountInfo_Click" Text="Update Contact Information" Visible="false"></asp:LinkButton>
                </p>
            </div>
        </div>
        <div class="row address tablet-view">
            <div class="col-md-6 pull-left-md pull-sm-no">
                <h5>Primary Billing Address</h5>
                <ul style="overflow: hidden;">
                    <li>
                        <asp:Label ID="lblBANA" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblBAFullName" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblBAAddress1" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblBAAddress2" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblBASuite" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblBACityStateZip" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblBACountry" runat="server" Visible="false"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblBAPhone" runat="server"></asp:Label></li>
                </ul>
                <div class="clearfix"></div>
                <asp:Button ID="btnChangeBillingAddress" runat="server" CssClass="btn btn-md btn-primary btn-block" Text="Change Billing Address" OnClick="btnChangeBillingAddress_Click" />
            </div>

            <div class="col-md-6 pull-left-md pull-sm-no">
                <h5>Primary Shipping Address</h5>
                <ul style="overflow: hidden;">
                    <li>
                        <asp:Label ID="lblSANA" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblSAFullName" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblSAAddress1" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblSAAddress2" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblSASuite" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblSACityStateZip" runat="server"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblSACountry" runat="server" Visible="false"></asp:Label></li>
                    <li>
                        <asp:Label ID="lblSAPhone" runat="server"></asp:Label></li>
                </ul>
                <div class="clearfix"></div>
                <asp:Button ID="btnChangeShippingAddress" runat="server" CssClass="btn btn-md btn-primary btn-block" Text="Change Shipping Address" OnClick="btnChangeShippingAddress_Click" />
            </div>
        </div>
    </div>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#btnViewAccountofuser").addClass("hide-element");
        });
    </script>

</asp:Content>

