window.forceResize = {};
$(document).ready(function () {
    resizeParent();
    //Handle window resizing
    $(window).on("resize", function () {
        console.log("window resized");
        resizeParent();
    });
    //Handle image loading
    $(document).on("load", "img", function () {
        console.log("image loaded");
        resizeParent();
    });
    //Brute force page load resizing
    window.forceResize = setInterval(resizeParent, 1000);
    setTimeout(function () { clearInterval(window.forceResize); }, 5500);
});

function resizeParent() {
    var height = $("body").parent().outerHeight() + 20;
    var message = { action: "resize", width: "100% !important", height: height + "px !important" };
    window.parent.postMessage(JSON.stringify(message), "*");
}
