<%@ Page language="c#" Inherits="AspDotNetStorefront.mobiledesktopwarning" CodeFile="mobiledesktopwarning.aspx.cs" EnableEventValidation="false"  MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<asp:Content ContentPlaceHolderID="PageContent" runat="server">
        <div id="MobileContent">
            <div style="padding:5px;">
                <ul data-role="listview">
                    <li><b><asp:Literal runat="server" ID="WarningMessage"></asp:Literal></b></li>
                    <li><asp:Button runat="server" ID="ContinueButton" CssClass="fullwidthshortgreen action" OnClick="btn_ContinueButtonClick" data-role="button" data-icon="arrow-r" data-iconpos="right" /></li>
                    <li><asp:Button runat="server" ID="CancelButton" CssClass="fullwidthshortred" OnClick="btn_CancelButtonClick" data-role="button" data-icon="arrow-l" /></li>
                </ul>
            </div>
        </div>
    <iframe style="height:0px;width:0px;visibility:hidden;display:none;" src="about:blank">
        this prevents back forward cache
    </iframe>
</asp:Content>
