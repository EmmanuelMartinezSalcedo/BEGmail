using GmailOrganizer.Core.Services;

namespace GmailOrganizer.UseCases.Auth.Login;
// Handler
public class LoginWithGoogleHandler(IGmailService _authService)
  : ICommandHandler<LoginWithGoogleCommand, Result<string>>
{
  public async Task<Result<string>> Handle(LoginWithGoogleCommand request, CancellationToken ct)
  {
    try
    {
      var url = await _authService.GenerateAuthUrlAsync(request.State);
      return Result.Success(url);
    }
    catch (Exception ex)
    {
      return Result.Error(ex.Message);
    }
  }
}
