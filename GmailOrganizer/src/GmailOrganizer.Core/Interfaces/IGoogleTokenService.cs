using GmailOrganizer.Core.Models;

namespace GmailOrganizer.Core.Interfaces;

public interface IGoogleTokenService
{
  Task<TokenResult> RefreshAccessTokenAsync(string refreshToken);
}
