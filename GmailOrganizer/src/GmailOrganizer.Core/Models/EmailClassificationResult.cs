namespace GmailOrganizer.Core.Models;
public record EmailClassificationResult
{
  public string EmailId { get; init; } = string.Empty;
  public List<string> SuggestedLabels { get; init; } = new();
}
