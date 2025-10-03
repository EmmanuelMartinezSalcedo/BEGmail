using GmailOrganizer.Core.UserAggregate.Entities;

namespace GmailOrganizer.Web.Google;

public record LabelStatDto(string LabelName, int EmailCount);

public record GetUserLabelStatsResponse(
    List<LabelStat> LabelStats,
    List<string>? Errors = null
);
