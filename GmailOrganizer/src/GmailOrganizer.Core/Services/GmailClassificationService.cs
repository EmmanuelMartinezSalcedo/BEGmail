using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GmailOrganizer.Core.UserAggregate.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.Core.Services;

public interface IGmailClassificationService
{
  Task<List<EmailClassificationResult>> ClassifyEmailsAsync(
    List<GmailEmail> emails,
    List<GmailLabel> userLabels,
    CancellationToken ct);
}

public class GmailClassificationService(HttpClient http, IConfiguration config, ILogger<GmailClassificationService> logger)
  : IGmailClassificationService
{
  private readonly string _apiKey = config["Google AI Studio:APIKey"] ?? throw new ArgumentNullException("Google AI Studio:APIKey");
  private readonly ILogger<GmailClassificationService> _logger = logger;

  public async Task<List<EmailClassificationResult>> ClassifyEmailsAsync(
  List<GmailEmail> emails,
  List<GmailLabel> userLabels,
  CancellationToken ct)
  {
    _logger.LogInformation("Starting classification request...");

    var labelsList = string.Join(", ", userLabels.Select(l => l.Name));

    var prompt = $@"
    Eres un clasificador de correos electrónicos.
    Tienes estas etiquetas posibles: [{labelsList}].

    Para cada correo, devuelve SOLO un JSON en este formato:
    [
      {{ ""emailId"": ""<id>"", ""suggestedLabels"": [""Label1"", ""Label2""] }},
      ...
    ]

    Correos a clasificar:
    {string.Join("\n", emails.Select(e =>
        $"- Id:{e.Id}, Subject:{e.Subject}, Body:{e.Body[..Math.Min(200, e.Body.Length)]}"))}
    ";

    var request = new
    {
      contents = new[]
      {
      new
      {
        parts = new[]
        {
          new { text = prompt }
        }
      }
    },
      generationConfig = new
      {
        responseMimeType = "application/json"
      }
    };

    var response = await http.PostAsJsonAsync(
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
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      // Esta opción es clave para records con constructor
      IncludeFields = false,
      DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    var classifications = JsonSerializer.Deserialize<List<EmailClassificationResult>>(json, options);

    return classifications ?? new();
  }


  private record GeminiResponse(List<Candidate> Candidates);
  private record Candidate(Content Content);
  private record Content(List<Part> Parts);
  private record Part(string Text);
}
