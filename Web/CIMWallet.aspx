<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CIMWallet.aspx.cs" Inherits="AspDotNetStorefront.CIMWallet" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register Src="CIM/Wallet.ascx" TagName="Wallet" TagPrefix="uc1" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <div>
        <asp:ScriptManagerProxy ID="SMProxy" runat="server">
            <Scripts>
                <asp:ScriptReference Path="~/CIM/scripts/ajaxHelpers.js" />
                <asp:ScriptReference Path="~/jscripts/tooltip.js" />
            </Scripts>
        </asp:ScriptManagerProxy>
        <uc1:Wallet ID="Wallet" runat="server" />
        <script type="text/javascript" language="Javascript">
            Sys.Application.add_load(function () {
            	var toolTip = new ToolTip('aCardCodeToolTip', 'card-code-tooltip',
                        '<iframe width="400" height="370" frameborder="0" marginheight="2" marginwidth="2" left="-50" top="-50" scrolling="no" src="App_Themes/skin_1/images/verificationnumber.gif"></iframe>'
                        );
            });
        </script>
    </div>
</asp:Content>
