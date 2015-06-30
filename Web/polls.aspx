<%@ Page language="c#" AutoEventWireup="true" CodeFile="polls.aspx.cs" Inherits="AspDotNetStorefront.polls" MasterPageFile="~/App_Templates/Skin_1/template.master"%>
<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Literal ID="litContent" runat="server" Text="" />
    
        <asp:Panel ID="pnlContent" runat="server">
        <div style="height:10px">
          <b> <asp:Label ID="lblPoll" runat="server" Text="Label"></asp:Label></b> 
        </div>
        
        <div style="width : 100%">
        <asp:DataList ID="dtlPolls" runat="server"
                                    RepeatColumns="2" 
                                    RepeatDirection="Horizontal"
                                    CellSpacing="7" 
                                    ItemStyle-Width="340"
                                    OnItemDataBound="dtlPolls_ItemDataBound" >            
            <ItemTemplate>
               <asp:Panel ID="pnlPoll" runat="server" Width="340px">
                    <aspdnsfc:PollControl ID="ctrlPoll" runat="server" 
                        HeaderBGColor="<%$ Tokens:AppConfig2, SiteDisplay.HeaderBGColor %>" PollButtonText="Vote" 
                        PollClass="<%$ Tokens:AppConfig2, SiteDisplay.BoxFrameStyle %>"
                        HeaderImage="~/images/poll.gif"
                        OnPollVoteButtonClick="ctrlPoll_PollVoteButtonClick" />
               </asp:Panel>
            </ItemTemplate>
        </asp:DataList>
        </div>
        
    </asp:Panel>
</asp:Content>  
