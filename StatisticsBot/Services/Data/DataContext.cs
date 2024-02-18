using Microsoft.EntityFrameworkCore;
using StatisticsBot.Services.Data.Models;

namespace StatisticsBot.Services.Data;

public class DataContext : DbContext
{
    public DataContext() : base() { }

    public DbSet<User> Users { get; set; }
    public DbSet<ChartMessage> ChartMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseSqlite(Config.Instance.ConnectionString);
    }

    public static DataContext Create()
    {
        return new DataContext();
    }

    public async Task ApplyMigrations()
    {
        await Database.MigrateAsync();
    }
}
