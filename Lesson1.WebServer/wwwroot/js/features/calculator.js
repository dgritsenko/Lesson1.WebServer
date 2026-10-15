import { errorMessageFrom, showMessage } from "../ui.js";
import { submitOnEnter } from "../ui.js";

export function createCalculatorFeature({ aInput, bInput, button, output, request, format, onStatus }) {
    const calculate = async () => {
        const response = await request(aInput.value.trim(), bInput.value.trim());

        if (!response.ok) {
            showMessage(output, errorMessageFrom(response.status, response.payload), "error");
            return;
        }

        showMessage(output, format(response.payload), "success");
        onStatus("Расчёт выполнен");
    };

    button.addEventListener("click", calculate);
    submitOnEnter(aInput, calculate);
    submitOnEnter(bInput, calculate);

    return { calculate };
}
