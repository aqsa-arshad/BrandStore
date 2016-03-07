<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserInfoAfterLogin.ascx.cs" Inherits="controls_UserInfoAfterLogin" %>
<h3 id="WelcomeHeadingAfterUserLogin" runat="server"></h3>

<p class="hideforpublicuser">
    <asp:Label ID="lblSOF" runat="server" />
</p>
<p class="hideforpublicuser" runat="server" id="pAboutSalesFunds" visible="false">
    <a class="underline-link" target="_blank" href="JWAboutSOF.aspx"><span>About Sales Funds</span></a>
</p>
<a href="JWMyAccount.aspx" class="btn btn-md btn-primary btn-block tablet-btn" type="button" id="btnViewAccountofuser">VIEW MY ACCOUNT</a>