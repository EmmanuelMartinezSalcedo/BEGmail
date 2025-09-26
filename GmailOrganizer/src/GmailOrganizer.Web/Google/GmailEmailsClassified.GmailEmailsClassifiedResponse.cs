namespace GmailOrganizer.Web.Google;

public record EmailDto(
    string Id,
    string ThreadId,
    string Subject,
    string From,
    string To,
    string? Cc,
    string? Bcc,
    DateTime Date,
    string Snippet,
    List<string> LabelIds,
    string Body,
    string? BodyHtml,
    bool IsRead,
    bool IsStarred,
    bool IsImportant,
    long SizeEstimate,
    List<AttachmentDto> Attachments
);

public record AttachmentDto(
    string PartId,
    string Filename,
    string MimeType,
    long Size,
    string AttachmentId
);

public record GmailEmailsResponse(
    bool Success,
    string Message,
    List<EmailDto> Emails,
    int TotalCount,
    DateTime SearchFrom,
    DateTime SearchTo,
    int ProcessedCount,
    int SkippedCount
);

public record EmailClassification(
    string EmailId,
    List<string> SuggestedLabels
);

public record GmailEmailsClassifiedResponse(
    bool Success,
    string Message,
    List<EmailClassification> ClassifiedEmails
);
