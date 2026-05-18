using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<EventParticipation>()
            .HasKey(ep => new { ep.EventId, ep.EmployeeId });
        modelBuilder.Entity<GroupMembership>()
            .HasKey(gm => new { gm.EmployeeId, gm.GroupId });
    }
}