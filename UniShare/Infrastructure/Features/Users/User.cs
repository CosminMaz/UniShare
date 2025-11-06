namespace UniShare.Infrastructure.Features.Users;

public enum Role
{
    Admin,
    User
}

public record User(Guid Id, string FullName, string Email, Role Role, DateTime CreatedAt);

