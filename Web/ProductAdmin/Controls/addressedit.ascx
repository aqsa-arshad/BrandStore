<%@ Control Language="C#" AutoEventWireup="true" CodeFile="addressedit.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.AddressEdit" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<asp:UpdatePanel ID="updAddressList" runat="server" RenderMode="Inline" UpdateMode="Conditional"
    ChildrenAsTriggers="false" Visible="true">
    <ContentTemplate>
        <div style="display:none;">
            <asp:Button ID="btnHiddenCloseAddress" CausesValidation="false" runat="server" />
        </div>
        <%-- ADDRESS EDIT PANEL --%>
        <asp:Panel ID="pnlEditAddresses" runat="server" Width="500px" Height="525px" style="z-index:999999;" ScrollBars="None">
            <asp:UpdatePanel ID="updAddressEdit" runat="server" RenderMode="Inline" UpdateMode="Conditional"
                ChildrenAsTriggers="true">
                <ContentTemplate>
                
                    <div class="modal_popup">
                        <div class="modal_popup_Content">
                        <asp:Panel ID="pnlDetailsHolder" runat="server" BorderStyle="Solid" BorderColor="#DDDDDD" ScrollBars="Vertical">
                            <asp:DetailsView ID="dtlAddressList" runat="server" Height="400px" Width="400px" 
                                AllowPaging="True" AutoGenerateRows="False" AlternatingRowStyle-Height="22px" AlternatingRowStyle-BorderStyle="None" RowStyle-BorderStyle="None"
                                AlternatingRowStyle-HorizontalAlign="Left" RowStyle-HorizontalAlign="Left" RowStyle-Height="22px"
                                DataSourceID="sqlAddressList" DataKeyNames="AddressID" OnItemInserting="dtlAddressList_OnItemInserting"
                                OnDataBound="dtlAddressList_OnDataBound" OnItemInserted="dtlAddressList_OnInserted"
                                OnItemDeleted="dtlAddressList_OnDeleted" OnItemUpdated="dtlAddressList_OnUpdated"
                                OnItemUpdating="dtlAddressList_OnItemUpdating" OnModeChanged="dtlAddressList_OnModeChanged">
                                <RowStyle Height="22px" HorizontalAlign="Left" />
                                <Fields>
                                    <%-- STATE SELECT DROPDOWN --%>
                                    <asp:TemplateField SortExpression="NickName" HeaderText="<%$ Tokens:StringResource, admin.editaddress.NickName %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtNickName" runat="server" Text='<%# Bind("NickName") %>'></asp:TextBox>
                                            <%--<asp:RequiredFieldValidator ID="vldNickName" runat="server" CssClass="errorMsg" ErrorMessage="<b>!!</b>" ControlToValidate="txtNickName" />--%>
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="txtNickName" runat="server" Text='<%# Bind("NickName") %>'></asp:TextBox>
                                            <%--<asp:RequiredFieldValidator ID="vldNickName" runat="server" CssClass="errorMsg" ErrorMessage="<b>!!</b>" ControlToValidate="txtNickName" />--%>
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblNickName" runat="server" Text='<%# Bind("NickName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="FirstName" HeaderText="<%$ Tokens:StringResource, admin.editaddress.FirstName %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtFirstName" runat="server" Text='<%# Bind("FirstName") %>'></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="vldFirstName" runat="server" CssClass="errorMsg"
                                                ErrorMessage="<b>!!</b>" ControlToValidate="txtFirstName" />
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="txtFirstName" runat="server" Text='<%# Bind("FirstName") %>'></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="vldFirstName" runat="server" CssClass="errorMsg"
                                                ErrorMessage="<b>!!</b>" ControlToValidate="txtFirstName" />
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label2" runat="server" Text='<%# Bind("FirstName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="LastName" HeaderText="<%$ Tokens:StringResource, admin.editaddress.LastName %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtLastName" runat="server" Text='<%# Bind("LastName") %>'></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="vldLastName" runat="server" CssClass="errorMsg" ErrorMessage="<b>!!</b>"
                                                ControlToValidate="txtLastName" />
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="txtLastName" runat="server" Text='<%# Bind("LastName") %>'></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="vldLastName" runat="server" CssClass="errorMsg" ErrorMessage="<b>!!</b>"
                                                ControlToValidate="txtLastName" />
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label3" runat="server" Text='<%# Bind("LastName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="Company" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Company %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox4" runat="server" Text='<%# Bind("Company") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="TextBox4" runat="server" Text='<%# Bind("Company") %>'></asp:TextBox>
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label4" runat="server" Text='<%# Bind("Company") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="Address1" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Address1 %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtAddress1" runat="server" Text='<%# Bind("Address1") %>'></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="vldAddress1" runat="server" CssClass="errorMsg" ErrorMessage="<b>!!</b>"
                                                ControlToValidate="txtAddress1" />
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="txtAddress1" runat="server" Text='<%# Bind("Address1") %>'></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="vldAddress1" runat="server" CssClass="errorMsg" ErrorMessage="<b>!!</b>"
                                                ControlToValidate="txtAddress1" />
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label5" runat="server" Text='<%# Bind("Address1") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="Address2" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Address2 %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox6" runat="server" Text='<%# Bind("Address2") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="TextBox6" runat="server" Text='<%# Bind("Address2") %>'></asp:TextBox>
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label6" runat="server" Text='<%# Bind("Address2") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="Suite" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Suite %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox7" runat="server" Text='<%# Bind("Suite") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="TextBox7" runat="server" Text='<%# Bind("Suite") %>'></asp:TextBox>
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label7" runat="server" Text='<%# Bind("Suite") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="City" HeaderText="<%$ Tokens:StringResource, admin.editaddress.City %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtCity" runat="server" Text='<%# Bind("City") %>'></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="vldCity" runat="server" CssClass="errorMsg" ErrorMessage="<b>!!</b>"
                                                ControlToValidate="txtCity" />
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="txtCity" runat="server" Text='<%# Bind("City") %>'></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="vldCity" runat="server" CssClass="errorMsg" ErrorMessage="<b>!!</b>"
                                                ControlToValidate="txtCity" />
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label8" runat="server" Text='<%# Bind("City") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.editaddress.State%>">
                                        <ItemTemplate>
                                            <asp:Literal runat="server" ID="ltlState" Text='<%# Eval("State") %>' />
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:DropDownList runat="server" ID="ddlState" />
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:DropDownList runat="server" ID="ddlState" />
                                        </InsertItemTemplate>
                                    </asp:TemplateField>
                                    <%-- END STATE SELECT DROPDOWN --%>
                                    <%-- COUNTRY SELECT DROPDOWN --%>
                                    <asp:TemplateField SortExpression="Zip" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Zip %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox9" runat="server" Text='<%# Bind("Zip") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="TextBox9" runat="server" Text='<%# Bind("Zip") %>'></asp:TextBox>
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label9" runat="server" Text='<%# Bind("Zip") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="<%$ Tokens:StringResource, admin.editaddress.Country%>">
                                        <ItemTemplate>
                                            <asp:Literal runat="server" ID="ltlCountry" Text='<%# Eval("Country") %>' />
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:DropDownList runat="server" ID="ddlCountry" AutoPostBack="True" OnSelectedIndexChanged="ddlCountry_OnSelectedIndexChanged" />
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:DropDownList runat="server" ID="ddlCountry" AutoPostBack="True" OnSelectedIndexChanged="ddlCountry_OnSelectedIndexChanged" />
                                        </InsertItemTemplate>
                                    </asp:TemplateField>
                                    <%-- END COUNTRY SELECT DROPDOWN --%>
                                    <asp:TemplateField SortExpression="ResidenceType" HeaderText="<%$ Tokens:StringResource, admin.editaddress.ResidenceType %>">
                                        <EditItemTemplate>
                                            <asp:DropDownList ID="ddlResidenceType" runat="server" />
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:DropDownList ID="ddlResidenceType" runat="server" />
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="lblResidenceType" runat="server"></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="Phone" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Phone %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox11" runat="server" Text='<%# Bind("Phone") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="TextBox11" runat="server" Text='<%# Bind("Phone") %>'></asp:TextBox>
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label11" runat="server" Text='<%# Bind("Phone") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField SortExpression="Email" HeaderText="<%$ Tokens:StringResource, admin.editaddress.Email %>">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox12" runat="server" Text='<%# Bind("Email") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:TextBox ID="TextBox12" runat="server" Text='<%# Bind("Email") %>'></asp:TextBox>
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label12" runat="server" Text='<%# Bind("Email") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" ShowInsertButton="True"
                                        ButtonType="Button" NewText="<%$ Tokens:StringResource, admin.common.AddNew %>"
                                        DeleteText="<%$ Tokens:StringResource, admin.common.Delete %>" EditText="<%$ Tokens:StringResource, admin.common.Edit %>"
                                        CancelText="<%$ Tokens:StringResource, admin.common.Cancel %>" InsertText="<%$ Tokens:StringResource, admin.common.Add %>"
                                        UpdateText="<%$ Tokens:StringResource, admin.common.Update %>">
                                        <ControlStyle CssClass="normalButtons" />
                                    </asp:CommandField>
                                </Fields>
                                <AlternatingRowStyle Height="22px" HorizontalAlign="Left" />
                            </asp:DetailsView>
                        </asp:Panel>
                        </div>
                    </div>
                    <div align="center">
                        <asp:Button runat="server" ID="btnMakeBilling" Text="<%$ Tokens:StringResource, admin.editaddress.MakePrimaryBilling %>" OnClick="btnMakeBilling_OnClick" CssClass="normalButtons" Width="200px" />&nbsp; &nbsp;
                        <asp:Button runat="server" ID="btnMakeShipping" Text="<%$ Tokens:StringResource, admin.editaddress.MakePrimaryShipping %>" OnClick="btnMakeShipping_OnClick" CssClass="normalButtons" Width="200px" />
                        <br />
                        <br />
                        <asp:Label ID="lblPrimaryChanged" runat="server" Visible="false" />
                        <br />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div class="modal_popup" align="center">
                <div class="modal_popup_Footer">
                    <asp:Button class="normalButtons" runat="server" ID="btnClosePopup" CausesValidation="false" OnClick="btnClosePopup_Click" Text="<%$ Tokens:StringResource, admin.customer.EditCustomer %>" />
                </div>
            </div>
        </asp:Panel>
        <%-- END ADDRESS EDIT PANEL --%>
        <asp:SqlDataSource ID="sqlAddressList" runat="server"
            DeleteCommand="aspdnsf_delAddressByID" DeleteCommandType="StoredProcedure" InsertCommand="aspdnsf_insAddress"
            InsertCommandType="StoredProcedure" ProviderName="System.Data.SqlClient" SelectCommand="aspdnsf_getAddressesByCustomer"
            SelectCommandType="StoredProcedure" UpdateCommand="aspdnsf_updAddress" UpdateCommandType="StoredProcedure"
            OnSelecting="sqlAddressList_OnSelecting">
            <SelectParameters>
                <asp:Parameter Name="CustomerID" Type="Int32" />
            </SelectParameters>
            <DeleteParameters>
                <asp:Parameter Name="AddressID" Type="Int32" />
            </DeleteParameters>
            <UpdateParameters>
                <asp:Parameter Name="AddressID" Type="Int32" />
                <asp:Parameter Name="NickName" Type="String" />
                <asp:Parameter Name="FirstName" Type="String" />
                <asp:Parameter Name="LastName" Type="String" />
                <asp:Parameter Name="Company" Type="String" />
                <asp:Parameter Name="Address1" Type="String" />
                <asp:Parameter Name="Address2" Type="String" />
                <asp:Parameter Name="Suite" Type="String" />
                <asp:Parameter Name="City" Type="String" />
                <asp:Parameter Name="Zip" Type="String" />
                <asp:Parameter Name="Phone" Type="String" />
                <asp:Parameter Name="Email" Type="String" />
            </UpdateParameters>
            <InsertParameters>
                <asp:Parameter Name="NickName" Type="String" />
                <asp:Parameter Name="FirstName" Type="String" />
                <asp:Parameter Name="LastName" Type="String" />
                <asp:Parameter Name="Company" Type="String" />
                <asp:Parameter Name="Address1" Type="String" />
                <asp:Parameter Name="Address2" Type="String" />
                <asp:Parameter Name="Suite" Type="String" />
                <asp:Parameter Name="City" Type="String" />
                <asp:Parameter Name="Zip" Type="String" />
                <asp:Parameter Name="Phone" Type="String" />
                <asp:Parameter Name="Email" Type="String" />
            </InsertParameters>
        </asp:SqlDataSource>
    </ContentTemplate>
</asp:UpdatePanel>
