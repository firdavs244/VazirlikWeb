using Microsoft.AspNetCore.Identity;

namespace VazirlikWeb.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}