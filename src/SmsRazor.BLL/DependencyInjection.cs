using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmsRazor.BLL.Services;
using SmsRazor.DAL.Data;

namespace SmsRazor.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SmsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISyllabusService, SyllabusService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<ITermService, TermService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();

        return services;
    }
}
