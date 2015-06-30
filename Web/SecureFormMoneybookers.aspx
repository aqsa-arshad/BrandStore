<%@ Page Language="c#" Inherits="AspDotNetStorefront.SecureFormMoneybookers" CodeFile="SecureFormMoneybookers.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <div style="margin: 0px; padding: 8px; border-width: 1px; border-style: solid; border-color: #888888; background-color: #EEEEEE; margin-bottom: 10px;">
        <aspdnsf:Topic runat="Server" TopicName="3DSecureExplanation"/>
    </div>
    <iframe src="secureAuthMoneybookers.aspx" width="100%" height="500" scrolling="auto" frameborder="0" style="margin: 0px; padding: 8px; border-width: 1px; border-style: solid; border-color: #888888;">
    </iframe>
</asp:Content>

