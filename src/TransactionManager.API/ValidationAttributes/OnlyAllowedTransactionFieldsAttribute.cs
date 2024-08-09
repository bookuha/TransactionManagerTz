using System.ComponentModel.DataAnnotations;
using TransactionManager.Logic;

namespace TransactionManager.API.ValidationAttributes;

public class OnlyAllowedTransactionFieldsAttribute : ValidationAttribute
{
    private static readonly string[] AllowedFields = TransactionsService.HeaderFields;

    protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
    {
        if (value is string[] fields)
        {
            if (fields.Length == 0) return new ValidationResult(GetNoFieldsErrorMessage());

            if (fields.GroupBy(f => f).Any(g => g.Count() > 1))
                return new ValidationResult("Duplicate fields are not allowed.");

            if (fields.Any(f => !AllowedFields.Contains(f.ToLower())))
                return new ValidationResult(GetWrongFieldsErrorMessage());
        }

        return ValidationResult.Success;
    }

    private static string GetNoFieldsErrorMessage()
    {
        return "At least one field must be specified.";
    }

    private static string GetWrongFieldsErrorMessage()
    {
        return "Only the following fields are allowed: " + string.Join(", ", AllowedFields);
    }
}