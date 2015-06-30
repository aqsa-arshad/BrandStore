<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.changeencryptkey" CodeFile="changeencryptkey.aspx.cs"
    EnableEventValidation="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <b></b><strong>
        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.ChangeYourEncryptKey %>" /><br />
    </strong>
    <br />
    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.Recommendation %>" /><br />
    <br />
    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.StoringCreditCards %>" />
    <asp:Label ID="StoringCC" runat="server" Font-Bold="True"></asp:Label><br />
    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.RequireCreditCardStorage %>" />
    <asp:Label ID="RecurringProducts" runat="server" Font-Bold="True"></asp:Label><br />
    <br />
    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.Reminder %>" />
    <asp:Panel runat="server" ID="DoItPanel" Width="100%" DefaultButton="Button1">
        <br />
        <hr size="1" />
        <br />
        <asp:Label ID="Label6" runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.Backup %>"
            CssClass="noticeMsg" />
        <br />
        <br />
        <asp:UpdatePanel ID="updPnlChangeEncryptKey" runat="server">
            <ContentTemplate>
                <h4>
                    <asp:Literal ID="ltChangeEncryptKey" runat="server" Text="<%$ Tokens:StringResource, admin.changeencrypt.ChangeEncryptKey %>" />
                    <asp:RadioButtonList ID="rblChangeEncryptKey" runat="server" OnSelectedIndexChanged="rblChangeEncryptKey_OnSelectedIndexChanged"
                        AutoPostBack="true">
                        <asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.yes %>"></asp:ListItem>
                        <asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.no %>"></asp:ListItem>
                    </asp:RadioButtonList>
                </h4>
                <asp:Panel ID="pnlChangeEncryptKeyMaster" runat="server" Visible="false">
                    <strong>
                        <asp:Label ID="Label5" runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.BePatient %>" /></strong><br />
                    <br />
                    <asp:Literal ID="ltEncryptKeyGeneration" runat="server" Text="<%$ Tokens:StringResource,admin.changeencrypt.EncryptKeyGeneration %>" />
                    <asp:RadioButtonList ID="rblEncryptKeyGenType" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="rblEncryptKeyGenType_OnSelectedIndexChanged">
                        <asp:ListItem Value="auto" Text="<%$ Tokens:StringResource, admin.changeencrypt.Auto %>" />
                        <asp:ListItem Value="manual" Text="<%$ Tokens:StringResource, admin.changeencrypt.Manual %>" />
                    </asp:RadioButtonList>
                    <asp:Image ID="imgEncryptKeyGeneration" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                        ToolTip="<%$Tokens:StringResource,admin.changeencrypt.tooltip.imgEncryptKeyGeneration %>"
                        runat="server" />
                    <asp:Panel runat="server" ID="pnlEncryptKey" Visible="false">
                        <br />
                        <asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.AtLeast %>" />
                        <br />
                        <br />
                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.NewEncryptKey %>" />
                        <asp:TextBox ID="NewEncryptKey" runat="server" Width="317px" MaxLength="50">
                        </asp:TextBox>
                    </asp:Panel>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <br />
        <br />
        <asp:UpdatePanel ID="updPnlChangeMachineKey" runat="server">
            <ContentTemplate>
                <h4>
                    <asp:Literal ID="ltChangeSetMachineKey" runat="server" Text="<%$ Tokens:StringResource, admin.changeencrypt.SetChangeMachineKey %>" />
                    <asp:RadioButtonList ID="rblChangeMachineKey" runat="server" OnSelectedIndexChanged="rblChangeMachineKey_OnSelectedIndexChanged"
                        AutoPostBack="true">
                        <asp:ListItem Value="true" Text="<%$Tokens:StringResource, admin.common.yes %>"></asp:ListItem>
                        <asp:ListItem Value="false" Text="<%$Tokens:StringResource, admin.common.no %>"></asp:ListItem>
                    </asp:RadioButtonList>
                </h4>
                <asp:Panel ID="pnlChangeSetMachineKey" runat="server" Visible="false">
                    <asp:Literal ID="ltMachineKeyGeneration" runat="server" Text="<%$ Tokens:StringResource,admin.changeencrypt.MachineKeyGeneration %>" />
                    <asp:RadioButtonList ID="rblMachineKeyGenType" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="rblMachineKeyGenType_OnSelectedIndexChanged">
                        <asp:ListItem Value="auto" Text="<%$ Tokens:StringResource, admin.changeencrypt.Auto %>" />
                        <asp:ListItem Value="manual" Text="<%$ Tokens:StringResource, admin.changeencrypt.Manual %>" />
                    </asp:RadioButtonList>
                    <asp:Image ID="imgMachineKeyGeneration" ImageUrl="~/App_Themes/Admin_Default/images/info.gif"
                        ToolTip="<%$Tokens:StringResource,admin.changeencrypt.tooltip.imgMachineKeyGeneration %>"
                        runat="server" />
                    <asp:Panel runat="server" ID="pnlMachineKey" Visible="false">
                        <br />
                        <asp:Label ID="lblMachineKeyLength" runat="server" Text="<%$Tokens:StringResource, admin.changeencryptkey.MachineKeyAtLeast %>" />
                        <br />
                        <br />
                        <asp:Label ID="lblEnterValidationKey" runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.NewValidationKey %>" />
                        <asp:TextBox ID="txtValidationKey" runat="server" Width="317px" MaxLength="64" />
                        <br />
                        <asp:Label ID="lblEnterDecryptKey" runat="server" Text="<%$Tokens:StringResource, admin.changeencrypt.NewDecryptKey %>" />
                        <asp:TextBox ID="txtDecryptKey" runat="server" Width="317px" MaxLength="32" />
                    </asp:Panel>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <br />
        <br />
        <div align="center">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:Button ID="Button1" runat="server" CssClass="normalButtons" OnClick="Button1_Click"
                        Text="<%$Tokens:StringResource, admin.changeencrypt.UpdateEncryptKey %>" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <br />
        <br />
        <asp:UpdatePanel ID="updPnlNotices" runat="server">
            <ContentTemplate>
                <asp:Label ID="OkLabel" runat="server" Font-Bold="True" ForeColor="Blue" Text="<%$Tokens:StringResource, admin.changeencryptkey.Done %>"
                    Visible="False"></asp:Label>
                <asp:Label ID="ErrorLabel" runat="server" Font-Bold="True" ForeColor="Crimson" Text="<%$Tokens:StringResource, admin.changeencryptkey.Error %>"
                    Visible="False"></asp:Label>
                <div runat="server" id="divError" style="text-align: center;">
                    <asp:Literal runat="server" ID="ltError" Visible="False"></asp:Literal>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
