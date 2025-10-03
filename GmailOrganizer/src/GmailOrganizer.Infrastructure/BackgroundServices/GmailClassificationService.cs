using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Specifications;
using GmailOrganizer.UseCases.Gmail.ClassifyUserEmails;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace GmailOrganizer.Infrastructure.BackgroundServices;

public class GmailClassificationBackgroundService : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<GmailClassificationBackgroundService> _logger;

  private bool _enabled = false; // Flag de control

  public GmailClassificationBackgroundService(
      IServiceProvider serviceProvider,
      ILogger<GmailClassificationBackgroundService> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public void Enable() => _enabled = true;
  public void Disable() => _enabled = false;
  public bool IsEnabled() => _enabled;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Gmail classification background service started");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        if (_enabled)
        {
          using var scope = _serviceProvider.CreateScope();

          var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<User>>();
          var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

          var users = await userRepo.ListAsync(new AllUsersSpec(), stoppingToken);

          foreach (var user in users)
          {
            var result = await mediator.Send(new ClassifyUserEmailsCommand(user), stoppingToken);

            if (!result.IsSuccess)
            {
              _logger.LogWarning("Failed to classify emails for {Email}: {Errors}",
                user.Email, string.Join("; ", result.Errors));
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en ciclo principal del background service");
      }

      await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
  }
}
