namespace GmailOrganizer.Web.Google;
//public record GoogleCallbackResponse(string AccessToken, string RefreshToken, string Email);
public record GoogleCallbackResponse(bool Success, string Message, bool IsNewUser, UserDto? User);
//{
//  public bool Success { get; set; }
//  public string Message { get; set; } = default!;
//  public bool IsNewUser { get; set; }
//  public UserDto? User { get; set; }
//}
public class UserDto
{
  public int Id { get; set; }
  public string Email { get; set; } = default!;
  public string GoogleUserId { get; set; } = default!;
}
