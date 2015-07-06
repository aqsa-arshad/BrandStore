<%@ Page Language="C#" AutoEventWireup="true" CodeFile="setupFTS.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Admin_setupFTS" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<script type="text/javascript">
function CreateNew()
{  
   var textBox1 = document.getElementById('<%=txtNewCatalogName.ClientID%>');
   var textBox2 = document.getElementById('<%=txtNewCatalogPath.ClientID%>');
   var radioReuse = document.getElementById('<%=radioReuse.ClientID%>');
   var radioCreate = document.getElementById('<%=radioCreate.ClientID%>');
   
   if (radioCreate.checked == true)
        {
            radioReuse.checked = false;
            var listBox = document.getElementById('<%= lstCatalogNames.ClientID %>');
            listBox.selectedIndex = -1;
            listBox.disabled = true;        
            textBox1.disabled = false;
            textBox2.disabled = false;
        }
}
function Reuse()
{
    var textBox1 = document.getElementById('<%=txtNewCatalogName.ClientID%>');
    var textBox2 = document.getElementById('<%=txtNewCatalogPath.ClientID%>');
    var radioReuse = document.getElementById('<%=radioReuse.ClientID%>');
    var radioCreate = document.getElementById('<%=radioCreate.ClientID%>');
    
    if (radioReuse.checked == true)
        {
            radioCreate.checked = false;
            var listBox = document.getElementById('<%= lstCatalogNames.ClientID %>');
            listBox.disabled = false;
            textBox1.disabled = true;
            textBox2.disabled = true;
            textBox1.value = "";
            textBox2.value = "";
        }
    }
    function CheckCatalog() {
        var textBox1 = document.getElementById('<%=txtNewCatalogName.ClientID%>');
        var textBox2 = document.getElementById('<%=txtNewCatalogPath.ClientID%>');
        var radioReuse = document.getElementById('<%=radioReuse.ClientID%>');
        var radioCreate = document.getElementById('<%=radioCreate.ClientID%>');

        if (radioCreate.checked == true && radioReuse.checked == false) {
            if (textBox1.value != "" && textBox2.value != "") {
                if (confirm("<%=JSwarn1 %>" + textBox1.value + "<%=JSwarn2 %>" + textBox2.value + "?")) {
                    document.getElementById('<%= Page.Form.ClientID %>').submit;
                }
                else {
                    return false;
                }
            }

            if (textBox1.value != "" && textBox2.value == "") {
                if (confirm("<%=JSwarn1 %>" + textBox1.value + "<%=JSwarn3 %>")) {
                    document.getElementById('<%= Page.Form.ClientID %>').submit;
                }
                else {
                    return false;
                }
            }

            if (textBox1.value == "" && textBox2.value != "") {
                alert("<%=JSwarn4 %>");
                return false;
            }

            if (textBox1.value == "" && textBox2.value == "") {
                alert("<%=JSwarn5 %>");
                return false;
            }
        }

        else if (radioReuse.checked == true && radioCreate.checked == false) {
            if (textBox1.value == "" && textBox2.value == "") {
                var listBox = document.getElementById('<%=lstCatalogNames.ClientID %>');
                var text = "";

                for (i = 0; i < listBox.options.length; i++) {
                    if (listBox.options[i].selected) {
                        text = text + listBox.options[i].text;
                    }
                }

                if (text == "") {
                    alert("<%=JSwarn6 %>");
                    return false;
                }
                else {
                    if (confirm("<%=JSwarn7 %>" + text + "?")) {
                        document.getElementById('<%= Page.Form.ClientID %>').submit;
                    }
                    else {
                        return false;
                    }
                }
            }

            if (textBox1.value != "" || textBox2.value != "") {
                alert("<%=JSwarn8 %>");
                textBox1.value = "";
                textBox2.value = "";
                return false;
            }
        }
        else {
            alert("<%=JSwarn6 %>");
            return false;
        }
    }

    function WarnUninstall() {
        if (confirm("<%=JSwarn9 %>")) {
            return true;
        }
        else {
            return false;
        }
    }

    function WarnOptimize() {
        if (confirm("<%=JSwarn10 %>")) {
            return true;
        }
        else {
            return false;
        }
    }
</script>    
      <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    <div>
    <table style="border-width: medium; border-color: #C0C0C0; border-top-style: solid;
        border-right-style: solid; border-left-style: solid; background-color: #F8F8F8;" 
        width="100%">
        <tr>
            <td></td>
        </tr>
        <tr>
            <td style="width:30%">
            </td>
            <td style="width:40%; padding-top: 15px;">
                <table style="border: thin solid #CACA00; background-color: #FFFFE8" width="100%">
                    <tr style="width:100%" align="center">
                        <td align="center">
                        <table>
                            <tr style="text-align:center" align="center">
                                <td align="center" style="padding-top: 15px; padding-bottom: 10px">
                                    <asp:Label ID="lblIntro" runat="server" Width="100%" Font-Bold="True" Text="<%$ Tokens:StringResource, setupFTS.aspx.2 %>">
                                    </asp:Label>
                                </td>
                            </tr>  
                            <tr style="text-align:center" align="center">
                                <td align="center" style="padding-top: 10px; padding-bottom: 10px">
                                    <asp:Label ID="lblMSFTESQL" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.3 %>" Width="100%">
                                    </asp:Label>
                                </td>
                            </tr>                            
                            <tr style="text-align:center" align="center">
                                <td align="center" style="padding-top: 10px; padding-bottom: 10px">
                                    <asp:Label ID="lblEnableFTS" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.4 %>" Width="100%">
                                    </asp:Label>                                    
                                </td>
                            </tr>
                            <tr style="text-align:center" align="center">
                                <td style="vertical-align:middle; padding-top: 10px; padding-bottom: 10px">
                                    <asp:Label ID="lblLanguage" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.5 %>"></asp:Label>
                                    <asp:DropDownList ID="ddlLanguage" runat="server" Width="155px" 
                                        ForeColor="Black">
                                        <asp:ListItem>Chinese-Simplified</asp:ListItem>
                                        <asp:ListItem>Chinese-Traditional</asp:ListItem>
                                        <asp:ListItem>Danish</asp:ListItem>
                                        <asp:ListItem>Dutch</asp:ListItem>
                                        <asp:ListItem>English-International</asp:ListItem>
                                        <asp:ListItem>English-US</asp:ListItem>
                                        <asp:ListItem>French</asp:ListItem>
                                        <asp:ListItem>German</asp:ListItem>
                                        <asp:ListItem>Italian</asp:ListItem>
                                        <asp:ListItem>Japanese</asp:ListItem>
                                        <asp:ListItem>Korean</asp:ListItem>
                                        <asp:ListItem Selected="True">Neutral</asp:ListItem>
                                        <asp:ListItem>Polish</asp:ListItem>
                                        <asp:ListItem>Portuguese</asp:ListItem>
                                        <asp:ListItem>Portuguese(Brazil)</asp:ListItem>
                                        <asp:ListItem>Russian</asp:ListItem>
                                        <asp:ListItem>Spanish</asp:ListItem>
                                        <asp:ListItem>Swedish</asp:ListItem>
                                        <asp:ListItem>Thai</asp:ListItem>
                                        <asp:ListItem>Turkish</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr style="text-align:center" align="center">
                                <td align="center" style="padding-top: 10px; padding-bottom: 10px">
                                <asp:RadioButton ID="radioCreate" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.6 %>"/>
                                <asp:RadioButton ID="radioReuse" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.7 %>" />                                
                                </td>
                            </tr>                            
                            <tr style="text-align:center" align="center">
                                <td align="center" style="padding-top: 10px; padding-bottom: 10px;">
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="lblNewCatalogName" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.8 %>"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtNewCatalogName" runat="server" BackColor="#FFFFE8" 
                                                MaxLength="30" Enabled="False" Width="300px"></asp:TextBox>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr style="text-align:center" align="center">
                                <td align="center" style="padding-top: 10px; padding-bottom: 10px">                                
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="lblNewCatalogPath" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.9 %>"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:TextBox ID="txtNewCatalogPath" runat="server" BackColor="#FFFFE8" 
                                                MaxLength="80" Enabled="False" Width="300px"></asp:TextBox>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr style="text-align:center" align="center">
                                <td align="center" style="padding-top:10px;">
                                    <asp:Label ID="lblCatalogList" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.10 %>" Width="100%">
                                    </asp:Label>                                    
                                </td>
                            </tr>                            
                            <tr style="text-align:center" align="center">
                                <td align="center" style="padding-bottom: 15px">                                    
                                    <asp:ListBox ID="lstCatalogNames" runat="server" Width="300px" 
                                        BackColor="#FFFFE8" Rows="6"></asp:ListBox>
                                        
                                </td>
                            </tr>                            
                        </table>                                               
                        </td>
                    </tr>
                </table>
            </td>
            <td style="width:30%">
            </td>
        </tr>
        <tr>
            <td style="padding-top: 10px; padding-bottom: 20px"></td>
        </tr>
    </table>
    </div>
    <div>
        <table style="border-width: medium; border-color: #C0C0C0; border-right-style: solid; border-left-style: solid; background-color: #F8F8F8; border-bottom-style: solid;"
            width="100%">            
            <tr align="center">
                <td style="width:28%">
                </td>
                <td style="padding-top: 10px; padding-bottom: 15px; width:44%">                                    
                        <asp:Button ID="btn_uninstallFTS" CssClass="normalButtons" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.21 %>" 
                        OnClientClick="return WarnUninstall()" Width="155px" onclick="btn_uninstallFTS_Click"/>&nbsp;
                        <asp:Button ID="btn_installFTS" CssClass="normalButtons" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.11 %>" 
                        OnClientClick="return CheckCatalog()" Width="155px"/>&nbsp;
                        <asp:Button ID="btn_optimize" CssClass="normalButtons" runat="server" Text="<%$ Tokens:StringResource, setupFTS.aspx.22 %>" 
                        OnClientClick="return WarnOptimize()" Width="155px" 
                            onclick="btn_optimize_Click"/>
                </td>
                <td style=" text-align:right; width:28%; vertical-align:bottom;">
                    <asp:HyperLink ID="hyperNoiseWord" runat="server" NavigateUrl="setupFTS_NoiseWords.aspx" 
                        Text="<%$ Tokens:StringResource, setupFTS.aspx.28 %>" Width="100%" Visible="False"></asp:HyperLink>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>