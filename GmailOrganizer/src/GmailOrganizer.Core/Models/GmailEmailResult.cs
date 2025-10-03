namespace GmailOrganizer.Core.Models;
public class GmailEmailsResult
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
  public List<GmailEmail> Emails { get; set; } = new();
  public int TotalCount { get; set; }
  public DateTime SearchFrom { get; set; }
  public DateTime SearchTo { get; set; }
  public string? NextPageToken { get; set; }
  public int ProcessedCount { get; set; }
  public int SkippedCount { get; set; }
}
