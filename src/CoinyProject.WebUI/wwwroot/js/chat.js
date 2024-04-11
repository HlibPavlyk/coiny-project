
var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);

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
    sender.textContent = message.sender; 

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
    textInput.value = ""; 
}*/