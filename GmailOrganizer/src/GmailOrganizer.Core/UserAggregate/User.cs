namespace GmailOrganizer.Core.UserAggregate;

public class User : EntityBase, IAggregateRoot
{
  public string GoogleUserId { get; private set; } = default!;
  public string Email { get; private set; } = default!;
  public AccessToken AccessToken { get; private set; } = default!;
  public RefreshToken RefreshToken { get; private set; } = default!;
  public DateTime TokenExpiry { get; private set; } = default!;

  public User(string googleUserId, string email, string accessToken, string refreshToken, DateTime tokenExpiry)
  {
    GoogleUserId = Guard.Against.NullOrEmpty(googleUserId, nameof(googleUserId));
    Email = Guard.Against.NullOrEmpty(email, nameof(email));
    AccessToken = new AccessToken(accessToken);
    RefreshToken = new RefreshToken(refreshToken);
    TokenExpiry = tokenExpiry;
  }

  protected User() { }

  public void UpdateRefreshToken(string refreshToken)
  {
    if (string.IsNullOrWhiteSpace(refreshToken))
      throw new ArgumentException("Refresh token cannot be empty", nameof(refreshToken));

    RefreshToken = new RefreshToken(refreshToken);
  }

  public User UpdateAccessToken(string newAccessToken, DateTime newExpiry)
  {
    AccessToken = new AccessToken(newAccessToken);
    TokenExpiry = newExpiry;
    return this;
  }

  public bool IsTokenExpired() => TokenExpiry <= DateTime.UtcNow;
}

public class AccessToken : ValueObject
{
  public string Value { get; private set; }

  public AccessToken(string value)
  {
    Value = Guard.Against.NullOrEmpty(value, nameof(value));
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}

public class RefreshToken : ValueObject
{
  public string Value { get; private set; }

  public RefreshToken(string value)
  {
    Value = Guard.Against.NullOrEmpty(value, nameof(value));
  }

  protected override IEnumerable<object> GetEqualityComponents()
  {
    yield return Value;
  }
}
