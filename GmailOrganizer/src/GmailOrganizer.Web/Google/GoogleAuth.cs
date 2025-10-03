using GmailOrganizer.UseCases.Auth.Login;

namespace GmailOrganizer.Web.Google;

public class GoogleAuth(IMediator mediator)
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
    var result = await mediator.Send(new GenerateGoogleAuthUrlCommand(), ct);

    if (!result.IsSuccess)
    {
      await SendErrorsAsync(cancellation: ct);
      return;
    }

    Response = new GoogleAuthResponse(result.Value.AuthUrl, result.Value.State);
  }
}
