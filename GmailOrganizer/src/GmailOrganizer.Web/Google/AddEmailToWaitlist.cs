using GmailOrganizer.UseCases.AddEmailToWaitlist;

namespace GmailOrganizer.Web.Google;

public class AddEmailToWaitlistEndpoint : Endpoint<AddEmailToWaitlistRequest, AddEmailToWaitlistResponse>
{
  private readonly IMediator _mediator;

  public AddEmailToWaitlistEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post(AddEmailToWaitlistRequest.Route);
    AllowAnonymous(); // si quieres permitir acceso sin autenticación
    Summary(s =>
    {
      s.ExampleRequest = new AddEmailToWaitlistRequest
      {
        Email = "test@example.com"
      };
    });
  }

  public override async Task HandleAsync(AddEmailToWaitlistRequest req, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(req.Email))
    {
      await SendAsync(new AddEmailToWaitlistResponse(false, "Email no proporcionado"), 400, ct);
      return;
    }

    var result = await _mediator.Send(new AddEmailToWaitlistCommand(req.Email), ct);

    if (!result.IsSuccess)
    {
      await SendAsync(
        new AddEmailToWaitlistResponse(false, result.Errors?.FirstOrDefault() ?? "Error desconocido"),
        400,
        ct
      );
      return;
    }

    Response = new AddEmailToWaitlistResponse(true, "Email agregado correctamente");
  }
}
