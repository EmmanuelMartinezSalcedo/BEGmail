namespace GmailOrganizer.Core.UserAggregate.Entities;
public class EmailProcessingLog : EntityBase
{
  public int UserId { get; private set; }
  public DateTime ProcessedAt { get; private set; } = DateTime.UtcNow;
  public string LabelAssigned { get; private set; } = default!;

  protected EmailProcessingLog() { }

  public EmailProcessingLog(int userId, string labelAssigned)
  {
    UserId = Guard.Against.NegativeOrZero(userId, nameof(userId));
    LabelAssigned = Guard.Against.NullOrEmpty(labelAssigned, nameof(labelAssigned));
  }
}
