namespace GmailOrganizer.Core.UserAggregate.Entities;
public class GmailLabel { 
  public string Id { get; set; } = string.Empty; 
  public string Name { get; set; } = string.Empty; 
  public string Type { get; set; } = string.Empty; 
  public int MessagesTotal { get; set; } 
  public int MessagesUnread { get; set; } 
  public int ThreadsTotal { get; set; } 
  public int ThreadsUnread { get; set; } 
  public LabelColor? Color { get; set; } 
}
public class GmailLabelsResult { 
  public bool Success { get; set; } 
  public string Message { get; set; } = string.Empty; 
  public List<GmailLabel> SystemLabels { get; set; } = new(); 
  public List<GmailLabel> UserLabels { get; set; } = new(); 
  public List<GmailLabel> AllLabels { get; set; } = new(); 
  public int TotalCount { get; set; } 
}

public class LabelColor { 
  public string? TextColor { get; set; } 
  public string? BackgroundColor { get; set; } 
}
