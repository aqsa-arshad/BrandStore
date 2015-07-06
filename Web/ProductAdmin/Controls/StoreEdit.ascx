<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StoreEdit.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.StoreEdit" %>
<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>
<%@ Register TagPrefix="AJAX" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>   

<style type="text/css" >
    
    .store_edit_panel
    {
    	padding:10px 15px 15px 10px;
    }

    .tblEditStore,
    .tblEditStore tr,
    .tblEditStore td
    {
    	border-style:none;
    }
    
    .leadCell
    {
    	width:20%;
    	text-align:right;
    	vertical-align:top;
    }
    .inputCell
    {
    	width:80%;
    	text-align:left;
    	vertical-align:top;
    }
    
</style>

<asp:Panel id="pnlEditStore" runat="server" DefaultButton="cmdSave" CssClass="modal_popup" >                                                
  <div class="modal_popup_Header"> <%= this.HeaderText  %></div>
  <asp:Panel ID="pnlMain" runat="server" Visible="true" CssClass="store_edit_panel" >
        <table class="tblEditStore" style="width: 37%;">
        <tr>
            <td>
            </td>
            <td colspan="2" >
                <asp:Label runat="Server" ID="lblStoreNameError" ForeColor="Red" /> 
            </td>            
        </tr>
            <tr>
                <td class="leadCell" valign="top" align="right" >
                    <asp:Label ID="ASPDNSFLabel1" runat="server" Text="<%$ Tokens:StringResource, Global.StoreName %>"></asp:Label>
                    
                </td>
                <td class="inputCell" valign="top" align="left" >
                    <asp:TextBox ID="txtStoreName" runat="server" 
                        Columns="30" Text='<%# Datasource.Name %>'></asp:TextBox>
                    &nbsp;
                    <asp:CheckBox ID="chkPublished" runat="server" 
                        Text="<%$ Tokens:StringResource, StoreControl.Published %>"
                        Checked=<%# Datasource.Published %> />
                    <asp:PlaceHolder runat="server" ID="phRegisterWithBuySafe" Visible="false">
                        &nbsp;
                        <asp:CheckBox ID="cbxBuySafe" runat="server" 
                        Text="Add to buySafe"
                        Checked="true" />
                    </asp:PlaceHolder>
                </td>
            </tr>
            
            <tr>
                <td class="leadCell" valign="top" align="right" >
                    <asp:Label ID="lblProdctionURI" runat="server" 
                        Text="<%$ Tokens:StringResource, StoreControl.ProductionURI %>"></asp:Label>
                </td>
                <td class="inputCell" valign="top" align="left" >
                    <asp:TextBox ID="txtProductionURI" runat="server" 
                        Columns="65" 
                        Text='<%# Datasource.ProductionURI %>' ></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="leadCell" valign="top" align="right" >
                    <asp:Label ID="Label1" runat="server" 
                        Text="<%$ Tokens:StringResource, StoreControl.StagingURI %>"></asp:Label>
                </td>
                <td class="inputCell" valign="top" align="left" >
                    <asp:TextBox ID="txtStagingURI" runat="server" 
                        Columns="65" 
                        Text='<%# Datasource.StagingURI %>' ></asp:TextBox>
                </td>
            </tr>
            
            <tr>
                <td class="leadCell" valign="top" align="right" >
                    <asp:Label ID="Label2" runat="server" 
                        Text="<%$ Tokens:StringResource, StoreControl.DevelopmentURI %>"></asp:Label>
                </td>
                <td class="inputCell" valign="top" align="left" >
                    <asp:TextBox ID="txtDevURI" runat="server" 
                        Columns="65" 
                        Text='<%# Datasource.DevelopmentURI %>' ></asp:TextBox>
                </td>
            </tr>        
           
            <tr>
                <td class="leadCell" valign="top" align="right" >
                    <asp:Label ID="lblSkinID" runat="server" Text="<%$ Tokens:StringResource, StoreControl.SkinID %>"></asp:Label>
                </td>
                <td class="inputCell" valign="top" align="left" >
                    <asp:DropDownList ID="cmbSkinID" runat="server" >
                    </asp:DropDownList>                
                </td>
            </tr>
            <tr>
                <td class="leadCell" valign="top" align="right" >
                    <asp:Label ID="lblDescription" runat="server" Text="<%$ Tokens:StringResource, StoreControl.Description %>"></asp:Label>
                </td>
                <td>
                    <asp:TextBox ID="txtDescription" runat="server" 
                        class="txtDescription" 
                        Columns="65" 
                        Rows="4"
                        TextMode="MultiLine"
                        Text='<%# Datasource.Description %>' ></asp:TextBox>
                </td>
            </tr>
            
        </table>
    </asp:Panel>
    
     <div align="center" class="modal_popup_Footer" >
            
            <asp:Button ID="cmdSave" runat="server" 
                Text="<%$ Tokens:StringResource, Global.SaveButtonText %>"
                CommandName="UpdateStore" 
                CommandArgument="<%# Datasource.StoreID %>" 
                OnClick="cmdSave_Click" /> &nbsp;
            <asp:Button ID="cmdCancel" runat="server" 
                Text="<%$ Tokens:StringResource, Global.CancelButtonText %>" />&nbsp;
            <%--<asp:Button ID="cmdUpdateLicense" runat="server" 
                Text="<%$ Tokens:StringResource, StoreControl.UpdateLicense %>" 
                Visible='<%# Datasource.ProductionURILicensed == false || Datasource.StagingURILicensed == false || Datasource.DevelopmentURILicensed == false %>' 
                Enabled="false"
                />--%>
            
             <%--
                We'll use animation extender as update progress notification instead
                since it reacts faster than the update progress and we can control
                which particular sections we want to display the progress template 
                based on which button clicked
            --%>
            <div id="divProgressGrid" runat="server" style="display:none">
                <p style="color:#3DDB0B;font-style:italic;">Save in progress...<img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-1.gif" /></p> 
            </div>
            
            <ajax:AnimationExtender ID="extSaveStoreGrid" runat="server" TargetControlID="cmdSave" Enabled="true" >
                 <Animations >                                                     
                     <OnClick>
                        <Sequence>
                            <EnableAction AnimationTarget="cmdSave" Enabled="false" />
                            <EnableAction AnimationTarget="cmdCancel" Enabled="false" />
                            <StyleAction AnimationTarget="divProgressGrid" Attribute="display" Value=""/>
                        </Sequence>
                    </OnClick>
                 </Animations>
            </ajax:AnimationExtender>
        </div>                                                
    </asp:Panel>

    <%--Target control that will trigger this popup is not contained here so we use the PoupControlId property as reference instead--%>
    <ajax:ModalPopupExtender ID="extEditStorePanel" runat="server" 
        PopupControlID="pnlEditStore" 
        BackgroundCssClass="modal_popup_background" 
        CancelControlID="cmdCancel" >
    </ajax:ModalPopupExtender> 

