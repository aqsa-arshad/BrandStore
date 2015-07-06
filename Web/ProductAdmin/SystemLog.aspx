<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SystemLog.aspx.cs" Inherits="AspDotNetStorefrontAdmin.SystemLog" MaintainScrollPositionOnPostback="true" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<%-- Emulate IE 7 to resolve IE 8 compatibility problems with the radDatePicker --%>
<meta http-equiv="X-UA-Compatible" content="IE=EmulateIE7" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
    
    <%-- BREADCRUMB --%>
    <div>
        Now in : <asp:literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.menu.viewsystemlog %>" /> : <a href="sitemap2.aspx" style="color: #5AD3FF">Site Map</a>
    </div>
    <%-- END BREADCRUMB --%>
    <div>
        <asp:Panel runat="server" ID="pnlExportTitle" >
            <h1>
                <asp:Label runat="server" ID="lblTitle" Text="<%$Tokens:StringResource, admin.systemlog.aspx.11 %>" /></h1>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlExportContents">
        <asp:Label runat="server" ID="Label1" Text="<%$Tokens:StringResource, admin.systemlog.aspx.9 %>" />
        <asp:DropDownList ID="ddlSeverity" runat="server" AutoPostBack="false">
        </asp:DropDownList>
        <asp:Label runat="server" ID="Label2" Text="<%$Tokens:StringResource, admin.systemlog.aspx.10 %>" />
        <asp:DropDownList ID="ddlType" runat="server" AutoPostBack="false">
        </asp:DropDownList>
        <asp:Label runat="server" ID="Label3" Text="<%$Tokens:StringResource, admin.systemlog.aspx.7 %>"  />
            <telerik:RadDatePicker ID="dpStartDate" runat="server"> 
            </telerik:RadDatePicker>
            <asp:Label runat="server" ID="Label4" Text="<%$Tokens:StringResource, admin.systemlog.aspx.8 %>" />
            <telerik:RadDatePicker ID="dpEndDate" runat="server">
            </telerik:RadDatePicker>
            &nbsp; &nbsp;
            <asp:Button runat="server" Text="<%$Tokens:StringResource, admin.systemlog.aspx.12 %>" ID="btnExport" OnClick="btnExport_OnClick" class="normalButtons" />
            <asp:Button runat="server" Text="<%$Tokens:StringResource, admin.systemlog.aspx.13 %>" ID="btnClear" OnClick="btnClear_OnClick" class="normalButtons" />
        </asp:Panel>
        <div>
            <asp:UpdatePanel ID="RadAjaxPanel1" runat="server">
            <ContentTemplate>
            <telerik:RadGrid ID="RadGrid1" runat="server" AllowPaging="True" AllowSorting="True"
                    DataSourceID="SqlDataSource1" GridLines="None" PageSize="30"
                    OnItemDataBound="RadGrid1_ItemDataBound">
                    <HeaderContextMenu>
                        <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                    </HeaderContextMenu>
                    <ClientSettings>
                        <Resizing AllowColumnResize="true" EnableRealTimeResize="true" ResizeGridOnColumnResize="true" ClipCellContentOnResize="false" />
                    </ClientSettings>
                    <PagerStyle HorizontalAlign="Left" Mode="NextPrevNumericAndAdvanced" Position="TopAndBottom"
                        AlwaysVisible="True" />
                    <MasterTableView AutoGenerateColumns="False" DataKeyNames="SysLogID" DataSourceID="SqlDataSource1"
                        Name="MasterTable">
                        <DetailTables>
                            <telerik:GridTableView DataKeyNames="SysLogID" DataSourceID="SqlDataSource2" AllowMultiColumnSorting="false"
                                runat="server" AutoGenerateColumns="false" Width="100%" PagerStyle-Visible="False"
                                Name="DetailTable">
                                <ParentTableRelation>
                                    <telerik:GridRelationFields DetailKeyField="SysLogID" MasterKeyField="SysLogID" />
                                </ParentTableRelation>
                                <Columns>
                                    <telerik:GridBoundColumn HeaderText="<%$ Tokens:StringResource, admin.systemlog.aspx.3 %>"
                                        HeaderButtonType="TextButton" DataField="Details" UniqueName="Details">
                                    </telerik:GridBoundColumn>
                                </Columns>
                            </telerik:GridTableView>
                        </DetailTables>
                        <Columns>
                            <telerik:GridBoundColumn DataField="SysLogID" DataType="System.Int32" HeaderText="<%$Tokens:StringResource, admin.systemlog.aspx.1%>"
                                ReadOnly="True" SortExpression="SysLogID" UniqueName="SysLogID">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="CreatedOn" DataType="System.DateTime" HeaderText="<%$Tokens:StringResource, admin.systemlog.aspx.6 %>"
                                SortExpression="CreatedOn" UniqueName="CreatedOn">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Type" HeaderText="<%$Tokens:StringResource, admin.systemlog.aspx.4 %>"
                                SortExpression="Type" UniqueName="Type">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Severity" HeaderText="<%$ Tokens:StringResource, admin.systemlog.aspx.5 %>"
                                SortExpression="Severity" UniqueName="Severity">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Message" HeaderText="<%$ Tokens:StringResource, admin.systemlog.aspx.2 %>"
                                SortExpression="Message" UniqueName="Message">
                            </telerik:GridBoundColumn>
                        </Columns>
                    </MasterTableView>
                    <FilterMenu>
                        <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                    </FilterMenu>
                </telerik:RadGrid>
            </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ProviderName="System.Data.SqlClient">
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="SqlDataSource2" runat="server" ProviderName="System.Data.SqlClient">
            <SelectParameters>
                <asp:SessionParameter Name="SysLogID" SessionField="SysLogID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>
</asp:Content>
