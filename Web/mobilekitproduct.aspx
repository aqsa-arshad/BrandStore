<%@ Page ClientTarget="UpLevel" Language="c#" Inherits="AspDotNetStorefront.mobilekitproduct" EnableViewState="true" CodeFile="mobilekitproduct.aspx.cs" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Skin_1/template.master" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls.Extenders" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="MobileGroupTemplate" src="controls/kit/MobileGroupTemplate.ascx" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server">
        <asp:Literal ID="litTempFileStub" runat="server"></asp:Literal>
        <asp:Literal ID="litKitHeader" runat="server"></asp:Literal>
        <asp:Label ID="ltKitError" CssClass="errorLg" runat="server" />
        <asp:Panel ID="pnlUnOrderableKit" runat="server" Visible="false" >
            <ul data-role="listview">
                <li class="group" style="color:Red;" data-role="list-divider">
                    <asp:Literal ID="Literal2" runat="server" Text='<%$ Tokens:StringResource, kitproduct.aspx.18 %>' />
                </li>
            </ul>
        </asp:Panel>
        <asp:Repeater ID="rptKitGroups" runat="server" >
            <ItemTemplate>            
                <aspdnsf:MobileGroupTemplate ID="ctrlKitGroupTemplate" runat="server"  ThisCustomer='<%# ThisCustomer %>' KitGroup='<%# Container.DataItemAs<KitGroupData>() %>' />
            </ItemTemplate>
        </asp:Repeater>
        <div id="KitPlaceHolder" style="width:150px;" ></div>
        <div id="KitSideBar" class="kit_sidebar" >
            <asp:Literal runat="server" ID="litIsCallToOrder" ></asp:Literal>
            <ul data-role="listview">
                <li class="group" data-role="list-divider">
                    <asp:Literal ID="litSummaryHeader" runat="server" Text='<%$ Tokens:StringResource, kitproduct.aspx.20 %>' />
                </li>
            </ul>        
            <asp:UpdatePanel ID="pnlUpdateSummary" runat="server" UpdateMode="Always" >
                <ContentTemplate>
                    <asp:Panel ID="pnlPrice" runat="server" CssClass="kit_priceDisplay">
                        <ul data-role="listview">
                                            
                            <% if (KitData.IsDiscounted) { %>
                            <li>
                                <asp:Label ID="lblRegularBasePrice" runat="server" CssClass="kit_regBasePrice" ></asp:Label>
                            </li>
                            <% } %>
                                                    
                            <% if (AppLogic.AppConfigBool("HideKitPrice") == false) { %>
                            <li>
                                <asp:Label ID="lblBasePrice" runat="server" CssClass="kit_basePrice" ></asp:Label>
                            </li>
                            <%} %>
                                                    
                            <li>
                            <asp:Label ID="lblCustomizedPrice" runat="server" CssClass="kit_customizedPrice"></asp:Label>
                            </li>
                                                    
                            <% if (KitData.HasCustomerLevelPricing) { %>
                            <li>
                                <asp:Label ID="lblLevelPrice" runat="server" CssClass="kit_levelPrice"></asp:Label>
                            </li>
                            <% } %>
                            
                        </ul>
                        <ajax:AnimationExtender ID="AnimationExtender1" runat="server" TargetControlID="pnlPrice"  >
                            <Animations >             
                            </Animations>
                        </ajax:AnimationExtender>
                    </asp:Panel>
                                                
                    <asp:Panel ID="pnlAddToCart" runat="server" Visible="true">
                        <ul data-role="listview">
                            <li>
                                Quantity: 
                                <asp:TextBox ID="txtQuantity" runat="server" Text="1" Columns="2" MaxLength="3" AutoPostBack="true" ontextchanged="txtQuantity_TextChanged"  ></asp:TextBox>
                                <asp:DropDownList ID="ddQuantity" runat="server" Visible="false" />
                                <aspdnsf:TextStateChangePostbackBehavior ID="ext_txtQuantity" runat="server" TargetControlID="txtQuantity" Timeout="1000" MonitorTextChanged="true" />
                            </li>
                            <li>
                                <%if (AppLogic.AppConfigBool("AddToCart.UseImageButton")){ %>
                                                    
                                    <asp:ImageButton ID="btnImgAddToCart" runat="server"  
                                        ImageUrl='<%$ Tokens:Invoke, AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToCartButton"))  %>' 
                                        AlternateText='<%$ Tokens:StringResource, AppConfig.CartButtonPrompt %>'
                                        CssClass="AddToCartButton"
                                        onclick="btnAddToCart_Click" />
                                    
                                    <asp:ImageButton ID="btnImgAddToWishList" runat="server" 
                                        ImageUrl='<%$ Tokens:Invoke, AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToWishButton"))  %>' 
                                        AlternateText='<%$ Tokens:StringResource, AppConfig.WishButtonPrompt %>'
                                        CssClass="AddToWishButton" 
                                        Visible='<%$ Tokens:AppConfigBool, ShowWishButtons %>'
                                        onclick="btnAddToWishList_Click" />
                                    
                                    <asp:ImageButton ID="btnImgAddToGiftRegistry" runat="server" 
                                        ImageUrl='<%$ Tokens:Invoke, AppLogic.SkinImage(AppLogic.AppConfig("AddToCart.AddToGiftRegistryButton"))  %>' 
                                        AlternateText='<%$ Tokens:StringResource, AppConfig.GiftButtonPrompt %>'
                                        CssClass="AddToGiftButton"
                                        Visible='<%$ Tokens:AppConfigBool, ShowGiftRegistryButtons %>'
                                        onclick="btnAddToGiftRegistry_Click" />
                                                    
                                <% } else{ %>
                                                        
                                    <asp:Button ID="btnAddToCart" runat="server" 
                                        Text='<%$ Tokens:StringResource, AppConfig.CartButtonPrompt %>'
                                        CssClass="AddToCartButton"
                                        onclick="btnAddToCart_Click" />
                                    
                                    <asp:Button ID="btnAddToWishList" runat="server" 
                                        Text='<%$ Tokens:StringResource, AppConfig.WishButtonPrompt %>'
                                        CssClass="AddToWishButton" 
                                        Visible='<%$ Tokens:AppConfigBool, ShowWishButtons %>'
                                        onclick="btnAddToWishList_Click" />
                                    
                                    <asp:Button ID="btnAddToGiftRegistry" runat="server" 
                                        Text='<%$ Tokens:StringResource, AppConfig.GiftButtonPrompt %>'
                                        CssClass="AddToGiftButton"
                                        Visible='<%$ Tokens:AppConfigBool, ShowGiftRegistryButtons %>'
                                        onclick="btnAddToGiftRegistry_Click" />
                                                    
                                <% } %>
                                </li>
                        </ul>                    
                    </asp:Panel> 
                </ContentTemplate>
            </asp:UpdatePanel>
                            
                        
        </div>
        <script type="text/javascript" language="javascript" >

        Type.registerNamespace("aspdnsf.Pages");
        Type.registerNamespace("aspdnsf.Controls");
        
        aspdnsf.Controls.FileUploadControl = function(id, key) {
            this.id = id;
            this.key = key;
            
            this.cmdRefresh = null;
        }
        aspdnsf.Controls.FileUploadControl.prototype =  {
            
            get_id : function() {
                return this.id;
            },
            
            get_key : function() {
                return this.key;
            },
            
            set_refreshCommand : function(cmd) {
                this.cmdRefresh = cmd;
            },
            
            refresh : function() {
                this.cmdRefresh();
            }
        }
        aspdnsf.Controls.FileUploadControl.registerClass('aspdnsf.Controls.FileUploadControl');
        
        aspdnsf.Pages.$KitPage = function() {
            this.uploadGroups = new Array();
        };
        aspdnsf.Pages.$KitPage.prototype =  {
        
            add_uploadGroup : function(grp) {
                this.uploadGroups.push(grp);
            },
            
            refreshUploadGroup : function(key) {
                for(var ctr=0; ctr<this.uploadGroups.length; ctr++) {
                    var current = this.uploadGroups[ctr];
                    if(key == current.get_key()) {
                        current.refresh();
                        break;
                    }
                }
            },
            
            highlightPriceChange : function(pnlId) {
                var pricePanel = $get(pnlId);
                var priceEls = WebForm_GetElementsByTagName(pricePanel, 'span');
                for(var ctr=0; ctr< priceEls.length; ctr++) {
                    var el = priceEls[ctr];
                    this.highlightPriceElement(el);
                }
            },
            
            highlightPriceElement : function(el) {
                if (el) {
                    var elId = el.id;
                    var defaultColor = "#FFFFFF";
                    var highlightColor = "#E6ECF9";
                    var sequence = AjaxControlToolkit.Animation.createAnimation(
                    {
                        AnimationName: "Sequence",
                        AnimationTarget: elId
                    });
                    
                    sequence.play();
                }
            }
            
            
        }
        aspdnsf.Pages.KitPage = new aspdnsf.Pages.$KitPage();
        
        function floatKitSideBar() {
            var placeHolder = $get('KitPlaceHolder');
            var sideBar = $get('KitSideBar');
            
            var vOffset = 10;
            var hOffset = 10;

            var py = 0;
            var px = 0;
            var mv = 0;
            
            var pageY = 0;
            pageY = (document.body.scrollTop > 0) ? document.body.scrollTop : pageY;
            pageY = (document.documentElement.scrollTop > 0) ? document.documentElement.scrollTop : pageY;     
            
            // requires aspnet client components
            var phY = WebForm_GetElementPosition(placeHolder).y;
            
            if(pageY < phY) {
                py = phY;
            }
            else {
                py = pageY + vOffset;
                mv = ( py + sideBar.offsetTop) / 2;            
            }
            
            mv = Math.abs(py - sideBar.offsetTop) / 2;
            mv = mv > 1 ? mv : 0;
            
            sideBar.style.top = (py + mv) + px;
            
            /*********************************/
            var mm = $get('KitPlaceHolder');
            if($get('KitPlaceHolder') == null) {
                return;
            }
            
            var my = 0;
            my = (document.body.scrollTop > 0) ? document.body.scrollTop : my;
            my = (document.documentElement.scrollTop > 0) ? document.documentElement.scrollTop : my;
            var mmy = WebForm_GetElementPosition(mm).y;

            var sideBar = $get("KitSideBar");
            
            if (my < mmy) {
                my = mmy;
            }      
            else {
                my = my + 10;
                mv = (my + sideBar.offsetTop) / 2;
            }
            
            var mv = Math.abs(my - sideBar.offsetTop) / 2;
            mv = mv > 1 ? mv : 0;
            
            sideBar.style.top = (my + mv) + "px";

            var dw = 0;
            dw = (document.body.clientWidth > 0) ? document.body.clientWidth : dw;
            dw = (document.documentElement.clientWidth > 0) ? document.documentElement.clientWidth : dw;

            var hx = WebForm_GetElementPosition(mm).x;
            var sw = WebForm_GetElementPosition(sideBar).width;
            var bx =  (dw - (sw + 10));
            
            if(bx > hx) {
                sideBar.style.left = hx + "px";
            }
            else {
                sideBar.style.left = bx + "px";
            }        

            setTimeout("floatKitSideBar()", 50);
        }

        Sys.Application.add_load(function() {
            floatKitSideBar();
        });
        
        function SetUniqueRadioButton(rptName, groupName, current) {
            
            var isInSameGroup = function(elName){ return elName.startsWith(rptName) && elName.endsWith(groupName);  }
        
            for (i = 0; i < document.forms[0].elements.length; i++) {
            elm = document.forms[0].elements[i]

            if (elm.type == 'radio') {
                if (isInSameGroup(elm.name)) {
                    elm.checked = false;
                }
            }
        }
        
        current.checked = true;
    }    

        
    
    </script>
        
    </asp:Panel>
</asp:Content>


