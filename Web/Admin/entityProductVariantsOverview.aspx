<%@ Page Language="C#" AutoEventWireup="true" CodeFile="entityProductVariantsOverview.aspx.cs" Inherits="entityProductVariantsOverview" Title="<%$Tokens:StringResource, admin.title.entityProductVariantsOverview %>"  %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ OutputCache  Duration="1"  Location="none" %>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <asp:Literal runat="server" id="ltStyles"></asp:Literal>    
    <link href="~/App_Themes/Admin_Default/StyleSheet.css" rel="stylesheet" type="text/css" />
</head>
<body> 
    <form id="frmEntityProducts" runat="server">
        <div style="width: 100%; color: #696969;">
            <asp:Literal ID="ltScript1" runat="server"></asp:Literal>
        </div>        
        <div class="breadCrumb3" style="padding-top:10px">
                <asp:Literal ID="ltEntity" runat="server"></asp:Literal>
        </div>
            <div style="float: left">
                &nbsp;<asp:Literal ID="ltProduct" runat="server"></asp:Literal></div>
            <div>
                &nbsp;<asp:Label ID="Label2" runat="server" Text="Managing Variants "></asp:Label><br />
                </div>
            <div style="padding-bottom: 5px; font-weight: normal; padding-top: 5px; color: #696969;">
                <asp:Literal ID="ltError" runat="server"></asp:Literal>
            </div>
        
    
    
   <div id="container">
           <table class="divBox" style="width: 100%" cellpadding="0" cellspacing="0">
            <tr>
                <td class="titleTable">
                    <asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.common.Variants %>" /></td>
            </tr>
            <tr>
                <td align="center" valign="middle">                  
                    <div style="width: 99%; text-align: left; padding-top:5px; padding-bottom:5px">
    				<asp:Button runat="server" ID="btnAdd" CssClass="normalButtons" Text="ADD NEW" OnClick="btnAdd_Click" />
                    <asp:Button ID="btnUpdate" runat="server" CssClass="normalButtons" Text="Update Order and Default Variant" OnClick="btnUpdate_Click" />
                    <asp:Button runat="server" ID="btnDeleteVariants" CssClass="normalButtons" Text="DELETE ALL VARIANTS" OnClick="btnDeleteVariants_Click" /></div>
                </td>
            </tr>
            <tr>
                <td align="center" valign="middle">
                    <div style="width: 99%">
                    <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="True" PageSize="15" AllowSorting="True" HorizontalAlign="Left" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" CellPadding="0" GridLines="None" ShowFooter="True">
                                                <Columns>
                                                    <asp:BoundField DataField="VariantID" HeaderText="ID" ReadOnly="True" SortExpression="VariantID" >
                                                        <ItemStyle HorizontalAlign="Left" />
                                                    </asp:BoundField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Image %>">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="ltImage" runat="server"></asp:Literal>
                                                        </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Left" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Product %>" SortExpression="Name">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="ltName" runat="server"></asp:Literal>
                                                        </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Left" Width="300px" />
                                                        <HeaderStyle Width="300px" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.SKU %>" SortExpression="SKUSuffix">
                                                        <ItemTemplate>
							                                <asp:Literal ID="ltSKU" runat="server"></asp:Literal>
						                                </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Left" />
                                                    </asp:TemplateField>
						                            <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Price %>" SortExpression="Price">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="ltPrice" runat="server"></asp:Literal>
                                                        </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Left" />
                                                    </asp:TemplateField>
						                            <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Inventory %>" SortExpression="Inventory">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="ltInventory" runat="server"></asp:Literal>
							                            </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Left" />
						                            </asp:TemplateField>
						                            <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.DisplayOrder %>" SortExpression="DisplayOrder">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="ltDisplayOrder" runat="server"></asp:Literal>
							                            </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Left" />
						                            </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.IsDefaultVariant %>">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="ltDefault" runat="server"></asp:Literal>
							                            </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Center" Width="150px" />
                                                        <HeaderStyle HorizontalAlign="Center" Width="150px" />
                                                    </asp:TemplateField>    
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.entityProducts.Clone %>">
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="lnkClone" CommandName="CloneItem" CommandArgument='<%# Eval("VariantID") %>' runat="Server" Text="Clone" />                                                        
                                                        </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Center" />
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                    </asp:TemplateField>      
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Move %>">
                                                        <ItemTemplate>
                                                            <input type="button" value="Move" name='Move_<%# Eval("VariantID") %>' onclick="window.open('entityMoveVariant.aspx?productid=<%# Eval("ProductID") %>&Variantid=<%# Eval("VariantID") %>','Move','height=200, width=300, scrollbars=yes, resizable=yes, toolbar=no, status=yes, location=no, directories=no, menubar=no, alwaysRaised=yes');" class="normalButtons" />
                                                        </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Center" />
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                    </asp:TemplateField>                                               
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.SoftDelete %>">
                                                        <ItemTemplate>
                                                            <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("VariantID") %>' runat="Server" AlternateText="<%$Tokens:StringResource, admin.common.Delete %>" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                                        </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Center" />
                                                        <HeaderStyle HorizontalAlign="Center" />
                                                    </asp:TemplateField>
                                                </Columns>
                                                <PagerSettings FirstPageText="<%$Tokens:StringResource, admin.countries.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.countries.LastPage %>"
                                                    Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                                <FooterStyle CssClass="gridFooter" />
                                                <RowStyle CssClass="table-row2" />
                                                <PagerStyle CssClass="gridPager" HorizontalAlign="Left" />
                                                <HeaderStyle CssClass="gridHeader" />
                                                <AlternatingRowStyle CssClass="table-alternatingrow2" />
                                            </asp:GridView>
                    </div>
                    &nbsp;
            </td>
            </tr>
               <tr>
                   <td>
        <asp:Literal ID="ltScript" runat="server"></asp:Literal></td>
               </tr>
        </table>
   </div>
    </form>
</body>
</html>
