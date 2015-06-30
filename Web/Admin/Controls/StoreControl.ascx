<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StoreControl.ascx.cs" Inherits="AspDotNetStorefrontControls.StoreControl" %>
<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>
<%@ Register TagPrefix="AJAX" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreEdit" src="StoreEdit.ascx" %>


<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>   

<asp:UpdatePanel runat="server" ID="updMain">
    <Triggers>
    </Triggers>
    <ContentTemplate>
    
    <telerik:RadWindowManager ID="rwMan" runat="server">
    </telerik:RadWindowManager>    
            
    <asp:Button ID="cmdAddStore" runat="server" 
                Text="<%$ Tokens:StringResource, StoreControl.AddStore %>"  />         
    &nbsp;
    
    <br />
    <br />
    
    <aspdnsf:StoreEdit id="ctrlAddStore" runat="server" 
            CssClass="modal_popup_Content" 
            HeaderText="Add New Store"            
            PopupTargetControlID="cmdAddStore"
            VisibleOnPageLoad="false" OnUpdatedChanges="ctrlStore_UpdatedChanges" />

            
            
            <telerik:RadGrid ID="grdStores" runat="server" GridLines="None" 
                                AllowSorting="true" 
                                OnItemCreated="grdStores_ItemCreated"
                                OnSortCommand="grdStores_SortCommand"  
                                OnItemCommand="grdStores_ItemCommand" 
                                OnItemDataBound="grdStores_ItemDataBound">
                <HeaderContextMenu>
                    <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                </HeaderContextMenu>
                
                <ClientSettings>
                    <Resizing AllowColumnResize="false" EnableRealTimeResize="false" ResizeGridOnColumnResize="true" ClipCellContentOnResize="false" />
                </ClientSettings>

                <MasterTableView AutoGenerateColumns="False">
                    <RowIndicatorColumn>
                    <HeaderStyle Width="20px"></HeaderStyle>
                    </RowIndicatorColumn>

                    <ExpandCollapseColumn>
                    <HeaderStyle Width="20px"></HeaderStyle>
                    </ExpandCollapseColumn>
                    
                    <Columns>
                    
                        <telerik:GridTemplateColumn>
                        
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                            
                            <ItemTemplate>
                                <asp:Label ID="lblStoreID" runat="server" Text='<%# DataItemAs<Store>(Container).StoreID %>' />
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        
                        <telerik:GridTemplateColumn>
                            <HeaderStyle Width="200px" />
                            <ItemStyle Width="200px" />
                            
                            <HeaderTemplate>
                                <div style="padding-left:35px;">
                                    <asp:Label ID="lblStoreNameHeader" runat="server" 
                                        Text="<%$ Tokens:StringResource, Global.StoreName %>" />
                                </div>
                            </HeaderTemplate>
                            <ItemTemplate>                           
                            
                            
                                <div style="padding-left:10px;">
                                    
                                     <asp:ImageButton ID="btnEditStore" runat="server" 
                                        ImageUrl="~/App_Themes/Admin_Default/images/edit.png" 
                                        CommandName="E_Edit" 
                                        CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>' />
                                    
                                    <asp:LinkButton ID="lnkStoreName" runat="server" 
                                        Text='<%# DataItemAs<Store>(Container).Name %>'>
                                    </asp:LinkButton>
                                </div>
                              
                                <aspdnsf:StoreEdit id="ctrlEditStore" runat="server" 
                                    CssClass="modal_popup_Content" 
                                    ThisCustomer='<%# ThisCustomer %>'
                                    HeaderText="Edit Store"
                                    Skins='<%# Skins %>'
                                    PopupTargetControlID="lnkStoreName"
                                    VisibleOnPageLoad="false"
                                    Datasource='<%# DataItemAs<Store>(Container) %>' />
                                
                                
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>                        
                        
                        
                        <telerik:GridTemplateColumn>
                            
                             <HeaderTemplate>
                                <div style="padding-left:5px;">
                                    <asp:Label ID="lblUrls" runat="server" Text="Urls" />
                                </div>
                            </HeaderTemplate>
                            
                            <ItemTemplate>
                            
                                <div class="url_area">
                                
                                    <table cellspacing="0" cellpadding="0" border="0" width="100%" class="url_area" > 
                                        <tr >
                                            <td class="tdUrlCaption" style="border-style:none;font-size:smaller;" >
                                                <asp:Literal ID="Literal1" runat="server" Text="<%$ Tokens:StringResource, StoreControl.ProductionURI %>" />
                                            </td>
                                            <td class="tdUrl" style="border-style:none;font-size:smaller;" >
                                                <asp:HyperLink ID="lnkProductionUri" runat="server" 
                                                    Text='<%# DataItemAs<Store>(Container).ProductionURI %>' 
                                                    NavigateUrl='<%# HTTPFy(DataItemAs<Store>(Container).ProductionURI) %>' 
                                                    Target="_blank">
                                                </asp:HyperLink>
                                                <%--<%# Eval("ProductionURI") %>--%>
                                            </td>
                                            <td class="tdUrlOptions" style="border-style:none;font-size:smaller;" >
                                                <asp:Image ID="Image1" runat="server" 
                                                    ImageUrl="~/App_Themes/Admin_Default/images/accept.png" 
                                                    Visible='<%# Eval("ProductionURILicensed")%>' 
                                                    ToolTip="<%$ Tokens:StringResource, StoreControl.SiteLicensed %>"  />
                                                <asp:Image ID="Image2" runat="server" 
                                                    ImageUrl="~/App_Themes/Admin_Default/images/cross.png" 
                                                    Visible='<%# (bool)Eval("ProductionURILicensed") == false %>' 
                                                    ToolTip="<%$ Tokens:StringResource, StoreControl.SiteUnLicensed %>"  />
                                            </td>
                                        </tr>
                                        <tr class="url_area" >
                                            <td class="tdUrlCaption" style="border-style:none;font-size:smaller;" >
                                                <asp:Literal ID="Literal2" runat="server" Text="<%$ Tokens:StringResource, StoreControl.StagingURI %>" />
                                            </td>                                            
                                            <td class="tdUrl" style="border-style:none;font-size:smaller;" >
                                                <asp:HyperLink ID="lnkStagingURI" runat="server" 
                                                    Text='<%# DataItemAs<Store>(Container).StagingURI %>' 
                                                    NavigateUrl='<%# HTTPFy(DataItemAs<Store>(Container).StagingURI) %>'
                                                    Target="_blank">
                                                </asp:HyperLink>
                                                <%--<%# Eval("StagingURI") %>--%>
                                            </td>
                                            <td class="tdUrlOptions" style="border-style:none;font-size:smaller;" >
                                                <asp:Image ID="Image3" runat="server" 
                                                    ImageUrl="~/App_Themes/Admin_Default/images/accept.png" 
                                                    Visible='<%# Eval("StagingURILicensed")%>'  
                                                    ToolTip="<%$ Tokens:StringResource, StoreControl.SiteLicensed %>"  />
                                                <asp:Image ID="Image4" runat="server" 
                                                    ImageUrl="~/App_Themes/Admin_Default/images/cross.png" 
                                                    Visible='<%# (bool)Eval("StagingURILicensed") == false %>'  
                                                    ToolTip="<%$ Tokens:StringResource, StoreControl.SiteUnLicensed %>"  />
                                            </td>
                                        </tr>
                                        <tr class="url_area" >
                                            <td class="tdUrlCaption" style="border-style:none;font-size:smaller;" >
                                               <asp:Literal ID="Literal3" runat="server" Text="<%$ Tokens:StringResource, StoreControl.DevelopmentURI %>" />
                                            </td>
                                            <td class="tdUrl" style="border-style:none;font-size:smaller;" >
                                                <asp:HyperLink ID="lnkDevelopmentURI" runat="server" 
                                                    Text='<%# DataItemAs<Store>(Container).DevelopmentURI %>' 
                                                    NavigateUrl='<%# HTTPFy(DataItemAs<Store>(Container).DevelopmentURI) %>'
                                                    Target="_blank">
                                                </asp:HyperLink>
                                                <%--<%# Eval("DevelopmentURI") %>--%>
                                            </td>
                                            <td class="tdUrlOptions" style="border-style:none;font-size:smaller;" >
                                                <asp:Image ID="Image5" runat="server" 
                                                    ImageUrl="~/App_Themes/Admin_Default/images/accept.png" 
                                                    Visible='<%# Eval("DevelopmentURILicensed")%>' 
                                                    ToolTip="<%$ Tokens:StringResource, StoreControl.SiteLicensed %>"  />
                                                <asp:Image ID="Image6" runat="server" 
                                                    ImageUrl="~/App_Themes/Admin_Default/images/cross.png" 
                                                    Visible='<%# (bool)Eval("DevelopmentURILicensed") == false %>'  
                                                    ToolTip="<%$ Tokens:StringResource, StoreControl.SiteUnLicensed %>"  />
                                            </td>
                                        </tr>
                                    </table>
                                
                                </div>
                            
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        
                        <%--Default column--%>
                        <telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, StoreControl.Default %>" >
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemTemplate>
                                <aspdnsf:DataCheckBox ID="chkDefault" runat="server" 
                                    Checked='<%# DataItemAs<Store>(Container).IsDefault %>' 
                                    Enabled='<%# Datasource.Count > 1 %>'
                                    Data='<%# DataItemAs<Store>(Container).StoreID %>'
                                    AutoPostBack="true"
                                    Visible='<%# DataItemAs<Store>(Container).Deleted == false && DataItemAs<Store>(Container).Published %>' />
                            </ItemTemplate>
                        </telerik:GridTemplateColumn> 
                        
                        <%--Published column--%>
                        <telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, StoreControl.Published %>" >
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemTemplate>
                            
                                <asp:LinkButton ID="lnkPublishToggle" 
                                    runat="server"
                                    CommandName="PublishToggle" 
                                    Text='<%# PublishText(DataItemAs<Store>(Container)) %>'
                                    CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>' 
                                    Visible='<%# Datasource.Count > 1 && DataItemAs<Store>(Container).Deleted == false && DataItemAs<Store>(Container).IsDefault == false %>' />
                                
                            </ItemTemplate>
                        </telerik:GridTemplateColumn> 
                        
                        <%--Clone column--%>
                        <telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, Global.CloneButtonText %>" >
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemTemplate>
                                <asp:ImageButton ID="btnCloneStore" runat="server" 
                                    ImageUrl="~/App_Themes/Admin_Default/images/Application_double.png" 
                                    CommandName="CloneStore" 
                                    CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>' 
                                    Visible='<%# Datasource.Count > 1 && DataItemAs<Store>(Container).Deleted == false && DataItemAs<Store>(Container).Published  %>' />
                                
                                <aspdnsf:StoreEdit id="StoreEdit1" runat="server" 
                                    CssClass="modal_popup_Content" 
                                    CloneMode="true"
                                    ThisCustomer='<%# ThisCustomer %>'
                                    HeaderText="Clone Store"
                                    Skins='<%# Skins %>'
                                    PopupTargetControlID="btnCloneStore"
                                    VisibleOnPageLoad="false"
                                    Datasource='<%# DataItemAs<Store>(Container) %>' />
                                    
                            </ItemTemplate>
                        </telerik:GridTemplateColumn> 
                        
                         <%--Copy to Store column--%>
                        <telerik:GridTemplateColumn HeaderText="Copy From Store" >
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemTemplate>
                            
                                <asp:ImageButton ID="btnCopyStore" runat="server" 
                                    ImageUrl="~/App_Themes/Admin_Default/images/Application_double.png" 
                                    Visible='<%# Datasource.Count > 1 && DataItemAs<Store>(Container).Deleted == false && DataItemAs<Store>(Container).Published %>' />
                                
                                 <%-- panel to show store selection for which to copy store to --%>
                                <div id="pnlCopyFromStore" runat="server" style="display:none; text-align:left;" class="modal_popup" >   
                                    <div class="modal_popup_Header">Copy From Store</div>
                                    
                                    <asp:Panel ID="pnlCopyStoreMain" runat="server" Visible="true" CssClass="store_edit_panel" >
                                        <p>
                                            Please select which store to copy settings into the current selected store.
                                            <br />
                                            Keep in mind that that this will <b>overwrite</b> the original settings on the destination store.
                                            <br />
                                            <span style="color:red;font-weight:bold;">This action is not undo-able</span>, click Ok if you wish to proceed.
                                            <br />
                                        </p>
                                        
                                        Copy settings From:&nbsp;
                                        <%--datasource and values will be populated on code-behind upon itemdatabound--%>
                                        <asp:DropDownList ID="cboCopystoreFrom" runat="server">
                                        </asp:DropDownList>
                                        
                                        <%--&nbsp;-&gt;&nbsp;--%>
                                        &nbsp;<img runat="server" src="~/App_Themes/Admin_Default/images/next.gif" alt="copy to" />&nbsp;
                                        
                                        Into:&nbsp;<asp:Label ID="lblCopyToStore" runat="server" Text='<%# DataItemAs<Store>(Container).Name %>'></asp:Label>
                                    </asp:Panel>
                                    
                                    <div align="center" class="modal_popup_Footer" >
                                        <asp:Button ID="btnCopyStoreFrom" runat="server" Text="Ok" 
                                            CommandName="CopyStore" 
                                            CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>'/>
                                        <asp:Button ID="btnCancelCopyStore" runat="server" Text="Cancel" />     
                                    </div>
                                    
                                     <div id="divProgressCopyGrid" runat="server" style="display:none;" align="center" >
                                        <p style="color:#3DDB0B;font-style:italic;">Saving in progress...<img alt="saving" runat="server" src="~/App_Themes/Admin_Default/images/ajax-loader-1.gif" /> </p> 
                                    </div>
                                    
                                </div>
                               
                                <ajax:ModalPopupExtender ID="extCopyFromStore" runat="server" 
                                    PopupControlID="pnlCopyFromStore" 
                                    BackgroundCssClass="modal_popup_background" 
                                    CancelControlID="btnCancelCopyStore" 
                                    TargetControlID="btnCopyStore" >
                                </ajax:ModalPopupExtender>
                                
                                 <%--
                                    We'll use animation extender as update progress notification instead
                                    since it reacts faster than the update progress and we can control
                                    which particular sections we want to display the progress template 
                                    based on which button clicked
                                --%>                               
                                
                                <ajax:AnimationExtender ID="extCopyFromStoreAnimation" runat="server" 
                                    TargetControlID="btnCopyStoreFrom" Enabled="true" >
                                     <Animations >                                                     
                                         <OnClick>
                                            <Sequence>
                                                <EnableAction AnimationTarget="btnCopyStoreFrom" Enabled="false" />
                                                <EnableAction AnimationTarget="btnCancelCopyStore" Enabled="false" />
                                                <StyleAction AnimationTarget="divProgressCopyGrid" Attribute="display" Value=""/>
                                            </Sequence>
                                        </OnClick>
                                     </Animations>
                                </ajax:AnimationExtender> 

                                    
                            </ItemTemplate>
                        </telerik:GridTemplateColumn> 
                        
                        <%--Delete column--%>
                        <telerik:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, Global.DeleteButtonText %>">
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" Width="60px" />
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkDeleteToggle" 
                                    runat="server"
                                    CommandName="DeleteToggle" 
                                    Text='<%# DeleteText(DataItemAs<Store>(Container)) %>'
                                    CommandArgument='<%# DataItemAs<Store>(Container).StoreID %>' 
                                    Visible=<%# Datasource.Count > 1 && DataItemAs<Store>(Container).IsDefault == false %> >
                                </asp:LinkButton>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn> 
                        
                    </Columns>
                </MasterTableView>
                
            </telerik:RadGrid>
        
       <br />
       
            
        <asp:HiddenField ID="txtY" runat="server" />
        <asp:HiddenField ID="txtX" runat="server" />
        <asp:Literal ID="litDLFrame" runat="server" />
        
    </ContentTemplate>
</asp:UpdatePanel>



