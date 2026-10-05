using BookMyHome.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookMyHome.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedNever();

            builder.OwnsOne(u => u.FullName, p =>
            {
                p.Property(x => x.FirstName).HasColumnName("FirstName");
                p.Property(x => x.LastName).HasColumnName("LastName");
                
            });

            builder.OwnsOne(u => u.Email, p =>
            {
                p.Property(x => x.EmailAddress).HasColumnName("Email");
                p.HasIndex(u => u.EmailAddress).IsUnique();
            });
            

            builder.OwnsOne(u => u.PhoneNumber, p =>
            {
                p.Property(x => x.Value).HasColumnName("PhoneNumber");
            });

            builder.HasIndex(u => u.Username).IsUnique();
        }
    }
}
