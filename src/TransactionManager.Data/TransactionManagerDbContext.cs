using Microsoft.EntityFrameworkCore;
using TransactionManager.Data.Configurations;
using TransactionManager.Data.Model;

namespace TransactionManager.Data;

public class TransactionManagerDbContext(DbContextOptions<TransactionManagerDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
    }
}