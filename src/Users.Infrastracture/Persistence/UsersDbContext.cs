using Microsoft.EntityFrameworkCore;
using Users.Domain;
namespace Users.Infrastracture.Persistence;

internal class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Users");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(256);

                email.HasIndex(e => e.Value)
                    .IsUnique()
                    .HasDatabaseName("UX_Users_Email");
            });

            entity.OwnsOne(u => u.Password, password =>
            {
                password.Property(p => p.HashedValue)
                    .HasColumnName("Password")
                    .IsRequired();
            });

            entity.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(100);
        });
        base.OnModelCreating(modelBuilder);
    }
}
