using System.ComponentModel.DataAnnotations;

namespace TransactionManager.API.ValidationAttributes;

public class AllowedFileExtensionsAttribute(string[] extensions) : ValidationAttribute
{
    protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!extensions.Contains(extension.ToLower())) return new ValidationResult(GetErrorMessage());
        }

        return ValidationResult.Success;
    }

    private static string GetErrorMessage()
    {
        return "Only csv files are allowed.";
    }
}