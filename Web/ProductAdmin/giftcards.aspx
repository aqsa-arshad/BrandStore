<%@ Page Language="C#" AutoEventWireup="true" CodeFile="giftcards.aspx.cs" Inherits="AspDotNetStorefrontAdmin.giftcards" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"  %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
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
                                <td class="titleTable" width="160">
                                    <font class="subTitle"><asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.GiftCardSearch %>" /></font>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #a2a2a2;" />
                                <td style="width: 5px;" />
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.GiftCards %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTablePRB" valign="top" width="160">
                                <asp:Panel ID="pnlsearch" DefaultButton="btnSearch" runat="server">
                                    <asp:TextBox ID="txtSearch" CssClass="singleAutoFull" runat="server"></asp:TextBox>
                                    <asp:DropDownList ID="ddSearch" CssClass="singleAutoFull" runat="server">
                                        <asp:ListItem Value="1" Text="<%$Tokens:StringResource, admin.giftcards.CustomerEmail %>"></asp:ListItem>
                                        <asp:ListItem Value="2" Text="<%$Tokens:StringResource, admin.giftcards.CustomerName %>"></asp:ListItem>
                                        <asp:ListItem Value="3" Selected="true" Text="<%$Tokens:StringResource, admin.giftcards.SerialNumber %>"></asp:ListItem>                                        
                                    </asp:DropDownList>
                                    <br />
                                    <asp:Button runat="server" ID="btnSearch" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Search %>" OnClick="btnSearch_Click" />
                                    </asp:Panel>
                                    <br /><br />
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Literal ID="Literal3" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.Types %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableNPLR">
                                                <div>
                                                    <asp:DropDownList CssClass="singleAutoFull" runat="server" ID="ddTypes" AutoPostBack="true" OnSelectedIndexChanged="ddTypes_SelectedIndexChanged">
                                                    </asp:DropDownList>                                                    
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Literal ID="Literal4" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.Status %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableNPLR">
                                                <div>
                                                    <asp:DropDownList CssClass="singleAutoFull" runat="server" ID="ddStatus" AutoPostBack="true" OnSelectedIndexChanged="ddStatus_SelectedIndexChanged">
                                                    </asp:DropDownList>                                                    
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <br />
                                    <div id="divForFilters" runat="server">
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Literal ID="Literal5" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.Product %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableNPLR">
                                                <div>
                                                    <asp:DropDownList CssClass="singleAutoFull" runat="server" ID="ddForProduct" AutoPostBack="true" OnSelectedIndexChanged="ddForProduct_SelectedIndexChanged">
                                                    </asp:DropDownList>                                                    
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Literal ID="Literal6" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.Manufacturer %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableNPLR">
                                                <div>
                                                    <asp:DropDownList CssClass="singleAutoFull" runat="server" ID="ddForManufacturer" AutoPostBack="true" OnSelectedIndexChanged="ddForManufacturer_SelectedIndexChanged">
                                                    </asp:DropDownList>                                                    
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Literal ID="Literal7" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.Category %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableNPLR">
                                                <div>
                                                    <asp:DropDownList CssClass="singleAutoFull" runat="server" ID="ddForCategory" AutoPostBack="true" OnSelectedIndexChanged="ddForCategory_SelectedIndexChanged">
                                                    </asp:DropDownList>                                                    
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Literal ID="Literal8" runat="server" Text="<%$Tokens:StringResource, admin.giftcard.Section %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableNPLR">
                                                <div>
                                                    <asp:DropDownList CssClass="singleAutoFull" runat="server" ID="ddForSection" AutoPostBack="true" OnSelectedIndexChanged="ddForSection_SelectedIndexChanged">
                                                    </asp:DropDownList>                                                    
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <br />
                                    </div>
                                    <asp:Button runat="server" ID="btnReset" CssClass="normalButtons" Text="Reset" OnClick="btnReset_Click" />
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #a2a2a2;" />
                                <td style="width: 5px;" />
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">
                                        <br />
                                        <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="ADD NEW" OnClick="btnInsert_Click" /><br />
                                        <br />
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="true" PageSize="25" AllowSorting="True" CssClass="tableoverallGrid" HorizontalAlign="Left" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" CellPadding="0" GridLines="None">
                                            <Columns>
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <span style='white-space: nowrap;'>
                                                            <a href='editgiftcard.aspx?iden=<%# Eval("GiftCardID") %>'>
                                                                <img src="../App_Themes/Admin_Default/images/edit.gif" alt="<%$Tokens:StringResource, admin.common.Edit %>" runat="server" />
                                                            </a>
                                                        </span>
						                            </ItemTemplate>
						                        </asp:TemplateField>
                                                <asp:BoundField DataField="GiftCardID" HeaderText="<%$Tokens:StringResource, admin.common.ID %>" ReadOnly="True" SortExpression="GiftCardID" >
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:BoundField>
						                        <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.SerialHeader %>" SortExpression="SerialNumber">
                                                    <ItemTemplate>
                                                        <a href='editgiftcard.aspx?iden=<%# Eval("GiftCardID") %>'>
                                                            <%# DataBinder.Eval(Container.DataItem, "SerialNumber")%>
                                                        </a>
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>						                        
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Type %>">
                                                    <ItemTemplate>
							                            <span style='white-space: nowrap;'>
							                                <asp:Literal ID="ltCardType" runat="server"></asp:Literal>
							                            </span>
						                            </ItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
						                        </asp:TemplateField>
						                        <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.CreatedOn %>" SortExpression="CreatedOn">
                                                    <ItemTemplate>
							                            <%# DataBinder.Eval(Container.DataItem, "CreatedOn")%>
						                            </ItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
						                        </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Customer %>" SortExpression="LastName">
                                                    <ItemTemplate>
                                                        <span style='white-space: nowrap;'>
                                                            <%# (DataBinder.Eval(Container.DataItem, "FirstName").ToString() + " " + DataBinder.Eval(Container.DataItem, "LastName").ToString()).Trim() %> 
                                                        </span>
						                            </ItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
						                        </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.OrderHeader %>" SortExpression="OrderNumber">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "OrderNumber")%>
						                            </ItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                    
						                        </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.InitialValue %>" SortExpression="InitialAmount">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="ltInitialAmount" runat="server"></asp:Literal>
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
                                                    
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.RemainingAmount %>" SortExpression="Balance">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="ltBalance" runat="server"></asp:Literal>
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
                                                   
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.ExpiresOn %>" SortExpression="ExpirationDate">
                                                    <ItemTemplate>
							                            <%# AspDotNetStorefrontCore.Localization.ToThreadCultureShortDateString(Convert.ToDateTime(DataBinder.Eval(Container.DataItem, "ExpirationDate")))%> 
							                            <asp:Literal ID="ltCardStatus" runat="server"></asp:Literal>
						                            </ItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
                                                    
						                        </asp:TemplateField>
						                        <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.UsageHistory %>">
                                                    <ItemTemplate>
                                                        <span style='white-space: nowrap;'>
                                                            <a href='giftcardusage.aspx?iden=<%# Eval("GiftCardID") %>'><asp:Literal Text="<%$Tokens:StringResource, admin.giftcards.Usage %>" runat="server" /></a>
                                                        </span>
						                            </ItemTemplate>
                                                    <ItemStyle HorizontalAlign="Center" />
                                                    <HeaderStyle HorizontalAlign="Center" />
						                        </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcards.Action %>">
                                                    <ItemTemplate>
                                                        <asp:LinkButton ID="lnkAction" CommandName="ItemAction" runat="Server"></asp:LinkButton>
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" HorizontalAlign="Center" />
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                </asp:TemplateField>
                                            </Columns>
                                            <PagerSettings FirstPageText="<%$Tokens:StringResource, admin.common.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.common.LastPage %>"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="tablefooterGrid" />
                                            <RowStyle CssClass="tableDataCellGrid" />
                                            <EditRowStyle CssClass="tableDataCellGridEdit" />
                                            <PagerStyle CssClass="tablepagerGrid" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="tableDataCellGridAlt" />
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
</asp:Content>