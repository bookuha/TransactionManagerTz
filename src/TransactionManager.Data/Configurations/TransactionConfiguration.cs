using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TransactionManager.Data.Model;

namespace TransactionManager.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(t => t.ClientLocation)
            .HasConversion(
                location => location.ToString(),
                locationString => Location.FromString(locationString)
            );
    }
}