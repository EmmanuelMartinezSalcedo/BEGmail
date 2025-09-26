namespace GmailOrganizer.Web.Google;

public record LabelDto(string Id, string Name, string Type);

public record GmailTagsResponse(
  List<LabelDto> Labels
);
