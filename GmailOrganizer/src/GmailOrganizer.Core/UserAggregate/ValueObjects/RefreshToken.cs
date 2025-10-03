namespace GmailOrganizer.Core.UserAggregate.ValueObjects;
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
