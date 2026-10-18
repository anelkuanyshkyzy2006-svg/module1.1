using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Validation;

/// <summary>Дата должна быть в прошлом и не раньше 1900 года.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PastDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateOnly date)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (date >= today || date.Year < 1900)
            {
                return new ValidationResult(
                    ErrorMessage ?? "BirthDate must be a valid date in the past (not earlier than 1900).",
                    new[] { validationContext.MemberName ?? "BirthDate" });
            }
        }

        return ValidationResult.Success;
    }
}
