namespace GmailOrganizer.Core.UserAggregate.Entities;
public class Subscription : EntityBase
{
  public int UserId { get; private set; }
  public SubscriptionTier Tier { get; private set; } = SubscriptionTier.Basic;
  public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Active;
  public DateTime StartDate { get; private set; } = DateTime.UtcNow;
  public DateTime EndDate { get; private set; } = DateTime.UtcNow.AddMonths(1);
  public int EmailsProcessed { get; private set; } = 0;
  public int EmailLimit { get; private set; } = 100;


  protected Subscription() { }

  public Subscription(int userId)
  {
    UserId = Guard.Against.NegativeOrZero(userId, nameof(userId));
  }
}
