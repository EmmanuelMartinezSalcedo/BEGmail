namespace GmailOrganizer.Core.UserAggregate.ValueObjects;
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
