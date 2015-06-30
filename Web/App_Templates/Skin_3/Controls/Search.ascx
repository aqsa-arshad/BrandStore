<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="controls_Search" %>

<asp:Panel runat="server" DefaultButton="SearchButton">

	<div class="form-group">
		<asp:TextBox runat="server" 
			ID="SearchBox" 
			CssClass="search-box form-control" />
	</div>
	<asp:Button runat="server" 
		ID="SearchButton" 
		CssClass="search-go btn btn-default" 
		OnClick="SearchButton_Click" 
		UseSubmitBehavior="false" 
		Text="<%$ Tokens:STRINGRESOURCE, search.go %>"/>
</asp:Panel>