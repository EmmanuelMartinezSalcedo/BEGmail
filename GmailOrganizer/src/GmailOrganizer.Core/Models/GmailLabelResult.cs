namespace GmailOrganizer.Core.Models;
public class GmailLabelsResult
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
  public List<GmailLabel> SystemLabels { get; set; } = new();
  public List<GmailLabel> UserLabels { get; set; } = new();
  public List<GmailLabel> AllLabels { get; set; } = new();
  public int TotalCount { get; set; }
}
