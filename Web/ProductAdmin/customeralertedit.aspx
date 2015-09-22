<%@ Page Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" AutoEventWireup="true" CodeFile="customeralertedit.aspx.cs" Inherits="AspDotNetStorefrontAdmin.customeralertedit" MaintainScrollPositionOnPostback="true" %>

<%@ Register TagPrefix="asdnsf" Namespace="eWorld.UI" Assembly="eWorld.UI" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="Server">

    <table width="100%" cellpadding="4" cellspacing="4" border="0">
        <tr>
            <td colspan="3">
                <asp:Label ID="lblHeading" runat="server" Font-Bold="true"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="3" valign="top">Please enter the following information about this Customer Alert. Fields marked with an asterisk (*) are required. All other fields are optional.
            </td>
        </tr>
        <tr>
            <td>
                <br />
            </td>
        </tr>
        <tr>
            <td style="text-align: right; width: 15%">
                <asp:Label ID="lblCustomerLevel" runat="server" Text="*Customer Level:"></asp:Label>
            </td>
            <td style="text-align: left; width: 35%">
                <asp:DropDownList ID="ddlCustomerLevel" runat="server" Width="70%"></asp:DropDownList>
            </td>
            <td align="left" valign="top">
                <small>Select the customer level. This alert will be sent to all the user of selected customer level.</small>
            </td>
        </tr>
        <tr>
            <td style="text-align: right; width: 15%">
                <asp:Label ID="lblTitle" runat="server" Text="*Title:"></asp:Label>
            </td>
            <td style="text-align: left; width: 30%">
                <asp:HiddenField ID="hfCustomerAlertID" runat="server" />
                <asp:TextBox ID="txtTitle" runat="server" MaxLength="100" Width="70%" CssClass="TXTFIELD"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ValidationGroup="CustomerAlertEdit" runat="server" ControlToValidate="txtTitle"
                    ErrorMessage="<br/>Please enter title here." ForeColor="Red" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </td>
            <td align="left" valign="top">
                <small>Enter the title of customer alert. Maximum character limit is 100.</small>
            </td>
        </tr>
        <tr>
            <td style="text-align: right; width: 15%">
                <asp:Label ID="lblDescription" runat="server" Text="*Description:"></asp:Label>
            </td>
            <td style="text-align: left; width: 35%">
                <asp:TextBox ID="txtDescription" runat="server" MaxLength="250" Width="100%"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ValidationGroup="CustomerAlertEdit" runat="server" ControlToValidate="txtDescription"
                    ErrorMessage="<br/>Please enter description here." ForeColor="Red" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </td>
            <td align="left" valign="top">
                <small>Enter the description of customer alert. Maximum character limit is 250.</small>
            </td>
        </tr>
        <tr>
            <td align="right">
                <asp:Label ID="lblAlertDate" runat="server" Text="*Alert Date:"></asp:Label>
            </td>
            <td align="left">
                <asdnsf:CalendarPopup ID="txtAlertDate" runat="server" Height="20px" DisableTextBoxEntry="True" AllowArbitraryText="False"
                    PadSingleDigits="True" Nullable="false" CalendarWidth="200" Width="80px" ShowGoToToday="True" ImageUrl="~/App_Themes/Admin_Default/images/calendar.gif"
                    Font-Size="9px">
                </asdnsf:CalendarPopup>
            </td>
            <td align="left" valign="top">
                <small>Select the alert date. This alert will be visible on the selected alert date.</small>
            </td>
        </tr>
        <tr>
            <td></td>
            <td>
                <br />
                <asp:Panel ID="pnlNewAlert" runat="server" Visible="false">
                    <asp:Button ValidationGroup="CustomerAlertEdit" CausesValidation="True"
                        ID="btnAddAlert" runat="server" CssClass="normalButtons" Text="Add Alert" OnClick="btnAddAlert_Click" />
                    <asp:Button ID="btnCancel" runat="server" CssClass="normalButtons" Text="Cancel" OnClick="btnCancel_Click" />
                </asp:Panel>
                <asp:Panel ID="pnlEditAlert" runat="server" Visible="false">
                    <asp:Button ValidationGroup="CustomerAlertEdit" CausesValidation="True" 
                        ID="btnUpdate" runat="server" CssClass="normalButtons" Text="Update Alert" OnClick="btnUpdate_Click" />
                    <asp:Button ID="btnReset" runat="server" CssClass="normalButtons" Text="Reset" OnClick="btnReset_Click" />
                </asp:Panel>
            </td>
            <td></td>
        </tr>
    </table>

</asp:Content>

