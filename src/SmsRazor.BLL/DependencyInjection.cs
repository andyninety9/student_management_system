using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmsRazor.BLL.Services;
using SmsRazor.DAL.Data;
using SmsRazor.DAL.Repositories;

namespace SmsRazor.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SmsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISyllabusService, SyllabusService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<ITermService, TermService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ITuitionService, TuitionService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IAssistantService, AssistantService>();

        return services;
    }
}
