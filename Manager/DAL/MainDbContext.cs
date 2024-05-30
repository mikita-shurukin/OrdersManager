using Manager.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Manager.DAL
{
    public class MainDbContext : DbContext
    {
        public DbSet<Item> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {
        }
    }
}
