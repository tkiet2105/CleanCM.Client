using CleanCCM.Api.Mapping;
using CleanCCM.API.Extensions;
using CleanCCM.Application;
using CleanCCM.Infrastructure;
using CleanCCM.Infrastructure.Data;
using CleanCCM.Infrastructure.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES ==========

// Controllers
builder.Services.AddControllersWithOptions();

// OpenAPI + Scalar
builder.Services.AddOpenApiWithScalar();

// CORS
builder.Services.AddCorsPolicy("AllowAll");

// Application Layer
builder.Services.AddApplication();

// Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);


// ========== BUILD APP ==========
builder.Services.AddAutoMapper(typeof(Program));


var app = builder.Build();

// ========== SEED DATABASE ==========
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<CleanCCM.Infrastructure.Identity.ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await ApplicationDbContextSeed.SeedDefaultRolesAsync(roleManager);
        await ApplicationDbContextSeed.SeedDefaultAdminAsync(userManager);

        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();


        await ApplicationDbContextSeed.SeedDefaultCategoriesAsync(context);
        await ApplicationDbContextSeed.SeedDefaultTagsAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// ========== MIDDLEWARE PIPELINE ==========

// Exception Handling (FIRST!)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Scalar UI + OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
         .WithTitle("CleanCCM API")
         .WithTheme(ScalarTheme.BluePlanet)
         .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
         .WithDarkMode()
         .WithSearchHotKey("k")
          .WithFavicon("https://lh4.googleusercontent.com/proxy/y7SzUjM8zQ1Y6wUeU-vdo-RuTvamyiqicljXeAEo2XBIg5RFoSEiV5UzyfZZR848pLBkIq3g5JO9OK_WXXwq6rThhgNkoUkwNdSS_jPYrC-5uGxzRd6cEcQ4Kfvw")
    ;
    });
}

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

// Authentication
app.UseAuthentication();

// Authorization
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();