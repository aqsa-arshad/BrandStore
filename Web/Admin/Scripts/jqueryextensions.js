function ToggleAppConfigStores(storeConfigTableElement) {
    var visible = $(storeConfigTableElement).find('.storeToggleRow:visible').length > 0;
    $(storeConfigTableElement).find('.storeToggleRow').toggle(!visible);
    $(storeConfigTableElement).find('.atomEditAppConfig').toggleClass('active');
}


function popThis(el) {
	var width = $(el).data('width');
	var height = $(el).data('height');
	var url = $(el).attr('href');
	window.open(url, '_blank', 'width=' + width + ',height=' + height, false);
	return false;
}
