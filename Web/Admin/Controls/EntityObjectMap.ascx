<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityObjectMap.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.EntityObjectMap" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>   



 <div>
     
    <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch"  OnFilter="ctrlSearch_Filter"
        OnContentCreated="ctrlSearch_ContentCreated" AlphaGrouping="2"  >
                    
                <Search SearchButtonCaption="Go" 
                    SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>" 
                    SearchTextMinLength="<%$ Tokens:AppConfigUSInt, MinSearchStringLength %>" 
                    SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>" 
                                ValidateInputLength="false" 
                                ShowValidationMessageBox="false" 
                                ShowValidationSummary="false" 
                                UseLandingPageBehavior="false" 
                                WillRenderInUpdatePanel="true"  />
                                
                <ContentTemplate>
                    
                    <telerik:RadGrid ID="grdMap" runat="server" GridLines="None" 
                        AllowSorting="false" 
                        OnItemCreated="grdMap_ItemCreated"
                        OnSortCommand="grdMap_SortCommand"  
                        OnItemCommand="grdMap_ItemCommand">
                        <HeaderContextMenu>
                            <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                        </HeaderContextMenu>
                        
                        <ClientSettings>
                            <Resizing AllowColumnResize="true" EnableRealTimeResize="true" ResizeGridOnColumnResize="true" ClipCellContentOnResize="false" />
                        </ClientSettings>

                        <MasterTableView AutoGenerateColumns="False">
                        <RowIndicatorColumn>
                        <HeaderStyle Width="20px"></HeaderStyle>
                        </RowIndicatorColumn>

                        <ExpandCollapseColumn>
                        <HeaderStyle Width="20px"></HeaderStyle>
                        </ExpandCollapseColumn>
                        
                        <AlternatingItemStyle  CssClass="config_alternating_item" />
                       
                            <Columns>
 
                                <telerik:GridTemplateColumn HeaderText="ID" UniqueName="TemplateColumn1" ItemStyle-Width="20px" SortExpression="Name" >
                                    <ItemTemplate>
                                       <asp:Label ID="lblID" runat="server" Text='<%# DataItemAs<IGrouping<String, MappedObject>>(Container).First().ID %>' ></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                
                                <telerik:GridTemplateColumn HeaderText="Name" UniqueName="TemplateColumn1" ItemStyle-Width="60px" SortExpression="Name" >
                                    <ItemTemplate>
                                       <asp:Label ID="lblName" runat="server" Text='<%# Trim(ML_Localize(DataItemAs<IGrouping<String, MappedObject>>(Container).First().Name), 50) %>' ></asp:Label>
                                    </ItemTemplate>
                                    
                                </telerik:GridTemplateColumn>                               
                            
                                <telerik:GridTemplateColumn UniqueName="columnEdit" ItemStyle-Width="35px"  HeaderStyle-HorizontalAlign="Center" HeaderStyle-VerticalAlign="Middle" HeaderStyle-Height="25px"  >
                                
                                    <HeaderTemplate>
                                    
                                        <div class="map_header"> 
                                            Map: <a href="javascript:void(0);" onclick="javascript:mapAll(true);" >All,</a>&nbsp;<a href="javascript:void(0);" onclick="javascript:mapAll(false);" >None</a> 
                                            |&nbsp;<asp:LinkButton ID="lnkSave" runat="server" CommandName="SaveMapping" >
                                                <img runat="server" src="~/App_Themes/Admin_Default/images/save_small.gif" alt="save" style="border-style:none; margin: 0px 0px -3px 0;" /> Save
                                            </asp:LinkButton>
                                        </div>
                                    
                                    </HeaderTemplate>
                                    
                                    <ItemStyle HorizontalAlign="Center" />
                                    <ItemTemplate >
                                    
                                        <%--hidden field for reference later during update--%>
                                        <asp:HiddenField ID="hdfEntityId" runat="server" Value='<%# DataItemAs<IGrouping<String, MappedObject>>(Container).First().ID %>' />
                                        
                                        <asp:CheckBoxList ID="chkStores" runat="server" RepeatDirection="Horizontal" BorderStyle="None" RepeatLayout="Table" RepeatColumns="10" />
                                        
                                    </ItemTemplate>
                                    
                                </telerik:GridTemplateColumn>
                                                                     
                            </Columns>
                        </MasterTableView>

                        <FilterMenu>
                        <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                        </FilterMenu>

                    </telerik:RadGrid>
                
                </ContentTemplate>
                
    </aspdnsf:SearcheableTemplate>


</div>
