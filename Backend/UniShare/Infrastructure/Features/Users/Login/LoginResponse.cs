namespace UniShare.Infrastructure.Features.Users.Login;

public record LoginResponse(string Token, UserDto User);
