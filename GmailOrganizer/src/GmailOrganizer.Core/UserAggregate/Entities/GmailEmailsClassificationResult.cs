namespace GmailOrganizer.Core.UserAggregate.Entities;
public record GmailEmailsClassifiedResult(
  bool Success,
  string Message,
  List<EmailClassificationResult> ClassifiedEmails
);

public record EmailClassificationResult
{
  public string EmailId { get; init; } = string.Empty;
  public List<string> SuggestedLabels { get; init; } = new();
}
