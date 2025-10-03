using GmailOrganizer.Core.UserAggregate.Entities;

namespace GmailOrganizer.UseCases.Gmail.GetUserProcessedEmails;
public record GetUserProcessedEmailsCommand(int UserId)
  : ICommand<Result<List<EmailProcessingLog>>>;
