using HCRS.Domain.Common;

namespace HCRS.Domain.Entities;

public class UserRole : BaseEntity
{
    public string Name { get; set; } = null!;

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
