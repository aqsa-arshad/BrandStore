<%@ Page language="c#" Inherits="AspDotNetStorefront.requestcatalog" CodeFile="requestcatalog.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    
         <asp:Panel ID="pnlCatalogRequest" HorizontalAlign="left" runat="server">
            <table>
                <tr><td colspan="2"><asp:Label ID="requestcatalog_aspx_7" runat="server" Font-Bold="true"></asp:Label></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_8" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:TextBox ID="txtFirstName" runat="server" MaxLength="50" Columns="35" CausesValidation="true"></asp:TextBox><asp:RequiredFieldValidator ID="reqFName" runat="server" ControlToValidate="txtFirstName" EnableClientScript="true" Display="None"></asp:RequiredFieldValidator></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_10" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:TextBox ID="txtLastName" runat="server" MaxLength="50" Columns="35" CausesValidation="true"></asp:TextBox><asp:RequiredFieldValidator ID="reqLName" runat="server" ControlToValidate="txtLastName" EnableClientScript="true" Display="None"></asp:RequiredFieldValidator></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_12" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:TextBox ID="txtCompany" runat="server" MaxLength="50" Columns="35"></asp:TextBox></td></tr>
                <tr><td><asp:Literal ID="address_cs_58" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:DropDownList ID="ddlShippingResidenceType" runat="server"></asp:DropDownList></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_13" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:TextBox ID="txtAddr1" runat="server" MaxLength="100" Columns="35" CausesValidation="true"></asp:TextBox><asp:RequiredFieldValidator ID="reqAddr1" runat="server" ControlToValidate="txtAddr1" EnableClientScript="true" Display="None"></asp:RequiredFieldValidator></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_15" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:TextBox ID="txtAddr2" runat="server" MaxLength="100" Columns="35"></asp:TextBox></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_16" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:TextBox ID="txtSuite" runat="server" MaxLength="50" Columns="35"></asp:TextBox></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_17" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:TextBox ID="txtCity" runat="server" MaxLength="50" Columns="35" CausesValidation="true"></asp:TextBox><asp:RequiredFieldValidator ID="reqCity" runat="server" ControlToValidate="txtCity" EnableClientScript="true" Display="None"></asp:RequiredFieldValidator></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_19" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:DropDownList ID="ddlState" runat="server"></asp:DropDownList></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_21" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:TextBox ID="txtZip" runat="server" MaxLength="10" Columns="35" CausesValidation="true"></asp:TextBox><asp:RequiredFieldValidator ID="reqZip" runat="server" ControlToValidate="txtZip" EnableClientScript="true" Display="None"></asp:RequiredFieldValidator></td></tr>
                <tr><td><asp:Literal ID="requestcatalog_aspx_24" runat="server" Mode="PassThrough"></asp:Literal></td><td><asp:DropDownList ID="ddlCountry" runat="server"></asp:DropDownList></td></tr>
                <tr><td colspan="2" height="25"></td></tr>
                <tr><td colspan="2" align="center"><asp:Button ID="btnContinue" runat="server" OnClick="btnContinue_OnClick" /></td></tr>
                <tr><td colspan="2" height="25"><asp:CustomValidator ID="reqState" ControlToValidate="ddlState" OnServerValidate="reqState_OnServerValidate" runat="server" ErrorMessage="Please select a state"></asp:CustomValidator></td></tr>
                <tr><td colspan="2"><asp:ValidationSummary ID="valSummary" ForeColor="Red" Font-Bold="true" DisplayMode="List" ShowSummary="true" runat="server" EnableClientScript="true" /></td></tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
            <asp:Label ID="lblSuccess" runat="server" Font-Bold="true"></asp:Label>
        </asp:Panel>
    
    </asp:Panel>
</asp:Content>

