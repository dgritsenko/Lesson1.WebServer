import { api, createSearchApi } from "../api-client.js";
import { clear, el, showMessage, submitOnEnter } from "../ui.js";

/**
 * Фича "Task Manager" согласно ТЗ (Занятие 9):
 * - Индикаторы загрузки: блокировка кнопок + текст, try/finally
 * - Живой поиск с AbortController (race-condition avoidance)
 * - Централизованный apiRequest
 */
export function createTaskManagerFeature({
    list, feedback, showButton, titleInput, addButton,
    searchInput, onStatus
}) {
    // Живой поиск с AbortController
    const liveSearch = searchInput ? createSearchApi("/api/tasks/search") : null;

    let currentPage = 1;
    const pageSize = 5;

    /**
     * Включает/выключает индикатор загрузки на кнопке.
     */
    function setLoading(btn, loading, text = "Загрузка...") {
        if (loading) {
            btn.disabled = true;
            btn.dataset.prevText = btn.textContent;
            btn.textContent = text;
        } else {
            btn.disabled = false;
            btn.textContent = btn.dataset.prevText || btn.textContent;
            delete btn.dataset.prevText;
        }
    }

    /**
     * Извлекает ID из payload с fallback'ом
     */
    function getTaskId(payload) {
        return payload?.id ?? payload?.Id ?? "???";
    }

    /**
     * Извлекает название задачи из payload
     */
    function getTaskTitle(payload) {
        return payload?.title ?? payload?.Title ?? "";
    }

    /**
     * Загрузка списка задач с поддержкой пагинации, фильтрации и поиска.
     */
    const load = async (page = 1) => {
        setLoading(showButton, true);
        currentPage = page;

        try {
            const params = { page, pageSize };

            const response = await api.tasks(params);

            if (!response.ok) {
                showMessage(feedback, response.error || `Ошибка ${response.status}`, "error");
                return [];
            }

            const data = response.payload;
            const items = data?.items ?? (Array.isArray(data) ? data : []);
            const totalCount = data?.totalCount ?? (Array.isArray(data) ? data.length : 0);

            render(items);
            onStatus(`Задач в списке: ${totalCount}`);
            return items;
        } catch (err) {
            showMessage(feedback, `Ошибка: ${err.message}`, "error");
            return [];
        } finally {
            setLoading(showButton, false);
        }
    };

    /**
     * Создание задачи.
     */
    const add = async () => {
        const title = titleInput.value.trim();
        if (!title) {
            showMessage(feedback, "Введите название задачи", "error");
            return;
        }

        setLoading(addButton, true, "Создание...");

        try {
            const response = await api.createTask(title);

            if (!response.ok) {
                showMessage(feedback, response.error || "Ошибка создания задачи", "error");
                return;
            }

            const taskId = getTaskId(response.payload);
            const taskTitle = getTaskTitle(response.payload);

            titleInput.value = "";
            showMessage(feedback, `Создана задача «${taskTitle}» (ID: ${taskId}, статус ${response.status})`, "success");
            await load(currentPage);
        } finally {
            setLoading(addButton, false, "Добавить задачу");
        }
    };

    /**
     * Отметка выполнения задачи.
     */
    const mark = async (id, isDone) => {
        try {
            const response = await api.updateTask(id, { isDone });

            if (!response.ok) {
                showMessage(feedback, response.error || "Ошибка обновления", "error");
                await load(currentPage);
                return;
            }

            showMessage(feedback,
                `Задача #${id} помечена как ${isDone ? "выполненная" : "невыполненная"}`,
                "success");
            await load(currentPage);
        } catch (err) {
            showMessage(feedback, `Ошибка: ${err.message}`, "error");
            await load(currentPage);
        }
    };

    /**
     * Удаление задачи с индикатором на кнопке удаления.
     */
    const removeTask = async (id, btn) => {
        setLoading(btn, true, "Удаление...");

        try {
            const response = await api.deleteTask(id);

            // 204 No Content — success, payload будет null
            if (!response.ok && response.status !== 204) {
                showMessage(feedback, response.error || "Ошибка удаления", "error");
                await load(currentPage);
                return;
            }

            showMessage(feedback, `Задача #${id} удалена`, "success");
            await load(currentPage);
        } finally {
            setLoading(btn, false, "Удалить");
        }
    };

    /**
     * Живой поиск при вводе текста.
     */
    const onSearchInput = () => {
        const query = searchInput?.value.trim() || "";
        if (!liveSearch) return;

        liveSearch(query).then((response) => {
            if (response.aborted) return; // отменённый запрос — игнорируем

            if (response.ok) {
                render(response.payload ?? []);
                onStatus(`Результатов поиска: ${(response.payload ?? []).length}`);
            }
        });
    };

    /**
     * Создание строки задачи для списка.
     */
    const createRow = (task) => {
        const row = el("li", task.isDone ? "task task--done" : "task");

        const checkbox = el("input", "task__check");
        checkbox.type = "checkbox";
        checkbox.checked = task.isDone;
        checkbox.title = "Отметить выполнение";
        checkbox.addEventListener("change", () => mark(task.id, checkbox.checked));

        const removeButton = el("button", "btn btn--danger", "Удалить");
        removeButton.type = "button";
        removeButton.addEventListener("click", () => removeTask(task.id, removeButton));

        const categoryLabel = task.categoryName
            ? ` [${task.categoryName}]`
            : "";

        row.append(
            checkbox,
            el("span", "task__title", `${task.id}. ${task.title}${categoryLabel}`),
            removeButton
        );
        return row;
    };

    /**
     * Отрисовка списка задач.
     */
    const render = (items) => {
        clear(list);

        if (!items || !items.length) {
            list.appendChild(el("li", "task-list__empty", "Задач нет"));
            return;
        }

        items.forEach((task) => list.appendChild(createRow(task)));
    };

    // Подписка на события
    showButton.addEventListener("click", () => load(1));
    addButton.addEventListener("click", add);
    submitOnEnter(titleInput, add);

    if (searchInput) {
        searchInput.addEventListener("input", onSearchInput);
    }

    return { load, add };
}