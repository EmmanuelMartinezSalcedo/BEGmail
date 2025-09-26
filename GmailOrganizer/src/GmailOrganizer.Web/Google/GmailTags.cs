using System.IdentityModel.Tokens.Jwt;
using Ardalis.SharedKernel;
using GmailOrganizer.Core.Services;
using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Entities;
using GmailOrganizer.Core.UserAggregate.Specifications;
using GmailOrganizer.Web.GmailTags;

public class GmailTags(
  IGmailService gmailService,
  IRepository<User> userRepo,
  ILogger<GmailTags> logger)
  : EndpointWithoutRequest<GmailLabelsResult>
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

    var user = await userRepo.FirstOrDefaultAsync(new UserByGoogleIdSpec(googleUserId), ct);
    if (user is null)
    {
      AddError("Usuario no encontrado");
      return;
    }

    logger.LogInformation("Fetching labels for user {Email}", user.Email);

    try
    {
      var labelsResult = await gmailService.GetLabelsAsync(user.AccessToken.Value, user.RefreshToken.Value, ct);
      Response = labelsResult;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error fetching Gmail labels");
      AddError($"Error fetching labels: {ex.Message}");
    }
  }
}
