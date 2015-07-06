<%@ Page Language="C#" AutoEventWireup="true" CodeFile="giftcardusage.aspx.cs" Inherits="AspDotNetStorefrontAdmin.giftcardusage" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>


<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div id="Div1">
            <table width="100%" border="0" cellspacing="0" cellpadding="0" class="toppage">
                  <tr>
                    <td align="left" valign="middle">
	                        <table border="0" cellspacing="0" cellpadding="5">
                                <tr>
                                    <td align="left" valign="middle">
                                        <asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.giftcardusage.Serial %>" />
                                        <span style="color: Red;"><asp:Literal ID="ltCard" runat="server"></asp:Literal></span>
                                        &nbsp;
                                    </td>
                                </tr>
                            </table>
	                </td>
                  </tr>
            </table>
            <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>    
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>                                
                                <td class="titleTable" width="100%">
                                    <font class="subTitle"><asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.giftcardusage.Usage %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">
                                        <br />
                                        <b>
                                            <asp:Literal ID="Literal3" runat="server" Text="<%$Tokens:StringResource, admin.giftcardusage.Balance %>" />
                                            &nbsp;&nbsp;&nbsp;&nbsp;<asp:Literal ID="ltBalance" runat="server"></asp:Literal>
                                        </b>
                                        <br />
                                        <asp:Literal ID="Literal4" runat="server" Text="<%$Tokens:StringResource, admin.giftcardusage.Adjust %>" />
                                        <asp:DropDownList ID="ddUsage" runat="server" CssClass="default" EnableViewState="False">
                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.common.Select %>" />
                                            <asp:ListItem Value="3" Text="<%$Tokens:StringResource, admin.giftcardusage.AddFunds %>" />
                                            <asp:ListItem Value="4" Text="<%$Tokens:StringResource, admin.giftcardusage.DecrementFunds %>" />
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator" InitialValue="-1" Display="Dynamic" ControlToValidate="ddUsage" runat="server"></asp:RequiredFieldValidator>
                                        &nbsp;
                                        <asp:Literal ID="Literal5" runat="server" Text="<%$Tokens:StringResource, admin.giftcardusage.Amount %>" />
                                        <asp:TextBox ID="txtUsage" runat="server" CssClass="singleShorter" EnableViewState="False"></asp:TextBox>
                                        <asp:RangeValidator MinimumValue="-9999" MaximumValue="9999" ID="rangeValidator" runat="server" Type="Currency" ControlToValidate="txtUsage">!</asp:RangeValidator><asp:RequiredFieldValidator ID="RequiredFieldValidator1" Display="Dynamic" ControlToValidate="txtUsage" runat="server"></asp:RequiredFieldValidator>
                                        <asp:Button ID="btnUsage" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.giftcardusage.AddUsage %>" runat="server" OnClick="btnUsage_Click" />
                                        <br /><br />
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="true" PageSize="50" AllowSorting="True" CssClass="overallGrid" 
                                            HorizontalAlign="Left" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" GridLines="None">
                                            <Columns>
						                        <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcardusage.ActivityReason %>" SortExpression="UsageTypeID" ItemStyle-CssClass="lighterData">
                                                    <ItemTemplate>
							                            <%# ((AspDotNetStorefrontCore.GiftCardUsageReasons)DataBinder.Eval(Container.DataItem, "UsageTypeID")).ToString() %>
						                            </ItemTemplate>
						                            <ItemStyle CssClass="lighterData"></ItemStyle>
						                        </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcardusage.ByCustomer %>" SortExpression="LastName" ItemStyle-CssClass="lightData">
                                                    <ItemTemplate>
							                            <span style='white-space: nowrap;'>
                                                            <%# (DataBinder.Eval(Container.DataItem, "FirstName") + " " + DataBinder.Eval(Container.DataItem, "LastName")).Trim() %>
                                                        </span>
						                            </ItemTemplate>
						                            <ItemStyle CssClass="lightData"></ItemStyle>
						                        </asp:TemplateField>
						                        <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcardusage.OrderNumber %>" SortExpression="OrderNumber" ItemStyle-CssClass="lighterData">
                                                    <ItemTemplate>
							                            <%# DataBinder.Eval(Container.DataItem, "OrderNumber")%>
						                            </ItemTemplate>
						                            <ItemStyle CssClass="lighterData"></ItemStyle>
						                        </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Amount %>" SortExpression="Amount" ItemStyle-CssClass="normalData">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="ltAmount" runat="server"></asp:Literal>
						                            </ItemTemplate>
						                            
						                            <ItemStyle HorizontalAlign="Right" CssClass="normalData"></ItemStyle>
						                        </asp:TemplateField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.giftcardusage.RecordDate %>" SortExpression="CreatedOn" ItemStyle-CssClass="normalData">
                                                    <ItemTemplate>
							                            <%# DataBinder.Eval(Container.DataItem, "CreatedOn")%> 
						                            </ItemTemplate>
						                            
						                            <ItemStyle HorizontalAlign="Center" CssClass="normalData"></ItemStyle>
						                        </asp:TemplateField>						                        
                                            </Columns>
                                            <PagerSettings FirstPageText="<%$Tokens:StringResource, admin.common.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.common.LastPage %>"
                                                Mode="NumericFirstLast" PageButtonCount="15" />
                                            <FooterStyle CssClass="footerGrid" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="DataCellGridEdit" />
                                            <PagerStyle CssClass="pagerGrid" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" />
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
</asp:Content>
