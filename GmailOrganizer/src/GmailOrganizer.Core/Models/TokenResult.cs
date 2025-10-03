namespace GmailOrganizer.Core.Models;

public class TokenResult
{
  public bool Success { get; set; }
  public string? AccessToken { get; set; }
  public string? RefreshToken { get; set; }
  public DateTime? ExpiresAt { get; set; }
  public string? Message { get; set; }
}
