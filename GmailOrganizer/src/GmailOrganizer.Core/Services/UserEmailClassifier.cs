using GmailOrganizer.Core.Services;
using GmailOrganizer.Core.UserAggregate;

public interface IUserEmailClassifier
{
  Task ClassifyAndLabelAsync(User user, CancellationToken ct);
}

public class UserEmailClassifier : IUserEmailClassifier
{
  private readonly IGmailService _gmailService;
  private readonly IGmailClassificationService _classificationService;
  private readonly ILogger<UserEmailClassifier> _logger;

  public UserEmailClassifier(
      IGmailService gmailService,
      IGmailClassificationService classificationService,
      ILogger<UserEmailClassifier> logger)
  {
    _gmailService = gmailService;
    _classificationService = classificationService;
    _logger = logger;
  }

  public async Task ClassifyAndLabelAsync(User user, CancellationToken ct)
  {
    try
    {
      _logger.LogInformation("Fetching and classifying recent emails for {Email}", user.Email);

      var emailsResult = await _gmailService.GetRecentEmailsAsync(
          user.AccessToken.Value,
          user.RefreshToken.Value,
          10,
          ct
      );

      if (!emailsResult.Success || emailsResult.Emails.Count == 0)
        return;

      var labelsResult = await _gmailService.GetLabelsAsync(user.AccessToken.Value, user.RefreshToken.Value, ct);
      if (!labelsResult.Success || labelsResult.UserLabels.Count == 0)
        return;

      var coreClassifiedEmails = await _classificationService.ClassifyEmailsAsync(
          emailsResult.Emails,
          labelsResult.UserLabels,
          ct
      );

      foreach (var core in coreClassifiedEmails)
      {
        var labelToApply = labelsResult.UserLabels.FirstOrDefault(l =>
            l.Name.Equals(core.SuggestedLabels.FirstOrDefault(), StringComparison.OrdinalIgnoreCase));

        if (labelToApply != null)
        {
          await _gmailService.ApplyLabelAsync(
              user.AccessToken.Value,
              user.RefreshToken.Value,
              core.EmailId,
              labelToApply.Id,
              ct
          );
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error clasificando correos para usuario {Email}", user.Email);
    }
  }
}
