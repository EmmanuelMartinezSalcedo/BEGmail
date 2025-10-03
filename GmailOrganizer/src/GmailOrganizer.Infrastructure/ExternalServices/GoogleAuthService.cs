using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using GmailOrganizer.Core.Models;
using GmailOrganizer.Core.Interfaces;
using Google.Apis.Services;
using GmailApiService = Google.Apis.Gmail.v1.GmailService;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.Infrastructure.ExternalServices;

public class GoogleAuthService : IGoogleAuthService
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<GoogleAuthService> _logger;
  private readonly string _clientId;
  private readonly string _clientSecret;
  private readonly string _redirectUri;
  private readonly string[] _scopes;

  public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
  {
    _configuration = configuration;
    _logger = logger;

    _clientId = _configuration["GoogleAuth:ClientId"] ?? throw new ArgumentNullException("GoogleAuth:ClientId");
    _clientSecret = _configuration["GoogleAuth:ClientSecret"] ?? throw new ArgumentNullException("GoogleAuth:ClientSecret");
    _redirectUri = _configuration["GoogleAuth:RedirectUri"] ?? throw new ArgumentNullException("GoogleAuth:RedirectUri");
    _scopes = _configuration.GetSection("GoogleAuth:Scopes").Get<string[]>() ?? throw new ArgumentNullException("GoogleAuth:Scopes");
  }

  public Task<string> GenerateAuthUrlAsync(string state)
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
    _logger.LogInformation("Generated Google OAuth URL with state: {State}", state);

    return Task.FromResult(authUrl);
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

      var tokenResponse = await flow.ExchangeCodeForTokenAsync("user", code, _redirectUri, CancellationToken.None);

      _logger.LogInformation("AccessToken: {AccessToken}", tokenResponse.AccessToken);
      _logger.LogInformation("RefreshToken: {RefreshToken}", tokenResponse.RefreshToken);
      _logger.LogInformation("ExpiresIn: {ExpiresIn}", tokenResponse.ExpiresInSeconds);

      var credential = new UserCredential(flow, "user", tokenResponse);

      var service = new GmailApiService(new BaseClientService.Initializer
      {
        HttpClientInitializer = credential,
        ApplicationName = "Gmail Organizer"
      });

      var profile = await service.Users.GetProfile("me").ExecuteAsync();

      return new AuthResult
      {
        Success = true,
        Message = "Authentication successful",
        AccessToken = tokenResponse.AccessToken,
        RefreshToken = tokenResponse.RefreshToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600),
        GoogleUserId = profile.EmailAddress,
        Email = profile.EmailAddress
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling Google OAuth callback");
      return new AuthResult
      {
        Success = false,
        Message = $"OAuth failed: {ex.Message}"
      };
    }
  }
}
