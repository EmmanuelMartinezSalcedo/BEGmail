using System.Net.Http.Json;
using System.Text.Json;
using GmailOrganizer.Core.Interfaces;
using GmailOrganizer.Core.Models;

namespace GmailOrganizer.Infrastructure.ExternalServices;
public class GeminiService : IGeminiService
{
  private readonly HttpClient _http;
  private readonly string _apiKey;
  private readonly ILogger<GeminiService> _logger;

  public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
  {
    _http = http;
    _logger = logger;
    _apiKey = config["Google AI Studio:APIKey"]
      ?? throw new ArgumentNullException("Google AI Studio:APIKey");
  }

  public async Task<List<EmailClassificationResult>> ClassifyEmailsAsync(
      List<GmailEmail> emails,
      List<GmailLabel> userLabels,
      CancellationToken ct)
  {
    if (emails == null || emails.Count == 0 || userLabels == null || userLabels.Count == 0)
      return new List<EmailClassificationResult>();

    try
    {
      _logger.LogInformation("Starting email classification for {Count} emails", emails.Count);

      // Construir lista de etiquetas para prompt
      var labelsList = string.Join(", ", userLabels.Select(l => l.Name));

      // Limitar longitud del body a 200 caracteres para el prompt
      var emailLines = emails.Select(e =>
        $"- Id:{e.Id}, Subject:{e.Subject}, Body:{e.Body[..Math.Min(200, e.Body.Length)]}"
      );

      var prompt = $@"
Eres un clasificador de correos electrónicos.
Tienes estas etiquetas posibles: [{labelsList}].

Para cada correo, devuelve SOLO un JSON en este formato:
[
  {{ ""emailId"": ""<id>"", ""suggestedLabels"": [""Label1"", ""Label2""] }},
  ...
]

Correos a clasificar:
{string.Join("\n", emailLines)}
";

      var request = new
      {
        contents = new[]
        {
          new { parts = new[] { new { text = prompt } } }
        },
        generationConfig = new { responseMimeType = "application/json" }
      };

      var response = await _http.PostAsJsonAsync(
        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}",
        request,
        ct
      );

      response.EnsureSuccessStatusCode();

      var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);

      var json = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "[]";

      var options = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

      var classifications = JsonSerializer.Deserialize<List<EmailClassificationResult>>(json, options);

      return classifications ?? new List<EmailClassificationResult>();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error clasificando correos con Gemini");
      return new List<EmailClassificationResult>();
    }
  }

  // Clases internas para mapear JSON de Gemini
  private record GeminiResponse(List<Candidate> Candidates);
  private record Candidate(Content Content);
  private record Content(List<Part> Parts);
  private record Part(string Text);
}
