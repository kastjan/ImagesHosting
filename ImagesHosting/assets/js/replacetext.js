function replaceText() {
    var oldText, newText;
    $(".editable").hover(
        function () {
            $(this).addClass("editHover");
        },
        function () {
            $(this).removeClass("editHover");
        }
    );

    $(".editable").on("dblclick", replaceHTML);


    $(".editable").on("click", ".btnSave",
                    function () {
                        newText = $(this).siblings("form")
                                         .children(".editBox")
                                         .val().replace(/"/g, "&quot;");
                        $.post("Upload/GetSetComments", { Id: $(".editable").data("value"), Text: newText }, function (data) {
                        });

                        $(this).parent()
                               .html(newText)
                               .removeClass("noPad")
                               .on("dblclick", replaceHTML);
                    }
                    );

    $(".editable").on("click", ".btnDiscard",
                    function () {
                        $(this).parent()
                               .html(oldText)
                               .removeClass("noPad")
                               .on("dblclick", replaceHTML);
                    }
                    );

    function replaceHTML() {
        oldText = $(this).html()
                         .replace(/"/g, "&quot;");
        $(this).addClass("noPad")
               .html("")
               .html("<form><input type=\"text\" class=\"editBox\" value=\"" + oldText + "\" /> </form><a href=\"#\" class=\"btnSave\">Save changes</a> <a href=\"#\" class=\"btnDiscard\">Discard changes</a>")
               .off('dblclick', replaceHTML);

    }
};