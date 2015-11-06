<%@ Page Language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefront.orderhistory" CodeFile="orderhistory.aspx.cs" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:content runat="server" contentplaceholderid="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel ID="pnlOrderHistory" runat="server">			
        <div class="content-box-03">
			<div>
				<table class="table">
					<tbody>						
						<asp:Repeater ID="rptOrderhistory" runat="server">							
							<ItemTemplate>
								<tr class="table-row">
									<td>
                                        <Span class="blk-normal-heading">
                                            <asp:Label ID="Label1" runat="server" Text="<%$ Tokens:StringResource,account.aspx.36 %>"/>
                                        </Span>								
										<a target="_blank" href='<%#m_StoreLoc + "receipt.aspx?ordernumber=" + DataBinder.Eval(Container.DataItem, "OrderNumber") %>'><%# DataBinder.Eval(Container.DataItem, "OrderNumber").ToString() %></a>										
                                        <a class="underline-link" href="#">View Detail</a>
									</td>
									<td>
									    <Span class="blk-normal-heading">
									        <asp:Label ID="Label2" runat="server" Text="<%$ Tokens:StringResource,account.aspx.38%>"/>
									    </Span>
									    <%#AspDotNetStorefrontCore.Localization.ConvertLocaleDateTime(DataBinder.Eval(Container.DataItem, "OrderDate").ToString(), Localization.GetDefaultLocale(), ThisCustomer.LocaleSetting)%>
									</td>
									<td>
									    <Span class="blk-normal-heading">
									        <asp:Label ID="Label3" runat="server" Text="<%$ Tokens:StringResource,account.aspx.39%>"/>
									    </Span>
									    <%#GetPaymentStatus(DataBinder.Eval(Container.DataItem, "PaymentMethod").ToString(), DataBinder.Eval(Container.DataItem, "CardNumber").ToString(), DataBinder.Eval(Container.DataItem, "TransactionState").ToString(), DataBinder.Eval(Container.DataItem, "OrderTotal").ToString())%>
									</td>
									<td>
									    <Span class="blk-normal-heading" >
									        <asp:Label ID="Label4" runat="server" Text="<%$ Tokens:StringResource,account.aspx.40 %>"/>
									    </Span>
									    <%#"&nbsp;" + GetShippingStatus(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "OrderNumber").ToString()), DataBinder.Eval(Container.DataItem, "ShippedOn").ToString(), DataBinder.Eval(Container.DataItem, "ShippedVIA").ToString(), DataBinder.Eval(Container.DataItem, "ShippingTrackingNumber").ToString(), DataBinder.Eval(Container.DataItem, "TransactionState").ToString(), DataBinder.Eval(Container.DataItem, "DownloadEMailSentOn").ToString()) + "&nbsp;"%>
									</td>									
								</tr>
							</ItemTemplate>
							<FooterTemplate></FooterTemplate>
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
