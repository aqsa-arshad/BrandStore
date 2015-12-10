<%@ Page ClientTarget="UpLevel" Language="c#" Inherits="AspDotNetStorefront.downloads" CodeFile="downloads.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="page-wrap downloads-page">
        <asp:Panel runat="server">

            <%--<h4>
                <asp:Literal ID="downloadaspx1" runat="server" Text="<%$ Tokens:StringResource,download.aspx.1 %>" /></h4>
            <div class="downloads-info">
                <aspdnsf:Topic runat="server" ID="HeaderMsg" TopicName="Download.Information" />
            </div>--%>
            <div class="error">
                <asp:Literal ID="ltError" runat="server" />
            </div>
            <div class="downloads-wrap">
                <ul id="pagetabs" class="tabbitTabs">
                    <li>
                        <a href="#available" href="javascript:TabbedUI.prototype.showTab('overview');">
                            <asp:Literal ID="downloadaspx2" runat="server" Text="<%$ Tokens:StringResource,download.aspx.2 %>" />
                        </a>
                    </li>
                    <li>
                        <a href="#pending" href="javascript:TabbedUI.prototype.showTab('pending');">
                            <asp:Literal ID="downloadaspx3" runat="server" Text="<%$ Tokens:StringResource,download.aspx.3 %>" />
                        </a>
                    </li>
                    <li>
                        <a href="#expired" href="javascript:TabbedUI.prototype.showTab('expired');">
                            <asp:Literal ID="downloadaspx4" runat="server" Text="<%$ Tokens:StringResource,download.aspx.4 %>" />
                        </a>
                    </li>
                </ul>
                <div id="tabcontent" class="tabbitTabWrap">
                    <div id="available">
                        <div class="tabContentItem">
                            <asp:GridView ID="gvDownloadsAvailable"
                                EmptyDataText="<%$ Tokens:StringResource,download.aspx.11 %>"
                                OnRowCommand="gvDownloadsAvailable_RowCommand"
                                AutoGenerateColumns="False"
                                Width="100%"
                                runat="server"
                                CssClass="table table-striped download-table"
                                HeaderStyle-CssClass="table-header"
                                RowStyle-CssClass="table-row"
                                GridLines="None">
                                <Columns>
                                    <asp:BoundField DataField="ShoppingCartRecordId" HeaderText="<%$ Tokens:StringResource,download.aspx.5 %>"
                                        ReadOnly="true" />
                                    <asp:BoundField DataField="DownloadName" HeaderText="<%$ Tokens:StringResource,download.aspx.6 %>"
                                        ReadOnly="True" />
                                    <asp:BoundField DataField="DownloadCategory" HeaderText="<%$ Tokens:StringResource,download.aspx.7 %>"
                                        ReadOnly="True" />
                                    <asp:TemplateField HeaderText="<%$ Tokens:StringResource,download.aspx.8 %>">
                                        <ItemTemplate>
                                            <%# ((DateTime)Eval("ExpiresOn") > DateTime.MinValue ? Eval("ExpiresOn") : AppLogic.GetString("download.aspx.16", ThisCustomer.LocaleSetting)) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:Button ID="btnDownload" CssClass="button download-button call-to-action" CommandName="download" CommandArgument='<%# Eval("ShoppingCartRecordId") %>' runat="Server" Text="<%$ Tokens:StringResource,download.aspx.14 %>" Visible='<%# AspDotNetStorefrontCore.AppLogic.AppConfigBool("Download.StreamFile") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:HyperLink ID="lnkDownload" runat="server" NavigateUrl='<%# Eval("DownloadLocation") %>' Text="<%$ Tokens:StringResource,download.aspx.14 %>" Visible='<%# !AspDotNetStorefrontCore.AppLogic.AppConfigBool("Download.StreamFile") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                    <div id="pending">
                        <div class="tabContentItem">
                            <asp:GridView ID="gvDownloadsPending"
                                AutoGenerateColumns="False"
                                EmptyDataText="<%$ Tokens:StringResource,download.aspx.12 %>"
                                Width="100%"
                                runat="server"
                                CssClass="table table-striped download-table"
                                HeaderStyle-CssClass="table-header"
                                RowStyle-CssClass="table-row"
                                GridLines="None"
                                OnRowDataBound="gvDownloadsPending_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="ShoppingCartRecordId" HeaderText="<%$ Tokens:StringResource,download.aspx.5 %>"
                                        ReadOnly="true" />
                                    <asp:BoundField DataField="DownloadName" HeaderText="<%$ Tokens:StringResource,download.aspx.6 %>"
                                        ReadOnly="True" />
                                    <asp:BoundField DataField="DownloadCategory" HeaderText="<%$ Tokens:StringResource,download.aspx.7 %>"
                                        ReadOnly="True" />
                                    <asp:BoundField DataField="PurchasedOn" HeaderText="<%$ Tokens:StringResource,download.aspx.9 %>"
                                        ReadOnly="True" />                                  
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:HyperLink ID="hlDownloadLink" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hfDownloadLocation" runat="server" Value='<%# Eval("DownloadLocation") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                    <div id="expired">
                        <div class="tabContentItem">
                            <asp:GridView ID="gvDownloadsExpired"
                                AutoGenerateColumns="False"
                                EmptyDataText="<%$ Tokens:StringResource,download.aspx.13 %>"
                                Width="100%"
                                runat="server"
                                CssClass="table table-striped download-table"
                                HeaderStyle-CssClass="table-header"
                                RowStyle-CssClass="table-row"
                                GridLines="None">
                                <Columns>
                                    <asp:BoundField DataField="ShoppingCartRecordId" HeaderText="<%$ Tokens:StringResource,download.aspx.5 %>"
                                        ReadOnly="true" />
                                    <asp:BoundField DataField="DownloadName" HeaderText="<%$ Tokens:StringResource,download.aspx.6 %>"
                                        ReadOnly="True" />
                                    <asp:BoundField DataField="DownloadCategory" HeaderText="<%$ Tokens:StringResource,download.aspx.7 %>"
                                        ReadOnly="True" />
                                    <asp:BoundField DataField="ExpiresOn" HeaderText="<%$ Tokens:StringResource,download.aspx.10 %>"
                                        ReadOnly="True" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
                <script type="text/javascript" src="jscripts/tabbit.js"></script>
            </div>

            <asp:Image ID="imgRelatedProductsTab" runat="server" />
            <div class="related-product-wrap">
                <asp:Repeater ID="rptRelatedProducts" runat="server">
                    <ItemTemplate>
                        <div class="related-product">
                            <div class="product-image">
                                <asp:Image ID="imgProduct" runat="server" ImageUrl='<%# AppLogic.LookupImage("Product", Container.DataItemAs<Product>().ProductID, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)  %>' />
                            </div>
                            <div class="product-name">
                                <asp:HyperLink ID="lnkProduct" runat="server" Text='<%# Container.DataItemAs<Product>().Name %>' NavigateUrl='<%# SE.MakeProductLink(Container.DataItemAs<Product>().ProductID, Container.DataItemAs<Product>().SEName) %>' />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <div class="clearfix"></div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>

