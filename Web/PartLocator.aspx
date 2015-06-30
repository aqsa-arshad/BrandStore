<%@ Page Title="" Language="C#" MasterPageFile="~/App_Templates/Skin_3/template.master" CodeFile="PartLocator.aspx.cs" Inherits="AspDotNetStorefront.PartLocator" %>
<%@ Register TagPrefix="aspdnsf" TagName="parts" Src="~/Controls/PartLocator.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="PageContent" Runat="Server">
      
    <div id="ctl00_PageContent_pnlContent">
        <h1><asp:Literal ID="TitleLiteral" runat="server"></asp:Literal></h1>
        <asp:Literal ID="BodyLiteral" runat="server"></asp:Literal>
    </div>
    <br />
    <asp:Panel ID="pnlContent" runat="server"></asp:Panel>

</asp:Content>

