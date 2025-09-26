using GmailOrganizer.Core.UserAggregate.Entities;
using GmailOrganizer.Core.Services;

namespace GmailOrganizer.UseCases.Gmail.GetLabels;

public class GetLabelsHandler(IGmailService _gmailService)
  : IQueryHandler<GetLabelsQuery, Result<List<GmailLabel>>>
{
  public async Task<Result<List<GmailLabel>>> Handle(GetLabelsQuery request, CancellationToken ct)
  {
    try
    {
      var result = await _gmailService.GetLabelsAsync(
        request.AccessToken,
        request.RefreshToken,
        ct
      );

      return result.Success
        ? Result.Success(result.AllLabels)
        : Result.Error(result.Message);
    }
    catch (Exception ex)
    {
      return Result.Error(ex.Message);
    }
  }
}
