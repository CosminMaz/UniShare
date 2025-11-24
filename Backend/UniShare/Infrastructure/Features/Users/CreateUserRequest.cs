namespace UniShare.Infrastructure.Features.Users;

public record CreateUserRequest(string Fullname, string Email, string Password);