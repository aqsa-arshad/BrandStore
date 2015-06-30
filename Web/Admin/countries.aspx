<%@ Page language="c#" Inherits="AspDotNetStorefrontAdmin.countries" CodeFile="countries.aspx.cs" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder"> 
        <div class="breadCrumb3">
    <asp:Literal ID="ltValid" runat="server"></asp:Literal><asp:Literal ID="ltScript" runat="server"></asp:Literal></div>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;" class="breadCrumb3">
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
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.CountryTaxes %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div>
                                        <asp:Panel runat="server" id="pnlGrid">
                                            <div style="padding-top: 15px; padding-bottom: 15px">
                                        <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.AddNewUC %>" OnClick="btnInsert_Click" />
                                            <asp:Button runat="server" ID="btnUpdateOrder" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.countries.button.UpdateTaxes %>" OnClick="btnUpdateOrder_Click" /></div>
                                        <asp:GridView Width="100%" ID="gMain" runat="server" 
                                            HeaderStyle-Wrap="true" 
                                            PagerStyle-HorizontalAlign="left" 
                                            PagerSettings-Position="TopAndBottom" 
                                            AutoGenerateColumns="False" 
                                            AllowPaging="True" 
                                            PageSize="999999" 
                                            AllowSorting="True" 
                                            HorizontalAlign="Left" 
                                            OnRowCancelingEdit="gMain_RowCancelingEdit" 
                                            OnRowCommand="gMain_RowCommand" 
                                            OnRowDataBound="gMain_RowDataBound" 
                                            OnSorting="gMain_Sorting" 
                                            OnPageIndexChanging="gMain_PageIndexChanging" 
                                            OnRowUpdating="gMain_RowUpdating" 
                                            OnRowEditing="gMain_RowEditing" 
                                            CellPadding="0" 
                                            GridLines="None" 
                                            ShowFooter="True">
                                            <Columns>
                                                <asp:CommandField ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" >
                                                    <ItemStyle Width="25px" CssClass="commandStyle" />
                                                </asp:CommandField>
                                                <asp:BoundField DataField="CountryID" HeaderText="<%$Tokens:StringResource, admin.common.ID %>" ReadOnly="True" SortExpression="CountryID" >
                                                    <ItemStyle Width="25px" />
                                                </asp:BoundField>
                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Country %>" SortExpression="Name" HeaderStyle-Wrap="true">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "Name") %>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <a name='a<%# Eval("CountryID") %>'></a>
                                                        <asp:TextBox  Width="100px" ID="txtName" runat="Server" CssClass="single3chars" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.phoneorder.validator.RequiredMessage %>" ControlToValidate="txtName"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" Width="200px" />
                                                </asp:TemplateField>
                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.countries.TwoLetterISOCode %>" SortExpression="TwoLetterISOCode">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "TwoLetterISOCode")%>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txt2LetterIso" runat="Server" CssClass="textBox30" Text='<%# DataBinder.Eval(Container.DataItem, "TwoLetterISOCode") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.phoneorder.validator.RequiredMessage %>" ControlToValidate="txt2LetterIso"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" Width="75px" />
                                                </asp:TemplateField>  
                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.countries.ThreeLetterISOCode %>" SortExpression="ThreeLetterISOCode">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "ThreeLetterISOCode")%>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txt3LetterIso" runat="Server" CssClass="textBox30" Text='<%# DataBinder.Eval(Container.DataItem, "ThreeLetterISOCode") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.phoneorder.validator.RequiredMessage %>" ControlToValidate="txt3LetterIso"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle Width="75px" />
                                                </asp:TemplateField>  
                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.countries.NumericISOCode %>" SortExpression="NumericISOCode">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "NumericISOCode")%>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtNumericISOCode" runat="Server" CssClass="textBox3" Text='<%# DataBinder.Eval(Container.DataItem, "NumericISOCode") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.phoneorder.validator.RequiredMessage %>" ControlToValidate="txtNumericISOCode"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle Width="75px" />
                                                </asp:TemplateField>  
                                                                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.countries.Published %>" SortExpression="Published">                                                
                                                    <ItemTemplate>                                                        
                                                        <asp:CheckBox ID="cbPublished" runat="server" Checked='<%# publishedCheck((object) Eval("Published")) %>' Enabled="false" />
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:CheckBox ID="cbPublished" runat="server" Checked='<%# publishedCheck((object) Eval("Published")) %>' Enabled="true" />
                                                    </EditItemTemplate>
                                                    <ItemStyle Width="75px" />
                                                </asp:TemplateField>  

                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.countries.PostalCodeRequired %>" SortExpression="PostalCodeRequired">                                                
                                                    <ItemTemplate>                                                        
                                                        <asp:CheckBox ID="cbPostalCodeRequired" runat="server" Checked='<%# postalCodeCheck((object) Eval("PostalCodeRequired")) %>' Enabled="false" />
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:CheckBox ID="cbPostalCodeRequired" runat="server" Checked='<%# postalCodeCheck((object) Eval("PostalCodeRequired")) %>' Enabled="true" />
                                                    </EditItemTemplate>
                                                    <ItemStyle Width="75px" />
                                                </asp:TemplateField>  
                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.countries.PostalCodeRegex %>" SortExpression="PostalCodeRegex">
                                                    <ItemTemplate>
                                                        <%# Wrap(DataBinder.Eval(Container.DataItem, "PostalCodeRegex").ToString()) %>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox TextMode="MultiLine" width="100px" Height="50px" ID="txtPostalCodeRegex" runat="Server" CssClass="textBox3" Text='<%# DataBinder.Eval(Container.DataItem, "PostalCodeRegex") %>'></asp:TextBox>                                                        
                                                    </EditItemTemplate>
                                                    <ItemStyle Width="100px" Wrap="true" />
                                                </asp:TemplateField>
                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.countries.PostalCodeExample %>" SortExpression="PostalCodeExample">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "PostalCodeExample")%>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtPostalCodeExample" runat="Server" CssClass="textBox3" Text='<%# DataBinder.Eval(Container.DataItem, "PostalCodeExample") %>'></asp:TextBox>                                                        
                                                    </EditItemTemplate>
                                                    <ItemStyle Width="75px" />
                                                </asp:TemplateField> 
                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.DisplayOrder %>" SortExpression="DisplayOrder">
                                                    <ItemTemplate>
                                                        &nbsp;<asp:Label ID="Label1" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:Label>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtOrder" runat="Server" CssClass="textBox30" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.phoneorder.validator.RequiredMessage %>" ControlToValidate="txtOrder"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" Width="75px" HorizontalAlign="Center" />
                                                    <HeaderStyle HorizontalAlign="Center" />
                                                </asp:TemplateField>  
                                                
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.TaxRate %>">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="ltTaxRate" runat="server"></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate><asp:Literal ID="ltTaxRate" runat="server"></asp:Literal></EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>                                            
                                               
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("CountryID") %>' runat="Server" AlternateText="<%$Tokens:StringResource, admin.common.Delete %>" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" Width="25px" />
                                                </asp:TemplateField>
                                                <asp:BoundField Visible="False" DataField="EditName" ReadOnly="True" />
                                            </Columns>
                                            <PagerSettings FirstPageText="<%$Tokens:StringResource, admin.countries.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.countries.LastPage %>"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="gridEdit2" />
                                            <PagerStyle CssClass="gridPager" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" BorderStyle="None" BorderWidth="0px" Wrap="true" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                        </asp:GridView>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlAdd" runat="Server" DefaultButton="btnSubmit">
                                        <div style="margin-top: 5px; margin-bottom: 15px;">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.RequiredFieldsPrompt %>" />
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.Country %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtName" runat="server" CssClass="singleNormal" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinName %>" ControlToValidate="txtName" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 
                                                    <asp:Image ID="imgCountryName" runat ="server" CssClass ="exampleText" ToolTip ="<%$Tokens:StringResource, admin.countries.tooltip.imgCountryName %>" ImageUrl ="~/App_Themes/Admin_Default/images/info.gif" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.TwoLetterISOCode %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txt2ISO" runat="server" CssClass="single3chars" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.countries.FillInISO %>" ControlToValidate="txt2ISO" ID="RequiredFieldValidator3" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 
                                                    <asp:Image ID="img2LetterISOCode" runat ="server" CssClass ="exampleText" ToolTip ="<%$Tokens:StringResource, admin.countries.tooltip.img2LetterISOCode %>" ImageUrl ="~/App_Themes/Admin_Default/images/info.gif" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.ThreeLetterISOCode %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txt3ISO" runat="server" CssClass="single3chars" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.countries.FillInISO %>" ControlToValidate="txt3ISO" ID="RequiredFieldValidator8" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 
                                                    <asp:Image ID="img3LetterISOCode" runat ="server" CssClass ="exampleText" ToolTip ="<%$Tokens:StringResource, admin.countries.tooltip.img3LetterISOCode %>" ImageUrl ="~/App_Themes/Admin_Default/images/info.gif" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.NumericISOCode %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtNumericISO" runat="server" CssClass="single3chars" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.countries.FillInISO %>" ControlToValidate="txtNumericISO" ID="RequiredFieldValidator9" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 
                                                    <asp:Image ID="imgNumericISOCode" runat ="server" CssClass ="exampleText" ToolTip ="<%$Tokens:StringResource, admin.countries.tooltip.imgNumericISOCode %>" ImageUrl ="~/App_Themes/Admin_Default/images/info.gif" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.Published %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">                                                     
                                                     <asp:CheckBox ID="cbPublished" runat="server" />                                                     
                                                </td>
                                            </tr>  
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.PostalCodeRequired %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">                                                     
                                                     <asp:CheckBox ID="cbPostalCodeReq" runat="server" />                                                     
                                                </td>
                                            </tr>  
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.PostalCodeRegex %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                     <asp:TextBox ID="txtPostalCodeRegEx" runat="Server" CssClass="singleNormal" ValidationGroup="gAdd"></asp:TextBox>                                                     
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.countries.PostalCodeExample %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                     <asp:TextBox ID="txtPostalCodeExample" runat="Server" CssClass="singleNormal" ValidationGroup="gAdd"></asp:TextBox>                                                     
                                                </td>
                                            </tr>                                            
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.DisplayOrder %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                     <asp:TextBox ID="txtOrder" runat="Server" CssClass="single3chars" ValidationGroup="gAdd">1</asp:TextBox>
                                                     <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.common.EnterDisplayOrder %>" ValidationGroup="gAdd" ControlToValidate="txtOrder"></asp:RequiredFieldValidator>
                                                </td>
                                            </tr>                                                                                       
                                            <tr>
                                                <td colspan="2">
                                                    <asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                                </td>
                                            </tr>
                                        </table>
                                        <div style="width: 100%; text-align: center;">
                                            &nbsp;&nbsp;<asp:Button ValidationGroup="gAdd" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Submit %>" />
                                            &nbsp;&nbsp;<asp:Button ID="btnCancel" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Cancel %>" OnClick="btnCancel_Click" />
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
    </asp:Content>
