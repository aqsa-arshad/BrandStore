$(document).ready(function () {

	// Set up UI elements
	$(".button").button();
	$(".accordion").accordion();

	// Setup events
	$('.expandAllExpandables').click(function () {
		ToggleAllExpandables(true);
	});

	$('.collapseAllExpandables').click(function () {
		ToggleAllExpandables(false);
	});

	$(".checkHeader").click(function () {
		// Show the corresponding section when a checkbox or radio button is clicked
		$(this).has(":checkbox:checked, :radio:checked").next().show();

		// Hide unselected sections
		$(this).closest('.checkTarget').find(".checkHeader").has(".checkToggler :checkbox:not(:checked), .checkToggler :radio:not(:checked)").next().hide();
	});

	SetupExpandableToggles();
	InitializeExpandableState();
});

function ToggleAllExpandables(visible) {
	$(".expandable").next().toggle(visible);
	$(".expandable").each(function () {
		SwitchExpandableIcon(this, visible);
	});
}

function SwitchExpandableIcon(expandableElement, expanded) {
	var buttons = $(expandableElement).find(".button").andSelf();
	var buttonIcons = buttons.find(".ui-icon");

	if (expanded) {
		buttonIcons.addClass("ui-icon-squaresmall-minus").removeClass("ui-icon-squaresmall-plus");
	}
	else {
		buttonIcons.removeClass("ui-icon-squaresmall-minus").addClass("ui-icon-squaresmall-plus");
	}
}

function SetupExpandableToggles() {
	var expanderHtml = "<span class=\"ui-state-default ui-corner-all ui-state-hover icon button\" title=\"Expand\"><span class=\"ui-icon ui-icon-squaresmall-plus\"></span></span>";

	// Add an expand/collapse button next to each expandable
	$(".expandable").prepend(expanderHtml);

	// Add the expand/collapse functionality to the button
	$(".expandable .button").click(function () {
		if ($(this).parent().next().is(":visible")) {
			$(this).parent().next().hide();
			SwitchExpandableIcon(this, false);
		}
		else {
			$(this).parent().next().show();
			SwitchExpandableIcon(this, true);
		}
	});
}

function InitializeExpandableState() {
	$(".checkHeader").each(function () {
		var hasChecked = $(this).find(":checkbox, :radio").is(":checked");
		var initiallyExpanded = $(this).is(".initiallyExpanded");
		var targetHasChecked = $(this).next('.checkTarget').find(".checkToggler :checkbox, .checkToggler :radio").is(":checked");

		var show = hasChecked || initiallyExpanded || targetHasChecked;

		$(this).next().toggle(show);
		SwitchExpandableIcon(this, show);
	});
}