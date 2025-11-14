namespace UniShare.Infrastructure.Features.Users;

public record LoginResponse(string Token, UserDto User);

public record UserDto(Guid Id, string FullName, string Email, string Role);
