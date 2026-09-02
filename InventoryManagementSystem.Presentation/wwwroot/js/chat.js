const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

const toButton = document.getElementById("chat-to");
const toLabel = document.getElementById("chat-to-label");
const toMenu = document.getElementById("chat-to-menu");
const messagesBox = document.getElementById("chat-messages");
const textInput = document.getElementById("chat-text");
const sendButton = document.getElementById("chat-send");
const unreadBadge = document.getElementById("chat-unread");
const chatBody = document.getElementById("chat-body");
const chatToggle = document.getElementById("chat-toggle");
const chatBox = document.getElementById("chat-box");
const searchInput = document.getElementById("chat-search");
const noMatch = document.getElementById("chat-no-match");

let filterText = "";

let contacts = [];

let selectedId = "";

const onlineUsers = new Set();

const unreadCounts = new Map();

const ONLINE_DOT = "\u{1F7E2}";
const OFFLINE_DOT = "\u{1F7E4}";

const BROADCAST_VALUE = "__all_employees__";
const BROADCAST_LABEL = "\u{1F4E2} All employees";

const canBroadcast = toButton.dataset.canBroadcast === "true";

function renderContacts() {
    toMenu.replaceChildren();

    if (canBroadcast && filterText === "") {
        toMenu.appendChild(buildRow(BROADCAST_VALUE, BROADCAST_LABEL, null, 0));
        toMenu.appendChild(buildDivider());
    }

    const matches = contacts.filter(function (contact) {
        return filterText === "" || contact.name.toLowerCase().includes(filterText);
    });

    matches.forEach(function (contact) {
        toMenu.appendChild(buildRow(
            contact.id,
            contact.name,
            onlineUsers.has(contact.id),
            unreadCounts.get(contact.id) || 0));
    });

    noMatch.classList.toggle("d-none", matches.length > 0);

    renderLabel();
    renderUnreadTotal();
}

function buildRow(id, name, isOnline, unread) {
    const item = document.createElement("li");

    const button = document.createElement("button");
    button.type = "button";
    button.className = "dropdown-item d-flex align-items-center gap-2";

    button.dataset.userId = id;

    if (id === selectedId) {
        button.classList.add("active");
    }

    const slot = document.createElement("span");
    slot.className = "chat-count-slot";

    if (unread > 0) {
        const count = document.createElement("span");
        count.className = "chat-count";
        count.textContent = unread;
        slot.appendChild(count);
    }

    button.appendChild(slot);

    if (isOnline !== null) {
        const dot = document.createElement("span");
        dot.textContent = isOnline ? ONLINE_DOT : OFFLINE_DOT;
        button.appendChild(dot);
    }

    const label = document.createElement("span");
    label.className = "text-truncate";
    label.textContent = name;
    button.appendChild(label);

    item.appendChild(button);

    return item;
}

function buildDivider() {
    const item = document.createElement("li");
    const line = document.createElement("hr");
    line.className = "dropdown-divider";
    item.appendChild(line);

    return item;
}

function renderLabel() {
    if (!selectedId) {
        toLabel.textContent = "Select a user...";
        return;
    }

    if (selectedId === BROADCAST_VALUE) {
        toLabel.textContent = BROADCAST_LABEL;
        return;
    }

    const contact = contacts.find(function (c) { return c.id === selectedId; });

    toLabel.textContent = contact
        ? (onlineUsers.has(selectedId) ? ONLINE_DOT : OFFLINE_DOT) + " " + contact.name
        : "Select a user...";
}

function renderUnreadTotal() {
    let total = 0;

    unreadCounts.forEach(function (count) {
        total += count;
    });

    unreadBadge.textContent = total;
    unreadBadge.classList.toggle("d-none", total === 0);
}

function isConversationVisible(userId) {
    return selectedId === userId && chatBody.style.display !== "none";
}

function addUnread(userId) {
    unreadCounts.set(userId, (unreadCounts.get(userId) || 0) + 1);

    renderContacts();
}

function markRead(userId) {
    if (!userId || userId === BROADCAST_VALUE || !unreadCounts.get(userId)) {
        return;
    }

    unreadCounts.set(userId, 0);

    renderContacts();

    connection.invoke("MarkConversationRead", userId)
        .catch(function (err) {
            console.error("Could not mark the conversation as read:", err);
        });
}

connection.on("UserPresenceChanged", function (userId, isOnline) {
    if (isOnline) {
        onlineUsers.add(userId);
    } else {
        onlineUsers.delete(userId);
    }

    renderContacts();
});

connection.on("ReceiveMessage", function (fromUserId, message) {
    if (isConversationVisible(fromUserId)) {
        addMessage(message, false);
        markRead(fromUserId);
        return;
    }

    addUnread(fromUserId);

    if (selectedId === fromUserId) {
        return;
    }

    selectId(fromUserId);
});

connection.start()
    .then(loadContacts)
    .catch(function (err) {
        console.error("Chat could not connect:", err);
    });

connection.onreconnected(loadContacts);

function loadContacts() {
    fetch("/Chat/Contacts")
        .then(function (response) { return response.json(); })
        .then(function (users) {
            contacts = [];
            onlineUsers.clear();
            unreadCounts.clear();

            users.forEach(function (user) {
                contacts.push({ id: user.id, name: user.name });

                if (user.isOnline) {
                    onlineUsers.add(user.id);
                }

                if (user.unread > 0) {
                    unreadCounts.set(user.id, user.unread);
                }
            });

            renderContacts();

            if (isConversationVisible(selectedId)) {
                markRead(selectedId);
            }
        })
        .catch(function (err) {
            console.error("Could not load contacts:", err);
        });
}

function selectId(userId) {
    selectedId = userId;

    filterText = "";
    searchInput.value = "";

    renderContacts();
    loadHistory(userId);

    bootstrap.Dropdown.getOrCreateInstance(toButton).hide();
}

searchInput.addEventListener("input", function () {
    filterText = searchInput.value.trim().toLowerCase();

    renderContacts();
});

searchInput.addEventListener("keydown", function (e) {
    if (e.key !== "Enter") {
        return;
    }

    e.preventDefault();

    const rows = toMenu.querySelectorAll("[data-user-id]");

    if (rows.length === 1) {
        selectId(rows[0].dataset.userId);
    }
});

toButton.addEventListener("shown.bs.dropdown", function () {
    searchInput.focus();
});

toMenu.addEventListener("click", function (e) {
    const row = e.target.closest("[data-user-id]");

    if (row) {
        selectId(row.dataset.userId);
    }
});

function loadHistory(userId) {
    messagesBox.replaceChildren();

    if (!userId) {
        return;
    }

    if (userId === BROADCAST_VALUE) {
        const hint = document.createElement("div");
        hint.className = "text-muted fst-italic";
        hint.textContent = "This message will be sent to every employee.";
        messagesBox.appendChild(hint);
        return;
    }

    fetch("/Chat/History?withUserId=" + encodeURIComponent(userId))
        .then(function (response) { return response.json(); })
        .then(function (messages) {
            if (selectedId !== userId) {
                return;
            }

            messages.forEach(function (m) {
                addMessage(m.message, m.mine);
            });

            if (isConversationVisible(userId)) {
                markRead(userId);
            }
        })
        .catch(function (err) {
            console.error("Could not load the conversation:", err);
        });
}

function send() {
    const message = textInput.value.trim();

    if (!selectedId) {
        alert("Choose who to send the message to first.");
        return;
    }

    if (!message) {
        return;
    }

    const isBroadcast = selectedId === BROADCAST_VALUE;

    const sending = isBroadcast
        ? connection.invoke("SendBroadcastMessage", message)
        : connection.invoke("SendPrivateMessage", selectedId, message);

    sending
        .then(function () {
            addMessage(message, true);
            textInput.value = "";
        })
        .catch(function (err) {
            console.error("Message not sent:", err);
        });
}

sendButton.addEventListener("click", send);

textInput.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
        send();
    }
});

function addMessage(message, mine) {
    const row = document.createElement("div");
    row.className = mine ? "mb-1 text-end" : "mb-1";

    const bubble = document.createElement("span");
    bubble.className = "d-inline-block px-2 py-1 rounded " + (mine ? "bg-primary text-white" : "bg-light border");

    bubble.textContent = message;

    row.appendChild(bubble);
    messagesBox.appendChild(row);

    messagesBox.scrollTop = messagesBox.scrollHeight;
}

function syncChatSpacer() {
    document.body.style.paddingBottom = (chatBox.offsetHeight + 24) + "px";
}

const COLLAPSED_KEY = "chat.collapsed";

function setCollapsed(collapsed) {
    chatBody.style.display = collapsed ? "none" : "";
    chatToggle.textContent = collapsed ? "+" : "–";

    try {
        localStorage.setItem(COLLAPSED_KEY, collapsed ? "1" : "0");
    } catch (err) {
    }

    syncChatSpacer();
}

chatToggle.addEventListener("click", function () {
    const isHidden = chatBody.style.display === "none";

    setCollapsed(!isHidden);

    if (isHidden && selectedId) {
        loadHistory(selectedId);
    }
});

try {
    if (localStorage.getItem(COLLAPSED_KEY) === "1") {
        setCollapsed(true);
    }
} catch (err) {
}

syncChatSpacer();

window.addEventListener("resize", syncChatSpacer);

window.addEventListener("pagehide", function () {
    connection.stop();
});
