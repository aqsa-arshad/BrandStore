<%@ Page Language="C#" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" AutoEventWireup="true" CodeFile="customeralerts.aspx.cs" Inherits="AspDotNetStorefrontAdmin.customeralerts" MaintainScrollPositionOnPostback="true" %>

<%@ Register TagPrefix="ew" Namespace="eWorld.UI" Assembly="eWorld.UI" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <table>
        <tr>
            <td>
                <label>
                    <asp:Label ID="Label1" runat="server" Text='Customer Name'></asp:Label>
                </label>
            </td>
            <td>
                <asp:DropDownList ID="CustomerDropDownList" name="CustomerName"
                    runat="server" MaxLength="100">
                </asp:DropDownList>
            </td>
        </tr>

        <tr>
            <td>
                <label>
                    <asp:Label ID="Label2" runat="server" Text='Title'></asp:Label>
                </label>
            </td>
            <td>
                <asp:TextBox ID="TitleVal" runat="server" name="Title" MaxLength="100"></asp:TextBox>
            </td>
            <td>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ValidationGroup="Group1" runat="server" ControlToValidate="TitleVal"
                    ErrorMessage="Please enter alert title here." ForeColor="Red" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </td>
        </tr>

        <tr>
            <td>
                <label>
                    <asp:Label ID="Label3" runat="server" Text='Description'></asp:Label>
                </label>
            </td>
            <td>
                <asp:TextBox ID="DescriptionVal" runat="server" name="Description" MaxLength="100"></asp:TextBox>
            </td>
            <td>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ValidationGroup="Group1" runat="server" ControlToValidate="DescriptionVal"
                    ErrorMessage="Please enter alert description here." ForeColor="Red" Display="Dynamic">
                </asp:RequiredFieldValidator>
            </td>
        </tr>

        <tr>
            <td>
                <label>
                    <asp:Label ID="Label4" runat="server" Text='Alert Date'></asp:Label>
                </label>
            </td>
            <td>
                <ew:CalendarPopup ID="AlertDateVal" runat="server" Height="20px" DisableTextBoxEntry="True" AllowArbitraryText="False"
                    PadSingleDigits="True" Nullable="True" CalendarWidth="200" Width="80px" ShowGoToToday="True" ImageUrl="~/App_Themes/Admin_Default/images/calendar.gif"
                    Font-Size="9px">
                </ew:CalendarPopup>
            </td>
        </tr>
        <tr>
            <th align="center" style="padding-left: 100px;" colspan="2">
                <asp:Button ValidationGroup="Group1" CausesValidation="True"
                    ID="btnAddAlert" runat="server" CssClass="normalButtons" Text="Add Alert" OnClick="btnAddAlert_Click"></asp:Button>

                <asp:Button ValidationGroup="Group1" CausesValidation="True"
                    ID="btnUpdate" runat="server" Visible="False" CssClass="normalButtons" CommandName="Abcdef" CommandArgument=''
                    Text="Update Alert" OnClick="btnUpdateAlert_Click"></asp:Button>
            </th>
        </tr>

    </table>

    <table border="0" cellpadding="0" cellspacing="0" width="100%">
        <asp:Repeater ID="rptCustomerAlerts" runat="server">
            <HeaderTemplate>
                <tr class="table-header">                    
                    <td style="padding: 10px">Customer Name</td>
                    <td>Title</td>
                    <td>Description</td>
                    <td>AlertDate</td>
                    <td>IsRead</td>
                    <td>IsDeleted</td>
                    <td >Actions</td>
                </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr class="table-row2">                    
                    <td style="padding: 10px"><%# Eval("CustomerName") %></td>
                    <td><%# Eval("Title") %></td>
                    <td><%# Eval("Description") %></td>
                    <td><%# Eval("AlertDate") %></td>
                    <td><%# Eval("IsRead") %></td>
                    <td><%# Eval("IsDeleted") %></td>                    
                    <td >
                        <asp:Button
                            ID="btnEdit" runat="server" CssClass="normalButtons" CommandName="Abcdef" CommandArgument='<%# Eval("CustomerAlertID") %>' Text="Edit" OnClick="btnEdit_Click"></asp:Button>
                        <asp:Button
                            ID="btnDelete" runat="server" CssClass="normalButtons" CommandName="Abcdef" CommandArgument='<%# Eval("CustomerAlertID") %>' Text="Delete" OnClick="btnDelete_Click"></asp:Button>
                    </td>

                </tr>
            </ItemTemplate>
            <AlternatingItemTemplate>
                <tr class="alternatingrow2">                    
                    <td style="padding: 10px"><%# Eval("CustomerName") %></td>
                    <td><%# Eval("Title") %></td>
                    <td><%# Eval("Description") %></td>
                    <td><%# Eval("AlertDate") %></td>
                    <td><%# Eval("IsRead") %></td>
                    <td><%# Eval("IsDeleted") %></td>                    
                    <td >
                        <asp:Button
                            ID="btnEdit" runat="server" CssClass="normalButtons" CommandName="Abcdef" CommandArgument='<%# Eval("CustomerAlertID") %>' Text="Edit" OnClick="btnEdit_Click"></asp:Button>
                        <asp:Button
                            ID="btnDelete" runat="server" CssClass="normalButtons" CommandName="Abcdef" CommandArgument='<%# Eval("CustomerAlertID") %>' Text="Delete" OnClick="btnDelete_Click"></asp:Button>
                    </td>                   
                </tr>
            </AlternatingItemTemplate>
        </asp:Repeater>
    </table>


    <asp:Repeater ID="rptPager" runat="server">
        <ItemTemplate>
            <asp:LinkButton ID="lnkPage" runat="server" Text='<%#Eval("Text") %>' CommandArgument='<%# Eval("Value") %>'
                OnClick="Page_Changed" OnClientClick='<%# !Convert.ToBoolean(Eval("Enabled")) ? "return false;" : "" %>'></asp:LinkButton>
        </ItemTemplate>
    </asp:Repeater>



</asp:Content>

