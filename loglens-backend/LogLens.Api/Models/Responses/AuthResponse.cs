namespace LogLens.Api.Models.Responses;
public record AuthResponse(string AccessToken, string RefreshToken);
public record UserResponse(Guid Id, string Email, string Plan);
