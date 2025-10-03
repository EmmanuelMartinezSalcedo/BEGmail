namespace GmailOrganizer.UseCases.AddEmailToWaitlist;

public record AddEmailToWaitlistCommand(string Email)
  : ICommand<Result>;
