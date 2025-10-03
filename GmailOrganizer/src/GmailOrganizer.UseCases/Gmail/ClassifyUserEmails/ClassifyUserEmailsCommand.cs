using GmailOrganizer.Core.UserAggregate;

namespace GmailOrganizer.UseCases.Gmail.ClassifyUserEmails;

public record ClassifyUserEmailsCommand(User User) : ICommand<Result>;
