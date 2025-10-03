using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using GmailApiService = Google.Apis.Gmail.v1.GmailService;
using GmailMessagePart = Google.Apis.Gmail.v1.Data.MessagePart;
using GmailOrganizer.Core.Models;

namespace GmailOrganizer.Infrastructure.ExternalServices;

public class GmailService : IGmailService
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<GmailService> _logger;
  private readonly string _clientId;
  private readonly string _clientSecret;
  private readonly string[] _scopes;

  public GmailService(IConfiguration configuration, ILogger<GmailService> logger)
  {
    _configuration = configuration;
    _logger = logger;

    _clientId = _configuration["GoogleAuth:ClientId"] ?? throw new ArgumentNullException("GoogleAuth:ClientId");
    _clientSecret = _configuration["GoogleAuth:ClientSecret"] ?? throw new ArgumentNullException("GoogleAuth:ClientSecret");
    _scopes = _configuration.GetSection("GoogleAuth:Scopes").Get<string[]>() ??
             throw new ArgumentNullException("GoogleAuth:Scopes");
  }

  public async Task<GmailLabelsResult> GetLabelsAsync(string accessToken, string? refreshToken, CancellationToken ct)
  {
    try
    {
      var service = CreateGmailService(accessToken, refreshToken);

      var request = service.Users.Labels.List("me");
      var response = await request.ExecuteAsync(ct);

      var userLabels = response.Labels?
        .Select(MapToGmailLabel)
        .Where(l => l.Type == "user")
        .ToList() ?? new List<GmailLabel>();

      _logger.LogInformation("Retrieved {Count} user labels from Gmail", userLabels.Count);

      return new GmailLabelsResult
      {
        Success = true,
        Message = "User labels retrieved successfully",
        UserLabels = userLabels,
        TotalCount = userLabels.Count
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving Gmail user labels");
      return new GmailLabelsResult
      {
        Success = false,
        Message = $"Failed to retrieve user labels: {ex.Message}"
      };
    }
  }

  public async Task<GmailEmailsResult> GetRecentEmailsAsync(
    string accessToken,
    string? refreshToken,
    int minutesBack,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var service = CreateGmailService(accessToken, refreshToken);

      var searchFrom = DateTime.UtcNow.AddMinutes(-minutesBack);
      var searchFromTimestamp = ((DateTimeOffset)searchFrom).ToUnixTimeSeconds();
      var query = $"after:{searchFromTimestamp}";

      var request = service.Users.Messages.List("me");
      request.Q = query;
      request.MaxResults = 50;

      var messages = await request.ExecuteAsync(cancellationToken);
      var emails = new List<GmailEmail>();

      if (messages.Messages != null)
      {
        var semaphore = new SemaphoreSlim(5, 5);
        var tasks = messages.Messages.Take(20).Select(async message =>
        {
          await semaphore.WaitAsync(cancellationToken);
          try
          {
            var messageRequest = service.Users.Messages.Get("me", message.Id);
            messageRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
            var fullMessage = await messageRequest.ExecuteAsync(cancellationToken);
            return MapToGmailEmail(fullMessage);
          }
          finally
          {
            semaphore.Release();
          }
        });

        emails = (await Task.WhenAll(tasks))
                .Where(email => email != null)
                .Select(email => email!)
                .ToList();
      }

      _logger.LogInformation("Retrieved {Count} recent emails from Gmail", emails.Count);

      return new GmailEmailsResult
      {
        Success = true,
        Message = $"Found {emails.Count} emails from the last {minutesBack} minutes",
        Emails = emails,
        TotalCount = emails.Count,
        SearchFrom = searchFrom,
        SearchTo = DateTime.UtcNow
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving recent Gmail emails");
      return new GmailEmailsResult
      {
        Success = false,
        Message = $"Failed to retrieve recent emails: {ex.Message}",
        Emails = new List<GmailEmail>(),
        TotalCount = 0,
        SearchFrom = DateTime.UtcNow.AddMinutes(-minutesBack),
        SearchTo = DateTime.UtcNow
      };
    }
  }

  public async Task<bool> ApplyLabelAsync(
    string accessToken,
    string? refreshToken,
    string emailId,
    string labelId,
    CancellationToken ct)
  {
    try
    {
      var service = CreateGmailService(accessToken, refreshToken);

      var modifyRequest = new ModifyMessageRequest
      {
        AddLabelIds = new List<string> { labelId }
      };

      await service.Users.Messages.Modify(modifyRequest, "me", emailId).ExecuteAsync(ct);

      _logger.LogInformation("Applied label {LabelId} to email {EmailId}", labelId, emailId);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to apply label {LabelId} to email {EmailId}", labelId, emailId);
      return false;
    }
  }

  private GmailApiService CreateGmailService(string accessToken, string? refreshToken)
  {
    var tokenResponse = new TokenResponse
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken
    };

    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
    {
      ClientSecrets = new ClientSecrets
      {
        ClientId = _clientId,
        ClientSecret = _clientSecret
      },
      Scopes = _scopes
    });

    var credential = new UserCredential(flow, "user", tokenResponse);
    return new GmailApiService(new BaseClientService.Initializer()
    {
      HttpClientInitializer = credential,
      ApplicationName = "Gmail Organizer"
    });
  }

  private static GmailLabel MapToGmailLabel(Label label)
  {
    return new GmailLabel
    {
      Id = label.Id,
      Name = label.Name,
      Type = label.Type?.ToLowerInvariant() ?? "unknown",
      MessagesTotal = (int)(label.MessagesTotal ?? 0),
      MessagesUnread = (int)(label.MessagesUnread ?? 0),
      ThreadsTotal = (int)(label.ThreadsTotal ?? 0),
      ThreadsUnread = (int)(label.ThreadsUnread ?? 0),
      Color = label.Color != null ? new Core.Models.LabelColor
      {
        TextColor = label.Color.TextColor,
        BackgroundColor = label.Color.BackgroundColor
      } : null
    };
  }

  private GmailEmail? MapToGmailEmail(Message message)
  {
    try
    {
      var headers = message.Payload?.Headers ?? new List<MessagePartHeader>();

      return new GmailEmail
      {
        Id = message.Id,
        ThreadId = message.ThreadId,
        Subject = GetHeaderValue(headers, "Subject") ?? "Sin asunto",
        From = GetHeaderValue(headers, "From") ?? "Desconocido",
        To = GetHeaderValue(headers, "To") ?? "",
        Date = ParseEmailDate(GetHeaderValue(headers, "Date")),
        Snippet = message.Snippet ?? "",
        LabelIds = message.LabelIds?.ToList() ?? new List<string>(),
        Body = ExtractEmailBody(message.Payload),
        IsRead = !message.LabelIds?.Contains("UNREAD") ?? true
      };
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error mapping email {MessageId} to GmailEmail", message.Id);
      return null;
    }
  }

  private static string? GetHeaderValue(IList<MessagePartHeader> headers, string name)
  {
    return headers.FirstOrDefault(h =>
        string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase))?.Value;
  }

  private static DateTime ParseEmailDate(string? dateString)
  {
    if (string.IsNullOrEmpty(dateString))
      return DateTime.UtcNow;

    try
    {
      return DateTime.Parse(dateString).ToUniversalTime();
    }
    catch
    {
      return DateTime.UtcNow;
    }
  }

  private static string ExtractEmailBody(GmailMessagePart? payload)
  {
    if (payload == null) return string.Empty;

    var textPart = FindPartByMimeType(payload, "text/plain");
    if (textPart?.Body?.Data != null)
    {
      return DecodeBase64(textPart.Body.Data);
    }

    var htmlPart = FindPartByMimeType(payload, "text/html");
    if (htmlPart?.Body?.Data != null)
    {
      return DecodeBase64(htmlPart.Body.Data);
    }

    return string.Empty;
  }

  private static GmailMessagePart? FindPartByMimeType(GmailMessagePart part, string mimeType)
  {
    if (string.Equals(part.MimeType, mimeType, StringComparison.OrdinalIgnoreCase))
      return part;

    if (part.Parts != null)
    {
      foreach (var subPart in part.Parts)
      {
        var found = FindPartByMimeType(subPart, mimeType);
        if (found != null) return found;
      }
    }

    return null;
  }

  private static string DecodeBase64(string base64String)
  {
    try
    {
      var data = Convert.FromBase64String(base64String.Replace('-', '+').Replace('_', '/'));
      return System.Text.Encoding.UTF8.GetString(data);
    }
    catch
    {
      return string.Empty;
    }
  }
}
