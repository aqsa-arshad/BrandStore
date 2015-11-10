<%@ Page Language="C#" AutoEventWireup="true" CodeFile="OrderDetail.aspx.cs" Inherits="OrderDetail" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:content runat="server" contentplaceholderid="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <div class="content-box-03">
        <div class="row">
            <div class="col-md-6">
                <p class="label-text">
                    <span>
                        <font>Order Number</font> 
                        <asp:Label runat="server" ID="lblOrderNumber"></asp:Label>
                    </span> 
                    <span>
                        <font>Order Date</font> 
                        <asp:Label runat="server" ID="lblOrderDate"></asp:Label>
                    </span>
                </p>
            </div>
        </div>
        <%--Bill and Ship to Address Section--%>
        <div class="top-row-adjsut border-line">
            <%--Ship to Address--%>
              <div class="row">
                <div class="col-md-6 ">
                   <span class="normal-heading black-color">Shipped to</span>
                  <p>
                    <span class="block-text">
                        <asp:Label ID="lblBAFullName" runat="server"></asp:Label>
                    </span>                    
                      <span class="block-text">
                        <asp:Label ID="lblBACompany" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblBAAddress1" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblBAAddress2" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblBASuite" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblBACityStateZip" runat="server"></asp:Label>
                    </span>                    
                      <span class="block-text">
                        <asp:Label ID="lblBACountry" runat="server"></asp:Label>
                    </span>
                      <span class="block-text">
                       <asp:Label ID="lblBAPhone" runat="server"></asp:Label>
                    </span>
                  </p>
                </div>
                <%--Bill to Address--%>
                <div class="col-md-6">
                  <span class="normal-heading black-color">Billed to</span>
                  <p>
                    <span class="block-text">
                        <asp:Label ID="lblSAFullName" runat="server"></asp:Label>
                    </span>                    
                    <span class="block-text">
                        <asp:Label ID="lblSACompany" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblSAAddress1" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblSAAddress2" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblSASuite" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblSACityStateZip" runat="server"></asp:Label>
                    </span>                    
                    <span class="block-text">
                        <asp:Label ID="lblSACountry" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                       <asp:Label ID="lblSAPhone" runat="server"></asp:Label>
                    </span>
                  </p>
                </div>
                <div class="col-md-6">
                  <span class="normal-heading black-color">Payment Method</span>
                  <p>
                    <span class="block-text">
                        <asp:Label ID="lblPMCardInfo" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblPMExpireDate" runat="server"></asp:Label>
                    </span>
                    <span class="block-text">
                        <asp:Label ID="lblPMCountry" runat="server"></asp:Label>
                    </span>                    
                  </p>
                </div>
            </div>
        </div>
 
       <div>
            <div class="clearfix"></div><div class="clearfix"></div><div class="clearfix"></div>
            <table class="table order-detail border-line">
                <tbody>
                    <asp:Repeater ID="rptOrderItemsDetail" runat="server">							
                        <ItemTemplate>
                            <tr>
                              <td>
                                <span class="normal-heading black-color">Branded Merch </span>
                                <div class="row">
                                  <div class="col-md-6">
                                      <div class="primary-img-box">                                          
                                           <img alt="Product" id="imgProduct" 
                                              
                                                class="img-responsive"/> 
                                      </div>
                                  </div>
                                    <%--TODO: All commented code'll be removed after showing the product image--%>
                                  <div class="col-md-5">
                                    <span class="normal-heading blue-color">
                                        <%# Eval("OrderedProductName") %>
                                        <%--<asp:Label ID="lblProductName" runat="server"></asp:Label>--%>
                                    </span>
                                    <span><%# Eval("ProductID") %></span>
                                      <%--<asp:Label ID="lblProductID" runat="server"></asp:Label>--%>
                                    <p>
                                        <%# Eval("Description").ToString().Take(70).Aggregate("", (x,y) => x + y) %>
                                        <%--<asp:Label ID="lblDescription" runat="server"></asp:Label>--%>
                                    </p>
                                  </div>
                                </div>
                              </td>
                              <td>
                                <span class="normal-heading black-color">Quantity</span>
                                <span>
                                    <%# Eval("Quantity") %>
                                    <%--<asp:Label ID="lblQuantity" runat="server"></asp:Label>--%>
                                </span>
                              </td>
                      
                              <td>
                                <span class="normal-heading black-color">Payment </span>
                                 <span>Price with (FUND) credit: $Y,YYY.YY</span>
                                 <span>Price with BLU Bucks used: $ZZZ.ZZ</span>
                              </td>
                              <td>
                                <span class="normal-heading black-color">Delivery </span>
                                <span><%# Eval("ShippingMethod") %></span>
                                <%--<asp:Label ID="lblDelivery" runat="server"></asp:Label>--%>
                              </td>
                              <td>
                                <a href="#" class="underline-link">Track item &gt;</a>
                              </td>
                            </tr> 
                        </ItemTemplate>
						<FooterTemplate></FooterTemplate>
					</asp:Repeater>
                </tbody>
            </table>
        </div>
               
        <%--Billing Amounts--%>
        <div class="row">
            <div class="col-md-6">
            </div>
            <div class="col-md-6">
                <p>
                <span class="black-blu-label">
                    <font>Subtotal: $</font>
                    <asp:Label runat="server" ID="lblSubTotal"></asp:Label>
                </span>
                </p>
                <span class="normal-heading black-color">Credits Used</span>
                <p>
                    <span class="block-text">Credit Name: $X,XXX.XX</span>
                    <span class="block-text">Credit Name: $XXX.XX</span>
                    <span class="block-text">BLU Bucks: $XXX.XX</span>
                </p>

                <span class="normal-heading black-color">Charges</span>
                <p>
                    <span class="block-text">Taxes: $<asp:Label runat="server" ID="lblTax"></asp:Label></span>
                    <span class="block-text">Shipping:  $<asp:Label runat="server" ID="lblShippingCost"></asp:Label></span>
                </p>

                <p>
                    <span class="black-blu-label"><font>Total:</font>    $<asp:Label runat="server" ID="lblTotalAmount"></asp:Label></span>
                </p>

                <div class="form-group">
                    <a class="btn btn-primary btn-half" type="submit" role="button" href='<%=m_StoreLoc + "receipt.aspx?ordernumber=" + orderNumber %>'>Print Receipt</a>   
                    <%=GetReorder()%>
                    <%--<a href="<%#GetReorder()%>" class="btn btn-primary btn-half" role="button">Reorder Items</a>--%>         
                </div>
            </div>
        </div>
    </div>
</asp:content>
