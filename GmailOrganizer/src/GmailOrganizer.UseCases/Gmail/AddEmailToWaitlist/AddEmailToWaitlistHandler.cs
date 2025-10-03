using GmailOrganizer.Core.WaitlistAggregate;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.UseCases.AddEmailToWaitlist;

public class AddEmailToWaitlistHandler : ICommandHandler<AddEmailToWaitlistCommand, Result>
{
  private readonly IRepository<Waitlist> _waitlistRepository;
  private readonly ILogger<AddEmailToWaitlistHandler> _logger;

  public AddEmailToWaitlistHandler(
    IRepository<Waitlist> waitlistRepository,
    ILogger<AddEmailToWaitlistHandler> logger)
  {
    _waitlistRepository = waitlistRepository;
    _logger = logger;
  }

  public async Task<Result> Handle(AddEmailToWaitlistCommand request, CancellationToken ct)
  {
    try
    {
      var waitlistEntry = new Waitlist(request.Email);

      await _waitlistRepository.AddAsync(waitlistEntry, ct);

      return Result.Success();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error adding email {Email} to waitlist", request.Email);
      return Result.Error($"No se pudo agregar el email a la waitlist: {ex.Message}");
    }
  }
}
