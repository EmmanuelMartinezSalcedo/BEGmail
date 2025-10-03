using GmailOrganizer.Core.Interfaces;
using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Specifications;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.UseCases.Gmail.ClassifyUserEmails;

public class ClassifyUserEmailsHandler : ICommandHandler<ClassifyUserEmailsCommand, Result>
{
  private readonly IRepository<User> _userRepository;
  private readonly IGmailService _gmailService;
  private readonly IGeminiService _classificationService;
  private readonly ILogger<ClassifyUserEmailsHandler> _logger;

  public ClassifyUserEmailsHandler(
      IRepository<User> userRepository,
      IGmailService gmailService,
      IGeminiService classificationService,
      ILogger<ClassifyUserEmailsHandler> logger)
  {
    _userRepository = userRepository;
    _gmailService = gmailService;
    _classificationService = classificationService;
    _logger = logger;
  }

  public async Task<Result> Handle(ClassifyUserEmailsCommand request, CancellationToken ct)
  {
    try
    {
      var user = await _userRepository.FirstOrDefaultAsync(
          new UserWithLabelStatsSpec(request.User.Id), ct);

      if (user == null)
        return Result.NotFound();

      _logger.LogInformation("Fetching and classifying emails for {Email}", user.Email);

      var emailsResult = await _gmailService.GetRecentEmailsAsync(
          user.AccessToken.Value,
          user.RefreshToken.Value,
          1,
          ct
      );

      if (!emailsResult.Success || emailsResult.Emails.Count == 0)
        return Result.Success();

      var labelsResult = await _gmailService.GetLabelsAsync(
          user.AccessToken.Value,
          user.RefreshToken.Value,
          ct
      );

      if (!labelsResult.Success || labelsResult.UserLabels.Count == 0)
        return Result.Success();

      foreach (var userLabel in labelsResult.UserLabels)
      {
        user.AddOrGetLabelStat(userLabel.Name);
      }

      var classifiedEmails = await _classificationService.ClassifyEmailsAsync(
          emailsResult.Emails,
          labelsResult.UserLabels,
          ct
      );

      foreach (var core in classifiedEmails)
      {
        var suggestedLabel = core.SuggestedLabels.FirstOrDefault();
        if (string.IsNullOrEmpty(suggestedLabel))
          continue;

        var labelToApply = labelsResult.UserLabels.FirstOrDefault(l =>
            l.Name.Equals(suggestedLabel, StringComparison.OrdinalIgnoreCase));

        if (labelToApply != null)
        {
          await _gmailService.ApplyLabelAsync(
              user.AccessToken.Value,
              user.RefreshToken.Value,
              core.EmailId,
              labelToApply.Id,
              ct
          );

          var labelStat = user.AddOrGetLabelStat(labelToApply.Name);
          labelStat.IncrementEmailCount();
          user.AddEmailProcessingLog(labelToApply.Name);
        }
      }


      await _userRepository.UpdateAsync(user, ct);

      return Result.Success();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error clasificando correos para usuario {Email}", request.User.Email);
      return Result.Error(ex.Message);
    }
  }
}
