namespace Lesson1.WebServer.Infrastructure;

public static class ApiErrorMessages
{
    public const string InvalidParameters = "Некорректные параметры";
    public const string DivisionByZero = "Деление на ноль невозможно";
    public const string Overflow = "Результат выходит за диапазон допустимых значений";
    public const string TaskNotFound = "Задача не найдена";
    public const string EmptyTaskTitle = "Название задачи не может быть пустым";
    public const string MissingEchoText = "В теле запроса ожидается строковое поле text";
    public const string ResourceNotFound = "Ресурс не найден";
    public const string MethodNotAllowed = "Метод не поддерживается для этого адреса";
    public const string Unauthorized = "Доступ запрещён";
    public const string Forbidden = "Запрос отклонён";
    public const string InternalServerError = "Внутренняя ошибка сервера";
    public const string RateLimitExceeded = "Слишком много запросов. Попробуйте позже.";
    public const string CategoryNotFound = "Категория не найдена";

    public static string MissingParameter(string name) => $"Не передан обязательный параметр {name}";

    public static string MissingParameters(string firstName, string secondName) =>
        $"Не переданы обязательные параметры {firstName} и {secondName}";
}

public static class ApiErrorCodes
{
    public const string InvalidParameters = "INVALID_PARAMETERS";
    public const string DivisionByZero = "DIVISION_BY_ZERO";
    public const string Overflow = "OVERFLOW";
    public const string TaskNotFound = "TASK_NOT_FOUND";
    public const string EmptyTaskTitle = "VALIDATION_ERROR";
    public const string MissingEchoText = "MISSING_ECHO_TEXT";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
}
