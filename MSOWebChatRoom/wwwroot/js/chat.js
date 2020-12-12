$(document).ready(function () {

    /**** VARIABLES *****/
    var connection = null;
    const currentUser = $("#currentUserName").val();
    const conversationBody = $("#chat-message");
    var fullDate = new Date();

    /// define template msg
    let MessageTemplate =
        `<div> <div class="media w-50 mb-3" id="message_{{id}}">
            <div class="media-body">
                    <div class="media-info">
                        <p class="small text-muted">{{sender}}</p>
                        <span class="checkmark">
                            <div class="checkmark_stem"></div>
                            <div class="checkmark_kick"></div>
                        </span>
                    </div>
                    <div class="bg-primary rounded py-2 px-3 mb-2">
                        <p class="text-small mb-0 text-white">{{body}}</p>
                    </div>
                    <p class="small text-muted">{{date}}</p>
            </div>
        </div></div>`;

    /// define welcom template msg
    let WelcomTmpl =
        `<div> <div class="media w-50 mb-3" id="message_0">
            <img src="./images/welcom-avatar.png" alt="user" width="50" class="rounded-circle">
            <div class="media-body">
                <div class="bg-light rounded py-2 px-3 mb-2">
                    <p class="text-small mb-0 text-muted">{{body}}</p>
                </div>
                <p class="small text-muted">{{date}}</p>
             </div>
        </div></div>`;

    /**** END VARIABLES *****/

    /**** fUNCTIONS *****/
    /**
     * get chat data
     * @param {any} contact
     * @param {any} url
     */    
    function getChat(contact, url) {
        $.ajax({
            data: { contact: contact },
            url: url,
            type: "GET",
            contentType: "application/json",
            dataType: 'json',
            success: function (response) {
                if (response == "failed") {
                    alert(response.responseText);
                }
                else {
                    var chat_data = response;
                    loadChat(chat_data);
                }

            },
            failure: function (response) {
                alert(response.responseText);
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    }

    /**
     * load chat data into view
     * @param {any} chat_data
     */
    function loadChat(chat_data) {
        /// new conversation, display welcom msg
        if (chat_data.length ==0) {
            var contact = $("#chat-receiver").val();
            let tmpl = $(WelcomTmpl).html();
            tmpl = tmpl.replace("{{body}}", "Welcome to the Chat Room with " + contact);
            tmpl = tmpl.replace("{{date}}", fullDate.toUTCString());

            tmpl = $(tmpl);
            conversationBody.append(tmpl);
        }
        /// display history conversation
        else {
            chat_data.forEach(function (data) {
                displayMessage(data, "loadmessage");
            });
        }
    }

    /**
     * display conversation in the view
     * @param {any} message_obj
     */
    function displayMessage(message_obj,source) {
        const msg_id = message_obj.messageId;
        const msg_body = message_obj.messageBody;
        const msg_sender = message_obj.senderName;
        const msg_date = message_obj.messageDate;

        let template = $(MessageTemplate).html();
        template = template.replace("{{id}}", msg_id);
        template = template.replace("{{body}}", msg_body);
        template = template.replace("{{sender}}", msg_sender);
        template = template.replace("{{date}}", msg_date);

        template = $(template);

        if (msg_sender == currentUser) {
            template.addClass('ml-auto');

            if (message_obj.status == "read" || message_obj.status == "1") {
                template.find('.checkmark').addClass('read');
            }
            else if (message_obj.status == "Unread" || message_obj.status == "0") {
                template.find('.checkmark').addClass('unread');
            }
        }
        else {
            if (source == "loadmessage") {
                template.removeClass('ml-auto');
                template.find('.checkmark').addClass('hidden');
            }
        }

        conversationBody.append(template);

        conversationBody.stop().animate({
            scrollTop: $('#chat-message')[0].scrollHeight
        });
    }

    /**
     * Remove the previous selected user/group
     * @param {any} Elem
     */
    function removeActiveClass(Elem) {
        $(Elem).each(function () {
            $(this).removeClass("active");
        });
    }

    /** Resize Overlay of Create Group**/
    var sizeTheOverlays = function () {
        $(".overlay").resize().each(function () {
            var h = $(this).parent().outerHeight();
            var w = $(this).parent().outerWidth();
            $(this).css("height", h);
            $(this).css("width", w + 50);
        });
    };

    /**** END fUNCTIONS *****/

    // choose specific user to chat with
    $(document).on("click","#chatUsers li.user__item", function (e) {
        e.preventDefault();
        $("#chat-message-welcom").addClass("hidden");
        conversationBody.removeClass("hidden");

        removeActiveClass($("#chatUsers li.active"));
        removeActiveClass($("#chatGroups li.active"));

        $(this).addClass("active");

        var username = $(this).attr("data-contact-name");
        $("#receivername").html(username);
        $("#chat-receiver").val(username);
        $("#chat-receiver-type").val("user");
        $(".info").addClass("hidden");

        $(".bg-light-left img").attr("src", "./images/default-avatar.png");

        conversationBody.attr("data-status", "open");
        $("#chat-contact").removeClass("hidden");
        $("#chat-send").removeClass("hidden");

        conversationBody.html("");

        // get messages conversation
        getChat(username, "/Chat/ConversationWithContact/");
    });

    
    sizeTheOverlays();

    // Open overlay to add new chat group
    $("#btnAddGroup").on("click", function () {
        $("#groupoverlay").css("display", "flex");
    });

    // close group's Overlay
    $("#btnCancel").on("click", function () {
        $("#groupNameInput").val("");
        $(".form-check-users-container .form-check").each(function () {
            var t = $(this)
                , i = t.find("input:checkbox");
            if (i.is(":checked")) 
                i.prop('checked', false);
        });
        $("#groupoverlay").css("display", "none");
        $(".userError").addClass("hidden");
        $("#groupNameInput").removeClass("inputError");
    });

    // close chat conversation when finish chatting
    $('#btnCloseChat').on("click", function (e) {
        e.preventDefault();
        removeActiveClass($("#chatUsers li.active"));
        removeActiveClass($("#chatGroups li.active"));

        $("#receivername").html("");
        $("#chat-receiver").val("");
        $("#chat-receiver-type").val("");
        $(".bg-light-left img").attr("src", "./images/default-avatar.png");
        $("#messageInput").val("");
        $("#btnCloseEmojis").click();

        conversationBody.attr("data-status", "closed");
        $("#chat-contact").addClass("hidden");
        $("#chat-send").addClass("hidden");

        $("#chat-message-welcom").removeClass("hidden");
        conversationBody.html("").addClass("hidden");
    });

    // display group's informations
    $(".info").on("click", function () {
        var group_id = $("#receivername").html();

        if ($(".info-users").hasClass("hidden")) {
            $.ajax({
                data: { group: group_id },
                url: "/Chat/GroupDetailsUsers",
                type: "GET",
                contentType: "application/json",
                dataType: 'json',
                success: function (response) {
                    if (response == "failed") {
                        alert(response.responseText);
                    }
                    else {
                        var chat_data = response;
                        let ulist = "";
                        chat_data.forEach(function (data) {
                            ulist += '<li>' + data + '</li>';
                        });

                        $(".info-users ul").html("").append(ulist);
                        $(".info-users").removeClass("hidden");
                    }
                },
                failure: function (response) {
                    alert(response.responseText);
                },
                error: function (response) {
                    alert(response.responseText);
                }
            });
        }
        else {
            $(".info-users").addClass("hidden");
        }
    });

    // Get value of user checkbox to add to new group
    $(".form-check").click(function () {
        $(".userError").addClass("hidden");
        var t = $(this)
            , i = t.find("input:checkbox");
        var id = i.attr("id");
        var index = id.replace("use_", "");
        var value = $("#"+id).val()

        if (i.is(":checked")) {
            $("#Ugrp_" + index).val(value);
            $("#Sugrp_" + index).val(true);
        } else {
            $("#Sugrp_" + index).val(false);
        }
    });

    $("#groupNameInput").on("input", function () {
        if ($(this).val().trim())
            $(this).removeClass("inputError");
    });

    // submit creation of new group
    $("#btnCreate").on("click", function (e) {
        var groupName = $("#groupNameInput").val();
        var sum = 0;
        $(".form-check-users-container .form-check").each(function () {
            var t = $(this)
                , i = t.find("input:checkbox");
            if (i.is(":checked"))
                sum++;
        });

        if (!groupName || sum == 0 || sum==1) {
            if (!groupName)
                $("#groupNameInput").addClass("inputError");
            if (sum == 0) {
                $(".userError span").html("You must choose Users to add");
                $(".userError").removeClass("hidden");
            }
            if (sum == 1) {
                $(".userError span").html("You must add at least 2 Users");
                $(".userError").removeClass("hidden");
            }
            event.preventDefault()
        }
        else 
            $(this).submit();
        
    });

    // display notification
    $(document).on("click", "#notification .fa-bell",".notificon", function () {
        $(".notificon").addClass("hidden");
        const Elem = $(".notifmessage");
        if ($("#notification .fa-bell").attr("data-count") == 1) {
            $("#notification .fa-bell").toggleClass("active");
            if ($(Elem).hasClass("hidden")) {
                $(".notifmessage").removeClass("hidden");
            } else {
                $(".notifmessage").addClass("hidden");
            }
        } 
    });


 /***** signal Hub configuration *****/

    if (connection === null) {
        // create signalR instance connection
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .build();

        // server signalr Hub method to brodcast message in the UI for the specific user
        connection.on("ReceiveMessage", function (response) {
            // display msg into view
            displayMessage(response, "privatemessage");

            //send real time notification if receiver is online
            const notifdate = response.messageDate;
            const notifreceiver = response.receiverName;
            const notifsender = response.senderName;

            if (notifreceiver == currentUser) {
                $("#notification .notificon").removeClass("hidden");
                $(".msg").html("").html('<i class="fas fa - envelope" aria-hidden="true"></i>You have received a message from <span id="notifsender" class= "notifsender-bold">' + notifsender+'</span >');
                $("#notifdate").html(notifdate);
                $("#notification .fa-bell").attr("data-count", 1);
            }

        });

        // signal Hub method SendGroupMessage to brodcast message in the UI for the specific group
        connection.on("ReceiveGroupMessage", function (response) {
            // display msg into view
            displayMessage(response, "groupmessage");

            //send real time notification if receiver is online
            const notifdate = response.messageDate;
            const notifreceiver = response.receiverName;
            const notifgroup = response.groupName;
            const notifsender = response.senderName;

            var in_grp = false;
            $('#chatGroups li').each(function () {
                var grpName = $(this).attr('data-group-name');
                if (notifgroup == grpName) {
                    in_grp = true;
                }
            });

            if (in_grp == true && notifsender !== currentUser) {
                $("#notification .notificon").removeClass("hidden");
                $(".msg").html("").html('<i class="fas fa-envelope" aria-hidden="true"></i>Your contact <span id="notifsender" class= "notifsender-bold">' + notifsender + '</span> have wrote a message in <span id="notifgroup" class= "notifgroup-bold">' + notifgroup + '</span>');
                $("#notifdate").html(notifdate);
                $("#notification .fa-bell").attr("data-count", 1);
            }
        });


        // server signalr Hub method UserConnected when a new user is connected
        connection.on("UserConnected", function (response) {
            const connectedusers = response.usersconnected;
            var connectionName = response.connectionName;
            var msg_date = response.messageDate;
            var exist = false;
            var userCount = parseInt($("#chatUsers").attr("data-count"));
            var nextCount = userCount + 1;

            // change the status of users(connected)
            for (i = 0; i < connectedusers.length; i++) {
                var nameValue = connectedusers[i].name;
                $('#chatUsers li').each(function () {
                    var contactId = $(this).attr('id');
                    var contactName = $(this).attr('data-contact-name');
                    if (contactName == nameValue) {
                        $("#" + contactId + " .status").addClass("connected");
                        $("#" + contactId + " .status").attr("data-status", "connected");
                    }

                    if (contactName == connectionName) {
                        exist = true;
                    }
                });
            }

            if (exist == false && connectionName !== currentUser) {
                const nextindex = connectedusers.length + 1;
                $("#chatUsers").append(
                    '<li class="user__item" id="contact-' + nextindex + '" data-contact-id="" data-contact-name="' + response.connectionName + '">' +
                    '<div class="avatar"> <img src="./images/default-avatar.png" alt="connect-avatar"></div>' +
                    '<span>' + response.connectionName + '</span><div class="status connected" data-status="connected"> </div>' +
                    '</li>'
                );
                $("#chatUsers").attr("data-count", nextCount);
                $(".b-user .h5").html("Contacts (" + nextCount + ")");
            }

            let tmpl = $(WelcomTmpl).html();
            tmpl = tmpl.replace("{{body}}", "Welcome to the Chat Room " + currentUser);
            tmpl = tmpl.replace("{{date}}", msg_date);
            tmpl = $(tmpl);

            // Send Welcome message only to the new connected Client
            if (response.connectionName == currentUser) {
                $("#chat-message-welcom").append(tmpl);
            }
        });


        connection.on("NotifCreateGroup", function (response) {
            var grpCount = parseInt($("#chatGroups").attr("data-count"));
            var nextCount = grpCount + 1;
            if (response.createBy != currentUser) {

                $("#notification .notificon").removeClass("hidden");
                $(".msg").html("").html('<i class="fas fa-envelope" aria-hidden="true"></i>Your are added by <span id="notifsender" class= "notifsender-bold">' + response.createBy + '</span > to the new Group <span id="notifgroup" class= "notifgroup-bold">' + response.groupName + '</span >');
                $("#notifdate").html(response.createDate);
                $("#notification .fa-bell").attr("data-count", 1);

                $("#chatGroups").append(
                    '<li class="user__item group-' + response.groupId + '" data-group-id="' + response.groupId + '" data-group-name="' + response.groupName + '">'+
                    '<div class="avatar" ><img src="./images/default-group-avatar.png" alt="group-avatar" >' +
                    '</div><span>' + response.groupName + '</span>' +
                     '</li > '
                );

                $("#chatGroups").attr("data-count", nextCount);
                $(".b-group .h5").html("Groups (" + nextCount + ")");
            }
        });


        // signalr Hub method UserDisconnected when a user is disconnected
        connection.on("UserDisconnected", function (reponse) {
            const connectedusers = response.usersconnected;

            // change the status of users(connected)
            for (i = 0; i < connectedusers.length; i++) {
                var nameValue = connectedusers[i].name;
                $('#chatUsers li').each(function () {
                    var contactId = $(this).attr('id');
                    var contactName = $(this).attr('data-contact-name');
                    if (contactName == nameValue) {
                        $("#" + contactId + " .status").addClass("connected");
                    } else {
                        $("#" + contactId + " .status").removeClass("connected");
                    }
                });
            }
        });

        // signalr Hub method when error generated
        connection.on("HubError", function (response) {
            alert(response.error);
        });

        // start signalr connection
        connection.start().catch(function (err) {
            return console.error(err.toString());
        });

        $("#messageInput").on("keyup", function (e)  {
            if (e.keyCode === 13) {
                $('#sendButton').click();
            }
        });


        //send message 
        $('#sendButton').on("click", function () {
            var message = $("#messageInput").val();
            var receiver = $("#chat-receiver").val();
            var typereceiver = $("#chat-receiver-type").val();
            $("#messageInput").val("");

            // verify message and username
            if (message.trim() && receiver.trim()) {

                // send to a specific user
                if (typereceiver == "user") {
                    // invoke client signalr method SendPrivateMessage to brodcast message to the specific user
                    connection.invoke("SendPrivateMessage", receiver, message).catch(function (err) {
                        return console.error(err.toString());
                    });
                }
                // send to specific group
                else {
                    // invoke client signalr method SendMessageToGroup to brodcast message to the specific group
                    connection.invoke("SendMessageToGroup", receiver, message).catch(function (err) {
                        return console.error(err.toString());
                    });
                }
            }
        });


        // choose group to chat with
        $(document).on("click", "#chatGroups li.user__item", function () {
            $(".info-users").addClass("hidden");
            $("#chat-message-welcom").addClass("hidden");
            conversationBody.removeClass("hidden");

            // remove the previous selected group/user
            removeActiveClass($("#chatUsers li.active"));
            removeActiveClass($("#chatGroups li.active"));

            // set the new group line active
            $(this).addClass("active");

            var groupname = $(this).attr("data-group-name");
            $("#receivername").html(groupname);
            $("#chat-receiver").val(groupname);
            $("#chat-receiver-type").val("group");
            $(".info").removeClass("hidden");

            $(".bg-light-left img").attr("src", "./images/default-group-avatar.png");

            conversationBody.attr("data-status","open");
            $("#chat-contact").removeClass("hidden");
            $("#chat-send").removeClass("hidden");
            $("#chat-message").html("");

            // invoke signalr method AddTogroup which add users connections to the given Group
            connection.invoke("AddToGroup", groupname);

            // get history's group
            getChat(groupname, "/Chat/ConversationWithGroup/");
        });
    }
});

