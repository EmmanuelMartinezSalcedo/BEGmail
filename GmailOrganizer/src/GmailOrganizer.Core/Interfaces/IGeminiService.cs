using GmailOrganizer.Core.Models;

namespace GmailOrganizer.Core.Interfaces;
public interface IGeminiService
{
  Task<List<EmailClassificationResult>> ClassifyEmailsAsync(
    List<GmailEmail> emails,
    List<GmailLabel> userLabels,
    CancellationToken ct);
}
