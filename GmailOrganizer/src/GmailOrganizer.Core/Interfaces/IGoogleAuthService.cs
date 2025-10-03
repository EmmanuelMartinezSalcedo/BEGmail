using GmailOrganizer.Core.Models;

namespace GmailOrganizer.Core.Interfaces;
public interface IGoogleAuthService
{
  Task<string> GenerateAuthUrlAsync(string state);
  Task<AuthResult> HandleAuthCallbackAsync(string code, string state);
}

