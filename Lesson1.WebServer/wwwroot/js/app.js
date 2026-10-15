import { api } from "./api-client.js";
import { createCalculatorFeature } from "./features/calculator.js";
import { createEchoFeature } from "./features/echo.js";
import { createServerInfoFeature } from "./features/server-info.js";
import { createTaskManagerFeature } from "./features/tasks.js";

const byId = (id) => document.getElementById(id);

function format(value) {
    return Number.isInteger(value) ? String(value) : String(Number(value.toFixed(6)));
}

function equation(symbol) {
    return (data) => `${format(data.a)} ${symbol} ${format(data.b)} = ${format(data.result)}`;
}

document.addEventListener("DOMContentLoaded", () => {
    const statusNode = byId("footer-status");
    const setStatus = (message) => {
        statusNode.textContent = message;
    };

    const serverInfo = createServerInfoFeature({
        button: byId("load-server-info"),
        output: byId("result"),
        onStatus: setStatus
    });

    createCalculatorFeature({
        aInput: byId("sum-a"),
        bInput: byId("sum-b"),
        button: byId("calculate-sum"),
        output: byId("sum-result"),
        request: api.sum,
        format: equation("+"),
        onStatus: setStatus
    });

    createCalculatorFeature({
        aInput: byId("divide-a"),
        bInput: byId("divide-b"),
        button: byId("calculate-divide"),
        output: byId("divide-result"),
        request: api.divide,
        format: equation("/"),
        onStatus: setStatus
    });

    createEchoFeature({
        input: byId("echo-text"),
        button: byId("send-echo"),
        output: byId("echo-result"),
        onStatus: setStatus
    });

    const taskManager = createTaskManagerFeature({
        list: byId("task-list"),
        feedback: byId("task-feedback"),
        showButton: byId("show-tasks"),
        titleInput: byId("new-task-title"),
        addButton: byId("add-task"),
        searchInput: byId("search-tasks"),
        onStatus: setStatus
    });

    serverInfo.load();
    taskManager.load();
});
