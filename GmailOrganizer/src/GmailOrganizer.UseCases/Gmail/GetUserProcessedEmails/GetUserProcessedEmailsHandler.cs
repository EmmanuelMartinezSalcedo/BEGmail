using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Entities;
using GmailOrganizer.Core.UserAggregate.Specifications;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.UseCases.Gmail.GetUserProcessedEmails;

public class GetUserProcessedEmailsHandler
  : ICommandHandler<GetUserProcessedEmailsCommand, Result<List<EmailProcessingLog>>>
{
  private readonly IRepository<User> _userRepository;
  private readonly ILogger<GetUserProcessedEmailsHandler> _logger;

  public GetUserProcessedEmailsHandler(
      IRepository<User> userRepository,
      ILogger<GetUserProcessedEmailsHandler> logger)
  {
    _userRepository = userRepository;
    _logger = logger;
  }

  public async Task<Result<List<EmailProcessingLog>>> Handle(
      GetUserProcessedEmailsCommand request,
      CancellationToken ct)
  {
    var user = await _userRepository.FirstOrDefaultAsync(
        new UserWithEmailProcessingLogsSpec(request.UserId), ct);

    if (user == null)
      return Result.NotFound("Usuario no encontrado");

    var twelveHoursAgo = DateTime.UtcNow.AddHours(-12);

    var recentLogs = user.EmailProcessingLogs?
        .Where(log => log.ProcessedAt >= twelveHoursAgo)
        .OrderByDescending(log => log.ProcessedAt)
        .ToList() ?? new List<EmailProcessingLog>();

    return Result.Success(recentLogs);
  }
}
