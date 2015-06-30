<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CSVHelper.ascx.cs" Inherits="AspDotNetStorefrontControls.CSVHelper" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="StoreSelector.ascx" %>
<div class="CSVHelper">
    <input type="textbox" id="<%= UniqueJSID %>SearchBox" />
    <input type="button" id="<%= UniqueJSID %>SearchButton" onclick="Reload<%= UniqueJSID %>();" value="<%= CSVSearchButtonText %>" />
    <div id="<%= UniqueJSID %>AjaxTarget"></div>
</div>

<script type="text/javascript">
    var <%= UniqueJSID %>targetTextBox = document.getElementById("<%= CSVTextBox.ClientID.ToString() %>");
    var <%= UniqueJSID %>targetDiv = document.getElementById("<%= UniqueJSID %>AjaxTarget");
    
    jQuery("#<%= UniqueJSID %>SearchBox").keypress (function(event){
      if(event.keyCode == 13){
        $("#<%= UniqueJSID %>SearchButton").click();
        event.preventDefault();
        return false;
      }
    });
    
    function Reload<%= UniqueJSID %>()
    {
        var searchurl = 'x-ajax.csvhelper.aspx?&<%= EntityType.ToString() %>=entity&searchterm='+jQuery('#<%= UniqueJSID %>SearchBox').val()+'&csvlist=,' + jQuery(<%= UniqueJSID %>targetTextBox).val() + ',';
        jQuery.ajax({
          url: searchurl,
          success: function(data) {
            jQuery(<%= UniqueJSID %>targetDiv).html(data);
            jQuery('input:checkbox', jQuery(<%= UniqueJSID %>targetDiv)).each(function(index){
                this.onclick = function(){
                    var builtRelated = getCheckedCSVList(jQuery('input:checkbox', jQuery(<%= UniqueJSID %>targetDiv)));
                    jQuery(<%= UniqueJSID %>targetTextBox).val(builtRelated);
                    Reload<%= UniqueJSID %>();
                }
            });
          }
        });
    }
    
    function getCheckedCSVList(inputs)
    {
        var csv = "";
        jQuery(inputs).each(function(index){
            if(this.checked)
            {
                csv += "," + this.value;
            }
        });
        return csv.substr(1);
    }
    
    Reload<%= UniqueJSID %>();
    
</script>
