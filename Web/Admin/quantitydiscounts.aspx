<%@ Page Language="C#" AutoEventWireup="true" CodeFile="quantitydiscounts.aspx.cs" Inherits="AspDotNetStorefrontAdmin.quantitydiscounts" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
        <div class="breadCrumb3">
    <asp:Literal ID="ltScript1" runat="server"></asp:Literal><asp:Literal ID="ltValid" runat="server"></asp:Literal><asp:Literal runat="server" ID="ltStyles"></asp:Literal></div>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;" class="breadCrumb3">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
            <tr>
                <td>
                    <div class="">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="" width="100%">
                            <tr>
                                <td class="titleTable" width="150">
                                    <font class="subTitle">Discount Tables List:</font>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #EBEBEB;" />
                                <td style="width: 5px;" />
                                <td class="titleTable">
                                    <font class="subTitle">Discount Table Details:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="150">
                                    <div class="wrapperTop">
                                        <asp:Button runat="server" ID="btnAdd" CssClass="normalButtons" Text="ADD NEW" OnClick="btnAdd_Click" />                                   
				    </div>
				    <div class="wrapperTopBottom">                                        
                                    	<asp:TreeView ID="treeMain" runat="server" OnSelectedNodeChanged="treeMain_SelectedNodeChanged">
                                        </asp:TreeView>
                                    </div>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #EBEBEB;" />
                                <td style="width: 5px;" />
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">
                                        <asp:PlaceHolder ID="phMain" runat="server" >
                                            <asp:Panel ID="pnlAdd" runat="server" DefaultButton="btnSubmit">
                                            <font class="titleMessage">
                                                <asp:Literal runat="server" ID="ltMode"></asp:Literal> Discount Table
                                            </font>
                                            <p>
                                                Please enter the name of the Discount Table. Fields marked with an asterisk (*) are required. All other fields are optional.
                                            </p>
                                            <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                                <tr>
                                                    <td align="right" valign="top" width="150">
                                                        <font class="subTitleSmall">Discount Table Name:</font>
                                                    </td>
                                                    <td align="left" valign="top" width="*">
                                                        <asp:Literal ID="ltName" runat="server"></asp:Literal>
                                                    </td>
                                                </tr>
                                            </table>
                                            <div id="divInitialDD" runat="server">
                                                <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                                    <tr>
                                                        <td align="right" valign="top" width="150">
                                                            <font class="subTitleSmall">How many initial values<br />do you want to add?</font>
                                                        </td>
                                                        <td align="left" valign="top" width="*">
                                                            <asp:DropDownList ID="ddValues" runat="server">
                                                                <asp:ListItem Value="1" />
                                                                <asp:ListItem Value="3" />
                                                                <asp:ListItem Value="5" Selected="true" />
                                                            </asp:DropDownList>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                            <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                                <tr>
                                                    <td align="right" valign="top" width="150">
                                                        <font class="subTitleSmall">Discount Type:</font>
                                                    </td>
                                                    <td align="left" valign="top" width="*">
                                                        <asp:DropDownList ID="ddlDscntType" runat="server">
                                                            <asp:ListItem Value="0" Text="Percent" />
                                                            <asp:ListItem Value="1" Text="Fixed Amount" />
                                                        </asp:DropDownList>
                                                    </td>
                                                </tr>
                                            </table>
                                            <div style="width: 100%; text-align: left; padding-left: 145px;">
                                                &nbsp;&nbsp;<asp:Button ID="btnSubmit" CssClass="normalButtons" runat="server" OnClick="btnSubmit_Click" />
                                                &nbsp;&nbsp;<asp:Button ID="btnDelete" CssClass="normalButtons" runat="server" OnClick="btnDelete_Click" Text="Delete Discount Table" />
                                            </div>
                                            </asp:Panel>
                                            <div style="width: 100%; text-align: left;" runat="server" id="divGrid">
                                                <br />
                                                <hr noshade="noshade" width="100%" />
                                                <p>
                                                    Please enter the Discount Table values.
                                                </p>
                                                <div id="divInitial" runat="server">
                                                    <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                                        <tr id="tr1" runat="server">
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Low Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtLow1" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidatorIni1" runat="server" ControlToValidate="txtLow1" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">High Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtHigh1" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidatorIni2" runat="server" ControlToValidate="txtHigh1" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>                                                    
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Discount:</font>
                                                                <asp:TextBox runat="server" ID="txtPercent1" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidatorIni3" runat="server" ControlToValidate="txtPercent1" ErrorMessage="Invalid Discount" Type="double" MinimumValue="0" MaximumValue="100"></asp:RangeValidator>
                                                            </td>
                                                        </tr>
                                                        <tr id="tr2" runat="server">
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Low Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtLow2" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator3" runat="server" ControlToValidate="txtLow2" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">High Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtHigh2" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator4" runat="server" ControlToValidate="txtHigh2" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>                                                    
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Discount:</font>
                                                                <asp:TextBox runat="server" ID="txtPercent2" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator5" runat="server" ControlToValidate="txtPercent2" ErrorMessage="Invalid Discount" Type="double" MinimumValue="0" MaximumValue="100"></asp:RangeValidator>
                                                            </td>
                                                        </tr>
                                                        <tr id="tr3" runat="server">
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Low Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtLow3" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator6" runat="server" ControlToValidate="txtLow3" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">High Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtHigh3" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator7" runat="server" ControlToValidate="txtHigh3" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>                                                    
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Discount:</font>
                                                                <asp:TextBox runat="server" ID="txtPercent3" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator8" runat="server" ControlToValidate="txtPercent3" ErrorMessage="Invalid Discount" Type="double" MinimumValue="0" MaximumValue="100"></asp:RangeValidator>
                                                            </td>
                                                        </tr>
                                                        <tr id="tr4" runat="server">
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Low Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtLow4" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator9" runat="server" ControlToValidate="txtLow4" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">High Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtHigh4" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator10" runat="server" ControlToValidate="txtHigh4" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>                                                    
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Discount:</font>
                                                                <asp:TextBox runat="server" ID="txtPercent4" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator11" runat="server" ControlToValidate="txtPercent4" ErrorMessage="Invalid Discount" Type="double" MinimumValue="0" MaximumValue="100"></asp:RangeValidator>
                                                            </td>
                                                        </tr>
                                                        <tr id="tr5" runat="server">
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Low Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtLow5" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator12" runat="server" ControlToValidate="txtLow5" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">High Quantity:</font>
                                                                <asp:TextBox runat="server" ID="txtHigh5" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator13" runat="server" ControlToValidate="txtHigh5" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>                                                    
                                                            </td>
                                                            <td align="left" valign="top">
                                                                <font class="subTitleSmall">Discount:</font>
                                                                <asp:TextBox runat="server" ID="txtPercent5" CssClass="singleShortest"></asp:TextBox>
                                                                <asp:RangeValidator ID="RangeValidator14" runat="server" ControlToValidate="txtPercent5" ErrorMessage="Invalid Discount" Type="double" MinimumValue="0" MaximumValue="100"></asp:RangeValidator>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </div>
                                                <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="ADD NEW" OnClick="btnInsert_Click" />
                                                <asp:GridView Width="100%" ID="gMain" runat="server" AutoGenerateColumns="False" CssClass="overallGrid" HorizontalAlign="Left" OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnRowUpdating="gMain_RowUpdating" OnRowEditing="gMain_RowEditing">
                                                    <Columns>
                                                        <asp:CommandField ItemStyle-Width="60" ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" CancelText="Cancel" EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" EditText="Edit" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" UpdateText="Update" />
                                                        <asp:BoundField DataField="QuantityDiscountTableID" HeaderText="ID" ReadOnly="True" SortExpression="QuantityDiscountTableID" ItemStyle-CssClass="lighterData" />
                                                        <asp:TemplateField HeaderText="Low Quantity" SortExpression="LowQuantity" ItemStyle-CssClass="normalData">
                                                            <ItemTemplate>
                                                                <asp:Literal runat="server" ID="ltLow" Text='<%# DataBinder.Eval(Container.DataItem, "LowQuantity") %>'></asp:Literal>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox runat="server" ID="txtLow" CssClass="singleShortest" Text='<%# DataBinder.Eval(Container.DataItem, "LowQuantity") %>'></asp:TextBox>
                                                                ex: 5
                                                                <asp:RangeValidator ID="RangeValidator" runat="server" ControlToValidate="txtLow" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>
                                                            </EditItemTemplate>
                                                        </asp:TemplateField>   
                                                        <asp:TemplateField HeaderText="High Quantity" SortExpression="HighQuantity" ItemStyle-CssClass="normalData">
                                                            <ItemTemplate>
                                                                <asp:Literal runat="server" ID="ltHigh" Text='<%# DataBinder.Eval(Container.DataItem, "HighQuantity") %>'></asp:Literal>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox runat="server" ID="txtHigh" CssClass="singleShortest" Text='<%# DataBinder.Eval(Container.DataItem, "HighQuantity") %>'></asp:TextBox>
                                                                ex: 15
                                                                <asp:RangeValidator ID="RangeValidator1" runat="server" ControlToValidate="txtHigh" ErrorMessage="Invalid Range" Type="Integer" MinimumValue="0" MaximumValue="999999"></asp:RangeValidator>
                                                            </EditItemTemplate>
                                                        </asp:TemplateField>    
                                                        <asp:TemplateField HeaderText="Discount" SortExpression="DiscountPercent" ItemStyle-CssClass="normalData">
                                                            <ItemTemplate>
                                                                <asp:Literal runat="server" ID="ltDiscount" Text='<%# DataBinder.Eval(Container.DataItem, "DiscountPercent") %>'></asp:Literal>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox runat="server" ID="txtPercent" CssClass="singleShortest" Text='<%# DataBinder.Eval(Container.DataItem, "DiscountPercent") %>'></asp:TextBox>
                                                                ex: 12.75
                                                                <asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="txtPercent" ErrorMessage="Invalid Percent" Type="double" MinimumValue="0" MaximumValue="<%#DiscountUpperBound() %>"></asp:RangeValidator>
                                                            </EditItemTemplate>
                                                        </asp:TemplateField>                                             
                                                        <asp:TemplateField ItemStyle-CssClass="selectData" ItemStyle-Width="25">
                                                            <ItemTemplate>
                                                                <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("QuantityDiscountTableID") %>' runat="Server" AlternateText="Delete" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField Visible="false" DataField="EditName" ReadOnly="true" />
                                                    </Columns>
                                                    <RowStyle CssClass="DataCellGrid" />
                                                    <EditRowStyle CssClass="DataCellGridEdit" />
                                                    <HeaderStyle CssClass="headerGrid" />
                                                    <AlternatingRowStyle CssClass="DataCellGridAlt" />
                                                </asp:GridView>
                                            </div>
                                        </asp:PlaceHolder>
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