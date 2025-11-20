using CleanCCM.BlazorUI;
using CleanCCM.BlazorUI.Services.Implementations;
using CleanCCM.BlazorUI.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddRadzenComponents();

// -----------------------------
// IHttpClientFactory
// -----------------------------
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7087");
    client.Timeout = TimeSpan.FromSeconds(10); // tùy chỉnh
});

// Nếu có JWT authentication (tuỳ thêm)
//builder.Services.AddScoped<AuthHeaderHandler>();
//builder.Services.AddHttpClient("ApiAuth", client =>
//{
//    client.BaseAddress = new Uri(apiBase);
//}).AddHttpMessageHandler<AuthHeaderHandler>();

// -----------------------------
// Register Api Clients
// -----------------------------
builder.Services.AddScoped<ITagClient, TagClient>();
builder.Services.AddScoped<ICategoryClient, CategoryClient>();
// thêm các client khác:
builder.Services.AddScoped<IProductClient, ProductClient>();
// builder.Services.AddScoped<IUserClient, UserClient>();


await builder.Build().RunAsync();
