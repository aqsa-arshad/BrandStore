<%@ Page language="c#" Inherits="AspDotNetStorefrontAdmin.encrypttest" CodeFile="encrypttest.aspx.cs" EnableEventValidation="false" 
 MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"%>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
       <div style="height:19;padding-top:5px;" class="tablenormal">
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.V2Encryption %>"></asp:Literal>
       </div>
       <br />
       <asp:Panel ID="pnlv2" runat="server" DefaultButton="Button1">       
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.Encrypt %>"></asp:Literal>
        <asp:TextBox ID="TextBox1" runat="server" Width="390px"></asp:TextBox>
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.EncryptIt %>" /><br />
       <br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.EncryptedValue %>"></asp:Literal>&nbsp; &nbsp;
        <asp:Label ID="Label1" runat="server" Width="395px"></asp:Label><br />
        <br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.DecryptedValue %>"></asp:Literal>&nbsp;
        <asp:Label ID="Label2" runat="server" Width="389px"></asp:Label>
        </asp:Panel>
        <br />
        <br />
        <hr size="1"/>
        <br />
        <div style="height:19;padding-top:5px;" class="tablenormal">
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.V1Encryption %>"></asp:Literal>
        </div>
        <br />
        <br />
        <asp:Panel ID="pnlv1" runat="server" DefaultButton="Button2">
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.StringToEncrypt %>"></asp:Literal>
        <asp:TextBox ID="TextBox2" runat="server" Width="390px"></asp:TextBox>
        <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.EncryptIt %>" /><br />
        <br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.EncryptedValue %>"></asp:Literal>&nbsp; &nbsp;
        <asp:Label ID="Label3" runat="server" Width="395px"></asp:Label><br />
        <br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.DecryptedValue %>"></asp:Literal>&nbsp;
        <asp:Label ID="Label4" runat="server" Width="389px"></asp:Label>
        </asp:Panel>
        <br />
        <br />
        <br />
        <hr size="1"/>
        <br />
        <div style="height:19;padding-top:5px;" class="tablenormal">
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.TextAgain %>"></asp:Literal>
        </div>
        <br />
        <br />
        <asp:Panel ID="pnlplain" runat="server" DefaultButton="Button3">
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.V1Encryption2 %>"></asp:Literal>
        <asp:TextBox ID="TextBox3" runat="server" Width="390px"></asp:TextBox>
        <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.DoIt %>" /><br />
        <br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.V1Plain %>"></asp:Literal>&nbsp; &nbsp;
        <asp:Label ID="Label5" runat="server" Width="395px"></asp:Label><br />
        <br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.V2Encryption2 %>"></asp:Literal>&nbsp; &nbsp;
        <asp:Label ID="Label6" runat="server" Width="395px"></asp:Label><br />
        <br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.encrypttest.V2Decrypted %>"></asp:Literal>&nbsp;
        <asp:Label ID="Label7" runat="server" Width="389px"></asp:Label>
        </asp:Panel>
</asp:Content>

