<%@ Page Language="c#" AutoEventWireup="true" CodeFile="manageratings.aspx.cs" Inherits="AspDotNetStorefrontAdmin.manageratings"
    MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div align="center" class="divBox2">
        <br />
        <br />
        <table runat="server" align="center" cellpadding="5">
                <tr>
                    <td>
                        &nbsp;
                    </td>
                    <td>
                        <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.SearchTerm %>" />
                    </td>
                    <td>
                        <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.ShowFilthy %>" />
                    </td>
                    <td>
                        <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.NumDays %>" />
                    </td>
                    <td runat="server" id="tdStoreTitle">
                        <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.ShowStore %>" />
                    </td>
                    <td runat="server" id="tdStoreFilter">
                        <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.FilterByStore %>" />
                    </td>
                    <td>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.manageratings.SearchRatings %>" />
                    </td>
                    <td>
                        <asp:TextBox ID="txtSearchTerm" runat="server" />
                    </td>
                    <td>
                        <asp:RadioButtonList ID="rblFilthyOnly" runat="server">
                            <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.yes %>" Selected="False"
                                Value="1" />
                            <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.no %>" Selected="true"
                                Value="0" />
                        </asp:RadioButtonList>
                    </td>
                    <td>
                        <asp:TextBox ID="txtShowDays" runat="server" MaxLength="10" Width="40" />
                    </td>
                    <td runat="server" id="tdStoreSelect">
                        <asp:DropDownList ID="ddlStoreID" runat="server" />
                    </td>
                    <td runat="server" id="tdFilterSelect">
                        <asp:RadioButtonList ID="rblFilterByStore" runat="server">
                            <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.yes %>" Selected="False"
                                Value="1" />
                            <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.no %>" Selected="true"
                                Value="0" />
                        </asp:RadioButtonList>
                    </td>
                    <td>
                    </td>
                </tr>
        </table>
        <br />
        <br />
        <asp:Button ID="btnSubmit" runat="server" Text="<%$ Tokens:StringResource, admin.common.submit %>"
            CssClass="normalButtons" />
    </div>
    <br />
    <div align="center">
            <asp:GridView ID="grdRatings" runat="server" AllowPaging="True" AllowSorting="True"
                AutoGenerateColumns="False" DataSourceID="sqlGridSource" DataKeyNames="RatingID" BorderWidth="0px" BorderStyle="None" Width="95%"
                GridLines="None" OnRowDataBound="grdRatings_RowDataBound" OnRowUpdating="grdRatings_RowUpdating" >
                <AlternatingRowStyle CssClass="gridAlternatingRowPlain" />
                <RowStyle CssClass="gridRowPlain" />
                <HeaderStyle CssClass="gridHeaderSmall" />
                <Columns>
                    <asp:CommandField ShowEditButton="True" ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif"
                        EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif"
                        ItemStyle-Width="115">
                        <ItemStyle Width="115px"></ItemStyle>
                    </asp:CommandField>
                    <asp:BoundField DataField="RatingID" HeaderText="<%$ Tokens:StringResource, admin.manageratings.RatingID %>"
                        InsertVisible="False" ReadOnly="True" SortExpression="RatingID" ItemStyle-Width="35">
                        <ItemStyle Width="35px"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="CreatedOn" DataFormatString="{0:d}" HeaderText="<%$ Tokens:StringResource, admin.manageratings.Date %>"
                        ReadOnly="true" SortExpression="CreatedOn" ItemStyle-Width="75">
                        <ItemStyle Width="75px"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="ProductID" HeaderText="<%$ Tokens:StringResource, admin.manageratings.ProductID %>"
                        SortExpression="ProductID" ReadOnly="true" ItemStyle-Width="75">
                        <ItemStyle Width="75px"></ItemStyle>
                    </asp:BoundField>
                    <asp:TemplateField SortExpression="Rating" HeaderText="<%$ Tokens:StringResource, admin.manageratings.Rating %>">
                        <EditItemTemplate>
                            <asp:DropDownList runat="server" ID="ddlRating" SelectedValue='<%# Bind("Rating") %>'>
                                <asp:ListItem Value="1">1</asp:ListItem>
                                <asp:ListItem Value="2">2</asp:ListItem>
                                <asp:ListItem Value="3">3</asp:ListItem>
                                <asp:ListItem Value="4">4</asp:ListItem>
                                <asp:ListItem Value="5">5</asp:ListItem>
                            </asp:DropDownList>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Rating") %>'></asp:Label>
                        </ItemTemplate>
                        <ItemStyle Width="55px" />
                    </asp:TemplateField>
                    <asp:TemplateField SortExpression="Comments" HeaderText="<%$ Tokens:StringResource, admin.manageratings.Comments %>" ItemStyle-Wrap="true">
                        <ItemTemplate>
                            <asp:Literal runat="server" Text='<%# Eval("Comments") %>' />
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox runat="server" ID="txtComments" TextMode="MultiLine" Text='<%# Bind("Comments") %>' Width="400" Rows="5" />
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="FoundHelpful" HeaderText="<%$ Tokens:StringResource, admin.manageratings.MarkedHelpful %>"
                        SortExpression="FoundHelpful" ReadOnly="true" ItemStyle-Width="50">
                        <ItemStyle Width="50px"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="FoundNotHelpful" HeaderText="<%$ Tokens:StringResource, admin.manageratings.MarkedUnhelpful %>"
                        SortExpression="FoundNotHelpful" ReadOnly="true" ItemStyle-Width="50">
                        <ItemStyle Width="50px"></ItemStyle>
                    </asp:BoundField>
                    <asp:CheckBoxField DataField="IsFilthy" HeaderText="<%$ Tokens:StringResource, admin.manageratings.IsFilthy %>"
                        ItemStyle-Width="50">
                        <ItemStyle Width="50px"></ItemStyle>
                    </asp:CheckBoxField>
                    <asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.common.Delete %>" ItemStyle-Width="55">
                    <ItemTemplate>
                        <asp:ImageButton runat="server" ID="btnDelete" CommandName="Delete" CommandArgument='<%# Eval("RatingID") %>' 
                            ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />
                    </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="sqlGridSource" runat="server" ProviderName="System.Data.SqlClient"
                ConnectionString="Data Source=(local);Initial Catalog=ML_9000;Persist Security Info=True;User ID=sa;Password=Sa12345$"
                SelectCommand="aspdnsf_SearchProductRatings" SelectCommandType="StoredProcedure"
                UpdateCommand="aspdnsf_updProductRating" UpdateCommandType="StoredProcedure"
                DeleteCommand="aspdnsf_delProductRating" DeleteCommandType="StoredProcedure">
                <SelectParameters>
                    <asp:ControlParameter Name="SearchTerm" ControlID="txtSearchTerm" PropertyName="Text"
                        Type="String" DefaultValue="" ConvertEmptyStringToNull="false" />
                    <asp:ControlParameter Name="FilthyOnly" ControlID="rblFilthyOnly" />
                    <asp:ControlParameter ControlID="txtShowDays" Name="days" PropertyName="Text" Type="Int32" />
                    <asp:ControlParameter ControlID="ddlStoreID" Name="storeID" PropertyName="SelectedValue"
                        Type="Int32" DefaultValue="1" />
                    <asp:ControlParameter ControlID="rblFilterByStore" Name="FilterByStore" PropertyName="SelectedValue"
                        Type="Byte" DefaultValue="0" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter Direction="Input" Name="RatingID" Type="Int32" />
                    <asp:Parameter Direction="Input" Name="Rating" Type="Int32" />
                    <asp:Parameter Direction="Input" Name="Comments" Type="String" />
                    <asp:Parameter Direction="Input" Name="IsFilthy" Type="Boolean" />
                </UpdateParameters>
                <DeleteParameters>
                    <asp:Parameter Direction="Input" Name="RatingID" Type="Int32" />
                </DeleteParameters>
            </asp:SqlDataSource>
    </div>
</asp:Content>
