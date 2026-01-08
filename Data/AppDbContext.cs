using Microsoft.EntityFrameworkCore;

namespace Craciun_Adriana_Lab4.Data
{
    public class AppDbContext: Microsoft.EntityFrameworkCore.DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Craciun_Adriana_Lab4.Models.PredictionHistory> PredictionHistories { get; set; } = null!;
    }
}
