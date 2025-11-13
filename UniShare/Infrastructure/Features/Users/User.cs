namespace UniShare.Infrastructure.Features.Users;

public enum Role
{
    Admin,
    User
}

public record User(Guid Id, string FullName, string Email, string PasswordHash, Role Role, DateTime CreatedAt);

