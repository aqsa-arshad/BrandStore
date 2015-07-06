<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.entityBulkDisplayOrder"
    CodeFile="entityBulkDisplayOrder.aspx.cs" MaintainScrollPositionOnPostback="true" %>

<%@ OutputCache Duration="1" Location="none" %>
<head runat="server">
    <style type="text/css">
        .style1
        {
            width: 100px;
        }
        .style2
        {
            width: 550px;
        }
    </style>
</head>
<body>
    <form runat="server">
    <div id="container" style="width: 100%">
        <table style="width: 100%">
            <tr>
                <td align="center" style="width: 98%; text-align: left">
                    <table cellpadding="0" cellspacing="0" style="width: 100%">
                        <tr>
                            <td class="" style="width: 100%">
                                <asp:Label ID="lblpagehdr" runat="server" Font-Bold="true" Visible="false"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 100%; height: 10px">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Panel ID="pnlSubEntityList" runat="server" Visible="false">
                                    <div style="width: 100%" align="left">
                                        <div align="center" style="padding-top: 5px; padding-bottom: 5px;">
                                            <asp:Button ID="btnTopUpdate" runat="server" Text="Update Order" CssClass="normalButtons"
                                                OnClick="UpdateDisplayOrder" />
                                        </div>
                                        <table width="98%" border="0" cellpadding="0" cellspacing="0" style="text-align: left">
                                            <tr align="left">
                                                <th align="left" class="style1">
                                                </th>
                                                <th align="left" class="style2">
                                                </th>
                                                <th align="left">
                                                </th>
                                            </tr>
                                            <asp:Repeater ID="subcategories" runat="server">
                                                <HeaderTemplate>
                                                    <tr class="table-header">
                                                        <th align="left">
                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.ID %>" />
                                                        </th>
                                                        <th align="left">
                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.Name %>" />
                                                        </th>
                                                        <th align="left">
                                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.DisplayOrder %>" />
                                                        </th>
                                                    </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr class="table-row2">
                                                        <td>
                                                            <%# ((System.Xml.XmlNode)Container.DataItem)["EntityID"].InnerText%>
                                                        </td>
                                                        <td>
                                                            <%# getLocaleValue(((System.Xml.XmlNode)Container.DataItem)["Name"], AspDotNetStorefrontCore.Localization.GetDefaultLocale())%>
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="DisplayOrder" runat="server" Columns="4" Text='<%# ((System.Xml.XmlNode)Container.DataItem)["DisplayOrder"].InnerText%>'></asp:TextBox>
                                                            <asp:TextBox ID="entityid" runat="server" Visible="false" Text='<%# ((System.Xml.XmlNode)Container.DataItem)["EntityID"].InnerText%>'></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                                <AlternatingItemTemplate>
                                                    <tr class="table-alternatingrow2">
                                                        <td>
                                                            <%# ((System.Xml.XmlNode)Container.DataItem)["EntityID"].InnerText%>
                                                        </td>
                                                        <td>
                                                            <%# getLocaleValue(((System.Xml.XmlNode)Container.DataItem)["Name"], "en-US") %>
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="DisplayOrder" runat="server" Columns="4" Text='<%# ((System.Xml.XmlNode)Container.DataItem)["DisplayOrder"].InnerText%>'></asp:TextBox>
                                                            <asp:TextBox ID="entityid" runat="server" Visible="false" Text='<%# ((System.Xml.XmlNode)Container.DataItem)["EntityID"].InnerText%>'></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                </AlternatingItemTemplate>
                                                <FooterTemplate>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                            <tr>
                                                <th align="left" class="style1">
                                                </th>
                                                <th align="left" class="style2">
                                                </th>
                                                <th align="left">
                                                </th>
                                            </tr>
                                        </table>
                                        <div align="center" style="padding-top: 5px; padding-bottom: 5px;">
                                            <asp:Button ID="btnBotUpdate" runat="server" Text="Update Order" class="normalButtons"
                                                OnClick="UpdateDisplayOrder" />
                                        </div>
                                    </div>
                                </asp:Panel>
                                <asp:Panel ID="pnlNoSubEntities" runat="server" Visible="true" HorizontalAlign="Center"
                                    Style="padding-top: 30px;">
                                    <asp:Label ID="lblError" runat="server" Font-Bold="true" Font-Names="verdana"></asp:Label>
                                </asp:Panel>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
