using System.Security.Claims;
using GmailOrganizer.UseCases.Gmail.GetUserProcessedEmails;

namespace GmailOrganizer.Web.Google;

public class GetUserProcessedEmailsEndpoint : EndpointWithoutRequest<GetUserProcessedEmailsResponse>
{
  private readonly IMediator _mediator;

  public GetUserProcessedEmailsEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get(GetUserProcessedEmailsRequest.Route);
    Summary(s =>
    {
      s.Summary = "Obtiene los emails procesados en las últimas 12 horas del usuario logueado";
      s.Description = "Devuelve la lista de EmailProcessingLogs usando la cookie JWT para identificar al usuario";
    });
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userIdClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userIdClaim))
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    int userId = int.Parse(userIdClaim);

    var result = await _mediator.Send(new GetUserProcessedEmailsCommand(userId), ct);

    if (!result.IsSuccess)
    {
      await SendAsync(
        new GetUserProcessedEmailsResponse(
          Emails: new List<EmailProcessingLogDto>(),
          Errors: result.Errors.ToList()
        ),
        400,
        ct
      );
      return;
    }

    var dtoList = result.Value
        .Select(log => new EmailProcessingLogDto(log.ProcessedAt, log.LabelAssigned))
        .ToList();

    Response = new GetUserProcessedEmailsResponse(dtoList);
  }
}
