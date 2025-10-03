namespace GmailOrganizer.Core.UserAggregate.Entities;
public class SubscriptionStatus : SmartEnum<SubscriptionStatus>
{
  public static readonly SubscriptionStatus Active = new(nameof(Active), 1);
  public static readonly SubscriptionStatus Expired = new(nameof(Expired), 2);
  public static readonly SubscriptionStatus Cancelled = new(nameof(Cancelled), 3);

  protected SubscriptionStatus(string name, int value) : base(name, value) { }
}

