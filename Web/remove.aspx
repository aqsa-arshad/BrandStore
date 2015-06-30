<%@ Page language="c#" Inherits="AspDotNetStorefront.remove" CodeFile="remove.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    
        <div id="Div1" align="left" runat="server">
            <asp:Literal runat="Server" ID="litRemoveEmailCompleteMessage" />
        </div>

    </asp:Panel>
</asp:Content>

