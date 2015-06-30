<%@ Page Language="C#" AutoEventWireup="true" CodeFile="LayoutHost.aspx.cs" Inherits="AspDotNetStorefront.LayoutHost" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="iPanel" src="controls/ImagePanel.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <style type="text/css">
        .EditButton
        {
        	border-style:outset;
        	background-color:LightGrey;
        	z-index:1;
        	position:absolute;
    	    text-align:center;
    	    width:60px;
    	    font-size:25px;
    	    opacity:0.4;
    	    filter:alpha(opacity=40);
        }
        
                
        .modal_popup
         {
         	background-color:White;
         	border:solid 1px #52355;
         }
         
        .modal_popup_background 
        {
        	opacity:0.7;
            background-color:Gray;
            filter: alpha(opacity = 70);
        }
        
        .modal_popup_Header
        {
        	background:url(images/variantListHeader_background.jpg) repeat;
	        height: 30px;
	        padding-right: 10px;
	        padding-left: 10px;
	        color:White;
	        text-align:center;
        	vertical-align:middle;
        	padding-top:10px;
        	font-weight:bold;
        	font-size:12px;
        }
        
        .modal_popup_Content
        {
        	padding-top:10px;
        	padding-left:10px;
        	padding-right:10px;
        	padding-bottom:10px;
        }
        
        .modal_popup_Content table
        {
        	border-style:none;
        }
        
        .modal_popup_Content table td
        {
        	border-style:none;
        }
        
        .modal_popup_Footer
        {
        	border-top:solid 1px #ccc;
        	margin-left:25px;
        	margin-right:25px;
        	padding-top:5px;
        	padding-bottom:10px;
        }
        
        .modal_popup_Header_Close        
        {
        	float:right;
        	padding-right:7px;
        }
        
        .config_alternating_item
        {
        	background-color:#eee;
        }
        
        .alpha_filters_column
        {
        	background-color:#2F577A;
        	width:30px;
        	text-align:center;
        	vertical-align:top;
        	padding-top:10px;
        	padding-left:5px;
        	padding-right:5px;
        	padding-bottom:10px;
        }
        
        .alpha_filters
        {
        	min-height:500px;
            height:auto !important; 
            min-height:500px;
        }
        
        .alpha_filters_column a,
        .alpha_filters_column span
        {
        	color:#fff;
        }
        
        .pnlMain
        {
        	padding-left:20px;
        	padding-top:10px;
        	padding-right:20px;
        	padding-bottom:30px;
        }
        
        .config_error        
        {
        	color:red;
        }
        
        a:link{text-decoration:none;}
        a:visited{text-decoration:none;}
        a:hover{text-decoration:underline;}
       
    </style>

    <asp:UpdatePanel ID="upImage" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlLayout" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
