namespace UniShare.Infrastructure.Features.Users.Login;

public record UserDto(Guid Id, string FullName, string Email, string Role);