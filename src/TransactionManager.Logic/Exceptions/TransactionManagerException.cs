using CsvHelper.TypeConversion;

namespace TransactionManager.Logic.Exceptions;

public class TransactionManagerException: Exception
{
    public TransactionManagerException(string entityName, Errors error, string title, string message)
        : base(message)
    {
        EntityName = entityName;
        Error = error;
        Title = title;
    }
    protected TransactionManagerException(string entityName, Errors error, string title, string message, Exception innerException)
        : base(message, innerException)
    {
        EntityName = entityName;
        Error = error;
        Title = title;
    }
    
    public string EntityName { get; }
    public Errors Error { get; }
    public string Title { get; }

    public static TransactionManagerException WrongTransactionField(string field)
    {
        return new TransactionManagerException("Transaction", Errors.WrongFlow, "Wrong Field",
            $"You cannot use the '{field}' for extraction.");
    }

    public static TransactionManagerException InvalidTimeZone(string timezone)
    {
        return new TransactionManagerException("Transaction", Errors.WrongFlow, "Invalid Timezone",
            $"The timezone '{timezone}' is invalid.");
    }
    
    public static TransactionManagerException CsvParsingError(TypeConverterException ex)
    {
        var message = "An error occurred while parsing the CSV file.";
        if (ex.Context is not null && ex.Context.Parser is not null && ex.Context.Reader is not null)
        {
            message += $" Specifically, at row {ex.Context.Parser.Row} and column {ex.Context.Reader.CurrentIndex + 1}." +
                       $"Text {ex.Text}.";
        }
        return new TransactionManagerException("Transaction", Errors.WrongFlow, "CSV Parsing Error",
            message, ex);
    }
}

public enum Errors
{
    WrongFlow
    // Already exists and others can be added here
}