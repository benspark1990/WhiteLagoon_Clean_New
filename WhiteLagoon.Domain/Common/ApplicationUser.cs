using Microsoft.AspNetCore.Identity;

namespace WhiteLagoon.Domain.Common
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
