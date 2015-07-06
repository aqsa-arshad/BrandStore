<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ModalConfigurationAtom.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.ModalConfigurationAtom" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" TagName="ConfigurationAtom" Src="ConfigurationAtom.ascx" %>

        <asp:UpdatePanel ID="upExtender" runat="server" UpdateMode="Always">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSave" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnCancel" EventName="Click" />
            </Triggers>
            <ContentTemplate>
                <asp:LinkButton ID="btnConfigureAtomConfig" runat="server" Text="configure" />
                <ajax:ModalPopupExtender 
                    ID="mpConfigurationAtom" runat="server" 
                    PopupControlID="pnlConfigurationAtom"
                    TargetControlID="btnConfigureAtomConfig" 
                    BackgroundCssClass="modal_popup_background" 
                    CancelControlID="btnCancelConfiguration"
                    >
                </ajax:ModalPopupExtender>
            </ContentTemplate>
        </asp:UpdatePanel>

        <div style="display: none;">
            
            <asp:Panel ID="pnlConfigurationAtom" runat="server" CssClass="atom_modal_popup" Width="825px" ScrollBars="None" DefaultButton="defaultButton">
                <asp:UpdatePanel ID="upModalAtom" UpdateMode="Always" ChildrenAsTriggers="true" runat="server">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnSave" EventName="Click" />
                    </Triggers>
                    <ContentTemplate>
                        <div class="modalHolder">
                            <div class="atomFixedHeader" id="modaldiv" runat="server">
								<div class="atomHeader">
									<asp:LinkButton ID="btnCancelConfiguration" CssClass="atomModalClose" OnClick="btnCancelConfiguration_Click" runat="server" />
									<asp:Literal ID="litTitle" runat="server" />
								</div>
                            </div>
                            <asp:Panel ID="pnlConfigAtomContainer" runat="server" CssClass="pnlOverAtomFixedButtons">
                                <div class="atomModalContent">
                                    <aspdnsf:ConfigurationAtom runat="server" ID="ConfigurationAtom" ShowSaveButton="false" LoadAdvancedConfigs="true" />
                                </div>
                            </asp:Panel>
                            <div class="atomFixedButtons">
                                <asp:Button ID="btnToggleAdvanced" runat="server" OnClientClick="$(this).closest('.modalHolder').find('.trConfigAtomAdvanced').toggle();return false;" Text="Toggle Advanced" CssClass="defaultButton atomAdvanced" />
                                <asp:Button ID="btnSave" Text="Save and Close" runat="server" CssClass="emphasisButton" OnClick="btnSave_Click" />
                                <asp:Button ID="btnCancel" Text="Cancel" CssClass="defaultButton" runat="server" />
                                <div style="clear:both;"></div>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:Button runat="server" ID="defaultButton" OnClientClick="return false;" style="display:none;" />
            </asp:Panel>
        </div>


        
