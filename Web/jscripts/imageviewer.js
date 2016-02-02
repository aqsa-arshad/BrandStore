//create and image object to store each image's attributes
function imageInfo(SRC, WIDTH, HEIGHT) {
	this.src = SRC;
	this.width = WIDTH;
	this.height = HEIGHT;
}

function setColor(newColor) {
    debugger;
	//don't set the color if the color has no image. This can be done from the color dropdown.
	if(imageSizes['Icon'][newColor][view].src.match("nopicture")) {
		return;
	}

	//set the global color
	color = newColor;

	//update product image
	updateMainProductImage();

	//rebuild the zoom
	reinitializeZoom();

	//update the view colors if they're there
	if(hasMultiViewImages) {
		$('#viewControls').find('img').each(function(index) {
			this.src = imageSizes['Icon'][color][index + 1].src;
		});
	}

	//get the add to cart color dropDown
	var dropOptions = $('select[id^="Color"]').find('option').each(function (index) {
		//get the color from the option's value. It's formatted Color,SkuModifier
		var optionColor = this.value.split(',', 1);
		if(optionColor == color) {
		    this.selected = true;
		    $('#Color_1_1').trigger("change");
		}
	});
	//$('#Color_1_1').trigger("change");
	//$('#Color_1_1').change();
}

function setView(newView) {

	//set the global view variable
	view = newView;

	//get and set the main image
	updateMainProductImage();

	//rebuild the zoom
	reinitializeZoom();

	//set the view of the colors if they are there
	if(hasMultiColorImages) {
		var colorImages = $('#colorControls').find('img').each(function () {
			var iconColor = this.alt;
			this.src = imageSizes['Icon'][iconColor][view].src;
		});
	}
}

function setVariant(newVariantId) {

	//set the global seelctedVariantId variable
	selectedVariantId = newVariantId;

	//get and set the main image
	updateMainVariantImage();

	//rebuild the zoom
	reinitializeZoom();

	//get and set the variant dropDown
	var dropOptions = $('#variantSelector').val(selectedVariantId);
}

function popUpLarge() {
	if((!imageSizes['Large'][color][view].src.match("nopicture")) && (imageSizes['Large'][color][view].src)) {
		window.open('popup.aspx?src=' + imageSizes['Large'][color][view].src, 'LargerImage', 'toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=no,resizable=no,copyhistory=no,width=' + imageSizes['Large'][color][view].width + ',height=' + imageSizes['Large'][color][view].height + ',left=0,top=0');
	}
	else if(defaultLargeImage.src != '') {
		window.open('popup.aspx?src=' + defaultLargeImage.src, 'LargerImage', 'toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=no,resizable=no,copyhistory=no,width=' + defaultLargeImage.width + ',height=' + defaultLargeImage.height + ',left=0,top=0');
	}
	else {
		alert('Image Unavailable');
	}
}

function reinitializeZoom() {
	if($.zoom) {
		$('.j-zoom').trigger('zoom.destroy');
		$('.j-zoom').zoom();
	}
}

function updateMainProductImage() {
	if(hasLargeImage) {
		$('#productImage').attr('src', imageSizes['Large'][color][view].src);
	}
	else {
		$('#productImage').attr('src', imageSizes['Medium'][color][view].src);
	}
}

function updateMainVariantImage() {
	if(hasLargeImage) {
		$('#productImage').attr('src', variantImages[selectedVariantId]['large'].src);
	}
	else {
		$('#productImage').attr('src', variantImages[selectedVariantId]['medium'].src);
	}
}

function parseColorFromSelectValue(colorSelectValue) {
	if(colorSelectValue.indexOf(',') != -1) {
		return colorSelectValue.split(',', 1);
	}
	else {
		return colorSelectValue;
	}
}
