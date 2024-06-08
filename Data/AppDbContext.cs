using Microsoft.EntityFrameworkCore;
using PersonManagement.Models;


namespace PersonManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Person> Person { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<TaxNumber> TaxNumber { get; set; }
        public DbSet<Address> Address { get; set; }
    }
}
