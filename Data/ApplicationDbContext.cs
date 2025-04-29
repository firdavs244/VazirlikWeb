using Microsoft.EntityFrameworkCore;
using VazirlikWeb.Models;

namespace VazirlikWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<News> News { get; set; }
    }
}
