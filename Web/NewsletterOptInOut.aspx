<%@ Page ClientTarget="UpLevel" language="c#" Inherits="AspDotNetStorefront.NewsletterOptInOut" CodeFile="NewsletterOptInOut.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master"   %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Import namespace="AspDotNetStorefrontCore" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >      
        <asp:Literal ID='litOptOption' runat="server" />
        <asp:Button ID="cmdConfirm" runat="server" Text="Confirm" Visible="false" 
            onclick="cmdConfirm_Click" />
    </asp:Panel>
</asp:Content>