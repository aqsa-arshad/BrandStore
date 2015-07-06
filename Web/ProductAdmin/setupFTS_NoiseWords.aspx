<%@ Page Language="C#" AutoEventWireup="true" CodeFile="setupFTS_NoiseWords.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Admin_setupFTS_NoiseWords" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <script type="text/javascript">
    function WarnAddNewNoiseWord()
    {
        if (document.getElementById("txtNewNoiseWord").value != "")
        {
            if(confirm("<%=JSWarnAddNewNoiseWord %>" + ' "' + document.getElementById("txtNewNoiseWord").value + '"'))
            {
                return true;        
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }        
    }
    </script>
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    <div>
    <table style="border-width: medium; border-color: #C0C0C0; border-top-style: solid;
        border-right-style: solid; border-left-style: solid; background-color: #F8F8F8;" 
        width="100%">
        <tr>
            <td></td>
        </tr>
        <tr>
            <td style="width:33%">
            </td>
            <td style="width:34%; padding-top: 15px; padding-bottom: 10px;">
                <table style="border: thin solid #CACA00; background-color: #FFFFE8;" width="100%">
                    <tr style="width:100%" align="center">
                        <td align="center">
                        <asp:GridView ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom"
                        AutoGenerateColumns="False" AllowPaging="True" PageSize="25" 
                                CssClass="overallGrid" HorizontalAlign="Center"
                        OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound"
                        OnPageIndexChanging="gMain_PageIndexChanging" OnRowUpdating="gMain_RowUpdating" OnRowEditing="gMain_RowEditing"
                        Width="100%">
                            <Columns>
                                <asp:CommandField ItemStyle-Width="60" ButtonType="Image" 
                                    CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" CancelText="Cancel" 
                                    EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" EditText="Edit" 
                                    ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" 
                                    UpdateText="Update">
<ItemStyle Width="60px" CssClass=""></ItemStyle>
                                </asp:CommandField>
                                <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" 
                                    ItemStyle-CssClass="lighterData" Visible="false" >
<ItemStyle CssClass="lighterData"></ItemStyle>
                                </asp:BoundField>
                                <asp:TemplateField HeaderText="Noise Word">
                                    <ItemTemplate>
                                        <asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "word") %>'></asp:Literal>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox runat="server" ID="txtNewNoiseWord" name="txtNewNoiseWord" Text='<%# DataBinder.Eval(Container.DataItem, "EditWord") %>' BorderColor="DarkGray" ></asp:TextBox>
                                        <asp:Label runat="server" ID="lblNewNoiseWordID" Visible="false" Text='<%# Eval("ID") %>'></asp:Label>
                                    </EditItemTemplate>
                                </asp:TemplateField>                                                
                                <asp:TemplateField ItemStyle-CssClass="selectData" ItemStyle-Width="25">
                                    <ItemTemplate>
                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("ID") %>' runat="Server" AlternateText="Delete" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                    </ItemTemplate>

<ItemStyle CssClass="selectData" Width="25px"></ItemStyle>
                                </asp:TemplateField>
                                <asp:BoundField Visible="false" DataField="EditWord" ReadOnly="true" />                                
                            </Columns>
                            <PagerSettings FirstPageText="&amp;lt;&amp;lt;First Page" LastPageText="Last Page&amp;gt;&amp;gt;"
                               Mode="NumericFirstLast" PageButtonCount="25" />
                            <FooterStyle CssClass="footerGrid" />
                            <RowStyle CssClass="DataCellGrid" BackColor="#F8F8F8" />
                            <EditRowStyle CssClass="DataCellGridEdit" />
                            <PagerStyle CssClass="pagerGrid" />
                            <HeaderStyle BackColor="LightGray" HorizontalAlign="Center" Font-Names="Aerial" Height="10px" />
                            <AlternatingRowStyle CssClass="DataCellGridAlt" BackColor="#FFFFE8" />
                        </asp:GridView> 
                        </td>                       
                    </tr>
                </table>
            </td>
            <td style="width:33%">
            </td>
        </tr>
        <tr>
            <td></td>
        </tr>
    </table>            
    </div>
    <asp:Panel ID="pnlAdd" DefaultButton="btn_AddNewNoiseWord" runat="server">
    <div>
        <table style="border-width: medium; border-color: #C0C0C0; border-right-style: solid; border-left-style: solid; background-color: #F8F8F8; border-bottom-style: solid;"
            width="100%">            
            <tr align="center">
                <td style="width:28%">
                </td>
                <td style="padding-bottom: 15px; width:44%">
                    <asp:Button ID="btn_AddNewNoiseWord" CssClass="normalButtons" runat="server" Text="<%$ Tokens:StringResource,setupFTS.aspx.29 %>" 
                        OnClientClick="return WarnAddNewNoiseWord()" 
                        onclick="btn_AddNewNoiseWord_Click"
                        />&nbsp;
                    <asp:TextBox ID="txtNewNoiseWord" runat="server" BackColor="#FFFFE8" 
                        MaxLength="80" Width="250px"></asp:TextBox>
                </td>
                <td style=" text-align:right; width:28%; vertical-align:bottom;">
                    <asp:HyperLink ID="hyperNoiseWord" runat="server" NavigateUrl="setupFTS.aspx" 
                        Text="<%$ Tokens:StringResource,setupFTS.aspx.1 %>" Width="100%"></asp:HyperLink>
                </td>
            </tr>
        </table>
    </div>
    </asp:Panel>
</asp:Content>