<%@ Page EnableViewState="true" language="c#" Inherits="AspDotNetStorefrontAdmin.orders" CodeFile="orders.aspx.cs" EnableEventValidation="false" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="ew" Namespace="eWorld.UI" Assembly="eWorld.UI" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="Controls/StoreSelector.ascx" %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
<script type="text/javascript">
    function toggleFilters(show) {
        if (show) {
            document.getElementById("OrderFilters").style.display = "block";
            document.getElementById("ShowHiddenFilters").style.display = "none";
        }
        else {
            document.getElementById("OrderFilters").style.display = "none";
            document.getElementById("ShowHiddenFilters").style.display = "block";
        }
    }
</script>
<form id="frmOrder" name="frmOrder" method="post" defaultbutton="btnSubmit">
    <div id="ShowHiddenFilters" style="text-align:center;font-weight:bold;padding:2px;display:none;">
        <a href="javascript:void(0);" onclick="toggleFilters(true);"><asp:literal runat="server" Text="<%$Tokens:StringResource, admin.order.ShowFilters %>"></asp:literal></a>
    </div>
    <div id="OrderFilters">
	    <table cellSpacing="0" cellPadding="0" width="100%" border="0">
		    <tr class="tablenormal">
			    <th width="50%">
				    <asp:literal id="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.orders.DateRange %>"></asp:literal></th>
			    <th>
				    <asp:literal id="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.orders.OrderQualifiers %>"></asp:literal></th></tr>
		    <tr>
			    <td valign="top" align="left" class="ordercustomer">
				    <table cellpadding="5px" cellspacing="0" border="0" width="100%">
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.orders.StartDate %>" />
							    <ew:calendarpopup id="dateStart" runat="server" Height="20px" DisableTextboxEntry="False" AllowArbitraryText="False"
								    padsingledigits="True" nullable="True" calendarwidth="200" Width="80px" showgototoday="True" imageurl="~/App_Themes/Admin_Default/images/calendar.gif"
								    Font-Size="9px">
								    <weekdaystyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Black"
									    backcolor="Gainsboro"></weekdaystyle>
								    <monthheaderstyle font-size="Small" font-names="Verdana,Helvetica,Tahoma,Arial" font-bold="True" forecolor="White"
									    backcolor="Gray"></monthheaderstyle>
								    <offmonthstyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Gainsboro"
									    backcolor="Gainsboro"></offmonthstyle>
								    <gototodaystyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Black"
									    backcolor="White"></gototodaystyle>
								    <todaydaystyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="MediumBlue"
									    backcolor="Gainsboro"></todaydaystyle>
								    <dayheaderstyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Black"
									    backcolor="Azure"></dayheaderstyle>
								    <weekendstyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Red"
									    backcolor="Gainsboro"></weekendstyle>
								    <selecteddatestyle borderstyle="Inset" font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial"
									    borderwidth="2px" forecolor="MediumBlue" backcolor="Silver"></selecteddatestyle>
								    <cleardatestyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Black"
									    backcolor="White"></cleardatestyle>
							    </ew:calendarpopup>&nbsp;&nbsp;<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.orders.EndDate %>" /><ew:calendarpopup id="dateEnd" runat="server" Height="20px" DisableTextboxEntry="False" AllowArbitraryText="False"
								    padsingledigits="True" nullable="True" calendarwidth="200" Width="80px" showgototoday="True" imageurl="~/App_Themes/Admin_Default/images/calendar.gif"
								    Font-Size="9px">
								    <weekdaystyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Black"
									    backcolor="Gainsboro"></weekdaystyle>
								    <monthheaderstyle font-size="Small" font-names="Verdana,Helvetica,Tahoma,Arial" font-bold="True" forecolor="White"
									    backcolor="Gray"></monthheaderstyle>
								    <offmonthstyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Gainsboro"
									    backcolor="Gainsboro"></offmonthstyle>
								    <gototodaystyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Black"
									    backcolor="White"></gototodaystyle>
								    <todaydaystyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="MediumBlue"
									    backcolor="Gainsboro"></todaydaystyle>
								    <dayheaderstyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Black"
									    backcolor="Azure"></dayheaderstyle>
								    <weekendstyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Red"
									    backcolor="Gainsboro"></weekendstyle>
								    <selecteddatestyle borderstyle="Inset" font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial"
									    borderwidth="2px" forecolor="MediumBlue" backcolor="Silver"></selecteddatestyle>
								    <cleardatestyle font-size="XX-Small" font-names="Verdana,Helvetica,Tahoma,Arial" forecolor="Black"
									    backcolor="White"></cleardatestyle>
							    </ew:calendarpopup></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.orders.Use %>" /><asp:RadioButton id="OrderDateType" runat="server" GroupName="ReportDateTypeGroup" Text="<%$Tokens:StringResource, admin.orders.OrderDate %>" Checked="True"></asp:RadioButton>&nbsp;&nbsp;
								    <asp:RadioButton id="TransactionDateType" runat="server" GroupName="ReportDateTypeGroup" Text="<%$Tokens:StringResource, admin.orders.TransactionDate %>"></asp:RadioButton>
						    </td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.orders.ReportType %>" /> <asp:RadioButton id="RegularReport" runat="server" GroupName="ReportTypeGroup" Text="<%$Tokens:StringResource, admin.orders.RegularReport %>"
									    Checked="True"></asp:RadioButton>&nbsp;&nbsp;
								    <asp:RadioButton id="BulkPrintingReport" runat="server" GroupName="ReportTypeGroup" Text="<%$Tokens:StringResource, admin.orders.BulkPrintingReport %>"></asp:RadioButton>&nbsp;&nbsp;
								    <asp:RadioButton id="SummaryReport" runat="server" GroupName="ReportTypeGroup" Text="<%$Tokens:StringResource, admin.order.SummaryReport %>"></asp:RadioButton>
						    </td>
					    </tr>
                    </table>
				    <hr/>
				    <asp:radiobutton id="rbRange" runat="server" groupname="rbEasyRange" text="<%$Tokens:StringResource, admin.orders.UseDateRangeAbove %>" Checked="True"></asp:radiobutton>
				    <table width="100%">
					    <tr>
						    <td>
							    <asp:radiobuttonlist id="rbEasyRange" runat="server" RepeatColumns="2">
								    <asp:listitem Value="Today" Text="<%$Tokens:StringResource, admin.order.EasyRangeToday %>" />
								    <asp:listitem Value="ThisWeek" Text="<%$Tokens:StringResource, admin.order.EasyRangeThisWeek %>" />
								    <asp:listitem Value="ThisMonth" Text="<%$Tokens:StringResource, admin.order.EasyRangeThisMonth %>" />
								    <asp:listitem Value="ThisYear" Text="<%$Tokens:StringResource, admin.order.EasyRangeThisYear %>" />
								    <asp:listitem Value="Yesterday" Text="<%$Tokens:StringResource, admin.order.EasyRangeYesterday %>" />
								    <asp:listitem Value="LastWeek" Text="<%$Tokens:StringResource, admin.order.EasyRangeLastWeek %>" />
								    <asp:listitem Value="LastMonth" Text="<%$Tokens:StringResource, admin.order.EasyRangeLastMonth %>" />
								    <asp:listitem Value="LastYear" Text="<%$Tokens:StringResource, admin.order.EasyRangeLastYear %>" />
							    </asp:radiobuttonlist></td>
					    </tr>
				    </table>
			    </td>
			    <td vAlign="top" class="ordercustomer">
				    <table id="Table1" cellSpacing="0" cellPadding="2" width="100%" align="center" border="0">
					    <tr>
						    <td>
							    <asp:literal id="Literal3" runat="server" Text="<%$Tokens:StringResource, admin.order.OrderNumberTransactionSubscriptionID %>"></asp:literal></td>
						    <td>
							    <asp:textbox id="txtOrderNumber" runat="server" Width="170px"></asp:textbox></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.CustomerID %>" /></td>
						    <td>
							    <ew:numericbox id="txtCustomerID" runat="server" Width="170px" PositiveNumber="True" RealNumber="False"></ew:numericbox></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.CustomerEmail %>" /></td>
						    <td>
							    <asp:textbox id="txtEMail" runat="server" width="170px"></asp:textbox></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.CreditCartOrLastFourNumber %>" /></td>
						    <td>
							    <asp:textbox id="txtCreditCardNumber" runat="server" width="170px"></asp:textbox></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.CustomerName %>" /></td>
						    <td>
							    <asp:textbox id="txtCustomerName" runat="server" width="170px"></asp:textbox></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.Company %>" /></td>
						    <td>
							    <asp:textbox id="txtCompany" runat="server" width="170px"></asp:textbox></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.PaymentMethod %>" /></td>
						    <td>
							    <asp:dropdownlist id="ddPaymentMethod" runat="server" Width="175px">
							    </asp:dropdownlist></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.TransactionState %>" /></td>
						    <td>
							    <asp:dropdownlist id="TransactionState" runat="server">
								    <asp:listitem Value="-" Selected="True" Text="<%$Tokens:StringResource, admin.order.TransactionStateAllStates %>" />
								    <asp:listitem Value="AUTHORIZED" Text="<%$Tokens:StringResource, admin.order.TransactionStateAuthorized %>" />
								    <asp:listitem Value="CAPTURED" Text="<%$Tokens:StringResource, admin.order.TransactionStateCaptured %>" />
								    <asp:listitem Value="VOIDED" Text="<%$Tokens:StringResource, admin.order.TransactionStateVoided %>" />
								    <asp:listitem Value="FORCE VOIDED" Text ="<%$Tokens:StringResource, admin.order.TransactionStateForceVoided %>" />
								    <asp:listitem Value="REFUNDED" Text="<%$Tokens:StringResource, admin.order.TransactionStateRefunded %>" />
								    <asp:listitem Value="FRAUD" Text="<%$Tokens:StringResource, admin.order.TransactionStateFraud %>" />
								    <asp:listitem Value="PENDING" Text="<%$Tokens:StringResource, admin.order.TransactionStatePending %>" />
							    </asp:dropdownlist></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.TransactionType %>" /></td>
						    <td>
							    <asp:dropdownlist id="TransactionType" runat="server">
								    <asp:listitem Value="-" Selected="True" Text="<%$Tokens:StringResource, admin.order.TransactionTypeAllTypes %>" />
								    <asp:listitem Value="UNKNOWN" Text="<%$Tokens:StringResource, admin.order.TransactionTypeUnknown %>" />
								    <asp:listitem Value="CHARGE" Text="<%$Tokens:StringResource, admin.order.TransactionTypeCharge %>" />
								    <asp:listitem Value="CREDIT" Text="<%$Tokens:StringResource, admin.order.TransactionTypeCredit %>" />
								    <asp:listitem Value="RECURRING_AUTO" Text="<%$Tokens:StringResource, admin.order.TransactionTypeRecurringAuto %>" />
							    </asp:dropdownlist></td>
					    </tr>
					    <tr id="ProductMatchRow" runat="server">
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.Product %>" /></td>
						    <td>
							    <asp:dropdownlist id="ProductMatch" runat="server">
							    </asp:dropdownlist></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.NewOrdersOnly %>" /></td>
						    <td>
							    <asp:radiobuttonlist id="rbNewOrdersOnly" runat="server" Width="150px" RepeatLayout="Flow" RepeatDirection="Horizontal">
								    <asp:listitem Value="0" Text="<%$Tokens:StringResource, admin.order.NewOrdersOnlyNo %>" />
								    <asp:listitem Value="1" Selected="True" Text="<%$Tokens:StringResource, admin.order.NewOrdersOnlyYes %>" />
							    </asp:radiobuttonlist></td>
					    </tr>
					    <tr id = "trAffiliate" runat = "server">
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.Affiliate %>" /></td>
						    <td>
							    <asp:dropdownlist id="ddAffiliate" runat="server" Width="150px" datatextfield="Name" datavaluefield="AffiliateID"></asp:dropdownlist></td>
					    </tr>
					    <tr>
						    <td><asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.order.CouponCode %>" /></td>
						    <td>
							    <asp:dropdownlist id="ddCouponCode" runat="server" Width="150px" datatextfield="CouponCode" datavaluefield="CouponCode"></asp:dropdownlist></td>
					    </tr>
					    <tr>
						    <td><asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.order.PromotionCode %>" />:</td>
						    <td>
							    <asp:dropdownlist id="ddPromotion" runat="server" Width="150px" datatextfield="Promotion" datavaluefield="Promotion"></asp:dropdownlist></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.ShipToState %>" /></td>
						    <td><asp:dropdownlist id="ddShippingState" runat="server" Width="150px" datatextfield="Name" datavaluefield="Abbreviation"></asp:dropdownlist></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.SKU %>" /></td>
						    <td><asp:textbox id="txtProductSKU" runat="server" width="150px"></asp:textbox></td>
					    </tr>
					    <tr>
						    <td><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.PriceRange %>" /></td>
						    <td>
                                <ew:numericbox id="txtPriceRangeLow" runat="server" Width="50px" DecimalPlaces="2" />
                                &nbsp;to&nbsp;
                                <ew:numericbox id="txtPriceRangeHigh" runat="server" Width="50px" DecimalPlaces="2" />
                            </td>
					    </tr>
                        <tr>
                            <td><asp:Label ID="lblStore" runat="server" Text="<%$Tokens:StringResource, admin.order.ForStore %>" /></td>
                            <td>
						        <aspdnsf:StoreSelector runat="server" ShowText="false" SelectMode="SingleDropDown" ShowDefaultForAllStores="true"  ID="ssOne" />
                            </td>
					    </tr>

                        

				    </table>
			    </td>
		    </tr>
		    <tr>
			    <th align="center" style="border-top:solid 2px #1B427D; background-color:#ffffff;padding-left:100px;" colspan="2">
				    <asp:button id="btnSubmit" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.order.Submit %>" OnClick="btnSubmit_Click">
				    </asp:button>
                    <a href="javascript:void(0);" onclick="toggleFilters(false);"><asp:literal runat="server" Text="<%$Tokens:StringResource, admin.order.HideFilters %>"></asp:literal></a>
		       </th>
		    </tr>
	    </table>
    </div>
	<asp:label id="lblError" runat="server" Width="100%" Font-Bold="True"></asp:label>
	<asp:panel id="pnlRegularReport" runat="server" Visible="False">
		<table cellSpacing="0" cellPadding="1" width="100%" border="0">
			<tr>
				<td vAlign="top" align="center" width="210" height="1024"><br/>
					<table cellSpacing="0" cellPadding="0" border="0">
						<tr>
							<td>
								<table id="tblHeader" cellSpacing="0" cellPadding="0" border="0">
									<tr>
										<td>
											<asp:image id=Image1 runat="server" >
											</asp:image></td>
									</tr>
								</table>
							</td>
						</tr>
						<tr>
							<td>
								<asp:datalist id="dlSelected" runat="server" Width="100%" ItemStyle-BorderWidth="1" ItemStyle-BorderStyle="Solid"
									BorderStyle="None">
									<AlternatingItemStyle CssClass="table-alternatingrow2" />
									<itemstyle borderwidth="1px" borderstyle="Solid" CssClass="table-row2"></itemstyle>
									<itemtemplate>
										<asp:label id="lblOrderDate" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"OrderDate","{0:d}") %>'>
										</asp:label>
										<asp:hyperlink id="hlOrderNumber" runat="server" NavigateUrl='<%# DataBinder.Eval(Container.DataItem,"OrderNumber","orderframe.aspx?ordernumber={0}") %>' Target="orderframe" Text='<%# DataBinder.Eval(Container.DataItem,"OrderNumber") %>' >
										</asp:hyperlink>
										<asp:image id="imgNew" runat="server" ImageUrl='<%# DataBinder.Eval(Page, "NewImage") %>' Visible='<%# (DataBinder.Eval(Container.DataItem,"IsNew").ToString()=="1") %>' ImageAlign="AbsMiddle">
										</asp:image>
									</itemtemplate>
									<headerstyle borderwidth="0px" borderstyle="None" backcolor="White"></headerstyle>
								</asp:datalist></td>
						</tr>
					</table>
					</td>
				<td vAlign="top">				
					<IFRAME  runat="server" id="orderframe" name="orderframe" frameBorder=0 width="100%" height="1000px">
					</IFRAME>
				</td>
			</tr>
		</table>
	</asp:panel>
	<asp:panel id="pnlBulkPrintingReport" runat="server" Visible="False">Bulk Printing Report<BR />&nbsp;<asp:Literal id="Literal4" runat="server"></asp:Literal></asp:panel>
	<asp:panel id="pnlSummaryReport" runat="server" Visible="False">
		<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.SummaryReport %>" /><br/>
<asp:DataGrid id="SummaryGrid" runat="server" CellPadding="2">
			<ItemStyle Font-Size="X-Small"></ItemStyle>
			<HeaderStyle Font-Bold="True"></HeaderStyle>
		</asp:DataGrid>
	</asp:panel>
</Form>
</asp:Content>
