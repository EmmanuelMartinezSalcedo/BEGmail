using GmailOrganizer.Core.Interfaces;
using GmailOrganizer.Core.Services;
using GmailOrganizer.Infrastructure.Data;
using GmailOrganizer.Infrastructure.Data.Queries;
using GmailOrganizer.UseCases.Contributors.List;

namespace GmailOrganizer.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger)
  {
    string? connectionString = config.GetConnectionString("PostgresConnection");
    Guard.Against.Null(connectionString);

    services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql(connectionString));

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
            .AddScoped<IListContributorsQueryService, ListContributorsQueryService>()
            .AddScoped<IDeleteContributorService, DeleteContributorService>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
