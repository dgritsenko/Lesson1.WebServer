const TONE_CLASS = {
    info: "message message--info",
    success: "message message--success",
    error: "message message--error"
};

export function el(tag, className, textContent = "") {
    const node = document.createElement(tag);
    if (className) {
        node.className = className;
    }
    if (textContent !== "") {
        node.textContent = textContent;
    }
    return node;
}

export function clear(node) {
    node.replaceChildren();
}

export function showMessage(node, text, tone = "info") {
    clear(node);
    node.appendChild(el("p", TONE_CLASS[tone] ?? TONE_CLASS.info, text));
}

export function renderPairs(node, entries) {
    clear(node);
    const list = el("dl", "pairs");
    entries.forEach(([key, value]) => {
        list.appendChild(el("dt", "pairs__key", key));
        list.appendChild(el("dd", "pairs__value", String(value ?? "")));
    });
    node.appendChild(list);
}

export function errorMessageFrom(status, payload) {
    if (payload?.error?.message) {
        return payload.error.message;
    }
    if (typeof payload?.error === "string") {
        return payload.error;
    }
    return status === 0
        ? "Сервер не отвечает"
        : `Сервер вернул код ${status} без описания ошибки`;
}

export function submitOnEnter(input, handler) {
    input.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            handler();
        }
    });
}
