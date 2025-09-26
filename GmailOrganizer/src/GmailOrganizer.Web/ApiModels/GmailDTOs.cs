namespace GmailOrganizer.Web.ApiModels;

public record GoogleAuthUrlResponse(
    bool Success,
    string AuthUrl,
    string State,
    string Message = ""
);

public record GoogleAuthCallbackRequest(
    string Code,
    string State
);

public record GoogleAuthResponse(
    bool Success,
    string Message,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    UserDto? User = null
);

public record UserDto(
    int Id,
    string Email,
    string GoogleUserId,
    DateTime CreatedAt
);

public record GmailLabelsRequest(
    string? AccessToken = null,
    string? RefreshToken = null
);

public record GmailLabelsResponse(
    bool Success,
    string Message,
    List<GmailLabelDto> SystemLabels,
    List<GmailLabelDto> UserLabels,
    List<GmailLabelDto> AllLabels,
    int TotalCount
);

public record GmailLabelDto(
    string Id,
    string Name,
    string Type,
    int MessagesTotal,
    int MessagesUnread,
    int ThreadsTotal,
    int ThreadsUnread,
    LabelColorDto? Color
);

public record LabelColorDto(
    string? TextColor,
    string? BackgroundColor
);
