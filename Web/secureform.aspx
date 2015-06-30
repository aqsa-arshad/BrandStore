<%@ Page Language="c#" Inherits="AspDotNetStorefront.secureform" CodeFile="secureform.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
        <asp:Panel ID="Panel2" runat="server" Width="100%" Visible="true">
            <div style="margin: 0px; padding: 8px; border-width: 1px; border-style: solid; border-color: #888888; background-color: #EEEEEE;">
                <aspdnsf:Topic ID="Hdr" runat="Server" TopicName="3DSecureExplanation"/>
            </div>
            
            <div>
                <iframe src="secureauth.aspx" width="100%" height="500" scrolling="auto" frameborder="0" style="margin: 0px; padding: 8px; border-width: 1px; border-style: solid; border-color: #888888;">
                </iframe>
            </div>
        </asp:Panel>
    
    </asp:Panel>
</asp:Content>
