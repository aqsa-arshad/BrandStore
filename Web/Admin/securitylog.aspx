<%@ Page language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.securitylog" CodeFile="securitylog.aspx.cs" EnableEventValidation="false" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %> 

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

<table style="width:100%">
        <tr>
            <td style="width: 100%; color: red; padding-left: 15px; font-size: 10pt;" >
                <asp:Literal ID="lterror" runat="server"></asp:Literal>
            </td>
        </tr>
        <tr id="trLog" runat="server">
            <td style="width: 100%">
                <b></b><strong>Your SecurityLog is Shown Below for the Last 365 Days:&nbsp;
                    <asp:Button ID="Refresh" runat="server" OnClick="Refresh_Click" CssClass="normalButtons" Text="Refresh Page" /><br /></strong>
                <br />
                <asp:GridView ID="GridView1" runat="server" AllowPaging="True" OnRowDataBound="GridView1_RowDataBound1" PageSize="100" Width="100%" OnPageIndexChanging="GridView1_PageIndexChanging" GridLines="None" ShowFooter="True">
                    <HeaderStyle Font-Bold="True" Font-Size="XX-Small" HorizontalAlign="Left" VerticalAlign="Middle" CssClass="gridHeader" />
                    <RowStyle HorizontalAlign="Left" VerticalAlign="Top" CssClass="gridRowPlain" />
                    <AlternatingRowStyle CssClass="gridAlternatingRowPlain" />
                    <FooterStyle CssClass="gridFooter" />
                    <PagerStyle CssClass="gridPager" />
                </asp:GridView>
            </td>
        </tr>
   </table> 

</asp:Content>


