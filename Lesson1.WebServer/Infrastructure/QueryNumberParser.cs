using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Lesson1.WebServer.Infrastructure;

public readonly record struct NumericPairOutcome(double First, double Second, string? ErrorMessage)
{
    public bool Failed => ErrorMessage is not null;

    public static NumericPairOutcome Success(double first, double second) => new(first, second, null);

    public static NumericPairOutcome Failure(string message) => new(default, default, message);
}

public static class QueryNumberParser
{
    public static NumericPairOutcome ReadPair(IQueryCollection query, string firstKey, string secondKey)
    {
        var keys = new[] { firstKey, secondKey };
        var values = new double[keys.Length];
        var missing = new List<string>();
        var invalid = new List<string>();

        for (var index = 0; index < keys.Length; index++)
        {
            if (!query.TryGetValue(keys[index], out var raw) || string.IsNullOrWhiteSpace(raw))
            {
                missing.Add(keys[index]);
                continue;
            }

            if (!TryReadNumber(raw, out var value))
            {
                invalid.Add(keys[index]);
                continue;
            }

            values[index] = value;
        }

        if (invalid.Count > 0)
        {
            return NumericPairOutcome.Failure(ApiErrorMessages.InvalidParameters);
        }

        if (missing.Count == keys.Length)
        {
            return NumericPairOutcome.Failure(ApiErrorMessages.MissingParameters(firstKey, secondKey));
        }

        if (missing.Count > 0)
        {
            return NumericPairOutcome.Failure(ApiErrorMessages.MissingParameter(missing[0]));
        }

        return NumericPairOutcome.Success(values[0], values[1]);
    }

    private static bool TryReadNumber(string? candidate, out double value)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            value = default;
            return false;
        }

        var text = candidate.Trim();
        var parsed = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);

        return parsed && double.IsFinite(value);
    }
}
