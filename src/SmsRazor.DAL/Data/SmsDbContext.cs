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
    public DbSet<Course> Courses { get; set; }
    public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
    public DbSet<SyllabusCourse> SyllabusCourses { get; set; }
    
    // Course and Timetabling
    public DbSet<Term> Terms { get; set; }
    public DbSet<TeacherAssignment> TeacherAssignments { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<AcademicCalendar> AcademicCalendars { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<TuitionPayment> TuitionPayments { get; set; }
    
    // Chat System
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<CoursePrerequisite>()
            .HasOne(cp => cp.Course)
            .WithMany(c => c.Prerequisites)
            .HasForeignKey(cp => cp.CourseId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent multiple cascade paths

        modelBuilder.Entity<CoursePrerequisite>()
            .HasOne(cp => cp.PrerequisiteCourse)
            .WithMany(c => c.PrerequisiteFor)
            .HasForeignKey(cp => cp.PrerequisiteCourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SyllabusCourse>()
            .HasOne(sc => sc.Syllabus)
            .WithMany()
            .HasForeignKey(sc => sc.SyllabusId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SyllabusCourse>()
            .HasOne(sc => sc.Course)
            .WithMany(c => c.SyllabusCourses)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AcademicCalendar>()
            .HasOne(ac => ac.Section)
            .WithMany(s => s.Calendars)
            .HasForeignKey(ac => ac.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enrollment rules
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentCode, e.SectionId })
            .IsUnique(); // Prevent duplicate exactly same enrollments
            
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentCode)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Section)
            .WithMany()
            .HasForeignKey(e => e.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Attendance rules
        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.StudentCode, a.AcademicCalendarId })
            .IsUnique(); // Prevent duplicate attendance tracking per calendar slot

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentCode)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Calendar)
            .WithMany()
            .HasForeignKey(a => a.AcademicCalendarId)
            .OnDelete(DeleteBehavior.Cascade);
    }

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
