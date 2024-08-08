namespace TransactionManager.Data.Model;

public class Transaction
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    // Note: Implies dollar
    public required decimal Amount { get; init; }
    public required DateTimeOffset Date { get; init; }
    public required string IanaTimeZone { get; init; }
    public required Location ClientLocation { get; init; }
}