$(document).ready(function () {

/** Emojis collection **/
    const Ecollection = ["&#128512;", "&#128513;", "&#128514;", "&#128515;", "&#128516;", "&#128517;", "&#128518;", "&#128519;", "&#128520;", "&#128521;", "&#128522;", "&#128523;", "&#128524;", "&#128525;", "&#128526;", "&#128527;", "&#128528;", "&#128529;", "&#128530;", "&#128531;", "&#128532;", "&#128533;", "&#128534;", "&#128535;", "&#128536;", "&#128537;", "&#128538;", "&#128539;", "&#128540;", "&#128541;", "&#128542;", "&#128543;", "&#128544;", "&#128545;", "&#128546;", "&#128547;"];

    $(function () {
        for (var n = 0; n < Ecollection.length; n++){
            $(".emojisContainer .emojisContainerContent").append("<div>" + Ecollection[n] + "<\/div>");
        }
    })

    /** Display Emojis collection **/
    $("#emojisTrigger").on("click", function () {
        $(".emojisContainer").fadeToggle()
    });

    /** Close Emojis container **/
    $("#btnCloseEmojis").on("click", function () {
        $(".emojisContainer").fadeToggle()
    });

    /** Add emoji selected to textarea **/
    $(document).on("click", "#chat-send .emojisContainerContent div", function () {
        var curPos =
            document.getElementById("messageInput").selectionStart;
        let textInput = $("#messageInput").val();
        $("#messageInput").val(textInput.slice(0, curPos) + $(this).html() + textInput.slice(curPos));
        //$("#emojisTrigger").click();
    })
});