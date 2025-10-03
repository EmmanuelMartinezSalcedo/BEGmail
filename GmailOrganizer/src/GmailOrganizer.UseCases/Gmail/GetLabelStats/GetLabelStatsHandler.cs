
using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Entities;
using GmailOrganizer.Core.UserAggregate.Specifications;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.UseCases.Gmail.GetLabelStats;
public class GetUserLabelStatsHandler
  : ICommandHandler<GetUserLabelStatsCommand, Result<List<LabelStat>>>
{
  private readonly IRepository<User> _userRepository;
  private readonly ILogger<GetUserLabelStatsHandler> _logger;

  public GetUserLabelStatsHandler(
      IRepository<User> userRepository,
      ILogger<GetUserLabelStatsHandler> logger)
  {
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<Result<List<LabelStat>>> Handle(
      GetUserLabelStatsCommand request,
      CancellationToken ct)
  {
    var user = await _userRepository.FirstOrDefaultAsync(
        new UserWithLabelStatsSpec(request.Id), ct);

    if (user == null)
      return Result.NotFound("Usuario no encontrado");

    var labelStats = user.LabelStats ?? new List<LabelStat>();
    return Result.Success(labelStats);
  }
}
