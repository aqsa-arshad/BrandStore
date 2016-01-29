<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/App_Templates/Skin_3/JeldWenTemplate.master" CodeFile="MyDownloads.aspx.cs" Inherits="AspDotNetStorefront.MyDownloads" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="content-box-03 padding-top-none">
        <div>
            <table id="tblDownload" class="table margin-none bottom-space-border">
                <tbody>
                    <tr id="trnoDownloadProductsFound" runat="server">
                        <td>
                            <asp:Label ID="noDownloadProductsFound" runat="server" Text="<%$ Tokens:StringResource,admin.common.NoDownloadProductsFound %>"></asp:Label>
                        </td>
                    </tr>
                    <asp:Repeater ID="rptDownloadableItems" runat="server" OnItemDataBound="rptAddresses_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <td class="td-55-percent">
                                    <div class="row">
                                        <div class="col-md-5">
                                            <div class="primary-img-box">
                                                <asp:Image ID="Image1" runat="server" class="img-responsive"
                                                    ImageUrl='<%# AspDotNetStorefrontCore.AppLogic.LookupImage("Product", int.Parse(Eval("ProductID").ToString()), Eval("ImageFileNameOverride").ToString(), Eval("SKU").ToString(), "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)%>' />
                                            </div>
                                        </div>
                                        <div class="col-md-5">
                                            <asp:HiddenField ID="hfSKU" runat="server" Value='<%# Eval("SKU") %>' />
                                            <asp:HiddenField ID="hfDownloadLocation" runat="server" Value='<%# Eval("DownloadLocation") %>' />
                                            <span class="normal-heading black-color">
                                                <asp:Label ID="lblProductName" runat="server" Text='<%# Eval("Name") %>'></asp:Label>
                                            </span>
                                            <span>
                                                <asp:Label ID="lblProductSKU" runat="server"></asp:Label>
                                            </span>
                                            <div class="shopping-cart-fix">
                                                <asp:Label ID="lblDescription" runat="server" Text='<%# Eval("Description") %>'></asp:Label>
                                            </div>
                                        </div>
                                    </div>
                                </td>
                                <td class="td-30-percent">
                                    <span class="normal-heading black-color">Categories</span>
                                    <span>
                                        <asp:Label ID="Label1" runat="server" Text='<%# Eval("DownloadCategory") %>'></asp:Label>
                                    </span>
                                </td>
                                <td class="td-25-percent">
                                    <span class="normal-heading black-color">&nbsp;</span>
                                    <asp:HyperLink class="underline-link" ID="hlDownload" runat="server"></asp:HyperLink>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate></FooterTemplate>
                    </asp:Repeater>
                </tbody>
            </table>

        </div>
    </div>
</asp:Content>
