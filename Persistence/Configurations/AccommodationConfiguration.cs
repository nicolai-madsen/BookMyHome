using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Accommodations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
    {
        public void Configure(EntityTypeBuilder<Accommodation> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(b => b.Id).ValueGeneratedNever();

            builder.HasMany(a => a.Bookings)
                .WithOne()
                .HasForeignKey("AccommodationId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata
                .FindNavigation(nameof(Accommodation.Bookings))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsOne(a => a.Address, p =>
            {
                p.Property(x => x.StreetName).HasColumnName("StreetName");
                p.Property(x => x.StreetNumber).HasColumnName("StreetNumber");
                p.Property(x => x.City).HasColumnName("City");
                p.Property(x => x.ZipCode).HasColumnName("PostalCode");
                p.Property(x => x.Country).HasColumnName("Country");
            });

            builder.OwnsOne(a => a.AvailablePeriod, p =>
            {
                p.Property(x => x.StartDate).HasColumnName("AvailableFrom");
                p.Property(x => x.EndDate).HasColumnName("AvailableTo");
            });

            builder.Property(a => a.PricePerDay).HasColumnType("decimal(18,2)");

            builder.HasMany(a => a.Bookings)
                .WithOne()
                .HasForeignKey(b => b.AccommodationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(Accommodation.Bookings))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
