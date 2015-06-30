<%@ Page Language="C#" AutoEventWireup="true" Title="Run Custom Report" CodeFile="rpt_custom.aspx.cs" Inherits="AspDotNetStorefrontAdmin.rpt_custom" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<script type="JavaScript">
    function getDelete()
    {
        return 'Confirm Delete';
    }
</script>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable" width="130">
                                    <font class="subTitle">Reports:</font></td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #a2a2a2;" />
                                <td style="width: 5px;" />
                                <td class="titleTable">
                                    <font class="subTitle">Report Settings:</font></td>
                            </tr>
                            <tr>
                                <td class="contentTableNP" valign="top" width="130">
                                     <asp:DropDownList runat="server" ID="ddReports" AutoPostBack="true" OnSelectedIndexChanged="ddReports_SelectedIndexChanged">
                                                    </asp:DropDownList>                                                    
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #a2a2a2;" />
                                <td style="width: 5px;" />
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">
                                        &nbsp;
                                        <asp:Panel ID="Panel1" runat="server" Height="50px" Width="125px">
                                        </asp:Panel>
                                        &nbsp;</div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
        <asp:Panel ID="ResultsPanel" runat="server" Height="100%" Visible="False" Width="100%">
            <asp:GridView ID="ResultsGrid" runat="server">
            </asp:GridView>
        </asp:Panel>
</asp:Content>