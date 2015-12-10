<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="controls_Search" %>
<div class="form-group search-form">
    <asp:Panel runat="server" DefaultButton="SearchButton">

        <asp:TextBox runat="server"
            ID="SearchBox"
            CssClass="form-control search-text"
            Text="<%$ Tokens:STRINGRESOURCE, search.prompt %>" />
        
        <asp:Button runat="server" CausesValidation="false"
            ID="SearchButton"
            CssClass="button search-button"
            OnClick="SearchButton_Click"
            UseSubmitBehavior="false"
            Text="<%$ Tokens:STRINGRESOURCE, search.go %>" />

    </asp:Panel>
</div>
