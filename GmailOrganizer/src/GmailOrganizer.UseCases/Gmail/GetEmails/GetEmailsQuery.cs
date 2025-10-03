using GmailOrganizer.Core.Models;

namespace GmailOrganizer.UseCases.Gmail.GetRecentEmails;

public record GetEmailsQuery(
    string AccessToken,
    string? RefreshToken,
    int MinutesBack = 5
) : IQuery<Result<List<GmailEmail>>>;
