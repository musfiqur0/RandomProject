using Microsoft.EntityFrameworkCore;
using UsersApi.Entities;

namespace UsersApi.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        base.OnModelCreating(b);
    }
}
