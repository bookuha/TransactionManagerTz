using System.ComponentModel.DataAnnotations;

namespace TransactionManager.API.ValidationAttributes;

public class AllowedTransactionFieldsAttribute : ValidationAttribute
{
    private static readonly string[] AllowedFields = ["id", "name", "email", "amount", "date", "location"];
    
    protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
    {
        if (value is string[] fields)
        {
            if (fields.Length == 0)
            {
                return new ValidationResult("At least one field must be specified.");
            }
            
            if (fields.Any(f => !AllowedFields.Contains(f)))
            {
                return new ValidationResult(GetErrorMessage());
            }
        }
        
        return ValidationResult.Success;
    }
    
    private static string GetErrorMessage() => "Only the following fields are allowed: id, name, email, amount, date, location.";
    
}