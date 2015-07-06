<%@ Page Language="C#" AutoEventWireup="true" CodeFile="rtshippingmgr.aspx.cs" Inherits="AspDotNetStorefrontAdmin.RTShippingMGR" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <p></p>
    <div align="left">
        <span class="SectionTitleText">
            <asp:Literal ID="litSectionTitle" runat="server"></asp:Literal>
            <asp:Label ID="lblInfo" runat="server" Text="Label"></asp:Label>
        </span>
    </div>
    <p></p>
        <asp:Panel ID="pnlMain" runat="server" Width="100%">
            <table width="100%" cellpadding="0" cellspacing="0">
                <tr>
                    <td>
                        <asp:GridView ID="grdProviders" runat="server" Width="100%" AutoGenerateColumns="False" >
                            <Columns>
                                <asp:CommandField ButtonType="Button" ShowDeleteButton="True" ShowEditButton="True" />
                                <asp:TemplateField HeaderText="ID">
                                    <EditItemTemplate>
                                        <asp:Label ID="lblID" runat="server" Text='<%# Eval("RTShippingProviderID") %>'></asp:Label>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblID" runat="server" Text='<%# Bind("RTShippingProviderID") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Name">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="colName" runat="server" Text='<%# Bind("Name") %>'></asp:TextBox>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="lblName" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Enabled">
                                    <EditItemTemplate>
                                        <asp:CheckBox ID="chkEnabled" runat="server" Checked='<%# Bind("Enabled") %>' />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkEnabled" runat="server" Checked='<%# Bind("Enabled") %>' Enabled="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Use Live Mode">
                                    <EditItemTemplate>
                                        <asp:CheckBox ID="chkUseLiveMode" runat="server" Checked='<%# Bind("UseLiveMode") %>' />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkUseLiveMode" runat="server" Checked='<%# Bind("UseLiveMode") %>'
                                            Enabled="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="XmlSpecificationFile" HeaderText="Provider Specification File"  ReadOnly="True"/>
                                <asp:TemplateField HeaderText="User Data" ShowHeader="False">
                                    <ItemTemplate>
                                        <asp:Button ID="btnUserData" runat="server" CausesValidation="false" CommandName="UserData" Text="Edit" CommandArgument='<%# Bind("XmlSpecificationFile") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Map Countries" ShowHeader="False">
                                    <ItemTemplate>
                                        <asp:Button ID="btnMap" runat="server" CausesValidation="false" CommandName="Map" Text="Map"  CommandArgument='<%# Bind("RtShippingProviderID") %>'/>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </td>
                </tr>
                <tr>
                    <td style="height: 26px" align="right">
                        <asp:Button ID="btnScan" CssClass="normalButtons" runat="server" Text="Re-Scan" OnClick="btnScan_Click" />
                    </td>
                </tr>
            </table>   
        </asp:Panel>
</asp:Content>