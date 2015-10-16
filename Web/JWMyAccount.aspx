﻿<%@ Page Title="My Account" Language="C#" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" AutoEventWireup="true" CodeFile="JWMyAccount.aspx.cs" Inherits="AspDotNetStorefront.JWMyAccount" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" runat="Server">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />

    <div class="content-box-03">
        <div class="row">
            <div class="col-md-6">
                <h5>Current Order Status</h5>
                <ul>
                    <li>Order Number: <b>xxxxx</b></li>
                    <li>Status: <b>Shipped 8/6/2015</b></li>
                    <li><a href="#" class="underline-link">Track Current Order</a> </li>
                </ul>
            </div>


            <div class="col-md-6">
                <h5>Downloads</h5>
                <ul>
                    <li><a href="#" class="underline-link">View Your Downloads</a> </li>
                </ul>
            </div>
        </div>
        <div class="text-center">
            <button class="btn btn-md btn-primary" type="submit">View Order History</button>
        </div>
    </div>

    <div class="content-box-03">
        <div class="row">
            <div class="col-md-6">
                <h5>Primary Billing Address</h5>
                <ul>
                    <li><asp:Label ID="lblBANA" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblBAFullName" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblBAAddress1" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblBAAddress2" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblBAStateZip" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblBACountry" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblBAPhone" runat="server"></asp:Label></li>
                </ul>
            </div>

            <div class="col-md-6">
                <h5>Primary Shipping Address</h5>
                <ul>
                    <li><asp:Label ID="lblSANA" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblSAFullName" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblSAAddress1" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblSAAddress2" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblSAStateZip" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblSACountry" runat="server"></asp:Label></li>
                    <li><asp:Label ID="lblSAPhone" runat="server"></asp:Label></li>
                </ul>
            </div>
        </div>
        <div class="text-center">
            <asp:Button ID="btnUpdateAddresses" runat="server" CssClass="btn btn-md btn-primary" Text="Update Your Address" OnClick="btnUpdateAddresses_Click" />
        </div>
    </div>

</asp:Content>
