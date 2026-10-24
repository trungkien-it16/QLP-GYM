using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

        b.Entity<User>().Property(x => x.Username).HasMaxLength(50);
        b.Entity<User>().HasIndex(x => x.Username).IsUnique();

        b.Entity<Role>().Property(x => x.Name).HasMaxLength(50);

        b.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Staff" },
            new Role { Id = 3, Name = "Member" });
    }
}