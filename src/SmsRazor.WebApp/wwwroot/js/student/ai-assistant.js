const aiConnection = new signalR.HubConnectionBuilder()
    .withUrl("/assistantHub")
    .withAutomaticReconnect()
    .build();

const aiChatMessages = document.getElementById("aiChatMessages");
const aiMessageInput = document.getElementById("aiMessageInput");
const aiSendButton = document.getElementById("aiSendButton");
const aiTypingIndicator = document.getElementById("aiTypingIndicator");

let currentStreamContainer = null;
let currentStreamContent = "";

aiConnection.on("ReceiveAssistantStreamStart", (responseId) => {
    aiTypingIndicator.classList.add("d-none");

    // Create new message container
    const messageDiv = document.createElement("div");
    messageDiv.className = "chat-message ai-message d-flex message-appear mt-3";

    messageDiv.innerHTML = `
        <div class="message-content bg-white border p-3 rounded-4 shadow-sm" style="max-width: 80%; border-bottom-left-radius: 4px !important;">
            <div class="markdown-body" id="ai-response-${responseId}"></div>
            <div class="message-time mt-1 text-muted small text-end" style="font-size: 11px;">${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
        </div>
    `;

    aiChatMessages.appendChild(messageDiv);
    currentStreamContainer = document.getElementById(`ai-response-${responseId}`);
    currentStreamContent = "";

    scrollToBottom();
});

aiConnection.on("ReceiveAssistantStreamChunk", (responseId, chunk) => {
    if (currentStreamContainer) {
        currentStreamContent += chunk;
        currentStreamContainer.innerHTML = marked.parse(currentStreamContent);
        scrollToBottom();
    }
});

aiConnection.on("ReceiveAssistantStreamEnd", (responseId) => {
    currentStreamContainer = null;
    currentStreamContent = "";
    aiMessageInput.focus();
    checkInputState();
});

aiConnection.on("ReceiveAssistantError", (errorMsg) => {
    aiTypingIndicator.classList.add("d-none");

    const messageDiv = document.createElement("div");
    messageDiv.className = "chat-message ai-message d-flex message-appear mt-3";

    messageDiv.innerHTML = `
        <div class="message-content bg-danger text-white p-3 rounded-4 shadow-sm" style="max-width: 80%; border-bottom-left-radius: 4px !important;">
            <div><i class="fa-solid fa-circle-exclamation me-1"></i> ${errorMsg}</div>
        </div>
    `;

    aiChatMessages.appendChild(messageDiv);
    scrollToBottom();
    currentStreamContainer = null;
    checkInputState();
});

aiConnection.start().catch(err => console.error("Error connecting to AI Hub: ", err));

function checkInputState() {
    aiSendButton.disabled = aiMessageInput.value.trim().length === 0 || currentStreamContainer !== null;
}

aiMessageInput.addEventListener("input", function () {
    this.style.height = "auto";
    this.style.height = (this.scrollHeight) + "px";
    checkInputState();
});

aiMessageInput.addEventListener("keypress", function (e) {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        if (!aiSendButton.disabled) {
            sendMessage();
        }
    }
});

aiSendButton.addEventListener("click", sendMessage);

async function sendMessage() {
    const text = aiMessageInput.value.trim();
    if (!text) return;

    aiMessageInput.value = "";
    aiMessageInput.style.height = "auto";
    checkInputState();

    // Append user message
    const messageDiv = document.createElement("div");
    messageDiv.className = "chat-message user-message d-flex justify-content-end message-appear mt-3";
    messageDiv.innerHTML = `
        <div class="message-content p-3 rounded-4 shadow-sm" style="max-width: 80%;">
            <div>${text.replace(/\n/g, '<br>')}</div>
            <div class="message-time mt-1 text-white-50 small text-end" style="font-size: 11px;">${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</div>
        </div>
    `;
    aiChatMessages.appendChild(messageDiv);
    scrollToBottom();

    aiTypingIndicator.classList.remove("d-none");

    try {
        await aiConnection.invoke("SendMessageToAssistant", text);
    } catch (err) {
        console.error(err);
        aiTypingIndicator.classList.add("d-none");
    }
}

function scrollToBottom() {
    aiChatMessages.scrollTop = aiChatMessages.scrollHeight;
}
