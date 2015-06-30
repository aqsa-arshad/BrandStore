<%@ Page Language="C#" AutoEventWireup="true" CodeFile="producttypes.aspx.cs" Inherits="AspDotNetStorefrontAdmin.producttypes" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<div class="breadCrumb3">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal></div>
    <div id="">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <div class="breadCrumb3">
            <asp:Literal ID="ltError" runat="server"></asp:Literal></div>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0"  width="100%">
            <tr>
                <td align="center" valign="middle" style="text-align:left; padding-left: 5px">
                    <div>                       
                        <table border="0" cellpadding="0" cellspacing="0" width="98%" class="divBox">
                            <tr>
                                <td class="titleTable">
                                    <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.title.producttypes %>" />
                                </td>
                            </tr>
                            <tr>
                                <td class=""  align="center" valign="middle">
                                    <div  style="width: 98%">
                                        <div style="padding-top:10px; padding-bottom:10px; text-align: left;">
                                        <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.AddNewUC %>" OnClick="btnInsert_Click" />
                                        </div>
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="True" PageSize="15" AllowSorting="True" HorizontalAlign="Left" OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" OnRowUpdating="gMain_RowUpdating" OnRowEditing="gMain_RowEditing" GridLines="None" CellPadding="0" ShowFooter="True">
                                            <Columns>
                                                <asp:CommandField ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" >
                                                    <ItemStyle Width="60px" />
                                                </asp:CommandField>
                                                <asp:BoundField DataField="ProductTypeID" HeaderText="ID" ReadOnly="True" SortExpression="ProductTypeID" >
                                                    <ItemStyle HorizontalAlign="Left" Width="100px" />
                                                    <HeaderStyle HorizontalAlign="Left" Width="100px" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="Product Type" SortExpression="Name">
                                                    <ItemTemplate>
                                                        <asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "EditName") %>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" HorizontalAlign="Left" />
                                                    <HeaderStyle HorizontalAlign="Left" />
                                                </asp:TemplateField>                                                
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <div style="padding-right: 5px" >
                                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("ProductTypeID") %>' runat="Server" AlternateText="Delete" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" /></div>                                            
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" Width="25px" />
                                                    <HeaderStyle HorizontalAlign="Left" />
                                                </asp:TemplateField>
                                                <asp:BoundField Visible="False" DataField="EditName" ReadOnly="True" />
                                            </Columns>
                                            <PagerSettings FirstPageText="<%$Tokens:StringResource, admin.common.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.common.LastPage %>"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="tableDataCellGrid" />
                                            <EditRowStyle  CssClass="gridEdit" />
                                            <PagerStyle CssClass="pagerGrid" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="tableDataCellGridAlt" />
                                        </asp:GridView>
                                        <br />
                                        <br />
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