<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConfigureTextPanel.ascx.cs" Inherits="AspDotNetStorefront.ConfigureTextPanel" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%--<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>--%>

<div class='<%# this.CssClass %>'>

    <asp:Panel ID="pnlError" runat="server" Visible='false' CssClass="config_error">
        <asp:Label ID="lblError" runat="server"></asp:Label>
    </asp:Panel>

    <table style="width: 650px; border-style: none;">
        <tr>
            <td valign="top" align="left">&nbsp;<b><asp:Label ID="Label1" Text="Layout:" runat="server" /></b>
            </td>
            <td valign="top" align="left">&nbsp;<b><asp:Label ID="lblLayout" runat="server" /></b>
            </td>
        </tr>
        <tr>
            <td valign="top" align="left">&nbsp;<b><asp:Label ID="Label10" Text="Field:" runat="server" /></b>
            </td>
            <td valign="top" align="left">&nbsp;<b><asp:Label ID="lblField" Text="Text" runat="server" /></b>
            </td>
        </tr>
        <tr>
            <td valign="top" align="left">&nbsp;<b><asp:Label ID="Label2" Text="Description:" runat="server" /></b>
            </td>
            <td valign="top" align="left">
                <asp:Literal ID="litText" runat="server"></asp:Literal>
                <%--<telerik:RadEditor runat="server" id="radText">
                            <ImageManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                            <DocumentManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                            <FlashManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                            <MediaManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                            <SilverlightManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                            <TemplateManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                        </telerik:RadEditor>--%>
            </td>
        </tr>
        <tr id="trAdvanced" runat="server" visible="false">
            <td valign="top" align="right">
                
            </td>
            <td valign="top" align="left">
                <a href="javascript:void(0);" onclick="$get('<%= pnlAdvanced.ClientID %>').style.display = $get('<%= pnlAdvanced.ClientID %>').style.display == 'none'? '':'none';">Advanced</a>
            </td>
        </tr>

    </table>

    <%--<asp:Panel ID="pnlAdvanced" runat="server" Visible="false" >--%>
    <div id="pnlAdvanced" runat="server" visible="false" class="appconfig_adv" style='display: none;'>

        <table style="width: 500px; border-style: none;">
            <tr>
                <td valign="top" align="right">Super Only:</td>
                <td valign="top" align="left">
                    <asp:RadioButtonList ID="rbSuperOnly" runat="server" RepeatDirection="Horizontal" />
                    <%--Will be bound on the code-behind--%>
                </td>
            </tr>

            <tr>
                <td valign="top" align="right">Value Type:</td>
                <td valign="top" align="left">
                    <%--Will be bound on the code-behind--%>
                    <asp:DropDownList ID="cboValueType" runat="server" />
                </td>
            </tr>

            <tr>
                <td valign="top" align="right">Allowable Values:</td>
                <td valign="top" align="left">
                    <%--Will be bound on the code-behind--%>
                    <asp:TextBox ID="txtAllowableValues" runat="server"
                        TextMode="MultiLine"
                        Columns="35"
                        Rows="3">
                    </asp:TextBox>
                </td>
            </tr>
        </table>

    </div>
</div>

