const JSON_HEADERS = { "Content-Type": "application/json" };
const API_KEY_HEADER = "X-Api-Key";
const API_KEY = "dev-secret-key-12345";

/** Добавляет X-Api-Key заголовок для мутирующих запросов */
function asMutating(headers = {}) {
    return { ...headers, [API_KEY_HEADER]: API_KEY };
}

/**
 * Централизованная обёртка над fetch по ТЗ.
 * - Автоматический JSON-парсинг
 * - Извлечение ошибки из { error: { message, code } }
 * - Обработка AbortError (отменённый запрос)
 * - Возвращает { ok, status, payload, error }
 */
export async function apiRequest(url, options = {}) {
    try {
        const response = await fetch(url, options);
        const text = await response.text();
        
        let payload = null;
        if (text) {
            try {
                payload = JSON.parse(text);
            } catch {
                payload = { error: text };
            }
        }

        // Отладочный вывод
        if (url.includes('/tasks')) {
            console.log('[API] ' + url, 'Status:', response.status, 'Payload:', payload);
        }

        // Извлекаем message из envelope { error: { message, code } }
        let errorMessage = null;
        if (!response.ok) {
            if (payload?.error?.message) {
                errorMessage = payload.error.message;
            } else if (typeof payload?.error === "string") {
                errorMessage = payload.error;
            } else {
                errorMessage = `HTTP ${response.status}`;
            }
        }

        return { ok: response.ok, status: response.status, payload, error: errorMessage };
    } catch (err) {
        // AbortError — запрос был отменён (AbortController)
        if (err.name === "AbortError") {
            return { ok: false, status: 0, payload: null, error: "aborted", aborted: true };
        }

        return {
            ok: false,
            status: 0,
            payload: null,
            error: `Сервер недоступен: ${err.message}`
        };
    }
}

/**
 * Создаёт AbortController и apiRequest, который его использует.
 * Повторный вызов отменяет предыдущий запрос.
 */
export function createSearchApi(baseUrl) {
    let controller = null;

    return async (query) => {
        // Отменяем предыдущий поисковый запрос
        if (controller) {
            controller.abort();
        }

        controller = new AbortController();
        const signal = controller.signal;

        const params = new URLSearchParams();
        if (query) {
            params.set("q", query);
        }

        try {
            return await apiRequest(`${baseUrl}?${params.toString()}`, { signal });
        } finally {
            controller = null;
        }
    };
}

function withQuery(path, values) {
    const search = new URLSearchParams();
    Object.entries(values).forEach(([key, value]) => search.set(key, value));
    return `${path}?${search.toString()}`;
}

export const api = {
    serverInfo: () => apiRequest("/api/server-info"),
    sum: (a, b) => apiRequest(withQuery("/api/sum", { a, b })),
    divide: (a, b) => apiRequest(withQuery("/api/divide", { a, b })),
    echo: (text) => apiRequest("/api/echo", {
        method: "POST",
        headers: JSON_HEADERS,
        body: JSON.stringify({ text })
    }),
    tasks: (params = {}) => apiRequest(withQuery("/api/tasks", params)),
    createTask: (title) => apiRequest("/api/tasks", {
        method: "POST",
        headers: asMutating(JSON_HEADERS),
        body: JSON.stringify({ title })
    }),
    updateTask: (id, changes) => apiRequest(`/api/tasks/${id}`, {
        method: "PUT",
        headers: asMutating(JSON_HEADERS),
        body: JSON.stringify(changes)
    }),
    deleteTask: (id) => apiRequest(`/api/tasks/${id}`, {
        method: "DELETE",
        headers: asMutating()
    }),
    searchTasks: (query) => {
        const params = new URLSearchParams();
        if (query) params.set("q", query);
        return apiRequest(`/api/tasks/search?${params.toString()}`);
    },
    categories: () => apiRequest("/api/tasks/categories")
};