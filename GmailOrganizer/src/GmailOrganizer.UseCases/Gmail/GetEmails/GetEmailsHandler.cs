using GmailOrganizer.Core.UserAggregate.Entities;
using GmailOrganizer.Core.Services;

namespace GmailOrganizer.UseCases.Gmail.GetRecentEmails;

public class GetEmailsHandler(IGmailService _gmailService)
    : IQueryHandler<GetEmailsQuery, Result<List<GmailEmail>>>
{
  public async Task<Result<List<GmailEmail>>> Handle(GetEmailsQuery request, CancellationToken ct)
  {
    try
    {
      var result = await _gmailService.GetRecentEmailsAsync(
          request.AccessToken,
          request.RefreshToken,
          request.MinutesBack,
          ct
      );

      return result.Success
          ? Result.Success(result.Emails)
          : Result.Error(result.Message);
    }
    catch (Exception ex)
    {
      return Result.Error($"Error fetching recent emails: {ex.Message}");
    }
  }
}
