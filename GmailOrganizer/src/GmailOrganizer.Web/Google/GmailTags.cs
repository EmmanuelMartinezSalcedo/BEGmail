using System.IdentityModel.Tokens.Jwt;
using GmailOrganizer.UseCases.Gmail.GetLabels;

namespace GmailOrganizer.Web.Google;

public class GmailTags(IMediator mediator)
  : EndpointWithoutRequest<GmailTagsResponse>
{
  public override void Configure()
  {
    Get(GmailTagsRequest.Route);
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var jwt = HttpContext.Request.Cookies["jwt"];
    if (string.IsNullOrEmpty(jwt))
    {
      AddError("Usuario no autenticado");
      return;
    }

    var handler = new JwtSecurityTokenHandler();
    var token = handler.ReadJwtToken(jwt);
    var googleUserId = token.Claims.FirstOrDefault(c => c.Type == "GoogleUserId")?.Value;

    if (string.IsNullOrEmpty(googleUserId))
    {
      AddError("JWT inválido");
      return;
    }

    var result = await mediator.Send(new GetGmailLabelsCommand(googleUserId), ct);

    if (!result.IsSuccess)
    {
      AddError(string.Join("; ", result.Errors));
      return;
    }

    Response = new GmailTagsResponse(
      result.Value.Success,
      result.Value.Message,
      result.Value.SystemLabels.Select(l => new LabelDto(l.Id, l.Name, l.Type)).ToList(),
      result.Value.UserLabels.Select(l => new LabelDto(l.Id, l.Name, l.Type)).ToList(),
      result.Value.AllLabels.Select(l => new LabelDto(l.Id, l.Name, l.Type)).ToList(),
      result.Value.TotalCount
    );
  }
}
