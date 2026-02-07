using Microsoft.EntityFrameworkCore;
using SmsRazor.DAL.Entities;

namespace SmsRazor.DAL.Data;

public class SmsDbContext : DbContext
{
    public SmsDbContext(DbContextOptions<SmsDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<StudentInfo> StudentInfos { get; set; }
    public DbSet<AdminInfo> AdminInfos { get; set; }
    public DbSet<TeacherInfo> TeacherInfos { get; set; }
    public DbSet<StudentStatus> StudentStatuses { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Intake> Intakes { get; set; }
    public DbSet<Syllabus> Syllabuses { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
