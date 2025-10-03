using GmailOrganizer.Infrastructure.BackgroundServices;

namespace GmailOrganizer.Web.Google;

public class ToggleGmailBackgroundServiceEndpoint : EndpointWithoutRequest<GmailBackgroundServiceToggleResponse>
{
  private readonly GmailClassificationBackgroundService _service;

  public ToggleGmailBackgroundServiceEndpoint(GmailClassificationBackgroundService service)
  {
    _service = service;
  }

  public override void Configure()
  {
    Post(GmailBackgroundServiceRequest.ToggleRoute);
    Summary(s =>
    {
      s.Summary = "Activa o desactiva el GmailClassificationBackgroundService";
      s.Description = "Cada vez que se llama al endpoint, cambia el estado del servicio (activado/desactivado)";
    });
  }

  public override Task HandleAsync(CancellationToken ct)
  {
    if (_service.IsEnabled())
    {
      _service.Disable();
      Response = new GmailBackgroundServiceToggleResponse("Background service desactivado");
    }
    else
    {
      _service.Enable();
      Response = new GmailBackgroundServiceToggleResponse("Background service activado");
    }

    return Task.CompletedTask;
  }
}
