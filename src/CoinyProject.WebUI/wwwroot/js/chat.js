"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user} says ${message}`;
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

/*class DiscussionMessageCreateDTO {
    constructor(text) {
        this.text = text;
    }
}

document.getElementById('submitButton').addEventListener('click', () => { });

// userName is declared in razor page.
const username = userName;
const textInput = document.getElementById('messageText');
const chat = document.getElementById('chat');
const messagesQueue = [];



function clearInputField() {
    messagesQueue.push(textInput.value);
    textInput.value = "";
}

function addMessageToChat(message) {
    let container = document.createElement('div');
    container.className = "container";

    let sender = document.createElement('p');
    sender.className = "sender";
    sender.textContent = message.sender; // Припускаючи, що ви передаєте таке поле з сервера

    let text = document.createElement('p');
    text.textContent = message.text;

    container.appendChild(sender);
    container.appendChild(text);
    chat.appendChild(container);
}

function sendMessageToHub() {
    let messageText = textInput.value;
    if (messageText.trim() === "") return;

    connection.invoke('SendMessage', messageText);
    textInput.value = ""; // Очищаємо поле вводу після відправлення повідомлення
}*/