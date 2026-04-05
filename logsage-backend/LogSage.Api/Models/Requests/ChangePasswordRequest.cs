namespace LogSage.Api.Models.Requests;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
