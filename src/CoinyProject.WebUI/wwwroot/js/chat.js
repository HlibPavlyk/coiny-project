var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

document.getElementById("sendButton").disabled = true;
var currentUser = document.getElementById("currentUser").dataset.userId;

connection.on("ReceiveMessage", function (user, message) {
    var div = document.createElement("div");
    document.getElementById("messagesList").appendChild(div);

    div.classList.add("message-item", "mb-3", "border", "rounded", "p-2");

    var badge = document.createElement("span");
    badge.classList.add("badge", "bg-info");
    badge.textContent = user;

    var messageContent = document.createElement("div");
    messageContent.classList.add("message-content");
    messageContent.textContent = message;

    if (user === currentUser) {
        div.classList.add("text-end", "bg-body-tertiary");
        badge.classList.add("bg-danger");
    }

    div.appendChild(badge);
    div.appendChild(messageContent);
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;

    if (message.trim() === "") {
        return;
    }

    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById("messageInput").value = "";
    event.preventDefault();
});
