using System.IdentityModel.Tokens.Jwt;
using Ardalis.SharedKernel;
using GmailOrganizer.Core.Services;
using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Specifications;
using GmailOrganizer.Web.GmailEmails;
using GmailOrganizer.Web.Google;

public class GmailEmailsClassified(
  IGmailService gmailService,
  IGmailClassificationService classificationService,
  IRepository<User> userRepo,
  ILogger<GmailEmailsClassified> logger)
  : EndpointWithoutRequest<GmailEmailsClassifiedResponse>
{
  public override void Configure()
  {
    Get(GmailEmailsClassifiedRequest.Route);
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    // 1. Leer JWT de la cookie
    var jwt = HttpContext.Request.Cookies["jwt"];
    if (string.IsNullOrEmpty(jwt))
    {
      AddError("Usuario no autenticado");
      return;
    }

    // 2. Validar JWT y extraer GoogleUserId
    var handler = new JwtSecurityTokenHandler();
    var token = handler.ReadJwtToken(jwt);
    var googleUserId = token.Claims.FirstOrDefault(c => c.Type == "GoogleUserId")?.Value;

    if (string.IsNullOrEmpty(googleUserId))
    {
      AddError("JWT inválido");
      return;
    }

    // 3. Buscar usuario en DB
    var user = await userRepo.FirstOrDefaultAsync(new UserByGoogleIdSpec(googleUserId), ct);
    if (user is null)
    {
      AddError("Usuario no encontrado");
      return;
    }

    logger.LogInformation("Fetching and classifying recent emails for user {Email}", user.Email);

    try
    {
      var emailsResult = await gmailService.GetRecentEmailsAsync(
        user.AccessToken.Value,
        user.RefreshToken.Value,
        30,
        ct
      );

      if (!emailsResult.Success || emailsResult.Emails.Count == 0)
      {
        Response = new GmailEmailsClassifiedResponse(false, "No se encontraron correos recientes", new List<EmailClassification>());
        return;
      }

      var labelsResult = await gmailService.GetLabelsAsync(user.AccessToken.Value, user.RefreshToken.Value, ct);
      if (!labelsResult.Success || labelsResult.UserLabels.Count == 0)
      {
        Response = new GmailEmailsClassifiedResponse(false, "No se pudieron recuperar etiquetas del usuario", new List<EmailClassification>());
        return;
      }

      var coreClassifiedEmails = await classificationService.ClassifyEmailsAsync(emailsResult.Emails, labelsResult.UserLabels, ct);
      var webClassifiedEmails = coreClassifiedEmails.Select(core =>
      {
        var labelToApply = labelsResult.UserLabels.FirstOrDefault(l =>
          l.Name.Equals(core.SuggestedLabels.FirstOrDefault(), StringComparison.OrdinalIgnoreCase));
        if (labelToApply != null)
        {
          gmailService.ApplyLabelAsync(user.AccessToken.Value, user.RefreshToken.Value, core.EmailId, labelToApply.Id, ct).Wait();
        }

        return new EmailClassification(core.EmailId, core.SuggestedLabels);
      }).ToList();

      Response = new GmailEmailsClassifiedResponse(true, "Correos clasificados y primera etiqueta aplicada exitosamente", webClassifiedEmails);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error clasificando correos con AI");
      AddError($"Error clasificando correos: {ex.Message}");
    }
  }
}
