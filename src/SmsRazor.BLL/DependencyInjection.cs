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

        return services;
    }
}
