namespace GmailOrganizer.Core.Models;
public class GmailLabel
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public int MessagesTotal { get; set; }
  public int MessagesUnread { get; set; }
  public int ThreadsTotal { get; set; }
  public int ThreadsUnread { get; set; }
  public LabelColor? Color { get; set; }
}
