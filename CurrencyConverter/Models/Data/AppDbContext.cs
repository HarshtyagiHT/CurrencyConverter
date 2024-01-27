using Microsoft.EntityFrameworkCore;
namespace CurrencyConverter.Models.Data
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<ExchangeRate> ExchangeRates { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public AppDbContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           optionsBuilder.UseSqlServer("Server=.;Database=CurrencyConverter;Trusted_Connection=True;");       
        }
    }
}
