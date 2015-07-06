<%@ Page Language="C#" AutoEventWireup="true" CodeFile="affiliates.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Affiliates" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<asp:Literal ID="ltScript1" runat="server"></asp:Literal>    
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltNotice" runat="server"></asp:Literal>
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable" width="130">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.affiliates.AffiliateSearch %>" /></font>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #a2a2a2;" />
                                <td style="width: 5px;" />
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.affiliates.Affiliates %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTableNP" valign="top" width="130">
                                    <asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnSearch">
                                    <asp:TextBox ID="txtSearch" Width="130" runat="server"></asp:TextBox>
                                    <asp:Button runat="server" ID="btnSearch" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Search %>" OnClick="btnSearch_Click" />
                                    </asp:Panel>
                                    <br /><br />
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.stringresources.Index %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableAPL">
                                                <div>
                                                    <asp:TreeView ID="treeMain" runat="server" OnSelectedNodeChanged="treeMain_SelectedNodeChanged">
                                                    </asp:TreeView>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #a2a2a2;" />
                                <td style="width: 5px;" />
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">
                                        <div class="wrapperTop">
                                            <br />
    					                    <asp:Button runat="server" ID="btnAdd" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.topic.addnew %>" OnClick="btnAdd_Click" /><br />
                                            <br />
                                            &nbsp;</div>
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" 
                                        AutoGenerateColumns="False" AllowPaging="true" PageSize="15" AllowSorting="True" HorizontalAlign="Left" OnRowCommand="gMain_RowCommand" 
                                        OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" BorderWidth="0px" 
                                        CellPadding="0" BorderStyle="None" GridLines="None">
                                            <Columns>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Edit %>">
                                                    <ItemTemplate>
                                                        <a href='editAffiliates.aspx?iden=<%# DataBinder.Eval(Container.DataItem, "AffiliateID")%>'>
                                                            <asp:Image ID="imgEdit" runat="Server" AlternateText="<%$Tokens:StringResource, admin.common.Delete %>" ImageUrl="~/App_Themes/Admin_Default/images/edit.gif" />
                                                        </a>                                                        
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:BoundField DataField="AffiliateID" HeaderText="<%$Tokens:StringResource, admin.common.ID %>" ReadOnly="True" SortExpression="AffiliateID" >
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Name %>" SortExpression="Name">
                                                    <ItemTemplate>
                                                        <a href='editAffiliates.aspx?iden=<%# DataBinder.Eval(Container.DataItem, "AffiliateID")%>'>
                                                            <%# DataBinder.Eval(Container.DataItem, "Name")%>
                                                        </a>
                                                        <asp:Literal id="ltName" runat="server"></asp:Literal>
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.E-Mail %>" SortExpression="EMail">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="ltEMail" runat="Server"></asp:Literal>
						                            </ItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:TemplateField>                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Address %>">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="ltShipTo" runat="server"></asp:Literal>
							                        </ItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
                                                </asp:TemplateField>                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.URL %>">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "URL")%>
							                        </ItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
						                        </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("AffiliateID") %>' runat="Server" AlternateText="<%$Tokens:StringResource, admin.common.Delete %>" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" />
                                                </asp:TemplateField>
                                            </Columns>
                                            <PagerSettings FirstPageText="&amp;lt;&amp;lt;First Page" LastPageText="Last Page&amp;gt;&amp;gt;"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="footerGrid" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="DataCellGridEdit" />
                                            <PagerStyle CssClass="pagerGrid" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                        </asp:GridView>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <asp:Literal ID="ltScript" runat="server"></asp:Literal>
</asp:Content>