<%@ Page language="c#" Inherits="AspDotNetStorefront.showproduct" CodeFile="showproduct.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" EnableViewStateMac="false"  %>
<%@ Register assembly="AspDotNetStorefrontControls" namespace="AspDotNetStorefrontControls" tagprefix="aspdnsf" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel runat="server" >      
        <asp:Literal ID="litOutput" runat="server"></asp:Literal>  
    </asp:Panel>
</asp:Content>

