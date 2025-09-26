namespace GmailOrganizer.Core.UserAggregate.Entities;
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
public class GmailEmail
{
  public string Id { get; set; } = string.Empty;
  public string ThreadId { get; set; } = string.Empty;
  public string Subject { get; set; } = string.Empty;
  public string From { get; set; } = string.Empty;
  public string To { get; set; } = string.Empty;
  public string Cc { get; set; } = string.Empty;
  public string Bcc { get; set; } = string.Empty;
  public DateTime Date { get; set; }
  public string Snippet { get; set; } = string.Empty;
  public List<string> LabelIds { get; set; } = new();
  public string Body { get; set; } = string.Empty;
  public string BodyHtml { get; set; } = string.Empty;
  public bool IsRead { get; set; }
  public bool IsStarred { get; set; }
  public bool IsImportant { get; set; }
  public long SizeEstimate { get; set; }
  public List<GmailAttachment> Attachments { get; set; } = new();
}

public class GmailAttachment
{
  public string PartId { get; set; } = string.Empty;
  public string Filename { get; set; } = string.Empty;
  public string MimeType { get; set; } = string.Empty;
  public long Size { get; set; }
  public string AttachmentId { get; set; } = string.Empty;
}

