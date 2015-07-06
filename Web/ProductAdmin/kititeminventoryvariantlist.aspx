<%@ Page Language="C#" AutoEventWireup="true" CodeFile="kititeminventoryvariantlist.aspx.cs"
    Inherits="AspDotNetStorefrontAdmin.Admin_kititeminventoryvariantlist" MaintainScrollPositionOnPostback="true" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<head runat="server">

    <script type="text/javascript" src="../jscripts/core.js"></script>
    
    <script type="text/javascript">
        function CloseAndUpdate(data) {
            
            GetRadWindow().Close();
            window.parent.aspdnsf.Pages.EditKit.pushData(data);
        }
        
        function GetRadWindow() {
            var oWindow = null;
            if (window.radWindow) oWindow = window.radWindow; //Will work in Moz in all cases, including clasic dialog
            else if (window.frameElement.radWindow) oWindow = window.frameElement.radWindow; //IE (and Moz as well)

            return oWindow;
        }
    </script>

    <style type="text/css">
        body
        {
            font-family: Verdana,Geneva,Arial,Helvetica,sans-serif;
            font-size: 11px;
            background-color: #fff;
        }
        table
        {
            font-family: Verdana,Geneva,Arial,Helvetica,sans-serif;
            font-size: 11px;
        }
        .product_list_table
        {
            width: 800px;
            border-right: solid 1px #ccc;
            border-left: solid 1px #ccc;
            border-bottom: solid 1px #ccc;
            border-spacing: 0;
            padding: 0;
            margin-left: 2px;
        }
        a:link
        {
            color: #068FE0;
            text-decoration: none;
        }
        .product_column_header td
        {
            background: #D3DBE9 url('images/grid-header1.gif') repeat-x scroll 0 -200px;
            border-bottom: 1px solid #9EB6CE;
            border-left: 1px solid #9EB6CE;
            border-top: 1px solid #9EB6CE;
            font-size: 12px;
            font-weight: normal;
            padding-bottom: 2px;
            padding-top: 3px;
            padding-left: 5px;
            text-align: left;
        }
        .variant_column_header td
        {
            background: #D3DBE9 url('images/grid-header1.gif') repeat-x scroll 0 -200px;
            border-bottom: 1px solid #9EB6CE;
            border-left: 1px solid #9EB6CE;
            border-top: 1px solid #9EB6CE;
            font-size: 12px;
            font-weight: normal;
            padding-bottom: 2px;
            padding-top: 3px;
            padding-left: 5px;
            text-align: left;
        }
        .product_line_item td
        {
            background-color: #eee;
            width: 55px;
        }
        .alpha_filters
        {
            width: 55px;
        }
        .alpha_filters_column
        {
            font-family: Consolas;
            background-color: #2F577A;
            width: 55px;
            text-align: center;
            vertical-align: top;
            padding-top: 10px;
            padding-left: 5px;
            padding-right: 5px;
            padding-bottom: 10px;
        }
        .alpha_filters_column a, .alpha_filters_column span
        {
            color: #fff;
        }
    </style>
</head>
<body>
    <form runat="server">
    <asp:ScriptManager runat="server" ID="ScriptManager1" />
    <div>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch" OnFilter="ctrlSearch_Filter"
                    OnContentCreated="ctrlSearch_ContentCreated" AlphaGrouping="2">
                    <Search SearchButtonCaption="Go" SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>"
                        SearchTextMinLength="<%$ Tokens:AppConfigUSInt, MinSearchStringLength %>" SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>"
                        ValidateInputLength="false" ShowValidationMessageBox="false" ShowValidationSummary="false"
                        UseLandingPageBehavior="false" />
                    <ContentTemplate>
                        <table class="product_list_table">
                            <asp:Repeater ID="rptProducts" runat="server">
                                <HeaderTemplate>
                                    <tr class="product_column_header">
                                        <td align="center" valign="middle" style="width: 80px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.ProductID %>" />
                                        </td>
                                        <td valign="middle" style="width: 590px;" colspan="6">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Name %>" />
                                        </td>
                                        <td align="center" valign="middle" style="width: 50px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Published %>" />
                                        </td>
                                        <td align="center" valign="middle" style="width: 50px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Deleted %>" />
                                        </td>
                                    </tr>
                                    <tr class="variant_column_header">
                                        <%-- empty line spacer --%>
                                        <td align="center" valign="middle" style="width: 80px;">
                                            <br />
                                        </td>
                                        <td align="center" valign="middle" style="width: 80px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.VariantID %>" />
                                        </td>
                                        <td valign="middle" style="width: 300px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Name %>" />
                                        </td>
                                        <td align="center" valign="middle" style="width: 50px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Inventory %>" />
                                        </td>
                                        <td align="center" valign="middle" style="width: 80px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Price %>" />
                                        </td>
                                        <td align="center" valign="middle" style="width: 80px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.SalePrice %>" />
                                        </td>
                                        <td align="center" valign="middle" style="width: 80px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Weight %>" />
                                        </td>
                                        <td align="center" valign="middle" style="width: 50px;">
                                            <br />
                                        </td>
                                        <%-- Published --%>
                                        <td align="center" valign="middle" style="width: 50px;">
                                            <br />
                                        </td>
                                        <%-- Deleted --%>
                                    </tr>
                                    <%--No products found--%>
                                    <tr runat="server" id="trNoResult" visible="false">
                                        <td colspan="6">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.NoProductsFound %>" />
                                        </td>
                                    </tr>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr class="product_line_item">
                                        <td align="center" valign="middle">
                                            <asp:Label ID="lblProuductId" runat="server" Text='<%# Container.DataItemAs<ProductForKit>().Id %>'>
                                            </asp:Label>
                                        </td>
                                        <td colspan="6">
                                            <asp:Label ID="lblProductName" runat="server" Text='<%# Container.DataItemAs<ProductForKit>().Name %>'>
                                            </asp:Label>
                                        </td>
                                        <td align="center" valign="middle">
                                            <asp:CheckBox ID="chkProductIsPublised" runat="server" Enabled="false" Checked='<%# Container.DataItemAs<ProductForKit>().IsPublished %>' />
                                        </td>
                                        <td align="center" valign="middle">
                                            <asp:CheckBox ID="chkProductIsDeleted" runat="server" Enabled="false" Checked='<%# Container.DataItemAs<ProductForKit>().IsDeleted %>' />
                                        </td>
                                    </tr>
                                    <asp:Repeater ID="rptVariants" runat="server" DataSource='<%# Container.DataItemAs<ProductForKit>().Variants %>'
                                        OnItemDataBound="rptVariants_ItemDataBound">
                                        <ItemTemplate>
                                            <tr>
                                                <%-- empty line spacer --%>
                                                <td>
                                                    <br />
                                                </td>
                                                <td align="center" valign="middle">
                                                    <asp:Label ID="Label1" runat="server" Text='<%# Container.DataItemAs<ProductVariantForKit>().Id %>'>
                                                    </asp:Label>
                                                </td>
                                                <td>
                                                    <asp:HyperLink ID="lnkName" runat="server" Text='<%# Container.DataItemAs<ProductVariantForKit>().Name %>'></asp:HyperLink>
                                                </td>
                                                <td align="center" valign="middle">
                                                    <asp:Label ID="Label2" runat="server" Text='<%# Container.DataItemAs<ProductVariantForKit>().InventoryCount %>'>
                                                    </asp:Label>
                                                </td>
                                                <td align="center" valign="middle">
                                                    <asp:Label ID="Label3" runat="server" Text='<%# FormatNumericDisplay(Container.DataItemAs<ProductVariantForKit>().Price) %>'>
                                                    </asp:Label>
                                                </td>
                                                <td align="center" valign="middle">
                                                    <asp:Label ID="Label4" runat="server" Text='<%# FormatNumericDisplay(Container.DataItemAs<ProductVariantForKit>().SalePrice) %>'>
                                                    </asp:Label>
                                                </td>
                                                <td align="center" valign="middle">
                                                    <asp:Label ID="Label5" runat="server" Text='<%# FormatNumericDisplay(Container.DataItemAs<ProductVariantForKit>().Weight) %>'>
                                                    </asp:Label>
                                                </td>
                                                <td align="center" valign="middle">
                                                    <asp:CheckBox ID="chkVariantIsPublised" runat="server" Enabled="false" Checked='<%# Container.DataItemAs<ProductVariantForKit>().IsPublished %>' />
                                                </td>
                                                <td align="center" valign="middle">
                                                    <asp:CheckBox ID="chkVariantIsDeleted" runat="server" Enabled="false" Checked='<%# Container.DataItemAs<ProductVariantForKit>().IsDeleted %>' />
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ItemTemplate>
                            </asp:Repeater>
                        </table>
                    </ContentTemplate>
                </aspdnsf:SearcheableTemplate>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    </form>
</body>
