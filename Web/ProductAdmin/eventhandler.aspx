<%@ Page Language="C#" AutoEventWireup="true" CodeFile="eventhandler.aspx.cs" Inherits="AspDotNetStorefrontAdmin.eventhandler" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<script type="text/javascript">
    function getDelete() {
        return 'Confirm Delete';
    }
</script>
    <div>
        <div id="">
            <div style="margin-top: 5px; margin-bottom: 5px">
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
                                    <td class="titleTable" width="130" style="height: 18px">
                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.AddNew %>" />:
                                    </td>
                                    <td style="width: 5px; height: 18px;">
                                    </td>
                                    <td style="width: 1px; background-color: #a2a2a2; height: 18px;">
                                    </td>
                                    <td style="width: 5px; height: 18px;">
                                    </td>
                                    <td class="titleTable" style="height: 18px">
                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.eventhandler.EventHandlerVariables %>" />:
                                    </td>
                                </tr>
                                <tr>
                                    <td class="contentTableNP" valign="top" width="130">
                                        <asp:Panel ID="pnlAdd" runat="server" DefaultButton="btnInsert">
                                            <asp:TextBox ID="txtAddName" runat="server" CssClass="default" Width="140" Text="<%$Tokens:StringResource, admin.eventhandler.EventHandlerName %>" />
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtAddName" Display="dynamic" ErrorMessage="!!"></asp:RequiredFieldValidator><br />
                                            <asp:DropDownList ID="ddAddXmlPackage" runat="server" CssClass="default" Width="200px" OnSelectedIndexChanged="ddAddXmlPackage_SelectedIndexChanged"></asp:DropDownList>
                                            <asp:TextBox ID="txtAddURL" runat="server" CssClass="default" Width="140" Text="<%$Tokens:StringResource, admin.eventhandler.CalloutUrl %>" />
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ControlToValidate="txtAddURL" Display="dynamic" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidatorMessage %>"></asp:RequiredFieldValidator>
                                            <asp:Button ID="btnInsert" runat="server" CssClass="normalButtons" OnClick="btnInsert_Click" Text="<%$Tokens:StringResource, admin.common.Add %>" />
                                        </asp:Panel>
                                        <br />
                                        <br />
                                        <br />
                                    </td>
                                    <td style="width: 5px">
                                    </td>
                                    <td style="width: 1px; background-color: #a2a2a2">
                                    </td>
                                    <td style="width: 5px">
                                    </td>
                                    <td class="contentTable" valign="top" width="*">
                                        <div class="wrapperLeft">
                                            <asp:GridView ID="gMain" runat="server" AllowPaging="True" AllowSorting="True" AutoGenerateColumns="False" CssClass="tableoverallGrid" HorizontalAlign="Left" OnPageIndexChanging="gMain_PageIndexChanging" OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnRowEditing="gMain_RowEditing" OnRowUpdating="gMain_RowUpdating" OnSorting="gMain_Sorting" PagerSettings-Position="TopAndBottom" PagerStyle-HorizontalAlign="left" PageSize="15" Width="100%" GridLines="None" BorderStyle="None" BorderWidth="0px" CellPadding="0" ShowFooter="True">
                                                <Columns>
                                                    <asp:CommandField ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif"
                                                        EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" />
                                                    <asp:BoundField DataField="EventID" HeaderText="<%$Tokens:StringResource, admin.common.ID %>"
                                                        ReadOnly="True" SortExpression="EventID" >
                                                        <ItemStyle CssClass="lighterData" />
                                                    </asp:BoundField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.eventhandler.EventName %>" SortExpression="EventName">
                                                        <ItemTemplate>
                                                            <%# DataBinder.Eval(Container.DataItem, "EventName") %>
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <asp:TextBox ID="txtEventName" runat="Server" CssClass="singleAuto" Text='<%# DataBinder.Eval(Container.DataItem, "EventName") %>'></asp:TextBox>
                                                            <asp:Literal ID="ltEventName" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "EventName") %>'></asp:Literal>
                                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ControlToValidate="txtEventName"
                                                                ErrorMessage="<%$Tokens:StringResource, admin.common.ValidatorMessage %>"></asp:RequiredFieldValidator>
                                                        </EditItemTemplate>
                                                        <ItemStyle CssClass="normalData" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.eventhandler.XMLpackage %>">
                                                        <ItemTemplate>
                                                            <div style="white-space: normal; overflow: visible; width: 100%;">
                                                                <%# DataBinder.Eval(Container.DataItem, "XmlPackage") %>
                                                            </div>
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <asp:DropDownList ID="ddEditXMLPackage" runat="server" CssClass="default"></asp:DropDownList>
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Or %>" />
                                                            <asp:TextBox ID="txtXMLPackage" runat="Server" CssClass="default"></asp:TextBox>
                                                        </EditItemTemplate>
                                                        <ItemStyle CssClass="lightestData" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.eventhandler.CalloutUrl %>">
                                                        <ItemTemplate>
                                                            <div style="white-space: normal; overflow: visible; width: 225px;">
                                                                <%# DataBinder.Eval(Container.DataItem, "CalloutURL") %>
                                                            </div>
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <asp:TextBox ID="txtCalloutURL" runat="server" CssClass="singleAuto" Text='<%# DataBinder.Eval(Container.DataItem, "CalloutURL") %>'
                                                                TextMode="SingleLine"></asp:TextBox>
                                                        </EditItemTemplate>
                                                        <ItemStyle CssClass="normalData" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Active %>">
                                                        <ItemTemplate>
                                                            <div style="white-space: normal; overflow: visible; width: 225px;">
                                                                <%# DataBinder.Eval(Container.DataItem, "Active") %>
                                                            </div>
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <asp:CheckBox ID="cbkActive" runat="server" CssClass="singleAuto" Checked='<%# DataBinder.Eval(Container.DataItem, "Active") %>'></asp:CheckBox>
                                                        </EditItemTemplate>
                                                        <ItemStyle CssClass="normalData" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Debug %>">
                                                        <ItemTemplate>
                                                            <div style="white-space: normal; overflow: visible; width: 225px;">
                                                                <%# DataBinder.Eval(Container.DataItem, "Debug") %>
                                                            </div>
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <asp:CheckBox ID="cbkDebug" runat="server" CssClass="singleAuto" Checked='<%# DataBinder.Eval(Container.DataItem, "Debug") %>'></asp:CheckBox>
                                                        </EditItemTemplate>
                                                        <ItemStyle CssClass="normalData" />
                                                    </asp:TemplateField>
                                                    <asp:TemplateField>
                                                        <ItemTemplate>
                                                            <asp:ImageButton ID="imgDelete" runat="Server" AlternateText="<%$Tokens:StringResource, admin.common.Delete %>" CommandArgument='<%# Eval("EventID") %>'
                                                                CommandName="DeleteItem" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />
                                                        </ItemTemplate>
                                                        <ItemStyle CssClass="selectData" />
                                                    </asp:TemplateField>
                                                </Columns>
                                                <PagerSettings FirstPageText="<%$Tokens:StringResource, admin.common.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.common.LastPage %>"
                                                    Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                                <FooterStyle CssClass="gridFooter" />
                                                <RowStyle CssClass="gridRowPlain" />
                                                <EditRowStyle CssClass="gridEdit2" />
                                                <PagerStyle CssClass="tablepagerGrid" HorizontalAlign="Left" />
                                                <HeaderStyle CssClass="gridHeader" />
                                                <AlternatingRowStyle CssClass="gridAlternatingRowPlain" />
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
    
    </div>
</asp:Content>