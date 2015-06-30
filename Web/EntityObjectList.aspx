<%@ Page Language="C#" AutoEventWireup="true" CodeFile="EntityObjectList.aspx.cs" Inherits="AspDotNetStorefront.EntityObjectListPage" StylesheetTheme="" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityObjectList" src="controls/EntityObjectList.ascx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>    
    
    
    <style type="text/css" >
        
        body
        {
             font-family: Tahoma, Verdana, sans-serif;
        	 font-size: 12px !important;
        	 background-color:#fff;
        }
        
        table
        {
        	font-family: Tahoma, Verdana, sans-serif;
        	font-size: 12px !important;
        }
        
        a:active, a:link, a:visited
        {
            color: #112837;
            text-decoration: underline;
        }
        a:hover
        {
            color: #4776BD;
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
        	background:url(images/kit/variantListHeader_background.jpg) repeat;
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
        	font-family:Consolas;
        	background-color:#2F577A;
        	width:30px;
        	text-align:center;
        	vertical-align:top;
        	padding-top:10px;
        	padding-left:5px;
        	padding-right:5px;
        	padding-bottom:10px;
        }
        
        /*
        .alpha_filters
        {
        	min-height:220px;
            height:auto !important; 
            min-height:220px;
        }
        */
        
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
        
        .map_header a:link
        {
        	color:#068FE0;
        	text-decoration:none;
        }
        
        .content_area
        {
             padding-top:5px;
             padding-left:5px;
             background-color:#fff;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">    
    
        <asp:ScriptManager ID="scrptMgr" runat="server">
        </asp:ScriptManager>
    
        <div id="pnlMain" runat="server" class="content_area">
            <aspdnsf:EntityObjectList id="ctrlObjectList" runat="server" />
        </div>
       
    </form>
</body>
</html>
