namespace TransactionManager.Data.Model;

public class Transaction
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime Date { get; init; }
    public required Location ClientLocation { get; init; }
}