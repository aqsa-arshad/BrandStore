<%@ Page language="c#" Inherits="AspDotNetStorefrontAdmin.states" CodeFile="states.aspx.cs"
 MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="litContent"  runat="server" Text="" />
        <div class="breadCrumb3">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal><asp:Literal ID="ltValid" runat="server"></asp:Literal></div>
    <div id="">
        <div class="errorMsg" style="margin-bottom: 5px; margin-top: 5px;">
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
                                <td class="titleTable">
                                    <font class="subTitle">State Taxes:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <asp:Panel runat="server" id="pnlGrid">
                                        <div style="padding-top: 15px; padding-bottom: 15px">
                                        <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="ADD NEW" OnClick="btnInsert_Click" />
                                        <asp:Button runat="server" ID="btnUpdateOrder" CssClass="normalButtons" Text="Update Taxes" OnClick="btnUpdateOrder_Click" /><br />
                                        </div>
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="True" PageSize="999999" AllowSorting="True" HorizontalAlign="Left" OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" OnRowUpdating="gMain_RowUpdating" OnRowEditing="gMain_RowEditing" BorderStyle="None" BorderWidth="0px" CellPadding="0" GridLines="None" ShowFooter="True">
                                            <Columns>
                                                <asp:CommandField ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/Images/cancel.gif" EditImageUrl="~/App_Themes/Admin_Default/Images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/Images/update.gif" >
                                                    <ItemStyle Width="25px" CssClass="commandStyle"/>
                                                </asp:CommandField>
                                                <asp:BoundField DataField="StateID" HeaderText="ID" ReadOnly="True" SortExpression="StateID" >
                                                    <ItemStyle Width="25px" />
                                                </asp:BoundField>
                                                
                                                <asp:TemplateField HeaderText="State/Province" SortExpression="Name">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "Name") %>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <a name='a<%# Eval("StateID") %>'></a>
                                                        <asp:TextBox ID="txtName" runat="Server" CssClass="singleAuto" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ErrorMessage="!!" ControlToValidate="txtName"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" Width="150px" />
                                                </asp:TemplateField>

                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.states.Published %>" SortExpression="Published">                                                
                                                    <ItemTemplate>                                                        
                                                        <asp:CheckBox ID="cbPublished" runat="server" Checked='<%# publishedCheck((object) Eval("Published")) %>' Enabled="false" />
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:CheckBox ID="cbPublished" runat="server" Checked='<%# publishedCheck((object) Eval("Published")) %>' Enabled="true" />
                                                    </EditItemTemplate>
                                                    <ItemStyle Width="75px" />
                                                </asp:TemplateField>
                                                
                                                <asp:TemplateField HeaderText="Display Order" SortExpression="DisplayOrder" HeaderStyle-Width="95px">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtOrder" runat="Server" CssClass="textBox30" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="!!" ControlToValidate="txtOrder"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" Width="75px" HorizontalAlign="Center" />
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                </asp:TemplateField>  
                                                
                                                <asp:TemplateField HeaderText=" Abbrev." SortExpression="Abbreviation">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "Abbreviation")%>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtAbbreviation" runat="Server" CssClass="textBox30" Text='<%# DataBinder.Eval(Container.DataItem, "Abbreviation") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="!!" ControlToValidate="txtAbbreviation"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" Width="75px" />
                                                </asp:TemplateField>  
                                                   
                                                <asp:TemplateField HeaderText="Tax Rate">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="ltTaxRate" runat="server"></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:Literal ID="ltTaxRate" runat="server"></asp:Literal>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>  
                                                
                                                <asp:TemplateField HeaderText="Country" SortExpression="Country">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "Country")%>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ID="ddCountry" runat="server" CssClass="default"></asp:DropDownList>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:TemplateField> 
                                                
                                                <asp:TemplateField HeaderText="Del">
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("StateID") %>' runat="Server" AlternateText="Delete" ImageUrl="~/App_Themes/Admin_Default/Images/delete2.gif" />                                                        
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" Width="25px" />
                                                </asp:TemplateField>
                                                <asp:BoundField Visible="False" DataField="EditName" ReadOnly="True" />
                                            </Columns>
                                            <PagerSettings FirstPageText="&amp;lt;&amp;lt;First Page" LastPageText="Last Page&amp;gt;&amp;gt;"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="gridEdit2" />
                                            <PagerStyle CssClass="tablepagerGrid" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" BorderWidth="0px" />
                                        </asp:GridView>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlAdd" runat="Server" DefaultButton="btnSubmit">
                                        <div style="margin-top: 5px; margin-bottom: 15px;">
                                            Fields marked with an asterisk (*) are required. All other fields are optional.
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*State/Province:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtName" runat="server" CssClass="singleNormal" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="Fill in Name" ControlToValidate="txtName" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator> 
                                                   <asp:Image runat="server" CssClass="exampleText" ID="imgStateName" ToolTip ="<%$Tokens:StringResource,admin.states.tooltip.imgStateName %>" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*Abbreviation:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtAbbr" runat="server" CssClass="single3chars" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="Fill in Abbreviation" ControlToValidate="txtAbbr" ID="RequiredFieldValidator3" ValidationGroup="gAdd" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator> 
                                                    <asp:Image runat ="server" CssClass ="exampleText" ID="imgAbbreviation" ToolTip ="<%$Tokens:StringResource,admin.states.tooltip.imgAbbreviation %>" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" />
                                                    
                                                </td>
                                            </tr>  
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">Country:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddCountry" runat="Server"></asp:DropDownList>
                                                    <asp:Image runat ="server" CssClass ="exampleText" ID="imgCountry" ToolTip ="<%$Tokens:StringResource,admin.states.tooltip.imgCountry %>" ImageUrl ="~/App_Themes/Admin_Default/images/info.gif" />
                                                </td>
                                            </tr>                                           
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*Display Order:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                     <asp:TextBox ID="txtOrder" runat="Server" CssClass="single3chars" ValidationGroup="gAdd">1</asp:TextBox>
                                                     <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="Enter Display Order" ValidationGroup="gAdd" ControlToValidate="txtOrder">!!</asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.states.Published %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">                                                     
                                                     <asp:CheckBox ID="cbPublished" runat="server" />                                                     
                                                </td>
                                            </tr>                                                                                         
                                            <tr>
                                                <td colspan="2">
                                                    <asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                                </td>
                                            </tr>
                                        </table>
                                        <div style="width: 100%; text-align: center;">
                                            &nbsp;&nbsp;<asp:Button ValidationGroup="gAdd" CssClass="normalButtons" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" Text="Submit" />
                                            &nbsp;&nbsp;<asp:Button ID="btnCancel" CssClass="normalButtons" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
                                        </div>
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
    <asp:Literal ID="ltMiscellaneous" runat="server"></asp:Literal>
        <br />
        <div style="width: 100px; height: 100px">
        </div>
</asp:Content> 