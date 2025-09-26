using GmailOrganizer.Core.UserAggregate.Entities;

namespace GmailOrganizer.UseCases.Gmail.GetLabels;
public record GetLabelsQuery(string AccessToken, string? RefreshToken)
  : IQuery<Result<List<GmailLabel>>>;
