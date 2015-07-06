<%@ Control Language="C#" AutoEventWireup="true" CodeFile="editkitgrouptemplate.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.Admin_controls_editkitgrouptemplate" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>

<% if(this.KitGroup.IsValid == false) { %>
    <div class="admin_kit_group_validationErrors">
        Please correct the following errors and try again:
        <ul>
        <% foreach(ValidationError verror in this.KitGroup.ValidationErrors) { %>
            <li> <%= verror.ErrorMessage%>            </li>
        <% } %>
        </ul>
    </div>
<% } %>

<div id="pnlGroupId" runat="server" >
    <% if (this.KitGroup.IsNew)   { %>
        <span class="admin_kit_group_newItem">[New]</span>
    <% } else { %>
        <b>Group:</b> <%=this.KitGroup.Name %>
        <br />
        Id: <%= KitGroup.Id.ToString() %>
    <% } %>
</div>

Name: <br />
<asp:TextBox ID="txtGroupName" runat="server" 
    Text='<%# KitGroup.Name %>'
    CssClass='<%# this.CssClassIfInvalid(KitGroup, "Name", "admin_kit_group_inputError")  %>'
    Columns="50" >
</asp:TextBox>
<br />

Summary:<br />
<asp:TextBox ID="txtGroupSummary" runat="server" 
    TextMode="MultiLine" 
    Text='<%# KitGroup.Summary %>' 
    Columns="100" Rows="4">                            
</asp:TextBox>
<br />

Description:<br />
<asp:TextBox ID="txtGroupDescription" runat="server" 
    TextMode="MultiLine" 
    Text='<%# KitGroup.Description %>' 
    Columns="100" Rows="4">                            
</asp:TextBox>
<br />

<asp:CheckBox ID="chkRequired" runat="server" Checked='<%# KitGroup.IsRequired %>' /> Required 
<asp:CheckBox ID="chkReadOnly" runat="server" Checked='<%# KitGroup.IsReadOnly %>' /> Read Only
<br />
Display Order: <asp:TextBox ID="txtDisplayOrder" runat="server" MaxLength="3" Columns="2" Text='<%# KitGroup.DisplayOrder%>' ></asp:TextBox>
<asp:DropDownList ID="cboGroupType" runat="server" 
    DataSource='<%# this.GroupTypes %>' 
    DataValueField="Id" 
    DataTextField="Name"
    CssClass='<%# this.CssClassIfInvalid(KitGroup, "SelectionControl", "admin_kit_group_inputError")  %>' >
</asp:DropDownList>

<%if(KitGroup.IsNew == false){ %>
    <asp:HyperLink ID="lnkManageImages" runat="server" Text="Manage Images" NavigateUrl='<%# this.GenerateManageImagesLink()  %>'></asp:HyperLink>
<%} %>

<br />
<br />
        
<div class="admin_kit_groupItems">

    <ajax:TabContainer ID="tabKitITems" runat="server" Width="100%" >
    
        <ajax:TabPanel ID="pgeGeneral" runat="server">
        
            <HeaderTemplate>General</HeaderTemplate>
            <ContentTemplate>
            
                    <asp:DataList ID="dlItemsGeneralGroup" runat="server"
                         DataSource='<%# KitGroup.Items %>' Width="100%"                         
                         onitemcommand="dlItemsGeneralGroup_ItemCommand" 
                         OnItemCreated="dlItemsGeneralGroup_ItemCreated" >
                        <HeaderTemplate>
                            <tr>
                                <td>ID</td>
                                <td>Order</td>
                                <td>Name</td>
                                <td>Description</td>
                                <td>Default</td>
                                <td>
                                <%= ShowDeleteHeader()%>
                                </td>
                            </tr>
                        </HeaderTemplate>
                        <ItemTemplate>                    
                            <tr>
                                <td>
                                    <asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
                                    <asp:Label ID="lblItemId" runat="server" 
                                        Text='<%# DetermineDisplayId(Container.DataItemAs<KitItemData>().Id) %>'
                                        CssClass='<%# this.CssClassIf(Container.DataItemAs<KitItemData>().IsNew, "admin_kit_group_newItem") %>'>
                                    </asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtKitItemDisplayOrder" runat="server"  
                                        Columns="2" 
                                        MaxLength="3"
                                        Text='<%# Container.DataItemAs<KitItemData>().DisplayOrder %>'>
                                    </asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtKitItemName" runat="server" 
                                        Text='<%# Container.DataItemAs<KitItemData>().Name %>'
                                        CssClass='<%# this.CssClassIfInvalid(Container.DataItemAs<KitItemData>(), "Name", "admin_kit_group_inputError")  %>'
                                        >
                                    </asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtKitItemDescription" runat="server" Columns="50"  
                                        Text='<%# Container.DataItemAs<KitItemData>().Description %>'>
                                    </asp:TextBox>
                                </td>
                                <td>
                                    <asp:CheckBox ID="chkKitItemDefault" runat="server" 
                                    Checked='<%# Container.DataItemAs<KitItemData>().IsDefault %>' />
                                </td>
                                <td>
                                    <%--
                                        let's only show the delete button if this is an
                                        already existing line item and there are more than
                                        one none-new line items for this group
                                    --%>
                                    <asp:ImageButton ID="cmdDeleteItem" runat="server" 
                                        CommandName="Delete_KitItem"
                                        CommandArgument='<%# Container.DataItemAs<KitItemData>().Id %>'
                                        Visible='<%# Container.DataItemAs<KitItemData>().Id > 0 && Container.DataItemAs<KitItemData>().Group.NonNewItems.Count() > 1 %>'
                                        ImageUrl="~/App_Themes/Admin_Default/images/kit/delete_small.jpg" />
                                    <br />
                                </td>
                            </tr>                    
                        </ItemTemplate>
                    </asp:DataList>
                    
            </ContentTemplate>
        
            
            
        </ajax:TabPanel>
        
     <ajax:TabPanel ID="pgeInventory" runat="server">
            
            <HeaderTemplate>
                <asp:Label ID="Label1" runat="server" Text="Inventory Variant"></asp:Label>
            </HeaderTemplate>
            <ContentTemplate>
                
                    <asp:DataList ID="dltemsInventoryVariantGroup" runat="server"
                        DataSource='<%# KitGroup.Items %>' 
                        onitemdatabound="dltemsInventoryVariantGroup_ItemDataBound" >
                        <HeaderTemplate>
                            <tr>
                                <td>ID</td>
                                <td>Variant Id</td>
                                <td>Quantity Delta</td>
                                <td>Size</td>
                                <td>Color</td>
                            </tr>
                        </HeaderTemplate>
                        <ItemTemplate>                    
                            <tr>
                                <td>
                                    <asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
                                    
                                    <asp:Label ID="lblItemId" runat="server" 
                                        Text='<%# DetermineDisplayId(Container.DataItemAs<KitItemData>().Id) %>'
                                        CssClass='<%# this.CssClassIf(Container.DataItemAs<KitItemData>().IsNew, "admin_kit_group_newItem") %>'>
                                    </asp:Label>
                                </td>
                                <td>                                    
                                    <asp:HyperLink ID="lnkSelect" runat="server" NavigateUrl="javascript:void(0);" >Select</asp:HyperLink>
                                    
                                    <asp:TextBox ID="txtInventoryVariantId" runat="server" 
                                        Text='<%# Container.DataItemAs<KitItemData>().InventoryVariantId %>'>
                                    </asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtInventoryQuantityDelta" runat="server" 
                                        Text='<%# Container.DataItemAs<KitItemData>().InventoryQuantityDelta %>'>
                                    </asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtInventoryVariantSize" runat="server" 
                                        Text='<%# Container.DataItemAs<KitItemData>().InventoryVariantSize %>'>
                                    </asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtInventoryVariantColor" runat="server" 
                                        Text='<%# Container.DataItemAs<KitItemData>().InventoryVariantColor %>'>
                                    </asp:TextBox>
                                </td>
                            </tr>                    
                        </ItemTemplate>
                    </asp:DataList>
                
            </ContentTemplate>
            
        </ajax:TabPanel>
        
        <ajax:TabPanel ID="pgePricing" runat="server">
            
            <HeaderTemplate>
                <asp:Label ID="Label3" runat="server" Text="Pricing"></asp:Label>
            </HeaderTemplate>
            <ContentTemplate>
                
                    <asp:DataList ID="dlItemsPricingGroup" runat="server"
                        DataSource='<%# KitGroup.Items %>' >
                        <HeaderTemplate>
                            <tr>
                                <td>ID</td>
                                <td>Price Delta</td>
                                <td>Weight Delta</td>
                            </tr>
                        </HeaderTemplate>
                        <ItemTemplate>                    
                            <tr>
                                <td>
                                    <asp:HiddenField ID="hdfKitItemId" runat="server" Value='<%# Container.DataItemAs<KitItemData>().Id %>' />
                                    
                                    <asp:Label ID="lblItemId" runat="server" 
                                        Text='<%# DetermineDisplayId(Container.DataItemAs<KitItemData>().Id) %>'
                                        CssClass='<%# this.CssClassIf(Container.DataItemAs<KitItemData>().IsNew, "admin_kit_group_newItem") %>'>
                                    </asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtPriceDelta" runat="server" 
                                        Text='<%# Container.DataItemAs<KitItemData>().PriceDelta %>'>
                                    </asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtWeightDelta" runat="server" 
                                        Text='<%# Container.DataItemAs<KitItemData>().WeightDelta %>'>
                                    </asp:TextBox>
                                </td>
                            </tr>                    
                        </ItemTemplate>
                    </asp:DataList>
                
            </ContentTemplate>
            
        </ajax:TabPanel>
    
        
    </ajax:TabContainer>
    
    
</div>
        
     


