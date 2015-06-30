<%@ Page Language="C#" AutoEventWireup="true" CodeFile="editkit2.aspx.cs" Inherits="AspDotNetStorefrontAdmin.Admin_editkit2" %>

<%@ Register TagPrefix="aspdnsf" TagName="KitGroupTemplate" Src="controls/editkitgrouptemplate.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<head runat="server">
    <style type="text/css">
        .admin_kit_container
        {
            margin-top: 10px;
            margin-left: 10px;
        }
        .admin_kit_group
        {
            border: solid 1px #ccc;
            width: 800px;
            padding-left: 15px;
            padding-bottom: 20px;
            padding-top: 10px;
        }

        .admin_kit_group .ajax__tab_xp .ajax__tab_tab {
            height: 20px;
        }

        .admin_kit_group .admin_kit_group_id
        {
            padding-top: 15px;
            padding-bottom: 5px;
            font-size: 15px;
            font-weight: bold;
        }
        .admin_kit_group .admin_kit_group_leftCommand
        {
            background-color: #eee;
            border: solid 1px #ccc;
            width: 30px;
        }
        .admin_kit_group .admin_kit_group_content
        {
            padding-top: 5px;
            padding-left: 10px;
            padding-right: 20px;
        }
        .admin_kit_group_validationErrors
        {
            color: Red;
        }
        .admin_kit_group_inputError
        {
            background-color: #FFEEEE;
        }
        .admin_kit_group_newItem
        {
            color: #71C144;
        }
        .modal_popup
        {
            background-color: White;
            border: solid 1px #52355;
        }
        .modal_popup_background
        {
            opacity: 0.7;
            background-color: Gray;
            filter: alpha(opacity = 70);
        }
        .admin_variantList_Header
        {
            background: url(images/kit/variantListHeader_background.jpg) repeat;
            height: 30px;
            padding-right: 10px;
            padding-left: 10px;
            color: White;
            text-align: center;
            vertical-align: middle;
            padding-top: 10px;
            font-weight: bold;
            font-size: 12px;
        }
        .admin_variantList_Footer
        {
            background-image: url(images/kit/selectorCell_bg.gif);
            background-repeat: repeat-y;
        }
        .admin_variantList_Header_Close
        {
            float: right;
            padding-right: 7px;
        }
        .admin_kit_updating_animation
        {
            float: right;
            visibility: hidden;
        }
    </style>
</head>
<body>
    <form runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <div class="admin_kit_container">
        Editing Kit :
        <asp:Literal ID="ltProduct" runat="server"></asp:Literal>
        <div style="padding-top: 3px; padding-bottom: 5px;">
            Please enter the following information about this kit. Kits are composed of groups,
            and groups are composed of items. Each item can have a price and weight delta applied
            to the base kit (product) price or weight.</div>
        <asp:UpdatePanel ID="pnlUpdateAllGroups" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel ID="pnlLocale" runat="server">
                    <br />
                    Select Locale:
                    <asp:DropDownList ID="cboLocale" runat="server" AutoPostBack="true" OnSelectedIndexChanged="cboLocale_SelectedIndexChanged">
                    </asp:DropDownList>
                    <br />
                    <br />
                </asp:Panel>
                <asp:DataList ID="dlKitGroups" runat="server" OnItemCommand="dlKitGroups_ItemCommand"
                    OnItemCreated="dlKitGroups_ItemCreated" OnItemDataBound="dlKitGroups_ItemDataBound">
                    <ItemTemplate>
                        <asp:UpdatePanel ID="pnlUpdateKitGroup" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                            <ContentTemplate>
                                <asp:Panel ID="pnlKitGroup" runat="server" CssClass="admin_kit_group">
                                    <asp:HiddenField ID="hdfGroupId" runat="server" Value='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().Id %>' />
                                    <table style="width: 100%">
                                        <tr>
                                            <td align="center" valign="top" class="admin_kit_group_leftCommand">
                                                <br />
                                                <asp:ImageButton ID="cmdSave1" runat="server" CommandName="Save" CommandArgument="cmdSave1"
                                                    ImageUrl="~/App_Themes/Admin_Default/images/kit/save.jpg" />
                                                <br />
                                                <br />
                                                <asp:ImageButton ID="cmdDelete1" runat="server" CommandName="DeleteGroup" Visible='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().IsNew == false %>'
                                                    ImageUrl="~/App_Themes/Admin_Default/images/kit/delete.jpg" />
                                            </td>
                                            <td align="left" valign="top" class="admin_kit_group_content">
                                                <%--
                                                    Top Save notification section upon clicking save:
                                                    We'll only show the update notification if the updated group
                                                    is the one we're currently bound upon and the one that triggered
                                                    the save was the save button in this area.
                                                    We use a panel here so that we can reclaim the space that it used
                                                    when it fades out of animation
                                            --%>
                                                <asp:Panel ID="pnlSaveNotification1" runat="server" Visible='<%# ShouldHighlightNotification(FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().Id, "cmdSave1") %>'>
                                                    <asp:Label ID="lblSaveNotification1" runat="server" Text="[Group Updated....]" ForeColor="#068FE0"></asp:Label>
                                                    <ajax:AnimationExtender ID="extSaveNotification1" runat="server" TargetControlID="lblSaveNotification1"
                                                        Enabled='<%# ShouldHighlightNotification(FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().Id, "cmdSave1") %>'>
                                                        <Animations>                                                     
                                                     <OnLoad>
                                                        <Sequence>
                                                            <Color Duration="2" 
                                                                    StartValue="#3DDB0B" 
                                                                    EndValue="#FFFFFF" 
                                                                    Property="style" PropertyKey="color" />
                                                                    
                                                            <HideAction AnimationTarget="pnlSaveNotification1" />
                                                        </Sequence>
                                                    </OnLoad>
                                                        </Animations>
                                                    </ajax:AnimationExtender>
                                                </asp:Panel>
                                                <%--
                                                We'll use animation extender as update progress notification instead
                                                since it reacts faster than the update progress and we can control
                                                which particular sections we want to display the progress template 
                                                based on which button clicked
                                            --%>
                                                <div id="divProgressTop" runat="server" style="display: none">
                                                    <p style="color: #3DDB0B; font-style: italic;">
                                                        Saving in progress...<img alt="saving" runat="server" src="~/App_Themes/Admin_Default/images/kit/ajax-loader.gif" />
                                                    </p>
                                                </div>
                                                <ajax:AnimationExtender ID="extSaveActionTop" runat="server" TargetControlID="cmdSave1"
                                                    Enabled="true">
                                                    <Animations>                                                     
                                                     <OnClick>
                                                        <Sequence>
                                                            <StyleAction AnimationTarget="divProgressTop" Attribute="display" Value=""/>
                                                        </Sequence>
                                                    </OnClick>
                                                    </Animations>
                                                </ajax:AnimationExtender>
                                                <%--Main KitGroup template--%>
                                                <aspdnsf:KitGroupTemplate ID="ctrlKitGroup" runat="server" KitGroup='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>() %>'
                                                    GroupTypes='<%# this.GroupTypes %>' />
                                                <br />
                                                <div class="admin_kit_command">
                                                    <asp:LinkButton ID="cmdSave2" runat="server" CommandName="Save" CommandArgument="cmdSave2">Save</asp:LinkButton>
                                                    &nbsp;|&nbsp;
                                                    <asp:LinkButton ID="cmdDelete2" runat="server" CommandName="DeleteGroup" Visible='<%# FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().IsNew == false %>'>Delete</asp:LinkButton>
                                                    &nbsp;&nbsp;
                                                    <%--
                                                    We'll only show the update notification if the updated group
                                                    is the one we're currently bound upon and the one that triggered
                                                    the save was the save button in this area
                                                --%>
                                                    <asp:Label ID="lblSaveNotification2" runat="server" Text="[Group Updated....]" ForeColor="#6391AC"
                                                        Visible='<%# ShouldHighlightNotification(FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().Id, "cmdSave2") %>'>
                                                    </asp:Label>
                                                    <ajax:AnimationExtender ID="extSaveNotification2" runat="server" TargetControlID="lblSaveNotification2"
                                                        Enabled='<%# ShouldHighlightNotification(FindContainer<DataListItem>(Container).DataItemAs<KitGroupData>().Id, "cmdSave2") %>'>
                                                        <Animations>                                                     
                                                         <OnLoad>
                                                            <Sequence>
                                                                <Color Duration="2" 
                                                                        StartValue="#3DDB0B" 
                                                                        EndValue="#FFFFFF" 
                                                                        Property="style" PropertyKey="color" />
                                                                <HideAction AnimationTarget="lblSaveNotification2" />
                                                            </Sequence>
                                                        </OnLoad>
                                                        </Animations>
                                                    </ajax:AnimationExtender>
                                                    <%--
                                                    We'll use animation extender as update progress notification instead
                                                    since it reacts faster than the update progress and we can control
                                                    which particular sections we want to display the progress template 
                                                    based on which button clicked
                                                --%>
                                                    <div id="divProgressBottom" runat="server" style="display: none">
                                                        <p style="color: #3DDB0B; font-style: italic;">
                                                            Saving in progress...<img alt="saving" runat="server" src="~/App_Themes/Admin_Default/images/kit/ajax-loader.gif" />
                                                        </p>
                                                    </div>
                                                    <ajax:AnimationExtender ID="extSaveActionBottom" runat="server" TargetControlID="cmdSave2"
                                                        Enabled="true">
                                                        <Animations>                                                     
                                                         <OnClick>
                                                            <Sequence>
                                                                <StyleAction AnimationTarget="divProgressBottom" Attribute="display" Value=""/>
                                                            </Sequence>
                                                        </OnClick>
                                                        </Animations>
                                                    </ajax:AnimationExtender>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                                <br />
                                <br />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </ItemTemplate>
                </asp:DataList>
            </ContentTemplate>
        </asp:UpdatePanel>
        <telerik:RadWindow ID="rwInventoryList" runat="server" VisibleOnPageLoad="false"
            ShowContentDuringLoad="false" VisibleStatusbar="false" Behaviors="Maximize, Close, Move, Resize"
            Width="650px" Height="450px" NavigateUrl="kititeminventoryvariantlist.aspx" Modal="true">
        </telerik:RadWindow>

        <script type="text/javascript">
            
                Type.registerNamespace('aspdnsf.Pages');
                Type.registerNamespace('aspdnsf.Controls');
                
                aspdnsf.Controls.KitItemLineControl = function(id, cmdSelect, txtName, txtDescription, txtVariantId, txtPrice, txtWeight) {
                    this._id = id;
                    
                    this._cmdSelect = $get(cmdSelect);
                    this._txtName = $get(txtName);
                    this._txtDescription = $get(txtDescription);
                    this._txtVariantId = $get(txtVariantId);
                    this._txtPrice = $get(txtPrice);
                    this._txtWeight = $get(txtWeight);
                    
                    // this controls could already be registered during the first page load
                    // check to see if this is still existing, otherwise they've probably been kicked out of the DOM via updatepanel refresh
                    // due to line item deletion 
                    if(this._cmdSelect) {
                        $addHandler(this._cmdSelect, 'click', Function.createDelegate(this, this.onSelectCommand));
                    }
                    
                    this.selectedHandler = null;
                }
                aspdnsf.Controls.KitItemLineControl.prototype = {
                
                    add_selected : function(handler) {
                        this.selectedHandler = handler;
                    },
                    
                    raiseSelected : function() {
                        this.selectedHandler(this);
                    },
                
                    onSelectCommand : function() {
                        this.raiseSelected();
                    },
                
                    initialize : function() {
                        
                    },
                    
                    dispose : function() {
                    },
                
                    get_Id : function() {
                        return this._id;
                    },                    
                    
                    setVariantData : function(variant) {
                        this._txtVariantId.value =    variant.Id;
                        
                        var allowPopulate = true;
                        if(this._txtName.value.trim() != '') {
                            var overwrite = confirm('Do you want to overwrite the existing name, description and price delta?');
                            allowPopulate = overwrite;
                        }
                        
                        if(allowPopulate)
                        {
                            this._txtName.value =         variant.Name;
                            this._txtDescription.value =  variant.Description;
                            
                            if(variant.SalePrice > 0) {
                                this._txtPrice.value =        variant.SalePrice;
                            }
                            else {
                                this._txtPrice.value =        variant.Price;
                            }
                            
                            this._txtWeight.value =       variant.Weight;
                        }
                        
                        this.provideHint();
                    },

                    provideHint: function() {
                        var sequence = AjaxControlToolkit.Animation.createAnimation(
                        {
                                AnimationName: "Sequence",
                                AnimationTarget: this._txtVariantId.id
                        });
                        
                        sequence.play();
                    }
                    
                }
                aspdnsf.Controls.KitItemLineControl.registerClass('aspdnsf.Controls.KitItemLineControl');

                aspdnsf.Pages.$EditKit = function() {
                    this.currentControl = null;
                    this.currentExtender = null;
                    
                    this.kitItemControls = new Array();
                    //this.modalPopup = null;
                }
                aspdnsf.Pages.$EditKit.registerClass('aspdnsf.Pages.$EditKit');
                aspdnsf.Pages.$EditKit.prototype = {
                
                    addKitItemLineControl : function(ctrl) {
                        var handler = Function.createDelegate(this, this.onKitItemLineControl_Selected);
                        ctrl.add_selected(handler);
                        this.kitItemControls.push(ctrl);
                    },
                    
                    ensureModalPopup : function() {
                    },
                    
                    onKitItemLineControl_Selected : function(sender, e) {
                        //alert(e);
                        this.currentControl = sender;
                        //this.modalPopup.show();
                        this.showVariantList();
                    },
                    
                    pushData: function(value) {
                        //this.modalPopup.hide();
                        this.hideVariantList();
                        var data = eval(value);
                        this.currentControl.setVariantData(data);
                    },
                    
                    /*
                    set_ModalPopup: function(id) {
                        this.modalPopup = $find(id);
                    },
                    */
                    
                    showVariantList: function() {
                        var rwInventoryList = $find('rwInventoryList');
                        if(rwInventoryList) {
                            // hack the internal flag to force the window to recompute
                            // the x and y display, the variable below is an internal cached
                            // of bounds information before hiding.
                            // This is so that we always have the popup display on the center locatio
                            // of the window regardless on where the customer is currently scrolled to
                            rwInventoryList._restoreRect = null;
                            rwInventoryList.show();
                        }
                    },
                    
                    hideVariantList: function() {
                        var rwInventoryList = $find('rwInventoryList');
                        if(rwInventoryList) {
                            rwInventoryList.hide();
                        }
                    }                    

                }

                aspdnsf.Pages.EditKit = new aspdnsf.Pages.$EditKit();

                window.aspdnsf.Pages.EditKit = aspdnsf.Pages.EditKit;

                /*
                Sys.Application.add_load(function() {
                    aspdnsf.Pages.EditKit.set_ModalPopup('extModalPopup');
                });
                */
                
                
        </script>

    </div>
    </form>
</body>
