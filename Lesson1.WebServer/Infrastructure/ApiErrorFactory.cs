using Lesson1.WebServer.Contracts;

namespace Lesson1.WebServer.Infrastructure;

public static class ApiErrorFactory
{
    public static ApiError ValidationError(string message) => new(message, ApiErrorCodes.EmptyTaskTitle);
    public static ApiError TaskNotFound() => new(ApiErrorMessages.TaskNotFound, ApiErrorCodes.TaskNotFound);
    public static ApiError CategoryNotFound() => new(ApiErrorMessages.CategoryNotFound, ApiErrorCodes.CategoryNotFound);
    public static ApiError InvalidParameters(string message) => new(message, ApiErrorCodes.InvalidParameters);
    public static ApiError Unauthorized() => new(ApiErrorMessages.Unauthorized, ApiErrorCodes.Unauthorized);
    public static ApiError InternalError() => new(ApiErrorMessages.InternalServerError, ApiErrorCodes.InternalServerError);
    public static ApiError DivideByZero() => new(ApiErrorMessages.DivisionByZero, ApiErrorCodes.DivisionByZero);
    public static ApiError Overflow() => new(ApiErrorMessages.Overflow, ApiErrorCodes.Overflow);
}
