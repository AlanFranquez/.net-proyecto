using Espectaculos.Application;
using Espectaculos.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Espectaculos.Infrastructure;
using Espectaculos.Infrastructure.Persistence;
using Espectaculos.Infrastructure.RealTime;
using Espectaculos.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages y ruta inicial al dashboard admin
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(o =>
    {
        o.Conventions.AddAreaPageRoute("Admin", "/Dashboard/Index", "");
    });

builder.Services.AddSignalR();


// Infraestructura (DbContext, repos, etc.)
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Default")
);

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(ApplicationAssembly.Value);
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(ApplicationAssembly.Value);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// 🔹 Notificador en tiempo real de accesos
builder.Services.AddScoped<IAccesosRealtimeNotifier, AccesosSignalRNotifier>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🚫 Sin autenticación ni autorización por ahora
// app.UseAuthentication();
// app.UseAuthorization();

app.MapRazorPages();

// Hub de accesos SIN RequireAuthorization
app.MapHub<AccesosHub>("/hubs/accesos");

app.Run();