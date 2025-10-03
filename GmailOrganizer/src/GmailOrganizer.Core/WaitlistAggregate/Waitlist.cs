namespace GmailOrganizer.Core.WaitlistAggregate;

public class Waitlist : EntityBase, IAggregateRoot
{
  public string Email { get; private set; } = default!;
  public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

  public Waitlist(string email)
  {
    Email = Guard.Against.NullOrEmpty(email, nameof(email));
  }

  // Constructor vacío protegido para EF Core
  protected Waitlist() { }
}
