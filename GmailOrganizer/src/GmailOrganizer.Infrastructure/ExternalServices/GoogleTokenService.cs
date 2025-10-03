using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using GmailOrganizer.Core.Interfaces;
using GmailOrganizer.Core.Models;

namespace GmailOrganizer.Infrastructure.ExternalServices;

public class GoogleTokenService : IGoogleTokenService
{
  private readonly IConfiguration _configuration;
  private readonly ILogger<GoogleTokenService> _logger;
  private readonly string _clientId;
  private readonly string _clientSecret;
  private readonly string[] _scopes;

  public GoogleTokenService(IConfiguration configuration, ILogger<GoogleTokenService> logger)
  {
    _configuration = configuration;
    _logger = logger;

    _clientId = _configuration["GoogleAuth:ClientId"]
      ?? throw new ArgumentNullException("GoogleAuth:ClientId");
    _clientSecret = _configuration["GoogleAuth:ClientSecret"]
      ?? throw new ArgumentNullException("GoogleAuth:ClientSecret");
    _scopes = _configuration.GetSection("GoogleAuth:Scopes").Get<string[]>()
      ?? throw new ArgumentNullException("GoogleAuth:Scopes");
  }

  public async Task<TokenResult> RefreshAccessTokenAsync(string refreshToken)
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
        RefreshToken = refreshToken
      };

      var newToken = await flow.RefreshTokenAsync("user", tokenResponse.RefreshToken, CancellationToken.None);

      if (newToken == null || string.IsNullOrEmpty(newToken.AccessToken))
      {
        return new TokenResult
        {
          Success = false,
          Message = "Failed to refresh access token"
        };
      }

      _logger.LogInformation("Successfully refreshed Google access token");

      return new TokenResult
      {
        Success = true,
        AccessToken = newToken.AccessToken,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddSeconds(newToken.ExpiresInSeconds ?? 3600),
        Message = "Token refreshed successfully"
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error refreshing Google access token");
      return new TokenResult
      {
        Success = false,
        Message = $"Error refreshing token: {ex.Message}"
      };
    }
  }
}
