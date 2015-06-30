<%@ Page Language="C#" CodeFile="mobileInvalidRequest.aspx.cs" Inherits="AspDotNetStorefront._InvalidRequest" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >   
        <ul data-role="listview" style="line-height:20px;">
            <li>
                <asp:Literal runat="server" ID="ltErrorTopic" />
            </li>
            <li  runat="server" ID="divErrorCode">
                <asp:Label runat="server" ID="lblErrorCodeText" Text="<%$ Tokens:StringResource, invalidrequest.aspx.1 %>" /><asp:Label runat="server" ID="lblErrorCode" />
            </li>
            <li runat="server" id="divAdminMessage" visible="false">
                <asp:Label runat="server" ID="lblAdminMessage" Text="<%$ Tokens:StringResource, invalidrequest.aspx.2 %>" />
            </li>
        </ul>
    </asp:Panel>
</asp:Content>
