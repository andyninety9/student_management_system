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
    const picker = document.querySelector('emoji-picker');
    if (emojiBtn && picker) {
        // Toggle picking overlay
        emojiBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            picker.classList.toggle('d-none');
        });

        // Insert Emoji to Input at Cursor
        picker.addEventListener('emoji-click', event => {
            const start = messageInput.selectionStart;
            const end = messageInput.selectionEnd;
            const text = messageInput.value;
            messageInput.value = text.substring(0, start) + event.detail.unicode + text.substring(end);

            // Move cursor past the emoji
            messageInput.selectionStart = messageInput.selectionEnd = start + event.detail.unicode.length;
            messageInput.focus();
        });

        // Click outside to hide
        document.addEventListener('click', (e) => {
            if (!picker.contains(e.target) && !emojiBtn.contains(e.target)) {
                picker.classList.add('d-none');
            }
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

    // Handle User Typing indicator
    const typingIndicator = document.getElementById("typingIndicator");
    let typingTimeout;

    connection.on("UserTyping", function (data) {
        // Only show if the typing event is from the active conversation and NOT from ourselves
        if (data.conversationId === conversationId && data.userId !== currentUserId) {
            if (data.isTyping) {
                typingIndicator.classList.remove("d-none");
                typingIndicator.classList.add("d-flex");

                // Auto-scroll so they see the indicator
                messagesList.scrollTop = messagesList.scrollHeight;
            } else {
                typingIndicator.classList.add("d-none");
                typingIndicator.classList.remove("d-flex");
            }
        }
    });

    // Broadcast our own typing status
    let isTyping = false;
    messageInput.addEventListener("input", function () {
        if (!isTyping) {
            isTyping = true;
            connection.invoke("NotifyTyping", conversationId, true).catch(err => console.error(err));
        }

        clearTimeout(typingTimeout);
        typingTimeout = setTimeout(() => {
            isTyping = false;
            connection.invoke("NotifyTyping", conversationId, false).catch(err => console.error(err));
        }, 2000);
    });

    // Handle Enter key to submit
    messageInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            chatForm.dispatchEvent(new Event("submit"));

            // Immediately kill the typing indicator when sent
            isTyping = false;
            clearTimeout(typingTimeout);
            connection.invoke("NotifyTyping", conversationId, false).catch(err => console.error(err));
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
    chatForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const content = messageInput.value.trim();
        const file = fileInput.files.length > 0 ? fileInput.files[0] : null;

        if (!content && !file) return;

        // Immediately clear UI to not block the user
        messageInput.value = "";
        clearFileSelection();

        if (file) {
            handleFileUploadAsync(content, file);
        } else {
            // Send text immediately
            connection.invoke("SendMessage", receiverId, content, 0, null)
                .catch(err => console.error("Send text error: ", err.toString()));
        }
    });

    async function handleFileUploadAsync(textCaption, file) {
        const tempMsgId = "temp-" + Date.now();
        const fileName = file.name;

        // 1. Inject Temporary Loading Bubble immediately
        appendTempFileMessage(tempMsgId, fileName);

        let fileUrl = null;
        const formData = new FormData();
        formData.append("file", file);

        try {
            // 2. Perform Async Blob Upload in background
            const response = await fetch('/api/ChatFile/Upload', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const result = await response.json();
                fileUrl = result.fileUrl;
            } else {
                removeTempMessage(tempMsgId);
                Swal.fire("Upload Failed", "The server rejected the file.", "error");
                return;
            }
        } catch (error) {
            console.error("File upload error:", error);
            removeTempMessage(tempMsgId);
            Swal.fire("Network Error", "Failed to connect to upload service.", "error");
            return;
        }

        // 3. Invoke SignalR with the newly returned Azure Blob link
        // SignalR will automatically broadcast the 'ReceiveMessage' containing the link to both clients
        // The sender will receive a 'permanent' bubble on screen from the broadcast.
        try {
            await connection.invoke("SendMessage", receiverId, fileUrl, 1, fileName);
            removeTempMessage(tempMsgId); // clean up the placeholder

            // If they also typed a caption during the file send, fire off a secondary text message
            if (textCaption) {
                await connection.invoke("SendMessage", receiverId, textCaption, 0, null);
            }
        } catch (err) {
            console.error("Send file-message error: ", err.toString());
            removeTempMessage(tempMsgId);
        }
    }

    function removeTempMessage(tempId) {
        const msgDiv = document.getElementById(`msg-${tempId}`);
        if (msgDiv) msgDiv.remove();
    }

    function appendTempFileMessage(tempId, fileName) {
        const timeString = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const bubbleClass = "bg-primary text-white ms-auto shadow-sm";
        const alignmentClass = "align-items-end";
        const flexRowClass = "flex-row-reverse";

        let contentHtml = `
            <div class="p-2 rounded-4 ${bubbleClass} msg-content opacity-75 d-flex align-items-center gap-2" style="max-width: fit-content;">
                <div class="spinner-border spinner-border-sm text-white" role="status"></div>
                <div class="fw-bold small">Sending ${escapeHtml(fileName)}...</div>
            </div>`;

        const msgDiv = document.createElement("div");
        msgDiv.className = `d-flex flex-column ${alignmentClass} mw-75 mb-2`;
        msgDiv.id = `msg-${tempId}`;
        msgDiv.innerHTML = `
            <div class="d-flex align-items-center ${flexRowClass} gap-2">
                ${contentHtml}
            </div>
            <small class="text-muted mt-1" style="font-size: 0.65rem;">${timeString}</small>
        `;

        const typingIndicator = document.getElementById("typingIndicator");
        if (typingIndicator) {
            messagesList.insertBefore(msgDiv, typingIndicator);
        } else {
            messagesList.appendChild(msgDiv);
        }
        messagesList.scrollTop = messagesList.scrollHeight;
    }

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
            const isVideo = lowerFileName.endsWith(".mp4") || lowerFileName.endsWith(".webm") || lowerFileName.endsWith(".ogg");
            const isPdf = lowerFileName.endsWith(".pdf");

            if (isImage) {
                contentHtml = `
                    <div class="p-2 rounded-4 ${bubbleClass} msg-content" style="max-width: fit-content;">
                        <a href="${msg.content}" target="_blank">
                            <img src="${msg.content}" class="img-fluid rounded-3" style="max-height: 200px;" alt="Attached Image" />
                        </a>
                    </div>`;
            } else if (isVideo) {
                const videoExt = lowerFileName.substring(lowerFileName.lastIndexOf('.') + 1);
                contentHtml = `
                    <div class="p-2 rounded-4 ${bubbleClass} msg-content" style="max-width: fit-content;">
                        <video controls style="max-width: 300px; max-height: 200px;" class="rounded-3">
                            <source src="${msg.content}" type="video/${videoExt}">
                            Your browser does not support the video tag.
                        </video>
                    </div>`;
            } else if (isPdf) {
                const textColorClass = isMine ? "text-white" : "text-primary";
                contentHtml = `
                    <div class="p-2 rounded-4 ${bubbleClass} msg-content" style="max-width: fit-content;">
                        <a href="${msg.content}" target="_blank" class="${textColorClass} text-decoration-none d-flex align-items-center gap-2 p-1">
                            <i class="fa-solid fa-file-pdf fa-2x text-danger"></i>
                            <div>
                                <div class="fw-bold small">${escapeHtml(msg.fileName)}</div>
                                <small class="opacity-75">Click to view PDF</small>
                            </div>
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

        // If the typing indicator exists and is visible, insert the message right BEFORE it
        const typingIndicator = document.getElementById("typingIndicator");
        if (typingIndicator) {
            messagesList.insertBefore(msgDiv, typingIndicator);
        } else {
            messagesList.appendChild(msgDiv);
        }

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
            Swal.fire("Error", "This message cannot be edited.", "error");
            return;
        }

        const currentText = textSpan.textContent;

        Swal.fire({
            title: 'Edit Message',
            input: 'textarea',
            inputValue: currentText,
            showCancelButton: true,
            confirmButtonText: 'Save',
            confirmButtonColor: '#0d6efd',
            cancelButtonColor: '#6c757d',
            inputValidator: (value) => {
                if (!value.trim()) {
                    return 'Message cannot be empty!';
                }
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const newContent = result.value.trim();
                if (newContent !== currentText) {
                    const conversationId = document.getElementById("conversationIdInput").value;
                    connection.invoke("EditMessage", conversationId, messageId, newContent)
                        .catch(err => Swal.fire({ icon: 'error', title: 'Oops...', text: err.toString() }));
                }
            }
        });
    };

    window.deleteMessage = function (messageId) {
        Swal.fire({
            title: 'Delete Message?',
            text: "This action cannot be undone!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                const conversationId = document.getElementById("conversationIdInput").value;
                connection.invoke("DeleteMessage", conversationId, messageId)
                    .catch(err => Swal.fire({ icon: 'error', title: 'Oops...', text: err.toString() }));
            }
        });
    };
});
