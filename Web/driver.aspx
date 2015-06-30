<%@ Page language="c#" Inherits="AspDotNetStorefront.driver" CodeFile="driver.aspx.cs" EnableEventValidation="false" MasterPageFile="~/App_Templates/Skin_1/template.master"  %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
        <aspdnsf:Topic id="Topic1" runat="server" EnforceDisclaimer="true" EnforcePassword="true" EnforceSubscription="true" AllowSEPropogation="true"/>
    </asp:Panel>
</asp:Content>
