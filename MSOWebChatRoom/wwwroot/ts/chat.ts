// The following sample code uses TypeScript and must be compiled to JavaScript
// before a browser can execute it.

import * as signalR from "@microsoft/signalr";
import * as $ from "jquery";

// The Messages container div
const boxMessages: HTMLDivElement = document.querySelector("#chat-message");
// The message to send
const inputMessage: HTMLInputElement = document.querySelector("#messageInput");
// the receiver user
const user: HTMLInputElement = document.querySelector("#chat-receiver");
// Send button for sending the message
const btnSend: HTMLButtonElement = document.querySelector("#sendButton");



$('#chatUsers li').on("click", function () {
    var username = $(this).attr("data-contact-name");
    $("#receivername").html(username);
    $("#chat-receiver").val(username);

    // get messages conversation

});


const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("ReceiveMessage", (response) => {
    let m = document.createElement("div");

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

connection.start().catch(err => document.write(err));

inputMessage.addEventListener("keyup", (e: KeyboardEvent) => {
    if (e.key === "Enter") {
        sendPrivateMessage();
    }
});

btnSend.addEventListener("click", sendPrivateMessage);

function sendPrivateMessage() {
    connection.send("SendPrivateMessage", user, inputMessage.value)
        .then(() => inputMessage.value = "");
}