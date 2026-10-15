using System.ComponentModel.DataAnnotations;

namespace Lesson1.WebServer.Services;

public sealed class NoForbiddenWordsAttribute : ValidationAttribute
{
    private static readonly string[] ForbiddenWords = ["тест"];

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text)
        {
            return ValidationResult.Success;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        var normalized = text.ToLowerInvariant();
        foreach (var word in ForbiddenWords)
        {
            if (normalized.Contains(word))
            {
                return new ValidationResult("Название не должно содержать слово 'тест'");
            }
        }

        return ValidationResult.Success;
    }
}
