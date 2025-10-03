namespace GmailOrganizer.Core.Models;
public class GmailAttachment
{
  public string PartId { get; set; } = string.Empty;
  public string Filename { get; set; } = string.Empty;
  public string MimeType { get; set; } = string.Empty;
  public long Size { get; set; }
  public string AttachmentId { get; set; } = string.Empty;
}
