var common = {};

/* Navbar */
common.toggleNavbar = function () {
	//Toggle the navbar
	var bar = $("#navItems");
	if (bar.hasClass("expanded")) {
		bar.removeClass("expanded").addClass("collapsed");
	}
	else {
		bar.removeClass("collapsed").addClass("expanded");
	}
}
//Hide the navbar when something else on the page is clicked on
$(document).ready(function () {
	var navThings = $("#btnToggleNavbar, #navItems");
	$("html").not(navThings).on("click", function () {
		$("#navItems").removeClass("expanded").addClass("collapsed");
	});
	navThings.on("click", function (e) {
		e.stopPropagation();
	});
	//Fix focus
	$("#btnToggleNavbar").on("mouseout", function () {
		$(this).blur();
	});
});