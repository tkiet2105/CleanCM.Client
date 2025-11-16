using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Infrastructure.Data;
using CleanCCM.Infrastructure.Data.Interceptors;
using CleanCCM.Infrastructure.Repositories;
using CleanCCM.Infrastructure.Services;

namespace CleanCCM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            );

            options.AddInterceptors(interceptor);
        });

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}