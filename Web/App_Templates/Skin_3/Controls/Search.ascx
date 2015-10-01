<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="controls_Search" %>

<asp:Panel runat="server" DefaultButton="SearchButton">

    <div class="form-group">       
        <asp:TextBox ID="SearchBox" runat="server"
             CssClass="search-feild" />

        <asp:Button runat="server"
            ID="SearchButton"            
            OnClick="SearchButton_Click"
            UseSubmitBehavior="false"             
            style="display:none"/>
    </div>

</asp:Panel>
