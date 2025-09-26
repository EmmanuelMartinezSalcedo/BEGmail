using GmailOrganizer.Core.Services;

namespace GmailOrganizer.Web.GoogleAuth;
public class GoogleAuth(IGmailService _authService, ILogger<GoogleAuth> _logger)
  : EndpointWithoutRequest<GoogleAuthResponse>
{
  public override void Configure()
  {
    Get(GoogleAuthRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Inicia el flujo de autenticación con Google.";
      s.Description = "Devuelve la URL a la que el usuario debe ser redirigido para loguearse con Google.";
    });
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var state = Guid.NewGuid().ToString();
    var authUrl = await _authService.GenerateAuthUrlAsync(state);

    _logger.LogInformation("Generated Google Auth URL with state {State}", state);

    Response = new GoogleAuthResponse(authUrl, state);
  }
}
