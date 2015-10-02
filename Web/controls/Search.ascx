<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="controls_Search" %>

<asp:Panel runat="server" DefaultButton="SearchButton">
    <div class="form-group search-form">

        <asp:TextBox runat="server"
            ID="SearchBox"
            CssClass="form-control search-text"
            Text="<%$ Tokens:STRINGRESOURCE, search.prompt %>"/>

        <asp:Button runat="server"
            ID="SearchButton"
            CssClass="button search-button"
            OnClick="SearchButton_Click"
            UseSubmitBehavior="false"
            Text="<%$ Tokens:STRINGRESOURCE, search.go %>" />
    </div>

</asp:Panel>
 