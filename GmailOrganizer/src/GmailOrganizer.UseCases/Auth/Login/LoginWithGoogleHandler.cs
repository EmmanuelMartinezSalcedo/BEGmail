using Ardalis.Result;
using GmailOrganizer.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.UseCases.Auth.Login;

public class GenerateGoogleAuthUrlHandler(
    IGoogleAuthService googleAuthService,
    ILogger<GenerateGoogleAuthUrlHandler> logger)
    : ICommandHandler<GenerateGoogleAuthUrlCommand, Result<GenerateGoogleAuthUrlResult>>
{
  public async Task<Result<GenerateGoogleAuthUrlResult>> Handle(
      GenerateGoogleAuthUrlCommand request,
      CancellationToken ct)
  {
    var state = Guid.NewGuid().ToString();
    var authUrl = await googleAuthService.GenerateAuthUrlAsync(state);

    logger.LogInformation("Generated Google Auth URL with state {State}", state);

    return Result.Success(new GenerateGoogleAuthUrlResult(authUrl, state));
  }
}
