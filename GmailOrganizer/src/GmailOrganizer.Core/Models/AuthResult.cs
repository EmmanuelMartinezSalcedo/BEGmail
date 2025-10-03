using GmailOrganizer.Core.UserAggregate;

namespace GmailOrganizer.Core.Models;
public class AuthResult
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
  public string? AccessToken { get; set; }
  public string? RefreshToken { get; set; }
  public DateTime? ExpiresAt { get; set; }
  public string? GoogleUserId { get; set; }
  public string? Email { get; set; }
  }
