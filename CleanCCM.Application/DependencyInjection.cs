using CleanCCM.Application.Common.Behaviours;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Reflection;
using System.Transactions;

namespace CleanCCM.Application;

public static class DependencyInjection
{
  
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // LẤY ASSEMBLY HIỆN TẠI
        // Assembly chứa tất cả types trong CleanCCM.Application
        var assembly = Assembly.GetExecutingAssembly();

       
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.Lifetime = ServiceLifetime.Scoped;
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });


        services.AddValidatorsFromAssembly(assembly);

        services.AddAutoMapper(assembly);

     
        return services;
    }
}
