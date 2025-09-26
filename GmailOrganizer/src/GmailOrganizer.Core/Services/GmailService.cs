using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Entities;
using GmailOrganizer.Core.UserAggregate.Specifications;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using GmailApiService = Google.Apis.Gmail.v1.GmailService;

namespace GmailOrganizer.Core.Services;

public interface IGmailService
{
  Task<string> GenerateAuthUrlAsync(string state);
  Task<AuthResult> HandleAuthCallbackAsync(string code, string state);
  Task<GmailLabelsResult> GetLabelsAsync(string accessToken, string? refreshToken, CancellationToken ct);
  Task<bool> RefreshTokenAsync(User user);
  Task<GmailEmailsResult> GetRecentEmailsAsync(
    string accessToken,
    string? refreshToken,
    int minutesBack,
    CancellationToken cancellationToken = default);

  Task<bool> ApplyLabelAsync(
    string accessToken,
    string? refreshToken,
    string emailId,
    string labelId,
    CancellationToken ct);
  
  }
public class GmailService : IGmailService
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<GmailService> _logger;
  private readonly IRepository<User> _userRepository;
  private readonly string _clientId;
  private readonly string _clientSecret;
  private readonly string _redirectUri;
  private readonly string[] _scopes;

  public GmailService(IConfiguration configuration, ILogger<GmailService> logger, IRepository<User> userRepository)
  {
    _configuration = configuration;
    _logger = logger;
    _userRepository = userRepository;
    _clientId = _configuration["GoogleAuth:ClientId"] ?? throw new ArgumentNullException("GoogleAuth:ClientId");
    _clientSecret = _configuration["GoogleAuth:ClientSecret"] ?? throw new ArgumentNullException("GoogleAuth:ClientSecret");
    _redirectUri = _configuration["GoogleAuth:RedirectUri"] ?? throw new ArgumentNullException("GoogleAuth:RedirectUri");
    _scopes = _configuration.GetSection("GoogleAuth:Scopes").Get<string[]>() ??
             throw new ArgumentNullException("GoogleAuth:Scopes");
  }

  public Task<string> GenerateAuthUrlAsync(string state)
  {
    try
    {
      var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
      {
        ClientSecrets = new ClientSecrets
        {
          ClientId = _clientId,
          ClientSecret = _clientSecret
        },
        Scopes = _scopes
      });

      var request = flow.CreateAuthorizationCodeRequest(_redirectUri);
      request.State = state;

      var authUrl = request.Build().ToString();

      _logger.LogInformation("Generated auth URL for state: {State}", state);
      return Task.FromResult(authUrl);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating auth URL");
      throw;
    }
  }

  public async Task<AuthResult> HandleAuthCallbackAsync(string code, string state)
  {
    try
    {
      var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
      {
        ClientSecrets = new ClientSecrets
        {
          ClientId = _clientId,
          ClientSecret = _clientSecret
        },
        Scopes = _scopes
      });

      var tokenResponse = await flow.ExchangeCodeForTokenAsync(
          "user", code, _redirectUri, CancellationToken.None);

      _logger.LogInformation("AccessToken: {AccessToken}", tokenResponse.AccessToken);
      _logger.LogInformation("RefreshToken: {RefreshToken}", tokenResponse.RefreshToken);
      _logger.LogInformation("ExpiresIn: {ExpiresIn}", tokenResponse.ExpiresInSeconds);

      var credential = new UserCredential(flow, "user", tokenResponse);
      var service = new GmailApiService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = "Gmail Organizer"
      });

      var profile = await service.Users.GetProfile("me").ExecuteAsync();

      var userSpec = new UserByGoogleIdSpec(profile.EmailAddress);
      var existingUser = await _userRepository.FirstOrDefaultAsync(userSpec);

      var refreshToken = tokenResponse.RefreshToken ?? existingUser?.RefreshToken?.Value ?? string.Empty;

      var result = new AuthResult
      {
        Success = true,
        Message = "Authentication successful",
        AccessToken = tokenResponse.AccessToken,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600),
        User = new User(
              googleUserId: profile.EmailAddress,
              email: profile.EmailAddress,
              accessToken: tokenResponse.AccessToken,
              refreshToken: refreshToken,
              tokenExpiry: DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600)
          )
      };

      _logger.LogInformation("Successfully authenticated user: {Email}", profile.EmailAddress);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling auth callback");
      return new AuthResult
      {
        Success = false,
        Message = $"Authentication failed: {ex.Message}"
      };
    }
  }

  public async Task<GmailLabelsResult> GetLabelsAsync(string accessToken, string? refreshToken, CancellationToken ct)
  {
    try
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

      var service = new GmailApiService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = "Gmail Organizer"
      });

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


  public async Task<bool> RefreshTokenAsync(User user)
  {
    try
    {
      var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
      {
        ClientSecrets = new ClientSecrets
        {
          ClientId = _clientId,
          ClientSecret = _clientSecret
        },
        Scopes = _scopes
      });

      var tokenResponse = new TokenResponse
      {
        RefreshToken = user.RefreshToken.Value
      };

      var newToken = await flow.RefreshTokenAsync("user", tokenResponse.RefreshToken, CancellationToken.None);

      user.UpdateAccessToken(
        newToken.AccessToken,
        DateTime.UtcNow.AddSeconds(newToken.ExpiresInSeconds ?? 3600)
      );

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error refreshing token for user {UserId}", user.Id);
      return false;
    }
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
      Color = label.Color != null ? new UserAggregate.Entities.LabelColor
      {
        TextColor = label.Color.TextColor,
        BackgroundColor = label.Color.BackgroundColor
      } : null
    };
  }

  public async Task<GmailEmailsResult> GetRecentEmailsAsync(
    string accessToken,
    string? refreshToken,
    int minutesBack,
    CancellationToken cancellationToken = default)
  {
    try
    {
      // Usar el mismo patrón que GetLabelsAsync
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
      var service = new GmailApiService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = "Gmail Organizer"
      });

      // Calcular timestamp para los últimos X minutos
      var searchFrom = DateTime.UtcNow.AddMinutes(-minutesBack);
      var searchFromTimestamp = ((DateTimeOffset)searchFrom).ToUnixTimeSeconds();

      // Construir query para buscar emails recientes
      var query = $"after:{searchFromTimestamp}";

      var request = service.Users.Messages.List("me");
      request.Q = query;
      request.MaxResults = 50; // Limitar resultados para evitar rate limits

      var messages = await request.ExecuteAsync(cancellationToken);
      var emails = new List<GmailEmail>();

      if (messages.Messages != null)
      {
        // Procesar cada mensaje (limitado por rate limits)
        var semaphore = new SemaphoreSlim(5, 5); // Máximo 5 requests concurrentes
        var tasks = messages.Messages.Take(20).Select(async message => // Máximo 20 emails
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

  private static string ExtractEmailBody(MessagePart? payload)
  {
    if (payload == null) return string.Empty;

    // Buscar parte de texto plano primero
    var textPart = FindPartByMimeType(payload, "text/plain");
    if (textPart?.Body?.Data != null)
    {
      return DecodeBase64(textPart.Body.Data);
    }

    // Si no hay texto plano, buscar HTML
    var htmlPart = FindPartByMimeType(payload, "text/html");
    if (htmlPart?.Body?.Data != null)
    {
      return DecodeBase64(htmlPart.Body.Data);
    }

    return string.Empty;
  }

  private static MessagePart? FindPartByMimeType(MessagePart part, string mimeType)
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

  public async Task<bool> ApplyLabelAsync(
  string accessToken,
  string? refreshToken,
  string emailId,
  string labelId,
  CancellationToken ct)
  {
    try
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
      var service = new GmailApiService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = "Gmail Organizer"
      });

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
}
