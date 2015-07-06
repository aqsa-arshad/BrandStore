<%@ Page Language="C#" AutoEventWireup="true" CodeFile="kitgroupimageupload.aspx.cs"
    Inherits="AspDotNetStorefrontAdmin.kitgroupimageupload" MaintainScrollPositionOnPostback="true" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<head runat="server">
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
        .upload_label
        {
            padding-left: 5px;
            border-right: solid 1px #ccc;
            width: 20%;
            padding-bottom: 5px;
        }
        .upload_image
        {
            padding-left: 10px;
            width: 80%;
            padding-top: 5px;
            padding-bottom: 5px;
        }
        .upload_group_table
        {
            width: 800px;
            border: solid 1px #ccc;
        }
        .upload_kitGroup_header
        {
            padding-left: 5px;
            padding-top: 5px;
            padding-bottom: 5px;
            background-color: #eee;
            font-weight: bold;
            font-size: 12px;
            border-bottom: solid 1px #ccc;
        }
        .upload_kitItem_header
        {
            padding-left: 5px;
            padding-top: 5px;
            padding-bottom: 5px;
            background-color: #eee;
            border-top: solid 1px #ccc;
            border-bottom: solid 1px #ccc;
            font-weight: bold;
            font-size: 12px;
        }
        .upload_kitItems
        {
            width: 100%;
            border: dashed 1px #ccc;
            padding-left: 10px;
            padding-top: 5px;
        }
    </style>
</head>
<body>
    <form runat="server">
    <div class="pnlMain" align="left">
        <table class="upload_group_table" border="0" cellpadding="0" cellspacing="0">
            <tr>
                <td colspan="2" class="upload_kitGroup_header">
                    <%= KitGroup.Name %>
                </td>
            </tr>
            <tr>
                <td class="upload_label" align="left" valign="top">
                    <br />
                </td>
                <td class="upload_image" align="left" valign="top">
                    <asp:FileUpload ID="flpGroup" runat="server" />
                    <br />
                    <asp:Panel ID="pnlGroupImage" runat="server" Visible="false">
                        <asp:LinkButton ID="lnkDeleteGroupImage" runat="server" OnClick="lnkDeleteGroupImage_Click"
                            Text="<%$Tokens:StringResource, admin.gallery.DeleteImage %>"></asp:LinkButton>
                        <br />
                        <asp:Image ID="imgGroupImage" runat="server" />
                    </asp:Panel>
                    <br />
                </td>
            </tr>
            <asp:Repeater ID="rptItemImages" runat="server" OnItemCommand="rptItemImages_ItemCommand">
                <HeaderTemplate>
                    <tr>
                        <td colspan="2" class="upload_kitItem_header">
                            Kit Items: (<%= KitGroup.Items.Count %>)
                        </td>
                    </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="upload_label" align="left" valign="top" style="padding-top: 10px;">
                            <asp:Literal ID="Literal1" runat="server" Text='<%# Localize(Container.DataItemAs<KitItemData>().Name) %>'></asp:Literal>
                        </td>
                        <td class="upload_image" align="left" valign="top" style="padding-top: 10px;">
                            <asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
                            <asp:FileUpload ID="flpKitItem" runat="server" />
                            <br />
                            <asp:Panel ID="pnlItemImage" runat="server" Visible='<%# Container.DataItemAs<KitItemData>().HasImage %>'>
                                <asp:LinkButton ID="lnkDeleteItemImage" runat="server" CommandName="DeleteKitItemImage"
                                    CommandArgument='<%# Container.DataItemAs<KitItemData>().Id %>'>Delete image</asp:LinkButton>
                                <br />
                                <asp:Image ID="imgGroupImage" runat="server" ImageUrl='<%# Container.DataItemAs<KitItemData>().ImagePath %>' />
                            </asp:Panel>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
            <tr>
                <td class="upload_label" style="border-top: solid 1px #ccc; border-right: none;">
                    <br />
                </td>
                <td class="upload_image" style="border-top: solid 1px #ccc;">
                    <asp:Button ID="btnUploadBottom" runat="server" Text="<%$Tokens:StringResource, admin.common.Upload %>"
                        OnClick="btnUpload_Click" />
                    &nbsp;&nbsp; <a href="javascript:self.close();">
                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Close %>" /></a>
                </td>
                <tr>
        </table>
        <br />
        <br />
        <br />
    </div>
    </form>
</body>
