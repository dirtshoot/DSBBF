(function ($) {
    $("fieldset legend").expandoControl(function (controller) { return controller.nextAll("ul"); }, { collapse: true, remember: false });
})(jQuery);