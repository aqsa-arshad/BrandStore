<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GlobalConfigList.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.GlobalConfigList" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="EditGlobalConfig" src="GlobalConfigEdit.ascx" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>   

<%--
      We enclose the progress bar inside a div panel 
      so that we can control it's visibility
      since by default, the progress template will trigger upon
      every asynchronous postback request. Although there are 2
      async scenarios that this panel should be hidden, that is through
      1. Add AppConfig
      2. Save Appconfig
      Which we use animation extenders to render the progress template
      since they react faster to save button clicks and more suited
      to those specialized scenarios. Therefore for those 2 requests
      We hide the generic progress template
--%>      

 Config Groups:  <asp:DropDownList ID="cboConfigGroups" runat="server" 
                    AutoPostBack="true" 
                    onselectedindexchanged="cboConfigGroups_SelectedIndexChanged">
                </asp:DropDownList>


<aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch"  OnFilter="ctrlSearch_Filter"
                OnContentCreated="ctrlSearch_ContentCreated"  >
                            
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
                                 
                            <telerik:RadGrid ID="grdConfigs" runat="server" GridLines="None" 
                                AllowSorting="true" 
                                OnItemCreated="grdAppConfigs_ItemCreated"
                                OnSortCommand="grdAppConfigs_SortCommand"  
                                OnItemCommand="grdAppConfigs_ItemCommand">
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
                                        <telerik:GridTemplateColumn UniqueName="columnEdit" ItemStyle-Width="35px"  >
                                            <ItemStyle HorizontalAlign="Center" />
                                            <ItemTemplate >
                                                
                                                <asp:ImageButton ID="imgEdit" runat="server" 
                                                    ImageUrl="~/App_Themes/Admin_Default/images/edit.png" />
                                                
                                                <ajax:ModalPopupExtender ID="extConfigPanelEdit" runat="server" 
                                                    PopupControlID="pnlConfig" 
                                                    TargetControlID="imgEdit" 
                                                    BackgroundCssClass="modal_popup_background" 
                                                    CancelControlID="btnCancel" >
                                                </ajax:ModalPopupExtender>
                                                
                                            </ItemTemplate>                                            
                                        </telerik:GridTemplateColumn>
                                        
                                        <telerik:GridTemplateColumn HeaderText="Name" UniqueName="TemplateColumn1" ItemStyle-Width="60px" SortExpression="Name" >
                                            <ItemTemplate>
                                                <asp:LinkButton ID="lnkName" runat="server" 
                                                    Text='<%# DataItemAs<GlobalConfig>(Container).Name %>'>
                                                </asp:LinkButton>
                                                
                                                <asp:Panel id="pnlConfig" runat="server" DefaultButton="btnSave" CssClass="modal_popup" >                                                
                                                    <div class="modal_popup_Header">Edit Config</div>
                                                    <aspdnsf:EditGlobalConfig id="ctrlConfig" runat="server" 
                                                        CssClass="modal_popup_Content" 
                                                        ThisCustomer='<%# ThisCustomer %>'
                                                        ConfigGroups='<%# ConfigGroups %>'
                                                        Stores='<%# Stores %>'
                                                        Datasource='<%# DataItemAs<GlobalConfig>(Container) %>' />
                                                    <div align="center" class="modal_popup_Footer" >
                                                        <asp:Button ID="btnSave" runat="server" 
                                                            Text="Save" 
                                                            CommandName="UpdateConfig" 
                                                            CommandArgument='<%# DataItemAs<GlobalConfig>(Container).ID %>' />
                                                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" />
                                                        
                                                         <%--
                                                            We'll use animation extender as update progress notification instead
                                                            since it reacts faster than the update progress and we can control
                                                            which particular sections we want to display the progress template 
                                                            based on which button clicked
                                                        --%>
                                                        <div id="divProgressGrid" runat="server" style="display:none">
                                                            <p style="color:#3DDB0B;font-style:italic;">Saving in progress...<img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-1.gif" /> </p> 
                                                        </div>
                                                        
                                                        <ajax:AnimationExtender ID="extSaveConfigGrid" runat="server" TargetControlID="btnSave" Enabled="true" >
                                                             <Animations >                                                     
                                                                 <OnClick>
                                                                    <Sequence>
                                                                        <EnableAction AnimationTarget="btnSave" Enabled="false" />
                                                                        <EnableAction AnimationTarget="btnCancel" Enabled="false" />
                                                                        <%--Hide the main progress template since we have our own specialized--%>
                                                                        <HideAction AnimationTarget="pnlLoadingGeneral" />
                                                                        <StyleAction AnimationTarget="divProgressGrid" Attribute="display" Value=""/>
                                                                    </Sequence>
                                                                </OnClick>
                                                             </Animations>
                                                        </ajax:AnimationExtender>
                                                    </div>                                                
                                                </asp:Panel>
                                                
                                                <ajax:ModalPopupExtender ID="extConfigPanel" runat="server" 
                                                    PopupControlID="pnlConfig" 
                                                    TargetControlID="lnkName" 
                                                    BackgroundCssClass="modal_popup_background" 
                                                    CancelControlID="btnCancel" >
                                                </ajax:ModalPopupExtender>          
                                            </ItemTemplate>
                                        </telerik:GridTemplateColumn>
                                        <telerik:GridTemplateColumn HeaderText="Description" UniqueName="TemplateColumn2" ItemStyle-Width="250px" SortExpression="Description" >
                                            <ItemTemplate>
                                                <asp:Label ID="lblDescription" runat="server" 
                                                    Text='<%# DataItemAs<GlobalConfig>(Container).Description %>'></asp:Label>
                                            </ItemTemplate>
                                        </telerik:GridTemplateColumn>
                                       <telerik:GridTemplateColumn HeaderText="Value" UniqueName="TemplateColumn3" ItemStyle-Width="150px" SortExpression="ConfigValue" >
                                            <ItemTemplate>
                                                <div id="test" runat="server" style="width:100%;height:20px;white-space:no-wrap;">
                                                    <asp:Label ID="lblConfigValue" runat="server" Text='<%# TrimText(DataItemAs<GlobalConfig>(Container).ConfigValue, 70) %>'></asp:Label>
                                                </div>
                                            </ItemTemplate>
                                        </telerik:GridTemplateColumn>
                                        <telerik:GridTemplateColumn HeaderText="GroupName" UniqueName="TemplateColumn" ItemStyle-Width="100px" SortExpression="GroupName" >
                                            <ItemTemplate>
                                                <asp:Label ID="lblGroupName" runat="server" Text='<%# DataItemAs<GlobalConfig>(Container).GroupName %>'></asp:Label>
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






