using GmailOrganizer.Core.UserAggregate.ValueObjects;
using GmailOrganizer.Core.UserAggregate.Entities;

namespace GmailOrganizer.Core.UserAggregate;

public class User : EntityBase, IAggregateRoot
{
  public string GoogleUserId { get; private set; } = default!;
  public string Email { get; private set; } = default!;
  public AccessToken AccessToken { get; private set; } = default!;
  public RefreshToken RefreshToken { get; private set; } = default!;
  public DateTime? TokenExpiry { get; private set; }
  public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

  public virtual List<EmailProcessingLog>? EmailProcessingLogs { get; private set; }
  public virtual List<LabelStat>? LabelStats { get; private set; }
  public virtual Subscription? Subscription { get; private set; }


  public User(string googleUserId, string email, string accessToken, string refreshToken, DateTime? tokenExpiry)
  {
    GoogleUserId = Guard.Against.NullOrEmpty(googleUserId, nameof(googleUserId));
    Email = Guard.Against.NullOrEmpty(email, nameof(email));

    AccessToken = new AccessToken(
      Guard.Against.NullOrEmpty(accessToken, nameof(accessToken))
    );

    RefreshToken = new RefreshToken(
      Guard.Against.NullOrEmpty(refreshToken, nameof(refreshToken))
    );

    TokenExpiry = tokenExpiry;
  }

  protected User() { }

  public void UpdateRefreshToken(string refreshToken)
  {
    RefreshToken = new RefreshToken(
      Guard.Against.NullOrEmpty(refreshToken, nameof(refreshToken))
    );
  }

  public User UpdateAccessToken(string newAccessToken, DateTime? newExpiry)
  {
    AccessToken = new AccessToken(
      Guard.Against.NullOrEmpty(newAccessToken, nameof(newAccessToken))
    );
    TokenExpiry = newExpiry;
    return this;
  }

  public LabelStat AddOrGetLabelStat(string labelName)
  {
    if (LabelStats == null)
      LabelStats = new List<LabelStat>();

    var existing = LabelStats.FirstOrDefault(ls =>
        ls.LabelName.Equals(labelName, StringComparison.OrdinalIgnoreCase));

    if (existing != null)
      return existing;

    var newStat = new LabelStat(Id, labelName);
    LabelStats.Add(newStat);
    return newStat;
  }

  public void AddEmailProcessingLog(string labelAssigned)
  {
    EmailProcessingLogs ??= new List<EmailProcessingLog>();
    EmailProcessingLogs.Add(new EmailProcessingLog(Id, labelAssigned));
  }
}
