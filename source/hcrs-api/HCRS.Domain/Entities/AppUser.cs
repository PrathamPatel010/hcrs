using HCRS.Domain.Common;

namespace HCRS.Domain.Entities;

public class AppUser : BaseEntity
{
    public string UserEmail { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string UserDisplayName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Guid RoleId { get; set; }
    public UserRole? Role { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}