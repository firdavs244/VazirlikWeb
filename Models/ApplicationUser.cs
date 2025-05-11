using Microsoft.AspNetCore.Identity;

namespace VazirlikWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}