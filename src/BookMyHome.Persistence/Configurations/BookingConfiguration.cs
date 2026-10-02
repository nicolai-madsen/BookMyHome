using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookMyHome.Domain.Aggregates.Accommodations;

namespace BookMyHome.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id).ValueGeneratedNever();
               

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
