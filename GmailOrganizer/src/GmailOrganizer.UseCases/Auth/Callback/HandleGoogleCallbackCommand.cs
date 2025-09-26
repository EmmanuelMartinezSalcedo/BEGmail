using GmailOrganizer.Core.UserAggregate;

namespace GmailOrganizer.UseCases.Auth.Callback;
public record HandleGoogleCallbackCommand(
    string Code,
    string State
) : ICommand<Result<GoogleCallbackResult>>;

public class GoogleCallbackResult
{
  public bool Success { get; set; }
  public string Message { get; set; } = default!;
  public User? User { get; set; }
  public bool IsNewUser { get; set; }
}
