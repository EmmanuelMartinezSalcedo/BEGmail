using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Specifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class GmailClassificationBackgroundService : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<GmailClassificationBackgroundService> _logger;

  public GmailClassificationBackgroundService(
      IServiceProvider serviceProvider,
      ILogger<GmailClassificationBackgroundService> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Gmail classification background service started");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        using var scope = _serviceProvider.CreateScope();

        var userRepo = scope.ServiceProvider.GetRequiredService<IRepository<User>>();
        var classifier = scope.ServiceProvider.GetRequiredService<IUserEmailClassifier>();

        var users = await userRepo.ListAsync(new AllUsersSpec(), stoppingToken);

        foreach (var user in users)
        {
          await classifier.ClassifyAndLabelAsync(user, stoppingToken);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error en ciclo principal del background service");
      }

      // Esperar 11 minutos
      await Task.Delay(TimeSpan.FromMinutes(11), stoppingToken);
    }
  }
}
