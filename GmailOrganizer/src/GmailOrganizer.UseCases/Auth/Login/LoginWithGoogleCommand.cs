namespace GmailOrganizer.UseCases.Auth.Login;

public record GenerateGoogleAuthUrlCommand() : ICommand<Result<GenerateGoogleAuthUrlResult>>;

public record GenerateGoogleAuthUrlResult(string AuthUrl, string State);
