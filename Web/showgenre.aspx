<%@ Page Language="C#" AutoEventWireup="true" CodeFile="showgenre.aspx.cs" Inherits="AspDotNetStorefront.showgenre" MasterPageFile="~/App_Templates/Skin_1/template.master"%>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
		<span class="paypalAd">
			<asp:Literal ID="ltPayPalAd" runat="server" />
		</span>
        <asp:Literal ID="litOutput" runat="server"></asp:Literal>  
    </asp:Panel>
</asp:Content>