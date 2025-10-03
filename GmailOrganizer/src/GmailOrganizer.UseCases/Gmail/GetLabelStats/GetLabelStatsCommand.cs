
using GmailOrganizer.Core.UserAggregate.Entities;

namespace GmailOrganizer.UseCases.Gmail.GetLabelStats;
public record GetUserLabelStatsCommand(int Id)
  : ICommand<Result<List<LabelStat>>>;
