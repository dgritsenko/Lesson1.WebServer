import { api } from "../api-client.js";
import { errorMessageFrom, renderPairs, showMessage } from "../ui.js";

export function createServerInfoFeature({ button, output, onStatus }) {
    const load = async () => {
        const response = await api.serverInfo();

        if (!response.ok) {
            showMessage(output, errorMessageFrom(response.status, response.payload), "error");
            return;
        }

        renderPairs(output, [
            ["serverName", response.payload.serverName],
            ["version", response.payload.version],
            ["currentTime", response.payload.currentTime]
        ]);
        onStatus(`Информация о сервере обновлена: ${response.payload.currentTime}`);
    };

    button.addEventListener("click", load);
    return { load };
}
