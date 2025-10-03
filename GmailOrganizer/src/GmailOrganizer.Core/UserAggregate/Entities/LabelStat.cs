namespace GmailOrganizer.Core.UserAggregate.Entities;
public class LabelStat : EntityBase
{
  public int UserId { get; private set; }
  public string LabelName { get; private set; } = default!;
  public int EmailCount { get; private set; } = 0;


  protected LabelStat() { }

  public LabelStat(int userId, string labelName)
  {
    UserId = Guard.Against.NegativeOrZero(userId, nameof(userId));
    LabelName = Guard.Against.NullOrEmpty(labelName, nameof(labelName));
  }

  public void IncrementEmailCount()
  {
    EmailCount++;
  }
}
