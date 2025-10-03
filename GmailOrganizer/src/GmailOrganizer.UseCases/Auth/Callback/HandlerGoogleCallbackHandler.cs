using GmailOrganizer.Core.Interfaces;
using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Specifications;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.UseCases.Auth.Callback;

public class HandleGoogleCallbackHandler(
    IGoogleAuthService googleAuthService,
    IRepository<User> userRepository,
    ILogger<HandleGoogleCallbackHandler> logger)
    : ICommandHandler<HandleGoogleCallbackCommand, Result<GoogleCallbackResult>>
{
  public async Task<Result<GoogleCallbackResult>> Handle(
      HandleGoogleCallbackCommand request,
      CancellationToken ct)
  {
    try
    {
      // Intercambiar el code por tokens y obtener el correo del usuario
      var authResult = await googleAuthService.HandleAuthCallbackAsync(request.Code, request.State);

      if (!authResult.Success)
        return Result<GoogleCallbackResult>.Error(authResult.Message);

      if (string.IsNullOrEmpty(authResult.AccessToken))
        return Result<GoogleCallbackResult>.Error("Access token missing from authentication result");

      if (string.IsNullOrEmpty(authResult.GoogleUserId))
        return Result<GoogleCallbackResult>.Error("Google user ID missing from authentication result");
      if (string.IsNullOrEmpty(authResult.Email))
        return Result<GoogleCallbackResult>.Error("Email missing from authentication result");

      var existingUser = await userRepository.FirstOrDefaultAsync(
          new UserByGoogleIdSpec(authResult.GoogleUserId), ct);

      // Determinar refreshToken a usar
      var refreshToken = authResult.RefreshToken ?? existingUser?.RefreshToken?.Value;

      User savedUser;
      bool isNewUser = false;

      if (existingUser == null)
      {
        // Nuevo usuario: refreshToken obligatorio
        if (string.IsNullOrEmpty(refreshToken))
          return Result<GoogleCallbackResult>.Error("Refresh token missing for new user");

        savedUser = new User(
          googleUserId: authResult.GoogleUserId,
          email: authResult.Email,
          accessToken: authResult.AccessToken,
          refreshToken: refreshToken,
          tokenExpiry: authResult.ExpiresAt
        );

        await userRepository.AddAsync(savedUser, ct);
        isNewUser = true;
        logger.LogInformation("Created new user: {Email}", savedUser.Email);
      }
      else
      {
        // Usuario existente: actualizar accessToken y opcional refreshToken
        existingUser.UpdateAccessToken(authResult.AccessToken, authResult.ExpiresAt);

        if (!string.IsNullOrEmpty(refreshToken))
          existingUser.UpdateRefreshToken(refreshToken);

        await userRepository.UpdateAsync(existingUser, ct);
        savedUser = existingUser;
        logger.LogInformation("Updated existing user: {Email}", savedUser.Email);
      }

      await userRepository.SaveChangesAsync(ct);

      return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult
      {
        Success = true,
        Message = isNewUser ? "User created successfully" : "User updated successfully",
        User = savedUser,
        IsNewUser = isNewUser
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error handling Google callback");
      return Result<GoogleCallbackResult>.Error(ex.Message);
    }
  }
}
