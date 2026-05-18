using HCRS.Domain.Common;

namespace HCRS.Domain.Entities;

public class UserRole : BaseEntity
{
    public string Name { get; set; } = null!;
}
