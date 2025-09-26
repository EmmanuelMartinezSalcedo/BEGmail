namespace GmailOrganizer.UseCases.Auth.Login;

public record LoginWithGoogleCommand(string State) : ICommand<Result<string>>;
