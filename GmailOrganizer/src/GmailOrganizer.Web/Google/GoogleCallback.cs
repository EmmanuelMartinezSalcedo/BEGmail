using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GmailOrganizer.UseCases.Auth.Callback;
using Microsoft.IdentityModel.Tokens;

namespace GmailOrganizer.Web.Google;

public class GoogleCallback : EndpointWithoutRequest<GoogleCallbackResponse>
{
  private readonly IMediator _mediator;
  private readonly ILogger<GoogleCallback> _logger;
  private readonly string _jwtKey;
  private readonly string _jwtIssuer;
  private readonly string _jwtAudience;

  public GoogleCallback(IMediator mediator, ILogger<GoogleCallback> logger, IConfiguration config)
  {
    _mediator = mediator;
    _logger = logger;
    _jwtKey = config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
    _jwtIssuer = config["Jwt:Issuer"] ?? "GmailOrganizer";
    _jwtAudience = config["Jwt:Audience"] ?? "GmailOrganizerUsers";
  }

  public override void Configure()
  {
    Get(GoogleCallbackRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Callback de Google OAuth2";
      s.Description = "Google redirige aquí después del login con el código de autorización.";
    });
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    _logger.LogInformation("Callback iniciado");

    var code = Query<string>("code");
    var state = Query<string>("state");

    if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
    {
      await SendErrorPageAsync("Authorization code or state not found.", ct);
      return;
    }

    try
    {
      var command = new HandleGoogleCallbackCommand(code, state);
      var result = await _mediator.Send(command, ct);

      if (!result.IsSuccess)
      {
        await SendErrorPageAsync(result.Errors.FirstOrDefault() ?? "Authentication failed", ct);
        return;
      }

      var user = result.Value.User;
      if (user is null)
      {
        await SendErrorPageAsync("Authenticated user is null", ct);
        return;
      }

      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.UTF8.GetBytes(_jwtKey);
      var claims = new List<Claim>
      {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email),
        new("GoogleUserId", user.GoogleUserId)
      };
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddDays(7),
        Issuer = _jwtIssuer,
        Audience = _jwtAudience,
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);
      var jwt = tokenHandler.WriteToken(token);

      HttpContext.Response.Cookies.Append("jwt", jwt, new CookieOptions
      {
        HttpOnly = false,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = tokenDescriptor.Expires
      });

      await SendSuccessPageAsync(result.Value, ct);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling Google callback");
      await SendErrorPageAsync("An unexpected error occurred during authentication.", ct);
    }
  }

  private async Task SendSuccessPageAsync(GoogleCallbackResult result, CancellationToken ct)
  {
    _logger.LogInformation("User {Action}: {Email}",
      result.IsNewUser ? "created" : "updated",
      result.User?.Email);

    var html = """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Authentication Success</title>
  <style>
    body {
      margin: 0;
      padding: 0;
      min-height: 100vh;
      display: flex;
      justify-content: center;
      align-items: center;
      background-color: #f9fafb; /* base-200 */
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      color: #1f2937; /* base-content */
    }
    .card {
      background-color: #ffffff; /* base-100 */
      border-radius: 1rem;
      padding: 2rem;
      box-shadow: 0 4px 10px rgba(0,0,0,0.1);
      max-width: 400px;
      width: 100%;
      text-align: center;
    }
    h1 {
      font-size: 1.5rem;
      font-weight: 600;
      color: #16a34a; /* success */
      margin-bottom: 1rem;
    }
    p {
      margin-bottom: 1.5rem;
      font-size: 1rem;
    }
    .spinner {
      width: 32px;
      height: 32px;
      border: 3px solid #e5e7eb;
      border-top: 3px solid #16a34a;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  </style>
</head>
<body>
  <div class="card">
    <h1>Authentication Success</h1>
    <p>Redirecting...</p>
    <div class="spinner"></div>
  </div>
  <script>
    setTimeout(function() {
      window.location.href = 'http://localhost:4200/dashboard';
    }, 2000);
  </script>
</body>
</html>
""";


    await SendStringAsync(html, 200, "text/html", ct);
  }

  private async Task SendErrorPageAsync(string error, CancellationToken ct)
  {
    var html = $$"""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Authentication Error</title>
  <style>
    body {
      margin: 0;
      padding: 0;
      min-height: 100vh;
      display: flex;
      justify-content: center;
      align-items: center;
      background-color: #f9fafb; /* base-200 */
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      color: #1f2937; /* base-content */
    }
    .card {
      background-color: #ffffff; /* base-100 */
      border-radius: 1rem;
      padding: 2rem;
      box-shadow: 0 4px 10px rgba(0,0,0,0.1);
      max-width: 400px;
      width: 100%;
      text-align: center;
    }
    h1 {
      font-size: 1.5rem;
      font-weight: 600;
      color: #dc2626; /* error */
      margin-bottom: 1rem;
    }
    p {
      font-size: 1rem;
      margin-bottom: 1rem;
    }
    a {
      display: inline-block;
      padding: 0.5rem 1.25rem;
      border-radius: 0.5rem;
      background-color: #dc2626;
      color: #fff;
      text-decoration: none;
      font-weight: 500;
      transition: background-color 0.2s;
    }
    a:hover {
      background-color: #b91c1c;
    }
  </style>
</head>
<body>
  <div class="card">
    <h1>❌ Authentication Error</h1>
    <p><strong>{{error}}</strong></p>
    <a href="http://localhost:4200">Try Again</a>
  </div>
</body>
</html>
""";

    await SendStringAsync(html, 400, "text/html", ct);
  }
}
