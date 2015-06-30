adnsf$(function () {
	adnsf$('.tabbitTabs').each(function (tabIndex, tabElement) {
		adnsf$(tabElement).find('a').each(function (aIndex, aElement) {
			adnsf$(aElement).click(function (event) {
				adnsf$('.tabbitTabs a').removeClass('state-active').addClass('state-default');
				adnsf$('.tabbitTabWrap').children().hide();
				adnsf$(this.hash).show()
				adnsf$(this).addClass('state-active').removeClass('state-default');
				event.preventDefault();
			});
			if (aIndex === 0) {
				adnsf$(aElement.hash).show().addClass('state-active');
				adnsf$(aElement).addClass('state-active');
			}
			else {
				adnsf$(aElement.hash).hide().addClass('state-default');
				adnsf$(aElement).addClass('state-default');
			}
		});
	});
});
