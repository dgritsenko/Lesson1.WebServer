import { api } from "../api-client.js";
import { errorMessageFrom, renderPairs, showMessage, submitOnEnter } from "../ui.js";

export function createEchoFeature({ input, button, output, onStatus }) {
    const send = async () => {
        const response = await api.echo(input.value);

        if (!response.ok) {
            showMessage(output, errorMessageFrom(response.status, response.payload), "error");
            return;
        }

        renderPairs(output, [
            ["original", response.payload.original],
            ["uppercase", response.payload.uppercase],
            ["length", response.payload.length]
        ]);
        onStatus("POST /api/echo выполнен");
    };

    button.addEventListener("click", send);
    submitOnEnter(input, send);

    return { send };
}
