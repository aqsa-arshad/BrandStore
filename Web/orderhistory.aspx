<%@ Page Language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefront.orderhistory" CodeFile="orderhistory.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:content runat="server" contentplaceholderid="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel ID="pnlOrderHistory" runat="server">			
        <div class="content-box-03 padding-top-none">
			<div>
				<table class="table">
					<tbody>						
						<asp:Repeater ID="rptOrderhistory" runat="server">							
							<ItemTemplate>
								<tr class="table-row">
									<td class="td-25-percent">
                                        <Span class="normal-heading black-color">
                                            <asp:Label ID="Label1" runat="server" Text="<%$ Tokens:StringResource,account.aspx.36 %>"/>
                                        </Span>								
										<%# DataBinder.Eval(Container.DataItem, "OrderNumber").ToString() %>
                                        <a class="underline-link" href='<%#m_StoreLoc + "OrderDetail.aspx?ordernumber=" + DataBinder.Eval(Container.DataItem, "OrderNumber") %>'>View Detail</a>
									</td>
									<td class="td-25-percent">
									    <Span class="normal-heading black-color">
									        <asp:Label ID="Label2" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>"/>
									    </Span>
									    <%#Localization.ConvertLocaleDateTime(DataBinder.Eval(Container.DataItem, "OrderDate").ToString(), Localization.GetDefaultLocale(), ThisCustomer.LocaleSetting)%>
									</td>
									<td class="td-25-percent">
									    <Span class="normal-heading black-color">
									        <asp:Label ID="Label3" runat="server" Text="<%$ Tokens:StringResource,account.aspx.39%>"/>
									    </Span>
                                        Total: $<%# Math.Round(Convert.ToDecimal(DataBinder.Eval(Container.DataItem, "OrderTotal")), 2).ToString() %> 
									    <%--<%#GetPaymentStatus(DataBinder.Eval(Container.DataItem, "PaymentMethod").ToString(), DataBinder.Eval(Container.DataItem, "CardNumber").ToString(), DataBinder.Eval(Container.DataItem, "TransactionState").ToString(), DataBinder.Eval(Container.DataItem, "OrderTotal").ToString())%>--%>
									</td>
									<td class="td-25-percent">
									    <Span class="normal-heading black-color">
									        <asp:Label ID="Label4" runat="server" Text="<%$ Tokens:StringResource,account.aspx.40 %>"/>
									    </Span>
									    <%#GetShippingStatus(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "OrderNumber").ToString()), DataBinder.Eval(Container.DataItem, "ShippedOn").ToString(), DataBinder.Eval(Container.DataItem, "ShippedVIA").ToString(), DataBinder.Eval(Container.DataItem, "ShippingTrackingNumber").ToString(), DataBinder.Eval(Container.DataItem, "TransactionState").ToString(), DataBinder.Eval(Container.DataItem, "DownloadEMailSentOn").ToString()) + "&nbsp;"%>
									</td>									
								</tr>
							</ItemTemplate>							
						</asp:Repeater>
                    </tbody>
				</table>
				<asp:Label ID="accountaspx55" runat="server" Text="<%$ Tokens:StringResource,account.aspx.55 %>"></asp:Label>
			</div>
            <asp:Repeater ID="rptPager" runat="server">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkPage" runat="server" Text='<%#Eval("Text") %>' CommandArgument='<%# Eval("Value") %>'
                     OnClick="Page_Changed" OnClientClick='<%# !Convert.ToBoolean(Eval("Enabled")) ? "return false;" : "" %>'></asp:LinkButton>
                </ItemTemplate>
            </asp:Repeater>			
        </div>
	</asp:Panel>
</asp:content>
