<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TopicMapping.ascx.cs"
    Inherits="AspDotNetStorefrontAdmin.Controls.TopicMapping" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>
<div>
    <asp:UpdatePanel runat="server" ID="updatePanelSearch" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_All" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_#" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_A" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_B" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_C" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_D" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_E" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_F" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_G" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_H" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_I" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_J" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_K" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_L" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_M" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_N" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_O" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_P" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Q" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_R" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_S" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_T" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_U" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_V" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_W" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_X" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Y" />
            <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Z" />
        </Triggers>
        <ContentTemplate>

            <script type="text/javascript">
                function mapAll(map) {
                    var chks = document.getElementsByTagName('input');
                    for (var ctr = 0; ctr < chks.length; ctr++) {
                        var chk = chks[ctr];
                        if (chk.className = 'map_check') {
                            chk.checked = map;
                        }
                    }
                }
            </script>

            <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch" OnFilter="ctrlSearch_Filter"
                OnContentCreated="ctrlSearch_ContentCreated" AlphaGrouping="1">
                <Search SearchButtonCaption="Go" SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>"
                    SearchTextMinLength="<%$ Tokens:AppConfigUSInt, MinSearchStringLength %>" SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>"
                    ValidateInputLength="false" ShowValidationMessageBox="false" ShowValidationSummary="false"
                    UseLandingPageBehavior="false" WillRenderInUpdatePanel="true" />
                <ContentTemplate>
                <div id="pnlLoadingGeneral" runat="server" style="float:left;margin-left:400px;position:absolute;"> 
                    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                            <ProgressTemplate>                    
                                <img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-2.gif" />                    
                            </ProgressTemplate>
                        </asp:UpdateProgress>
                </div>
                        <telerik:RadGrid ID="grdTopics" runat="server" GridLines="None" AllowSorting="false" OnSortCommand="grdTopics_SortCommand" OnItemCommand="grdTopics_ItemCommand" Width="500px">
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
                            <AlternatingItemStyle CssClass="config_alternating_item" />
                            <Columns>
                                <telerik:GridTemplateColumn UniqueName="columnEdit" ItemStyle-Width="35px" HeaderStyle-HorizontalAlign="Center"
                                    HeaderStyle-VerticalAlign="Middle" HeaderStyle-Height="25px">
                                    <HeaderTemplate>
                                        <div class="map_header">
                                            Map: <a href="javascript:void(0);" onclick="javascript:mapAll(true);">All,</a>&nbsp;<a
                                                href="javascript:void(0);" onclick="javascript:mapAll(false);">None</a> |&nbsp;<asp:LinkButton
                                                    ID="lnkSave" runat="server" CommandName="SaveMapping">
                                            Save
                                                </asp:LinkButton>
                                        </div>
                                        <ajax:AnimationExtender ID="extSaveTopicMapping" runat="server" TargetControlID="lnkSave" Enabled="true" >
                                             <Animations >                                                     
                                                 <OnClick>
                                                    <Sequence>
                                                        <%--Hide the main progress template since we have our own specialized--%>
                                                        <HideAction AnimationTarget="pnlLoadingGeneral" />
                                                        <StyleAction AnimationTarget="divProgressAdd" Attribute="display" Value=""/>
                                                    </Sequence>
                                                </OnClick>
                                             </Animations>
                                        </ajax:AnimationExtender>
                                    </HeaderTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                    <ItemTemplate>
                                        <%--hidden field for reference later during update--%>
                                        <asp:HiddenField ID="hdfTopicID" runat="server" Value='<%# DataItemAs<Topic>(Container).TopicID %>' />
                                        <asp:CheckBox ID="chkIsMapped" runat="server" CssClass="map_check" Checked='<%# DataItemAs<Topic>(Container).HasBeenMapped(int.Parse(ThisTopicID)) %>' />
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="ID" UniqueName="TemplateColumn1" ItemStyle-Width="20px"
                                    SortExpression="TopicID">
                                    <ItemTemplate>
                                        <asp:Label ID="lblID" runat="server" Text='<%# DataItemAs<Topic>(Container).TopicID %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Name" UniqueName="TemplateColumn1" ItemStyle-Width="60px"
                                    SortExpression="TopicName">
                                    <ItemTemplate>
                                        <asp:Label ID="lblName" runat="server" Text='<%# LocalizeName(DataItemAs<Topic>(Container).TopicName) %>'></asp:Label>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                        <FilterMenu>
                            <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                        </FilterMenu>
                    </telerik:RadGrid>
                    <div id="divProgressAdd" runat="server" style="display:none" >
                        <p style="color:#3DDB0B;font-style:italic;">Saving in progress...<img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/ajax-loader-1.gif" /></p> 
                    </div>
                </ContentTemplate>
            </aspdnsf:SearcheableTemplate>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
