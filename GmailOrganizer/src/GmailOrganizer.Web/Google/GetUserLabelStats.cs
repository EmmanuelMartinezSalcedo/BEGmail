using GmailOrganizer.Core.UserAggregate.Entities;
using System.Security.Claims;
using GmailOrganizer.UseCases.Gmail.GetLabelStats;

namespace GmailOrganizer.Web.Google;

public class GetUserLabelStatsEndpoint : EndpointWithoutRequest<GetUserLabelStatsResponse>
{
  private readonly IMediator _mediator;

  public GetUserLabelStatsEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override void Configure()
  {
    Get(GetUserLabelStatsRequest.Route);
    Summary(s =>
    {
      s.Summary = "Obtiene los LabelStats del usuario logueado";
      s.Description = "Devuelve la lista de LabelStats usando la cookie JWT para identificar al usuario";
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

    var result = await _mediator.Send(new GetUserLabelStatsCommand(userId), ct);

    if (!result.IsSuccess)
{
    await SendAsync(new GetUserLabelStatsResponse(LabelStats: new List<LabelStat>(), Errors: result.Errors.ToList()), 400, ct);
    return;
}

    Response = new GetUserLabelStatsResponse(LabelStats: result.Value);
  }
}
