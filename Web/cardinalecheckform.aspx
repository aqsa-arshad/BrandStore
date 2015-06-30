<%@ Page Language="c#" Inherits="AspDotNetStorefront.cardinalecheckform" CodeFile="cardinalecheckform.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    
        <asp:Panel ID="Panel1" runat="server" Height="50px" Width="100%" Visible="false">
            <p>
                <b>
                    <asp:Literal ID="Literal1" runat="server" Text="<%$ Tokens:StringResource,cardinalform.aspx.1 %>"></asp:Literal></b></p>
        </asp:Panel>
        <asp:Panel ID="Panel2" runat="server" Width="100%" Visible="true">
            <div style="margin: 0px; padding: 8px; border-width: 1px; border-style: solid; border-color: #888888; background-color: #EEEEEE;">
                <aspdnsf:Topic ID="Hdr" runat="Server" TopicName="CardinalMyECheckExplanation"/>
            </div>
            
            <div>
                <iframe src="cardinalecheckauth.aspx" width="100%" height="700" scrolling="auto" frameborder="0" style="margin: 0px; padding: 8px; border-width: 1px; border-style: solid; border-color: #888888;">
                </iframe>
            </div>
        </asp:Panel>
        
    </asp:Panel>
</asp:Content>
