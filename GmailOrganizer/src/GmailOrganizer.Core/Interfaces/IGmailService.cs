using GmailOrganizer.Core.Models;

public interface IGmailService
{
  Task<GmailLabelsResult> GetLabelsAsync(string accessToken, string? refreshToken, CancellationToken ct);
  Task<GmailEmailsResult> GetRecentEmailsAsync(string accessToken, string? refreshToken, int minutesBack, CancellationToken cancellationToken = default);
  Task<bool> ApplyLabelAsync(string accessToken, string? refreshToken, string emailId, string labelId, CancellationToken ct);
}
