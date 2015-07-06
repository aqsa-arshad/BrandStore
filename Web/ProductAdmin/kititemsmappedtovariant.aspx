<%@ Page Language="C#" AutoEventWireup="true" CodeFile="kititemsmappedtovariant.aspx.cs"
    Inherits="AspDotNetStorefrontAdmin.kititemsmappedtovariant" MaintainScrollPositionOnPostback="true" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<html>
<head id="Head1" runat="server">
    <style type="text/css">
        body
        {
            font-family: Verdana,Geneva,Arial,Helvetica,sans-serif;
            font-size: 11px;
        }
        table
        {
            font-family: Verdana,Geneva,Arial,Helvetica,sans-serif;
            font-size: 11px;
        }
        .pnlMain
        {
            margin-left: 10px;
            margin-top: 10px;
        }
        .tblVariantMap
        {
            width: 600px;
            border: solid 1px #ccc;
        }
        .tdLevel1
        {
            width: 10px;
        }
        .tdLevel2
        {
            width: 10px;
        }
    </style>
</head>
<body>
    <form runat="server">
    <div class="pnlMain">
        <table class="tblVariantMap" border="0" cellspacing="0" cellpadding="0">
            <asp:Repeater ID="dlMappedKits" runat="server">
                <HeaderTemplate>
                    <tr>
                        <td colspan="6" style="background-color: #ccc;">
                            Product
                        </td>
                    </tr>
                    <tr>
                        <%--spacer--%>
                        <td class="tdLevel1" style="background-color: #ccc;">
                            <br />
                        </td>
                        <td colspan="5" style="background-color: #ccc;">
                            Group
                        </td>
                    </tr>
                    <tr>
                        <%--spacer--%>
                        <td class="tdLevel1" style="background-color: #ccc; border-bottom: solid 2px #aaa;">
                            <br />
                        </td>
                        <td class="tdLevel2" style="background-color: #ccc; border-bottom: solid 2px #aaa;">
                            <br />
                        </td>
                        <td style="background-color: #ccc; border-bottom: solid 2px #aaa;">
                            Item
                        </td>
                        <td style="background-color: #ccc; border-bottom: solid 2px #aaa;">
                            Description
                        </td>
                        <td style="background-color: #ccc; border-bottom: solid 2px #aaa;">
                            Price Delta
                        </td>
                        <td style="background-color: #ccc; border-bottom: solid 2px #aaa;">
                            Weight Delta
                        </td>
                    </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td colspan="6">
                            <asp:HiddenField ID="hdfKitProductId" runat="server" Value='<%# Container.DataItemAs<KitProductData>().Id %>' />
                            <asp:Literal ID="Literal1" runat="server" Text='<%# Container.DataItemAs<KitProductData>().Name %>'></asp:Literal>
                        </td>
                    </tr>
                    <asp:Repeater ID="rptKitGroups" runat="server" DataSource='<%# Container.DataItemAs<KitProductData>().GetGroupsWithItemsMappedToVariant(this.VariantId) %>'>
                        <ItemTemplate>
                            <tr>
                                <%--spacer--%>
                                <td class="tdLevel1">
                                    <br />
                                </td>
                                <td colspan="5">
                                    <asp:HiddenField ID="hdfGroupId" runat="server" Value='<%# Container.DataItemAs<KitGroupData>().Id %>' />
                                    <asp:Literal ID="Literal1" runat="server" Text='<%# Container.DataItemAs<KitGroupData>().Name %>'></asp:Literal>
                                </td>
                            </tr>
                            <asp:Repeater ID="rptKitItems" runat="server" DataSource='<%# Container.DataItemAs<KitGroupData>().GetItemsMappedToVariant(this.VariantId) %>'>
                                <ItemTemplate>
                                    <tr>
                                        <%--spacer--%>
                                        <td class="tdLevel1">
                                            <br />
                                        </td>
                                        <td class="tdLevel2">
                                            <asp:HiddenField ID="hdfKitItemtId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
                                            <asp:CheckBox ID="chkUpdate" runat="server" Checked="true" />
                                        </td>
                                        <td>
                                            <asp:Literal ID="Literal1" runat="server" Text='<%# Container.DataItemAs<KitItemData>().Name %>'></asp:Literal>
                                        </td>
                                        <td>
                                            <asp:Literal ID="Literal2" runat="server" Text='<%# Container.DataItemAs<KitItemData>().Description %>'></asp:Literal>
                                        </td>
                                        <td>
                                            <asp:Literal ID="Literal3" runat="server" Text='<%# FormatCurrencyDisplay(Container.DataItemAs<KitItemData>().PriceDelta) %>'></asp:Literal>
                                        </td>
                                        <td>
                                            <asp:Literal ID="Literal4" runat="server" Text='<%# FormatWeightDisplay(Container.DataItemAs<KitItemData>().WeightDelta) %>'></asp:Literal>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ItemTemplate>
                    </asp:Repeater>
                </ItemTemplate>
            </asp:Repeater>
        </table>
        <br />
        <asp:LinkButton ID="btnUpdateAll" runat="server" Text="<%$Tokens:StringResource, admin.kititemsmappedtovariant.UpdateAll %>"
            OnClick="btnUpdateAll_Click"></asp:LinkButton>
        <br />
        <asp:LinkButton ID="btnUpdateNameAndDescription" runat="server" Text="<%$Tokens:StringResource, admin.kititemsmappedtovariant.UpdateNameDescription %>"
            OnClick="btnUpdateNameAndDescription_Click"></asp:LinkButton>
        <br />
        <br />
    </div>
    </form>
</body>
</html>


