using Microsoft.EntityFrameworkCore;
using ProductsManager.Models;


namespace ProductsManager.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        public DbSet<Products> products { get; set; }
    }
}