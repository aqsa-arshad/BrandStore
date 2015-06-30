<%@ Page Language="C#" AutoEventWireup="true" CodeFile="failedtransactions.aspx.cs" Inherits="AspDotNetStorefrontAdmin.failedtransactions"  MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" EnableEventValidation="false" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<asp:Panel ID="pnlContent" DefaultButton="btnGo" runat="server">
	<P>&nbsp;
		<TABLE id="Table1" cellSpacing="1" cellPadding="4" width="300" border="0">
			<TR>
				<TD height="32">
					<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.failedtransactions.ReportDate %>" />
					<asp:DropDownList id="DateSelect" runat="server"></asp:DropDownList>&nbsp;
					<asp:Button ID="btnGo" CssClass="normalButtons" runat="server" Text="<%$Tokens:StringResource, admin.common.Go %>" OnClick="btnGo_Click" />
					</TD>
			</TR>
			<TR>
				<TD>
					<asp:DataGrid id="DataGrid1" runat="server" CellPadding="4" BorderColor="White" BorderStyle="Solid" BorderWidth="0px" GridLines="None" CssClass="tableoverallGrid">
<FooterStyle Width="0px" CssClass="gridFooter"></FooterStyle>
<SelectedItemStyle Width="0px"></SelectedItemStyle>
<PagerStyle Width="0px"></PagerStyle>
<AlternatingItemStyle BorderWidth="0px" CssClass="tableDataCellGridAlt"></AlternatingItemStyle>
<ItemStyle Width="0px"></ItemStyle>
<HeaderStyle Width="0px" ForeColor="White" CssClass="gridHeader"></HeaderStyle>
</asp:DataGrid>
</TD>
			</TR>
		</TABLE>
	</P>
</asp:Panel>
</asp:Content>

