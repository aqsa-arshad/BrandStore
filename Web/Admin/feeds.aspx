<%@ Page Language="C#" AutoEventWireup="true" CodeFile="feeds.aspx.cs" Inherits="AspDotNetStorefrontAdmin.feeds" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<script type="text/javascript" >
     function EditFeed(feedid) {
         document.location.href = "editfeed.aspx?feedid=" + feedid;
     }
</script>

        <div id="Div1"><b><asp:Label runat="server" text="<%$Tokens:StringResource, admin.feeds.header %>" /></b><br /><br />
         <asp:Label ID="lblError" runat="server" ForeColor="red" Visible="false"></asp:Label>
        </div>
        <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"> <asp:Label ID="Label1" runat="server" text="<%$Tokens:StringResource, admin.feeds.ProductFeeds %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapperLeft">
                                        <asp:Panel ID="pnlFeeds" runat="server" DefaultButton="btnAddFeed1">
                                            <asp:Button ID="btnAddFeed1" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.feeds.AddnewFeed %>" OnClientClick="document.location.href='editfeed.aspx'; return false;" />
                                            <br /><br />
                            
                                            <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                                <asp:Repeater ID="rptrFeeds" runat="server" OnItemCommand="rptrFeeds_ItemCommand" OnItemDataBound="rptrFeeds_ItemDataBound">
                                                    <HeaderTemplate>
                                                        <tr class="tablenormal">
                                                            <th width="5%" align="left" valign="middle">ID</th>
                                                            <th align="left" valign="middle"><asp:Label ID="Label1" runat="server" text="<%$Tokens:StringResource, admin.feeds.FeedName %>" /></th>
                                                            <th align="left" valign="middle"><asp:Label ID="Label2" runat="server" text="<%$Tokens:StringResource, admin.feeds.XMLPackage %>" /></th>
                                                            <th align="left" valign="middle"><asp:Label ID="Label3" runat="server" text="<%$Tokens:StringResource, admin.feeds.Storename%>" /></th>
                                                            <th width="10%" align="left" valign="middle"><asp:Label ID="Label4" runat="server" text="<%$Tokens:StringResource, admin.feeds.Edit %>" /></th>
                                                            <th width="10%" align="left" valign="middle"><asp:Label ID="Label5" runat="server" text="<%$Tokens:StringResource, admin.feeds.Execute %>" /></th>
                                                            <th width="10%" align="left" valign="middle"><asp:Label ID="Label6" runat="server" text="<%$Tokens:StringResource, admin.feeds.Delete %>" /></th>
                                                        </tr>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <tr class="tabletdnormal">
                                                            <td><%# DataBinder.Eval(Container.DataItem, "FeedID") %></td>
                                                            <td><asp:HyperLink ID="lnkFeedEdit" runat="server" NavigateUrl='<%# "editfeed.aspx?feedid=" + DataBinder.Eval(Container.DataItem, "FeedID").ToString()%>' Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:HyperLink></td>
                                                            <td><%# DataBinder.Eval(Container.DataItem, "XmlPackage") %></td>
                                                            <td><%# DataBinder.Eval(Container.DataItem, "StoreName") %></td>
                                                            <td align="center"><asp:Button ID="btnEditFeed" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.feeds.EditFeed %>" OnClientClick='<%# "EditFeed(" + DataBinder.Eval(Container.DataItem, "FeedID").ToString() + "); return false;"%>' /></td>
                                                            <td align="center"><asp:Button ID="btnExecuteFeed" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.feeds.ExecuteFeed %>" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FeedID") + ":" + DataBinder.Eval(Container.DataItem, "StoreID") %>' CommandName='execute' /></td>
                                                            <td align="center"><asp:Button ID="btnDeleteFeed" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.feeds.DeleteFeed %>" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FeedID") %>' CommandName='delete' /></td>
                                                        </tr>
                                                    </ItemTemplate>
                                                    <AlternatingItemTemplate>
                                                        <tr class="tabletdalternormal">
                                                            <td><%# DataBinder.Eval(Container.DataItem, "FeedID") %></td>
                                                            <td><asp:HyperLink ID="lnkFeedEdit" runat="server" NavigateUrl='<%# "editfeed.aspx?feedid=" + DataBinder.Eval(Container.DataItem, "FeedID").ToString()%>' Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:HyperLink></td>
                                                            <td><%# DataBinder.Eval(Container.DataItem, "XmlPackage") %></td>
                                                            <td><%# DataBinder.Eval(Container.DataItem, "StoreName") %></td>
                                                            <td align="center"><asp:Button ID="btnEditFeed" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.feeds.EditFeed %>" OnClientClick='<%# "EditFeed(" + DataBinder.Eval(Container.DataItem, "FeedID").ToString() + "); return false;"%>' /></td>
                                                            <td align="center"><asp:Button ID="btnExecuteFeed" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.feeds.ExecuteFeed %>" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FeedID") %>' CommandName='execute' /></td>
                                                            <td align="center"><asp:Button ID="btnDeleteFeed" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.feeds.DeleteFeed %>" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FeedID") %>' CommandName='delete' /></td>
                                                        </tr>
                                                    </AlternatingItemTemplate>
                                                    <FooterTemplate></FooterTemplate>
                                                </asp:Repeater>
                                            </table>
                                        </asp:Panel>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    
</asp:Content>
