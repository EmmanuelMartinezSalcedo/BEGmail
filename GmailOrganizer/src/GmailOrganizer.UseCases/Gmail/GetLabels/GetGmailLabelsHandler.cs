using GmailOrganizer.Core.Models;
using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Specifications;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.UseCases.Gmail.GetLabels;

public class GetGmailLabelsHandler(
  IRepository<User> userRepo,
  IGmailService gmailService,
  ILogger<GetGmailLabelsHandler> logger)
  : ICommandHandler<GetGmailLabelsCommand, Result<GmailLabelsResult>>
{
  public async Task<Result<GmailLabelsResult>> Handle(
    GetGmailLabelsCommand request,
    CancellationToken ct)
  {
    var user = await userRepo.FirstOrDefaultAsync(new UserByGoogleIdSpec(request.GoogleUserId), ct);
    if (user is null)
    {
      return Result.NotFound("Usuario no encontrado");
    }

    try
    {
      var labelsResult = await gmailService.GetLabelsAsync(user.AccessToken.Value, user.RefreshToken.Value, ct);
      return Result.Success(labelsResult);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error fetching Gmail labels for user {GoogleUserId}", request.GoogleUserId);
      return Result.Error($"Error fetching labels: {ex.Message}");
    }
  }
}
