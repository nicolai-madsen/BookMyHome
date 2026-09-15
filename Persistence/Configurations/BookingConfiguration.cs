using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Accommodations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Enums;

namespace Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.OwnsOne(b => b.RentalPeriod, p =>
            {
                p.Property(x => x.StartDate).HasColumnName("BookingStartDate");
                p.Property(x => x.EndDate).HasColumnName("BookingEndDate");
            });

            builder.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(b => b.PricePerDayWhenBooked).HasColumnType("decimal(18,2)");
        }
    }
}
