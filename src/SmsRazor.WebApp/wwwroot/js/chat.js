"use strict";

document.addEventListener("DOMContentLoaded", function () {
    const chatForm = document.getElementById("chatForm");

    // Only initialize SignalR if we're on an active conversation
    if (!chatForm) return;

    const messageInput = document.getElementById("messageInput");
    const fileInput = document.getElementById("fileInput");
    const messagesList = document.getElementById("messagesList");
    const conversationId = document.getElementById("conversationIdInput").value;
    const receiverId = document.getElementById("receiverIdInput").value;
    const currentUserId = document.getElementById("currentUserIdInput").value;

    const filePreviewContainer = document.getElementById("filePreviewContainer");
    const fileNamePreview = document.getElementById("fileNamePreview");
    const clearFileBtn = document.getElementById("clearFileBtn");

    const emojiBtn = document.getElementById("emojiBtn");

    // Initialize Emoji Picker
    if (emojiBtn && window.picmoPopup) {
        const picker = picmoPopup.createPopup({
            rootElement: document.body,
        }, {
            triggerElement: emojiBtn,
            referenceElement: emojiBtn,
            position: 'top-start'
        });

        // Toggle picking overlay
        emojiBtn.addEventListener('click', () => {
            picker.toggle();
        });

        // Insert Emoji to Input at Cursor
        picker.addEventListener('emoji:select', selection => {
            const start = messageInput.selectionStart;
            const end = messageInput.selectionEnd;
            const text = messageInput.value;
            messageInput.value = text.substring(0, start) + selection.emoji + text.substring(end);

            // Move cursor past the emoji
            messageInput.selectionStart = messageInput.selectionEnd = start + selection.emoji.length;
            messageInput.focus();
        });
    }

    // Initialize SignalR Connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    // Start Connection
    connection.start().then(function () {
        console.log("SignalR Connected.");
        // Join the specific conversation group
        connection.invoke("JoinConversation", conversationId).catch(function (err) {
            return console.error("Error joining conversation: ", err.toString());
        });
    }).catch(function (err) {
        return console.error("SignalR Connection Error: ", err.toString());
    });

    // Handle incoming messages
    connection.on("ReceiveMessage", function (messagePayload) {
        // Only render if it belongs to the current active window
        if (messagePayload.conversationId === conversationId) {
            appendMessage(messagePayload);
        }
    });

    // Handle incoming message edits
    connection.on("MessageEdited", function (data) {
        const msgDiv = document.getElementById(`msg-${data.messageId}`);
        if (msgDiv) {
            const textSpan = msgDiv.querySelector('.msg-text');
            if (textSpan) {
                textSpan.textContent = data.content;
                if (!msgDiv.innerHTML.includes('(edited)')) {
                    textSpan.insertAdjacentHTML('afterend', '<small class="ms-1 fw-light" style="font-size: 0.65rem;">(edited)</small>');
                }
            }
        }
    });

    // Handle incoming message deletions
    connection.on("MessageDeleted", function (data) {
        const msgDiv = document.getElementById(`msg-${data.messageId}`);
        if (msgDiv) {
            const contentContainer = msgDiv.querySelector('.msg-content');
            if (contentContainer) {
                contentContainer.innerHTML = '<em class="opacity-75"><i class="fa-solid fa-ban me-1"></i>This message was deleted</em>';
            }
            // Remove dropdown
            const dropdown = msgDiv.querySelector('.dropdown');
            if (dropdown) dropdown.remove();
        }
    });

    // Handle Enter key to submit
    messageInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            chatForm.dispatchEvent(new Event("submit"));
        }
    });

    // Handle File Selection
    fileInput.addEventListener("change", function () {
        if (fileInput.files.length > 0) {
            const file = fileInput.files[0];
            fileNamePreview.textContent = file.name;
            filePreviewContainer.classList.remove("d-none");
            messageInput.placeholder = "Add a caption...";
        } else {
            clearFileSelection();
        }
    });

    clearFileBtn.addEventListener("click", function () {
        clearFileSelection();
    });

    // Form Submit Handler
    chatForm.addEventListener("submit", async function (e) {
        e.preventDefault();

        const content = messageInput.value.trim();
        const file = fileInput.files.length > 0 ? fileInput.files[0] : null;

        if (!content && !file) return;

        let messageType = 0; // Text
        let fileUrl = null;
        let finalContent = content;
        let fileName = null;

        // If there's a file, we need to upload it first via a standard API endpoint
        if (file) {
            messageType = 1; // File
            fileName = file.name;

            const formData = new FormData();
            formData.append("file", file);

            try {
                // We need an API endpoint to physically upload to Blob Storage and return the URL
                const response = await fetch('/api/ChatFile/Upload', {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    const result = await response.json();
                    fileUrl = result.fileUrl; // Blob Storage URL
                    finalContent = fileUrl; // For file types, content is the URL
                } else {
                    alert("Failed to upload file.");
                    return;
                }
            } catch (error) {
                console.error("File upload error:", error);
                alert("Error connecting to upload service.");
                return;
            }
        }

        // Send via SignalR
        connection.invoke("SendMessage", receiverId, finalContent, messageType, fileName)
            .catch(function (err) {
                return console.error("Send message error: ", err.toString());
            });

        // Clear UI
        messageInput.value = "";
        clearFileSelection();
    });

    function clearFileSelection() {
        fileInput.value = "";
        filePreviewContainer.classList.add("d-none");
        fileNamePreview.textContent = "";
        messageInput.placeholder = "Aa";
    }

    // Helper to dynamically render a new bubble
    function appendMessage(msg) {
        // Format time
        const date = new Date(msg.createdAt);
        const timeString = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        const isMine = msg.senderId === currentUserId;
        const bubbleClass = isMine ? "bg-primary text-white ms-auto shadow-sm" : "bg-white text-dark me-auto border shadow-sm";
        const alignmentClass = isMine ? "align-items-end" : "align-items-start";
        const flexRowClass = isMine ? "flex-row-reverse" : "flex-row";

        let contentHtml = "";

        if (msg.type === 0) { // Text
            contentHtml = `
                <div class="p-3 py-2 rounded-4 ${bubbleClass} text-wrap text-break msg-content" style="max-width: fit-content;">
                    <span class="msg-text">${escapeHtml(msg.content)}</span>
                </div>`;
        } else if (msg.type === 1) { // File
            const lowerFileName = (msg.fileName || "").toLowerCase();
            const isImage = lowerFileName.endsWith(".jpg") || lowerFileName.endsWith(".png") || lowerFileName.endsWith(".jpeg");

            if (isImage) {
                contentHtml = `
                    <div class="p-2 rounded-4 ${bubbleClass} msg-content" style="max-width: fit-content;">
                        <a href="${msg.content}" target="_blank">
                            <img src="${msg.content}" class="img-fluid rounded-3" style="max-height: 200px;" alt="Attached Image" />
                        </a>
                    </div>`;
            } else {
                const textColorClass = isMine ? "text-white" : "text-primary";
                contentHtml = `
                    <div class="p-2 rounded-4 ${bubbleClass} msg-content" style="max-width: fit-content;">
                        <a href="${msg.content}" target="_blank" class="${textColorClass} text-decoration-none d-flex align-items-center gap-2 p-1">
                            <i class="fa-solid fa-file-arrow-down fa-2x"></i>
                            <div>
                                <div class="fw-bold small">${escapeHtml(msg.fileName || "Download File")}</div>
                            </div>
                        </a>
                    </div>`;
            }
        }

        let dropdownHtml = "";
        if (isMine) {
            dropdownHtml = `
            <div class="dropdown">
                <button class="btn btn-link text-muted p-0 ms-1 opacity-50 shadow-none border-0" type="button" data-bs-toggle="dropdown" aria-expanded="false" style="outline: none;">
                    <i class="fa-solid fa-ellipsis-vertical fs-6"></i>
                </button>
                <ul class="dropdown-menu shadow-sm border-0 fs-7" style="min-width: 120px;">
                    <li><a class="dropdown-item py-2" href="javascript:void(0)" onclick="editMessage('${msg.messageId}')"><i class="fa-solid fa-pen text-secondary me-2"></i>Edit</a></li>
                    <li><a class="dropdown-item py-2 text-danger" href="javascript:void(0)" onclick="deleteMessage('${msg.messageId}')"><i class="fa-solid fa-trash me-2"></i>Delete</a></li>
                </ul>
            </div>`;
        }

        const msgDiv = document.createElement("div");
        msgDiv.className = `d-flex flex-column ${alignmentClass} mw-75 mb-2`;
        msgDiv.id = `msg-${msg.messageId}`;
        msgDiv.innerHTML = `
            <div class="d-flex align-items-center ${flexRowClass} gap-2">
                ${contentHtml}
                ${dropdownHtml}
            </div>
            <small class="text-muted mt-1" style="font-size: 0.65rem;">${timeString}</small>
        `;

        messagesList.appendChild(msgDiv);

        // Auto-scroll to bottom
        messagesList.scrollTop = messagesList.scrollHeight;
    }

    // Simple XSS prevention for user input
    function escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    window.editMessage = function (messageId) {
        const msgDiv = document.getElementById(`msg-${messageId}`);
        if (!msgDiv) return;

        const textSpan = msgDiv.querySelector('.msg-text');
        if (!textSpan) {
            alert("This message cannot be edited.");
            return;
        }

        const currentText = textSpan.textContent;
        const newContent = prompt("Edit your message:", currentText);

        if (newContent !== null && newContent.trim() !== "" && newContent !== currentText) {
            const conversationId = document.getElementById("conversationIdInput").value;
            connection.invoke("EditMessage", conversationId, messageId, newContent.trim())
                .catch(err => console.error(err.toString()));
        }
    };

    window.deleteMessage = function (messageId) {
        if (confirm("Are you sure you want to delete this message?")) {
            const conversationId = document.getElementById("conversationIdInput").value;
            connection.invoke("DeleteMessage", conversationId, messageId)
                .catch(err => console.error(err.toString()));
        }
    };
});
