(function (factory) {
    if (typeof module === "object" && typeof module.exports === "object") {
        var v = factory(require, exports);
        if (v !== undefined) module.exports = v;
    }
    else if (typeof define === "function" && define.amd) {
        define(["require", "exports", "@microsoft/signalr", "jquery"], factory);
    }
})(function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var signalR = require("@microsoft/signalr");
    var $ = require("jquery");
    var boxMessages = document.querySelector("#chat-message");
    var inputMessage = document.querySelector("#messageInput");
    var user = document.querySelector("#chat-receiver");
    var btnSend = document.querySelector("#sendButton");
    $('#chatUsers li').on("click", function () {
        var username = $(this).attr("data-contact-name");
        $("#receivername").html(username);
        $("#chat-receiver").val(username);
    });
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();
    connection.on("ReceiveMessage", function (response) {
        var m = document.createElement("div");
        m.innerHTML =
            '<div class="media w-50 ml-auto mb-3">' +
                '<img src="https://img.icons8.com/dusk/64/000000/user-female-skin-type-1-2.png" alt="user" width="50" class="rounded-circle">' +
                '<div class="media-body">' +
                '<div class="bg-primary rounded py-2 px-3 mb-2">' +
                '<p class="text-small mb-0 text-white">' + response.messageBody + '</p>' +
                '</div>' +
                '<p class="small text-muted">' + response.senderId + '</p>' +
                '<p class="small text-muted">' + response.messageDate + '</p>' +
                '</div>' +
                '</div>';
        boxMessages.appendChild(m);
        boxMessages.scrollTop = boxMessages.scrollHeight;
    });
    connection.start().catch(function (err) { return document.write(err); });
    inputMessage.addEventListener("keyup", function (e) {
        if (e.key === "Enter") {
            sendPrivateMessage();
        }
    });
    btnSend.addEventListener("click", sendPrivateMessage);
    function sendPrivateMessage() {
        connection.send("SendPrivateMessage", user, inputMessage.value)
            .then(function () { return inputMessage.value = ""; });
    }
});
//# sourceMappingURL=chat.js.map