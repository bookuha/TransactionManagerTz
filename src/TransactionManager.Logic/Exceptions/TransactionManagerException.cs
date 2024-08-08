namespace TransactionManager.Logic.Exceptions;

public class TransactionManagerException(string entityName, Errors error, string title, string message)
    : Exception(message)
{
    public string EntityName { get; } = entityName;
    public Errors Error { get; } = error;
    public string Title { get; } = title;

    public static TransactionManagerException WrongTransactionField(string field)
    {
        return new TransactionManagerException("Transaction",  Errors.WrongFlow, "Wrong Field",
            $"You cannot use the '{field}' for extraction.");
    }
    
    public static TransactionManagerException InvalidTimeZone(string timezone)
    {
        return new TransactionManagerException("Transaction",  Errors.WrongFlow, "Invalid Timezone",
            $"The timezone '{timezone}' is invalid.");
    }
}

public enum Errors
{
    WrongFlow
    // Already exists and others can be added here
}