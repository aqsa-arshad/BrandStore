<%@ Page Language="C#" AutoEventWireup="true" CodeFile="StoreMaintenance.aspx.cs" Inherits="AspDotNetStorefrontAdmin.StoreMaintaince" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="rwrapped" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreControl" Src="controls/StoreControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="GlobalConfigList" src="controls/GlobalConfigList.ascx" %>
<%@ Register TagPrefix="AJAX" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="controls/StoreSelector.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityObjectMap" src="controls/EntityObjectMap.ascx" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>   

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">  
		<div id="pnlLoadingGeneral" runat="server" style="float:left;margin-left:400px;position:absolute;"> 
			<asp:UpdateProgress ID="UpdateProgress1" runat="server">
					<ProgressTemplate>                    
						<img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-2.gif" />                    
					</ProgressTemplate>
				</asp:UpdateProgress>
		</div>       
        <div class="content_area" >                    
                 <AJAX:TabContainer runat="server" ID="tabPanel">                   
                    <AJAX:TabPanel ID="TabPanel1"  runat="server" HeaderText="Global Configs" >
                        <ContentTemplate>
                            <div class="pane">
                                <aspdnsf:GlobalConfigList id="ctrlConfigList" runat="server" />
                            </div>
                        </ContentTemplate>
                    </AJAX:TabPanel>
                    <AJAX:TabPanel ID="TabPanel2" runat="server" HeaderText="<%$ Tokens:StringResource, StoreMaintenance.Domains %>" >
                        <ContentTemplate>
                            <div class="pane">
                                <aspdnsf:StoreControl ID="scMain" runat="server" />
                            </div>
                        </ContentTemplate>
                    </AJAX:TabPanel>
                    <AJAX:TabPanel ID="TabPanel3" runat="server" HeaderText="Store Mapping" >
                        <ContentTemplate>
                            <div class="pane">
                                Choose an entity to map:
                               <asp:DropDownList ID="ddlEntities" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlEntities_SelectedIndexChanged">
                                   <asp:ListItem Value="" Text="Select an Entity" />
                                    <asp:ListItem Value="Affiliate" Text="Affiliates" />
                                    <asp:ListItem Value="Category" Text="Categories" />
                                    <asp:ListItem Value="Promotion" Text="Promotions" />
                                    <asp:ListItem Value="GiftCard" Text="GiftCards" />
                                    <asp:ListItem Value="Manufacturer" Text="Manufacturers" />
                                    <asp:ListItem Value="New" Text="News" />
                                    <asp:ListItem Value="OrderOption" Text="Order Options" />
                                    <asp:ListItem Value="Product" Text="Products" />
                                    <asp:ListItem Value="Section" Text="Departments" />
                                    <asp:ListItem Value="ShippingMethod" Text="Shipping Methods" />                                
                               </asp:DropDownList><br />

                                <asp:Panel ID="pnlEntityMap" runat="server" Visible="false">
     
                                    <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch"  
                                        OnFilter="ctrlSearch_Filter" 
                                        OnContentCreated="ctrlSearch_ContentCreated" OnSearchInvoked="ctrlSearch_SearchInvoked">
                    
                                        <Search SearchButtonCaption="Go" runat="server"
                                            SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>" 
                                            SearchTextMinLength="<%$ Tokens:AppConfigUSInt, MinSearchStringLength %>" 
                                            SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>" 
                                            ValidateInputLength="false" 
                                            ShowValidationMessageBox="false" 
                                            ShowValidationSummary="false" 
                                            UseLandingPageBehavior="false" 
                                            WillRenderInUpdatePanel="false"  />
                                
                                        <ContentTemplate>
											<asp:Repeater ID="repeatMap" runat="server"  Visible="false"
												OnItemCreated="repeatMap_ItemCreated" 
												OnItemDataBound="repeatMap_ItemDataBound"
												OnItemCommand="repeatMap_ItemCommand">
												<HeaderTemplate>
													<asp:Button ID="btnSave" runat="server" Text="Save" CommandName="SaveMapping"  />
													<div class="RadGrid RadGrid_Default">
														<table cellpadding="1" cellspacing="0" class="rgMasterTable">
															<tr>
																<td class="rgHeader">Id</td>
																<td class="rgHeader">Name</td>
																<asp:PlaceHolder ID="phStoreHeadTableData" runat="server" />
															</tr>
												</HeaderTemplate>
	                                            
												<ItemTemplate>
													<tr class="rgRow">
														<td>
															<asp:Label ID="lblID" runat="server" Text='<%# DataItemAs<IGrouping<Int32, MappedObject>>(Container).First().ID %>' ></asp:Label>
														</td>
														 <td>
															<asp:Label ID="lblName" runat="server" Text='<%# ML_Localize(DataItemAs<IGrouping<Int32, MappedObject>>(Container).First().Name) %>' ></asp:Label>
														</td>
														<asp:PlaceHolder ID="phStoreRowTableData" runat="server" />
													</tr>
												</ItemTemplate>
												<AlternatingItemTemplate>
													<tr class="rgRow config_alternating_item">
														<td>
															<asp:Label ID="lblID" runat="server" Text='<%# DataItemAs<IGrouping<Int32, MappedObject>>(Container).First().ID %>' ></asp:Label>
														</td>
														 <td>
															<asp:Label ID="lblName" runat="server" Text='<%# ML_Localize(DataItemAs<IGrouping<Int32, MappedObject>>(Container).First().Name) %>' ></asp:Label>
														</td>
														<asp:PlaceHolder ID="phStoreRowTableData" runat="server" />
													</tr>
												</AlternatingItemTemplate>
	                                            
												<FooterTemplate>
														</table>
													</div>
												</FooterTemplate>
											</asp:Repeater>                                            
                                        </ContentTemplate>
                
									</aspdnsf:SearcheableTemplate>
								</asp:Panel>
                                
                        </ContentTemplate>
                    </AJAX:TabPanel>
                </AJAX:TabContainer>            
        </div>
</asp:Content>
