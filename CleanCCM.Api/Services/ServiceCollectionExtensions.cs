using Microsoft.OpenApi.Models;

namespace CleanCCM.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiWithScalar(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new()
                {
                    Title = "CleanCCM API",
                    Version = "v1",
                    Description = "Clean Architecture CCM API with CQRS, Result Pattern, and JWT Authentication",
                    Contact = new()
                    {
                        Name = "CleanCCM Team",
                        Email = "tkiet21590@gmail.com"
                    }
                };
                return Task.CompletedTask;
            });

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new();
                document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
                {
                    ["Bearer"] = new()
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Enter your JWT token in the format: Bearer {token}"
                    }
                };

                document.SecurityRequirements = new List<OpenApiSecurityRequirement>
                {
                    new()
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }] = Array.Empty<string>()
                    }
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, string policyName = "AllowAll")
    {
        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Đăng ký Controllers
    /// 
    /// LƯU Ý .NET 9.0:
    /// - SuppressModelStateInvalidFilter đã bị remove
    /// - Validation tự động vẫn hoạt động
    /// - ValidationBehaviour sẽ override automatic validation
    /// </summary>
    public static IServiceCollection AddControllersWithOptions(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}