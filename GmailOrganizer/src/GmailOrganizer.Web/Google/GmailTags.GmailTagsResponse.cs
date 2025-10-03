namespace GmailOrganizer.Web.Google;

public record LabelDto(string Id, string Name, string Type);

public record GmailTagsResponse(
  bool Success,
  string Message,
  List<LabelDto> SystemLabels,
  List<LabelDto> UserLabels,
  List<LabelDto> AllLabels,
  int TotalCount
);
