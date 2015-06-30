<%@ Page Language="C#" AutoEventWireup="true" CodeFile="shipwire.aspx.cs" Inherits="AspDotNetStorefrontAdmin.shipwire" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
<link rel="Stylesheet" type="text/css" href="<%= ResolveClientUrl("~/App_Themes/Admin_Default/Shipwire.css") %>" />
    <div class="sw_ext_pgcont">
        <h1 id="sw_logo">
            <a href="http://partner.shipwire.com/o.php?id=1886"><em>Shipwire</em></a></h1>
        <div id="sw_main">
            <div id="SalesRow" runat="server" class="salesrow">
                <div class="salesrowhd">
                    <h2 class="focus">
                        <em>Shipwire lets you focus on growing your business by removing the hassle of shipping
                            &amp; storage</em></h2>
                </div>
                <div class="salesrowbd">
                    <ol class="steps">
                        <li class="sendstep">You Send Us Merchandise</li>
                        <li class="orderstep">Your Customers Order Online</li>
                        <li class="shipstep">Shipwire Takes Care of Shipping</li>
                    </ol>
                    <div class="tryitnow">
                        <h3>
                            Try it Out</h3>
                        <p class="free">
                            Free, no obligation trial</p>
                        <p class="store">
                            Store and ship up to 6 products, free!</p>
                        <a href="http://partner.shipwire.com/o.php?id=1886" class="startbtn"><em>Get More Information</em></a>
                    </div>
                </div>
            </div>
            <div id="TrackingRow" runat="server" class="salesrow">
                <div class="salesrowhd"></div><div class="salesrowbd">
                    <ol class="steps">
                        <li class="shipstep">Update Shipment Tracking</li>
                    </ol>
                    <div class="tryitnow">
                        <p class="free">Pull shipper tracking numbers into AspDotNetStorefront</p>
                        <asp:LinkButton ID="UpdateTracking" runat="server" CssClass="startbtn" OnClick="UpdateTracking_Click" ><em>Update Order Tracking</em></asp:LinkButton>
                        <asp:Label ID="UpdateTrackingStatus" runat="server"></asp:Label>
                    </div>
               </div>
            </div>
            <div id="InventoryRow" runat="server" class="salesrow">
                <div class="salesrowhd"></div><div class="salesrowbd">
                    <ol class="steps">
                        <li class="orderstep">Update Product Inventory</li>
                    </ol>
                    <div class="tryitnow">
                        <p class="free">Pull Warehouse Inventory in AspDotNetStorefront</p>
                        <p class="store">Ensure that your product quantities are in sync!</p>
                        <asp:LinkButton ID="UpdateInventory" runat="server" CssClass="startbtn" OnClick="UpdateInventory_Click" ><em>Update Product Inventory</em></asp:LinkButton>
                        <asp:Label ID="UpdateInventoryStatus" runat="server"></asp:Label>
                    </div>
               </div>            </div>
            <div id="AppConfigRow" runat="server" class="salesrow">
                <div class="salesrowhd"></div>
                <div class="salesrowbd">
                    <ol class="steps">
                        <li class="sendstep">Configure your Storefront</li>
                    </ol>
                    <div id="divMain" runat="server" class="tryitnow">
                    <p class="free">Configure AspDotNetStorefront</p>
                    <p class="store">Get these values from Shipwire:</p>
                        <table cellpadding="1" cellspacing="0" border="0">
                            <tr>
                                <td align="right" valign="middle">
                                    Username:
                                </td>
                                <td align="left" valign="middle">
                                    <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td align="right" valign="middle">
                                    Password:
                                </td>
                                <td align="left" valign="middle">
                                    <asp:TextBox ID="txtPassword" runat="server"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                        <asp:LinkButton ID="ConfigureShipwire" runat="server" CssClass="startbtn" OnClick="ConfigureShipwire_Click"><em>Store Values</em></asp:LinkButton>
                        <asp:Label ID="ConfigureShipwireStatus" runat="server"></asp:Label>
                    </div>
                </div>
            </div>
        </div><!-- sw_main -->
    </div>
</asp:Content>