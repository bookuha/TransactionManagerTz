namespace TransactionManager.Logic.Exceptions;

public class TransactionManagerException(string entityName, string error, string title, string message)
    : Exception(message)
{
    public string EntityName { get; } = entityName;
    public string Error { get; } = error;
    public string Title { get; } = title;

    public static TransactionManagerException WrongTransactionField(string field)
    {
        return new TransactionManagerException("Transaction", "WrongField", "Wrong Field",
            $"You cannot use the '{field}' for extraction.");
    }
}