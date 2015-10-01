<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomerAlerts.ascx.cs" Inherits="CustomerAlerts" %>
<div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-body">
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <img src="images/closs-popup.png"></button>
                <h4 class="modal-title" id="myModalLabel">Daily Alerts</h4>
                <ul class="modal-list">
                <asp:Repeater ID="rptCustomerAlerts" runat="server" OnItemCommand="rptCustomerAlerts_ItemCommand">
                    <ItemTemplate>
                        
                            <li class="unread">
                                <asp:Label ID="lblAlertTitle" runat="server" Text='<%# Eval("Title") %>' Font-Bold='<%# bool.Parse(Eval("IsRead").ToString()) ? false : true %>'></asp:Label>
                                <asp:Label ID="lblSeprator" runat="server" Text=" - " Font-Bold='<%# bool.Parse(Eval("IsRead").ToString()) ? false : true %>'></asp:Label>
                                <asp:Label ID="lblAlertDescription" runat="server" ForeColor="GrayText" Text='<%# Eval("Description") %>' Font-Bold='<%# bool.Parse(Eval("IsRead").ToString()) ? false : true %>'></asp:Label>
                                <asp:LinkButton ID="btnRead" runat="server" Text="Read" CommandName="Read" CommandArgument='<%# Eval("CustomerAlertStatusID") %>' Visible='<%# bool.Parse(Eval("IsRead").ToString()) ? false : true %>'></asp:LinkButton>
                                <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CommandName="Delete" CommandArgument='<%# Eval("CustomerAlertStatusID") %>'></asp:LinkButton>
                            </li>
                        
                    </ItemTemplate>
                </asp:Repeater>
              </ul>
            </div>
        </div>
    </div>
</div>

