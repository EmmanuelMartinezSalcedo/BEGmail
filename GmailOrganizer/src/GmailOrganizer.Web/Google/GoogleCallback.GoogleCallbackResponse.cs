namespace GmailOrganizer.Web.Google;
public record GoogleCallbackResponse(string AccessToken, string RefreshToken, string Email);
