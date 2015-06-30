<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityObjectList.ascx.cs" Inherits="AspDotNetStorefront.EntityObjectListControl" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>   



 <div>
     
    <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch"  OnFilter="ctrlSearch_Filter" 
        HighlightCurrent="false"
        OnContentCreated="ctrlSearch_ContentCreated" 
        AlphaGrouping="2"  >
                    
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
                        OnItemCommand="grdMap_ItemCommand" 
                        OnItemDataBound="grdMap_ItemDataBound"
                        Width="400px">
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
                                
                                <telerik:GridTemplateColumn HeaderText="ID" UniqueName="TemplateColumn1" ItemStyle-Width="5%" SortExpression="Name" >
                                    <ItemTemplate>
                                       
                                       <asp:Label ID="lblID" runat="server" Text='<%# DataItemAs<DatabaseObject>(Container).ID %>' ></asp:Label>
                                              
                                    </ItemTemplate>
                                    
                                </telerik:GridTemplateColumn>
                                
                   
                                <telerik:GridTemplateColumn HeaderText="Name" UniqueName="TemplateColumn1" ItemStyle-Width="95%" SortExpression="Name" >
                                    <ItemTemplate>
                                        <asp:HyperLink ID="lnkName" runat="server" 
                                            NavigateUrl="javascript:void(0);"
                                            Text='<%# ML_Localize(DataItemAs<DatabaseObject>(Container).Name) %>' >
                                        </asp:HyperLink>
                                              
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