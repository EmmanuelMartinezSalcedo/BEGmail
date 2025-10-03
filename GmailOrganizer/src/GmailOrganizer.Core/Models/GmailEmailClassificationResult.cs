namespace GmailOrganizer.Core.Models;
public record GmailEmailsClassifiedResult(
  bool Success,
  string Message,
  List<EmailClassificationResult> ClassifiedEmails
);
