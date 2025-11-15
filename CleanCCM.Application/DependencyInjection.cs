using CleanCCM.Application.Common.Behaviours;
using CleanCCM.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Reflection;
using System.Transactions;

namespace CleanCCM.Application;

/// <summary>
/// DEPENDENCY INJECTION CONFIGURATION cho Application Layer
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// DEPENDENCY INJECTION (DI) LÀ GÌ?
/// - Pattern để quản lý dependencies
/// - Container tự động tạo và inject objects
/// - Loose coupling, dễ test, dễ maintain
/// 
/// SERVICE REGISTRATION:
/// - AddMediatR: Register MediatR + Handlers
/// - AddValidatorsFromAssembly: Register FluentValidation validators
/// - AddAutoMapper: Register AutoMapper profiles
/// 
/// EXTENSION METHOD:
/// - Method mở rộng cho IServiceCollection
/// - Gọi trong Program.cs: services.AddApplication()
/// - Tách biệt registration logic
/// 
/// VÍ DỤ TRONG Program.cs:
/// 
/// var builder = WebApplication.CreateBuilder(args);
/// 
/// // Add layers
/// builder.Services.AddApplication();           ← Method này
/// builder.Services.AddInfrastructure(config);
/// 
/// ASSEMBLY SCANNING:
/// - Assembly.GetExecutingAssembly(): Assembly hiện tại (Application)
/// - Tự động scan và register tất cả:
///   + MediatR Handlers (IRequestHandler)
///   + FluentValidation Validators (AbstractValidator)
///   + AutoMapper Profiles (Profile)
/// 
/// CONVENTION-BASED REGISTRATION:
/// - RegisterCommand → RegisterCommandHandler (auto-detected)
/// - RegisterCommand → RegisterCommandValidator (auto-detected)
/// - User → UserDto (auto-mapped via Profile)
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// EXTENSION METHOD để register Application services
    /// 
    /// CÁCH DÙNG:
    /// // Program.cs
    /// builder.Services.AddApplication();
    /// 
    /// SERVICES ĐƯỢC REGISTER:
    /// 1. MediatR + Pipeline Behaviours
    /// 2. FluentValidation Validators
    /// 3. AutoMapper Profiles
    /// </summary>
    /// <param name="services">IServiceCollection từ DI container</param>
    /// <returns>IServiceCollection để method chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // LẤY ASSEMBLY HIỆN TẠI
        // Assembly chứa tất cả types trong CleanCCM.Application
        var assembly = Assembly.GetExecutingAssembly();

        // ========== MEDIATR REGISTRATION ==========

        /// <summary>
        /// REGISTER MEDIATR + HANDLERS + PIPELINE BEHAVIOURS
        /// 
        /// GIẢI THÍCH:
        /// - RegisterServicesFromAssembly: Scan assembly và register handlers
        /// - AddBehavior: Register pipeline behaviours (middleware)
        /// 
        /// HANDLERS ĐƯỢC REGISTER:
        /// - RegisterCommandHandler
        /// - LoginCommandHandler
        /// - RefreshTokenCommandHandler
        /// - (và tất cả handlers khác trong assembly)
        /// 
        /// PIPELINE BEHAVIOURS:
        /// - ValidationBehaviour: Chạy TRƯỚC handler (validate request)
        /// - PerformanceBehaviour: Chạy TRƯỚC handler (measure time)
        /// 
        /// PIPELINE ORDER:
        /// Request
        ///   → ValidationBehaviour
        ///   → PerformanceBehaviour
        ///   → Handler
        ///   → Response
        /// 
        /// VÍ DỤ:
        /// await _mediator.Send(new RegisterCommand { ... });
        /// 
        /// Flow:
        /// 1. MediatR nhận RegisterCommand
        /// 2. Route đến RegisterCommandHandler
        /// 3. Chạy ValidationBehaviour (validate)
        /// 4. Chạy PerformanceBehaviour (start timer)
        /// 5. Chạy RegisterCommandHandler.Handle()
        /// 6. PerformanceBehaviour (stop timer, log nếu slow)
        /// 7. Return Result<string>
        /// </summary>
        services.AddMediatR(cfg =>
        {
            // REGISTER HANDLERS
            cfg.RegisterServicesFromAssembly(assembly);

            // REGISTER VALIDATION BEHAVIOUR
            // Generic type: IPipelineBehavior<TRequest, TResponse>
            // Implementation: ValidationBehaviour<TRequest, TResponse>
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            // REGISTER PERFORMANCE BEHAVIOUR
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });

        // GIẢI THÍCH typeof(IPipelineBehavior<,>):
        // - Open generic type (có 2 type parameters)
        // - MediatR sẽ tự động close type khi cần
        // 
        // VÍ DỤ:
        // RegisterCommand, Result<string>
        // → IPipelineBehavior<RegisterCommand, Result<string>>
        // → ValidationBehaviour<RegisterCommand, Result<string>>

        // ========== FLUENT VALIDATION REGISTRATION ==========

        /// <summary>
        /// REGISTER FLUENT VALIDATION VALIDATORS
        /// 
        /// GIẢI THÍCH:
        /// - Scan assembly tìm tất cả classes kế thừa AbstractValidator<T>
        /// - Auto-register vào DI container
        /// 
        /// VALIDATORS ĐƯỢC REGISTER:
        /// - RegisterCommandValidator
        /// - LoginCommandValidator
        /// - (và tất cả validators khác)
        /// 
        /// CONVENTION:
        /// - Command: RegisterCommand
        /// - Validator: RegisterCommandValidator
        /// 
        /// ValidationBehaviour SỬ DỤNG:
        /// public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        /// → DI tự động inject validators phù hợp với TRequest
        /// 
        /// VÍ DỤ:
        /// RegisterCommand
        /// → DI inject IEnumerable<IValidator<RegisterCommand>>
        /// → Collection chứa [RegisterCommandValidator]
        /// </summary>
        services.AddValidatorsFromAssembly(assembly);

        // ========== AUTOMAPPER REGISTRATION ==========

        /// <summary>
        /// REGISTER AUTOMAPPER PROFILES
        /// 
        /// GIẢI THÍCH:
        /// - Scan assembly tìm classes kế thừa Profile
        /// - Auto-register mapping configurations
        /// 
        /// PROFILES ĐƯỢC REGISTER:
        /// - MappingProfile (trong Common/Mappings/)
        /// 
        /// MAPPING EXAMPLE:
        /// // MappingProfile.cs
        /// public class MappingProfile : Profile
        /// {
        ///     public MappingProfile()
        ///     {
        ///         CreateMap<User, UserDto>();
        ///         CreateMap<Product, ProductDto>();
        ///     }
        /// }
        /// 
        /// SỬ DỤNG:
        /// private readonly IMapper _mapper;
        /// 
        /// var userDto = _mapper.Map<UserDto>(user);
        /// 
        /// LƯU Ý:
        /// - Project này chưa dùng nhiều AutoMapper
        /// - Có thể xóa nếu không cần
        /// - Hoặc thêm mappings sau
        /// </summary>
        services.AddAutoMapper(assembly);

        // RETURN services để method chaining
        // VÍ DỤ:
        // services
        //     .AddApplication()
        //     .AddInfrastructure(config)
        //     .AddOtherServices();
        return services;
    }
}
