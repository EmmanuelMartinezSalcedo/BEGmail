using System.ComponentModel.DataAnnotations;

namespace GmailOrganizer.Web.Google;

public class AddEmailToWaitlistRequest
{
  public const string Route = "/waitlist/add-email";

  [Required]
  [EmailAddress]
  public string? Email { get; set; }
}
