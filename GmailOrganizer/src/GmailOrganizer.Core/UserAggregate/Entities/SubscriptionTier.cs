namespace GmailOrganizer.Core.UserAggregate.Entities;
public class SubscriptionTier : SmartEnum<SubscriptionTier>
{
  public static readonly SubscriptionTier Basic = new(nameof(Basic), 1);
  public static readonly SubscriptionTier Medium = new(nameof(Medium), 2);
  public static readonly SubscriptionTier Advanced = new(nameof(Advanced), 3);

  protected SubscriptionTier(string name, int value) : base(name, value) { }
}

