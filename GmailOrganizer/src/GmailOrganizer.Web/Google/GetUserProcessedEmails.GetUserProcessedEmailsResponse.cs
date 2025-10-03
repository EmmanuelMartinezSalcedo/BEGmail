namespace GmailOrganizer.Web.Google;

public record EmailProcessingLogDto(
    DateTime ProcessedAt,
    string LabelAssigned
);

public record GetUserProcessedEmailsResponse(
    List<EmailProcessingLogDto> Emails,
    List<string>? Errors = null
);
