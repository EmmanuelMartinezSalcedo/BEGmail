using GmailOrganizer.Core.Services;
using GmailOrganizer.Core.UserAggregate;
using GmailOrganizer.Core.UserAggregate.Specifications;
using Microsoft.Extensions.Logging;

namespace GmailOrganizer.UseCases.Auth.Callback;
public class HandleGoogleCallbackHandler(
    IGmailService gmailService,
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
      // 1. Intercambiar código por tokens
      var authResult = await gmailService.HandleAuthCallbackAsync(request.Code, request.State);

      if (!authResult.Success || authResult.User == null)
      {
        return Result<GoogleCallbackResult>.Error(authResult.Message);
      }

      // 2. Verificar si el usuario ya existe usando Specification
      var userSpec = new UserByGoogleIdSpec(authResult.User.GoogleUserId);
      var existingUser = await userRepository.FirstOrDefaultAsync(userSpec, ct);

      bool isNewUser = false;
      User savedUser;

      if (existingUser == null)
      {
        // 3a. Usuario nuevo - crear
        savedUser = await userRepository.AddAsync(authResult.User, ct);
        isNewUser = true;
        logger.LogInformation("Created new user: {Email}", savedUser.Email);
      }
      else
      {
        // 3b. Usuario existente - actualizar tokens
        existingUser.UpdateAccessToken(
            authResult.User.AccessToken.Value,
            authResult.User.TokenExpiry);

        if (!string.IsNullOrEmpty(authResult.User.RefreshToken.Value))
        {
          existingUser.UpdateRefreshToken(authResult.User.RefreshToken.Value);
        }

        await userRepository.UpdateAsync(existingUser, ct);
        savedUser = existingUser;
        logger.LogInformation("Updated existing user: {Email}", savedUser.Email);
      }

      // 4. Guardar cambios
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
