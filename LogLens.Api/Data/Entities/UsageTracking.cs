namespace LogLens.Api.Data.Entities;

public class UsageTracking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Identifier { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public int SessionCount { get; set; }
}
