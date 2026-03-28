namespace LogSage.Api.Data.Entities;

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string? DetectedFormat { get; set; }
    public int TotalLines { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
    public ICollection<ErrorGroupEntity> ErrorGroups { get; set; } = new List<ErrorGroupEntity>();
}
