using Microsoft.EntityFrameworkCore;
using MojeApiMariaDB.Models;

namespace MojeApiMariaDB.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Location> Locations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Report> Reports { get; set; } // Nowa tabela

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seeding ról
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "user" },
            new Role { Id = 2, Name = "worker" },
            new Role { Id = 3, Name = "manager" },
            new Role { Id = 4, Name = "admin" }
        );

        // Konfiguracja relacji dla Zgłoszeń (Report)
        modelBuilder.Entity<Report>()
            .HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.Assignee)
            .WithMany()
            .HasForeignKey(r => r.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}