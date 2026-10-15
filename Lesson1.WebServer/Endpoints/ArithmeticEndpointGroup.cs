using Lesson1.WebServer.Contracts;
using Lesson1.WebServer.Infrastructure;
using Lesson1.WebServer.Services;

namespace Lesson1.WebServer.Endpoints;

public sealed class ArithmeticEndpointGroup(IMathService math) : IEndpointGroup
{
    public string Name => "Вычисления";

    private static object Err(string message, string? code = null) => new { error = new ApiError(message, code) };

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        api.MapGet("/sum", HandleSum).WithTags(Name);
        api.MapGet("/divide", HandleDivide).WithTags(Name);
    }

    private IResult HandleSum(HttpContext httpContext)
    {
        var operands = QueryNumberParser.ReadPair(httpContext.Request.Query, "a", "b");
        if (operands.Failed)
        {
            return TypedResults.BadRequest(Err(operands.ErrorMessage!, ApiErrorCodes.InvalidParameters));
        }

        return TypedResults.Ok(new SumResponse(
            operands.First,
            operands.Second,
            math.Add(operands.First, operands.Second)));
    }

    private IResult HandleDivide(HttpContext httpContext)
    {
        var operands = QueryNumberParser.ReadPair(httpContext.Request.Query, "a", "b");
        if (operands.Failed)
        {
            return TypedResults.BadRequest(Err(operands.ErrorMessage!, ApiErrorCodes.InvalidParameters));
        }

        if (operands.Second == 0d)
        {
            return TypedResults.BadRequest(Err(ApiErrorMessages.DivisionByZero, ApiErrorCodes.DivisionByZero));
        }

        if (!math.TryDivide(operands.First, operands.Second, out var quotient))
        {
            return TypedResults.BadRequest(Err(ApiErrorMessages.Overflow, ApiErrorCodes.Overflow));
        }

        // Возвращаем поля, соответствующие JavaScript-клиенту в Index.cshtml
        return TypedResults.Ok(new DivideResponse(operands.First, operands.Second, quotient));
    }
}