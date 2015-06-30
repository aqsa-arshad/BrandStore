<%@ Page Language="C#" CodeFile="InvalidRequest.aspx.cs" Inherits="AspDotNetStorefront._InvalidRequest" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >   
    
    <div>
        <asp:Literal runat="server" ID="ltErrorTopic" />
    </div>
    <div runat="server" ID="divErrorCode">
        <p>
        <asp:Label runat="server" ID="lblErrorCodeText" Text="<%$ Tokens:StringResource, invalidrequest.aspx.1 %>" /><asp:Label runat="server" ID="lblErrorCode" />
        </p>
    </div>
    <div runat="server" id="divAdminMessage" visible="false">
        <p>
        <asp:Label runat="server" ID="lblAdminMessage" Text="<%$ Tokens:StringResource, invalidrequest.aspx.2 %>" />
        </p>
    </div>
</asp:Panel>
</asp:Content>
