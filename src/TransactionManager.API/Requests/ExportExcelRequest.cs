using System.ComponentModel.DataAnnotations;
using TransactionManager.API.ValidationAttributes;

namespace TransactionManager.API.Requests;

public class ExportExcelRequest : IValidatableObject
{
    /// <summary>
    /// Is the start date in the specified time zone. e.g.: 2022-12-31, 2022-12-31 23:59:59.
    /// </summary>
    [Required(ErrorMessage = "Please specify the start date.")]
    public DateTime StartDate { get; init; }
    /// <summary>
    /// Is the end date in the specified time zone. Exclusive.
    /// </summary>
    [Required(ErrorMessage = "Please specify the end date.")]
    public DateTime EndDate { get; init; }
    /// <summary>
    /// The time zone identifier from tzdb (https://en.wikipedia.org/wiki/Tz_database). e.g.: "Europe/London" or "Europe/Kyiv".
    /// </summary>
    [Required(ErrorMessage = "Please specify the IANA time zone.")]
    public string IanaTimeZone { get; init; } = null!;
    /// <summary>
    /// The fields to export in the resulting Excel file. Supported: ["transaction_id", "name", "email", "amount", "transaction_date", "client_location"]
    /// </summary>
    [OnlyAllowedTransactionFields]
    public IEnumerable<string> Fields { get; init; } = new List<string>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate > EndDate)
        {
            yield return new ValidationResult("The start date must be less than the end date.");
        }
    }
}